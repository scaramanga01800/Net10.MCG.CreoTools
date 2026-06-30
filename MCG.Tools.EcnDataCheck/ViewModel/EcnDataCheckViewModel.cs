using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Models.SAP;
using MCG.CommonLib.SapTools.Interfaces;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WebtermLib.Models;
using MCG.CommonLib.WebtermLib.Services;
using MCG.CommonLib.WebtermLib.Services.Interfaces;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.Services;
using MCG.CommonLib.WpfComponent.WindchillCredential;
using MCG.CREO_Tools.MiscTools.Interfaces;
using MCG.Tools.EcnDataCheck.Configuration;
using MCG.Tools.EcnDataCheck.Exceptions;
using MCG.Tools.EcnDataCheck.Interfaces;
using MCG.Tools.EcnDataCheck.Models;
using MCG.Tools.EcnDataCheck.View;
using MCG.WindchillRequestTool;
using MCG.WindchillRequestTool.Model.BomComparison;
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

namespace MCG.Tools.EcnDataCheck.ViewModel
{
    public class EcnDataCheckViewModel : ObservableObject, IEcnDataCheckViewModel
    {
        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private WindchillCredentialItem WindchillNetworkCredential { get; set; } = null;
        private Dispatcher MainDispatcher { get; set; } = null;
        private int MultiplierStep { get; set; } = 2;
        private EcnDataCheckConfiguration CurrentEcnDataCheckConfiguration { get; set; }
        private bool IsWebtermListSearched { get; set; } = false;
        private WindchillNamingConvention NamingConvention { get; set; } = null;
        private List<WindchillObjectWtPart> ListPartComponentNotApproved { get; set; } = new List<WindchillObjectWtPart>();
        private string CurrentEcnNumber { get; set; } = "";
        private int IndexBomCompCheck { get; set; } = 0;
        private int IndexMaxBomCompCheck { get; set; } = 0;
        private McgQuickChangeTools CurrentMcgQuickChangeTool { get; set; } = null;
        private List<BrandGroupSubGroupItem> ListBrandGroupSubGroup { get; set; }
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

        #region [REGION] Properties
        private IEcnDataCheckDataContext _CurrentEcnDataCheckDataContext;
        public IEcnDataCheckDataContext CurrentEcnDataCheckDataContext
        {
            get { return this._CurrentEcnDataCheckDataContext; }
            set
            {
                if (this._CurrentEcnDataCheckDataContext != value)
                {
                    this._CurrentEcnDataCheckDataContext = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region [REGION] Commands
        public ICommand CommandStartEcnDataCheck { get => new RelayCommand(() => ExecuteStartEcnDataCheck()); }
        public ICommand CommandBtHelpMouseLeftButtonUpEvent { get => new RelayCommand(() => ExecuteBtHelpMouseLeftButtonUpEvent()); }
        public ICommand CommandStartSapBOMComparison { get => new RelayCommand(() => ExecuteStartSapBOMComparison()); }
        public ICommand CommandStartExportXLS { get => new RelayCommand(() => ExecuteStartExportXLS()); }
        public ICommand CommandRestartCheck { get => new RelayCommand<string>((str) => ExecuteRestartCheck(str)); }
        public ICommand CommandOpenEcn { get => new RelayCommand<string>((str) => ExecuteOpenEcn(str)); }
        public ICommand CommandOpenEca { get => new RelayCommand<string>((str) => ExecuteOpenEca(str)); }
        public ICommand CommandOpenPart { get => new RelayCommand<string>((str) => ExecuteOpenPart(str)); }
        public ICommand CommandOpenBomPdmComp { get => new RelayCommand<string>((str) => ExecuteOpenBomPdmComp(str)); }
        public ICommand CommandOpenBomSapComp { get => new RelayCommand<string>((str) => ExecuteOpenBomSapComp(str)); }
        public ICommand CommandMoveSendMail { get => new RelayCommand(() => ExecuteMoveSendMail()); }
        public ICommand CommandMoveUdpateContextList { get => new RelayCommand(() => ExecuteMoveUdpateContextList()); }
        public ICommand CommandSetContextToAll { get => new RelayCommand(() => ExecuteSetContextToAll()); }
        public ICommand CommandRenameSendMail { get => new RelayCommand(() => ExecuteRenameSendMail()); }
        public ICommand CommandCopyPartNumber { get => new RelayCommand<string>((txt) => ExecuteCopyPartNumber(txt)); }
        public ICommand CommandStartSapCraneSearch { get => new RelayCommand(() => ExecuteStartSapCraneSearch()); }
        public ICommand CommandOpenLink { get => new RelayCommand<string>((str) => ExecuteOpenLink(str)); }
        #endregion

        #region [REGION] Init
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly IRegExTools _regExTools;
        private readonly IWebtermTools _webtermTools;
        private readonly IWindchillReportingManagementService _windchillReportingManagementService;
        private readonly IWindchillNavigationService _windchillNavigationService;
        private readonly IWindchillChangeManagementService _windchillChangeManagementService;
        private readonly IWindchillPartManagementService _windchillPartManagementService;
        private readonly IWindchillRequestTool _windchillRequestTool;
        private readonly IWindchillBomManagementService _windchillBomManagementService;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly IWindchillCheckNumberService _windchillCheckNumberService;
        private readonly ISapSessionManager _ISapSessionManager;
        private readonly ISapBomService _sapBomService;
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        private readonly IBomComparisonToolService _bomComparisonToolService;
        private readonly IWindchillRequestMiscService _windchillRequestMiscService;
        private readonly ISapHupService _sapHupService;
        private readonly IEcnDataCheckWindchillService _ecnDataCheckWindchillService;
        private readonly IMiscToolsWindchillService _miscToolsWindchillService;
        private readonly IMcgQuickChangeTools _mcgQuickChangeTools;
        private readonly ISharedAppContext _sharedAppContext;

        public EcnDataCheckViewModel(IXmlSerializeTools xmlSerializeTools,
                                     IRegExTools regExTools,
                                     IWebtermTools webtermTools,
                                     IWindchillReportingManagementService windchillReportingManagementService,
                                     IWindchillNavigationService windchillNavigationService,
                                     IWindchillChangeManagementService windchillChangeManagementService,
                                     IWindchillPartManagementService windchillPartManagementService,
                                     IWindchillRequestTool windchillRequestTool,
                                     IWindchillBomManagementService windchillBomManagementService,
                                     IWindchillCredentialService windchillCredentialService,
                                     IWindchillCheckNumberService windchillCheckNumberService,
                                     ISapSessionManager sapSessionManager,
                                     ISapBomService sapBomService,
                                     IMcgCommonLibWindowService mcgCommonLibWindowService,
                                     IBomComparisonToolService bomComparisonToolService,
                                     IWindchillRequestMiscService windchillRequestMiscService,
                                     ISapHupService sapHupService,
                                     IEcnDataCheckWindchillService ecnDataCheckWindchillService,
                                     IMiscToolsWindchillService miscToolsWindchillService,
                                     IMcgQuickChangeTools mcgQuickChangeTools,
                                     ISharedAppContext sharedAppContext)
        {
            try
            {
                _xmlSerializeTools = xmlSerializeTools;
                _regExTools = regExTools;
                _webtermTools = webtermTools;
                _windchillReportingManagementService = windchillReportingManagementService;
                _windchillNavigationService = windchillNavigationService;
                _windchillChangeManagementService = windchillChangeManagementService;
                _windchillPartManagementService = windchillPartManagementService;
                _windchillRequestTool = windchillRequestTool;
                _windchillBomManagementService = windchillBomManagementService;
                _windchillCredentialService = windchillCredentialService;
                _windchillCheckNumberService = windchillCheckNumberService;
                _ISapSessionManager = sapSessionManager;
                _sapBomService = sapBomService;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;
                _bomComparisonToolService = bomComparisonToolService;
                _windchillRequestMiscService = windchillRequestMiscService;
                _sapHupService = sapHupService;
                _ecnDataCheckWindchillService = ecnDataCheckWindchillService;
                _miscToolsWindchillService = miscToolsWindchillService;
                _mcgQuickChangeTools = mcgQuickChangeTools;
                _sharedAppContext = sharedAppContext;

                CurrentEcnDataCheckDataContext = new EcnDataCheckDataContext();
                MainDispatcher = Dispatcher.CurrentDispatcher;
                InitApp();
            }
            catch (Exception ex)
            {
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }

            _regExTools = regExTools;
            _windchillCheckNumberService = windchillCheckNumberService;
            _mcgCommonLibWindowService = mcgCommonLibWindowService;
            _bomComparisonToolService = bomComparisonToolService;
        }

        public void InitApp()
        {
            try
            {
                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                // Read Xml configuration File
                CurrentEcnDataCheckConfiguration = _xmlSerializeTools.GetDeserializedXml<EcnDataCheckConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{EcnDataCheckConstants.ConfigurationFile}");

                // Read Naming Convention
                NamingConvention = _xmlSerializeTools.GetDeserializedXml<WindchillNamingConvention>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{CommonLibConstants.NamingConventionFile}");

                // Update DataContext object
                // Local Language - Main Interface
                foreach (MCGLanguage Lang in CurrentEcnDataCheckConfiguration.LocalLanguageList)
                    CurrentEcnDataCheckDataContext.ListLanguage.Add(Lang);
                CurrentEcnDataCheckDataContext.SelectedLanguage = (from elem in CurrentEcnDataCheckDataContext.ListLanguage where Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpper() == elem.SAPCode select elem).FirstOrDefault();

                // ERP system - Main interface
                foreach (string Erp in CurrentEcnDataCheckConfiguration.ErpList)
                    CurrentEcnDataCheckDataContext.ErpList.Add(Erp);
                CurrentEcnDataCheckDataContext.ErpSystem = CurrentEcnDataCheckConfiguration.ErpSystem;

                // SAP Menu - Plants
                foreach (SapPlant plant in CurrentEcnDataCheckConfiguration.ListSapPlant)
                {
                    CurrentEcnDataCheckDataContext.ListSapPlant.Add(plant);
                    if (plant.Number == CurrentEcnDataCheckConfiguration.SelectedSapPlant) CurrentEcnDataCheckDataContext.SelectedSapPlant = plant;
                }
                foreach (int LienNumber in CurrentEcnDataCheckConfiguration.NumericalLineNumberDigitList)
                    CurrentEcnDataCheckDataContext.NumericalLineNumberDigitList.Add(LienNumber);
                CurrentEcnDataCheckDataContext.NumericalLineNumberDigit = CurrentEcnDataCheckConfiguration.NumericalLineNumberDigit;

                // Liste Location - Move Tab
                foreach (string Location in CurrentEcnDataCheckConfiguration.ListLocation)
                    CurrentEcnDataCheckDataContext.ListLocation.Add(Location);

                CurrentEcnDataCheckDataContext.SelectedLocation = CurrentEcnDataCheckConfiguration.Location;
                CurrentEcnDataCheckDataContext.ContextFilter = CurrentEcnDataCheckConfiguration.ContextFilter;
                CurrentEcnDataCheckDataContext.IsCheckBoxLibraySelected = CurrentEcnDataCheckConfiguration.IsCheckBoxLibraySelected;
                CurrentEcnDataCheckDataContext.IsCheckBoxProductSelected = CurrentEcnDataCheckConfiguration.IsCheckBoxProductSelected;

                MCGLanguage CurrentMCGLANGUAGE = _sharedAppContext.CurrentLanguage?.Language;
                if (CurrentMCGLANGUAGE != null)
                    CurrentMCGLANGUAGE.ChangeLanguageInterface += UpdateInterfaceLanguage;

                UpdateInterfaceLanguage();
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateInterfaceLanguage(object sender = null, EventArgs e = null)
        {
            try
            {
                CurrentEcnDataCheckDataContext.StatusBarMsg1 = McgWpfTools.GetStringResource("EDC_SbMsg1");
                CurrentEcnDataCheckDataContext.StatusBarMsg2 = McgWpfTools.GetStringResource("EDC_SbMsg2");
                UpdateUpdateDataCheckResultItemListLanguage();
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteStartEcnDataCheck()
        {
            try
            {
                TraceLog.AddTraceLog($"Start ExecuteStartEcnDataCheck on {CurrentEcnDataCheckDataContext.EcnNumber}");
                CurrentEcnNumber = CurrentEcnDataCheckDataContext.EcnNumber;
                if (CurrentEcnDataCheckDataContext.EcnNumber == null || CurrentEcnDataCheckDataContext.EcnNumber.Trim() == "")
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("EDC_MsgEcnBlank"), "ECN Data Check", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CheckWindchillCredential();

               // WindchillChangeNotice CurrentWindchillObjectEcn = _windchillReportingManagementService.GetQueryBuilderEcn(WindchillNetworkCredential.WindchillCredential, CurrentEcnDataCheckDataContext.EcnNumber);
                WindchillChangeNotice CurrentWindchillObjectEcn = _windchillRequestTool.GetQueryBuilderEcn(WindchillNetworkCredential.WindchillCredential, CurrentEcnDataCheckDataContext.EcnNumber);

                if (CurrentWindchillObjectEcn != null && CurrentWindchillObjectEcn.ListEca != null && CurrentWindchillObjectEcn.ListEca.Count > 0)
                {
                    // update Eca List, with ALL s first value
                    CurrentEcnDataCheckDataContext.EcaList.Clear();
                    WindchillChangeActivity EcaAll = new WindchillChangeActivity() { Number = "ALL" };
                    CurrentEcnDataCheckDataContext.EcaList.Add(EcaAll);
                    CurrentEcnDataCheckDataContext.EcaNumber = EcaAll;

                    foreach (WindchillChangeActivity eca in CurrentWindchillObjectEcn.ListEca)
                        CurrentEcnDataCheckDataContext.EcaList.Add(eca);

                    MessageBoxResult EcaMessageBoxResult = MessageBoxResult.OK;
                    if (CurrentWindchillObjectEcn.ListEca.Count > 1)
                    {
                        EcaMessageBoxResult = _ecnDataCheckWindchillService.ShowDialogEcnDataCheckEcaSelection();
                    }

                    if (EcaMessageBoxResult == MessageBoxResult.OK)
                    {
                        CurrentEcnDataCheckDataContext.ShowActionButton = false;
                        CurrentEcnDataCheckDataContext.EcnDataCheckInProgress = true;
                        CurrentEcnDataCheckDataContext.GlobalStatus = DataCheckStatus.UNKNOWN;

                        // Start Complete Ecn Check

                        // init lists
                        CurrentEcnDataCheckDataContext.DataCheckItemList.Clear();
                        CurrentEcnDataCheckDataContext.DataCheckResultItemList.Clear();
                        ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).MissingWtPartInEcnList.Clear();
                        ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).OtherCheckItemList.Clear();
                        CurrentEcnDataCheckDataContext.RenameItemList.Clear();
                        CurrentEcnDataCheckDataContext.MoveItemList.Clear();

                        // Update Webterm List
                        GetWebtermList();
                        ReadPdmContextList();

                        // Start new thread
                        RaiseActionInProgressEvent();
                        Thread aThread = new Thread(new ThreadStart(StartCompleteEcnCheckAsynch));
                        aThread.IsBackground = true;
                        aThread.Start();
                    }
                }
                else
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("EDC_ErrorMsgEcnNotFound"), McgWpfTools.GetStringResource("EDC_ErrorMsgTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteBtHelpMouseLeftButtonUpEvent()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("EDC_LinkHelpEcnDataCheck"));

            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartSapBOMComparison()
        {
            try
            {
                if (CurrentEcnDataCheckDataContext.EcnNumber == CurrentEcnNumber)
                {
                    DownloadErpBom();
                }
                else
                {
                    if (CurrentEcnDataCheckDataContext.EcnNumber != null && CurrentEcnDataCheckDataContext.EcnNumber.Trim() != "")
                    {
                        CheckWindchillCredential();
                        ExportEcnBomInformationAsynch(CurrentEcnDataCheckDataContext.EcnNumber);
                    }
                }
                UpdateDataCheckResultItemList(true);
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartExportXLS()
        {
            try
            {
                ExportEcnDataCheckToExcel();
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRestartCheck(string FromWhichTab)
        {
            try
            {
                EcnDataCheckItem CurrentEcnDataCheckItem = GetDataCheckItemFromTab(FromWhichTab);

                CheckWindchillCredential();
                StartOneDatacheckItem(CurrentEcnDataCheckItem);
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenEcn(string FromWhichTab)
        {
            try
            {
                string EcnId = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice.Id;
                _windchillNavigationService.OpenEcnDetailPage(EcnId, null, CommonLibConstants.WindchillUrl);
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenEca(string FromWhichTab)
        {
            try
            {
                EcnDataCheckItem CurrentEcnDataCheckItem = GetDataCheckItemFromTab(FromWhichTab);
                if (CurrentEcnDataCheckItem == null || CurrentEcnDataCheckItem.EcnWtPart == null || CurrentEcnDataCheckItem.EcnWtPart.EcaNumber == null)
                    return;

                WindchillChangeActivity EcaId = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice.ListEca.FirstOrDefault((eca) => eca.Number == CurrentEcnDataCheckItem.EcnWtPart.EcaNumber);

                if (EcaId != null)
                    _windchillNavigationService.OpenEcaDetailPage(EcaId.Id, null, CommonLibConstants.WindchillUrl);
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenPart(string FromWhichTab)
        {
            try
            {
                EcnDataCheckItem CurrentEcnDataCheckItem = GetDataCheckItemFromTab(FromWhichTab);
                _windchillNavigationService.OpenWtPartDetailPage(CurrentEcnDataCheckItem.EcnWtPart.Id, null, CommonLibConstants.WindchillUrl);
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenBomPdmComp(string FromWhichTab)
        {
            try
            {
                BomComparisonItem CurrentBomComparisonItem = GetDataCheckItemFromTab(FromWhichTab).PdmBomComparison;
                _mcgCommonLibWindowService.ShowBomComparisonWindow(CurrentBomComparisonItem);

                //BomComparisonToolsView.BomComparisonWindow BomWindow = new BomComparisonToolsView.BomComparisonWindow();
                //BomWindow.DataContext = CurrentBomComparisonItem;
                //BomWindow.Show();
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenBomSapComp(string FromWhichTab)
        {
            try
            {
                BomComparisonItem CurrentBomComparisonItem = GetDataCheckItemFromTab(FromWhichTab).ErpBomComparison;
                _mcgCommonLibWindowService.ShowBomComparisonWindow(CurrentBomComparisonItem);

                //BomComparisonToolsView.BomComparisonWindow BomWindow = new BomComparisonToolsView.BomComparisonWindow();
                //BomWindow.DataContext = CurrentBomComparisonItem;
                //BomWindow.Show();
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMoveSendMail()
        {
            try
            {
                // Create Main Body Text
                string MailBody = String.Concat("<html><body><p>Hello, </p><p><br>Could you move following Objects in Windchill?",
                                                "<br>All PDM Objects with same Part Number should be moved in the new context and new folder.",
                                                "<br><br><strong>ECN Number: ", CurrentEcnDataCheckDataContext.EcnNumber, "</strong><br>",
                                                "<table cellpadding=2 border=1><tr align=center bgcolor=8DB4E2>",
                                                "<td width=200><strong>Part Number</strong></td>",
                                                "<td width=220><strong>Current Part Context</strong></td>",
                                                "<td width=220><strong>Current Part Folder</strong></td>",
                                                "<td width=220 bgcolor=C4D79B><strong>New Context for all PDM Objects</strong></td>",
                                                "<td width=220 bgcolor=C4D79B><strong>New Folder for all PDM Objects</strong></td></tr>");

                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.MoveItemList)
                    MailBody = $"{MailBody}<tr> <td>{CurrentItem.EcnWtPart.Number}</td> <td>{CurrentItem.EcnWtPart.Context.Name}</td> <td>{CurrentItem.EcnWtPart.Folder}</td> <td>{CurrentItem.NewContextName}</td>  <td>{CurrentItem.NewFolderName}</td> </tr>";

                string MailFrom = $"{System.Environment.GetEnvironmentVariable("USERNAME")}@manitowoc.com";
                MailBody = $"{MailBody}</table><br><p>Regards,</p><p>{MailFrom}</p></body></html>";

                CurrentEcnDataCheckConfiguration.CadAminEMail.MailRestritedListAddress = CurrentEcnDataCheckConfiguration.CadAminEMail.MailListAddress.FindAll((mail) => mail.SupportedLocation.Exists((location) => location == CurrentEcnDataCheckDataContext.SelectedLocation));
                CurrentEcnDataCheckConfiguration.CadAminEMail.Mailsubject = "Move Objects Activity";
                CurrentEcnDataCheckConfiguration.CadAminEMail.MailBody = MailBody;
                CurrentEcnDataCheckConfiguration.CadAminEMail.SendMailOutlook();
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMoveUdpateContextList()
        {
            try
            {
                List<WindchillContext> TempList = new List<WindchillContext>();

                if (CurrentEcnDataCheckDataContext.IsCheckBoxLibraySelected)
                    TempList.AddRange(((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).AllWindchillContextList.FindAll((c) => c.Type == WindchillContextType.LIBRARY));
                if (CurrentEcnDataCheckDataContext.IsCheckBoxProductSelected)
                    TempList.AddRange(((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).AllWindchillContextList.FindAll((c) => c.Type == WindchillContextType.PRODUCT));

                List<WindchillContext> TempListKeyWord = null;
                if (CurrentEcnDataCheckDataContext.ContextFilter != null && CurrentEcnDataCheckDataContext.ContextFilter.Trim() != "")
                {
                    CurrentEcnDataCheckDataContext.ContextFilter = CurrentEcnDataCheckDataContext.ContextFilter.Trim().Replace(" ", "|");
                    TempListKeyWord = new List<WindchillContext>();
                    List<Regex> ListKeyWordRegex = _regExTools.GetRegexList(CurrentEcnDataCheckDataContext.ContextFilter, true);
                    TempListKeyWord = TempList.FindAll((c) => _regExTools.CheckStringWithRegExList(c.Name, ListKeyWordRegex)).OrderBy((c) => c.Name).ToList();
                }
                else
                    TempListKeyWord = TempList.OrderBy((c) => c.Name).ToList();

                CurrentEcnDataCheckDataContext.WindchillContextList.Clear();
                foreach (WindchillContext context in TempListKeyWord)
                {
                    CurrentEcnDataCheckDataContext.WindchillContextList.Add(context);
                }
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSetContextToAll()
        {
            try
            {
                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.MoveItemList)
                    CurrentItem.NewContextName = CurrentEcnDataCheckDataContext.SelectedContext.ToString();
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRenameSendMail()
        {
            try
            {
                // Create Main Body Text
                string MailBody = String.Concat("<html><body><p>Hello, </p><p><br>Could you rename following Parts in Windchill",
                                                "<br><br><strong>ECN Number: ", CurrentEcnDataCheckDataContext.EcnNumber, "</strong><br>",
                                                "<table cellpadding=2 border=1><tr align=center bgcolor=8DB4E2>",
                                                "<td width=200><strong>Part Number</strong></td>",
                                                "<td width=150><strong>Revision</strong></td>",
                                                "<td width=220><strong>Old Name</strong></td>",
                                                "<td width=220 bgcolor=C4D79B><strong>New Name</strong></td></tr>");

                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.RenameItemList)
                    MailBody = $"{MailBody}<tr> <td>{CurrentItem.EcnWtPart.Number}</td> <td>{CurrentItem.EcnWtPart.Revision}</td> <td>{CurrentItem.EcnWtPart.Name}</td> <td>{CurrentItem.NewName}</td> </tr>";

                string MailFrom = $"{System.Environment.GetEnvironmentVariable("USERNAME")}@manitowoc.com";
                MailBody = $"{MailBody}</table><br><p>Regards,</p><p>{MailFrom}</p></body></html>";

                CurrentEcnDataCheckConfiguration.CadAminEMail.MailRestritedListAddress = CurrentEcnDataCheckConfiguration.CadAminEMail.MailListAddress.FindAll((mail) => mail.SupportedLocation.Exists((location) => location == CurrentEcnDataCheckDataContext.SelectedLocation));
                CurrentEcnDataCheckConfiguration.CadAminEMail.Mailsubject = "Rename Objects Activity";
                CurrentEcnDataCheckConfiguration.CadAminEMail.MailBody = MailBody;
                CurrentEcnDataCheckConfiguration.CadAminEMail.SendMailOutlook();
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCopyPartNumber(string Number)
        {
            try
            {
                if (Number != null)
                {
                    McgWpfTools.CopyTextClipboard(Number);
                }
            }
            catch (Exception ex)
            {
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartSapCraneSearch()
        {
            try
            {
                _miscToolsWindchillService.ShowAndExecuteCraneSearchMainView(CurrentEcnDataCheckDataContext.DataCheckItemList.Select(item => item.EcnWtPart.Number).ToList(), true);

                //CraneSearchMainView aCraneSearchMainView = McgWpfTools.IsWindowAlreadyCreated<CraneSearchMainView>(true);

                //if (aCraneSearchMainView == null)
                //{
                //    List<string> ListObject = CurrentEcnDataCheckDataContext.DataCheckItemList.Select(item => item.EcnWtPart.Number).ToList();
                //    aCraneSearchMainView = new CraneSearchMainView(ListObject);
                //    aCraneSearchMainView.Show();
                //    aCraneSearchMainView.CurrentDataContext.CommandSearchSapCrane.Execute(null);

                //}
                //else
                //    aCraneSearchMainView.Activate();
            }
            catch (Exception ex)
            {
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenLink(string htmlLink)
        {
            try
            {
                if (string.IsNullOrEmpty(htmlLink))
                    McgFileAndSystemTools.OpenFile(htmlLink);
            }
            catch (Exception ex)
            {
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private EcnDataCheckItem GetDataCheckItemFromTab(string FromWhichTab)
        {
            try
            {
                if (FromWhichTab == "ResultItem")
                    return (EcnDataCheckItem)CurrentEcnDataCheckDataContext.SelectedDataCheckResultItem.ParentEcnDataCheckItem;
                else
                    return (EcnDataCheckItem)CurrentEcnDataCheckDataContext.SelectedDataCheckItem;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Read information in SQL Server DataBase
        private void ReadPdmContextList()
        {
            try
            {
                var TempContextList = _webtermTools.GetPdmContextList();
                List<WindchillContext> AllContext = new List<WindchillContext>();
                foreach (var context in TempContextList)
                    AllContext.Add(new WindchillContext()
                    {
                        Name = context.PdmContext,
                        ParticipantId = context.ParticipantId,
                        ParticipantName = context.ParticipantName,
                        ParticipantType = context.ParticipantType,
                        TeamRole = context.TeamRole,
                        Type = context.Type == "PRODUCT" ? WindchillContextType.PRODUCT : WindchillContextType.LIBRARY
                    });

                ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).AllWindchillContextList = AllContext.OrderBy((c) => c.Name).ToList();
                MainDispatcher.Invoke(new Action(ExecuteMoveUdpateContextList));
                //}
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Check Data Methods
        private void StartCompleteEcnCheckAsynch()
        {
            try
            {
                // Update Number of step
                CurrentEcnDataCheckDataContext.CurrentStep = 0;
                CurrentEcnDataCheckDataContext.GlobalStatus = DataCheckStatus.OK;

                // Extract All Main Ecn Information (Parts, CAD Doc, Doc, Links between objects)
                CurrentEcnDataCheckDataContext.ExtraStatusBarMsg = McgWpfTools.GetStringResource("EDC_SbExtraMsg13");
                ExtractMainEcnInformationAsynch();

                // Extract document attached to the ECN
                DataCheckRule DataCheckRuleEcnWord = GetDataCheckRule("EcnAttachedWordFile");
                DataCheckRule DataCheckRuleEcnExcel = GetDataCheckRule("EcnAttachedExcelFile");
                if ((DataCheckRuleEcnWord != null && (DataCheckRuleEcnWord.RuleOption == DataCheckOption.E || DataCheckRuleEcnWord.RuleOption == DataCheckOption.W))
                    || (DataCheckRuleEcnExcel != null && (DataCheckRuleEcnExcel.RuleOption == DataCheckOption.E || DataCheckRuleEcnExcel.RuleOption == DataCheckOption.W)))
                    CheckEcnAttachedFile(DataCheckRuleEcnWord, DataCheckRuleEcnExcel);

                // Update Part list shown in the main Interface, invoke method on main thread
                MainDispatcher.Invoke(new Action(UpdateMainEcnInformationMainThread));

                // Update number of step if Rule PartRepresentationLoadedFromLegacy is activated
                DataCheckRule DataCheckRuleRepLoadedFromLegacy = GetDataCheckRule("PartRepresentationLoadedFromLegacy");
                if (DataCheckRuleRepLoadedFromLegacy != null && (DataCheckRuleRepLoadedFromLegacy.RuleOption == DataCheckOption.E || DataCheckRuleRepLoadedFromLegacy.RuleOption == DataCheckOption.W))
                    MultiplierStep++;

                CurrentEcnDataCheckDataContext.TotalStep = CurrentEcnDataCheckDataContext.DataCheckItemList.Count() * MultiplierStep + 1;

                // Search all context
                CheckWindchillCredential();
                //ReadPdmContextList();

                // Check all Parts MetaData
                CheckAllPartsAttributesAsync();

                // Check all CadDocuments MetatData
                CheckAllCadDocumentAttributesAsync();

                // Check Link accuracy between Part And CadDocument
                CheckAllLinkPartCadDocAsync();

                MainDispatcher.Invoke(() => UpdateDataCheckResultItemList());
                //MainDispatcher.Invoke(new Action(UpdateDataCheckResultItemList));

                // Check if CadDocument is not linked to Part
                SearchAllCadDocAsync();
                CheckAllMissingLinkCadDocAsync();

                MainDispatcher.Invoke(() => UpdateDataCheckResultItemList());
                //MainDispatcher.Invoke(new Action(UpdateDataCheckResultItemList));

                // Check if Part is missing in ECN (CAD Document in ECN but not the part)
                CheckAllMissingPartInEcnAsync();
                MainDispatcher.Invoke(new Action(UpdateDataCheckItemListWithMissingPart));

                // Check if Part is in several ECN
                CheckAllPartEcnLinkAsync();

                // Check if Part Representaion has Rep loaded from legacy (For Shady Grove Only)
                if (DataCheckRuleRepLoadedFromLegacy != null && (DataCheckRuleRepLoadedFromLegacy.RuleOption == DataCheckOption.E || DataCheckRuleRepLoadedFromLegacy.RuleOption == DataCheckOption.W))
                    SearchAndCheckAllPartRepresentationAsync();

                // Check if CAD is missing in ECN 
                CheckAllMissingCadDocInEcnAsync();

                // Check Extra Cad Doc (With wrong revision) in the ECN 
                CheckAllExtraCadDocInEcnAsync();

                // Check Link Part - WTDocument. 
                // Check is WTDocument is TifPlan and accurate with Part (Number+Revision)
                // Check if WTDocument is missing in ECN
                // check if WTDocument has a content
                // Check if Part is missing in ECN (WTDocument in ECN but not the part)
                // Check if WtDocument is linked to the part
                CheckAllPartsWTDocLinkAsync();

                // Check unchecked Epm Doc in the ECN
                CheckAllUncheckedEpmDocAttributes();

                MainDispatcher.Invoke(() => UpdateDataCheckResultItemList());
                //MainDispatcher.Invoke(new Action(UpdateDataCheckResultItemList));

                // Download All Parts and EpmDocs (owner link) BOM and do comparison
                DownloadAllDataCheckItemBomAsync();
                MainDispatcher.Invoke(() => UpdateDataCheckResultItemList());
                //MainDispatcher.Invoke(new Action(UpdateDataCheckResultItemList));

                // Check components of the diffrent Part BOM
                CurrentEcnDataCheckDataContext.ExtraStatusBarMsg = McgWpfTools.GetStringResource("EDC_SbExtraMsg09");
                CheckAllPartBomComponentAsync();

                MainDispatcher.Invoke(() => UpdateDataCheckResultItemList());
                //MainDispatcher.Invoke(new Action(UpdateDataCheckResultItemList));

                CurrentEcnDataCheckDataContext.CurrentStep++;
                CurrentEcnDataCheckDataContext.ExtraStatusBarMsg = McgWpfTools.GetStringResource("EDC_SbExtraMsg04");
                ResetInterface();

            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void StartOneDatacheckItem(EcnDataCheckItem CurrentItem)
        {
            try
            {
                CurrentItem.ListDataCheckResult.Clear();
                CurrentItem.ListDataCheckResultShown.Clear();
                CurrentItem.IsResultItem = false;

                CurrentItem.BomPdmComparisonStatus = DataCheckStatus.UNKNOWN;
                CurrentItem.Desc1EnStatus = DataCheckStatus.OK;
                CurrentItem.Desc1LocalStatus = DataCheckStatus.OK;
                CurrentItem.Desc2EnStatus = DataCheckStatus.OK;
                CurrentItem.Desc2LocalStatus = DataCheckStatus.OK;
                CurrentItem.GroupCreatorStatus = DataCheckStatus.OK;
                CurrentItem.MassStatus = DataCheckStatus.OK;
                CurrentItem.MetaDataStatus = DataCheckStatus.OK;
                CurrentItem.QualInspGrpStatus = DataCheckStatus.OK;
                CurrentItem.PartMissingCheck = DataCheckStatus.OK;
                CurrentItem.ContextStatus = DataCheckStatus.OK;

                CurrentItem.EpmDocStructure = null;
                CurrentItem.LinkWtPartEpmDocumentDescribe.Clear();
                CurrentItem.LinkWtPartEpmDocumentOwner.Clear();
                CurrentItem.LinkWtPartWtDocumentDescribe.Clear();
                CurrentItem.LinkWtPartWtDocumentReference.Clear();
                CurrentItem.ListEpmDocument.Clear();
                CurrentItem.ListSearchedEpmDocument.Clear();
                CurrentItem.ListWtDocument.Clear();

                // Extra MainECN Information
                ExtractMainEcnInformationAsynch();
                WindchillChangeNotice CurrentWindchillChangeNotice = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice;
                WindchillObjectWtPart CurrentWtPart = CurrentWindchillChangeNotice.ListWtPart.FirstOrDefault((part) => part.Equals(CurrentItem.EcnWtPart));

                if (CurrentItem.EcnWtPart == null)
                    return;
                else
                    UpdateMainOneDataItemInformation(CurrentItem, CurrentWtPart);

                // Check Part MetaData
                CheckOnePartAttributes(CurrentItem);

                // Check all CadDocument MetatData
                CheckOneDataCheckItemAllCadDocAttributes(CurrentItem);

                // Check Link accuracy between Part And CadDocument
                CheckOneLinkPartCadDoc(CurrentItem);

                // Check if CadDocument is not linked to Part
                SearchOneCadDoc(CurrentItem);
                CheckOneMissingLinkCadDoc(CurrentItem);

                // Check if Part is in several ECN
                CheckOnePartEcnLink(CurrentItem);

                // Check if CAD is missing in ECN 
                CheckOneMissingCadDocInEcn(CurrentItem);

                // Check Extra Cad Doc (With wrong revision) in the ECN
                CheckExtraCadDocInEcnAsync(CurrentItem);

                // Check Link Part - WTDocument. 
                // Check is WTDocument is TifPlan and accurate with Part (Number+Revision)
                // Check if WTDocument is missing in ECN
                // check if WTDocument has a content
                // Check if Part is missing in ECN (WTDocument in ECN but not the part)
                // Check if WtDocument is linked to the part
                CheckOnePartWTDocLink(CurrentItem);


                // Download Part and and EpmDoc (owner link) BOM
                // Check if Physical part
                if (CurrentItem.EcnWtPart.CheckedObject.NumberTemplate != null && CurrentItem.EcnWtPart.CheckedObject.NumberTemplate.FunctionalType == WindchillObjectType.PHYSICAL_PART)
                {
                    DownloadOneDataCheckItemBomAsync(CurrentItem);
                    CompareOneDataCheckItemPdmBomAsync(CurrentItem);
                }
                else
                {
                    CurrentItem.BomPdmComparisonStatus = DataCheckStatus.NONE;
                    CurrentItem.IsPdmBomComparison = false;
                }

                // Check components of the diffrent Part BOM
                CheckOnePartBomComponentAsync(CurrentItem);

                UpdateDataCheckResultItemList();
            }
            catch (Exception ex)
            {
                ResetInterface();
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckEcnAttachedFile(DataCheckRule DataCheckRuleEcnWord, DataCheckRule DataCheckRuleEcnExcel)
        {
            try
            {
                bool IsWordDoc = false;
                bool IsExcelDoc = false;
                bool IsAlreadyAdded = false;
                Regex WordRegex = new Regex($@"{CurrentEcnDataCheckDataContext.EcnNumber}.*\.doc[x,m]?$", RegexOptions.IgnoreCase);
                Regex ExcelRegex = new Regex($@"{CurrentEcnDataCheckDataContext.EcnNumber}.*\.xls[x,m]?$", RegexOptions.IgnoreCase);

                List<RestOdataAttachment> CurrentListDoc = _windchillChangeManagementService.GetChangeNoticeattachments(WindchillNetworkCredential.WindchillCredential, CurrentEcnDataCheckDataContext.EcnNumber, CommonLibConstants.WindchillUrl);
                if (CurrentListDoc != null && CurrentListDoc.Count > 0)
                {
                    IsWordDoc = CurrentListDoc.Exists((item) => WordRegex.IsMatch(item.FileName));
                    IsExcelDoc = CurrentListDoc.Exists((item) => ExcelRegex.IsMatch(item.FileName));
                }

                EcnDataCheckItem CurrentItem = new EcnDataCheckItem() { EcnWtPart = new WindchillObjectWtPart() { Number = CurrentEcnDataCheckDataContext.EcnNumber, Revision = "" } };
                if (!IsWordDoc && DataCheckRuleEcnWord != null && DataCheckRuleEcnWord.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    CreateResultItem(CurrentItem, DataCheckRuleEcnWord, "EDC_CheckMsg55");
                    ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).OtherCheckItemList.Add(CurrentItem);
                    IsAlreadyAdded = true;
                }
                if (!IsExcelDoc && DataCheckRuleEcnExcel != null && DataCheckRuleEcnExcel.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    CreateResultItem(CurrentItem, DataCheckRuleEcnExcel, "EDC_CheckMsg56");
                    if (!IsAlreadyAdded)
                        ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).OtherCheckItemList.Add(CurrentItem);
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckAllPartsAttributesAsync()
        {
            try
            {
                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList)
                    CheckOnePartAttributes(CurrentItem);

            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckOnePartAttributes(EcnDataCheckItem CurrentItem)
        {
            try
            {
                DataCheckRule CurrentDataCheckRule = null;
                DataCheckValue CurrentDataCheckValue;
                WebtermLanguage CurrentWebtermLanguage = _webtermTools.GetWebtermLanguage(CurrentEcnDataCheckDataContext.SelectedLanguage);

                // Check PartNumber
                CurrentDataCheckRule = GetDataCheckRule("PartNumberAccurate");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    if (CurrentItem.EcnWtPart.CheckedObject == null || !CurrentItem.EcnWtPart.CheckedObject.IsNumberAccurate)
                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg16");

                // Check Webterm English
                CurrentDataCheckRule = GetDataCheckRule("PartWebtermEnglish");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    CurrentDataCheckValue = IsAccurateDesc1Eng(CurrentItem);
                    if (CurrentDataCheckValue != DataCheckValue.OK)
                        CurrentItem.Desc1EnStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg09").Status;
                }

                // Check Webterm Local
                CurrentDataCheckRule = GetDataCheckRule("PartWebtermLocal");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    CurrentDataCheckValue = IsAccurateDesc1Local(CurrentItem);
                    if (CurrentDataCheckValue != DataCheckValue.OK)
                        CurrentItem.Desc1LocalStatus = CreateResultItem(CurrentItem,
                                                                        CurrentDataCheckRule,
                                                                        "EDC_CheckMsg10",
                                                                        new string[2]
                                                                        {
                                                                            _webtermTools.GetTermFromEnglish(CurrentItem.EcnWtPart.Name,CurrentWebtermLanguage), 
                                                                            CurrentEcnDataCheckDataContext.SelectedLanguage.DataTableColonne
                                                                        }).Status;
                }

                // Check Description 2 English
                CurrentDataCheckValue = IsAccurateDesc2Eng(CurrentItem);
                if (CurrentDataCheckValue != DataCheckValue.OK)
                {
                    // Check if Description 2 in English is defined
                    CurrentDataCheckRule = GetDataCheckRule("PartDetailDescEnglishUndefined");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.UNDEFINED)
                        CurrentItem.Desc2EnStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg11").Status;

                    //Check if the length of Description 2 in English is not too long
                    CurrentDataCheckRule = GetDataCheckRule("PartDetailDescEnglishLength");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.HIGH)
                        CurrentItem.Desc2EnStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg14").Status;
                }

                // Check Description 2 Local
                CurrentDataCheckValue = IsAccurateDesc2Local(CurrentItem);
                if (CurrentDataCheckValue != DataCheckValue.OK)
                {
                    // Check if Description 2 in local language is defined
                    CurrentDataCheckRule = GetDataCheckRule("PartDetailDescLocalUndefined");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.UNDEFINED)
                        CurrentItem.Desc2LocalStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg12").Status;

                    // Check if the length of Description 2 in local language is blank
                    CurrentDataCheckRule = GetDataCheckRule("PartDetailDescLocalBlank");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                        CurrentItem.Desc2LocalStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg13").Status;

                    // Check if the length of Description 2 in local language is not too long
                    CurrentDataCheckRule = GetDataCheckRule("PartDetailDescLocalLength");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.HIGH)
                        CurrentItem.Desc2LocalStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg15").Status;
                }

                // Check that concatenated DESC1+DESC2 does not exceed XX characters
                CurrentDataCheckRule = GetDataCheckRule("PartConcatenatedDescEnLength");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE &&
                    $"{CurrentItem.EcnWtPart.Name} {CurrentItem.EcnWtPart.DescriptionEn2}".Length > EcnDataCheckConstants.MaxLenghtConcatenatedDesc)
                    CurrentItem.Desc2LocalStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg51", new string[1] { EcnDataCheckConstants.MaxLenghtConcatenatedDesc.ToString() }).Status;
                CurrentDataCheckRule = GetDataCheckRule("PartConcatenatedDescLocalLength");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE &&
                    $"{CurrentItem.EcnWtPart.DescriptionLocal1} {CurrentItem.EcnWtPart.DescriptionLocal2}".Length > EcnDataCheckConstants.MaxLenghtConcatenatedDesc)
                    CurrentItem.Desc2LocalStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg52", new string[1] { EcnDataCheckConstants.MaxLenghtConcatenatedDesc.ToString() }).Status;
                // Check if detected Language is same than selected local language: Warning
                CurrentDataCheckRule = GetDataCheckRule("PartWebtermLocalLocation");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    CurrentDataCheckValue = IsSameLocalLanguage(CurrentItem);
                    if (CurrentDataCheckValue != DataCheckValue.OK)
                    {
                        CurrentItem.Desc1LocalStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg37").Status;
                    }
                }

                // Check GroupCreator
                CurrentDataCheckValue = IsAccurateGroupCreator(CurrentItem);
                if (CurrentDataCheckValue != DataCheckValue.OK)
                {
                    // Check if GROUP_CREATOR is defined
                    CurrentDataCheckRule = GetDataCheckRule("PartGroupCreatorUndefined");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.UNDEFINED)
                        CurrentItem.GroupCreatorStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg06").Status;

                    // Check if GROUP_CREATOR is accurate
                    CurrentDataCheckRule = GetDataCheckRule("PartGroupCreatorAccurate");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                        CurrentItem.GroupCreatorStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg07", new string[1] { CurrentItem.EcnWtPart.GroupCreator }).Status;

                    // Check if GROUP_CREATOR is defined for the right location
                    // Not yet implemented
                    CurrentDataCheckRule = GetDataCheckRule("PartGroupCreatorLocation");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.LOCATIONISSUE)
                        CurrentItem.GroupCreatorStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg08").Status;
                }

                //Check MASS and QualInspGroup only for PHYSICAL_PART
                if (CurrentItem.EcnWtPart.CheckedObject.NumberTemplate != null && CurrentItem.EcnWtPart.CheckedObject.NumberTemplate.FunctionalType == WindchillObjectType.PHYSICAL_PART)
                {
                    // Check Mass
                    CurrentDataCheckValue = IsAccurateMass(CurrentItem);
                    if (CurrentDataCheckValue != DataCheckValue.OK)
                    {
                        // Check if MASS is defined
                        CurrentDataCheckRule = GetDataCheckRule("PartMassUndefined");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.UNDEFINED)
                            CurrentItem.MassStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg01").Status;

                        // Check if MASS is not too low
                        CurrentDataCheckRule = GetDataCheckRule("PartMassLow");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.LOW)
                            CurrentItem.MassStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg02", new string[1] { CurrentItem.EcnWtPart.Mass.ToString() }).Status;

                        // Check if MASS is not too high
                        CurrentDataCheckRule = GetDataCheckRule("PartMassHigh");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.HIGH)
                            CurrentItem.MassStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg03", new string[1] { CurrentItem.EcnWtPart.Mass.ToString() }).Status;
                    }

                    // Check Quality Inspection Group
                    CurrentDataCheckValue = IsAccurateQualInspGrp(CurrentItem);
                    if (CurrentDataCheckValue != DataCheckValue.OK)
                    {
                        // Check if QualInspGrp is defined
                        CurrentDataCheckRule = GetDataCheckRule("PartQualInspGrpUndefined");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.UNDEFINED)
                            CurrentItem.QualInspGrpStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg04").Status;

                        // Check if QualInspGrp is not defined at value X
                        CurrentDataCheckRule = GetDataCheckRule("PartQualInspGrpValueX");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.WARNING)
                            CurrentItem.QualInspGrpStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg39").Status;

                        // Check if QualInspGrp is not too low
                        CurrentDataCheckRule = GetDataCheckRule("PartQualInspGrpLow");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.LOW)
                            CurrentItem.QualInspGrpStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg05", new string[1] { CurrentItem.EcnWtPart.QualInspGrp }).Status;
                    }

                    // Check Default Unit with Webterm
                    CurrentDataCheckRule = GetDataCheckRule("PartDefaultUnit");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {
                        CurrentDataCheckValue = IsAccurateDefaultUnit(CurrentItem);
                        if (CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                            CurrentItem.DefaultUnitStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg49", new string[2] { CurrentItem.EcnWtPart.DefaultUnit, CurrentItem.EcnWtPart.TemplateWebterm.Defaultunit }).Status;
                    }

                    // Check Material
                    // Check if Default Material assigned (Attribute equel to UNDEFINED or UNDEFINED_MC)
                    CurrentDataCheckRule = GetDataCheckRule("MaterialDefaultAssigned");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {
                        CurrentDataCheckValue = IsMaterialDefaultAssigned(CurrentItem);
                        if (CurrentDataCheckValue == DataCheckValue.WARNING)
                            CurrentItem.MaterialStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg58", new string[1] { CurrentItem.EcnWtPart.Material }).Status;

                    }
                    // Check if Material assigned (Attribute not blank)
                    CurrentDataCheckRule = GetDataCheckRule("MaterialAssigned");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {
                        CurrentDataCheckValue = IsMaterialAssigned(CurrentItem);
                        if (CurrentDataCheckValue == DataCheckValue.UNDEFINED)
                            CurrentItem.MaterialStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg57").Status;
                    }

                    // Check if Revision is higher on Windchill than in SAP
                    CurrentDataCheckRule = GetDataCheckRule("PartRevisionHigherThanInSap");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {
                        CurrentDataCheckValue = IsRevisionHigherThanInSap(CurrentItem);
                        if (CurrentDataCheckValue == DataCheckValue.EQUAL)
                            CurrentItem.RevisionStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg73", new string[1] { CurrentItem.EcnWtPart.Revision }).Status;
                        if (CurrentDataCheckValue == DataCheckValue.LOW)
                            CurrentItem.RevisionStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg74", new string[2] { CurrentItem.EcnWtPart.Revision, CurrentItem.EcnWtPart.ErpRevision }).Status;
                    }

                    // Check Brand, Group and SubGroup
                    CurrentDataCheckRule = GetDataCheckRule("PartBrandGroupSubGroup");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {
                        if (ListBrandGroupSubGroup == null || ListBrandGroupSubGroup.Count == 0)
                            ListBrandGroupSubGroup = McgBusinessTools.GetLIstBrandGroupSubGroup();

                        // brand
                        CurrentDataCheckValue = IsBrandAssigned(CurrentItem);
                        if (CurrentDataCheckValue == DataCheckValue.UNDEFINED)
                            CurrentItem.BrandStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg63", new string[1] { CurrentItem.EcnWtPart.Name }).Status;
                        if (CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                            CurrentItem.BrandStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg68", new string[1] { CurrentItem.EcnWtPart.Brand }).Status;

                        // Group
                        if (CurrentDataCheckValue == DataCheckValue.OK)
                        {
                            CurrentDataCheckValue = IsGroupAssigned(CurrentItem);
                            //if (CurrentDataCheckValue == DataCheckValue.UNDEFINED)
                            //    CurrentItem.MaterialStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg63", new string[1] { CurrentItem.EcnWtPart.Brand }).Status;
                            if (CurrentDataCheckValue == DataCheckValue.UNDEFINED)
                                CurrentItem.BrandStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg63", new string[1] { CurrentItem.EcnWtPart.Name }).Status;
                            if (CurrentDataCheckValue == DataCheckValue.NOTFOUND)
                                CurrentItem.BrandStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg64", new string[2] { CurrentItem.EcnWtPart.Group, CurrentItem.EcnWtPart.Brand }).Status;
                            if (CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                                CurrentItem.BrandStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg65", new string[3] { CurrentItem.EcnWtPart.Brand, CurrentItem.EcnWtPart.Group, string.Join(";", GetAccuratBrand(CurrentItem.EcnWtPart.Group)) }).Status;

                            CurrentDataCheckValue = IsGroupTermAccurate(CurrentItem);
                            if (CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                                CurrentItem.GroupStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg66", new string[2] { CurrentItem.EcnWtPart.Name, CurrentItem.EcnWtPart.Group }).Status;
                        }

                        // Sub_Group
                        if (CurrentDataCheckValue == DataCheckValue.OK)
                        {
                            CurrentDataCheckValue = IsSubGroupAssigned(CurrentItem);
                            if (CurrentDataCheckValue == DataCheckValue.UNDEFINED)
                                CurrentItem.SubGroupStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg60", new string[1] { CurrentItem.EcnWtPart.Group }).Status;
                            if (CurrentDataCheckValue == DataCheckValue.NOTFOUND)
                                CurrentItem.SubGroupStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg61", new string[2] { CurrentItem.EcnWtPart.SubGroup, CurrentItem.EcnWtPart.Group }).Status;
                            if (CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                                CurrentItem.GroupStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg62", new string[3] { CurrentItem.EcnWtPart.Group, CurrentItem.EcnWtPart.SubGroup, string.Join(";", GetAccuratGroup(CurrentItem.EcnWtPart.SubGroup)) }).Status;

                            CurrentDataCheckValue = IsSubGroupTermAccurate(CurrentItem);
                            if (CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                                CurrentItem.SubGroupStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg67", new string[3] { CurrentItem.EcnWtPart.Name, CurrentItem.EcnWtPart.Group, CurrentItem.EcnWtPart.SubGroup }).Status;
                        }

                        // Option
                        CurrentDataCheckValue = IsOptionAccurate(CurrentItem);
                        if (CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                            CurrentItem.OptionStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg69", new string[4] { CurrentItem.EcnWtPart.Option,
                                                                                                                                             CurrentItem.EcnWtPart.Brand,
                                                                                                                                             CurrentItem.EcnWtPart.Group,
                                                                                                                                             CurrentItem.EcnWtPart.SubGroup}).Status;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckAllCadDocumentAttributesAsync()
        {
            try
            {
                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList)
                    CheckOneDataCheckItemAllCadDocAttributes(CurrentItem);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckOneDataCheckItemAllCadDocAttributes(EcnDataCheckItem CurrentItem)
        {
            try
            {
                foreach (WindchillObjectEpmDocument epm in CurrentItem.ListEpmDocument.Where((item) => item.FileName != null))
                {
                    CheckOneCadDocAttributes(CurrentItem, epm);
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckOneCadDocAttributes(EcnDataCheckItem CurrentItem, WindchillObjectEpmDocument CurrentEpm)
        {
            try
            {
                DataCheckRule CurrentDataCheckRule = null;
                DataCheckValue CurrentDataCheckValue;

                // Check if EPM Doc Number and FileName are the same
                CurrentDataCheckRule = GetDataCheckRule("CadDocFileNameNumber");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    CurrentDataCheckValue = IsSameNumberFileName(CurrentEpm);
                    if (CurrentDataCheckValue != DataCheckValue.OK)
                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg19", new string[2] { CurrentEpm.FileName, CurrentEpm.Number }, CurrentEpm);
                }

                // Check if EPM Doc Number is accurate
                CurrentDataCheckRule = GetDataCheckRule("CadDocNumberAccurate");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    if (CurrentEpm.CheckedObject == null || !CurrentEpm.CheckedObject.IsNumberAccurate)
                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg20", new string[1] { CurrentEpm.Number }, CurrentEpm);
                }

                // Check if Suffix is Accurate
                CurrentDataCheckRule = GetDataCheckRule("CadDocSuffixAccurate");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    if (CurrentEpm.CheckedObject != null && !CurrentEpm.CheckedObject.IsSuffixAccurate)
                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg21", new string[1] { CurrentEpm.Number }, CurrentEpm);
                }

                // check if suffix separator is accurate
                CurrentDataCheckRule = GetDataCheckRule("CadDocSuffixSepAccurate");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    if (CurrentEpm.CheckedObject != null && !CurrentEpm.CheckedObject.IsSuffixSeparatorAccurate)
                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg22", new string[2] { CurrentEpm.Number, NamingConvention.SuffixSeparator }, CurrentEpm);
                }

                // Check if there is an extension to the Number
                CurrentDataCheckRule = GetDataCheckRule("CadDocFileNameExtensionAccurate");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    CurrentDataCheckValue = IsNumberHasAccurateExtension(CurrentEpm);
                    if (CurrentDataCheckValue == DataCheckValue.UNDEFINED)
                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg23", new string[1] { CurrentEpm.Number }, CurrentEpm);
                    if (CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg46", new string[3] { CurrentEpm.Number,
                                                                                                              CurrentEpm.FileName.Split('.')[1].ToUpper(),
                                                                                                              CurrentEpm.Number.Split('.')[1].ToUpper() }, CurrentEpm);
                }

                // Checks only for Drawing
                if (CurrentEpm.CheckedObject != null && CurrentEpm.CheckedObject.SubType == WindchillObjectType.DRW)
                {
                    // MCC REP Check: Check if REP has been run on the DRW
                    CurrentDataCheckRule = GetDataCheckRule("CadDocRepBlank");
                    DataCheckRule CurrentDataCheckRule2 = GetDataCheckRule("CadDocRepImplemented");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule2 != null && (CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE || CurrentDataCheckRule2.GetDataCheckStatus() != DataCheckStatus.NONE))
                    {
                        CurrentDataCheckValue = IsRepImplemented(CurrentEpm);

                        // Check if Type for Drawing is Blank --> REP not implemented
                        if (CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.UNDEFINED)
                            CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg17", null, CurrentEpm);

                        // Check if Type for Drawing is ***NO REP*** --> REP not implemented
                        if (CurrentDataCheckRule2.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                            CreateResultItem(CurrentItem, CurrentDataCheckRule2, "EDC_CheckMsg18", null, CurrentEpm);
                    }

                    // Check if DRW Last Modified On is equal or erlier to 3D Last Modified On (same base Number)
                    CurrentDataCheckRule = GetDataCheckRule("CadDocDrwModifedOn");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {
                        CurrentDataCheckValue = IsDrwErlierThan3D(CurrentItem, CurrentEpm);
                        if (CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                            CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg50", new string[1] { CurrentItem.TempMessage }, CurrentEpm);
                    }
                }



                CurrentEpm.HasBeenchecked = true;

            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckAllLinkPartCadDocAsync()
        {
            try
            {
                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList)
                    CheckOneLinkPartCadDoc(CurrentItem);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckOneLinkPartCadDoc(EcnDataCheckItem CurrentItem)
        {
            try
            {
                DataCheckRule CurrentDataCheckRule = null;

                foreach (WindchillObjectLink CurrentLinkObj in CurrentItem.LinkWtPartEpmDocumentDescribe.Union(CurrentItem.LinkWtPartEpmDocumentOwner))
                {
                    // Check if Link between Part and CadDoc is accurate
                    CurrentDataCheckRule = GetDataCheckRule("LinkPartCadDocAccurate");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {

                        // check if double required link possible (CONTENT_OR_IMAGE)
                        bool isLinkAccurate = false;
                        if (CurrentLinkObj.LinkedObject.CheckedObject != null && CurrentLinkObj.LinkedObject.CheckedObject.SuffixTemplate.RequiredLink == WindchillObjectLinkType.CONTENT_OR_IMAGE)
                        {
                            if (CurrentLinkObj.LinkType == WindchillObjectLinkType.CONTENT || CurrentLinkObj.LinkType == WindchillObjectLinkType.IMAGE)
                            {
                                isLinkAccurate = true;
                            }
                            else
                            {
                                CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg24",
                                    new string[1] { McgWpfTools.GetStringResource($"EDC_Link_{CurrentLinkObj.LinkedObject.CheckedObject.SuffixTemplate.RequiredLink}") },
                                    CurrentLinkObj.LinkedObject);
                            }
                        }
                        else if (CurrentLinkObj.LinkedObject.CheckedObject != null && CurrentLinkObj.LinkedObject.CheckedObject.SuffixTemplate.RequiredLink != CurrentLinkObj.LinkType)
                            CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg24",
                                new string[1] { McgWpfTools.GetStringResource($"EDC_Link_{CurrentLinkObj.LinkedObject.CheckedObject.SuffixTemplate.RequiredLink}") },
                                CurrentLinkObj.LinkedObject);
                    }

                    // Check if Part and Cad Doc have the same Revision
                    CurrentDataCheckRule = GetDataCheckRule("LinkPartCadDocRevision");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {
                        if (CurrentLinkObj.MainObject.Revision != CurrentLinkObj.LinkedObject.Revision)
                            CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg25", null, CurrentLinkObj.LinkedObject);
                    }

                    // Check if Part and Cad Doc have the same BaseNumber
                    CurrentDataCheckRule = GetDataCheckRule("LinkPartCadDocNumber");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {
                        if (CurrentLinkObj.LinkedObject.CheckedObject != null && CurrentLinkObj.MainObject.CheckedObject != null
                                && CurrentLinkObj.LinkedObject.CheckedObject.ExtractedNumber != CurrentLinkObj.MainObject.CheckedObject.ExtractedNumber)
                            CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg26", null, CurrentLinkObj.LinkedObject);
                    }

                    // Check if Part and Cad Doc are in the same Context
                    CurrentDataCheckRule = GetDataCheckRule("LinkPartCadDocContext");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {
                        if (CurrentLinkObj.LinkedObject.CheckedObject != null && CurrentLinkObj.MainObject.CheckedObject != null
                                && CurrentLinkObj.LinkedObject.CheckedObject.IsAccurate
                                && CurrentLinkObj.LinkedObject.CheckedObject.ExtractedNumber == CurrentLinkObj.MainObject.CheckedObject.ExtractedNumber
                                && CurrentLinkObj.LinkedObject.Context.Name != CurrentLinkObj.MainObject.Context.Name)
                            CurrentItem.ContextStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg35", null, CurrentLinkObj.LinkedObject).Status;
                    }

                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckAllMissingLinkCadDocAsync()
        {
            try
            {
                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList)
                    CheckOneMissingLinkCadDoc(CurrentItem);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckOneMissingLinkCadDoc(EcnDataCheckItem CurrentItem)
        {
            try
            {
                DataCheckRule CurrentDataCheckRule = null;
                List<WindchillObjectLink> LinkedEpm = CurrentItem.LinkWtPartEpmDocumentDescribe.Union(CurrentItem.LinkWtPartEpmDocumentOwner).ToList();

                foreach (WindchillObjectEpmDocument CurrentEpmDoc in CurrentItem.ListSearchedEpmDocument.FindAll((epm) => !LinkedEpm.Exists((objlink) => objlink.LinkedObject.Equals(epm))))
                {
                    CurrentDataCheckRule = GetDataCheckRule("LinkPartCadDocMissing");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg27", new string[1] { CurrentEpmDoc.CheckedObject.SuffixTemplate.RequiredLink.ToString() }, CurrentEpmDoc);

                    CheckOneCadDocAttributes(CurrentItem, CurrentEpmDoc);
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckAllMissingPartInEcnAsync()
        {
            try
            {
                DataCheckRule CurrentDataCheckRule = GetDataCheckRule("PartEcnMissing");

                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    // Concat all WtPart List from all ECA
                    List<WindchillObjectWtDocument> AllWtDoc = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice.ListWtDocument;
                    List<WindchillObjectEpmDocument> AllEpmDoc = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice.ListEpmDocument;
                    List<WindchillObjectWtPart> AllWtPartDoc = new List<WindchillObjectWtPart>();
                    foreach (WindchillChangeActivity CurrentEca in ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice.ListEca)
                        AllWtPartDoc.AddRange(CurrentEca.ListWtPart);

                    EcnDataCheckItem CurrentItem = null;

                    foreach (string MissingNumber in AllEpmDoc.
                                    FindAll((epm) => !AllWtPartDoc.Exists((wtpart) => epm.CheckedObject.ExtractedNumber == wtpart.Number)).
                                    Select((epm) => epm.CheckedObject.ExtractedNumber).Distinct())
                    {
                        CurrentItem = new EcnDataCheckItem() { EcnWtPart = new WindchillObjectWtPart() { Number = MissingNumber, Revision = "?" } };
                        CurrentItem.PartMissingCheck = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg28", new string[1] { MissingNumber }).Status;
                        ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).MissingWtPartInEcnList.Add(CurrentItem);
                    }


                    // Check if WTPart missing in the ECN for the WTDocuments
                    var allMissingPart = AllWtDoc.FindAll((epm) => !AllWtPartDoc.Exists((wtpart) => epm.CheckedObject.ExtractedNumber == wtpart.Number)).
                                    Select((epm) => epm.CheckedObject.ExtractedNumber).Distinct().ToList();

                    Dictionary<string, string> listNumbers = new Dictionary<string, string>();
                    foreach (var number in allMissingPart)
                        listNumbers.Add(number, "");

                    var listPart = _windchillPartManagementService.GetListPart(WindchillNetworkCredential.WindchillCredential, listNumbers, CommonLibConstants.WindchillUrl);

                    foreach (var MissingNumber in listPart)
                    {
                        CurrentItem = new EcnDataCheckItem() { EcnWtPart = new WindchillObjectWtPart() { Number = MissingNumber.Number, Revision = "?" } };
                        CurrentItem.PartMissingCheck = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg71", new string[1] { MissingNumber.Number }).Status;
                        ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).MissingWtPartInEcnList.Add(CurrentItem);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckAllPartEcnLinkAsync()
        {
            try
            {
                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList)
                    CheckOnePartEcnLink(CurrentItem);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckOnePartEcnLink(EcnDataCheckItem CurrentItem)
        {
            try
            {
                DataCheckRule CurrentDataCheckRule = null;
                string StringEcnList = null;

                CurrentDataCheckRule = GetDataCheckRule("PartEcnSeveral");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    if (CurrentItem.EcnWtPart.AllEca != null && CurrentItem.EcnWtPart.AllEca.Count() > 1)
                    {
                        foreach (WindchillObject eca in CurrentItem.EcnWtPart.AllEca)
                            if (eca.State.ToUpper() == EcnDataCheckConstants.EcnResolvedState)
                                StringEcnList = String.Concat(StringEcnList, eca.Number, "*, ");
                            else
                                StringEcnList = String.Concat(StringEcnList, eca.Number, ", ");

                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg29", new string[1] { StringEcnList });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckAllMissingCadDocInEcnAsync()
        {
            try
            {
                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList)
                    CheckOneMissingCadDocInEcn(CurrentItem);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckOneMissingCadDocInEcn(EcnDataCheckItem CurrentItem)
        {
            try
            {
                DataCheckRule CurrentDataCheckRule = null;

                CurrentDataCheckRule = GetDataCheckRule("CadDocEcnMissing");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    // Concat all EpmDoc List from all ECA
                    WindchillChangeNotice CurrentWindchillChangeNotice = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice;
                    List<WindchillObject> AllEpmDoc = new List<WindchillObject>();
                    foreach (WindchillChangeActivity CurrentEca in CurrentWindchillChangeNotice.ListEca)
                        AllEpmDoc.AddRange(CurrentEca.ListEpmDocument);

                    // create missing EpmDoc list
                    List<WindchillObjectEpmDocument> MissingEpmDocList = CurrentItem.ListSearchedEpmDocument.
                                                FindAll((epm) => !AllEpmDoc.Exists((epmEcn) => epmEcn.Equals(epm)));

                    foreach (WindchillObjectEpmDocument CurrentEpmDoc in MissingEpmDocList)
                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg30", null, CurrentEpmDoc);
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckAllExtraCadDocInEcnAsync()
        {
            try
            {
                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList.Where((item) => item.EcnWtPart.Revision != "?"))
                    CheckExtraCadDocInEcnAsync(CurrentItem);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckExtraCadDocInEcnAsync(EcnDataCheckItem CurrentItem)
        {
            try
            {
                DataCheckRule CurrentDataCheckRule = null;

                CurrentDataCheckRule = GetDataCheckRule("CadDocEcnRevision");

                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    // create extra EpmDoc list
                    WindchillChangeNotice CurrentWindchillChangeNotice = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice;
                    List<WindchillObjectEpmDocument> ExtraEpmDocList = CurrentItem.ListEpmDocument.Where((epm) => epm.CheckedObject.ExtractedNumber == CurrentItem.EcnWtPart.Number && epm.Revision != CurrentItem.EcnWtPart.Revision).ToList();

                    foreach (WindchillObjectEpmDocument CurrentEpmDoc in ExtraEpmDocList)
                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg31", null, CurrentEpmDoc);
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckAllPartsWTDocLinkAsync()
        {
            try
            {
                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList.Where((item) => item.EcnWtPart.Revision != "?"))
                    CheckOnePartWTDocLink(CurrentItem);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckOnePartWTDocLink(EcnDataCheckItem CurrentItem)
        {
            try
            {
                DataCheckRule CurrentDataCheckRule = null;

                foreach (WindchillObject CurrentWtDoc in CurrentItem.LinkWtPartWtDocumentDescribe.Select((item) => item.LinkedObject))
                {
                    // Wtpart and WtDoc with same Number
                    if (CurrentItem.EcnWtPart.Number.IndexOf(CurrentWtDoc.Number) == 0)
                    {

                        // Check if WTDocument with same number than Part has the same revision: Issue if not
                        CurrentDataCheckRule = GetDataCheckRule("LinkPartWtDocRevision");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                        {
                            if (CurrentItem.EcnWtPart.Revision != CurrentWtDoc.Revision)
                                CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg32", null, CurrentWtDoc);
                        }

                        // Check if WTDocument is in the same Context that the part: Warning if not 
                        CurrentDataCheckRule = GetDataCheckRule("LinkPartWtDocContext");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                        {
                            if (CurrentItem.EcnWtPart.Context.Name != CurrentWtDoc.Context.Name)
                                CurrentItem.ContextStatus = CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg33", null, CurrentWtDoc).Status;
                        }
                    }

                    // Check if WTDocument as "Tif Plan" type has same part number : Warning if not
                    CurrentDataCheckRule = GetDataCheckRule("LinkPartWtDocTif");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {
                        if (CurrentItem.EcnWtPart.Number.IndexOf(CurrentWtDoc.Number) != 0 && CurrentWtDoc.DisplayType.ToUpper() == EcnDataCheckConstants.WtDocumentTypePlanTif.ToUpper())
                            CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg34", null, CurrentWtDoc);
                    }

                    //  Check if WTDocument linked to the Part is in the ECN : Issue if not
                    CurrentDataCheckRule = GetDataCheckRule("WtDocEcnMissing");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {
                        // Concat all WtDoc List from all ECA
                        WindchillChangeNotice CurrentWindchillChangeNotice = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice;
                        List<WindchillObject> AllWtDoc = new List<WindchillObject>();
                        foreach (WindchillChangeActivity CurrentEca in CurrentWindchillChangeNotice.ListEca)
                            AllWtDoc.AddRange(CurrentEca.ListWtDocument);

                        if (!AllWtDoc.Exists((wtDoc) => wtDoc.Number == CurrentWtDoc.Number && wtDoc.Revision == CurrentWtDoc.Revision))
                            CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg36", null, CurrentWtDoc);
                    }

                    // Check if WTDocument has a content: filename should not be null or empty
                    CurrentDataCheckRule = GetDataCheckRule("WtDocumentMissingContent");
                    if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                    {
                        if (string.IsNullOrEmpty(CurrentWtDoc.FileName))
                            CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg70", new string[1] { CurrentWtDoc.Number }, CurrentWtDoc);
                    }
                }

                // Check if WtDocument is linked to the part
                CurrentDataCheckRule = GetDataCheckRule("LinkPartWtDocMissing");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    // var doc = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice.ListWtDocument.FirstOrDefault(item => item.CheckedObject.ExtractedNumber == CurrentItem.EcnWtPart.Number);

                    var allDocWithSameNumber = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice.ListWtDocument.Where(item => item.CheckedObject.ExtractedNumber == CurrentItem.EcnWtPart.Number).ToList();
                    if (allDocWithSameNumber != null || allDocWithSameNumber.Any())
                    {
                        foreach (var docToCheck in allDocWithSameNumber)
                        {
                            var doc = CurrentItem.LinkWtPartWtDocumentDescribe.FirstOrDefault(item => item.LinkedObject.Number == docToCheck.Number);

                            if (doc == null)
                            {
                                CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg72", new string[1] { docToCheck.Number }, docToCheck);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckAllUncheckedEpmDocAttributes()
        {
            try
            {
                WindchillChangeNotice CurrentWindchillChangeNotice = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice;
                EcnDataCheckItem CurrentItem;
                foreach (WindchillObjectEpmDocument CurrentEpm in CurrentWindchillChangeNotice.ListEpmDocument.Where((epm) => !epm.HasBeenchecked))
                {
                    CurrentItem = (EcnDataCheckItem)CurrentEcnDataCheckDataContext.DataCheckItemList.FirstOrDefault((item) => item.EcnWtPart.Number == CurrentEpm.CheckedObject.ExtractedNumber);
                    if (CurrentItem != null)
                        CheckOneCadDocAttributes(CurrentItem, CurrentEpm);
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckAllPartBomComponentAsync()
        {
            try
            {
                // Update nb total step 
                int NbAddStep = 0;
                Regex RegexState = new Regex(EcnDataCheckConstants.BomComponentStateNotApproved, RegexOptions.IgnoreCase);
                List<EcnDataCheckItem> ListItem = CurrentEcnDataCheckDataContext.DataCheckItemList.Where((item) => item.EcnWtPart.CheckedObject != null
                                                               && item.EcnWtPart.CheckedObject.NumberTemplate != null
                                                               && item.EcnWtPart.CheckedObject.NumberTemplate.FunctionalType == WindchillObjectType.PHYSICAL_PART).Select((item) => (EcnDataCheckItem)item).ToList();
                foreach (EcnDataCheckItem CurrentItem in ListItem)
                    NbAddStep += CurrentItem.PartStructure.Structure.Where((part) => RegexState.IsMatch(part.MainWindchillObject.State)).Count();

                CurrentEcnDataCheckDataContext.TotalStep += NbAddStep;

                ListPartComponentNotApproved.Clear();
                IndexBomCompCheck = 0;
                IndexMaxBomCompCheck = NbAddStep;
                foreach (EcnDataCheckItem CurrentItem in ListItem)
                    CheckOnePartBomComponentAsync(CurrentItem);

            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckOnePartBomComponentAsync(EcnDataCheckItem CurrentItem)
        {
            try
            {
                DataCheckRule CurrentDataCheckRule = null;
                if (CurrentItem.EcnWtPart.CheckedObject != null && CurrentItem.EcnWtPart.CheckedObject.NumberTemplate != null && CurrentItem.EcnWtPart.CheckedObject.NumberTemplate.FunctionalType == WindchillObjectType.PHYSICAL_PART)
                {

                    foreach (WindchillObjStructureComponent CurrentComponent in CurrentItem.PartStructure.Structure)
                    {
                        CurrentEcnDataCheckDataContext.ExtraStatusBarMsg = $"{McgWpfTools.GetStringResource("EDC_SbExtraMsg09")} {IndexBomCompCheck}/{IndexMaxBomCompCheck}";
                        // Check that there is only one REP value for one component
                        CurrentDataCheckRule = GetDataCheckRule("PartBomRepSeveral");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                            if (CurrentComponent.REP != null && CurrentComponent.REP.IndexOf('|') >= 0)
                                CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg41", new string[1] { CurrentComponent.MainWindchillObject.Number }, CurrentComponent.MainWindchillObject, "EDC_Link_COMPONENT");

                        // Check that REP value is not too long, 4 digit maximum for SAP
                        CurrentDataCheckRule = GetDataCheckRule("PartBomRepTooLong");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                            if (CurrentComponent.REP != null && CurrentComponent.REP.Length > CurrentEcnDataCheckConfiguration.NumericalLineMaxNumberDigit && CurrentComponent.REP.IndexOf('|') < 0)
                                CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg44", new string[3] { CurrentComponent.MainWindchillObject.Number, CurrentComponent.REP, CurrentEcnDataCheckConfiguration.NumericalLineMaxNumberDigit.ToString() }, CurrentComponent.MainWindchillObject, "EDC_Link_COMPONENT");

                        // Check that component is not a obsolete fastener: should provide the new fastener
                        CurrentDataCheckRule = GetDataCheckRule("PartBomObsoleteComponent");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                        {
                            DataCheckValue CurrentDataCheckValue = IsComponentObsolete(CurrentComponent);
                            if (CurrentDataCheckValue == DataCheckValue.NOTACCURATE)
                                CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg59", new string[2] { CurrentComponent.MainWindchillObject.Number, CurrentComponent.ReplacementNumber }, CurrentComponent.MainWindchillObject, "EDC_Link_COMPONENT");
                        }

                        // Check that component in "In Work" or "Rework" are part of an ECN
                        CurrentDataCheckRule = GetDataCheckRule("PartBomComponentInWork");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                        {
                            Regex RegexState = new Regex(EcnDataCheckConstants.BomComponentStateNotApproved, RegexOptions.IgnoreCase);
                            WindchillChangeNotice CurrentWindchillChangeNotice = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice;
                            if (RegexState.IsMatch(CurrentComponent.MainWindchillObject.State))
                            {
                                // Check if Component in the current ECN
                                if (!CurrentWindchillChangeNotice.ListEpmDocument.Exists((part) => part.Equals(CurrentComponent.MainWindchillObject)))
                                {
                                    // Check if Part in another ECN
                                    WindchillObjectWtPart CurrentPart = ListPartComponentNotApproved.FirstOrDefault((part) => part.Equals(CurrentComponent.MainWindchillObject));
                                    if (CurrentPart == null)
                                    {
                                        CurrentPart = new WindchillObjectWtPart();
                                        CurrentPart.Revision = CurrentComponent.MainWindchillObject.Revision;
                                        CurrentPart.Number = CurrentComponent.MainWindchillObject.Number;
                                        //List<RestOdataWtPart> TempList = SearchOnePartEcnLink(CurrentPart);
                                        List<WindchillObjectWtPart> TempList = SearchOnePartEcnLink(CurrentPart);
                                        if (TempList != null && TempList.Count() > 0)
                                            CurrentPart.EcnNumber = TempList.ElementAt(0).EcnNumber;
                                        else
                                            CurrentPart.EcnNumber = "NONE";
                                        ListPartComponentNotApproved.Add(CurrentPart);
                                    }

                                    if (CurrentPart.EcnNumber == "NONE")
                                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg42", new string[2] { CurrentComponent.MainWindchillObject.Number, CurrentComponent.MainWindchillObject.State }, CurrentComponent.MainWindchillObject, "EDC_Link_COMPONENT");

                                }
                                CurrentEcnDataCheckDataContext.CurrentStep++;
                                IndexBomCompCheck++;
                            }
                        }
                    }

                    foreach (WindchillObjStructureComponent CurrentComponent in CurrentItem.PartStructure.RawAllComponentStructure)
                    {
                        // Check that REP value is not blank
                        CurrentDataCheckRule = GetDataCheckRule("PartBomRepBlank");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                            if (CurrentComponent.REP == null || CurrentComponent.REP.Trim() == "")
                                CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg45", new string[1] { CurrentComponent.MainWindchillObject.Number }, CurrentComponent.MainWindchillObject, "EDC_Link_COMPONENT");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private EcnDataCheckResultItem CreateResultItem(EcnDataCheckItem CurrentItem, DataCheckRule CurrentDataCheckRule, string KeyStringResource, string[] ParamString = null, WindchillObject LinkedObj = null, string KeyString = null)
        {
            try
            {
                // create new CheckResult Issue Item
                EcnDataCheckResultItem CurrentResultItem = new EcnDataCheckResultItem()
                {
                    ParentEcnDataCheckItem = CurrentItem,
                    Status = CurrentDataCheckRule.GetDataCheckStatus(),
                    KeyStringResource = KeyStringResource,
                    ParamString = ParamString,
                    LinkedObj = LinkedObj,
                    KeyString = KeyString,
                    CurrentDataCheckItem = CurrentItem,
                    IssueDocumentationPath = McgMiscTools.GetSharePointDocument(McgWpfTools.GetStringResource(CurrentDataCheckRule.Document)),
                    IssueDocumentation = McgWpfTools.GetStringResource(CurrentDataCheckRule.Document)
                };
                if (ParamString == null || ParamString.Count() == 0)
                    CurrentResultItem.Comments = McgWpfTools.GetStringResource(KeyStringResource);
                else
                    CurrentResultItem.Comments = string.Format(McgWpfTools.GetStringResource(KeyStringResource), ParamString);

                // If an object is linked to the part, update properties regarding the link
                if (LinkedObj != null)
                {
                    CurrentResultItem.LinkedObjNumber = LinkedObj.Number;
                    CurrentResultItem.LinkedObjRevision = LinkedObj.Revision;
                    if (KeyString == null)
                        CurrentResultItem.CurrentLink = McgWpfTools.GetStringResource($"EDC_Link_{GetCurrentLinkWithWindchillObject(CurrentItem, LinkedObj).ToString()}");
                    else
                        CurrentResultItem.CurrentLink = McgWpfTools.GetStringResource(KeyString);
                }

                CurrentItem.ListDataCheckResult.Add(CurrentResultItem);
                CurrentItem.IsResultItem = true;
                UpdateDataCheckStatus(CurrentItem, CurrentResultItem.Status);

                // MainDispatcher.Invoke(new Action(UpdateCurrentResultItem(CurrentItem, CurrentResultItem));
                MainDispatcher.Invoke(new Action(() => UpdateCurrentResultItem(CurrentItem, CurrentResultItem)));

                return CurrentResultItem;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateCurrentResultItem(EcnDataCheckItem CurrentItem, EcnDataCheckResultItem CurrentResultItem)
        {
            try
            {
                CurrentItem.ListDataCheckResultShown.Add(CurrentResultItem);
            }
            catch (Exception ex)
            {
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private WindchillObjectLinkType GetCurrentLinkWithWindchillObject(EcnDataCheckItem CurrentItem, WindchillObject LinkedObj)
        {
            try
            {
                WindchillObjectLink CurrentObjLink = null;

                if (LinkedObj.GetType() == typeof(WindchillObjectEpmDocument))
                {
                    CurrentObjLink = CurrentItem.LinkWtPartEpmDocumentDescribe.FirstOrDefault((i) => i.LinkedObject.Equals(LinkedObj));
                    if (CurrentObjLink != null)
                        return CurrentObjLink.LinkType;
                    CurrentObjLink = CurrentItem.LinkWtPartEpmDocumentOwner.FirstOrDefault((i) => i.LinkedObject.Equals(LinkedObj));
                    if (CurrentObjLink != null)
                        return CurrentObjLink.LinkType;
                    else
                        return WindchillObjectLinkType.NO_LINK;
                }
                else if (LinkedObj.GetType() == typeof(WindchillObjectWtDocument))
                {
                    CurrentObjLink = CurrentItem.LinkWtPartWtDocumentDescribe.FirstOrDefault((i) => i.LinkedObject.Equals(LinkedObj));
                    if (CurrentObjLink != null)
                        return CurrentObjLink.LinkType;
                    CurrentObjLink = CurrentItem.LinkWtPartWtDocumentReference.FirstOrDefault((i) => i.LinkedObject.Equals(LinkedObj));
                    if (CurrentObjLink != null)
                        return CurrentObjLink.LinkType;
                    else
                        return WindchillObjectLinkType.NO_LINK;
                }

                return WindchillObjectLinkType.UNKNOWN;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Sub check methods for Part Attributes
        private DataCheckValue IsAccurateDesc1Eng(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.EcnWtPart.TemplateWebterm == null || CurrentItem.EcnWtPart.LocalLanguage.SAPCode == "WRONGTERM")
                    return DataCheckValue.NOTACCURATE;
                else
                    return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsAccurateDesc1Local(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (IsAccurateDesc1Eng(CurrentItem) == DataCheckValue.OK && CurrentItem.EcnWtPart.LocalLanguage.SAPCode == "NOTFOUND")
                    return DataCheckValue.NOTACCURATE;
                else
                    return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsAccurateDesc2Eng(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.EcnWtPart.DescriptionEn2 == null)
                    CurrentItem.EcnWtPart.DescriptionEn2 = "";
                string desc2Eng = CurrentItem.EcnWtPart.DescriptionEn2.Trim();
                if (desc2Eng == "" || desc2Eng == "-")
                    return DataCheckValue.UNDEFINED;
                if (desc2Eng.Length > EcnDataCheckConstants.MaxLenghtSapDetailDesc)
                    return DataCheckValue.HIGH;
                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsAccurateDesc2Local(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.EcnWtPart.DescriptionLocal2 == null)
                    CurrentItem.EcnWtPart.DescriptionLocal2 = "";
                string Desc2Local = CurrentItem.EcnWtPart.DescriptionLocal2.Trim();
                if (CurrentItem.EcnWtPart.LocalLanguage.SAPCode != "EN")
                {
                    if (Desc2Local == "" || Desc2Local == "-")
                        return DataCheckValue.UNDEFINED;
                    if (Desc2Local.Length > EcnDataCheckConstants.MaxLenghtSapDetailDesc)
                        return DataCheckValue.HIGH;
                    return DataCheckValue.OK;
                }
                else
                {
                    // if English for local language, description2local should be empty
                    if (Desc2Local != "" && Desc2Local != "-")
                        return DataCheckValue.NOTACCURATE;
                    return DataCheckValue.OK;
                }

            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsSameLocalLanguage(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (CurrentEcnDataCheckDataContext.SelectedLanguage.SAPCode != CurrentItem.EcnWtPart.LocalLanguage.SAPCode && CurrentItem.EcnWtPart.LocalLanguage.SAPCode != "WRONGTERM" && CurrentItem.EcnWtPart.LocalLanguage.SAPCode != "NOTFOUND")
                    return DataCheckValue.WARNING;
                else
                    return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsAccurateGroupCreator(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.EcnWtPart.GroupCreator == null)
                    CurrentItem.EcnWtPart.GroupCreator = "";
                string GroupCreator = CurrentItem.EcnWtPart.GroupCreator.Trim();
                Regex RegExGc = new Regex("^[A-Z]{3}$");

                if (GroupCreator == "")
                    return DataCheckValue.UNDEFINED;

                if (!RegExGc.IsMatch(GroupCreator))
                    return DataCheckValue.NOTACCURATE;

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsAccurateQualInspGrp(EcnDataCheckItem CurrentItem)
        {
            try
            {
                char QualInspLimit = 'C';
                if (CurrentItem.EcnWtPart.TemplateWebterm != null)
                    QualInspLimit = CurrentItem.EcnWtPart.TemplateWebterm.Qualinspgrplimit.First();

                if (CurrentItem.EcnWtPart.QualInspGrp == null)
                    CurrentItem.EcnWtPart.QualInspGrp = "";
                string QualInspGrp = CurrentItem.EcnWtPart.QualInspGrp.Trim();
                if (QualInspGrp == "")
                    return DataCheckValue.UNDEFINED;

                if (QualInspGrp == "X")
                    return DataCheckValue.WARNING;

                if (QualInspGrp.First() > QualInspLimit)
                    return DataCheckValue.LOW;

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsAccurateMass(EcnDataCheckItem CurrentItem)
        {
            try
            {
                double MassLowLimit = 0;
                double MassHighLimit = 200000;
                if (CurrentItem.EcnWtPart.TemplateWebterm != null)
                {
                    MassLowLimit = CurrentItem.EcnWtPart.TemplateWebterm.Masslowlimit.Value;
                    MassHighLimit = CurrentItem.EcnWtPart.TemplateWebterm.Masshighlimit.Value;
                }

                if (CurrentItem.EcnWtPart.Mass == 0)
                    return DataCheckValue.UNDEFINED;

                if (CurrentItem.EcnWtPart.Mass < MassLowLimit)
                    return DataCheckValue.LOW;

                if (CurrentItem.EcnWtPart.Mass > MassHighLimit)
                    return DataCheckValue.HIGH;

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsAccurateDefaultUnit(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.EcnWtPart.TemplateWebterm == null || CurrentItem.EcnWtPart.TemplateWebterm.Defaultunit == null || CurrentItem.EcnWtPart.TemplateWebterm.Defaultunit.Trim() == "")
                    return DataCheckValue.UNDEFINED;

                if (McgBusinessTools.GetBomUnit(CurrentItem.EcnWtPart.TemplateWebterm.Defaultunit) != McgBusinessTools.GetBomUnit(CurrentItem.EcnWtPart.DefaultUnit))
                    return DataCheckValue.NOTACCURATE;

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsMaterialAssigned(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.EcnWtPart.Material == null || CurrentItem.EcnWtPart.Material.Trim() == "")
                    return DataCheckValue.UNDEFINED;

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsMaterialDefaultAssigned(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.EcnWtPart.Material != null && CurrentItem.EcnWtPart.Material.ToUpper().Contains("UNDEFINED"))
                    return DataCheckValue.WARNING;

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsComponentObsolete(WindchillObjStructureComponent CurrentComponent)
        {
            try
            {
                CurrentComponent.ReplacementNumber = _mcgQuickChangeTools.GetNewPartNumber(CurrentComponent.MainWindchillObject.Number);

                if (CurrentComponent.ReplacementNumber != null)
                    return DataCheckValue.NOTACCURATE;

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsBrandAssigned(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentItem.EcnWtPart.Brand) && CurrentItem.EcnWtPart.Brand.Trim() != "-")
                {
                    List<string> termes = GetBrandWebterm(CurrentItem.EcnWtPart.Brand);
                    if (termes == null || !termes.Any())
                        return DataCheckValue.NOTACCURATE;
                }
                else
                {
                    if (GetAllBrandGroupSubGroupdWebterm().Contains(CurrentItem.EcnWtPart.Name))
                        return DataCheckValue.UNDEFINED;
                }

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsGroupAssigned(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentItem.EcnWtPart.Brand) && CurrentItem.EcnWtPart.Brand.Trim() != "-")
                {

                    if ((string.IsNullOrEmpty(CurrentItem.EcnWtPart.Group) || CurrentItem.EcnWtPart.Group.Trim() == "-") && GetAllWebterm().Contains(CurrentItem.EcnWtPart.Name))
                        return DataCheckValue.UNDEFINED;

                    List<string> brand = GetAccuratBrand(CurrentItem.EcnWtPart.Group);
                    //if (string.IsNullOrEmpty(CurrentItem.EcnWtPart.Group) || CurrentItem.EcnWtPart.Group == "-")
                    //    return DataCheckValue.UNDEFINED;
                    if ((brand == null || brand.Count == 0) && !(string.IsNullOrEmpty(CurrentItem.EcnWtPart.Group) || CurrentItem.EcnWtPart.Group == "-"))
                        return DataCheckValue.NOTFOUND;
                    else if (!brand.Contains(CurrentItem.EcnWtPart.Brand) && !(string.IsNullOrEmpty(CurrentItem.EcnWtPart.Group) || CurrentItem.EcnWtPart.Group == "-"))
                        return DataCheckValue.NOTACCURATE;
                    // check if the term could have classification
                }

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsGroupTermAccurate(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentItem.EcnWtPart.Group) && CurrentItem.EcnWtPart.Group.Trim() != "-")
                {
                    List<string> termes = GetBrandGroupWebterm(CurrentItem.EcnWtPart.Brand, CurrentItem.EcnWtPart.Group);
                    if (!termes.Contains(CurrentItem.EcnWtPart.Name))
                        return DataCheckValue.NOTACCURATE;
                }

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsSubGroupAssigned(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentItem.EcnWtPart.Group) && CurrentItem.EcnWtPart.Group.Trim() != "-")
                {
                    List<string> group = GetAccuratGroup(CurrentItem.EcnWtPart.SubGroup);
                    if (string.IsNullOrEmpty(CurrentItem.EcnWtPart.SubGroup) || CurrentItem.EcnWtPart.SubGroup == "-")
                        return DataCheckValue.UNDEFINED;
                    else if (group == null || group.Count == 0)
                        return DataCheckValue.NOTFOUND;
                    else if (!group.Contains(CurrentItem.EcnWtPart.Group))
                        return DataCheckValue.NOTACCURATE;
                }

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsOptionAccurate(EcnDataCheckItem CurrentItem)
        {
            try
            {
                var options = GetAccuratOptions(CurrentItem.EcnWtPart.Brand, CurrentItem.EcnWtPart.Group, CurrentItem.EcnWtPart.SubGroup);
                if (!string.IsNullOrEmpty(CurrentItem.EcnWtPart.Option) && CurrentItem.EcnWtPart.Option.Trim() != "-")
                {
                    if (options != null)
                        if (!options.Contains(CurrentItem.EcnWtPart.Option))
                            return DataCheckValue.NOTACCURATE;
                }

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsSubGroupTermAccurate(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentItem.EcnWtPart.SubGroup) && CurrentItem.EcnWtPart.SubGroup.Trim() != "-")
                {
                    List<string> termes = GetBrandGroupSubGroupWebterm(CurrentItem.EcnWtPart.Brand, CurrentItem.EcnWtPart.Group, CurrentItem.EcnWtPart.SubGroup);
                    if (!termes.Contains(CurrentItem.EcnWtPart.Name))
                        return DataCheckValue.NOTACCURATE;
                }

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsRevisionHigherThanInSap(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentItem.EcnWtPart.Number))
                {
                    var listResultMaterial = _sapHupService.GetListMaterialMasterRevision(CurrentItem.EcnWtPart.Number);
                    //List<SAPMaterialMaster> listPart = SAPTools.GetMaterialMasterRevision(CurrentItem.EcnWtPart.Number);

                    if (listResultMaterial != null && listResultMaterial.Count > 0)
                    {
                        List<string> revisions = listResultMaterial.Select(p => p.REVISION).ToList();
                        string latestRevision = McgBusinessTools.GetLatestRevisionEnum(revisions);
                        CurrentItem.EcnWtPart.ErpRevision = latestRevision;
                        string windchillRevision = CurrentItem.EcnWtPart.Revision;
                        string sapRevision = latestRevision;

                        // Nouveau test : si EcnNumber et Revision correspondent à un élément de listPart
                        if (listResultMaterial.Any(p => p.ECONUMBER == CurrentEcnDataCheckDataContext.EcnNumber && p.REVISION == windchillRevision))
                        {
                            return DataCheckValue.OK;
                        }

                        // Comparaison des révisions
                        int compareResult = McgBusinessTools.CompareRevision(windchillRevision, sapRevision);

                        if (compareResult > 0)
                            return DataCheckValue.OK; // Windchill > SAP
                        else if (compareResult == 0)
                            return DataCheckValue.EQUAL; // Windchill == SAP
                        else
                            return DataCheckValue.LOW; // Windchill < SAP
                    }
                }

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Sub check methods for EpmDocument Attributes
        private DataCheckValue IsSameNumberFileName(WindchillObjectEpmDocument CurrentEpm)
        {
            try
            {
                if (CurrentEpm.FileName == null || CurrentEpm.Number.ToUpper() != CurrentEpm.FileName.ToUpper())
                    return DataCheckValue.NOTACCURATE;
                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsNumberHasAccurateExtension(WindchillObjectEpmDocument CurrentEpm)
        {
            try
            {
                if (CurrentEpm.Number.IndexOf(".") > 0)
                {
                    if (CurrentEpm.FileName == null || CurrentEpm.Number.Split('.')[1].ToUpper() != CurrentEpm.FileName.Split('.')[1].ToUpper())
                        return DataCheckValue.NOTACCURATE;
                }
                else
                    return DataCheckValue.UNDEFINED;

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsRepImplemented(WindchillObjectEpmDocument CurrentEpm)
        {
            try
            {
                if (CurrentEpm.Type == null || CurrentEpm.Type.Trim() == "")
                    return DataCheckValue.UNDEFINED;
                else if (CurrentEpm.Type.Trim() == "***NO REP***" || CurrentEpm.Type.Trim() == "*** NO REP ***")
                    return DataCheckValue.NOTACCURATE;

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckValue IsDrwErlierThan3D(EcnDataCheckItem CurrentItem, WindchillObjectEpmDocument CurrentEpm)
        {
            try
            {
                List<WindchillObjectEpmDocument> TempList = CurrentItem.ListEpmDocument.Where((cad) => (cad.CheckedObject.SubType == WindchillObjectType.ASM || cad.CheckedObject.SubType == WindchillObjectType.PRT) && cad.ModifiedOn > CurrentEpm.ModifiedOn).ToList();

                if (TempList.Count > 0)
                {
                    CurrentItem.TempMessage = "(";
                    foreach (var cad in TempList)
                        CurrentItem.TempMessage = $"{CurrentItem.TempMessage}{cad.Number};";
                    CurrentItem.TempMessage = $"{CurrentItem.TempMessage})";
                    return DataCheckValue.NOTACCURATE;
                }

                return DataCheckValue.OK;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Extract Ecn Information
        private void ExtractMainEcnInformationAsynch()
        {
            try
            {
                // Extract Main ECN Information
                //WindchillChangeNotice CurrentWindchillChangeNotice2 = WindchillRestOdataTool.GetWindchillChangeNoticeFull(WindchillNetworkCredential.WindchillCredential, CurrentEcnDataCheckDataContext.EcnNumber, CurrentEcnDataCheckDataContext.EcaNumber.Number);
                WindchillChangeNotice CurrentWindchillChangeNotice = _windchillRequestTool.GetQueryBuilderFullEcn(WindchillNetworkCredential.WindchillCredential, CurrentEcnDataCheckDataContext.EcnNumber, CurrentEcnDataCheckDataContext.EcaNumber.Number);

                ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice = CurrentWindchillChangeNotice;

                // Search Naming Convention Template for all objects and WebTerm Information
                List<WindchillObject> AllObj = new List<WindchillObject>();
                AllObj.AddRange(CurrentWindchillChangeNotice.ListWtPart);
                AllObj.AddRange(CurrentWindchillChangeNotice.ListEpmDocument);
                AllObj.AddRange(CurrentWindchillChangeNotice.ListWtDocument);
                AllObj.AddRange(CurrentWindchillChangeNotice.LinkWtPartEpmDocumentDescribe.Select((L) => L.LinkedObject));
                AllObj.AddRange(CurrentWindchillChangeNotice.LinkWtPartEpmDocumentOwner.Select((L) => L.LinkedObject));
                AllObj.AddRange(CurrentWindchillChangeNotice.LinkWtPartWtDocumentDescribe.Select((L) => L.LinkedObject));
                AllObj.AddRange(CurrentWindchillChangeNotice.LinkWtPartWtDocumentReference.Select((L) => L.LinkedObject));
                AllObj.AddRange(CurrentWindchillChangeNotice.LinkWtPartEpmDocumentDescribe.Select((L) => L.MainObject));
                AllObj.AddRange(CurrentWindchillChangeNotice.LinkWtPartEpmDocumentOwner.Select((L) => L.MainObject));
                AllObj.AddRange(CurrentWindchillChangeNotice.LinkWtPartWtDocumentDescribe.Select((L) => L.MainObject));
                AllObj.AddRange(CurrentWindchillChangeNotice.LinkWtPartWtDocumentReference.Select((L) => L.MainObject));

                GetWebtermList();
                foreach (WindchillObject CurrentObj in AllObj)
                    GetOneWindchillObjectInformation(CurrentObj);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void ExtractOneEcnDataCheckItemInformation(EcnDataCheckItem CurrentEcnDataCheckItem)
        {
            try
            {
                GetOneWindchillObjectInformation(CurrentEcnDataCheckItem.EcnWtPart);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateMainEcnInformationMainThread()
        {
            try
            {
                WindchillChangeNotice CurrentWindchillChangeNotice = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice;

                EcnDataCheckItem CurrentEcnDataCheckItem = null;
                foreach (WindchillObjectWtPart CurrentWtPart in ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice.ListWtPart.OrderBy((item) => item.Number))
                {
                    CurrentEcnDataCheckItem = new EcnDataCheckItem();
                    UpdateMainOneDataItemInformation(CurrentEcnDataCheckItem, CurrentWtPart);
                    CurrentEcnDataCheckDataContext.DataCheckItemList.Add(CurrentEcnDataCheckItem);
                }
                // End CODE TEST
            }
            catch (Exception ex)
            {
                ResetInterface();
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateMainOneDataItemInformation(EcnDataCheckItem CurrentEcnDataCheckItem, WindchillObjectWtPart CurrentWtPart)
        {
            try
            {
                WindchillChangeNotice CurrentWindchillChangeNotice = ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice;

                if (CurrentEcnDataCheckItem == null)
                    CurrentEcnDataCheckItem = new EcnDataCheckItem();

                CurrentEcnDataCheckItem.IsErpBomComparison = false;
                CurrentEcnDataCheckItem.IsPdmBomComparison = false;
                CurrentEcnDataCheckItem.PartMissingCheck = DataCheckStatus.OK;
                CurrentEcnDataCheckItem.EcnWtPart = CurrentWtPart;

                if (CurrentWtPart == null)
                    return;

                CurrentEcnDataCheckItem.NewContextName = CurrentWtPart.Context.Name;
                CurrentEcnDataCheckItem.NewFolderName = CurrentWtPart.Folder;
                CurrentEcnDataCheckItem.WindchillContextList = CurrentEcnDataCheckDataContext.WindchillContextList;

                // update linked object list
                CurrentEcnDataCheckItem.LinkWtPartEpmDocumentOwner = (CurrentWindchillChangeNotice.LinkWtPartEpmDocumentOwner.FindAll((i) => i.MainObject.Number == CurrentWtPart.Number && i.MainObject.Revision == CurrentWtPart.Revision));
                CurrentEcnDataCheckItem.LinkWtPartEpmDocumentDescribe = (CurrentWindchillChangeNotice.LinkWtPartEpmDocumentDescribe.FindAll((i) => i.MainObject.Number == CurrentWtPart.Number && i.MainObject.Revision == CurrentWtPart.Revision));
                CurrentEcnDataCheckItem.LinkWtPartWtDocumentDescribe = (CurrentWindchillChangeNotice.LinkWtPartWtDocumentDescribe.FindAll((i) => i.MainObject.Number == CurrentWtPart.Number && i.MainObject.Revision == CurrentWtPart.Revision));
                CurrentEcnDataCheckItem.LinkWtPartWtDocumentReference = (CurrentWindchillChangeNotice.LinkWtPartWtDocumentReference.FindAll((i) => i.MainObject.Number == CurrentWtPart.Number && i.MainObject.Revision == CurrentWtPart.Revision));

                // update list EpmDoc and WtDoc list whith same base number
                if (CurrentWtPart.CheckedObject != null)
                {
                    CurrentEcnDataCheckItem.ListEpmDocument = (CurrentWindchillChangeNotice.ListEpmDocument.FindAll((i) => i.CheckedObject.ExtractedNumber == CurrentWtPart.CheckedObject.ExtractedNumber));
                    CurrentEcnDataCheckItem.ListWtDocument = (CurrentWindchillChangeNotice.ListWtDocument.FindAll((i) => i.CheckedObject.ExtractedNumber == CurrentWtPart.CheckedObject.ExtractedNumber));
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void GetOneWindchillObjectInformation(WindchillObject CurrentObj)
        {
            try
            {
                // Check number
                CurrentObj.CheckedObject = _windchillCheckNumberService.CheckObject(CurrentObj.Number, CurrentObj.ObjectType, NamingConvention);

                // Search WebTerm
                CurrentObj.TemplateWebterm = _webtermTools.GetWebterm(CurrentObj.Name, ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).AllWebtermList);
                // To manage case where English term and local term are the same (like OPTION in fr or de), use local language selected in priority to defined the local language of the item
                string TempLocalTerm = _webtermTools.GetTermFromEnglish(_webtermTools.GetWebtermLanguage(CurrentEcnDataCheckDataContext.SelectedLanguage), CurrentObj.TemplateWebterm);
                if (TempLocalTerm == CurrentObj.Name && TempLocalTerm == CurrentObj.DescriptionLocal1)
                    CurrentObj.LocalLanguage = CurrentEcnDataCheckDataContext.SelectedLanguage;
                else
                    CurrentObj.LocalLanguage = _webtermTools.GetMcgLanguage(CurrentObj.DescriptionLocal1, CurrentObj.TemplateWebterm);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void SearchAllCadDocAsync()
        {
            try
            {
                int index = 0;
                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList)
                {
                    CurrentEcnDataCheckDataContext.CurrentStep++;
                    index++;
                    CurrentEcnDataCheckDataContext.ExtraStatusBarMsg = string.Format(McgWpfTools.GetStringResource("EDC_SbExtraMsg05"), index, CurrentEcnDataCheckDataContext.DataCheckItemList.Count);

                    if (CurrentItem.EcnWtPart.CheckedObject != null && CurrentItem.EcnWtPart.CheckedObject.IsNumberAccurate)
                        SearchOneCadDoc(CurrentItem);
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void SearchOneCadDoc(EcnDataCheckItem CurrentItem)
        {
            try
            {
                string CurrentNumber = CurrentItem.EcnWtPart.Number;
                if (CurrentNumber != null && CurrentNumber.Trim() != "" && CurrentNumber.IndexOf('*') < 0)
                {
                    CurrentNumber = $"{CurrentNumber}*";
                    //List<WindchillObjectEpmDocument> TempList = WindchillRequestTool.WindchillRequestTool.GetQueryBuilderEpmList(WindchillNetworkCredential.WindchillCredential, CurrentNumber, CurrentItem.EcnWtPart.Revision);
                    List<WindchillObjectEpmDocument> TempList = _windchillRequestTool.GetQueryBuilderEpmListLite(WindchillNetworkCredential.WindchillCredential, CurrentNumber, CurrentItem.EcnWtPart.Revision);
                    //List<McgObjectNumber> NumberList = new List<McgObjectNumber>();
                    //NumberList.Add(new McgObjectNumber() { Number = CurrentNumber, Revision = CurrentItem.EcnWtPart.Revision });
                    //List<WindchillObjectEpmDocument> TempList = WindchillRestOdataTool.GetWindchillEpmDocumentList(WindchillNetworkCredential.WindchillCredential, NumberList, RestOdataEnumFilterType.STARTS_WITH);
                    Thread.Sleep(CommonLibConstants.RequestAwaitingTime);
                    // Check Naming Convention
                    foreach (WindchillObject CurrentObj in TempList)
                        GetOneWindchillObjectInformation(CurrentObj);

                    //Add only Cad if suffix is accurate, same revision, not in a excluded context and not in an excluded state
                    CurrentItem.ListSearchedEpmDocument = (TempList.
                                        FindAll((o) => o.CheckedObject.IsAccurate).
                                        FindAll((o) => o.CheckedObject.ExtractedNumber == CurrentItem.EcnWtPart.Number).
                                        FindAll((o) => !CurrentEcnDataCheckConfiguration.ListExcludedState.Exists((s) => o.State.ToUpper() == s.ToUpper())).
                                        FindAll((o) => !CurrentEcnDataCheckConfiguration.ListExcludedContext.Exists((c) => o.Context.Name == c.Name))
                                        ).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private List<WindchillObjectWtPart> SearchOnePartEcnLink(WindchillObjectWtPart CurrentPart)
        {
            try
            {
                string CurrentNumber = CurrentPart.Number;
                string CurrentRevision = CurrentPart.Revision;
                //List<RestOdataWtPart> TempList = null;
                List<WindchillObjectWtPart> TempList2 = null;
                if (CurrentNumber != null && CurrentNumber.Trim() != "" && CurrentNumber.IndexOf('*') < 0 && CurrentRevision != null && CurrentRevision.Trim() != "" && CurrentRevision.IndexOf('*') < 0)
                {
                    //TempList = WindchillRestOdataTool.GetQueryBuilderLinkPartEcnList<RestOdataWtPart>(WindchillNetworkCredential.WindchillCredential, CurrentNumber, CurrentRevision);
                    TempList2 = _windchillRequestTool.GetQueryBuilderLinkPartEcnList(WindchillNetworkCredential.WindchillCredential, CurrentNumber, CurrentRevision);
                    Thread.Sleep(CommonLibConstants.RequestAwaitingTime);

                }
                return TempList2;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void ExportEcnBomInformationAsynch(string EcnNumber)
        {
            try
            {
                bool UpdateDone = false;
                CurrentEcnDataCheckDataContext.DataCheckItemList.Clear();
                CurrentEcnDataCheckDataContext.DataCheckResultItemList.Clear();
                ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).MissingWtPartInEcnList.Clear();
                ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).OtherCheckItemList.Clear();
                CurrentEcnDataCheckDataContext.RenameItemList.Clear();
                CurrentEcnDataCheckDataContext.MoveItemList.Clear();
                ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).IsSapBomExtracted = false;
                ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).IsPdmBomExtracted = false;
                // Message ECN search in progress

                // Event subscribe and Thread to extract PDM BOM
                EventHandler PdmBomhandler = (s, e) =>
                {
                    new Thread(() =>
                    {
                        DownloadAllDataCheckItemBomAsync();
                        ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).IsPdmBomExtracted = true;
                        ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).RaiseEcnPdmBomExtractedEvent();
                    }).Start();
                };
                ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).EcnPartExtractedEvent += PdmBomhandler;

                // Event subscribe and Thread to extract SAP BOM
                EventHandler SapBomhandler = (s, e) =>
                {
                    new Thread(() =>
                    {
                        DownloadErpBom(false);
                        ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).IsSapBomExtracted = true;
                        ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).RaiseEcnSapBomExtractedEvent();
                    }).Start();
                };
                ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).EcnPartExtractedEvent += SapBomhandler;

                // Event subscribe and Thread to compare SAP/PDM BOM
                EventHandler SapPdmBomComparisonhandler = (s, e) =>
                {
                    new Thread(() =>
                    {
                        if (((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).IsSapBomExtracted && ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).IsPdmBomExtracted)
                        {
                            ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).EcnPartExtractedEvent -= PdmBomhandler;
                            ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).EcnPartExtractedEvent -= SapBomhandler;
                            ReadSapBom();
                            foreach (var item in CurrentEcnDataCheckDataContext.DataCheckItemList)
                                item.MetaDataStatus = DataCheckStatus.UNKNOWN;
                            MessageBox.Show(String.Format(McgWpfTools.GetStringResource("EDC_InfoMsgErpCompEnd"), CurrentEcnDataCheckDataContext.ErpSystem), McgWpfTools.GetStringResource("EDC_InfoTitleErpBom"), MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }).Start();
                };
                ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).EcnPdmBomExtractedEvent += SapPdmBomComparisonhandler;
                ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).EcnSapBomExtractedEvent += SapPdmBomComparisonhandler;

                // Thread to extract ECN parts
                new Thread(() =>
                {
                    //WindchillChangeNotice CurrentWindchillObjectEcn = WindchillRequestTool.WindchillRequestTool.GetQueryBuilderEcn(WindchillNetworkCredential.WindchillCredential, CurrentEcnDataCheckDataContext.EcnNumber);
                    CurrentEcnDataCheckDataContext.EcaNumber = new WindchillChangeActivity() { Number = "ALL" };
                    //WindchillChangeNotice CurrentWindchillChangeNotice = WindchillRequestTool.WindchillRequestTool.GetQueryBuilderFullEcn(WindchillNetworkCredential.WindchillCredential, CurrentEcnDataCheckDataContext.EcnNumber, CurrentEcnDataCheckDataContext.EcaNumber.Number);
                    WindchillChangeNotice CurrentWindchillChangeNotice = _windchillChangeManagementService.GetWindchillChangeNoticeFull(WindchillNetworkCredential.WindchillCredential, CurrentEcnDataCheckDataContext.EcnNumber, CurrentEcnDataCheckDataContext.EcaNumber.Number);
                    ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).CurrentWindchillChangeNotice = CurrentWindchillChangeNotice;
                    MainDispatcher.Invoke(new Action(() =>
                    {
                        UpdateMainEcnInformationMainThread();
                        foreach (var item in CurrentEcnDataCheckDataContext.DataCheckItemList)
                        {
                            item.EcnWtPart.CheckedObject = new WindchillCheckedObject() { NumberTemplate = new WindchillObjectNumberTemplate() { FunctionalType = WindchillObjectType.PHYSICAL_PART } };
                            item.MetaDataStatus = DataCheckStatus.UNKNOWN;
                        }
                        UpdateDone = true;
                    }));
                    while (!UpdateDone)
                        System.Threading.Thread.Sleep(100);
                    ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).RaiseEcnPartExtractedEvent();
                }).Start();


            }
            catch (Exception ex)
            {
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void SearchAndCheckAllPartRepresentationAsync()
        {
            try
            {
                int index = 0;
                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList)
                {
                    CurrentEcnDataCheckDataContext.CurrentStep++;
                    index++;
                    CurrentEcnDataCheckDataContext.ExtraStatusBarMsg = String.Format(McgWpfTools.GetStringResource("EDC_SbExtraMsg14"), index, CurrentEcnDataCheckDataContext.DataCheckItemList.Count);

                    if (CurrentItem.EcnWtPart.CheckedObject != null)
                        SearchAndCheckOnePartRepresentationAsync(CurrentItem);
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void SearchAndCheckOnePartRepresentationAsync(EcnDataCheckItem CurrentItem)
        {
            try
            {
                DataCheckRule CurrentDataCheckRule = GetDataCheckRule("PartRepresentationLoadedFromLegacy");
                if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                {
                    string Number = CurrentItem.EcnWtPart.Number;
                    string Revision = CurrentItem.EcnWtPart.Revision;
                    if (Number != null && Number.Trim() != "" && Number.IndexOf('*') < 0)
                    {
                        RestOdataWtPart CurrentPart = _windchillPartManagementService.GetOnePartRepresentation(WindchillNetworkCredential.WindchillCredential, Number, Revision);
                        Regex ReGexRepFrom = new Regex(EcnDataCheckConstants.RegExPartRepresentationLoadedFromLegacy, RegexOptions.IgnoreCase);
                        Regex ReGexPdf = new Regex($"{Number}_{Revision}.pdf", RegexOptions.IgnoreCase);
                        if (CurrentPart != null && CurrentPart.Representations != null)
                        {
                            foreach (var PartRep in CurrentPart.Representations.Where((item) => ReGexRepFrom.IsMatch(item.Description)))
                            {
                                if (PartRep != null && PartRep.AdditionalFiles != null)
                                {
                                    var PartRepFile = PartRep.AdditionalFiles.FirstOrDefault();
                                    if (PartRepFile != null && PartRepFile.FileName != null)
                                    {
                                        if (!ReGexPdf.IsMatch(PartRepFile.FileName))
                                        {
                                            CurrentItem.IsPartRepresentationFromLegacy = true;
                                            CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg54", new string[1] { PartRepFile.FileName });
                                        }
                                    }
                                    else
                                    {
                                        CurrentItem.IsPartRepresentationFromLegacy = true;
                                        CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg53");
                                    }
                                }
                                else
                                {
                                    CurrentItem.IsPartRepresentationFromLegacy = true;
                                    CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg53");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private List<string> GetAllBrandGroupSubGroupdWebterm()
        {
            try
            {
                List<string> allTerms = ListBrandGroupSubGroup.SelectMany(item => item.Terms).Distinct().ToList();
                return allTerms;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private List<string> GetBrandWebterm(string brand)
        {
            try
            {
                List<string> allTerms = ListBrandGroupSubGroup.Where(item => item.Brand == brand).SelectMany(item => item.Terms).ToList();
                return allTerms;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private List<string> GetBrandGroupWebterm(string brand, string group)
        {
            try
            {
                List<string> allTerms = ListBrandGroupSubGroup.Where(item => item.Brand == brand && item.Group == group).SelectMany(item => item.Terms).ToList();
                return allTerms;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private List<string> GetAllWebterm()
        {
            try
            {
                List<string> allTerms = ListBrandGroupSubGroup.SelectMany(item => item.Terms).Distinct().ToList();
                return allTerms;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private List<string> GetSubGroupWebterm(string subGroup)
        {
            try
            {
                List<string> allTerms = ListBrandGroupSubGroup.Where(item => item.SubGroup == subGroup).SelectMany(item => item.Terms).ToList();
                return allTerms;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private List<string> GetBrandGroupSubGroupWebterm(string brand, string group, string subGroup)
        {
            try
            {
                List<string> allTerms = ListBrandGroupSubGroup.Where(item => item.Brand == brand && item.Group == group && item.SubGroup == subGroup).SelectMany(item => item.Terms).ToList();
                return allTerms;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private List<string> GetAccuratGroup(string subGroup)
        {
            try
            {
                List<string> group = ListBrandGroupSubGroup.Where(item => item.SubGroup == subGroup)?.Select(item => item.Group).ToList();
                return group;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private List<string> GetAccuratBrand(string group)
        {
            try
            {
                List<string> brand = ListBrandGroupSubGroup.Where(item => item.Group == group)?.Select(item => item.Brand).ToList();
                return brand;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private List<string> GetAccuratOptions(string brand, string group, string subGroup)
        {
            try
            {
                List<string> options = ListBrandGroupSubGroup.FirstOrDefault(item => item.Brand == brand && item.Group == group && item.SubGroup == subGroup)?.OptionList;
                return options;
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods to extract and compare PDM BOM
        private void DownloadAllDataCheckItemBomAsync()
        {
            try
            {
                int index = 0;

                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList)
                {
                    CurrentEcnDataCheckDataContext.CurrentStep++;
                    index++;
                    CurrentEcnDataCheckDataContext.ExtraStatusBarMsg = String.Format(McgWpfTools.GetStringResource("EDC_SbExtraMsg08"), index, CurrentEcnDataCheckDataContext.DataCheckItemList.Count);
                    // Check if Physical part
                    if (CurrentItem.EcnWtPart.CheckedObject != null && CurrentItem.EcnWtPart.CheckedObject.NumberTemplate != null && CurrentItem.EcnWtPart.CheckedObject.NumberTemplate.FunctionalType == WindchillObjectType.PHYSICAL_PART)
                    {
                        DownloadOneDataCheckItemBomAsync(CurrentItem);
                        CompareOneDataCheckItemPdmBomAsync(CurrentItem);
                    }
                    else
                    {
                        CurrentItem.BomPdmComparisonStatus = DataCheckStatus.NONE;
                        CurrentItem.IsPdmBomComparison = false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void DownloadOneDataCheckItemBomAsync(EcnDataCheckItem CurrentItem)
        {
            try
            {
                if (CurrentItem.EcnWtPart.Revision != "?" && CurrentItem.EcnWtPart.Revision.Trim() != "")
                {
                    //CurrentItem.PartStructure = WindchillRequestTool.GetBomFirstLevelNamingConventionOneOccurence(CurrentItem.EcnWtPart.Number,
                    //                                                                                                        CurrentItem.EcnWtPart.Revision,
                    //                                                                                                        WindchillObjectInternalType.WTPart,
                    //                                                                                                        WindchillNetworkCredential.WindchillCredential,
                    //                                                                                                        NamingConvention,
                    //                                                                                                        true);
                    CurrentItem.PartStructure = _windchillBomManagementService.GetBomFirstLevelNamingConventionOneOccurence(CurrentItem.EcnWtPart.Number,
                                                                                                                            CurrentItem.EcnWtPart.Revision,
                                                                                                                            WindchillObjectType.PART,
                                                                                                                            WindchillNetworkCredential.WindchillCredential,
                                                                                                                            NamingConvention,
                                                                                                                            true);
                    CurrentItem.PartStructure.MainObject = CurrentItem.EcnWtPart;
                }
                WindchillObjectLink CurrentLinkOwner = CurrentItem.LinkWtPartEpmDocumentOwner.FirstOrDefault((link) => link.LinkType == WindchillObjectLinkType.OWNER);
                if (CurrentLinkOwner != null && CurrentLinkOwner.LinkedObject != null && CurrentLinkOwner.LinkedObject.CheckedObject != null && CurrentLinkOwner.LinkedObject.CheckedObject.SubType == WindchillObjectType.ASM)
                {
                    //CurrentItem.EpmDocStructure = WindchillRequestTool.WindchillRequestTool.GetBomFirstLevelNamingConventionOneOccurence(CurrentLinkOwner.LinkedObject.Number,
                    //                                                                                    CurrentLinkOwner.LinkedObject.Revision,
                    //                                                                                    WindchillObjectInternalType.EPMDoc,
                    //                                                                                    WindchillNetworkCredential.WindchillCredential,
                    //                                                                                    NamingConvention,
                    //                                                                                    true);
                    CurrentItem.EpmDocStructure = _windchillBomManagementService.GetBomFirstLevelNamingConventionOneOccurence(CurrentLinkOwner.LinkedObject.Number,
                                                                                    CurrentLinkOwner.LinkedObject.Revision,
                                                                                    WindchillObjectType.ASM,
                                                                                    WindchillNetworkCredential.WindchillCredential,
                                                                                    NamingConvention,
                                                                                    true);
                    CurrentItem.EpmDocStructure.MainObject = CurrentLinkOwner.LinkedObject;
                }
                System.Threading.Thread.Sleep(CommonLibConstants.RequestAwaitingTime);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CompareOneDataCheckItemPdmBomAsync(EcnDataCheckItem CurrentItem)
        {
            try
            {
                DataCheckRule CurrentDataCheckRule = null;

                // Case if no BOM in part and EPmDoc --> single part
                if ((CurrentItem.PartStructure == null || CurrentItem.PartStructure.Structure.Count() == 0) && (CurrentItem.EpmDocStructure == null || CurrentItem.EpmDocStructure.Structure.Count() == 0))
                {
                    CurrentItem.BomPdmComparisonStatus = DataCheckStatus.NONE;
                    CurrentItem.IsPdmBomComparison = false;
                }
                else
                {
                    // start BOM Comparison
                    BomItem EpmDocBomItem = null, PartBomItem = null;
                    if (CurrentItem.EpmDocStructure != null) EpmDocBomItem = _windchillRequestMiscService.GetBomItem(CurrentItem.EpmDocStructure, CurrentEcnDataCheckDataContext.NumericalLineNumberDigit);
                    if (CurrentItem.PartStructure != null) PartBomItem = _windchillRequestMiscService.GetBomItem(CurrentItem.PartStructure, CurrentEcnDataCheckDataContext.NumericalLineNumberDigit);

                    CurrentItem.PdmBomComparison = _bomComparisonToolService.GetBomComparison(EpmDocBomItem, PartBomItem);
                    CurrentItem.PdmBomComparison.Type = "PDM";
                    CurrentItem.PdmBomComparison.SourceBom1 = McgWpfTools.GetStringResource("EDC_PdmSource1");
                    CurrentItem.PdmBomComparison.SourceBom2 = McgWpfTools.GetStringResource("EDC_PdmSource2");
                    CurrentItem.PdmBomComparison.WindowTitle = McgWpfTools.GetStringResource("EDC_PdmCompWindowTitle");
                    CurrentItem.PdmBomComparison.PartNumber = CurrentItem.EcnWtPart.Number;
                    CurrentItem.PdmBomComparison.Description = $"{CurrentItem.EcnWtPart.Name} {CurrentItem.EcnWtPart.DescriptionEn2}";
                    CurrentItem.PdmBomComparison.XlsFileTemplateFullPath = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{EcnDataCheckConstants.ExcelTemplateEcnDataCheck}";
                    CurrentItem.PdmBomComparison.XlsFileTemplateSheetName = EcnDataCheckConstants.ExcelTemplateBomSheet;
                    CurrentItem.IsPdmBomComparison = CurrentItem.PdmBomComparison.BomComparison.Any();

                    // Check BOM Comparison
                    if (CurrentItem.PdmBomComparison.IsIdentical)
                        CurrentItem.BomPdmComparisonStatus = DataCheckStatus.OK;
                    else
                    {
                        CurrentDataCheckRule = GetDataCheckRule("PartCadDocBomAccurate");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                        {
                            string AdditionalMessage = "";
                            // Check if EpmDoc Bom has Intermediaris assembly
                            if (CurrentItem.EpmDocStructure != null && CurrentItem.EpmDocStructure.IntermediateStructure.Any())
                            {
                                AdditionalMessage = McgWpfTools.GetStringResource("EDC_CheckMsg38Add");
                            }
                            CurrentItem.BomPdmComparisonStatus = GetWorstDataCheckStatus(CurrentItem.BomPdmComparisonStatus, CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg38", new string[1] { AdditionalMessage }).Status);
                        }
                    }

                    // Check if component with incorrect state
                    if (CurrentItem.PdmBomComparison.IsStateWarning)
                    {
                        CurrentDataCheckRule = GetDataCheckRule("BomComponentIncorrectState");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                            CurrentItem.BomPdmComparisonStatus = GetWorstDataCheckStatus(CurrentItem.BomPdmComparisonStatus, CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg48", new string[1] { CurrentItem.PdmBomComparison.ComponentIncorrectState }).Status);
                    }

                    // Check if there is components with wrong number
                    // EpmDocument Structure
                    string MissingCompList = "";
                    CurrentDataCheckRule = GetDataCheckRule("CadDocBomExport");
                    if (CurrentItem.EpmDocStructure != null && CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentItem.EpmDocStructure.ComponentError.Count() > 0)
                    {
                        foreach (WindchillObjStructureComponent comp in CurrentItem.EpmDocStructure.ComponentError)
                            MissingCompList = $"{MissingCompList}, {comp.MainWindchillObject.Number}";
                        CurrentItem.BomPdmComparisonStatus = GetWorstDataCheckStatus(CurrentItem.BomPdmComparisonStatus, CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg43", new string[1] { MissingCompList }).Status);
                    }

                    // Check if there is components with wrong number
                    // WtPart Structure
                    CurrentDataCheckRule = GetDataCheckRule("PartBomExport");
                    if (CurrentItem.EpmDocStructure != null && CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE && CurrentItem.PartStructure.ComponentError.Count() > 0)
                    {
                        MissingCompList = "";
                        foreach (WindchillObjStructureComponent comp in CurrentItem.PartStructure.ComponentError)
                            MissingCompList = $"{MissingCompList}, {comp.MainWindchillObject.Number}";
                        CurrentItem.BomPdmComparisonStatus = GetWorstDataCheckStatus(CurrentItem.BomPdmComparisonStatus, CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg47", new string[1] { MissingCompList }).Status);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods to extract and compare ERP BOM
        private void DownloadErpBom(bool StartReadBom = true)
        {
            try
            {
                // For SAP as ERP
                if (CurrentEcnDataCheckDataContext.ErpSystem == "SAP")
                    ExtractSapBom(StartReadBom);

                // For BAAN As ERP
                else if (CurrentEcnDataCheckDataContext.ErpSystem == "BAAN")
                    ExtractBaanBom(StartReadBom);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void ExtractSapBom(bool StartReadBom = true)
        {
            try
            {
                List<SAPMaterialMaster> MaterilaList = new List<SAPMaterialMaster>();
                string SapPlantNumber = "";
                if (CurrentEcnDataCheckDataContext.SelectedSapPlant.Name != "WITHOUT")
                    SapPlantNumber = CurrentEcnDataCheckDataContext.SelectedSapPlant.Number;

                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList.Where((item) => item.EcnWtPart.CheckedObject != null &&
                                                                                                                          item.EcnWtPart.CheckedObject.NumberTemplate != null &&
                                                                                                                          item.EcnWtPart.CheckedObject.NumberTemplate.FunctionalType == WindchillObjectType.PHYSICAL_PART &&
                                                                                                                          item.PartMissingCheck == DataCheckStatus.OK))
                    MaterilaList.Add(new SAPMaterialMaster() { PartNumber = CurrentItem.EcnWtPart.Number });

                if (MaterilaList.Count > 0)
                {
                    object SapConnection = _ISapSessionManager.GetActiveSession();
                    if (SapConnection != null)
                    {
                        var sapBom = _sapBomService.ExtractOneMaterialMasterSapBom(MaterilaList.Select(m => m.PartNumber).ToList(), EcnDataCheckConstants.ErpValidityDate, SapPlantNumber);
                        //if (SAPTools.ZDTB_MATERIAL_BOM_DL(SapConnection, MaterilaList, EcnDataCheckConstants.ErpValidityDate, SapPlantNumber))
                        if (sapBom != null && sapBom.Count > 0)
                        {
                            if (StartReadBom)
                            {
                                ReadSapBom(sapBom);
                                MessageBox.Show(String.Format(McgWpfTools.GetStringResource("EDC_InfoMsgErpCompEnd"), CurrentEcnDataCheckDataContext.ErpSystem), McgWpfTools.GetStringResource("EDC_InfoTitleErpBom"), MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                            MessageBox.Show(String.Format(McgWpfTools.GetStringResource("EDC_InfoMsgErpCom"), CurrentEcnDataCheckDataContext.ErpSystem), McgWpfTools.GetStringResource("EDC_InfoTitleErpBom"), MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("EDC_InfoMsgErpConNotFound"), CurrentEcnDataCheckDataContext.ErpSystem), McgWpfTools.GetStringResource("EDC_InfoTitleErpBom"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void ReadSapBom(List<BomComponent> sapBom = null)
        {
            try
            {
                if (sapBom == null)
                    sapBom = _sapBomService.ExtractSapBomFromFile($"{System.Environment.GetEnvironmentVariable("TEMP")}\\{EcnDataCheckConstants.ExtractedSapBomFileName}", CurrentEcnDataCheckDataContext.NumericalLineNumberDigit);

                BomItem CurrentBomItem = null;
                DataCheckRule CurrentDataCheckRule = null;

                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList.Where((item) => item.EcnWtPart.CheckedObject != null &&
                                                                                                                          item.EcnWtPart.CheckedObject.NumberTemplate != null &&
                                                                                                                          item.EcnWtPart.CheckedObject.NumberTemplate.FunctionalType == WindchillObjectType.PHYSICAL_PART &&
                                                                                                                          item.PartMissingCheck == DataCheckStatus.OK))
                {
                    var tempBom = sapBom.Where((elem) => elem.ParentNumber == CurrentItem.EcnWtPart.Number && elem.Level == 1);
                    CurrentBomItem = new BomItem();
                    CurrentBomItem.Bom = new ObservableCollection<BomComponent>();
                    foreach (BomComponent comp in tempBom)
                        CurrentBomItem.Bom.Add(comp);

                    CurrentItem.EprStructure = CurrentBomItem;
                    CurrentItem.ErpBomComparison = _bomComparisonToolService.GetBomComparison(_windchillRequestMiscService.GetBomItem(CurrentItem.PartStructure, CurrentEcnDataCheckDataContext.NumericalLineNumberDigit),
                                                                                      CurrentItem.EprStructure, false, false);
                    CurrentItem.ErpBomComparison.Type = "ERP";
                    CurrentItem.ErpBomComparison.SourceBom1 = McgWpfTools.GetStringResource("EDC_PdmSource2");
                    CurrentItem.ErpBomComparison.SourceBom2 = McgWpfTools.GetStringResource("EDC_ErpSource");
                    CurrentItem.ErpBomComparison.WindowTitle = McgWpfTools.GetStringResource("EDC_ErpCompWindowTitle");
                    CurrentItem.ErpBomComparison.PartNumber = CurrentItem.EcnWtPart.Number;
                    CurrentItem.ErpBomComparison.Description = $"{CurrentItem.EcnWtPart.Name} {CurrentItem.EcnWtPart.DescriptionEn2}";
                    CurrentItem.ErpBomComparison.XlsFileTemplateFullPath = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{EcnDataCheckConstants.ExcelTemplateEcnDataCheck}";
                    CurrentItem.ErpBomComparison.XlsFileTemplateSheetName = EcnDataCheckConstants.ExcelTemplateBomSheet;
                    CurrentItem.IsErpBomComparison = true;

                    if (CurrentItem.ErpBomComparison.IsIdentical)
                        CurrentItem.BomErpComparisonStatus = DataCheckStatus.OK;
                    else
                    {
                        CurrentDataCheckRule = GetDataCheckRule("PartErpBomAccurate");
                        if (CurrentDataCheckRule != null && CurrentDataCheckRule.GetDataCheckStatus() != DataCheckStatus.NONE)
                            CurrentItem.BomErpComparisonStatus = GetWorstDataCheckStatus(CurrentItem.BomErpComparisonStatus, CreateResultItem(CurrentItem, CurrentDataCheckRule, "EDC_CheckMsg40").Status);
                    }

                    if (CurrentItem.ErpBomComparison.BomComparison.Count() == 0)
                    {
                        CurrentItem.BomErpComparisonStatus = DataCheckStatus.NONE;
                        CurrentItem.IsErpBomComparison = false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void ExtractBaanBom(bool StartReadBom = true)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc methods to Extract Windchill Information and CREOTools Database Information
        private void GetWebtermList()
        {
            try
            {
                if (!IsWebtermListSearched)
                {
                    ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).AllWebtermList.Clear();
                    ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).AllWebtermList.AddRange(_webtermTools.GetWebtermList());

                    CurrentEcnDataCheckDataContext.WebTermList.Clear();
                    foreach (var term in _webtermTools.GetListTerm(WebtermLanguage.ENGLISH, ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).AllWebtermList))
                        CurrentEcnDataCheckDataContext.WebTermList.Add(term);
                    IsWebtermListSearched = true;
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods to export to Excel
        private void ExportEcnDataCheckToExcel()
        {
            try
            {
                Regex RegexProc = new Regex("Excel", RegexOptions.IgnoreCase);
                List<Process> OldExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                string UserDocumentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string XlsFileName = $"{UserDocumentFolder}\\ECN_DATA_CHECK_{CurrentEcnDataCheckDataContext.EcnNumber}.xlsx";

                string resourcesFolder = CommonLibConstants.ResourcesFolder;
                string templateFile = EcnDataCheckConstants.ExcelTemplateEcnDataCheck;
                string templatePath = Path.Combine(MainAppFolder, resourcesFolder, templateFile);

                ExcelToolsClosedXml CurrentExcel = new ExcelToolsClosedXml { CompleteFileName = XlsFileName, CompleteTemplateFileName = templatePath };

                if (CurrentExcel.OpenFile(templatePath) != ExcelStatus.OK)
                {
                    MessageBox.Show(String.Format(McgWpfTools.GetStringResource("EDC_ExportXlsIssue"), XlsFileName), "Excel Export Issue", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }

                List<Process> NewExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                Process newExcelProcess = NewExcelCurrentProcess.FirstOrDefault((proc) => !OldExcelCurrentProcess.ToList().Exists((oldprc) => proc.Id == oldprc.Id));

                // Update Main Table
                CurrentExcel.CurrentSheet = "Main";
                CurrentExcel.SetCellValue(CurrentEcnDataCheckDataContext.EcnNumber, 2, 3);
                CurrentExcel.SetCellValue(CurrentEcnDataCheckDataContext.EcnNumber, 2, 4);

                int Index = 7;

                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList)
                {
                    CurrentExcel.CurrentSheet = "Main";
                    CurrentExcel.SetCellValue(CurrentItem.EcnWtPart.Number, Index, 2);
                    CurrentExcel.SetCellValue(CurrentItem.EcnWtPart.Revision, Index, 3);
                    CurrentExcel.SetCellValue(CurrentItem.MetaDataStatus.ToString(), Index, 4);
                    CurrentExcel.SetCellValue(CurrentItem.BomPdmComparisonStatus.ToString(), Index, 5);
                    CurrentExcel.SetCellValue(CurrentItem.BomErpComparisonStatus.ToString(), Index, 6);

                    if (CurrentItem.PartMissingCheck == DataCheckStatus.OK)
                    {
                        CurrentExcel.SetCellValue(CurrentItem.EcnWtPart.Name, Index, 7);
                        CurrentExcel.SetCellValue(CurrentItem.EcnWtPart.DescriptionEn2, Index, 8);
                        CurrentExcel.SetCellValue(CurrentItem.EcnWtPart.DescriptionLocal1, Index, 9);
                        CurrentExcel.SetCellValue(CurrentItem.EcnWtPart.DescriptionLocal2, Index, 10);
                        CurrentExcel.SetCellValue(CurrentItem.EcnWtPart.GroupCreator, Index, 11);
                        CurrentExcel.SetCellValue(CurrentItem.EcnWtPart.Mass, Index, 12);
                        CurrentExcel.SetCellValue(CurrentItem.EcnWtPart.Material, Index, 13);
                        CurrentExcel.SetCellValue(CurrentItem.EcnWtPart.QualInspGrp, Index, 14);
                        CurrentExcel.SetCellValue(CurrentItem.EcnWtPart.Context.Name, Index, 15);
                        CurrentExcel.SetCellValue(CurrentItem.EcnWtPart.Folder, Index, 16);

                        // Export PDM BOM Comparison
                        if (CurrentItem.IsPdmBomComparison && CurrentItem.PdmBomComparison != null)
                            CurrentItem.PdmBomComparison.ExportExcelPdm(CurrentExcel);

                        // Export ERP BOM Comparison
                        if (CurrentItem.IsErpBomComparison && CurrentItem.ErpBomComparison != null)
                            CurrentItem.ErpBomComparison.ExportExcelErp(CurrentExcel);
                    }

                    Index++;
                }

                // Export Result Tab
                CurrentExcel.CurrentSheet = "Check Result";
                Index = 3;
                foreach (EcnDataCheckResultItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckResultItemList)
                {
                    CurrentExcel.SetCellValue(CurrentItem.ParentEcnDataCheckItem.EcnWtPart.Number, Index, 1);
                    CurrentExcel.SetCellValue(CurrentItem.ParentEcnDataCheckItem.EcnWtPart.Revision, Index, 3);
                    CurrentExcel.SetCellValue(CurrentItem.Status.ToString(), Index, 4);
                    CurrentExcel.SetCellValue(CurrentItem.LinkedObjNumber, Index, 5);
                    CurrentExcel.SetCellValue(CurrentItem.LinkedObjRevision, Index, 6);
                    CurrentExcel.SetCellValue(CurrentItem.CurrentLink, Index, 7);
                    CurrentExcel.SetCellValue(CurrentItem.Comments, Index, 8);
                    Index++;
                }

                if (CurrentExcel.SaveClose() != ExcelStatus.OK)
                {
                    MessageBox.Show(String.Format(McgWpfTools.GetStringResource("EDC_ExportXlsIssue"), XlsFileName), "Excel Export Issue", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }

                if (newExcelProcess != null)
                    newExcelProcess.Kill();

                _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("EDC_BtExportResult"), String.Format(McgWpfTools.GetStringResource("EDC_ExportXls"), XlsFileName), McgWpfTools.GetStringResource("EDC_ToolTipOpen"), McgWpfTools.GetStringResource("EDC_ToolTipOpenFolder"), McgWpfTools.GetStringResource("EDC_ToolTipClose"), XlsFileName);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        private DataCheckRule GetDataCheckRule(string DataCheckName)
        {
            try
            {
                return CurrentEcnDataCheckConfiguration.AllDataCheckRules.FirstOrDefault((r) => r.Name == DataCheckName);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void CheckWindchillCredential()
        {
            try
            {
                if (WindchillNetworkCredential == null || !WindchillNetworkCredential.IsCredentialOk)
                {
                    WindchillNetworkCredential = _windchillCredentialService.GetWindchillCredential(CommonLibConstants.WindchillUrl, CommonLibConstants.WindchillUrl);
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void ResetInterface()
        {
            try
            {
                CurrentEcnDataCheckDataContext.EcnDataCheckInProgress = false;
                CurrentEcnDataCheckDataContext.ShowActionButton = true;
                RaiseActionDoneEvent();
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateDataCheckResultItemList(bool isSapBomUpdate = false)
        {
            try
            {
                // reset all resulting lists
                CurrentEcnDataCheckDataContext.DataCheckResultItemList.Clear();
                CurrentEcnDataCheckDataContext.MoveItemList.Clear();
                CurrentEcnDataCheckDataContext.RenameItemList.Clear();

                // Udpate all lists
                foreach (EcnDataCheckItem CurrentItem in CurrentEcnDataCheckDataContext.DataCheckItemList.Concat(((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).OtherCheckItemList))
                {
                    // update Result Item
                    foreach (EcnDataCheckResultItem CurrentResultItem in CurrentItem.ListDataCheckResult)
                    {
                        // check if aDataCheckResultItem is not several time
                        // due to a bug with Query Builder request that can send back several time the same lines
                        if (!CurrentEcnDataCheckDataContext.DataCheckResultItemList.Any((r) => ((EcnDataCheckResultItem)r).GetFullString() == CurrentResultItem.GetFullString()))
                            CurrentEcnDataCheckDataContext.DataCheckResultItemList.Add(CurrentResultItem);
                    }

                    // Update Rename List
                    if (CurrentItem.Desc1EnStatus != DataCheckStatus.OK)
                        CurrentEcnDataCheckDataContext.RenameItemList.Add(CurrentItem);

                    // Update Move Context List
                    if (CurrentItem.ContextStatus != DataCheckStatus.OK)
                        CurrentEcnDataCheckDataContext.MoveItemList.Add(CurrentItem);
                }

                // Show/hide Rename and Move tab
                CurrentEcnDataCheckDataContext.ShowRenameTab = CurrentEcnDataCheckDataContext.RenameItemList.Any();
                CurrentEcnDataCheckDataContext.ShowMoveTab = CurrentEcnDataCheckDataContext.MoveItemList.Any();
                if (!isSapBomUpdate)
                {
                    // Update Global Status
                    if (CurrentEcnDataCheckDataContext.DataCheckItemList.Concat(((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).OtherCheckItemList).Any((item) => item.MetaDataStatus == DataCheckStatus.ISSUE || item.BomPdmComparisonStatus == DataCheckStatus.ISSUE))
                        CurrentEcnDataCheckDataContext.GlobalStatus = DataCheckStatus.ISSUE;
                    else if (CurrentEcnDataCheckDataContext.DataCheckItemList.Concat(((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).OtherCheckItemList).Any((item) => item.MetaDataStatus == DataCheckStatus.WARNING || item.BomPdmComparisonStatus == DataCheckStatus.WARNING)
                        && CurrentEcnDataCheckDataContext.GlobalStatus != DataCheckStatus.ISSUE)
                        CurrentEcnDataCheckDataContext.GlobalStatus = DataCheckStatus.WARNING;
                    else if (CurrentEcnDataCheckDataContext.DataCheckItemList.Concat(((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).OtherCheckItemList).Any((item) => item.MetaDataStatus == DataCheckStatus.OK || item.BomPdmComparisonStatus == DataCheckStatus.OK)
                        && CurrentEcnDataCheckDataContext.GlobalStatus != DataCheckStatus.ISSUE && CurrentEcnDataCheckDataContext.GlobalStatus != DataCheckStatus.WARNING)
                        CurrentEcnDataCheckDataContext.GlobalStatus = DataCheckStatus.OK;
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateDataCheckItemListWithMissingPart()
        {
            try
            {
                foreach (EcnDataCheckItem item in ((EcnDataCheckDataContext)CurrentEcnDataCheckDataContext).MissingWtPartInEcnList)
                    CurrentEcnDataCheckDataContext.DataCheckItemList.Add(item);
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateDataCheckStatus(IEcnDataCheckItem currentItem, DataCheckStatus status)
        {
            try
            {
                if (status == DataCheckStatus.ISSUE)
                    currentItem.MetaDataStatus = DataCheckStatus.ISSUE;
                else if (status == DataCheckStatus.WARNING && currentItem.MetaDataStatus != DataCheckStatus.ISSUE)
                    currentItem.MetaDataStatus = DataCheckStatus.WARNING;
                else if (status == DataCheckStatus.OK && (currentItem.MetaDataStatus == DataCheckStatus.UNKNOWN || currentItem.MetaDataStatus == DataCheckStatus.NONE))
                    currentItem.MetaDataStatus = DataCheckStatus.OK;

            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private DataCheckStatus GetWorstDataCheckStatus(DataCheckStatus OldDataCheckStatus, DataCheckStatus NewDataCheckStatus)
        {
            try
            {
                switch (NewDataCheckStatus)
                {
                    case DataCheckStatus.ISSUE:
                        return DataCheckStatus.ISSUE;

                    case DataCheckStatus.WARNING:
                        if (OldDataCheckStatus == DataCheckStatus.ISSUE)
                            return OldDataCheckStatus;
                        else
                            return NewDataCheckStatus;
                    case DataCheckStatus.OK:
                        if (OldDataCheckStatus == DataCheckStatus.ISSUE || OldDataCheckStatus == DataCheckStatus.WARNING)
                            return OldDataCheckStatus;
                        else
                            return NewDataCheckStatus;
                    default:
                        return OldDataCheckStatus;
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateUpdateDataCheckResultItemListLanguage()
        {
            try
            {
                EcnDataCheckResultItem CurrentItem;
                foreach (var item in CurrentEcnDataCheckDataContext.DataCheckResultItemList)
                {
                    CurrentItem = (EcnDataCheckResultItem)item;
                    if (CurrentItem.ParamString == null || CurrentItem.ParamString.Count() == 0)
                        CurrentItem.Comments = McgWpfTools.GetStringResource(CurrentItem.KeyStringResource);
                    else
                        CurrentItem.Comments = string.Format(McgWpfTools.GetStringResource(CurrentItem.KeyStringResource), CurrentItem.ParamString);
                    if (CurrentItem.LinkedObj != null)
                    {
                        if (CurrentItem.KeyString == null)
                            CurrentItem.CurrentLink = McgWpfTools.GetStringResource($"EDC_Link_{GetCurrentLinkWithWindchillObject(CurrentItem.CurrentDataCheckItem, CurrentItem.LinkedObj).ToString()}");
                        else
                            CurrentItem.CurrentLink = McgWpfTools.GetStringResource(CurrentItem.KeyString);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
