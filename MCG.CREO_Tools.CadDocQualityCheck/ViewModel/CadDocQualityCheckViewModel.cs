using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.WindchillCredential;
using MCG.CREO_Tools.CadDocQualityCheck.Configuration;
using MCG.CREO_Tools.CadDocQualityCheck.Exceptions;
using MCG.CREO_Tools.CadDocQualityCheck.View;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Services.Interfaces;
using pfcls;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class CadDocQualityCheckViewModel : ObservableObject, ICadDocQualityCheckViewModel
    {
        #region [REGION] Events Action
        public event EventHandler ActionInProgressEvent;
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

        public event EventHandler ActionDoneEvent;
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

        #region [REGION] Properties from Interface
        public CadDocQualityCheckDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private bool StopCurrentProcess { get; set; } = false;
        private Thread ListModelThread { get; set; } = null;
        private Dispatcher MainDispatcher { get; set; }
        private string MainAppFolder { get; set; }
        private CadDocQualityCheckDataConfiguration CurrentConfiguration { get; set; }
        private WindchillCredentialItem WindchillCredential { get; set; } = null;
        #endregion

        #region [REGION] Commands
        public ICommand CommandListCadDoc { get => new RelayCommand(() => ExecuteListCadDoc()); }
        public ICommand CommandCheckCadDoc { get => new RelayCommand(() => ExecuteCheckCadDoc()); }
        public ICommand CommandUpdateRelations { get => new RelayCommand(() => ExecuteUpdateRelations()); }
        public ICommand CommandUpdateAttributes { get => new RelayCommand(() => ExecuteUpdateAttributes()); }
        public ICommand CommandUpdateLayers { get => new RelayCommand(() => ExecuteUpdateLayers()); }
        public ICommand CommandUpdateUnits { get => new RelayCommand(() => ExecuteUpdateUnits()); }
        public ICommand CommandSelectUnselectAll { get => new RelayCommand<bool>((isselected) => ExecuteSelectUnselectAll(isselected)); }
        public ICommand CommandCopyPreRelations { get => new RelayCommand<CadDocQualityCheckItem>((cadItem) => ExecuteCopyPreRelations(cadItem)); }
        public ICommand CommandInitPreRelations { get => new RelayCommand<CadDocQualityCheckItem>((cadItem) => ExecuteInitPreRelations(cadItem)); }
        public ICommand CommandCopyPostRelations { get => new RelayCommand<CadDocQualityCheckItem>((cadItem) => ExecuteCopyPostRelations(cadItem)); }
        public ICommand CommandInitPostRelations { get => new RelayCommand<CadDocQualityCheckItem>((cadItem) => ExecuteInitPostRelations(cadItem)); }
        public ICommand CommandImportFromExcel { get => new RelayCommand(() => ExecuteImportFromExcel()); }
        public ICommand CommandOpenModelInCreo { get => new RelayCommand<bool>((isasynch) => ExecuteOpenModelInCreo(isasynch)); }
        public ICommand CommandCheckIn { get => new RelayCommand(() => ExecuteCheckIn()); }
        public ICommand CommandCheckOut { get => new RelayCommand(() => ExecuteCheckOut()); }
        public ICommand CommandDeleteSelectedCadDoc { get => new RelayCommand(() => ExecuteDeleteSelectedCadDoc()); }
        public ICommand CommandUpdateComponentAssembly { get => new RelayCommand(() => ExecuteUpdateComponentAssembly()); }
        #endregion

        #region [REGION] Init
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        private readonly ICreoMacroService _creoMacroService;
        private readonly ICreoFeatureService _creoFeatureService;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly ICreoParameterService _creoParameterService;
        private readonly ICreoLayerService _creoLayerService;
        private readonly IWindchillPartManagementService _windchillPartManagementService;

        public CadDocQualityCheckViewModel(IXmlSerializeTools xmlSerializeTools,
                                          ICreoSessionProvider creoSessionProvider,
                                          ICreoModelService creoModelService,
                                          ICreoMacroService creoMacroService,
                                          IWindchillCredentialService windchillCredentialService,
                                          ICreoParameterService creoParameterService,
                                          ICreoFeatureService creoFeatureService,
                                          ICreoLayerService creoLayerService)
        {
            try
            {
                _xmlSerializeTools = xmlSerializeTools;
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;
                _creoMacroService = creoMacroService;
                _windchillCredentialService = windchillCredentialService;
                _creoParameterService = creoParameterService;
                _creoFeatureService = creoFeatureService;
                _creoLayerService = creoLayerService;

                CurrentDataContext = new CadDocQualityCheckDataContext();

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                MainDispatcher = Dispatcher.CurrentDispatcher;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDataContext.ShowActionButton = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDataContext.ShowActionButton = e;

                CurrentConfiguration = _xmlSerializeTools.GetDeserializedXml<CadDocQualityCheckDataConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{CadDocQualityCheckConstants.ConfigurationFile}");
                CurrentDataContext.MandatoryLayers = CurrentConfiguration.ListStandardLayers.Where(layer => layer.ToBeCreatedIfMissing).ToList();

                CurrentDataContext.ListTemplate = new List<CadDocTemplate>();
                if (CurrentConfiguration.ListTemplate != null)
                    foreach (var item in CurrentConfiguration.ListTemplate)
                        CurrentDataContext.ListTemplate.Add(item.GetCadDocTemplate());

            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }

        }

        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteListCadDoc()
        {
            try
            {
                CurrentDataContext.IsCheckDone = false;

                if (!CurrentDataContext.IsSearchCadModelInProgress)
                {

                    RaiseActionInProgressEvent();
                    CurrentDataContext.IsLoadedFromCreo = true;
                    CurrentDataContext.IsNoActionInProgress = false;

                    if (CurrentDataContext.AllCadModels.Count != 0 && CurrentDataContext.AllCadModels.Count == CurrentDataContext.AllCadModels.Count(item => item.FromExcelImport))
                        ListModelThread = new Thread(new ThreadStart(OpenExcelListModelsInSession));
                    else
                        ListModelThread = new Thread(new ThreadStart(SearchListModelsInSession));

                    ListModelThread.IsBackground = true;
                    ListModelThread.Start();
                }
                else if (MessageBox.Show(McgWpfTools.GetStringResource("MUA_AbordProcessMsg"), McgWpfTools.GetStringResource("MUA_AbordProcessTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && ListModelThread != null)
                {
                    StopCurrentProcess = true;
                    CurrentDataContext.IsSearchCadModelInProgress = false;
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckCadDoc()
        {
            try
            {
                if (!CurrentDataContext.IsSearchCadModelInProgress)
                {

                    RaiseActionInProgressEvent();
                    CurrentDataContext.IsLoadedFromCreo = true;
                    CurrentDataContext.IsNoActionInProgress = false;

                    ListModelThread = new Thread(new ThreadStart(CheckAllCadDocumentsAsynch));
                    ListModelThread.IsBackground = true;
                    ListModelThread.Start();
                }
                else if (MessageBox.Show(McgWpfTools.GetStringResource("MUA_AbordProcessMsg"), McgWpfTools.GetStringResource("MUA_AbordProcessTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && ListModelThread != null)
                {
                    StopCurrentProcess = true;
                    CurrentDataContext.IsSearchCadModelInProgress = false;
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateRelations()
        {
            try
            {
                if (!CurrentDataContext.IsSearchCadModelInProgress)
                {

                    RaiseActionInProgressEvent();
                    CurrentDataContext.IsLoadedFromCreo = true;
                    CurrentDataContext.IsNoActionInProgress = false;

                    ListModelThread = new Thread(new ThreadStart(UpdateAllCadDocumentsRelationsAsynch));
                    ListModelThread.IsBackground = true;
                    ListModelThread.Start();
                }
                else if (MessageBox.Show(McgWpfTools.GetStringResource("MUA_AbordProcessMsg"), McgWpfTools.GetStringResource("MUA_AbordProcessTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && ListModelThread != null)
                {
                    StopCurrentProcess = true;
                    CurrentDataContext.IsSearchCadModelInProgress = false;
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateAttributes()
        {
            try
            {
                if (!CurrentDataContext.IsSearchCadModelInProgress)
                {

                    RaiseActionInProgressEvent();
                    CurrentDataContext.IsLoadedFromCreo = true;
                    CurrentDataContext.IsNoActionInProgress = false;

                    ListModelThread = new Thread(new ThreadStart(UpdateAllCadDocumentsAttributesAsynch));
                    ListModelThread.IsBackground = true;
                    ListModelThread.Start();
                }
                else if (MessageBox.Show(McgWpfTools.GetStringResource("MUA_AbordProcessMsg"), McgWpfTools.GetStringResource("MUA_AbordProcessTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && ListModelThread != null)
                {
                    StopCurrentProcess = true;
                    CurrentDataContext.IsSearchCadModelInProgress = false;
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateLayers()
        {
            try
            {
                if (!CurrentDataContext.IsSearchCadModelInProgress)
                {

                    RaiseActionInProgressEvent();
                    CurrentDataContext.IsLoadedFromCreo = true;
                    CurrentDataContext.IsNoActionInProgress = false;

                    ListModelThread = new Thread(new ThreadStart(UpdateAllCadDocumentsLayersAsynch));
                    ListModelThread.IsBackground = true;
                    ListModelThread.Start();
                }
                else if (MessageBox.Show(McgWpfTools.GetStringResource("MUA_AbordProcessMsg"), McgWpfTools.GetStringResource("MUA_AbordProcessTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && ListModelThread != null)
                {
                    StopCurrentProcess = true;
                    CurrentDataContext.IsSearchCadModelInProgress = false;
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateUnits()
        {
            try
            {
                if (!CurrentDataContext.IsSearchCadModelInProgress)
                {

                    RaiseActionInProgressEvent();
                    CurrentDataContext.IsLoadedFromCreo = true;
                    CurrentDataContext.IsNoActionInProgress = false;

                    ListModelThread = new Thread(new ThreadStart(UpdateAllCadDocumentsUnitsAsynch));
                    ListModelThread.IsBackground = true;
                    ListModelThread.Start();
                }
                else if (MessageBox.Show(McgWpfTools.GetStringResource("MUA_AbordProcessMsg"), McgWpfTools.GetStringResource("MUA_AbordProcessTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && ListModelThread != null)
                {
                    StopCurrentProcess = true;
                    CurrentDataContext.IsSearchCadModelInProgress = false;
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSelectUnselectAll(bool IsSelected)
        {
            try
            {
                foreach (var elem in CurrentDataContext.ShownCadModels)
                    elem.IsSelected = IsSelected;
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCopyPreRelations(CadDocQualityCheckItem CadItem)
        {
            try
            {
                CadItem.NewPreRegenRelations = CadItem.CurrentPreRegenRelations;
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteInitPreRelations(CadDocQualityCheckItem CadItem)
        {
            try
            {
                if (CadItem.Template != null && CadItem.Template.PreRegenRelations != null)
                    CadItem.NewPreRegenRelations = CadItem.Template.PreRegenRelations;
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCopyPostRelations(CadDocQualityCheckItem CadItem)
        {
            try
            {
                CadItem.NewPostRegenRelations = CadItem.CurrentPostRegenRelations;
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteInitPostRelations(CadDocQualityCheckItem CadItem)
        {
            try
            {
                if (CadItem.Template != null && CadItem.Template.PostRegenRelations != null)
                    CadItem.NewPostRegenRelations = CadItem.Template.PostRegenRelations;
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteImportFromExcel()
        {
            try
            {
                System.Windows.Forms.OpenFileDialog CurrentOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
                CurrentOpenFileDialog.Filter = "Excel files (*.xls,*.xlsx)|*.xls;*.xlsx";
                CurrentOpenFileDialog.ShowDialog();
                if (CurrentOpenFileDialog.FileName != null && CurrentOpenFileDialog.FileName != "")
                {
                    CurrentDataContext.IsSearchCadModelInProgress = true;

                    CurrentDataContext.IsLoadedFromCreo = false;
                    McgLinkToExcel CurrentMcgLinkToExcel = new McgLinkToExcel(CurrentOpenFileDialog.FileName);
                    List<ScannedDrawingImportItem> CurrentList = CurrentMcgLinkToExcel.Read<ScannedDrawingImportItem>("NUMBERS");

                    CurrentDataContext.ShownCadModels.Clear();
                    CurrentDataContext.AllCadModels.Clear();

                    RaiseActionInProgressEvent();

                    CheckWindchillCredential();

                    Thread aThread = new Thread(new ThreadStart(() =>
                    {
                        ImportFromExcelAsynch(CurrentList);
                    }));
                    aThread.IsBackground = true;
                    aThread.Start();
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ImportFromExcelAsynch(List<ScannedDrawingImportItem> CurrentList)
        {
            try
            {
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.NbModelsInSession = CurrentList.Count(); ;

                CadDocQualityCheckItem CurrentMassUpdateAttributeItem = null;
                bool AddAsm = false;
                foreach (var item in CurrentList)
                {
                    // If Number without extension, search part link Cad Doc
                    if (!item.NUMBER.Contains("."))
                    {
                        RestOdataWtPart TemPartObj = _windchillPartManagementService.GetOneWtPartWithWtDocumentAssociation(WindchillCredential.WindchillCredential, item.NUMBER);

                        if (TemPartObj != null && TemPartObj.PartDocAssociations != null)
                            foreach (var cad in TemPartObj.PartDocAssociations)
                            {
                                if (cad.RelatedCADDoc != null && cad.RelatedCADDoc.FileName != null)
                                {
                                    CurrentMassUpdateAttributeItem = new CadDocQualityCheckItem()
                                    {
                                        Number = cad.RelatedCADDoc.FileName.ToUpper(),
                                        IsUpdated = false,
                                        FromExcelImport = true,
                                        IsCheckedIn = true,
                                        IsCheckedOut = true,
                                        IsLocallyModified = true
                                    };

                                    CurrentDataContext.AllCadModels.Add(CurrentMassUpdateAttributeItem);
                                    MainDispatcher.Invoke(new Action(UpdateShownModelsList));
                                }
                            }
                    }

                    else
                    {

                        AddAsm = false;
                        CurrentMassUpdateAttributeItem = new CadDocQualityCheckItem()
                        {
                            Number = item.NUMBER.ToUpper(),
                            IsUpdated = false,
                            FromExcelImport = true,
                            IsCheckedIn = true,
                            IsCheckedOut = true,
                            IsLocallyModified = true
                        };
                        if (!CurrentMassUpdateAttributeItem.Number.Contains("."))
                        {
                            CurrentMassUpdateAttributeItem.Number = $"{CurrentMassUpdateAttributeItem.Number}.PRT";
                            AddAsm = true;
                        }

                        CurrentDataContext.AllCadModels.Add(CurrentMassUpdateAttributeItem);
                        MainDispatcher.Invoke(new Action(UpdateShownModelsList));

                        if (AddAsm)
                        {
                            CurrentMassUpdateAttributeItem = new CadDocQualityCheckItem()
                            {
                                Number = item.NUMBER.ToUpper(),
                                IsUpdated = false,
                                FromExcelImport = true,
                                IsCheckedIn = true,
                                IsCheckedOut = true,
                                IsLocallyModified = true
                            };
                            CurrentMassUpdateAttributeItem.Number = $"{CurrentMassUpdateAttributeItem.Number}.ASM";
                            
                            CurrentDataContext.AllCadModels.Add(CurrentMassUpdateAttributeItem);
                            MainDispatcher.Invoke(new Action(UpdateShownModelsList));
                        }
                    }

                    CurrentDataContext.NbModelsInSessionInProgress++;
                }
            }
            catch (Exception ex)
            {
                StopCurrentProcess = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.IsNoActionInProgress = true;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = "";
            }
        }

        private void ExecuteOpenModelInCreo(bool InAsynch = false)
        {
            try
            {
                if (CurrentDataContext.SelectedItem != null)
                {
                    EPMDocument CurrentEpm = new EPMDocument(CurrentDataContext.SelectedItem.Number, CurrentDataContext.SelectedItem.Number, CurrentDataContext.SelectedItem.Number);

                    if (InAsynch)
                    {
                        RaiseActionInProgressEvent();


                        Thread aThread = new Thread(new ThreadStart(() =>
                        {
                            CurrentEpm.OpenInCreo( _creoSessionProvider, _creoModelService);
                            RaiseActionDoneEvent();
                        }));
                        aThread.IsBackground = true;
                        aThread.Start();
                    }
                    else
                        CurrentEpm.OpenInCreo(_creoSessionProvider, _creoModelService);
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckIn()
        {
            try
            {
                foreach (var cad in CurrentDataContext.ShownCadModels.Where(item => item.IsSelected && item.CurrentCadModel != null && item.IsCheckedOut))
                {
                    try
                    {
                        _creoSessionProvider.Session.GetActiveServer().CheckinObjects(cad.CurrentCadModel, null);
                        cad.IsCheckedIn = true;
                        cad.IsCheckedOut = false;
                    }
                    catch (Exception)
                    {
                        cad.IsCheckedIn = false;
                        cad.IsCheckedOut = true;
                    }

                }

            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckOut()
        {
            try
            {
                foreach (var cad in CurrentDataContext.ShownCadModels.Where(item => item.IsSelected && item.CurrentCadModel != null && item.IsCheckedIn))
                {
                    try
                    {
                        _creoSessionProvider.Session.GetActiveServer().CheckoutObjects(cad.CurrentCadModel, null, true, null);
                        cad.IsCheckedIn = false;
                        cad.IsCheckedOut = true;
                    }
                    catch (Exception)
                    {
                        cad.IsCheckedIn = true;
                        cad.IsCheckedOut = false;
                    }

                }

            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDeleteSelectedCadDoc()
        {
            try
            {
                var toBeRemoved = CurrentDataContext.ShownCadModels.Where(item => item.IsSelected).ToList();
                foreach (var cad in toBeRemoved)
                    CurrentDataContext.ShownCadModels.Remove(cad);
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateComponentAssembly()
        {
            try
            {
                IpfcSolid ActiveModel = (IpfcSolid)_creoModelService.GetActiveModel();

                UpdateAllComponentAssembly(ActiveModel);
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        #endregion

        #region [REGION] Read CREO
        private void SearchListModelsInSession()
        {
            try
            {
                CurrentDataContext.TextStatusBar = McgWpfTools.GetStringResource("CQC_SearchCadDocInProgress");
                CurrentDataContext.IsSearchCadModelInProgress = true;

                _creoSessionProvider.CheckConnection();
                _creoModelService.SearchModelsInSession();

                List<object> ListModels;

                if (CurrentDataContext.IsOnlyActiveModel)
                {
                    GetActiveModelDependenciesAsynch();
                    MainDispatcher.Invoke(new Action(UpdateShownModelsList));
                    if (StopCurrentProcess)
                    {
                        CurrentDataContext.AllCadModels.Clear();
                        CurrentDataContext.NbModelsInSession = 1;
                        CurrentDataContext.NbModelsInSessionInProgress = 0;
                        StopCurrentProcess = false;
                        MainDispatcher.Invoke(new Action(UpdateShownModelsList));
                        return;
                    }
                }
                else
                {
                    if (CurrentDataContext.IsOnlyDisplayedModels)
                        ListModels = _creoModelService.ListModelsWindow;
                    else
                        ListModels = _creoModelService.ListModels;
                    if (ListModels != null)
                    {
                        CadDocQualityCheckItem CurrentCadDocQualityCheckItem = null;

                        CurrentDataContext.AllCadModels.Clear();
                        CurrentDataContext.NbModelsInSession = ListModels.Count;
                        CurrentDataContext.NbModelsInSessionInProgress = 0;

                        Regex RegexCadDoc = new Regex(@"\.prt|\.asm|\.drw", RegexOptions.IgnoreCase);
                        int Index = 0;

                        foreach (IpfcModel CurrentModel in ListModels)
                        {
                            if (RegexCadDoc.IsMatch(CurrentModel.FileName))
                            {

                                if (!StopCurrentProcess)
                                {
                                    CurrentDataContext.NbModelsInSessionInProgress = Index;
                                    CurrentCadDocQualityCheckItem = SearchCadModelInformation(CurrentModel);
                                    CurrentDataContext.AllCadModels.Add(CurrentCadDocQualityCheckItem);

                                    Index++;
                                }
                                else
                                {
                                    CurrentDataContext.AllCadModels.Clear();
                                    CurrentDataContext.NbModelsInSession = 1;
                                    CurrentDataContext.NbModelsInSessionInProgress = 0;
                                    StopCurrentProcess = false;
                                    MainDispatcher.Invoke(new Action(UpdateShownModelsList));
                                    return;
                                }
                            }
                        }

                        MainDispatcher.Invoke(new Action(UpdateShownModelsList));
                    }
                }

                GetAllRelations();
                GetAllAttributes();
                GetAllLayers();

            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.IsNoActionInProgress = true;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = "";
            }
        }

        private void OpenExcelListModelsInSession()
        {
            try
            {
                CurrentDataContext.TextStatusBar = McgWpfTools.GetStringResource("CQC_SearchCadDocInProgress");
                CurrentDataContext.IsSearchCadModelInProgress = true;

                _creoSessionProvider.CheckConnection();
                _creoModelService.SearchModelsInSession();

                List<CadDocQualityCheckItem> ListTempItem = new List<CadDocQualityCheckItem>();
                IpfcModel CurrentModel;
                CadDocQualityCheckItem TempItem = null;


                CurrentDataContext.NbModelsInSession = CurrentDataContext.AllCadModels.Count;
                CurrentDataContext.NbModelsInSessionInProgress = 0;


                foreach (var item in CurrentDataContext.AllCadModels)
                {

                    CurrentModel = _creoModelService.RetrieveModelOrNothing(item.Number.ToUpper());
                    if (CurrentModel != null)
                    {
                        TempItem = SearchCadModelInformation(CurrentModel);
                        ListTempItem.Add(TempItem);
                    }
                    else
                        item.IsFound = false;

                    CurrentDataContext.NbModelsInSessionInProgress++;
                }

                foreach (var item in ListTempItem)
                {
                    TempItem = CurrentDataContext.AllCadModels.FirstOrDefault(cad => cad.Number.ToUpper() == item.Number.ToUpper());
                    if (TempItem != null)
                    {
                        CurrentDataContext.AllCadModels.Remove(TempItem);
                        CurrentDataContext.AllCadModels.Add(item);
                    }
                }

                MainDispatcher.Invoke(new Action(UpdateShownModelsList));

                GetAllRelations();
                GetAllAttributes();
                GetAllLayers();

            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.IsNoActionInProgress = true;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = "";
            }
        }

        private void GetActiveModelDependenciesAsynch()
        {
            try
            {
                CurrentDataContext.AllCadModels.Clear();
                CurrentDataContext.NbModelsInSession = 0;
                CurrentDataContext.NbModelsInSessionInProgress = 0;

                // Check if Active Model available (3D)
                IpfcModel ActiveModel = _creoModelService.GetActiveModel();

                // if Active model not found, check if currentWindow is a drawxing
                if (ActiveModel == null)
                {
                    var CurrentMWindow = _creoSessionProvider.Session.get_CurrentWindow();
                    if (CurrentMWindow != null)
                        ActiveModel = CurrentMWindow.Model;
                }
                if (ActiveModel != null)
                {
                    CurrentDataContext.AllCadModels.Add(SearchCadModelInformation(ActiveModel));
                    GetAllDependenciesRecursive(ActiveModel);

                    // Check if drawing for active model+compo is in session
                    GetAllDependenciesDrawing();
                }
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void GetAllRelations()
        {
            try
            {
                Regex CommentLineRegex = new Regex(@"^/\*");
                CurrentDataContext.NbModelsInSession = CurrentDataContext.AllCadModels.Count;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = McgWpfTools.GetStringResource("CQC_SearchCadDocRelInProgress");
                foreach (var cad in CurrentDataContext.AllCadModels.Where(item => item.CurrentCadModel != null))
                {
                    if (cad.IsCheckedOut || CurrentDataContext.CheckUncheckedOutItem)
                    {
                        cad.CurrentPreRegenRelations = _creoParameterService.GetPreRegenRelations(cad.CurrentCadModel);
                        cad.CurrentPostRegenRelations = _creoParameterService.GetPostRegenRelations(cad.CurrentCadModel);
                        cad.PurgedPreRegenRelation = cad.CurrentPreRegenRelations.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Where(text => text != null && text.Trim() != "" && !CommentLineRegex.IsMatch(text.Trim())).ToList();
                        cad.PurgedPostRegenRelation = cad.CurrentPostRegenRelations.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Where(text => text != null && text.Trim() != "" && !CommentLineRegex.IsMatch(text.Trim())).ToList();

                        foreach (var line in cad.PurgedPreRegenRelation)
                            cad.ListCurrentPreRegenRelations.Add(new CadDocRelationLineItem()
                            {
                                Relation = line
                            });

                        foreach (var line in cad.PurgedPostRegenRelation)
                            cad.ListCurrentPostRegenRelations.Add(new CadDocRelationLineItem()
                            {
                                Relation = line
                            });

                    }
                    CurrentDataContext.NbModelsInSessionInProgress++;
                }
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void GetAllAttributes()
        {
            try
            {
                Regex CommentLineRegex = new Regex(@"^/\*");
                CurrentDataContext.NbModelsInSession = CurrentDataContext.AllCadModels.Count;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = McgWpfTools.GetStringResource("CQC_SearchCadDocAttribInProgress");

                List<CadDocAttributeItem> AttribList;

                foreach (var cad in CurrentDataContext.AllCadModels.Where(item => item.CurrentCadModel != null))
                {
                    if (cad.IsCheckedOut || CurrentDataContext.CheckUncheckedOutItem)
                    {
                        AttribList = GetOneCadDocAttributes(cad.CurrentCadModel);
                        foreach (var attrib in AttribList)
                            cad.ListAttributes.Add(attrib);
                    }
                    CurrentDataContext.NbModelsInSessionInProgress++;
                }
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private List<CadDocAttributeItem> GetOneCadDocAttributes(IpfcModel CurrentModel)
        {
            try
            {
                List<CadDocAttributeItem> ListAttrib = new List<CadDocAttributeItem>();
                IpfcParameterOwner ModelParamOwner = (IpfcParameterOwner)CurrentModel;
                IpfcParameters AllParams = ModelParamOwner.ListParams();
                string ParamName;
                List<IpfcParameter> ListParam = new List<IpfcParameter>();
                foreach (IpfcParameter attrib in AllParams)
                    ListParam.Add(attrib);

                Regex RegexAttribName = new Regex(CadDocQualityCheckConstants.RegExPtcAttributeName, RegexOptions.IgnoreCase);

                foreach (IpfcParameter attrib in ListParam.Where(attrib => !RegexAttribName.IsMatch(((IpfcNamedModelItem)attrib).Name)).OrderBy(attrib => ((IpfcNamedModelItem)attrib).Name))
                // foreach (IpfcParameter attrib in ListParam)
                {
                    ParamName = ((IpfcNamedModelItem)attrib).Name;

                    //if (!RegexAttribName.IsMatch(ParamName))
                    ListAttrib.Add(new CadDocAttributeItem()
                    {
                        Name = ParamName,
                        Type = _creoParameterService.GetParameterType(attrib).ToString(),
                        IsDesignated = ((IpfcBaseParameter)attrib).IsDesignated,
                        Attribute = attrib
                    });
                }

                return ListAttrib;
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void GetAllLayers()
        {
            try
            {
                CurrentDataContext.NbModelsInSession = CurrentDataContext.AllCadModels.Count;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = McgWpfTools.GetStringResource("CQC_SearchCadDocLayerInProgress");
                IpfcModelItems CadModelItems;
                foreach (var cad in CurrentDataContext.AllCadModels.Where(item => item.CurrentCadModel != null))
                {
                    if (cad.IsCheckedOut || CurrentDataContext.CheckUncheckedOutItem)
                    {
                        CadModelItems = ((IpfcModelItemOwner)cad.CurrentCadModel).ListItems(EpfcModelItemType.EpfcITEM_LAYER);
                        if (CadModelItems != null)
                        {
                            cad.ListLayers.Clear();
                            foreach (var layer in CadModelItems)
                            {
                                cad.ListLayers.Add(new CadDocLayerItem()
                                {
                                    LayerItem = (IpfcLayer)layer,
                                    Name = ((IpfcModelItem)layer).GetName(),
                                    State = ObjectState.CREATED,
                                    DisplayStatus = (EpfcDisplayStatus)((IpfcLayer)layer).Status,
                                    IsDisplayed = ((EpfcDisplayStatus)((IpfcLayer)layer).Status) == EpfcDisplayStatus.EpfcLAYER_NORMAL
                                });
                            }
                        }
                    }
                    CurrentDataContext.NbModelsInSessionInProgress++;
                }
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private CadDocQualityCheckItem SearchCadModelInformation(IpfcModel CurrentModel)
        {
            try
            {
                CadDocQualityCheckItem CurrentCadDocQualityCheckItem = new CadDocQualityCheckItem();
                CREOModelStatus CurrentCREOModelStatus;

                CurrentCadDocQualityCheckItem.CurrentCadModel = CurrentModel;
                CurrentCadDocQualityCheckItem.Number = CurrentModel.FileName;

                CurrentCadDocQualityCheckItem.IsCheckedOut = false;
                CurrentCadDocQualityCheckItem.IsCheckedIn = true;
                CurrentCadDocQualityCheckItem.IsSelected = false;
                CurrentCREOModelStatus = _creoModelService.GetModelStatus(CurrentModel);

                if (CurrentCREOModelStatus == CREOModelStatus.CHECKEDOUT || CurrentCREOModelStatus == CREOModelStatus.NEWINSESSION)
                {
                    CurrentCadDocQualityCheckItem.IsCheckedOut = true;
                    CurrentCadDocQualityCheckItem.IsCheckedIn = false;
                    CurrentCadDocQualityCheckItem.IsModifiable = true;
                }
                else if (CurrentCREOModelStatus == CREOModelStatus.LOCALLYMODIFIED)
                {
                    CurrentCadDocQualityCheckItem.IsCheckedIn = true;
                    CurrentCadDocQualityCheckItem.IsLocallyModified = true;
                    CurrentCadDocQualityCheckItem.IsModifiable = true;
                }
                else
                {
                    CurrentCadDocQualityCheckItem.IsReadOnly = true;
                    CurrentCadDocQualityCheckItem.IsCheckedIn = true;
                    CurrentCadDocQualityCheckItem.IsModifiable = false;
                }

                CurrentCadDocQualityCheckItem.IsUpdated = false;

                if (CurrentCadDocQualityCheckItem.Number.ToUpper().Contains(".PRT"))
                {
                    var SolidModel = (IpfcSolid)CurrentModel;
                    var SolidModelBody = SolidModel.GetDefaultBody();

                    // Check if BulkItem
                    bool IsBulk = _creoFeatureService.IsBulkItem(CurrentModel);
                    if (IsBulk)
                    {
                        CurrentCadDocQualityCheckItem.CadDocType = EpmDocumentTypeEnum.BULK;
                        CurrentCadDocQualityCheckItem.CadDocSubType = EpmDocumentTypeEnum.BULK;
                    }
                    else if (SolidModelBody.IsSheetmetal())
                    {
                        CurrentCadDocQualityCheckItem.CadDocType = EpmDocumentTypeEnum.SHEETMETAL;
                        CurrentCadDocQualityCheckItem.CadDocSubType = EpmDocumentTypeEnum.PRT;
                    }
                    else if (_creoFeatureService.IsInstance(CurrentModel))
                    {
                        CurrentCadDocQualityCheckItem.CadDocType = EpmDocumentTypeEnum.PRT;
                        CurrentCadDocQualityCheckItem.CadDocSubType = EpmDocumentTypeEnum.PRT_INST;
                    }
                    else if (_creoFeatureService.IsGeneric(CurrentModel))
                    {
                        CurrentCadDocQualityCheckItem.CadDocType = EpmDocumentTypeEnum.PRT;
                        CurrentCadDocQualityCheckItem.CadDocSubType = EpmDocumentTypeEnum.PRT_GEN;
                    }
                    else
                    {
                        if (((IpfcSolid)CurrentModel).IsSkeleton)
                            CurrentCadDocQualityCheckItem.CadDocSubType = EpmDocumentTypeEnum.SKEL;
                        else
                            CurrentCadDocQualityCheckItem.CadDocSubType = EpmDocumentTypeEnum.PRT;

                        CurrentCadDocQualityCheckItem.CadDocType = EpmDocumentTypeEnum.PRT;
                    }

                }
                else if (CurrentCadDocQualityCheckItem.Number.ToUpper().Contains(".ASM"))
                {
                    if (_creoFeatureService.IsInstance(CurrentModel))
                    {
                        CurrentCadDocQualityCheckItem.CadDocSubType = EpmDocumentTypeEnum.ASM_INST;
                    }
                    else if (_creoFeatureService.IsGeneric(CurrentModel))
                    {
                        CurrentCadDocQualityCheckItem.CadDocSubType = EpmDocumentTypeEnum.ASM_GEN;
                    }
                    else
                        CurrentCadDocQualityCheckItem.CadDocSubType = EpmDocumentTypeEnum.ASM;

                    CurrentCadDocQualityCheckItem.CadDocType = EpmDocumentTypeEnum.ASM;
                }
                else if (CurrentCadDocQualityCheckItem.Number.ToUpper().Contains(".DRW"))
                {
                    CurrentCadDocQualityCheckItem.CadDocType = EpmDocumentTypeEnum.DRW;
                    CurrentCadDocQualityCheckItem.CadDocSubType = EpmDocumentTypeEnum.DRW;
                }

                // Search Default Unit
                if (CurrentCadDocQualityCheckItem.CadDocType != EpmDocumentTypeEnum.DRW)
                    CurrentCadDocQualityCheckItem.DefaultUnits = ((IpfcSolid)CurrentModel).GetPrincipalUnits();

                return CurrentCadDocQualityCheckItem;
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }

        }

        private void GetAllDependenciesRecursive(IpfcModel CurrentModel)
        {
            try
            {
                if (!StopCurrentProcess)


                    if (CurrentModel != null)
                    {
                        IpfcDependencies AllDependencies = CurrentModel.ListDependencies();
                        if (AllDependencies != null)
                        {
                            CurrentDataContext.NbModelsInSession += AllDependencies.Count;
                            IpfcDependency TempDep = null;
                            IpfcModelDescriptor TempModDesc = null;
                            IpfcModel TempModel = null;
                            foreach (var item in AllDependencies)
                            {
                                if (StopCurrentProcess) return;

                                TempDep = (IpfcDependency)item;
                                TempModDesc = TempDep.DepModel;
                                TempModel = _creoSessionProvider.Session.GetModelFromDescr(TempModDesc);

                                if (TempModel != null)
                                {
                                    // Assembly
                                    if (TempModDesc.Type == 0)
                                    {
                                        if (!CurrentDataContext.AllCadModels.Any((cad) => cad.CurrentCadModel.FileName == TempModel.FileName))
                                        {
                                            CurrentDataContext.AllCadModels.Add(SearchCadModelInformation(TempModel));
                                            GetAllDependenciesRecursive(TempModel);
                                        }
                                    }
                                    // Part
                                    else if (TempModDesc.Type == 1)
                                    {
                                        if (!CurrentDataContext.AllCadModels.Any((cad) => cad.CurrentCadModel.FileName == TempModel.FileName))
                                        {
                                            CurrentDataContext.AllCadModels.Add(SearchCadModelInformation(TempModel));
                                        }
                                    }
                                    // Drawing
                                    else if (TempModDesc.Type == 2)
                                    {

                                    }
                                }
                                CurrentDataContext.NbModelsInSessionInProgress++;
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void GetAllDependenciesDrawing()
        {
            try
            {
                var ListModels = _creoModelService.ListModels;
                Dictionary<string, IpfcModel> ListModelsDic = new Dictionary<string, IpfcModel>();
                foreach (var item in ListModels)
                    ListModelsDic.Add(((IpfcModel)item).FileName, (IpfcModel)item);

                List<CadDocQualityCheckItem> NewList = new List<CadDocQualityCheckItem>();
                Regex RegexNumber = null;
                string tempNumber = "";
                foreach (var cad in CurrentDataContext.AllCadModels)
                {
                    tempNumber = cad.Number.Split('.').FirstOrDefault();
                    RegexNumber = new Regex($@"^{cad.Number}.+drw$", RegexOptions.IgnoreCase);
                    var TempList = ListModelsDic.Where((item) => RegexNumber.IsMatch(item.Key)).ToList();
                    if (TempList != null)
                        foreach (var extraCad in TempList)
                            if (!CurrentDataContext.AllCadModels.Any((item) => item.Number == extraCad.Key) && !NewList.Any((item) => item.Number == extraCad.Key))
                            {
                                NewList.Add(SearchCadModelInformation(extraCad.Value));
                            }
                }

                foreach (var cad in NewList)
                    CurrentDataContext.AllCadModels.Add(cad);
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateShownModelsList()
        {
            try
            {
                CurrentDataContext.UpdateShownModelsList();
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void GetTemplateInformation()
        {
            try
            {
                IpfcModel TemplateCadModel = null;
                Regex CommentLineRegex = new Regex(@"^/\*");
                CREOCadModelItem CSysItem;
                List<CREOCadModelItem> RefPlans;

                EpfcModelType CadDocType = EpfcModelType.EpfcMDL_PART;

                foreach (var CurrentCadDocTemplate in CurrentDataContext.ListTemplate.Where(temp => !temp.IsLoaded && temp.ShouldBeLoaded))
                {
                    if (CurrentCadDocTemplate != null && CurrentCadDocTemplate.FileName != null && CurrentCadDocTemplate.CadDocType != null)
                    {
                        if (CurrentCadDocTemplate.CadDocType.ToUpper().Trim() == "PRT")
                            CadDocType = EpfcModelType.EpfcMDL_PART;
                        else if (CurrentCadDocTemplate.CadDocType.ToUpper().Trim() == "ASM")
                            CadDocType = EpfcModelType.EpfcMDL_ASSEMBLY;
                        else if (CurrentCadDocTemplate.CadDocType.ToUpper().Trim() == "DRW")
                            CadDocType = EpfcModelType.EpfcMDL_DRAWING;

                        try
                        {
                            TemplateCadModel = _creoModelService.RetrieveModel( CurrentCadDocTemplate.FileName, CadDocType);
                        }
                        catch (Exception)
                        {
                            TemplateCadModel = null;
                        }

                        if (TemplateCadModel != null)
                        {
                            CurrentCadDocTemplate.PreRegenRelations = _creoParameterService.GetPreRegenRelations(TemplateCadModel);
                            CurrentCadDocTemplate.PostRegenRelations = _creoParameterService.GetPostRegenRelations(TemplateCadModel);
                            CurrentCadDocTemplate.PurgedPreRegenRelations = CurrentCadDocTemplate.PreRegenRelations.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Where(text => text != null && text.Trim() != "" && !CommentLineRegex.IsMatch(text.Trim())).ToList();
                            CurrentCadDocTemplate.PurgedPostRegenRelations = CurrentCadDocTemplate.PostRegenRelations.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Where(text => text != null && text.Trim() != "" && !CommentLineRegex.IsMatch(text.Trim())).ToList();
                            CurrentCadDocTemplate.Attributes = GetOneCadDocAttributes(TemplateCadModel);


                            RefPlans = _creoFeatureService.GetRefPlanFromCadModel(TemplateCadModel);
                            CurrentCadDocTemplate.MainRefPlans = new List<string>();
                            if (RefPlans != null && RefPlans.Count >= 3)
                                for (int i = 0; i < 3; i++)
                                    CurrentCadDocTemplate.MainRefPlans.Add(RefPlans.ElementAt(i).Name);

                            CSysItem = _creoFeatureService.GetRefCoordinateSystemFromCadModel(TemplateCadModel).FirstOrDefault();
                            if (CSysItem != null)
                                CurrentCadDocTemplate.MainCoordSystem = CSysItem.Name;

                            CurrentCadDocTemplate.MandatoryLayers = CurrentDataContext.MandatoryLayers;

                            CurrentCadDocTemplate.IsLoaded = true;

                            try
                            {
                                TemplateCadModel.Erase();
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CurrentDataContext.IsTemplateInformationReaded = false;
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Check CAD Document
        private void CheckAllCadDocumentsAsynch()
        {
            try
            {
                // Read Template information

                CurrentDataContext.NbModelsInSession = CurrentDataContext.ShownCadModels.Count;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = McgWpfTools.GetStringResource("CQC_CheckCadDocInProgress");
                CurrentDataContext.IsSearchCadModelInProgress = true;
                foreach (var cadItem in CurrentDataContext.ShownCadModels.Where(item => item.CurrentCadModel != null))
                {
                    if (cadItem.IsCheckedOut || CurrentDataContext.CheckUncheckedOutItem)
                    {
                        SearchCadDocTemplate(cadItem);

                        if (cadItem.Template != null && cadItem.Template.FileName == cadItem.Number)
                            cadItem.IsExcluded = true;

                        if (!cadItem.IsExcluded)
                        {
                            GetTemplateInformation();

                            CheckCadDocRelations(cadItem);
                            CheckCadDocAttributes(cadItem);
                            CheckCadDocLayers(cadItem);
                            CheckCadDocumentMaterial(cadItem);
                            CheckCadDocumentUnit(cadItem);
                            CheckCadDocumentComponent(cadItem);
                            CheckCadDocumentFeature(cadItem);
                        }
                    }
                    CurrentDataContext.NbModelsInSessionInProgress++;
                }

                CurrentDataContext.IsCheckDone = true;
            }
            catch (Exception ex)
            {
                StopCurrentProcess = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.IsNoActionInProgress = true;
                CurrentDataContext.IsSearchCadModelInProgress = false;
            }
        }

        private void SearchCadDocTemplate(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                CadDocAttributeItem TypeAttrib = CurrentItem.ListAttributes.FirstOrDefault(attrib => attrib.Name == "TEMPLATE");
                CadDocTemplate CurrentTemplate = null;
                if (TypeAttrib != null && TypeAttrib.Attribute != null)
                    CurrentTemplate = CurrentDataContext.ListTemplate.FirstOrDefault(item => item.CadDocType == CurrentItem.CadDocType.ToString()
                                                                                                       && item.Template == TypeAttrib.Attribute.GetScaledValue().StringValue);
                if (CurrentTemplate == null)
                {
                    if (CurrentItem.CadDocType == EpmDocumentTypeEnum.ASM)
                        CurrentTemplate = CurrentDataContext.ListTemplate.FirstOrDefault(item => item.IsDefaultAsm);
                    else if (CurrentItem.CadDocType == EpmDocumentTypeEnum.PRT)
                        CurrentTemplate = CurrentDataContext.ListTemplate.FirstOrDefault(item => item.IsDefaultPrt);
                    else if (CurrentItem.CadDocType == EpmDocumentTypeEnum.SHEETMETAL)
                        CurrentTemplate = CurrentDataContext.ListTemplate.FirstOrDefault(item => item.IsDefaultSheetMetal);
                    else if (CurrentItem.CadDocType == EpmDocumentTypeEnum.BULK)
                        CurrentTemplate = CurrentDataContext.ListTemplate.FirstOrDefault(item => item.IsDefaultBulk);
                }

                if (CurrentTemplate != null)
                    CurrentTemplate.ShouldBeLoaded = true;

                CurrentItem.Template = CurrentTemplate;

            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckCadDocRelations(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.Template != null)
                {
                    CurrentItem.RelationsStatus = CadDocCheckStatus.OK;


                    CurrentItem.PreRelationsCheckResult = CheckOneRelation(CurrentItem.ListCurrentPreRegenRelations, CurrentItem.Template.PurgedPreRegenRelations);
                    CurrentItem.PostRelationsCheckResult = CheckOneRelation(CurrentItem.ListCurrentPostRegenRelations, CurrentItem.Template.PurgedPostRegenRelations);

                    if (!CurrentItem.PostRelationsCheckResult.IsRelationsOK || !CurrentItem.PreRelationsCheckResult.IsRelationsOK)
                        CurrentItem.RelationsStatus = CadDocCheckStatus.ISSUE;

                    if (CurrentItem.PreRelationsCheckResult != null)
                        foreach (var rel in CurrentItem.PreRelationsCheckResult.MissingRelations)
                            CurrentItem.ListCurrentPreRegenRelations.Add(rel);
                    if (CurrentItem.PostRelationsCheckResult != null)
                        foreach (var rel in CurrentItem.PostRelationsCheckResult.MissingRelations)
                            CurrentItem.ListCurrentPostRegenRelations.Add(rel);

                    if (CurrentItem.RelationsStatus == CadDocCheckStatus.ISSUE)
                    {
                        CurrentItem.NewPreRegenRelations = CurrentItem.Template.PreRegenRelations;
                        CurrentItem.NewPostRegenRelations = CurrentItem.Template.PostRegenRelations;

                    }

                    // Check if Regen RelationOK
                    if (CurrentItem.CurrentPreRegenRelations != null && CurrentItem.CurrentPreRegenRelations.Trim() != "")
                    {
                        try
                        {
                            ((IpfcRelationOwner)CurrentItem.CurrentCadModel).RegenerateRelations();
                            CurrentItem.IsPreRegenRelationsOk = true;
                        }
                        catch (Exception)
                        {
                            CurrentItem.IsPreRegenRelationsOk = false;
                        }
                    }
                    else
                        CurrentItem.IsPreRegenRelationsOk = true;

                    if (CurrentItem.CurrentPostRegenRelations != null && CurrentItem.CurrentPostRegenRelations.Trim() != "")
                    {
                        try
                        {
                            CurrentItem.CurrentCadModel.RegeneratePostRegenerationRelations();
                            CurrentItem.IsPostRegenRelationsOk = true;
                        }
                        catch (Exception)
                        {
                            CurrentItem.IsPostRegenRelationsOk = false;
                        }
                    }
                    else
                        CurrentItem.IsPostRegenRelationsOk = true;

                }
                else
                    CurrentItem.Comment = "Template missing";
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private CadDocRelationsList CheckOneRelation(ObservableCollection<CadDocRelationLineItem> ToBeChecked, List<string> Relations)
        {
            try
            {
                List<CadDocRelationLineItem> TempToBeChecked = new List<CadDocRelationLineItem>();
                TempToBeChecked.AddRange(ToBeChecked);
                List<CadDocRelationLineItem> TempRelations = new List<CadDocRelationLineItem>();
                foreach (var line in Relations)
                    TempRelations.Add(new CadDocRelationLineItem() { Relation = line });

                CadDocRelationLineItem TempRel;

                foreach (var line in ToBeChecked)
                {
                    TempRel = TempRelations.FirstOrDefault(rel => rel.Relation == line.Relation);
                    if (TempRel != null)
                        TempRelations.Remove(TempRel);
                }
                foreach (var rel in TempRelations)
                    rel.IsMissing = true;

                foreach (var line in Relations)
                {
                    TempRel = TempToBeChecked.FirstOrDefault(rel => rel.Relation == line);
                    if (TempRel != null)
                        TempToBeChecked.Remove(TempRel);
                }

                foreach (var rel in TempToBeChecked)
                    rel.IsExtra = true;

                bool isRelationOk = false;
                if (TempRelations.Count == 0 && TempToBeChecked.Count == 0)
                    isRelationOk = true;

                return new CadDocRelationsList()
                {
                    ExtraRelations = TempToBeChecked,
                    MissingRelations = TempRelations,
                    IsRelationsOK = isRelationOk
                };
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckCadDocAttributes(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                CurrentItem.AttributesStatus = CadDocCheckStatus.OK;
                if (CurrentItem.Template != null)
                    CurrentItem.AttributesStatus = CheckOneAttribute(CurrentItem.ListAttributes, CurrentItem.Template.Attributes);
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private CadDocCheckStatus CheckOneAttribute(ObservableCollection<CadDocAttributeItem> ToBeChecked, List<CadDocAttributeItem> Attributes)
        {
            try
            {
                CadDocAttributeItem CadAttrib;
                CadDocCheckStatus GlobalStatus = CadDocCheckStatus.OK;
                foreach (var attrib in Attributes)
                {
                    CadAttrib = ToBeChecked.FirstOrDefault(item => item.Name == attrib.Name);
                    if (CadAttrib != null)
                    {
                        CadAttrib.IsTemplateAttrib = true;
                        if (CadAttrib.IsDesignated != attrib.IsDesignated)
                        {
                            CadAttrib.IsDesignatedOk = false;
                            CadAttrib.IsDesignated = attrib.IsDesignated;
                            CadAttrib.AttributeStatus = CadDocCheckStatus.ISSUE;
                            CadAttrib.IsUpdated = true;
                            GlobalStatus = CadDocCheckStatus.ISSUE;
                        }
                    }
                    else
                    {
                        ToBeChecked.Add(new CadDocAttributeItem()
                        {
                            Name = attrib.Name,
                            AttributeStatus = CadDocCheckStatus.ISSUE,
                            IsDesignated = attrib.IsDesignated,
                            IsMissing = true,
                            IsDesignatedOk = true,
                            Type = attrib.Type,
                            IsTemplateAttrib = true
                        });
                        GlobalStatus = CadDocCheckStatus.ISSUE;
                    }
                }

                // Change IsDesignated to fasle if not an attrib from the template
                foreach (var attrib in ToBeChecked.Where(item => !item.IsTemplateAttrib && item.IsDesignated))
                {
                    attrib.IsDesignated = false;
                    attrib.IsDesignatedOk = false;
                    attrib.AttributeStatus = CadDocCheckStatus.WARNING;
                    attrib.IsUpdated = true;
                    GlobalStatus = UpdateCheckStatus(GlobalStatus, CadDocCheckStatus.WARNING);
                }

                if (ToBeChecked.FirstOrDefault(item => item.AttributeStatus == CadDocCheckStatus.WARNING) != null)
                    GlobalStatus = CadDocCheckStatus.WARNING;

                if (ToBeChecked.FirstOrDefault(item => item.AttributeStatus == CadDocCheckStatus.ISSUE) != null)
                    GlobalStatus = CadDocCheckStatus.ISSUE;


                return GlobalStatus;
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckCadDocLayers(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                CurrentItem.LayersStatus = CadDocCheckStatus.OK;

                var ListLayerToRemove = CurrentItem.ListLayers.Where((layer) => !CurrentConfiguration.ListStandardLayers.Select((item) => item.Name).Contains(layer.Name)).ToList();
                foreach (var layer in ListLayerToRemove)
                {
                    layer.State = ObjectState.TO_BE_REMOVED;
                    CurrentItem.LayersStatus = CadDocCheckStatus.ISSUE;
                    layer.LayerStatus = CadDocCheckStatus.ISSUE;
                }

                foreach (var layer in CurrentItem.ListLayers.Where((item) => item.State == ObjectState.CREATED))
                {
                    var tempLayer = CurrentConfiguration.ListStandardLayers.FirstOrDefault((item) => item.Name == layer.Name);
                    if (tempLayer.IsDisplayed)
                        layer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_NORMAL;
                    else
                        layer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_BLANK;
                    if (tempLayer.IsDisplayed != layer.IsDisplayed)
                    {
                        layer.LayerStatus = UpdateCheckStatus(layer.LayerStatus, CadDocCheckStatus.WARNING);
                        CurrentItem.LayersStatus = UpdateCheckStatus(CurrentItem.LayersStatus, layer.LayerStatus);
                    }
                    else
                    {
                        layer.LayerStatus = UpdateCheckStatus(layer.LayerStatus, CadDocCheckStatus.OK);
                        CurrentItem.LayersStatus = UpdateCheckStatus(CurrentItem.LayersStatus, layer.LayerStatus);
                    }
                    layer.ListModelItems = _creoLayerService.GetModelItemsFromLayer(layer.LayerItem).Select(item => item.Item).ToList();
                }

                CurrentItem.ListRefAxis = _creoFeatureService.GetRefAxisFromCadModel(CurrentItem.CurrentCadModel);
                CurrentItem.ListRefPlans = _creoFeatureService.GetRefPlanFromCadModel(CurrentItem.CurrentCadModel);
                CurrentItem.ListRefPoints = _creoFeatureService.GetRefPointFromCadModel(CurrentItem.CurrentCadModel);
                CurrentItem.ListRefCSys = _creoFeatureService.GetRefCoordinateSystemFromCadModel(CurrentItem.CurrentCadModel);

                // Check Mandatory Layers that alredy exist: 
                CadDocLayerItem CurrentLayer;
                CREOCadModelItem CurrentModelItem;
                List<CREOCadModelItem> TempList = new List<CREOCadModelItem>();
                if (CurrentItem.ListRefAxis != null)
                    TempList.AddRange(CurrentItem.ListRefAxis);
                if (CurrentItem.ListRefPlans != null)
                    TempList.AddRange(CurrentItem.ListRefPlans);
                if (CurrentItem.ListRefPoints != null)
                    TempList.AddRange(CurrentItem.ListRefPoints);
                if (CurrentItem.ListRefCSys != null)
                    TempList.AddRange(CurrentItem.ListRefCSys);

                foreach (var layer in CurrentDataContext.MandatoryLayers)
                {
                    CurrentLayer = CurrentItem.ListLayers.FirstOrDefault(item => item.Name == layer.Name);
                    if (CurrentLayer != null)
                    {
                        foreach (var item in CurrentLayer.ListModelItems)
                        {
                            CurrentModelItem = null;
                            CurrentModelItem = TempList.FirstOrDefault(elem => ((object)item.GetName()) != DBNull.Value && elem.Name == item.GetName());
                            if (CurrentModelItem != null)
                                CurrentModelItem.IsInLayer = true;
                        }
                    }
                }

                // Add New Layers
                foreach (var layer in CurrentDataContext.MandatoryLayers)
                {
                    if (CurrentItem.ListLayers.FirstOrDefault(item => item.Name == layer.Name) == null)
                        if (CheckIfLayerToBeCreated(CurrentItem, layer))
                            CurrentItem.LayersStatus = CadDocCheckStatus.ISSUE;
                }

            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private bool CheckIfLayerToBeCreated(CadDocQualityCheckItem CurrentItem, CadDocLayerItemConfig Layer)
        {
            try
            {
                List<CREOCadModelItem> ListItems = new List<CREOCadModelItem>();
                CREOCadModelItem CurrentModelItem;
                CadDocLayerItem CurrentLayer;
                switch (Layer.RefType)
                {
                    case "MAINPLAN":
                        if (CurrentItem.Template != null)
                        {
                            foreach (var plan in CurrentItem.Template.MainRefPlans)
                            {
                                CurrentModelItem = CurrentItem.ListRefPlans?.FirstOrDefault(item => item.Name == plan);
                                if (CurrentModelItem != null)
                                {
                                    ListItems.Add(CurrentModelItem);
                                    CurrentModelItem.IsInLayer = true;
                                }
                            }
                            if (ListItems?.Count > 0)
                            {
                                CurrentLayer = new CadDocLayerItem()
                                {
                                    IsDisplayed = Layer.IsDisplayed,
                                    LayerStatus = CadDocCheckStatus.ISSUE,
                                    State = ObjectState.NEW,
                                    ListModelItems = new List<IpfcModelItem>(),
                                    Name = Layer.Name
                                };
                                if (CurrentLayer.IsDisplayed)
                                    CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_NORMAL;
                                else
                                    CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_BLANK;
                                CurrentLayer.ListModelItems.AddRange(ListItems.Select(layer => layer.Item));
                                CurrentItem.ListLayers.Add(CurrentLayer);

                                return true;
                            }
                        }
                        break;
                    case "PLAN":
                        ListItems = CurrentItem.ListRefPlans?.Where(layer => !layer.IsInLayer).ToList();
                        if (ListItems?.Count > 0)
                        {
                            CurrentLayer = new CadDocLayerItem()
                            {
                                IsDisplayed = Layer.IsDisplayed,
                                LayerStatus = CadDocCheckStatus.ISSUE,
                                State = ObjectState.NEW,
                                ListModelItems = new List<IpfcModelItem>(),
                                Name = Layer.Name
                            };
                            if (CurrentLayer.IsDisplayed)
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_NORMAL;
                            else
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_BLANK;
                            CurrentLayer.ListModelItems.AddRange(ListItems.Select(layer => layer.Item));
                            CurrentItem.ListLayers.Add(CurrentLayer);
                            return true;
                        }
                        break;
                    case "CSYS":
                        ListItems = CurrentItem.ListRefCSys?.Where(layer => !layer.IsInLayer).ToList();
                        if (ListItems?.Count > 0)
                        {
                            CurrentLayer = new CadDocLayerItem()
                            {
                                IsDisplayed = Layer.IsDisplayed,
                                LayerStatus = CadDocCheckStatus.ISSUE,
                                State = ObjectState.NEW,
                                ListModelItems = new List<IpfcModelItem>(),
                                Name = Layer.Name
                            };
                            if (CurrentLayer.IsDisplayed)
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_NORMAL;
                            else
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_BLANK;
                            CurrentLayer.ListModelItems.AddRange(ListItems.Select(layer => layer.Item));
                            CurrentItem.ListLayers.Add(CurrentLayer);
                            return true;
                        }
                        break;
                    case "AXIS":
                        ListItems = CurrentItem.ListRefAxis?.Where(layer => !layer.IsInLayer).ToList();
                        if (ListItems?.Count > 0)
                        {
                            CurrentLayer = new CadDocLayerItem()
                            {
                                IsDisplayed = Layer.IsDisplayed,
                                LayerStatus = CadDocCheckStatus.ISSUE,
                                State = ObjectState.NEW,
                                ListModelItems = new List<IpfcModelItem>(),
                                Name = Layer.Name
                            };
                            if (CurrentLayer.IsDisplayed)
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_NORMAL;
                            else
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_BLANK;
                            CurrentLayer.ListModelItems.AddRange(ListItems.Select(layer => layer.Item));
                            CurrentItem.ListLayers.Add(CurrentLayer);
                            return true;
                        }
                        break;
                    case "POINTS":
                        ListItems = CurrentItem.ListRefPoints?.Where(layer => !layer.IsInLayer).ToList();
                        if (ListItems?.Count > 0)
                        {
                            CurrentLayer = new CadDocLayerItem()
                            {
                                IsDisplayed = Layer.IsDisplayed,
                                LayerStatus = CadDocCheckStatus.ISSUE,
                                State = ObjectState.NEW,
                                ListModelItems = new List<IpfcModelItem>(),
                                Name = Layer.Name
                            };
                            if (CurrentLayer.IsDisplayed)
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_NORMAL;
                            else
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_BLANK;
                            CurrentLayer.ListModelItems.AddRange(ListItems.Select(layer => layer.Item));
                            CurrentItem.ListLayers.Add(CurrentLayer);
                            return true;
                        }
                        break;
                    default:
                        break;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private CadDocCheckStatus UpdateCheckStatus(CadDocCheckStatus CurrentStatus, CadDocCheckStatus NewStatus)
        {
            try
            {
                if (CurrentStatus == CadDocCheckStatus.ISSUE)
                    return CadDocCheckStatus.ISSUE;
                if ((CurrentStatus == CadDocCheckStatus.WARNING || CurrentStatus == CadDocCheckStatus.OK) && NewStatus == CadDocCheckStatus.ISSUE)
                    return CadDocCheckStatus.ISSUE;
                if ((CurrentStatus == CadDocCheckStatus.OK || CurrentStatus == CadDocCheckStatus.WARNING) && NewStatus == CadDocCheckStatus.WARNING)
                    return CadDocCheckStatus.WARNING;
                if (CurrentStatus == CadDocCheckStatus.WARNING && NewStatus == CadDocCheckStatus.OK)
                    return CadDocCheckStatus.WARNING;

                return NewStatus;

            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckCadDocumentMaterial(CadDocQualityCheckItem CurrentItem)
        {
            try
            {

                if (CurrentItem.CadDocType == EpmDocumentTypeEnum.PRT || CurrentItem.CadDocType == EpmDocumentTypeEnum.SHEETMETAL)
                {
                    var material = _creoFeatureService.GetCurrentMaterial(CurrentItem.CurrentCadModel);
                    CurrentItem.MaterialStatus = CadDocCheckStatus.OK;
                    if (material == null)
                    {
                        CurrentItem.IsMaterialAssigned = false;
                        CurrentItem.MaterialStatus = CadDocCheckStatus.ISSUE;
                    }
                    else if (material.Name == "PTC_SYSTEM_MTRL_PROPS")
                    {
                        CurrentItem.IsMaterialAssigned = false;
                        CurrentItem.MaterialStatus = CadDocCheckStatus.ISSUE;
                    }
                    else
                    {
                        try
                        {
                            if (material.Condition == null)
                            {
                                CurrentItem.IsMaterialConditionDefined = false;
                                CurrentItem.MaterialStatus = CadDocCheckStatus.ISSUE;
                            }
                        }
                        catch (Exception)
                        {
                            CurrentItem.IsMaterialConditionDefined = false;
                            CurrentItem.MaterialStatus = CadDocCheckStatus.ISSUE;
                        }

                        if (material.Name != null && material.Name.Contains("UNDEFINED"))
                        {
                            CurrentItem.IsNotDefaultMaterialAssigned = false;
                            CurrentItem.MaterialStatus = UpdateCheckStatus(CurrentItem.MaterialStatus, CadDocCheckStatus.WARNING);
                        }
                    }
                }
                else
                {
                    CurrentItem.IsMaterialAssigned = true;
                    CurrentItem.IsNotDefaultMaterialAssigned = true;
                    CurrentItem.IsMaterialConditionDefined = true;
                    CurrentItem.MaterialStatus = CadDocCheckStatus.OK;
                }

            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckCadDocumentUnit(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.CadDocType != EpmDocumentTypeEnum.DRW && CurrentItem.DefaultUnits != null && !CurrentItem.DefaultUnits.Name.Contains(CadDocQualityCheckConstants.DefaultUnits))
                    CurrentItem.IsUnitsOk = false;
                else
                    CurrentItem.IsUnitsOk = true;

            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckCadDocumentComponent(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.CadDocType == EpmDocumentTypeEnum.ASM)
                {
                    var ListComp = _creoFeatureService.GetAllComponents(CurrentItem.CurrentCadModel);
                    if (ListComp.FirstOrDefault(item => item.IsFrozen) != null)
                        CurrentItem.ComponentStatus = CadDocCheckStatus.ISSUE;
                    else if (ListComp.FirstOrDefault(item => !item.IsFullyConstrain || item.IsSuppressed) != null)
                        CurrentItem.ComponentStatus = CadDocCheckStatus.WARNING;
                    else
                        CurrentItem.ComponentStatus = CadDocCheckStatus.OK;

                    foreach (var item in ListComp)
                    {
                        if (item.IsFrozen)
                            CurrentItem.ListQualityCheckResult.Add(CreateResultItem(CurrentItem, CadDocCheckStatus.ISSUE, "CQC_QualCheckMsg001", new string[1] { item.Name }));
                        else if (item.IsSuppressed)
                            CurrentItem.ListQualityCheckResult.Add(CreateResultItem(CurrentItem, CadDocCheckStatus.WARNING, "CQC_QualCheckMsg002", new string[1] { item.Name }));
                        else if (!item.IsFullyConstrain)
                            CurrentItem.ListQualityCheckResult.Add(CreateResultItem(CurrentItem, CadDocCheckStatus.WARNING, "CQC_QualCheckMsg003", new string[1] { item.Name }));
                    }
                }
                else
                    CurrentItem.ComponentStatus = CadDocCheckStatus.NONE;

            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckCadDocumentFeature(CadDocQualityCheckItem CurrentItem)
        {
            try
            {

                if (CurrentItem.CadDocType != EpmDocumentTypeEnum.DRW)

                {
                    var ListComp = _creoFeatureService.GetAllFeatures(CurrentItem.CurrentCadModel);
                    if (ListComp.FirstOrDefault(item => item.IsFrozen) != null)
                        CurrentItem.FeatureStatus = CadDocCheckStatus.ISSUE;
                    else if (ListComp.FirstOrDefault(item => !item.IsFullyConstrain && !item.IsSuppressed) != null)
                        CurrentItem.FeatureStatus = CadDocCheckStatus.ISSUE;
                    else if (ListComp.FirstOrDefault(item => item.IsSuppressed) != null)
                        CurrentItem.FeatureStatus = CadDocCheckStatus.WARNING;
                    else
                        CurrentItem.FeatureStatus = CadDocCheckStatus.OK;

                    foreach (var item in ListComp)
                    {
                        if (item.IsFrozen)
                            CurrentItem.ListQualityCheckResult.Add(CreateResultItem(CurrentItem, CadDocCheckStatus.ISSUE, "CQC_QualCheckMsg004", new string[2] { item.Name, item.Id.ToString() }));
                        else if (item.IsSuppressed)
                            CurrentItem.ListQualityCheckResult.Add(CreateResultItem(CurrentItem, CadDocCheckStatus.WARNING, "CQC_QualCheckMsg005", new string[2] { item.Name, item.Id.ToString() }));
                        else if (!item.IsFullyConstrain)
                            CurrentItem.ListQualityCheckResult.Add(CreateResultItem(CurrentItem, CadDocCheckStatus.ISSUE, "CQC_QualCheckMsg006", new string[2] { item.Name, item.Id.ToString() }));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private CadDocQualityCheckResultItem CreateResultItem(CadDocQualityCheckItem CurrentItem, CadDocCheckStatus Status, string KeyStringResource, string[] ParamString = null)
        {
            try
            {
                CadDocQualityCheckResultItem CurrentResultItem = new CadDocQualityCheckResultItem()
                {
                    Status = Status,
                    ParentQualityCheckItem = CurrentItem,
                    KeyString = KeyStringResource,
                    ParamString = ParamString
                };

                if (ParamString == null || ParamString.Count() == 0)
                    CurrentResultItem.Comments = McgWpfTools.GetStringResource(KeyStringResource);
                else
                    CurrentResultItem.Comments = string.Format(McgWpfTools.GetStringResource(KeyStringResource), ParamString);

                return CurrentResultItem;
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Update CAD Document
        private void UpdateAllCadDocumentsRelationsAsynch()
        {
            CadDocQualityCheckItem CurrentItem = null;
            try
            {
                bool IsSaveOk = false;
                CurrentDataContext.IsSearchCadModelInProgress = true;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = McgWpfTools.GetStringResource("CQC_UpdateCadDocRelInProgress");
                foreach (var caditem in CurrentDataContext.ShownCadModels.Where(item => item.RelationsStatus != CadDocCheckStatus.OK && item.IsSelected))
                {
                    CurrentItem = caditem;
                    caditem.Comment = "Start Relations Update";
                    if (caditem.IsCheckedOut || CurrentDataContext.CheckUncheckedOutItem)
                        IsSaveOk = UpdateOneCadDocumentRelations(caditem);
                    CurrentDataContext.NbModelsInSessionInProgress++;

                    if (IsSaveOk)
                        caditem.Comment = "Relations Update Done";
                    else
                        caditem.Comment = "Relations Update Done - SaveIssue";
                }
            }
            catch (Exception ex)
            {
                StopCurrentProcess = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
                if (CurrentItem != null)
                    CurrentItem.Comment = "Relations Update Issue";
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.IsNoActionInProgress = true;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = "";
            }
        }

        private bool UpdateOneCadDocumentRelations(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.CurrentCadModel != null)
                {
                    CurrentItem.IsPreRegenRelationsOk = _creoParameterService.SetPreRegenRelations(CurrentItem.CurrentCadModel, CurrentItem.NewPreRegenRelations);
                    CurrentItem.IsPostRegenRelationsOk = _creoParameterService.SetPostRegenRelations(CurrentItem.CurrentCadModel, CurrentItem.NewPostRegenRelations);
                    return SaveCadItem(CurrentItem);
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateAllCadDocumentsAttributesAsynch()
        {
            CadDocQualityCheckItem CurrentItem = null;
            try
            {
                bool IsSaveOk = false;
                CurrentDataContext.IsSearchCadModelInProgress = true;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = McgWpfTools.GetStringResource("CQC_UpdateCadDocAttribInProgress");
                foreach (var caditem in CurrentDataContext.ShownCadModels.Where(item => item.AttributesStatus != CadDocCheckStatus.OK && item.IsSelected))
                {
                    CurrentItem = caditem;
                    caditem.Comment = "Start Attributes Update";
                    if (caditem.IsCheckedOut || CurrentDataContext.CheckUncheckedOutItem)
                        IsSaveOk = UdpateOneCadDocumentAttribute(caditem);
                    CurrentDataContext.NbModelsInSessionInProgress++;
                    if (IsSaveOk)
                        caditem.Comment = "Attributes Update Done";
                    else
                        caditem.Comment = "Attributes Update Done - save Issue";
                }

                if (CurrentDataContext.ForceTypeProeUpdate)
                {
                    CurrentDataContext.NbModelsInSessionInProgress = 0;
                    CurrentDataContext.TextStatusBar = McgWpfTools.GetStringResource("CQC_UpdateCadDocAttribInProgress");

                    foreach (var caditem in CurrentDataContext.ShownCadModels.Where(item => item.IsSelected))
                    {
                        CurrentItem = caditem;
                        caditem.Comment = "Start Attribute TypeProe Update";
                        if (caditem.IsCheckedOut || CurrentDataContext.CheckUncheckedOutItem)
                            IsSaveOk = UdpateOneCadDocumentAttributeTypeProe(caditem);
                        CurrentDataContext.NbModelsInSessionInProgress++;
                        if (IsSaveOk)
                            caditem.Comment = "Attribute TypeProe Update Done";
                        else
                            caditem.Comment = "Attribute TypeProe Update Done - save Issue";
                    }
                }
            }
            catch (Exception ex)
            {
                StopCurrentProcess = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
                if (CurrentItem != null)
                    CurrentItem.Comment = "Attributes Update Issue";
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.IsNoActionInProgress = true;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = "";
            }
        }

        private bool UdpateOneCadDocumentAttribute(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.CurrentCadModel != null)
                {
                    foreach (var param in CurrentItem.ListAttributes.Where(item => item.IsMissing))
                    {
                        switch (param.Type)
                        {
                            case "STRING":
                                if (param.Name == "QUALINSPGRP")
                                    _creoParameterService.SetParameter(CurrentItem.CurrentCadModel, param.Name, "X", param.IsDesignated);
                                else if (param.Name == "ADDITIONALPUBFORMAT")
                                    _creoParameterService.SetParameter(CurrentItem.CurrentCadModel, param.Name, "Nothing", param.IsDesignated);
                                else
                                    _creoParameterService.SetParameter(CurrentItem.CurrentCadModel, param.Name, "", param.IsDesignated);
                                break;
                            case "REAL":
                                _creoParameterService.SetParameter(CurrentItem.CurrentCadModel, param.Name, 0.0, param.IsDesignated);
                                break;
                            case "INT":
                                _creoParameterService.SetParameter(CurrentItem.CurrentCadModel, param.Name, 0, param.IsDesignated);
                                break;
                            case "BOOL":
                                _creoParameterService.SetParameter(CurrentItem.CurrentCadModel, param.Name, false, param.IsDesignated);
                                break;
                            default:
                                break;
                        }
                    }
                    foreach (var param in CurrentItem.ListAttributes.Where(item => !item.IsDesignatedOk))
                    {
                        if (((IpfcBaseParameter)param.Attribute).IsDesignated != param.IsDesignated)
                            ((IpfcBaseParameter)param.Attribute).IsDesignated = param.IsDesignated;
                    }

                    // Update Param TYPEPROE if exist
                    var ParamTypeProe = CurrentItem.ListAttributes.FirstOrDefault(item => item.Name == "TYPEPROE");
                    var ParamType = CurrentItem.ListAttributes.FirstOrDefault(item => item.Name == "TYPE");
                    if (ParamTypeProe != null && ParamType != null)
                    {
                        string ParamTypeProeVal = _creoParameterService.GetParameterAsString(ParamTypeProe.Attribute);
                        string ParamTypeVal = _creoParameterService.GetParameterAsString(ParamType.Attribute);
                        if (ParamTypeProeVal != null && ParamTypeVal != null && ParamTypeProeVal.Trim() == "" && ParamTypeVal.Trim() != "")
                        {
                            _creoParameterService.SetParameter(ParamTypeProe.Attribute, ParamTypeVal.Split('#').FirstOrDefault());
                        }
                    }

                    return SaveCadItem(CurrentItem);
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private bool UdpateOneCadDocumentAttributeTypeProe(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                // Update Param TYPEPROE if exist
                if (CurrentItem.CurrentCadModel != null)
                {
                    var ParamTypeProe = CurrentItem.ListAttributes.FirstOrDefault(item => item.Name == "TYPEPROE");
                    var ParamType = CurrentItem.ListAttributes.FirstOrDefault(item => item.Name == "TYPE");
                    if (ParamTypeProe != null && ParamType != null)
                    {
                        string ParamTypeProeVal = _creoParameterService.GetParameterAsString(ParamTypeProe.Attribute);
                        string ParamTypeVal = _creoParameterService.GetParameterAsString(ParamType.Attribute);
                        if (ParamTypeProeVal != null && ParamTypeVal != null && ParamTypeProeVal.Trim() == "" && ParamTypeVal.Trim() != "")
                        {
                            _creoParameterService.SetParameter(ParamTypeProe.Attribute, ParamTypeVal.Split('#').FirstOrDefault());
                        }
                    }

                    return SaveCadItem(CurrentItem);
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateAllCadDocumentsLayersAsynch()
        {
            CadDocQualityCheckItem CurrentItem = null;
            try
            {
                bool IsSaveOk = false;
                CurrentDataContext.IsSearchCadModelInProgress = true;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = McgWpfTools.GetStringResource("CQC_UpdateCadDocLayerInProgress");
                foreach (var caditem in CurrentDataContext.ShownCadModels.Where(item => item.LayersStatus != CadDocCheckStatus.OK && item.IsSelected))
                {
                    CurrentItem = caditem;
                    caditem.Comment = "Start Layers Update";
                    if (caditem.IsCheckedOut || CurrentDataContext.CheckUncheckedOutItem)
                        IsSaveOk = UpdateCadDocLayers(caditem);
                    CurrentDataContext.NbModelsInSessionInProgress++;
                    if (IsSaveOk)
                        caditem.Comment = "Layers Update Done";
                    else
                        caditem.Comment = "Layers Update Done - Save Issue";
                }
            }
            catch (Exception ex)
            {
                StopCurrentProcess = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
                if (CurrentItem != null)
                    CurrentItem.Comment = "Layers Update Issue";
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.IsNoActionInProgress = true;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = "";
            }
        }

        private bool UpdateCadDocLayers(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.CurrentCadModel != null)
                {
                    bool IsDisplayedWindow = false;
                    if (CurrentItem.LayersStatus != CadDocCheckStatus.OK)
                    {
                        IsDisplayedWindow = _creoModelService.ActiveCadDocWindow(CurrentItem.CurrentCadModel);

                        foreach (var item in CurrentItem.ListLayers.Where((layer) => layer.State == ObjectState.TO_BE_REMOVED))
                        {
                            try
                            {
                                item.LayerItem.Delete();
                                item.State = ObjectState.REMOVED;
                            }
                            catch (Exception)
                            {

                            }
                        }

                        IpfcLayer CurrentLayer;
                        List<CREOCadModelItem> CurrentListItems;
                        foreach (var item in CurrentItem.ListLayers.Where((layer) => layer.State == ObjectState.NEW))
                        {
                            CurrentLayer = _creoLayerService.CreateCadModelLayer(CurrentItem.CurrentCadModel, item.Name);
                            if (CurrentLayer != null)
                            {
                                CurrentListItems = new List<CREOCadModelItem>();
                                foreach (var cadItem in item.ListModelItems)
                                {
                                    CurrentListItems.Add(new CREOCadModelItem() { Item = cadItem });
                                }
                                _creoLayerService.AddItemsToLayer(CurrentLayer, CurrentListItems?.Select(item => item.Item));

                                CurrentLayer.Status = (int)item.DisplayStatus;
                            }
                        }

                        foreach (var item in CurrentItem.ListLayers.Where((layer) => layer.State == ObjectState.CREATED))
                        {
                            item.LayerItem.Status = (int)item.DisplayStatus;
                        }

                        _creoMacroService.SaveLayerStatus();

                        if (!IsDisplayedWindow)
                        {
                            IpfcWindow CurrentWindow = _creoModelService.GetCadDocWindow(CurrentItem.CurrentCadModel);
                            CurrentWindow.Close();
                        }
                    }

                    return SaveCadItem(CurrentItem);
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateAllCadDocumentsUnitsAsynch()
        {
            CadDocQualityCheckItem CurrentItem = null;
            try
            {
                bool IsSaveOk = false;
                CurrentDataContext.IsSearchCadModelInProgress = true;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = McgWpfTools.GetStringResource("CQC_UpdateCadDocUnitInProgress");
                foreach (var caditem in CurrentDataContext.ShownCadModels.Where(item => !item.IsUnitsOk && item.IsSelected))
                {
                    CurrentItem = caditem;
                    caditem.Comment = "Start Units Update";
                    if (caditem.IsCheckedOut || CurrentDataContext.CheckUncheckedOutItem)
                        IsSaveOk = UpdateOneCadDocUnits(caditem);
                    CurrentDataContext.NbModelsInSessionInProgress++;
                    if (IsSaveOk)
                        caditem.Comment = "Units Update Done";
                    else
                        caditem.Comment = "Units Update Done - Save Issue";

                }
            }
            catch (Exception ex)
            {
                StopCurrentProcess = false;
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
                if (CurrentItem != null)
                    CurrentItem.Comment = "Units Update Issue";
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.IsNoActionInProgress = true;
                CurrentDataContext.IsSearchCadModelInProgress = false;
                CurrentDataContext.NbModelsInSessionInProgress = 0;
                CurrentDataContext.TextStatusBar = "";
            }
        }

        private bool UpdateOneCadDocUnits(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.CurrentCadModel != null)
                {
                    if (!CurrentItem.IsUnitsOk && CurrentItem.CadDocType != EpmDocumentTypeEnum.DRW)
                    {
                        IpfcSolid CurrentSolid = (IpfcSolid)CurrentItem.CurrentCadModel;
                        IpfcUnitSystem DefaultUnit = null;
                        foreach (IpfcUnitSystem unit in CurrentSolid.ListUnitSystems())
                        {
                            if (unit.Name.Contains(CadDocQualityCheckConstants.DefaultUnits))
                                DefaultUnit = unit;
                        }
                        if (DefaultUnit != null && CurrentSolid.GetPrincipalUnits().Name != DefaultUnit.Name)
                        {
                            CCpfcUnitConversionOptions CreateConversionUnit = new CCpfcUnitConversionOptions();
                            CurrentSolid.SetPrincipalUnits(DefaultUnit, CreateConversionUnit.Create((int)EpfcUnitDimensionConversion.EpfcUNITCONVERT_SAME_SIZE));
                        }
                    }

                    return SaveCadItem(CurrentItem);
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private bool SaveCadItem(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.CadDocType == EpmDocumentTypeEnum.DRW)
                {
                    //((IpfcModel2D)CurrentItem.CurrentCadModel).Regenerate();
                }
                else
                {
                    CCpfcRegenInstructions CreateInstrustions = new CCpfcRegenInstructions();
                    IpfcRegenInstructions Instruction = CreateInstrustions.Create(false, true, null);
                    Instruction.UpdateInstances = true;

                    ((IpfcSolid)CurrentItem.CurrentCadModel).Regenerate(Instruction);
                }

                CurrentItem.CurrentCadModel.Save();
                return true;
            }
            catch (Exception)
            {
                return false;
                // throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateAllComponentAssembly(IpfcSolid assembly)
        {
            try
            {
                // Initialisation de la connexion asynchrone
                //IpfcAsyncConnection asyncConnection;
                //CCpfcAsyncConnection cAC = new CCpfcAsyncConnection();
                //asyncConnection = cAC.Start("pro -g:no_graphics -i:rpc_input", ".");

                // Récupération de la session
                // IpfcBaseSession session = CurrentCREOConnection.Session;

                // Parcours des composants de l'assemblage
                IpfcFeatures features = assembly.ListFeaturesByType(false, EpfcFeatureType.EpfcFEATTYPE_COMPONENT);
                foreach (IpfcFeature feature in features)
                {
                    IpfcComponentFeat componentFeat = (IpfcComponentFeat)feature;
                    componentFeat.RedefineThroughUI();
                    componentFeat.Regenerate();
                    //IpfcComponentPath compPath = componentFeat.GetPath();

                    //// Ouvrir la fenêtre de placement du composant
                    //IpfcComponentPlacementWindow placementWindow;
                    //placementWindow = assembly.OpenComponentPlacementWindow(compPath);

                    //// Valider le placement du composant
                    //placementWindow.ValidatePlacement();
                }
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        private void CheckWindchillCredential()
        {
            try
            {
                if (WindchillCredential == null || !WindchillCredential.IsCredentialOk)
                {
                    WindchillCredential = _windchillCredentialService.GetWindchillCredential(CommonLibConstants.WindchillUrl, CommonLibConstants.WindchillUrl);
                }
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
