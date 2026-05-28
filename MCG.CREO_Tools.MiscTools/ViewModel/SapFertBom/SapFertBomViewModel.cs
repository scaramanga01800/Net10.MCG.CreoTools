using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.SapTools.Exceptions;
using MCG.CommonLib.SapTools.Interfaces;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.Interfaces;
using MCG.CREO_Tools.MiscTools.Services;
using MCG.CREO_Tools.MiscTools.View.SapFertBom;
using MCG.WindchillRequestTool;
using MCG.WindchillRequestTool.Model.BomComparison;
using MCG.WindchillRequestTool.Services.Interfaces;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.MiscTools.ViewModel.SapFertBom
{
    public class SapFertBomViewModel : ObservableObject, ISapFertBomViewModel
    {
        #region [REGION] Properties from Interface
        private SapFertBomDataContext _CurrentDataContext;
        public SapFertBomDataContext CurrentDataContext
        {
            get { return _CurrentDataContext; }
            set
            {
                if (this._CurrentDataContext != value)
                {
                    this._CurrentDataContext = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private Dispatcher MainDispatcher { get; set; } = null;
        private SapConfiguration CurrentConfiguration { get; set; }
        private Thread ThreadSearchBom { get; set; }
        private BomItem BomItem1 { get; set; } = new BomItem();
        private BomItem BomItem2 { get; set; } = new BomItem();
        private List<string> ListSapPartSearched { get; set; } = new List<string>();
        private List<BomMissingComponentItem> ListMissingParts { get; set; }
        public bool IsPartInSap { get; set; } = false;
        public bool IsBomExist { get; set; } = false;
        public bool IsBomUpdated { get; set; } = false;
        #endregion

        #region [REGION] Events Action
        /// <summary>
        /// Occurs when [Action in Progress event].
        /// </summary>
        public event EventHandler ActionInProgressEvent;
        /// <summary>
        /// Raises the Action in progress event.
        /// </summary>
        public void RaiseActionInProgressEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                ActionInProgressEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// Occurs when [Action done event].
        /// </summary>
        public event EventHandler ActionDoneEvent;
        /// <summary>
        /// Raises the Action done event.
        /// </summary>
        public void RaiseActionDoneEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                ActionDoneEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Commands
        public ICommand CommandStartSapBomExport { get => new RelayCommand(() => ExecuteStartSapBomExport()); }
        public ICommand CommandStartImportExcel { get => new RelayCommand(() => ExecuteStartExportExcel()); }
        public ICommand CommandStartCheckPartSap { get => new RelayCommand(() => ExecuteStartCheckPartSap()); }
        public ICommand CommandStartUpdateBomSap { get => new RelayCommand(() => ExecuteStartUpdateBomSap()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        public ICommand CommandPaste { get => new RelayCommand<KeyEventArgs>((obj) => ExecuteCommandPaste(obj)); }
        #endregion

        #region [REGION] Init
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly ISapMaterialService _sapMaterialService;
        private readonly ISapBomService _sapBomService;
        private readonly IMiscToolsWindchillService _miscToolsWindchillService;
        private readonly IBomComparisonToolService _bomComparisonToolService;
        public SapFertBomViewModel(IXmlSerializeTools xmlSerializeTools,
                                   ISapMaterialService sapMaterialService,
                                   ISapBomService sapBomService,
                                   IMiscToolsWindchillService miscToolsWindchillService,
                                   IBomComparisonToolService bomComparisonToolService)
        {
            try
            {
                _xmlSerializeTools = xmlSerializeTools;
                _sapMaterialService = sapMaterialService;
                _sapBomService = sapBomService;
                _miscToolsWindchillService = miscToolsWindchillService;
                _bomComparisonToolService = bomComparisonToolService;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                MainDispatcher = Dispatcher.CurrentDispatcher;

                CurrentDataContext = new SapFertBomDataContext();

                CurrentConfiguration = _xmlSerializeTools.GetDeserializedXml<SapConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.ConfigurationSapBomExport}");
                if (CurrentConfiguration != null && CurrentConfiguration.ListSapPlant != null && CurrentConfiguration.ListBomApplication != null)
                {
                    foreach (var item in CurrentConfiguration.ListSapPlant)
                        CurrentDataContext.AllSapPlants.Add(item.Number);
                    CurrentDataContext.Plant = CurrentDataContext.AllSapPlants.FirstOrDefault();
                }

                CurrentDataContext.BomComparison = new BomComparisonItem();
                CurrentDataContext.BomComparison.SourceBom1 = McgWpfTools.GetStringResource("SFB_BomLeft");
                CurrentDataContext.BomComparison.SourceBom2 = McgWpfTools.GetStringResource("SFB_BomRight");
                BomItem1.Bom = new System.Collections.ObjectModel.ObservableCollection<BomComponent>();
                BomItem2.Bom = new System.Collections.ObjectModel.ObservableCollection<BomComponent>();
                CurrentDataContext.BomComparison.IsCommentShown = true;

                CurrentDataContext.FertNumberUpsateEvent += (o, e) => { IsPartInSap = false; };
                CurrentDataContext.PlantChangeEvent += (o, e) => { ListMissingParts?.Clear(); ListSapPartSearched?.Clear(); };
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteStartSapBomExport()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentDataContext.FertNumber) || CurrentDataContext.FertNumber.Contains("*"))
                    MessageBox.Show(McgWpfTools.GetStringResource("SFB_EnterNumber"), McgWpfTools.GetStringResource("SFB_WindowsIssue"), MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                else
                {
                    CurrentDataContext.IsActionProgress = true;
                    CurrentDataContext.IsPleaseWaitShown = true;
                    RaiseActionInProgressEvent();
                    IsBomExist = false;
                    IsPartInSap = false;
                    ThreadSearchBom = new Thread(() => SearchBomSapAsynch());
                    ThreadSearchBom.Start();
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartExportExcel()
        {
            try
            {
                GetBomFromClipboard();
                IsBomUpdated = true;
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartCheckPartSap()
        {
            try
            {
                if (CurrentDataContext.BomComparison.BomComparison != null && CurrentDataContext.BomComparison.BomComparison.Count > 0)
                {
                    CurrentDataContext.IsActionProgress = true;
                    CurrentDataContext.IsPleaseWaitShown = true;
                    RaiseActionInProgressEvent();
                    ThreadSearchBom = new Thread(() => CheckPartInSapAsynch());
                    ThreadSearchBom.Start();
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartUpdateBomSap()
        {
            try
            {
                if (!IsPartInSap)
                {
                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("SFB_PartNotFound"), CurrentDataContext.Plant), McgWpfTools.GetStringResource("SFB_TitleFertBomIssue"), MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (!IsBomUpdated)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("SFB_BomUpdateMissing"), McgWpfTools.GetStringResource("SFB_TitleFertBomIssue"), MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                CurrentDataContext.IsActionProgress = true;
                CurrentDataContext.IsPleaseWaitShown = true;
                RaiseActionInProgressEvent();
                ThreadSearchBom = new Thread(() => UpdateBomSapAsynch());
                ThreadSearchBom.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenHelp()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("SFB_UserGuide"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCommandPaste(KeyEventArgs e = null)
        {
            try
            {
                if (e == null || (Keyboard.Modifiers == ModifierKeys.Control && e != null && e.Key == Key.V))
                {
                    GetBomFromClipboard();
                    IsBomUpdated = true;
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        private void SearchBomSapAsynch()
        {
            try
            {
                BomItem1.Bom.Clear();
                IsPartInSap = false;
                IsBomExist = false;

                try
                {
                    IsPartInSap = _sapMaterialService.IsPartExist(CurrentDataContext.FertNumber, true, CurrentDataContext.Plant);

                    if (IsPartInSap)
                    {
                        var ListComp = _sapBomService.ExtractOneMaterialMasterSapBom(CurrentDataContext.FertNumber.ToUpper(), DateTime.Today.ToString("yyyyMMdd"), CurrentDataContext.Plant.Replace("0000", ""));
                        if (ListComp != null)
                        {
                            if (ListComp.Count > 0)
                                IsBomExist = true;

                            foreach (var Comp in ListComp.Where(item => item.Level == 1))
                            {
                                Comp.State = "";
                                Comp.Revision = "";
                                Comp.Unit = BomUnit.UNKNOWN;
                                BomItem1.Bom.Add(Comp);
                            }
                        }
                        else
                            MessageBox.Show(McgWpfTools.GetStringResource("SFB_InfoMsgErpCom"), McgWpfTools.GetStringResource("SFB_InfoTitleErpBom"), MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                        MessageBox.Show(McgWpfTools.GetStringResource("SFB_PartNotFound"), McgWpfTools.GetStringResource("SFB_InfoTitleErpBom"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (SapToolsNoConnectionException)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("SFB_InfoMsgErpConNotFound"), McgWpfTools.GetStringResource("SFB_InfoTitleErpBom"), MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CurrentDataContext.BomComparison = _bomComparisonToolService.GetBomComparison(BomItem1, BomItem2, false, false, false);
                CurrentDataContext.BomComparison.SourceBom1 = McgWpfTools.GetStringResource("SFB_BomLeft");
                CurrentDataContext.BomComparison.SourceBom2 = McgWpfTools.GetStringResource("SFB_BomRight");
                CurrentDataContext.BomComparison.IsCommentShown = true;

                foreach (var comp in CurrentDataContext.BomComparison.BomComparison)
                {
                    comp.Comment = "";
                    if (!comp.CheckPartBom1)
                        comp.Comment = McgWpfTools.GetStringResource("SFB_ToBeAdded");
                    else if (!comp.CheckPartBom2)
                        comp.Comment = McgWpfTools.GetStringResource("SFB_ToBeRemoved");
                    else if (!comp.CheckQty)
                        comp.Comment = McgWpfTools.GetStringResource("SFB_ToBeUpdated");
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsPleaseWaitShown = false;
                CurrentDataContext.IsActionProgress = false;
                RaiseActionDoneEvent();
            }
        }

        private void GetBomFromClipboard()
        {
            try
            {
                BomItem2.Bom.Clear();
                string CompleteString = null;
                if (Clipboard.GetData(DataFormats.Text) != null)
                    CompleteString = Clipboard.GetData(DataFormats.Text).ToString();

                if (CompleteString != null)
                {
                    var AllLines = CompleteString.Split('\n');

                    string linePurged = null;
                    string TempNumber;
                    string TempRep;
                    double TempQty;
                    string tempDesc;
                    foreach (var line in AllLines)
                    {
                        linePurged = line.Split('\r').FirstOrDefault();
                        var AllValues = linePurged.Split('\t');
                        if (AllValues != null && AllValues.Count() > 4)
                        {

                            TempNumber = AllValues[0].Trim().ToUpper();
                            if (!string.IsNullOrWhiteSpace(TempNumber))
                            {

                                TempRep = AllValues[1].Trim().ToUpper();
                                try { TempQty = Convert.ToDouble(AllValues[2].Trim().ToUpper()); }
                                catch { TempQty = 0; }
                                tempDesc = $"{AllValues[3]}|{AllValues[4]}";

                                BomComponent currentComp = BomItem2.Bom.FirstOrDefault(item => item.Number == TempNumber);

                                if (currentComp != null)
                                    currentComp.Quantity += TempQty;
                                else
                                    BomItem2.Bom.Add(new BomComponent()
                                    {
                                        Number = TempNumber,
                                        LineNumber = TempRep,
                                        Quantity = TempQty,
                                        Revision = "",
                                        State = "",
                                        Unit = BomUnit.UNKNOWN,
                                        Description = tempDesc
                                    });
                            }
                        }
                    }
                }

                CurrentDataContext.BomComparison = _bomComparisonToolService.GetBomComparison(BomItem1, BomItem2, false, false, false);
                CurrentDataContext.BomComparison.SourceBom1 = McgWpfTools.GetStringResource("SFB_BomLeft");
                CurrentDataContext.BomComparison.SourceBom2 = McgWpfTools.GetStringResource("SFB_BomRight");
                CurrentDataContext.BomComparison.IsCommentShown = true;

                foreach (var comp in CurrentDataContext.BomComparison.BomComparison)
                {
                    comp.Comment = "";
                    if (!comp.CheckPartBom1)
                        comp.Comment = McgWpfTools.GetStringResource("SFB_ToBeAdded");
                    else if (!comp.CheckPartBom2)
                        comp.Comment = McgWpfTools.GetStringResource("SFB_ToBeRemoved");
                    else if (!comp.CheckQty)
                        comp.Comment = McgWpfTools.GetStringResource("SFB_ToBeUpdated");
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void SavelistPart(List<BomMissingComponentItem> listPart)
        {
            try
            {
                string fileName = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\MissingPart_{CurrentDataContext.FertNumber}.txt";
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    foreach (var item in listPart)
                    {
                        writer.WriteLine($"{item.Number}\t{item.Comment}");
                    }
                }

            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void CheckPartInSapAsynch()
        {
            try
            {
                var listCheckPart = CurrentDataContext.BomComparison.BomComparison.Where(item => !item.CheckPartBom1).Select(item => item.PartNumber).ToList();

                List<string> listPlants = new List<string>();
                listPlants.Add("None");
                if (CurrentDataContext.Plant != "0000")
                    listPlants.Add(CurrentDataContext.Plant);

                ListMissingParts = _sapMaterialService.CheckPartInSap(listCheckPart, listPlants, ListSapPartSearched, true, ListMissingParts);

                SavelistPart(ListMissingParts);

                MainDispatcher.BeginInvoke(new Action(() =>
                {
                    if (ListMissingParts.Count > 0)
                    {
                        _miscToolsWindchillService.ShowSapFertMissingPart(ListMissingParts, true);

                        //SapFertMissingPart aSapFertMissingPart = new SapFertMissingPart();
                        //aSapFertMissingPart.ListPart = ListMissingParts;
                        //aSapFertMissingPart.Show();
                    }
                }));
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsPleaseWaitShown = false;
                CurrentDataContext.IsActionProgress = false;
                RaiseActionDoneEvent();
            }
        }

        private void UpdateBomSapAsynch()
        {
            try
            {
                try
                {
                    CheckPartInSapAsynch();

                    CurrentDataContext.IsActionProgress = true;
                    CurrentDataContext.IsPleaseWaitShown = true;

                    SAPBom currentSapBom = new SAPBom()
                    {
                        AllComponents = new List<SAPBomComponent>(),
                        Plant = CurrentDataContext.Plant,
                        Alternative = "",
                        BomUsage = "3",
                        UpperLevelPartNumber = CurrentDataContext.FertNumber,
                        BomStatus = SAPBomMsg.BOMNOTEXIST
                    };

                    SAPBomComponent CurrentSAPBomComponent = null;

                    foreach (var comp in CurrentDataContext.BomComparison.BomComparison)
                    {
                        // Component to be deleted
                        if (!comp.CheckPartBom2)
                            CurrentSAPBomComponent = new SAPBomComponent()
                            {
                                ComponentAction = SAPBomAction.TOBEDELETED,
                                LineNumber = comp.LineNumberBom1,
                                PartNumber = comp.PartNumber,
                                TypeComponent = "L",
                                Unit = comp.UnitBom1.ToString(),
                                Quantity = Convert.ToDouble(comp.QtyBom1)
                            };
                        // Component to be addded - Remove component not found in SAP
                        else if (!comp.CheckPartBom1 && ListMissingParts.FirstOrDefault(item => item.Number == comp.PartNumber) == null)
                            CurrentSAPBomComponent = new SAPBomComponent()
                            {
                                ComponentAction = SAPBomAction.TOBEADDED,
                                LineNumber = comp.LineNumberBom2,
                                PartNumber = comp.PartNumber,
                                TypeComponent = "L",
                                Unit = comp.UnitBom2.ToString(),
                                Quantity = Convert.ToDouble(comp.QtyBom2)
                            };
                        // Component to be updated
                        //else if (!comp.CheckLineNumber || !comp.CheckQty)
                        else if (!comp.CheckQty)
                            CurrentSAPBomComponent = new SAPBomComponent()
                            {
                                ComponentAction = SAPBomAction.TOBEUPDATED,
                                LineNumber = comp.LineNumberBom2,
                                PartNumber = comp.PartNumber,
                                TypeComponent = "L",
                                Unit = comp.UnitBom1.ToString(),
                                Quantity = Convert.ToDouble(comp.QtyBom2)
                            };
                        // Component not updated
                        else
                            CurrentSAPBomComponent = new SAPBomComponent()
                            {
                                ComponentAction = SAPBomAction.UNMODIFIEDBOM,
                                LineNumber = comp.LineNumberBom1,
                                PartNumber = comp.PartNumber,
                                TypeComponent = "L",
                                Unit = comp.UnitBom1.ToString(),
                                Quantity = Convert.ToDouble(comp.QtyBom1)
                            };
                        currentSapBom.AllComponents.Add(CurrentSAPBomComponent);
                    }

                    _sapBomService.UpdateBomWithoutPartCheck(currentSapBom, "", false);
                }
                catch (SapToolsNoConnectionException)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("SFB_InfoMsgErpConNotFound"), McgWpfTools.GetStringResource("SFB_InfoTitleErpBom"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsActionProgress = false;
                CurrentDataContext.IsPleaseWaitShown = false;
                RaiseActionDoneEvent();
            }
        }
        #endregion
    }
}
