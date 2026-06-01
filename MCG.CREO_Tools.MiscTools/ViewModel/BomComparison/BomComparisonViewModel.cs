using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.SapTools.Exceptions;
using MCG.CommonLib.SapTools.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.WindchillCredential;
using MCG.CREO_Tools.MiscTools.View.BomComparison;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.WindchillRequestTool;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.MiscTools.ViewModel.BomComparison
{
    public class BomComparisonViewModel : ObservableObject, IBomComparisonViewModel
    {
        #region [REGION] Properties from Interface
        public BomComparisonDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; } = "";
        private Dispatcher MainDispatcher { get; set; } = null;
        private WindchillObjectType CurrentWindchillType { get; set; } = WindchillObjectType.UNKNOWN;
        private WindchillCredentialItem WindchillNetworkCredential { get; set; }
        private Thread ThreadSearchBom { get; set; }

        private string Number { get; set; } = null;
        private string Revision { get; set; } = null;
        private string StatusBarMsg { get; set; } = "";
        private bool IsLatestRevision { get; set; } = false;
        private ObservableCollection<WindchillObjStructureComponent> MainBom { get; set; } = null;
        private BomItem MainBomItem { get; set; } = null;
        private string BomSide { get; set; } = "";
        private WindchillObjStructureComponent RawBom { get; set; }
        private WindchillObject UpperWindchillObject { get; set; }
        private List<WindchillObjStructureComponent> AllComponent { get; set; } = new List<WindchillObjStructureComponent>();
        private List<WindchillObjStructureComponent> CompleteBom { get; set; } = new List<WindchillObjStructureComponent>();
        private string BomFrom { get; set; } = "";
        private string SapPlant { get; set; } = "";
        private DateTime? ValidityDate { get; set; }
        private string DescriptionBomL { get; set; } = "";
        private string DescriptionBomR { get; set; } = "";
        #endregion

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

        #region [REGION] Commands
        public ICommand CommandStartBomSearch { get => new RelayCommand<string>((obj) => ExecuteStartBomSearch(obj)); }
        public ICommand CommandStartExportXLS { get => new RelayCommand<string>((obj) => ExecuteStartExportXLS(obj)); }
        public ICommand CommandHelp { get => new RelayCommand(() => ExecuteHelp()); }
        #endregion

        #region [REGION] Init
        private readonly IWindchillEpmDocumentManagementService _windchillEpmDocumentManagementService;
        private readonly IWindchillPartManagementService _windchillPartManagementService;
        private readonly IWindchillBomManagementService _windchillBomManagementService;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly IWindchillRequestMiscService _windchillRequestMiscService;
        private readonly ISapBomService _sapBomService;
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        private readonly IBomComparisonToolService _bomComparisonToolService;

        public BomComparisonViewModel(IWindchillEpmDocumentManagementService windchillEpmDocumentManagementService,
                                      IWindchillPartManagementService windchillPartManagementService,
                                      IWindchillBomManagementService windchillBomManagementService,
                                      IWindchillCredentialService windchillCredentialService,
                                      IWindchillRequestMiscService IWindchillRequestMiscService,
                                      ISapBomService sapBomService,
                                      IMcgCommonLibWindowService mcgCommonLibWindowService,
                                      IBomComparisonToolService bomComparisonToolService)
        {
            try
            {
                _windchillEpmDocumentManagementService = windchillEpmDocumentManagementService;
                _windchillPartManagementService = windchillPartManagementService;
                _windchillBomManagementService = windchillBomManagementService;
                _windchillCredentialService = windchillCredentialService;
                _windchillRequestMiscService = IWindchillRequestMiscService;
                _sapBomService = sapBomService;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;
                _bomComparisonToolService = bomComparisonToolService;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                MainDispatcher = Dispatcher.CurrentDispatcher;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);

                CurrentDataContext = new BomComparisonDataContext();
                CurrentDataContext.BomComparison = new BomComparisonItem();
                CurrentDataContext.BomComparison.SourceBom1 = McgWpfTools.GetStringResource("BCE_BomLeft");
                CurrentDataContext.BomComparison.SourceBom2 = McgWpfTools.GetStringResource("BCE_BomRight");


                List<string> AllValues = MiscToolsConstants.BomFromValues.Split('|').ToList();
                foreach (string value in AllValues)
                    CurrentDataContext.ListBomFrom.Add(value);
                CurrentDataContext.SelectedBomFromL = CurrentDataContext.ListBomFrom.FirstOrDefault();
                CurrentDataContext.SelectedBomFromR = CurrentDataContext.ListBomFrom.FirstOrDefault();
                AllValues = MiscToolsConstants.SapPlantValues.Split('|').ToList();
                foreach (string value in AllValues)
                    CurrentDataContext.ListSapPlant.Add(value);
                CurrentDataContext.SelectedSapPlantL = CurrentDataContext.ListSapPlant.FirstOrDefault();
                CurrentDataContext.SelectedSapPlantR = CurrentDataContext.ListSapPlant.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteStartBomSearch(string pBomSide)
        {
            try
            {
                BomSide = pBomSide;
                if (CurrentDataContext.NumberL == null) CurrentDataContext.NumberL = "";
                if (CurrentDataContext.NumberR == null) CurrentDataContext.NumberR = "";
                if (CurrentDataContext.RevisionL == null) CurrentDataContext.RevisionL = "";
                if (CurrentDataContext.RevisionR == null) CurrentDataContext.RevisionR = "";
                if (BomSide == "L")
                {
                    CurrentDataContext.IsSearchBomDoneL = false;
                    Number = CurrentDataContext.NumberL.ToUpper();
                    Revision = CurrentDataContext.RevisionL.ToUpper();
                    StatusBarMsg = CurrentDataContext.StatusBarMsgL;
                    IsLatestRevision = CurrentDataContext.IsLatestRevisionL;
                    MainBomItem = CurrentDataContext.BomL;
                    BomFrom = CurrentDataContext.SelectedBomFromL;
                    SapPlant = CurrentDataContext.SelectedSapPlantL;
                    ValidityDate = CurrentDataContext.ValidityDateL;

                }
                else if (BomSide == "R")
                {
                    CurrentDataContext.IsSearchBomDoneR = false;
                    Number = CurrentDataContext.NumberR.ToUpper();
                    Revision = CurrentDataContext.RevisionR.ToUpper();
                    StatusBarMsg = CurrentDataContext.StatusBarMsgR;
                    IsLatestRevision = CurrentDataContext.IsLatestRevisionR;
                    MainBomItem = CurrentDataContext.BomR;
                    BomFrom = CurrentDataContext.SelectedBomFromR;
                    SapPlant = CurrentDataContext.SelectedSapPlantR;
                    ValidityDate = CurrentDataContext.ValidityDateR;
                }


                StatusBarMsg = "";
                if (Number == null || Number == "" || Number.Contains("*"))
                    MessageBox.Show(McgWpfTools.GetStringResource("BCE_EnterNumber"), "BOM Search Issue", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                else
                {
                    CurrentDataContext.IsActionProgress = true;
                    if (CurrentDataContext.IsAssemblyChecked)
                    {
                        CurrentWindchillType = WindchillObjectType.ASM;
                        if (!Number.Contains(".")) Number = $"{Number}.ASM";
                    }
                    else if (CurrentDataContext.IsPartChecked)
                    {
                        CurrentWindchillType = WindchillObjectType.PART;
                        if (Number.Contains(".")) Number = Number.Split('.').FirstOrDefault();
                    }

                    WindchillNetworkCredential = _windchillCredentialService.GetWindchillCredential(CommonLibConstants.WindchillUrl, CommonLibConstants.WindchillUrl);
                    if (IsLatestRevision) Revision = "LATEST";
                    if (Revision == null || Revision == "") Revision = "LATEST";

                    RaiseActionInProgressEvent();

                    if (BomFrom == "PDM")
                    {
                        ThreadSearchBom = new Thread(() => SearchBomPdmAsynch());
                        ThreadSearchBom.Start();
                    }
                    else if (BomFrom == "SAP")
                    {
                        ThreadSearchBom = new Thread(() => SearchBomSapAsynch());
                        ThreadSearchBom.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                CurrentDataContext.IsActionProgress = false;
                if (BomSide == "L")
                {
                    CurrentDataContext.StatusBarMsgL = McgWpfTools.GetStringResource("BCE_StBarMsg11");
                    CurrentDataContext.IsSearchBomDoneL = true;
                }
                else if (BomSide == "R")
                {
                    CurrentDataContext.IsSearchBomDoneR = true;
                    CurrentDataContext.StatusBarMsgR = McgWpfTools.GetStringResource("BCE_StBarMsg11");
                }
                RaiseActionDoneEvent();
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartExportXLS(string pBomSide)
        {
            try
            {
                Regex RegexProc = new Regex("Excel", RegexOptions.IgnoreCase);
                List<Process> OldExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));

                string UserDocumentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string XlsFileName = $"{UserDocumentFolder}\\BOM_COMP_{CurrentDataContext.NumberL}_{CurrentDataContext.RevisionL}_{CurrentDataContext.NumberR}_{CurrentDataContext.RevisionR}.xlsx";

                ExcelToolsClosedXml CurrentExcel = new ExcelToolsClosedXml() { CompleteFileName = XlsFileName, CompleteTemplateFileName = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.ExcelTemplateBomComparison}" };
                if (CurrentExcel.OpenFile($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.ExcelTemplateBomComparison}") != ExcelStatus.OK)
                {
                    MessageBox.Show(String.Format(McgWpfTools.GetStringResource("BCE_ExportXlsIssue"), XlsFileName), "Excel Export Issue", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }

                List<Process> NewExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                Process newExcelProcess = NewExcelCurrentProcess.FirstOrDefault((proc) => !OldExcelCurrentProcess.ToList().Exists((oldprc) => proc.Id == oldprc.Id));


                CurrentExcel.CurrentSheet = "BOM_COMPARISON";

                CurrentDataContext.BomComparison.ExportExcelGenericBomComparison(CurrentExcel);

                if (CurrentExcel.SaveClose() != ExcelStatus.OK)
                {
                    MessageBox.Show(String.Format(McgWpfTools.GetStringResource("BCE_ExportXlsIssue"), XlsFileName), "Excel Export Issue", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }

                if (newExcelProcess != null)
                    newExcelProcess.Kill();

                if (File.Exists(XlsFileName))
                    _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("BCE_BtExportResult"), String.Format(McgWpfTools.GetStringResource("BCE_ExportXls"), XlsFileName), McgWpfTools.GetStringResource("BCE_ToolTipOpen"), McgWpfTools.GetStringResource("BCE_ToolTipOpenFolder"), McgWpfTools.GetStringResource("BCE_ToolTipClose"), XlsFileName);
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteHelp()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("BCE_LinkHelpBomComparison"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        #endregion

        #region [REGION] Misc
        private void SearchBomPdmAsynch()
        {
            try
            {
                WindchillObject CurrentWindchillObject = null;
                if (CurrentDataContext.IsAssemblyChecked)
                {
                    StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg01");
                    RestOdataEpmDocument CurrentRestOdataEpmDocument;
                    if (IsLatestRevision || Revision == "LATEST")
                        CurrentRestOdataEpmDocument = _windchillEpmDocumentManagementService.GetOneEpmDocument(WindchillNetworkCredential.WindchillCredential, Number);
                    else
                        CurrentRestOdataEpmDocument = _windchillEpmDocumentManagementService.GetOneEpmDocument(WindchillNetworkCredential.WindchillCredential, Number, Revision);
                    if (CurrentRestOdataEpmDocument != null)
                        CurrentWindchillObject = _windchillRequestMiscService.GetWindchillEpmDocument(CurrentRestOdataEpmDocument);
                }

                else if (CurrentDataContext.IsPartChecked)
                {
                    StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg02");
                    RestOdataWtPart CurrentRestOdataWtPart;
                    if (IsLatestRevision || Revision == "LATEST")
                        CurrentRestOdataWtPart = _windchillPartManagementService.GetOnePart(WindchillNetworkCredential.WindchillCredential, Number);
                    else
                        CurrentRestOdataWtPart = _windchillPartManagementService.GetOnePart(WindchillNetworkCredential.WindchillCredential, Number, Revision);

                    if (CurrentRestOdataWtPart != null)
                        CurrentWindchillObject = _windchillRequestMiscService.GetWindchillPart(CurrentRestOdataWtPart);
                }


                if (CurrentWindchillObject == null)
                {
                    CurrentDataContext.IsActionProgress = false;
                    MessageBox.Show(String.Format(McgWpfTools.GetStringResource("BCE_ObjectNotFound"), CurrentWindchillType, Number, Revision), "BOM Search Issue", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                }
                else
                {
                    if (Revision == "LATEST")
                    {
                        Revision = CurrentWindchillObject.Revision;
                        if (BomSide == "L")
                        {
                            CurrentDataContext.RevisionL = CurrentWindchillObject.Revision;
                            DescriptionBomL = $"PDM: {CurrentDataContext.NumberL} - Revision: {CurrentDataContext.RevisionL}";
                        }
                        else if (BomSide == "R")
                        {
                            CurrentDataContext.RevisionR = CurrentWindchillObject.Revision;
                            DescriptionBomR = $"PDM: {CurrentDataContext.NumberR} - Revision: {CurrentDataContext.RevisionR}";
                        }
                    }

                    WindchillObjectStructure CurrentBom = null;
                    WindchillObjectType TypeBom = WindchillObjectType.PART;
                    if (CurrentDataContext.IsAssemblyChecked) TypeBom = WindchillObjectType.ASM;
                    else if (CurrentDataContext.IsPartChecked) TypeBom = WindchillObjectType.PART;

                    CurrentBom = _windchillBomManagementService.GetBomFirstLevelOneOccurence(Number, Revision, TypeBom, WindchillNetworkCredential.WindchillCredential);
                    MainBomItem = _windchillRequestMiscService.GetBomItemNotNamingConvention(CurrentBom, CurrentDataContext.NumericalLineNumberDigit);
                    
                    foreach (var item in MainBomItem.BomComponentIssue)
                        MainBomItem.Bom.Add(item);

                    if (BomSide == "L")
                        CurrentDataContext.BomL = MainBomItem;
                    else if (BomSide == "R")
                        CurrentDataContext.BomR = MainBomItem;

                    CurrentDataContext.BomComparison = _bomComparisonToolService.GetBomComparison(CurrentDataContext.BomL, CurrentDataContext.BomR);
                    CurrentDataContext.BomComparison.SourceBom1 = McgWpfTools.GetStringResource("BCE_BomLeft");
                    CurrentDataContext.BomComparison.SourceBom2 = McgWpfTools.GetStringResource("BCE_BomRight");

                    CurrentDataContext.BomComparison.DescriptionBom1 = DescriptionBomL;
                    CurrentDataContext.BomComparison.DescriptionBom2 = DescriptionBomR;


                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                if (BomSide == "L")
                {
                    CurrentDataContext.StatusBarMsgL = McgWpfTools.GetStringResource("BCE_StBarMsg03");
                    CurrentDataContext.IsSearchBomDoneL = true;
                }
                else if (BomSide == "R")
                {
                    CurrentDataContext.IsSearchBomDoneR = true;
                    CurrentDataContext.StatusBarMsgR = McgWpfTools.GetStringResource("BCE_StBarMsg03");
                }
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsActionProgress = false;
                RaiseActionDoneEvent();
            }
        }

        private void SearchBomSapAsynch()
        {
            try
            {
                if (BomSide == "L")
                {
                    CurrentDataContext.RevisionL = "";
                }
                else if (BomSide == "R")
                {
                    CurrentDataContext.RevisionR = "";
                }

                MainBomItem = new BomItem();

                try
                {
                    var ListComp = _sapBomService.ExtractOneMaterialMasterSapBom(Number, ValidityDate.Value.ToString("yyyyMMdd"), SapPlant.Replace("Without", " "));
                    if (ListComp != null)
                    {
                        foreach (var Comp in ListComp.Where(item => item.Level == 1))
                        {
                            Comp.State = "";
                            MainBomItem.Bom.Add(Comp);
                        }
                    }
                    else
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("BCE_InfoMsgErpCom"), BomFrom), McgWpfTools.GetStringResource("BCE_InfoTitleErpBom"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (SapToolsNoConnectionException)
                {
                    MessageBox.Show(String.Format(McgWpfTools.GetStringResource("BCE_InfoMsgErpConNotFound"), BomFrom), McgWpfTools.GetStringResource("BCE_InfoTitleErpBom"), MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (BomSide == "L")
                {
                    CurrentDataContext.BomL = MainBomItem;
                    DescriptionBomL = $"SAP: {CurrentDataContext.NumberL} - Validity Date: {ValidityDate.Value.ToShortDateString()}";
                }
                else if (BomSide == "R")
                {
                    CurrentDataContext.BomR = MainBomItem;
                    DescriptionBomR = $"SAP: {CurrentDataContext.NumberR} - Validity Date: {ValidityDate.Value.ToShortDateString()}";
                }

                CurrentDataContext.BomComparison = _bomComparisonToolService.GetBomComparison(CurrentDataContext.BomL, CurrentDataContext.BomR);
                CurrentDataContext.BomComparison.SourceBom1 = McgWpfTools.GetStringResource("BCE_BomLeft");
                CurrentDataContext.BomComparison.SourceBom2 = McgWpfTools.GetStringResource("BCE_BomRight");

                CurrentDataContext.BomComparison.DescriptionBom1 = DescriptionBomL;
                CurrentDataContext.BomComparison.DescriptionBom2 = DescriptionBomR;
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                if (BomSide == "L")
                {
                    CurrentDataContext.StatusBarMsgL = McgWpfTools.GetStringResource("BCE_StBarMsg03");
                    CurrentDataContext.IsSearchBomDoneL = true;
                }
                else if (BomSide == "R")
                {
                    CurrentDataContext.IsSearchBomDoneR = true;
                    CurrentDataContext.StatusBarMsgR = McgWpfTools.GetStringResource("BCE_StBarMsg03");
                }
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsActionProgress = false;
                RaiseActionDoneEvent();
            }
        }

        #endregion
    }
}
