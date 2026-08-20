using ClosedXML;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.DataBaseAccess.Models.SapHupDb;
using MCG.CommonLib.DataBaseAccess.Services;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.SapTools.Interfaces;
using MCG.CommonLib.Services;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.Models.McgWindows;
using MCG.CommonLib.WpfComponent.View;
using MCG.CommonLib.WpfComponent.WindchillCredential;
using MCG.Tools.EcnEcoFollowUp.Configuration;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Interfaces.Models;
using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.View;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.Services;
using MCG.WindchillRequestTool.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using PdfSharp.Pdf.Content.Objects;
using System.Collections.ObjectModel;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MCG.Tools.EcnEcoFollowUp.ViewModel
{
    public class EcnEcoFollowUpViewModel : ObservableObject, IEcnEcoFollowUpViewModel
    {
        #region [REGION] Properties from Interface
        public EcnEcoFollowUpDataContext CurrentEcnEcoFollowUpDataContext { get; set; }
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

        #region [REGION] Internal variables
        private EcnEcoFollowUpConfiguration CurrentEcnEcoFollowUpConfiguration { get; set; }
        private string MainAppFolder { get; set; }
        private string SaveSearchFolder { get; set; }
        public NetworkCredential WindchillNetworkCredential { get; set; } = null;
        private UserPrincipal LoggedUser { get; set; }
        private EcnEcoFollowUpUserConfiguration CurrentEcnEcoFollowUpUserConfiguration { get; set; }
        private string ImageResourcePath { get; set; }

        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly IRegExTools _regExTools;
        private readonly IUserAuthorizationService _userAuthorizationService;
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        private readonly IEcnEcoFollowUpWindowService _ecnEcoFollowUpWindowService;
        private readonly IEcnEcoFollowUpService _ecnEcoFollowUpService;

        private readonly ISapEcoService _sapEcoService;
        private readonly ISapHupService _sapHupService;

        private readonly IWindchillChangeManagementService _windchillChangeManagementService;
        private readonly IWindchillReportingManagementService _windchillReportingManagementService;
        private readonly IWindchillRequestMiscService _windchillRequestMiscService;
        private readonly IWindchillRequestTool _windchillRequestTool;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly IWindchillNavigationService _windchillNavigationService;
        private readonly ISharedAppContext _sharedAppContext;
        #endregion

        #region [REGION] Commands
        public ICommand CommandExportXls { get => new RelayCommand(() => ExecuteExportXls()); }
        public ICommand CommandMenuItemCreateDashboardEmpty { get => new RelayCommand(() => ExecuteMenuItemCreateDashboard()); }
        public ICommand CommandMenuItemCreateDashboardFromSearch { get => new RelayCommand(() => ExecuteMenuItemCreateDashboard(false)); }
        public ICommand CommandMenuItemSaveSearch { get => new RelayCommand(() => ExecuteMenuItemSaveSearch()); }
        public ICommand CommandSavedOrRecentSearch { get => new RelayCommand<EFU_SearchTemplate>((item) => ExecuteSavedOrRecentSearch(item)); }
        public ICommand CommandShowHelp { get => new RelayCommand(() => ExecuteShowHelp()); }
        public ICommand CommandSearchEcnEco { get => new RelayCommand(() => ExecuteSearchEcnEco()); }
        public ICommand CommandMenuItemOpenEcn { get => new RelayCommand(() => ExecuteMenuItemOpenEcn()); }
        public ICommand CommandMenuItemOpenEco { get => new RelayCommand(() => ExecuteMenuItemOpenEco()); }
        public ICommand CommandMenuItemOpenEcnDocs { get => new RelayCommand(() => ExecuteMenuItemOpenEcnDocs()); }
        public ICommand CommandMenutItemSearchEcnWfTask { get => new RelayCommand(() => ExecuteMenutItemSearchEcnWfTask()); }
        public ICommand CommandMenutItemSearchEcoWfTask { get => new RelayCommand(() => ExecuteMenutItemSearchEcoWfTask()); }
        public ICommand CommandMenuItemOpenEcoDashboard { get => new RelayCommand(() => ExecuteMenuItemOpenEcoDashboard()); }
        public ICommand CommandRenameSearch { get => new RelayCommand<EFU_SearchTemplate>((item) => ExecuteRenameSearch(item)); }
        public ICommand CommandUpdateSearch { get => new RelayCommand<EFU_SearchTemplate>((item) => ExecuteUpdateSearch(item)); }
        public ICommand CommandDeleteSearch { get => new RelayCommand<EFU_SearchTemplate>((item) => ExecuteDeleteSearch(item)); }
        public ICommand CommandExportSearch { get => new RelayCommand<EFU_SearchTemplate>((item) => ExecuteExportSearch(item)); }
        public ICommand CommandMenutItemAddEcnEcoToDashboard { get => new RelayCommand<EcnEcoFollowUpDashboardViewModel>((item) => ExecuteMenutItemAddEcnEcoToDashboard(item)); }
        public ICommand CommandMenuItemSearchDashboard { get => new RelayCommand(() => ExecuteMenuItemSearchDashboard()); }
        public ICommand CommandDashBoardShow { get => new RelayCommand<EcnEcoFollowUpDashboardViewModel>((item) => ExecuteDashBoardShow(item)); }
        public ICommand CommandDashBoardHide { get => new RelayCommand<EcnEcoFollowUpDashboardViewModel>((item) => ExecuteDashBoardHide(item)); }
        public ICommand CommandDashBoardExport { get => new RelayCommand<EcnEcoFollowUpDashboardViewModel>((item) => ExecuteDashBoardExport(item)); }
        public ICommand CommandDashBoardRename { get => new RelayCommand<EcnEcoFollowUpDashboardViewModel>((item) => ExecuteDashBoardRename(item)); }
        public ICommand CommandDashBoardDelete { get => new RelayCommand<EcnEcoFollowUpDashboardViewModel>((item) => ExecuteDashBoardDelete(item)); }
        public ICommand CommandDashBoardRemove { get => new RelayCommand<EcnEcoFollowUpDashboardViewModel>((item) => ExecuteDashBoardRemove(item)); }
        public ICommand CommandAddSelectedEcnEcotoDashboard { get => new RelayCommand(() => ExecuteAddSelectedEcnEcotoDashboard()); }
        public ICommand CommandAdmToolDeleteEcnEco { get => new RelayCommand(() => ExecuteAdmToolDeleteEcnEco()); }
        public ICommand CommandAdmToolSeachDeletedEcn { get => new RelayCommand(() => ExecuteAdmToolSeachDeletedEcn()); }
        public ICommand CommandCheckAllMain { get => new RelayCommand(() => ExecuteCheckUncheckAllMain(true)); }
        public ICommand CommandUncheckAllMain { get => new RelayCommand(() => ExecuteCheckUncheckAllMain(false)); }
        public ICommand CommandOpenAttachment { get => new RelayCommand<RestOdataAttachment>((item) => ExecuteOpenAttachment(item)); }
        #endregion

        #region [REGION] Init
        public EcnEcoFollowUpViewModel(IXmlSerializeTools xmlSerializeTools,
                                       IUserAuthorizationService userAuthorizationService,
                                       IMcgCommonLibWindowService mcgCommonLibWindowService,
                                       IWindchillChangeManagementService windchillChangeManagementService,
                                       IEcnEcoFollowUpWindowService ecnEcoFollowUpWindowService,
                                       IEcnEcoFollowUpService ecnEcoFollowUpService,
                                       IWindchillReportingManagementService windchillReportingManagementService,
                                       IWindchillRequestMiscService windchillRequestMiscService,
                                       IRegExTools regExTools,
                                       IWindchillRequestTool windchillRequestTool,
                                       IWindchillCredentialService windchillCredentialService,
                                       IWindchillNavigationService windchillNavigationService,
                                       ISapEcoService sapEcoService,
                                       ISapHupService sapHupService,
                                       ISharedAppContext sharedAppContext)
        {
            try
            {
                _xmlSerializeTools = xmlSerializeTools;
                _userAuthorizationService = userAuthorizationService;
                _ecnEcoFollowUpWindowService = ecnEcoFollowUpWindowService;
                _windchillChangeManagementService = windchillChangeManagementService;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;
                _ecnEcoFollowUpService = ecnEcoFollowUpService;
                _windchillReportingManagementService = windchillReportingManagementService;
                _windchillRequestMiscService = windchillRequestMiscService;
                _regExTools = regExTools;
                _windchillRequestTool = windchillRequestTool;
                _windchillCredentialService = windchillCredentialService;
                _windchillNavigationService = windchillNavigationService;
                _sapEcoService = sapEcoService;
                _sapHupService = sapHupService;
                _sharedAppContext = sharedAppContext;

                ImageResourcePath = EcnEcoFollowUpConstants.ImageResourcesPath;
                LoggedUser = UserPrincipal.Current;

                CurrentEcnEcoFollowUpDataContext = new EcnEcoFollowUpDataContext();

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                SaveSearchFolder = $"{System.Environment.GetEnvironmentVariable("APPDATA")}\\{CommonLibConstants.SavedSearchFolder}\\";

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{EcnEcoFollowUpConstants.MainDictionary}", UriKind.Absolute);

                CurrentEcnEcoFollowUpConfiguration = _xmlSerializeTools.GetDeserializedXml<EcnEcoFollowUpConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{EcnEcoFollowUpConstants.ConfigurationFile}");
                CurrentEcnEcoFollowUpUserConfiguration = _xmlSerializeTools.GetDeserializedXmlFromAppData<EcnEcoFollowUpUserConfiguration>(EcnEcoFollowUpConstants.UserConfigurationFile);
                if (CurrentEcnEcoFollowUpUserConfiguration != null)
                    CurrentEcnEcoFollowUpUserConfiguration.UserConfigurationUpdateEvent += UpdateUserConfigXmlFile;

                // Update List for ECNState
                if (CurrentEcnEcoFollowUpConfiguration.EcnStateList != null)
                {
                    CurrentEcnEcoFollowUpDataContext.EcnStateList.Clear();
                    foreach (var item in CurrentEcnEcoFollowUpConfiguration.EcnStateList)
                        CurrentEcnEcoFollowUpDataContext.EcnStateList.Add(item);
                }
                CurrentEcnEcoFollowUpDataContext.EcnState = CurrentEcnEcoFollowUpDataContext.EcnStateList.FirstOrDefault();

                ReadPdmContextList();
                CurrentEcnEcoFollowUpDataContext.PdmProduct = CurrentEcnEcoFollowUpDataContext.PdmProductList.FirstOrDefault();

                UpdateRecentSearchesList();
                UpdateSavedSearchesList();
                SearchActiveDashboard();

                // Update Personal Dashboard
                EFU_DashboardItem DashboardItem = new EFU_DashboardItem()
                {
                    Name = McgWpfTools.GetStringResource("EFU_TabPersonal"),
                    IsActive = true,
                    IsShown = true,
                    ParentApp = this,
                    IsAddDeletEcnEcoAllowed = false,
                    IsHideShowDashboardAllowed = false,
                    IsPersonalInfoShown = false,
                    IsPersonalDashBoard = true
                };
                DashboardItem.CurrentDashboardConfiguration.Parent = DashboardItem;
                DashboardItem.CurrentDashboardConfiguration.IsUpdateFilterEvent += ApplyFilterDashboardListEcnEco;
                UpdatePersonalDashboardEcnEco(DashboardItem);
                UpdateDashboardInformation(DashboardItem);
                CurrentEcnEcoFollowUpDataContext.PersonalDashboard = _ecnEcoFollowUpWindowService.GetEcnEcoFollowUpDashboardView(DashboardItem);

                MCGLanguage CurrentMCGLANGUAGE = _sharedAppContext.CurrentLanguage?.Language;
                if (CurrentMCGLANGUAGE != null)
                    CurrentMCGLANGUAGE.ChangeLanguageInterface += UpdateInterfaceLanguage;

                CurrentEcnEcoFollowUpDataContext.IsAdminToolsEnabled = _userAuthorizationService.GetIsAppCadAdmin(Environment.UserName, EcnEcoFollowUpConstants.EcnEcoFollowUpAppName);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }

            _sharedAppContext = sharedAppContext;
        }

        private void UpdateInterfaceLanguage(object sender = null, EventArgs e = null)
        {
            try
            {
                CurrentEcnEcoFollowUpDataContext.RaiseDashboardListEvent();
                CurrentEcnEcoFollowUpDataContext.RaiseSavedSearchesListEvent();
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateUserConfigXmlFile(object sender = null, EventArgs e = null)
        {
            try
            {
                if (CurrentEcnEcoFollowUpUserConfiguration != null)
                    _xmlSerializeTools.SerializedXmlInAppData<EcnEcoFollowUpUserConfiguration>(CurrentEcnEcoFollowUpUserConfiguration, EcnEcoFollowUpConstants.UserConfigurationFile);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteExportXls()
        {
            try
            {
                ExportResultInExcel();
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenuItemCreateDashboard(bool IsEmpty = true)
        {
            try
            {
                EFU_SearchTemplate currentSearch = GetCurrentSearch();

                var dialogResult = _mcgCommonLibWindowService.ShowDialogMcgWindowOkCancel(McgWpfTools.GetStringResource("EFU_WTitleCreateDashboard"),
                                                                                        "",
                                                                                        500,
                                                                                        100,
                                                                                        15);
                if (dialogResult.DialogValue == MessageBoxResult.OK)
                {
                    if (!string.IsNullOrEmpty(dialogResult.Value))
                    {
                        CreateDashboard(dialogResult.Value.Trim(), IsEmpty);
                        CurrentEcnEcoFollowUpDataContext.RaiseDashboardListEvent();
                    }
                    else
                        MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgCreateDashboardIssueBlankName"), McgWpfTools.GetStringResource("EFU_WTitleCreateDashboardIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenuItemSaveSearch()
        {
            try
            {
                EFU_SearchTemplate currentSearch = GetCurrentSearch();
                var dialogResult = _mcgCommonLibWindowService.ShowDialogMcgWindowOkCancel(McgWpfTools.GetStringResource("EFU_WTitleSaveSearch"),
                                                                                        currentSearch.Name,
                                                                                        500,
                                                                                        100,
                                                                                        15);

                if (dialogResult.DialogValue == MessageBoxResult.OK)
                {
                    string LastXmlFileName = ReturnLastSavedSearchFileNameAvailable();
                    if (LastXmlFileName != null)
                    {
                        if (!string.IsNullOrEmpty(dialogResult.Value))
                        {
                            currentSearch.Name = dialogResult.Value.Trim();
                            _xmlSerializeTools.SerializedXmlInAppData<EFU_SearchTemplate>(currentSearch, LastXmlFileName);
                            UpdateSavedSearchesList();
                            CurrentEcnEcoFollowUpDataContext.RaiseSavedSearchesListEvent();
                        }
                        else
                            MessageBox.Show(McgWpfTools.GetStringResource("EFU_MesgSaveSearchNameError"), McgWpfTools.GetStringResource("EFU_WTitleSaveSearchImpossible"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                        MessageBox.Show(McgWpfTools.GetStringResource("EFU_MesgSaveSearchImpossible"), McgWpfTools.GetStringResource("EFU_WTitleSaveSearchImpossible"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSavedOrRecentSearch(EFU_SearchTemplate currentSearch)
        {
            try
            {
                if (currentSearch != null)
                {
                    // Udpate different fields for the search
                    UpdateSearchFieldFromSavedSearch(currentSearch);

                    if (CurrentEcnEcoFollowUpDataContext.EcnNumber != null && CurrentEcnEcoFollowUpDataContext.EcnNumber.Trim() != "")
                        CurrentEcnEcoFollowUpDataContext.IsOtherFieldEnable = false;
                    else
                        CurrentEcnEcoFollowUpDataContext.IsOtherFieldEnable = true;

                    // Start the search
                    ExecuteSearchEcnEco();
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteShowHelp()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("EFU_LinkHelpEcnFollowUp"));
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSearchEcnEco()
        {
            try
            {
                List<EFU_EcnEcoToShowEndUser> EcnListMainList = new List<EFU_EcnEcoToShowEndUser>();
                EFU_EcnEcoToShowEndUser CurrentEcn = null;

                // Check if search is only for one ECN
                if (!CurrentEcnEcoFollowUpDataContext.IsOtherFieldEnable)
                {
                    CurrentEcn = SearchOneEcnEcoFollowUp();
                    if (CurrentEcn != null)
                        EcnListMainList.Add(CurrentEcn);
                }
                // If not, search all ECN depending filters
                else
                    EcnListMainList = MainSearchEcnEcoFollowUp();

                // init all values
                CurrentEcnEcoFollowUpDataContext.EcnShownList.Clear();
                CurrentEcnEcoFollowUpDataContext.NbParts = 0;
                CurrentEcnEcoFollowUpDataContext.NbPartsPdmApproved = 0;
                CurrentEcnEcoFollowUpDataContext.NbPartsSapApproved = 0;
                CurrentEcnEcoFollowUpDataContext.NbDrawings = 0;
                CurrentEcnEcoFollowUpDataContext.NbDrawingsPdmApproved = 0;
                CurrentEcnEcoFollowUpDataContext.NbDrawingsSapApproved = 0;
                CurrentEcnEcoFollowUpDataContext.NbEpmDoc = 0;
                CurrentEcnEcoFollowUpDataContext.NbEpmDocPdmApproved = 0;
                CurrentEcnEcoFollowUpDataContext.NbEpmDocSapApproved = 0;
                CurrentEcnEcoFollowUpDataContext.NbWtDoc = 0;
                CurrentEcnEcoFollowUpDataContext.NbWtDocPdmApproved = 0;
                CurrentEcnEcoFollowUpDataContext.NbWtDocSapApproved = 0;
                CurrentEcnEcoFollowUpDataContext.NbEcnPdmApproved = 0;
                CurrentEcnEcoFollowUpDataContext.NbEcnSapApproved = 0;


                foreach (var ecn in EcnListMainList)
                {
                    // Update general information
                    // Nb Part/drawings/EPM Doc/WT Doc
                    CurrentEcnEcoFollowUpDataContext.NbParts = CurrentEcnEcoFollowUpDataContext.NbParts + ecn.EcnEcoFollowUp.Nb_Part;
                    CurrentEcnEcoFollowUpDataContext.NbDrawings = CurrentEcnEcoFollowUpDataContext.NbDrawings + ecn.EcnEcoFollowUp.Nb_Drw;
                    CurrentEcnEcoFollowUpDataContext.NbEpmDoc = CurrentEcnEcoFollowUpDataContext.NbEpmDoc + ecn.EcnEcoFollowUp.Nb_Epm_Doc;
                    CurrentEcnEcoFollowUpDataContext.NbWtDoc = CurrentEcnEcoFollowUpDataContext.NbWtDoc + ecn.EcnEcoFollowUp.Nb_Wt_Doc;
                    //UpdateEcnStatus(ecn);
                    CurrentEcnEcoFollowUpDataContext.EcnShownList.Add(ecn);

                    if ((ecn.EcnEcoFollowUp.Ecn_State.ToUpper() == "RESOLVED"))
                    {
                        CurrentEcnEcoFollowUpDataContext.NbPartsPdmApproved = (CurrentEcnEcoFollowUpDataContext.NbPartsPdmApproved + ecn.EcnEcoFollowUp.Nb_Part);
                        CurrentEcnEcoFollowUpDataContext.NbDrawingsPdmApproved = (CurrentEcnEcoFollowUpDataContext.NbDrawingsPdmApproved + ecn.EcnEcoFollowUp.Nb_Drw);
                        CurrentEcnEcoFollowUpDataContext.NbEpmDocPdmApproved = (CurrentEcnEcoFollowUpDataContext.NbEpmDocPdmApproved + ecn.EcnEcoFollowUp.Nb_Epm_Doc);
                        CurrentEcnEcoFollowUpDataContext.NbWtDocPdmApproved = (CurrentEcnEcoFollowUpDataContext.NbWtDocPdmApproved + ecn.EcnEcoFollowUp.Nb_Wt_Doc);
                        CurrentEcnEcoFollowUpDataContext.NbEcnPdmApproved = (CurrentEcnEcoFollowUpDataContext.NbEcnPdmApproved + 1);
                    }

                    if ((ecn.EcnEcoFollowUp.Eco_Status.ToUpper() == "02"))
                    {
                        CurrentEcnEcoFollowUpDataContext.NbPartsSapApproved = (CurrentEcnEcoFollowUpDataContext.NbPartsSapApproved + ecn.EcnEcoFollowUp.Nb_Part);
                        CurrentEcnEcoFollowUpDataContext.NbDrawingsSapApproved = (CurrentEcnEcoFollowUpDataContext.NbDrawingsSapApproved + ecn.EcnEcoFollowUp.Nb_Drw);
                        CurrentEcnEcoFollowUpDataContext.NbEpmDocSapApproved = (CurrentEcnEcoFollowUpDataContext.NbEpmDocSapApproved + ecn.EcnEcoFollowUp.Nb_Epm_Doc);
                        CurrentEcnEcoFollowUpDataContext.NbWtDocSapApproved = (CurrentEcnEcoFollowUpDataContext.NbWtDocSapApproved + ecn.EcnEcoFollowUp.Nb_Wt_Doc);
                        CurrentEcnEcoFollowUpDataContext.NbEcnSapApproved = (CurrentEcnEcoFollowUpDataContext.NbEcnSapApproved + 1);
                    }
                }

                // Update general information
                // Nb ECN
                CurrentEcnEcoFollowUpDataContext.NbEcn = CurrentEcnEcoFollowUpDataContext.EcnShownList.Count;

                // saved search as recent Searches
                SaveRecentSearch();
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteMenuItemOpenEcn(EFU_EcnEcoToShowEndUser CurrentEcn = null)
        {
            try
            {
                if (CurrentEcn == null) CurrentEcn = CurrentEcnEcoFollowUpDataContext.SelectedEcn;
                if (CurrentEcn != null)
                    _windchillNavigationService.OpenEcnDetailPage(CurrentEcn.EcnEcoFollowUp.Pdm_Ecn_Id);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteMenuItemOpenEco(EFU_EcnEcoToShowEndUser CurrentEcn = null)
        {
            try
            {
                if (CurrentEcn == null) CurrentEcn = CurrentEcnEcoFollowUpDataContext.SelectedEcn;
                if (CurrentEcn != null)
                {
                    if (!_sapEcoService.OpenEcoInCC03(CurrentEcn.EcnEcoFollowUp.Ecn_Number))
                        MessageBox.Show(McgWpfTools.GetStringResource("EFU_WindowSapNotStarted"), McgWpfTools.GetStringResource("EFU_TitleSapIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteMenuItemOpenEcnDocs(EFU_EcnEcoToShowEndUser CurrentEcn = null)
        {
            try
            {
                if (CurrentEcn == null) CurrentEcn = CurrentEcnEcoFollowUpDataContext.SelectedEcn;
                if (CurrentEcn != null)
                {
                    if (!CurrentEcn.AlreadySearchAttachments)
                    {
                        CheckWindchillCredential();
                        List<RestOdataAttachment> CurrentListDoc = _windchillChangeManagementService.GetChangeNoticeattachments(WindchillNetworkCredential, CurrentEcn.EcnEcoFollowUp.Ecn_Number, CommonLibConstants.WindchillRestUrl);

                        if (CurrentListDoc != null && CurrentListDoc.Count > 0)
                        {
                            foreach (var doc in CurrentListDoc)
                            {
                                MenuItem newMenuItem = new MenuItem() { DataContext = this, Header = doc.Content.Label };
                                newMenuItem.Icon = new Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/doc_document.gif") };
                                newMenuItem.SetBinding(MenuItem.CommandProperty, new Binding("CommandOpenAttachment"));
                                newMenuItem.CommandParameter = doc;
                                CurrentEcn.MenuAttachments.Add(newMenuItem);
                            }
                            CurrentEcn.AlreadySearchAttachments = true;
                        }
                        else
                        {
                            CurrentEcn.AlreadySearchAttachments = false;
                            MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgDocNotFound"), McgWpfTools.GetStringResource("EFU_WTitleSearchDoc"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteMenutItemSearchEcnWfTask(EFU_EcnEcoToShowEndUser CurrentEcn = null)
        {
            try
            {
                if (CurrentEcn == null) CurrentEcn = CurrentEcnEcoFollowUpDataContext.SelectedEcn;
                if (CurrentEcn != null && CheckWindchillCredential())
                    ShowEcnWfTask(CurrentEcn);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteMenutItemSearchEcoWfTask(EFU_EcnEcoToShowEndUser CurrentEcn = null)
        {
            try
            {
                if (CurrentEcn == null) CurrentEcn = CurrentEcnEcoFollowUpDataContext.SelectedEcn;
                if (CurrentEcn != null)
                {
                    List<EFU_SapHupOracle_DmEcoTasks> ListAllTask = SearchSapHupDmEcoViewWfEcoTasks(CurrentEcn);

                    _ecnEcoFollowUpWindowService.ShowEcoWorkFlowTasksView(CurrentEcn, ListAllTask);
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteMenuItemOpenEcoDashboard(EFU_EcnEcoToShowEndUser CurrentEcn = null)
        {
            try
            {
                if (CurrentEcn == null) CurrentEcn = CurrentEcnEcoFollowUpDataContext.SelectedEcn;
                if (CurrentEcn != null)
                {
                    if (!_sapEcoService.OpenEcoDashboard(CurrentEcn.EcnEcoFollowUp.Ecn_Number))
                        MessageBox.Show(McgWpfTools.GetStringResource("EFU_WindowSapNotStarted"), McgWpfTools.GetStringResource("EFU_TitleSapIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRenameSearch(EFU_SearchTemplate eFU_SearchTemplate)
        {
            try
            {
                if (eFU_SearchTemplate != null)
                {
                    var dialogResult = _mcgCommonLibWindowService.ShowDialogMcgWindowOkCancel(McgWpfTools.GetStringResource("EFU_WTitleSaveSearch"),
                                                                        eFU_SearchTemplate.Name,
                                                                        500,
                                                                        100,
                                                                        15);

                    if (dialogResult.DialogValue == MessageBoxResult.OK)
                    {
                        if (!string.IsNullOrEmpty(dialogResult.Value))
                        {
                            eFU_SearchTemplate.Name = dialogResult.Value.Trim();
                            if (eFU_SearchTemplate.CompleteXmlFileName != null && eFU_SearchTemplate.CompleteXmlFileName.Trim() != "")
                                if (File.Exists(eFU_SearchTemplate.CompleteXmlFileName))
                                    File.Delete(eFU_SearchTemplate.CompleteXmlFileName);

                            string LastXmlFileName = ReturnLastSavedSearchFileNameAvailable();
                            _xmlSerializeTools.SerializedXmlInAppData<EFU_SearchTemplate>(eFU_SearchTemplate, LastXmlFileName);
                        }
                        else
                            MessageBox.Show(McgWpfTools.GetStringResource("EFU_MesgSaveSearchNameError"), McgWpfTools.GetStringResource("EFU_WTitleSaveSearchImpossible"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    CurrentEcnEcoFollowUpDataContext.RaiseSavedSearchesListEvent();
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateSearch(EFU_SearchTemplate eFU_SearchTemplate)
        {
            try
            {
                if (eFU_SearchTemplate != null)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgUpdateSearch"), McgWpfTools.GetStringResource("EFU_WTitleUpdateSearch"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        if (eFU_SearchTemplate.CompleteXmlFileName != null && eFU_SearchTemplate.CompleteXmlFileName.Trim() != "")
                        {
                            if (File.Exists(eFU_SearchTemplate.CompleteXmlFileName))
                                File.Delete(eFU_SearchTemplate.CompleteXmlFileName);

                            EFU_SearchTemplate tempSearch = GetCurrentSearch();
                            tempSearch.Name = eFU_SearchTemplate.Name;
                            tempSearch.CompleteXmlFileName = eFU_SearchTemplate.CompleteXmlFileName;
                            string LastXmlFileName = ReturnLastSavedSearchFileNameAvailable();
                            _xmlSerializeTools.SerializedXmlInAppData<EFU_SearchTemplate>(tempSearch, LastXmlFileName);
                            UpdateSavedSearchesList();
                            CurrentEcnEcoFollowUpDataContext.RaiseSavedSearchesListEvent();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDeleteSearch(EFU_SearchTemplate eFU_SearchTemplate)
        {
            try
            {
                if (eFU_SearchTemplate != null)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgDeleteSearch"), McgWpfTools.GetStringResource("EFU_WTitleDeleteSearch"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        if (eFU_SearchTemplate.CompleteXmlFileName != null && eFU_SearchTemplate.CompleteXmlFileName.Trim() != "")
                        {
                            if (File.Exists(eFU_SearchTemplate.CompleteXmlFileName))
                            {
                                File.Delete(eFU_SearchTemplate.CompleteXmlFileName);
                                UpdateSavedSearchesList();
                                CurrentEcnEcoFollowUpDataContext.RaiseSavedSearchesListEvent();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteExportSearch(EFU_SearchTemplate eFU_SearchTemplate)
        {
            try
            {
                if (eFU_SearchTemplate != null)
                {
                    ExecuteSavedOrRecentSearch(eFU_SearchTemplate);
                    ExecuteExportXls();
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenutItemAddEcnEcoToDashboard(EcnEcoFollowUpDashboardViewModel Dashboard = null)
        {
            try
            {
                if (CurrentEcnEcoFollowUpDataContext.SelectedEcn != null && Dashboard != null)
                    AddOneEcnEcoToDashboard(Dashboard, CurrentEcnEcoFollowUpDataContext.SelectedEcn.EcnEcoFollowUp.Ecn_Number);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenuItemSearchDashboard()
        {
            try
            {
                var (dialogResult, selectedDashboards) = _ecnEcoFollowUpWindowService.ShowDialogEcnEcoFollowUpDashboardSearchWindow();
                if (dialogResult == MessageBoxResult.OK)
                {
                    foreach (var item in selectedDashboards)
                        AddOneSearchedDashboard(item);
                    CurrentEcnEcoFollowUpDataContext.RaiseDashboardListEvent();
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDashBoardShow(EcnEcoFollowUpDashboardViewModel Dashboard = null)
        {
            try
            {
                if (Dashboard != null)
                {
                    Dashboard.DashboardItem.IsShown = true;
                    UpdateDashboardInDatabase(Dashboard.DashboardItem);
                    UpdateDashboardInformation(Dashboard.DashboardItem);

                    ApplyFilterDashboardListEcnEco(Dashboard.DashboardItem);

                    Dashboard.RaiseDashboardShowEvent();
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteDashBoardHide(EcnEcoFollowUpDashboardViewModel Dashboard = null)
        {
            try
            {
                if (Dashboard != null)
                {
                    Dashboard.DashboardItem.IsShown = false;
                    UpdateDashboardInDatabase(Dashboard.DashboardItem);
                    Dashboard.RaiseDashboardHideEvent();
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteDashBoardExport(EcnEcoFollowUpDashboardViewModel Dashboard = null)
        {
            try
            {
                if (Dashboard != null)
                {
                    ObservableCollection<EFU_EcnEcoToShowEndUser> ListEcnEco = new ObservableCollection<EFU_EcnEcoToShowEndUser>();
                    foreach (var item in Dashboard.DashboardItem.ListEcnEco)
                        ListEcnEco.Add(item.EcnEcoToShowEndUser);
                    ExportResultInExcel(ListEcnEco, Dashboard);
                }

            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteDashBoardRename(EcnEcoFollowUpDashboardViewModel Dashboard = null)
        {
            try
            {
                if (Dashboard != null)
                {
                    EFU_SearchTemplate currentSearch = GetCurrentSearch();

                    var dialogResult = _mcgCommonLibWindowService.ShowDialogMcgWindowOkCancel(McgWpfTools.GetStringResource("EFU_WTitleCreateDashboard"),
                                                                                            Dashboard.DashboardItem.Name,
                                                                                            500,
                                                                                            100,
                                                                                            15);

                    if (dialogResult.DialogValue == MessageBoxResult.OK)
                    {
                        if (!string.IsNullOrEmpty(dialogResult.Value))
                        {
                            Dashboard.DashboardItem.Name = dialogResult.Value.Trim();
                            UpdateDashboardInDatabase(Dashboard.DashboardItem);
                        }
                        else
                            MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgCreateDashboardIssueBlankName"), McgWpfTools.GetStringResource("EFU_WTitleCreateDashboardIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteDashBoardDelete(EcnEcoFollowUpDashboardViewModel Dashboard = null)
        {
            try
            {
                if (Dashboard != null)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgDeleteDashboard"), McgWpfTools.GetStringResource("EFU_WTitleDeleteDashboard"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        DeleteDashboard(Dashboard);
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteDashBoardRemove(EcnEcoFollowUpDashboardViewModel Dashboard = null)
        {
            try
            {
                if (Dashboard != null)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgRemoveDashboard"), McgWpfTools.GetStringResource("EFU_WTitleRemoveDashboard"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        RemoveDashboard(Dashboard);
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddSelectedEcnEcotoDashboard()
        {
            try
            {

                List<EFU_EcnEcoToShowEndUser> ListEcnEco = null;
                List<EFU_DashboardEcnEco> ListEcnEcoDashBoard = null;
                String FromName = "";

                if (CurrentEcnEcoFollowUpDataContext.SelectedTab != null && CurrentEcnEcoFollowUpDataContext.SelectedTab.DataContext != null && CurrentEcnEcoFollowUpDataContext.SelectedTab.Content != null)
                {
                    FromName = CurrentEcnEcoFollowUpDataContext.SelectedTab.Header.ToString();
                    var CurrentContent = CurrentEcnEcoFollowUpDataContext.SelectedTab.Content;
                    if (CurrentContent.GetType() == typeof(DockPanel))
                    {
                        ListEcnEco = CurrentEcnEcoFollowUpDataContext.EcnShownList.Where((item) => item.IsSelected).ToList();
                    }
                    else if (CurrentContent.GetType() == typeof(EcnEcoFollowUpDashboardView) && ((EcnEcoFollowUpDashboardView)CurrentContent).CurrentEcnEcoFollowUpDashboardViewModel != null)
                    {
                        ListEcnEcoDashBoard = ((EcnEcoFollowUpDashboardView)CurrentContent).CurrentEcnEcoFollowUpDashboardViewModel.DashboardItem.ListEcnEco.Where((item) => item.IsSelected).ToList();

                        ListEcnEco = new List<EFU_EcnEcoToShowEndUser>();
                        foreach (var item in ListEcnEcoDashBoard)
                            ListEcnEco.Add(item.EcnEcoToShowEndUser);
                    }
                }

                // ListEcnEco = CurrentEcnEcoFollowUpDataContext.EcnShownList.Where((item) => item.IsSelected).ToList();
                if (ListEcnEco != null && ListEcnEco.Count > 0)
                {
                    List<EcnEcoFollowUpDashboardViewModel> ListDashboardTemp = CurrentEcnEcoFollowUpDataContext.DashboardList.Where((item) => (item.DashboardItem.IsCreator || (!item.DashboardItem.IsCreator && !item.DashboardItem.IsReadOnly)) && item.DashboardItem.Name != FromName).OrderBy((item) => item.DashboardItem.Name).ToList();
                    if (ListDashboardTemp != null && ListDashboardTemp.Count > 0)
                    {
                        ObservableCollection<object> ListDasboard = new ObservableCollection<object>();
                        foreach (var item in ListDashboardTemp)
                            ListDasboard.Add(item);

                        // search last selected Dashboard 
                        EcnEcoFollowUpDashboardViewModel LatestSelectedDashboard = null;
                        if (CurrentEcnEcoFollowUpUserConfiguration != null)
                            LatestSelectedDashboard = (EcnEcoFollowUpDashboardViewModel)ListDasboard.FirstOrDefault((item) => ((EcnEcoFollowUpDashboardViewModel)item).DashboardItem.Id == CurrentEcnEcoFollowUpUserConfiguration.LatestSelectedDashboardId);

                        var (DialogValue, SelectedItem) = _mcgCommonLibWindowService.ShowDialogMcgWindowOkCancelComboBox(ListDasboard, LatestSelectedDashboard, string.Format(McgWpfTools.GetStringResource("EFU_LabelSendEcnToDashboard"), $"\"{FromName}\""), McgWpfTools.GetStringResource("EFU_MenuTitleDashboard"), 200);

                        //McgWindowOkCancelComboBox CurrentMcgWindowOkCancelComboBox = new McgWindowOkCancelComboBox(ListDasboard, LatestSelectedDashboard)
                        //{
                        //    Value = string.Format(McgWpfTools.GetStringResource("EFU_LabelSendEcnToDashboard"), $"\"{FromName}\""),
                        //    WindowTitle = McgWpfTools.GetStringResource("EFU_MenuTitleDashboard"),
                        //    WindowHeight = 200
                        //};
                        //CurrentMcgWindowOkCancelComboBox.ShowDialog();
                        if (DialogValue == MessageBoxResult.OK)
                        {
                            EcnEcoFollowUpDashboardViewModel SelectedDashboard = (EcnEcoFollowUpDashboardViewModel)SelectedItem;

                            ObservableCollection<EFU_EcnEcoToShowEndUser> CurrentListEcnEco = new ObservableCollection<EFU_EcnEcoToShowEndUser>();
                            foreach (var item in ListEcnEco)
                                CurrentListEcnEco.Add(item);
                            AddEcnEcoToDashboardFromSearch(SelectedDashboard, CurrentListEcnEco);

                            // Update User configuration
                            if (CurrentEcnEcoFollowUpUserConfiguration == null)
                            {
                                CurrentEcnEcoFollowUpUserConfiguration = new EcnEcoFollowUpUserConfiguration();
                                CurrentEcnEcoFollowUpUserConfiguration.UserConfigurationUpdateEvent += UpdateUserConfigXmlFile;
                            }
                            CurrentEcnEcoFollowUpUserConfiguration.LatestSelectedDashboardId = (SelectedDashboard).DashboardItem.Id;
                            CurrentEcnEcoFollowUpUserConfiguration.RaiseUserConfigurationUpdateEvent();

                        }
                    }
                    else
                        MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgCreateDashboard"), McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAdmToolDeleteEcnEco()
        {
            try
            {
                var dialogResult = _mcgCommonLibWindowService.ShowDialogMcgWindowOkCancel("Delete ECN");

                if (dialogResult.DialogValue == MessageBoxResult.OK)
                {
                    string EcnNumber = dialogResult.Value;

                    if (EcnNumber != null && EcnNumber.Trim() != "")
                    {
                        if (DeleteOneEcnEcoFollowUp(EcnNumber) == EFU_Status.OK)
                            MessageBox.Show(string.Format("ECN deleted: {0}.", EcnNumber.Trim().ToUpper()), "ECN Follow Up", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                        MessageBox.Show("Fill in an ECN/ECO Number and start again.", "ECN Follow Up", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAdmToolSeachDeletedEcn()
        {
            try
            {
                List<Ecnecofollowup> EFU_EcnEcoFollowUp_All = _ecnEcoFollowUpService.GetAllEcnEcoNotCreated();

                List<string> deletedEcnList = new List<string>();
                if (EFU_EcnEcoFollowUp_All != null)
                {
                    CheckWindchillCredential();
                    WindchillChangeNotice tempEcn;
                    foreach (var ecn in EFU_EcnEcoFollowUp_All)
                    {
                        tempEcn = _windchillReportingManagementService.GetQueryBuilderEcn(WindchillNetworkCredential, ecn.EcnNumber);
                        if (tempEcn == null)
                            deletedEcnList.Add(ecn.EcnNumber);
                    }

                    List<McgValueItem> listValues = new List<McgValueItem>();

                    foreach (var val in deletedEcnList)
                        listValues.Add(new McgValueItem() { Value = val });
                    _mcgCommonLibWindowService.ShowMcgWindowOkCancelListValue(listValues);
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckUncheckAllMain(bool IsChecked)
        {
            try
            {
                foreach (var item in CurrentEcnEcoFollowUpDataContext.EcnShownList)
                    item.IsSelected = IsChecked;
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenAttachment(RestOdataAttachment CurrentDoc)
        {
            try
            {
                if (CurrentDoc != null)
                    _windchillRequestMiscService.OpenAttachment(CurrentDoc, WindchillNetworkCredential);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Read nformation in SQL Server DataBase
        private void ReadPdmContextList()
        {
            try
            {
                var TempContextList = _ecnEcoFollowUpService.GetAllPdmContextName();
                CurrentEcnEcoFollowUpDataContext.PdmProductList.Clear();
                CurrentEcnEcoFollowUpDataContext.PdmProductList.Add("All");
                foreach (var item in TempContextList)
                    CurrentEcnEcoFollowUpDataContext.PdmProductList.Add(item);
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private EFU_EcnEcoToShowEndUser SearchOneEcnEcoFollowUp()
        {
            try
            {
                EFU_EcnEcoToShowEndUser returnEcn = null;

                returnEcn = new EFU_EcnEcoToShowEndUser();
                var CurrentItem = _ecnEcoFollowUpService.GetOneEcnEcoFollowUp(CurrentEcnEcoFollowUpDataContext.EcnNumber);

                if (CurrentItem != null)
                    returnEcn.EcnEcoFollowUp = EFU_EcnEcoFollowUp.GetEFU_EcnEcoFollowUp(CurrentItem);

                if (returnEcn.EcnEcoFollowUp == null)
                    returnEcn = null;

                return returnEcn;
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private List<EFU_EcnEcoToShowEndUser> MainSearchEcnEcoFollowUp()
        {
            try
            {
                List<Ecnecofollowup> EFU_EcnEcoFollowUp_All = _ecnEcoFollowUpService.GetAllEcnEco();

                List<EFU_EcnEcoToShowEndUser> EcnList = new List<EFU_EcnEcoToShowEndUser>();
                EFU_EcnEcoToShowEndUser CurrentEcn;
                List<Ecnecofollowup> TempList;

                // Two Ecn State Filters to manage the case if "In progress" and "Under review" selection, meaning "Implementation" or "Open" in PDM
                Regex EcnStateFilter = new Regex(CurrentEcnEcoFollowUpDataContext.EcnState);
                Regex EcnStateFilter2 = new Regex(CurrentEcnEcoFollowUpDataContext.EcnState);
                if (CurrentEcnEcoFollowUpDataContext.EcnState == "All")
                    EcnStateFilter = new Regex(".*");

                if (CurrentEcnEcoFollowUpDataContext.EcnState == "In progress" || CurrentEcnEcoFollowUpDataContext.EcnState == "Under review")
                {
                    EcnStateFilter = new Regex("Implementation");
                    EcnStateFilter2 = new Regex("Open");
                }

                // Filter Product
                Regex ProductFilter = new Regex($"^{CurrentEcnEcoFollowUpDataContext.PdmProduct.Trim()}$");
                if (CurrentEcnEcoFollowUpDataContext.PdmProduct.Trim() == "All" || CurrentEcnEcoFollowUpDataContext.PdmProduct.Trim() == "")
                    ProductFilter = new Regex(".*");

                // Filter SAP Status
                List<string> ListSapFilter = new List<string>();
                if (CurrentEcnEcoFollowUpDataContext.IsStatusNotCreated)
                    ListSapFilter.Add("Not Created");

                if (CurrentEcnEcoFollowUpDataContext.IsStatus99)
                    ListSapFilter.Add("99");

                if (CurrentEcnEcoFollowUpDataContext.IsStatus01)
                    ListSapFilter.Add("01");

                if (CurrentEcnEcoFollowUpDataContext.IsStatus02)
                    ListSapFilter.Add("02");

                if (CurrentEcnEcoFollowUpDataContext.IsStatus03)
                    ListSapFilter.Add("03");

                // request to manage the case if "In progress" and "Under review" selection, meaning that ECN could be "Implementation" or "Open" in PDM
                TempList = EFU_EcnEcoFollowUp_All.Where((item) => item.EcnState != null
                        && (EcnStateFilter.IsMatch(item.EcnState) || EcnStateFilter2.IsMatch(item.EcnState))
                        && item.PdmProduct != null && ProductFilter.IsMatch(item.PdmProduct)
                        && ListSapFilter.Contains(item.EcoStatus.Trim())).OrderBy((item) => item.EcnNumber).ToList();

                // Apply Ecn State filter "Under review"
                if (CurrentEcnEcoFollowUpDataContext.EcnState == "Under review")
                    TempList = TempList.Where((item) => item.DesignerStartAppDate != null
                                && item.DesignerStartAppDate.ToString().Trim() != "").OrderBy((item) => item.EcnNumber).ToList();

                // Apply Created After Filter
                if (CurrentEcnEcoFollowUpDataContext.CreatedAfter != null && CurrentEcnEcoFollowUpDataContext.CreatedAfter.ToString().Trim() != "")
                    TempList = TempList.Where((item) => item.EcnCreatedOn.Value.ToDateTime(TimeOnly.MinValue) >= CurrentEcnEcoFollowUpDataContext.CreatedAfter.Value).OrderBy((item) => item.EcnNumber).ToList();
                // Apply Created Before Filter
                if (CurrentEcnEcoFollowUpDataContext.CreatedBefore != null && CurrentEcnEcoFollowUpDataContext.CreatedBefore.ToString().Trim() != "")
                    TempList = TempList.Where((item) => item.EcnCreatedOn.Value.ToDateTime(TimeOnly.MinValue) <= CurrentEcnEcoFollowUpDataContext.CreatedBefore).OrderBy((item) => item.EcnNumber).ToList();

                // Apply Resolved After Filter
                if (CurrentEcnEcoFollowUpDataContext.ResolvedAfter != null && CurrentEcnEcoFollowUpDataContext.ResolvedAfter.ToString().Trim() != "")
                    TempList = TempList.Where((item) => item.CaiiiApprovalDate.Value.ToDateTime(TimeOnly.MinValue) >= CurrentEcnEcoFollowUpDataContext.ResolvedAfter
                                && item.EcnState == "Resolved").OrderBy((item) => item.EcnNumber).ToList();

                // Apply Resolved Before Filter
                if (CurrentEcnEcoFollowUpDataContext.ResolvedBefore != null && CurrentEcnEcoFollowUpDataContext.ResolvedBefore.ToString().Trim() != "")
                    TempList = TempList.Where((item) => item.CaiiiApprovalDate.Value.ToDateTime(TimeOnly.MinValue) <= CurrentEcnEcoFollowUpDataContext.ResolvedBefore
                                && item.EcnState == "Resolved").OrderBy((item) => item.EcnNumber).ToList();

                // Apply SAP Created After Filter
                if (CurrentEcnEcoFollowUpDataContext.CreatedAfterSap != null && CurrentEcnEcoFollowUpDataContext.CreatedAfterSap.ToString().Trim() != "")
                    TempList = TempList.Where((item) => item.EcoCreatedOn.Value.ToDateTime(TimeOnly.MinValue) >= CurrentEcnEcoFollowUpDataContext.CreatedAfterSap).OrderBy((item) => item.EcnNumber).ToList();
                // Apply SAP Created Before Filter
                if (CurrentEcnEcoFollowUpDataContext.CreatedBeforeSap != null && CurrentEcnEcoFollowUpDataContext.CreatedBeforeSap.ToString().Trim() != "")
                    TempList = TempList.Where((item) => item.EcoCreatedOn.Value.ToDateTime(TimeOnly.MinValue) <= CurrentEcnEcoFollowUpDataContext.CreatedBeforeSap).OrderBy((item) => item.EcnNumber).ToList();

                // Apply KeyWords description filter and KeyWords created by filter
                if (CurrentEcnEcoFollowUpDataContext.KeyWords == null)
                    CurrentEcnEcoFollowUpDataContext.KeyWords = "";
                var ListOfRegexDescription = _regExTools.GetRegexList(CurrentEcnEcoFollowUpDataContext.KeyWords.Trim(), true);
                if (CurrentEcnEcoFollowUpDataContext.EcnCreator == null)
                    CurrentEcnEcoFollowUpDataContext.EcnCreator = "";
                var ListOfRegexEcnCreator = _regExTools.GetRegexList(CurrentEcnEcoFollowUpDataContext.EcnCreator.Trim(), true);
                if (CurrentEcnEcoFollowUpDataContext.KeyWordsProject == null)
                    CurrentEcnEcoFollowUpDataContext.KeyWordsProject = "";
                var ListOfRegexSapPrject = _regExTools.GetRegexList(CurrentEcnEcoFollowUpDataContext.KeyWordsProject.Trim(), true);
                if (CurrentEcnEcoFollowUpDataContext.KeyWordsCategory == null)
                    CurrentEcnEcoFollowUpDataContext.KeyWordsCategory = "";
                var ListOfRegexSapCategory = _regExTools.GetRegexList(CurrentEcnEcoFollowUpDataContext.KeyWordsCategory.Trim(), true);

                foreach (var ecn in TempList)
                    if (_regExTools.CheckStringWithRegExList(ecn.EcnName, ListOfRegexDescription)
                        && _regExTools.CheckStringWithRegExList(ecn.EcnCreatedBy, ListOfRegexEcnCreator)
                        && _regExTools.CheckStringWithRegExList(ecn.EcoProject, ListOfRegexSapPrject)
                        && _regExTools.CheckStringWithRegExList(ecn.EcoCateg, ListOfRegexSapCategory))
                    {
                        CurrentEcn = new EFU_EcnEcoToShowEndUser() { EcnEcoFollowUp = EFU_EcnEcoFollowUp.GetEFU_EcnEcoFollowUp(ecn) };
                        EcnList.Add(CurrentEcn);
                    }

                return EcnList;
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private EFU_Status DeleteOneEcnEcoFollowUp(string pEcnNumber)
        {
            try
            {
                _ecnEcoFollowUpService.DeleteEcnEcoFollowUp(pEcnNumber);
                _ecnEcoFollowUpService.DeleteEcnEcoDashboardDetail(pEcnNumber);

                return EFU_Status.OK;
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().FullName, ex);
            }
        }
        #endregion

        #region [REGION] Methods Windchill Search
        private bool CheckWindchillCredential()
        {
            try
            {
                if (WindchillNetworkCredential == null)
                {
                    WindchillCredentialItem WindchillCredential = _windchillCredentialService.GetWindchillCredential(CommonLibConstants.WindchillUrl, CommonLibConstants.WindchillUrl);
                    if (!WindchillCredential.IsCredentialOk) return false;
                    WindchillNetworkCredential = WindchillCredential.WindchillCredential;
                    //WindchillNetworkCredential = new NetworkCredential();
                    //WindchillNetworkCredential.UserName = WindchillCredential.Login;
                    //WindchillNetworkCredential.Password = WindchillCredential.PassWord;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }

        }

        private void ShowEcnWfTask(EFU_EcnEcoToShowEndUser CurrentEcn)
        {
            try
            {

                var ListWfTasks = SearchWfTaskOneEcn(CurrentEcn.EcnEcoFollowUp.Ecn_Number);

                if (ListWfTasks != null && ListWfTasks.Count > 0)
                {
                    _ecnEcoFollowUpWindowService.ShowDialogEcnEcaWorkFlowTasksView(CurrentEcn, ListWfTasks);
                }
                else
                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("EFU_MsgSearchEcnWfTaskNotFound"), CurrentEcn.EcnEcoFollowUp.Ecn_Number), McgWpfTools.GetStringResource("EFU_WTitleSearchEcnWfTaskNotFound"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private List<EFU_EcnEcoWorkflowItem> SearchWfTaskOneEcn(string pEcnNumber)
        {
            try
            {
                // Search workflow Tasks for the different ECA and ECN 
                var AllEcaWfTask = _windchillRequestTool.GetQueryBuilderEcaWorkflowTask(WindchillNetworkCredential, pEcnNumber, CommonLibConstants.WindchillRestUrl);
                var AllEcnWfTask = _windchillRequestTool.GetQueryBuilderEcnWorkflowTask(WindchillNetworkCredential, pEcnNumber, CommonLibConstants.WindchillRestUrl);
                //var AllEcaWfTask = WindchillRestOdataTool.GetQueryBuilderEcaWorkflowTask<WindchillObjectWorkflowTask>(WindchillNetworkCredential, pEcnNumber, CommonLibConstants.WindchillUrlQuery);
                //var AllEcnWfTask = WindchillRestOdataTool.GetQueryBuilderEcnWorkflowTask<WindchillObjectWorkflowTask>(WindchillNetworkCredential, pEcnNumber, CommonLibConstants.WindchillUrlQuery);
                List<EFU_EcnEcoWorkflowItem> ListWfTask = new List<EFU_EcnEcoWorkflowItem>();
                foreach (var WfTask in AllEcnWfTask.Union(AllEcaWfTask))
                    ListWfTask.Add(EFU_EcnEcoWorkflowItem.GetEFU_EcnEcoWorkflowItem(WfTask));

                return ListWfTask;
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Search Information in Oracle DataBase SAP_HUP
        private List<EFU_SapHupOracle_DmEcoTasks> SearchSapHupDmEcoViewWfEcoTasks(EFU_EcnEcoToShowEndUser pEco)
        {
            try
            {
                List<EFU_SapHupOracle_DmEcoTasks> pEcoTaskList = new List<EFU_SapHupOracle_DmEcoTasks>();

                EFU_SapHupOracle_DmEcoTasks CurrentSapHupOracle_DmEcoTasks = null;


                var listWf = _sapHupService.GetEcoWfTaskData(pEco.EcnEcoFollowUp.Ecn_Number);
                foreach (var WfTask in listWf)
                {
                    CurrentSapHupOracle_DmEcoTasks = new EFU_SapHupOracle_DmEcoTasks() { ECO = pEco.EcnEcoFollowUp.Ecn_Number };

                    CurrentSapHupOracle_DmEcoTasks.CALCULATED_PLANT_DESC = WfTask.CalculatedPlantDesc;
                    CurrentSapHupOracle_DmEcoTasks.CALCULATED_PLANT = WfTask.CalculatedPlant;
                    CurrentSapHupOracle_DmEcoTasks.ECO_COORD = WfTask.EcoCoord;
                    CurrentSapHupOracle_DmEcoTasks.ECO_COORD_DESC = WfTask.EcoCoordDesc;
                    CurrentSapHupOracle_DmEcoTasks.TYPE_ITEM = WfTask.TypeItem;
                    CurrentSapHupOracle_DmEcoTasks.WI_ACTUAL_AGENT = WfTask.WiActualAgent;
                    CurrentSapHupOracle_DmEcoTasks.WI_CREATION_DATE = WfTask.WiCreationDate;
                    CurrentSapHupOracle_DmEcoTasks.WI_END_DATE = WfTask.WiEndDate;
                    CurrentSapHupOracle_DmEcoTasks.WI_STATUS = WfTask.WiStatus;
                    CurrentSapHupOracle_DmEcoTasks.WI_TEXT = WfTask.WiText;

                    if (CurrentSapHupOracle_DmEcoTasks.WI_STATUS != "COMPLETED")
                        CurrentSapHupOracle_DmEcoTasks.WI_END_DATE = null;

                    pEcoTaskList.Add(CurrentSapHupOracle_DmEcoTasks);
                }

                //OracleCmd.CommandText = string.Format(McgMiscTools.GetAppSetting(this, "SapHupSqlEcoWfTask"), pEco.EcnEcoFollowUp.Ecn_Number);
                //while (OracleDr.Read())
                //{
                //    CurrentSapHupOracle_DmEcoTasks = new EFU_SapHupOracle_DmEcoTasks() { ECO = pEco.EcnEcoFollowUp.Ecn_Number };
                //    CurrentSapHupOracle_DmEcoTasks.CALCULATED_PLANT_DESC = GetStringColumnItem(OracleDr, "CALCULATED_PLANT_DESC");
                //    CurrentSapHupOracle_DmEcoTasks.CALCULATED_PLANT = GetStringColumnItem(OracleDr, "CALCULATED_PLANT");
                //    CurrentSapHupOracle_DmEcoTasks.ECO_COORD = GetStringColumnItem(OracleDr, "ECO_COORD");
                //    CurrentSapHupOracle_DmEcoTasks.ECO_COORD_DESC = GetStringColumnItem(OracleDr, "ECO_COORD_DESC");
                //    CurrentSapHupOracle_DmEcoTasks.TYPE_ITEM = GetStringColumnItem(OracleDr, "TYPE_ITEM");
                //    CurrentSapHupOracle_DmEcoTasks.WI_ACTUAL_AGENT = GetStringColumnItem(OracleDr, "WI_ACTUAL_AGENT");
                //    CurrentSapHupOracle_DmEcoTasks.WI_CREATION_DATE = GetDateColumnItem(OracleDr, "WI_CREATION_DATE");
                //    CurrentSapHupOracle_DmEcoTasks.WI_END_DATE = GetDateColumnItem(OracleDr, "WI_END_DATE");
                //    CurrentSapHupOracle_DmEcoTasks.WI_STATUS = GetStringColumnItem(OracleDr, "WI_STATUS");
                //    CurrentSapHupOracle_DmEcoTasks.WI_TEXT = GetStringColumnItem(OracleDr, "WI_TEXT");
                //    if (CurrentSapHupOracle_DmEcoTasks.WI_STATUS != "COMPLETED")
                //        CurrentSapHupOracle_DmEcoTasks.WI_END_DATE = null;
                //    pEcoTaskList.Add(CurrentSapHupOracle_DmEcoTasks);
                //}

                return pEcoTaskList;
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private string GetStringColumnItem(OracleDataReader OracleDr, string pItem)
        {
            try
            {
                return (string)OracleDr[pItem];
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static double GetDoubleColumnItem(OracleDataReader OracleDr, string pItem)
        {
            try
            {
                return Convert.ToDouble(OracleDr[pItem]);
            }
            catch (Exception)
            {
                return 0.0;
            }
        }

        private static int GetIntegerColumnItem(OracleDataReader OracleDr, string pItem)
        {
            try
            {
                if (OracleDr[pItem] != null)
                {
                    double currentNum = (short)OracleDr[pItem];
                    return Convert.ToInt32(currentNum);
                }
                else
                    return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static DateTime? GetDateColumnItem(OracleDataReader OracleDr, string pItem)
        {
            try
            {
                if (OracleDr[pItem] != null)
                    return (DateTime)OracleDr[pItem];
                else
                    return default(DateTime?);
            }
            catch (Exception)
            {
                return default(DateTime?);
            }
        }
        #endregion

        #region [REGION] Methods for Recent and saved searches
        private void UpdateRecentSearchesList()
        {
            try
            {
                string XmlFileName;
                string CompleteXmlFile;
                EFU_SearchTemplate NewRecentSearch;
                CurrentEcnEcoFollowUpDataContext.RecentSearchesList.Clear();

                for (int Index = 1; Index <= EcnEcoFollowUpConstants.MaxiRecentSearch; Index++)
                {
                    XmlFileName = String.Format(EcnEcoFollowUpConstants.RecentSearchTemplateFileName, Index);
                    CompleteXmlFile = $"{SaveSearchFolder}\\{XmlFileName}";

                    if (File.Exists(CompleteXmlFile))
                    {
                        NewRecentSearch = _xmlSerializeTools.GetDeserializedXmlFromAppData<EFU_SearchTemplate>(XmlFileName);
                        if (NewRecentSearch != null)
                            CurrentEcnEcoFollowUpDataContext.RecentSearchesList.Add(NewRecentSearch);
                    }
                }
                CurrentEcnEcoFollowUpDataContext.RaiseRecentSearchesListEvent();
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateSavedSearchesList()
        {
            try
            {
                string XmlFileName;
                string CompleteXmlFile;
                EFU_SearchTemplate NewSavedSearch;

                List<EFU_SearchTemplate> TempListSearch = new List<EFU_SearchTemplate>();
                for (int Index = 1; Index <= EcnEcoFollowUpConstants.MaxiSavedSearch; Index++)
                {
                    XmlFileName = String.Format(EcnEcoFollowUpConstants.SavedSearchTemplateFileName, Index);
                    CompleteXmlFile = $"{SaveSearchFolder}\\{XmlFileName}";
                    if (File.Exists(CompleteXmlFile))
                    {
                        NewSavedSearch = _xmlSerializeTools.GetDeserializedXmlFromAppData<EFU_SearchTemplate>(XmlFileName);
                        if (NewSavedSearch != null)
                        {
                            NewSavedSearch.CompleteXmlFileName = CompleteXmlFile;
                            TempListSearch.Add(NewSavedSearch);
                        }
                    }
                }

                CurrentEcnEcoFollowUpDataContext.SavedSearchesList.Clear();
                CurrentEcnEcoFollowUpDataContext.SavedSearchesList.AddRange(TempListSearch.OrderBy((search) => search.Name));

                CurrentEcnEcoFollowUpDataContext.RaiseSavedSearchesListEvent();
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void SaveRecentSearch()
        {
            try
            {
                _xmlSerializeTools.SerializedXmlInAppData<EFU_SearchTemplate>(GetCurrentSearch(), ReturnLastRecentSearchFileNameAvailable());
                UpdateRecentSearchesList();
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private EFU_SearchTemplate GetCurrentSearch()
        {
            try
            {
                string TempCreatedOnAfter = null;
                string TempCreatedOnBefore = null;
                string TempResolvedOnAfter = null;
                string TempResolvedOnBefore = null;
                string TempEcnCreator = null;
                string TempEcnNumber = null;
                string TempKeyWords = null;

                if (CurrentEcnEcoFollowUpDataContext.CreatedAfter != null && CurrentEcnEcoFollowUpDataContext.CreatedAfter.ToString().Trim() != "")
                    TempCreatedOnAfter = CurrentEcnEcoFollowUpDataContext.CreatedAfter.Value.ToString("dd-MM-yyyy");
                if (CurrentEcnEcoFollowUpDataContext.CreatedBefore != null && CurrentEcnEcoFollowUpDataContext.CreatedBefore.ToString().Trim() != "")
                    TempCreatedOnBefore = CurrentEcnEcoFollowUpDataContext.CreatedBefore.Value.ToString("dd-MM-yyyy");
                if (CurrentEcnEcoFollowUpDataContext.ResolvedAfter != null && CurrentEcnEcoFollowUpDataContext.ResolvedAfter.ToString().Trim() != "")
                    TempResolvedOnAfter = CurrentEcnEcoFollowUpDataContext.ResolvedAfter.Value.ToString("dd-MM-yyyy");
                if (CurrentEcnEcoFollowUpDataContext.ResolvedBefore != null && CurrentEcnEcoFollowUpDataContext.ResolvedBefore.ToString().Trim() != "")
                    TempResolvedOnBefore = CurrentEcnEcoFollowUpDataContext.ResolvedBefore.Value.ToString("dd-MM-yyyy");

                if (CurrentEcnEcoFollowUpDataContext.EcnCreator != null && CurrentEcnEcoFollowUpDataContext.EcnCreator.Trim() != "")
                    TempEcnCreator = CurrentEcnEcoFollowUpDataContext.EcnCreator.Trim();
                if (CurrentEcnEcoFollowUpDataContext.EcnNumber != null && CurrentEcnEcoFollowUpDataContext.EcnNumber.Trim() != "")
                    TempEcnNumber = CurrentEcnEcoFollowUpDataContext.EcnNumber.Trim();
                if (CurrentEcnEcoFollowUpDataContext.KeyWords != null && CurrentEcnEcoFollowUpDataContext.KeyWords.Trim() != "")
                    TempKeyWords = CurrentEcnEcoFollowUpDataContext.KeyWords.Trim();

                EFU_SearchTemplate NewRecentSearch = new EFU_SearchTemplate()
                {
                    CreatedOnAfter = TempCreatedOnAfter,
                    CreatedOnBefore = TempCreatedOnBefore,
                    Creator = TempEcnCreator,
                    EcnNumber = TempEcnNumber,
                    EcnState = CurrentEcnEcoFollowUpDataContext.EcnState,
                    IsStatusNotCreated = CurrentEcnEcoFollowUpDataContext.IsStatusNotCreated,
                    IsStatus99 = CurrentEcnEcoFollowUpDataContext.IsStatus99,
                    IsStatus03 = CurrentEcnEcoFollowUpDataContext.IsStatus03,
                    IsStatus02 = CurrentEcnEcoFollowUpDataContext.IsStatus02,
                    IsStatus01 = CurrentEcnEcoFollowUpDataContext.IsStatus01,
                    KeyWords = TempKeyWords,
                    Product = CurrentEcnEcoFollowUpDataContext.PdmProduct,
                    ResolvedOnAfter = TempResolvedOnAfter,
                    ResolvedOnBefore = TempResolvedOnBefore
                };

                UpdateSearchName(NewRecentSearch);
                return NewRecentSearch;

            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateSearchName(EFU_SearchTemplate pSearch)
        {
            try
            {
                string EcnNum;
                string Product;
                string EcnState;
                string CreatedOnAfter;
                string CreatedOnBefore;
                string ResolvedOnAfter;
                string ResolvedOnBefore;
                string Creator;
                string KeyWords;

                if (pSearch.EcnNumber != null && pSearch.EcnNumber.Trim() != "")
                    EcnNum = pSearch.EcnNumber.Trim();
                else
                    EcnNum = "*";
                if (pSearch.Product != null && pSearch.Product.Trim() != "All")
                    Product = pSearch.Product.Trim();
                else
                    Product = "*";
                if (pSearch.EcnState != null && pSearch.EcnState.Trim() != "All")
                    EcnState = pSearch.EcnState.Trim();
                else
                    EcnState = "*";
                if (pSearch.CreatedOnAfter != null && pSearch.CreatedOnAfter.ToString().Trim() != "")
                    CreatedOnAfter = pSearch.CreatedOnAfter.ToString().Trim();
                else
                    CreatedOnAfter = "*";
                if (pSearch.CreatedOnBefore != null && pSearch.CreatedOnBefore.ToString().Trim() != "")
                    CreatedOnBefore = pSearch.CreatedOnBefore.ToString().Trim();
                else
                    CreatedOnBefore = "*";
                if (pSearch.ResolvedOnAfter != null && pSearch.ResolvedOnAfter.ToString().Trim() != "")
                    ResolvedOnAfter = pSearch.ResolvedOnAfter.ToString().Trim();
                else
                    ResolvedOnAfter = "*";
                if (pSearch.ResolvedOnBefore != null && pSearch.ResolvedOnBefore.ToString().Trim() != "")
                    ResolvedOnBefore = pSearch.ResolvedOnBefore.ToString().Trim();
                else
                    ResolvedOnBefore = "*";
                if (pSearch.Creator != null && pSearch.Creator.Trim() != "")
                    Creator = pSearch.Creator;
                else
                    Creator = "*";
                if (pSearch.KeyWords != null && pSearch.KeyWords.Trim() != "")
                    KeyWords = pSearch.KeyWords;
                else
                    KeyWords = "*";

                pSearch.Name = $"ECN={EcnNum};Product={Product};EcnState={EcnState};CreatedOn={CreatedOnAfter}/{CreatedOnBefore};ResolvedOn={ResolvedOnAfter}/{ResolvedOnBefore};Creator={Creator};KeyWords={KeyWords}";
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().FullName, ex);
            }
        }

        private string ReturnLastRecentSearchFileNameAvailable()
        {
            try
            {
                string XmlFileName;
                string CompleteXmlFile;
                string newXmlFileName;
                string newCompleteXmlFile;
                int MaxIndex = EcnEcoFollowUpConstants.MaxiRecentSearch - 1;

                // Return the next available file
                for (int index = 1; index <= EcnEcoFollowUpConstants.MaxiRecentSearch; index++)
                {
                    XmlFileName = string.Format(EcnEcoFollowUpConstants.RecentSearchTemplateFileName, index);
                    CompleteXmlFile = $"{SaveSearchFolder}\\{XmlFileName}";
                    if (!File.Exists(CompleteXmlFile))
                    {
                        MaxIndex = (index - 1);
                        break;
                    }
                }

                // If rename file 1 to 2, 2 to 3... until max-1 to max
                // The old max will be deleted and return the first one  
                for (int indexRen = MaxIndex; indexRen >= 1; indexRen--)
                {
                    XmlFileName = string.Format(EcnEcoFollowUpConstants.RecentSearchTemplateFileName, indexRen);
                    newXmlFileName = string.Format(EcnEcoFollowUpConstants.RecentSearchTemplateFileName, (indexRen + 1));
                    CompleteXmlFile = $"{SaveSearchFolder}\\{XmlFileName}";
                    newCompleteXmlFile = $"{SaveSearchFolder}\\{newXmlFileName}";
                    if (File.Exists(newCompleteXmlFile))
                    {
                        File.Delete(newCompleteXmlFile);
                    }

                    File.Move(CompleteXmlFile, newCompleteXmlFile);
                }

                return String.Format(EcnEcoFollowUpConstants.RecentSearchTemplateFileName, 1);

            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private string ReturnLastSavedSearchFileNameAvailable()
        {
            try
            {
                string CompleteXmlFile;
                string XmlFileName;

                // Return the next available file
                for (int index = 1; index <= EcnEcoFollowUpConstants.MaxiSavedSearch; index++)
                {
                    XmlFileName = string.Format(EcnEcoFollowUpConstants.SavedSearchTemplateFileName, index);
                    CompleteXmlFile = $@"{SaveSearchFolder}\{XmlFileName}";
                    if (!File.Exists(CompleteXmlFile))
                    {
                        return XmlFileName;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateSearchFieldFromSavedSearch(EFU_SearchTemplate currentSearch)
        {
            try
            {
                DateTime? CreatedOnAfter = null;
                DateTime? CreatedOnBefore = null;
                DateTime? ResolvedOnAfter = null;
                DateTime? ResolvedOnBefore = null;
                EFU_Date tempEFU_Date = null;

                if (currentSearch.CreatedOnAfter != null && currentSearch.CreatedOnAfter.Trim() != "")
                {
                    tempEFU_Date = new EFU_Date();
                    tempEFU_Date.StandardDate = currentSearch.CreatedOnAfter.Trim();
                    CreatedOnAfter = tempEFU_Date.GetDate();
                }

                if (currentSearch.CreatedOnBefore != null && currentSearch.CreatedOnBefore.Trim() != "")
                {
                    tempEFU_Date = new EFU_Date();
                    tempEFU_Date.StandardDate = currentSearch.CreatedOnBefore.Trim();
                    CreatedOnBefore = tempEFU_Date.GetDate();
                }

                if (currentSearch.ResolvedOnAfter != null && currentSearch.ResolvedOnAfter.Trim() != "")
                {
                    tempEFU_Date = new EFU_Date();
                    tempEFU_Date.StandardDate = currentSearch.ResolvedOnAfter.Trim();
                    ResolvedOnAfter = tempEFU_Date.GetDate();
                }

                if (currentSearch.ResolvedOnBefore != null && currentSearch.ResolvedOnBefore.Trim() != "")
                {
                    tempEFU_Date = new EFU_Date();
                    tempEFU_Date.StandardDate = currentSearch.ResolvedOnBefore.Trim();
                    ResolvedOnBefore = tempEFU_Date.GetDate();
                }

                CurrentEcnEcoFollowUpDataContext.EcnNumber = currentSearch.EcnNumber;
                CurrentEcnEcoFollowUpDataContext.CreatedAfter = CreatedOnAfter;
                CurrentEcnEcoFollowUpDataContext.CreatedBefore = CreatedOnBefore;
                CurrentEcnEcoFollowUpDataContext.ResolvedAfter = ResolvedOnAfter;
                CurrentEcnEcoFollowUpDataContext.ResolvedBefore = ResolvedOnBefore;
                CurrentEcnEcoFollowUpDataContext.PdmProduct = currentSearch.Product;
                CurrentEcnEcoFollowUpDataContext.EcnState = currentSearch.EcnState;
                CurrentEcnEcoFollowUpDataContext.KeyWords = currentSearch.KeyWords;
                CurrentEcnEcoFollowUpDataContext.EcnCreator = currentSearch.Creator;

                CurrentEcnEcoFollowUpDataContext.IsStatusNotCreated = currentSearch.IsStatusNotCreated;
                CurrentEcnEcoFollowUpDataContext.IsStatus99 = currentSearch.IsStatus99;
                CurrentEcnEcoFollowUpDataContext.IsStatus03 = currentSearch.IsStatus03;
                CurrentEcnEcoFollowUpDataContext.IsStatus02 = currentSearch.IsStatus02;
                CurrentEcnEcoFollowUpDataContext.IsStatus01 = currentSearch.IsStatus01;
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods To export in Excel
        private void ExportResultInExcel(ObservableCollection<EFU_EcnEcoToShowEndUser> ListEcnEco = null, EcnEcoFollowUpDashboardViewModel Dashboard = null)
        {
            if (ListEcnEco == null) ListEcnEco = CurrentEcnEcoFollowUpDataContext.EcnShownList;
            if (ListEcnEco == null) return;
            ObservableCollection<EFU_DashboardEcnEco> ListDashboardEcnEco = null;
            if (Dashboard != null)
                ListDashboardEcnEco = Dashboard.DashboardItem.ListEcnEco;

            ExcelToolsClosedXml aExcelToolEpPlus = null;
            try
            {
                Random generator = new Random();
                int indexFile = generator.Next(1, 10000);
                string TempXlsFile = $@"{System.Environment.GetEnvironmentVariable("TEMP")}\{EcnEcoFollowUpConstants.XlsExportDefaultFileName.Replace(".", $"_{indexFile}.")}";

                // Open Template Xls File
                string resourcesFolder = CommonLibConstants.ResourcesFolder;
                string templateExportFile = EcnEcoFollowUpConstants.TemplateExportResultXls;
                string templateFileName = Path.Combine(MainAppFolder, resourcesFolder, templateExportFile);
                aExcelToolEpPlus = new ExcelToolsClosedXml() { CompleteFileName = TempXlsFile, CompleteTemplateFileName = templateFileName };

                //string templateFileName = $@"{MainAppFolder}\{McgMiscTools.GetAppSetting(this, "ResourcesFolder")}\{McgMiscTools.GetAppSetting(this, "TemplateExportResultXls")}";
                //aExcelToolEpPlus = new ExcelToolsClosedXml() { CompleteFileName = TempXlsFile, CompleteTemplateFileName = templateFileName };
                aExcelToolEpPlus.OpenFile(templateFileName);


                // Update the MAIN tab
                aExcelToolEpPlus.CurrentSheet = EcnEcoFollowUpConstants.XlsResultMainTab;
                aExcelToolEpPlus.SetCellValue("Export ECN/ECO Follow Up", 1, 1);
                aExcelToolEpPlus.SetCellValue($"Export Date: {DateTime.Now.ToString("dd/MM/yyyy")}", 2, 3);

                // Update the ECN/ECO Info Tab tab
                //aExcelToolEpPlus.CurrentSheet = EcnEcoFollowUpConstants.XlsResultEcnEcoInfoTab;
                int index = 2;

                foreach (var EcnEco in ListEcnEco)
                {
                    aExcelToolEpPlus.CurrentSheet = EcnEcoFollowUpConstants.XlsResultEcnEcoInfoTab;
                    // add new Line
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Ecn_Number, index + 4, 2, EcnEcoFollowUpConstants.XlsResultMainTab);

                    // add new Line
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Ecn_Number, index, 1, EcnEcoFollowUpConstants.XlsResultStatTab);

                    // add new line
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Ecn_Number, index, 2);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Ecn_Name, index, 3);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Ecn_Description, index, 4);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Ecn_State, index, 5);

                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Ecn_Created_On, index, 6);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Designer_Start_App_Date, index, 7);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.First_Approval_Date, index, 8);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Qual_Check_Approval_Date, index, 9);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.CAIII_Approval_Date, index, 10);

                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.CaIII_Name, index, 11);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Pdm_Product, index, 12);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Pdm_Context, index, 13);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Nb_Part, index, 14);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Nb_Epm_Doc, index, 15);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Nb_Drw, index, 16);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Nb_Wt_Doc, index, 17);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Pdm_Ecn_Id, index, 18);

                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Eco_Created_On, index, 19);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Eco_Wf_Started_On, index, 20);

                    if (EcnEco.EcnEcoFollowUp.Eco_Closed_On != null)
                        aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Eco_Closed_On, index, 21);

                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Eco_Effectivity_Date, index, 22);

                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Eco_Status, index, 23);
                    aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Ecn_Creator_Name.ToString(), index, 24);

                    if (EcnEco.EcnEcoFollowUp.Pdm_Update_Status != null)
                        aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Pdm_Update_Status, index, 25);

                    if (EcnEco.EcnEcoFollowUp.Sap_Update_Status != null)
                        aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Sap_Update_Status, index, 26);

                    if (EcnEco.EcnEcoFollowUp.Eco_Categ != null)
                        aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Eco_Categ, index, 27);

                    if (EcnEco.EcnEcoFollowUp.Eco_Project != null)
                        aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Eco_Project, index, 28);
                    if (EcnEco.EcnEcoFollowUp.Eco_Sub_Line != null)
                        aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Eco_Sub_Line, index, 29);
                    if (EcnEco.EcnEcoFollowUp.Eco_Tmlpse_Wi_Close != null)
                        aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Eco_Tmlpse_Wi_Close.Value, index, 30);
                    else
                        aExcelToolEpPlus.SetCellValue(0, index, 30);

                    if (EcnEco.EcnEcoFollowUp.Eco_Next_Step != null)
                        aExcelToolEpPlus.SetCellValue(EcnEco.EcnEcoFollowUp.Eco_Next_Step, index, 31);

                    // update information from DashBoard if required
                    if (ListDashboardEcnEco != null)
                    {
                        EFU_DashboardEcnEco CurrentDashboardEcnEco = ListDashboardEcnEco.FirstOrDefault((item) => item.EcnEcoToShowEndUser.EcnEcoFollowUp.Ecn_Number == EcnEco.EcnEcoFollowUp.Ecn_Number);
                        if (CurrentDashboardEcnEco != null)
                        {
                            aExcelToolEpPlus.CurrentSheet = EcnEcoFollowUpConstants.XlsResultMainTab;
                            if (CurrentDashboardEcnEco.Department != null)
                                aExcelToolEpPlus.SetCellValue(CurrentDashboardEcnEco.Department, index + 4, 1);
                            if (CurrentDashboardEcnEco.Comment != null)
                                aExcelToolEpPlus.SetCellValue(CurrentDashboardEcnEco.Comment, index + 4, 7);
                            if (CurrentDashboardEcnEco.Information != null)
                                aExcelToolEpPlus.SetCellValue(CurrentDashboardEcnEco.Information, index + 4, 10);
                            if (CurrentDashboardEcnEco.SapOrder != null)
                                aExcelToolEpPlus.SetCellValue(CurrentDashboardEcnEco.SapOrder, index + 4, 15);
                        }
                    }
                    index++;
                }
                aExcelToolEpPlus.SaveClose();

                McgFileAndSystemTools.OpenFile(TempXlsFile);
            }
            catch (Exception ex)
            {
                aExcelToolEpPlus.SaveClose();
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods to manage Dashboards
        private void CreateDashboard(string DashboardName, bool IsEmpty = true)
        {
            try
            {
                // Create new empty Dashboard 
                Ecnecodashboard CurrentDashboard = new Ecnecodashboard()
                {
                    Dashboardname = DashboardName,
                    Createdon = DateOnly.FromDateTime(DateTime.Today),
                    Isactive = true,
                    Isshown = "TRUE",
                    Isreadonly = true,
                    Isshared = true,
                    Createdby = Environment.UserName,
                    Createdbyfullname = LoggedUser.DisplayName,
                    Dashboardid = 0,
                    Generalcomment = ""
                };
                _ecnEcoFollowUpService.CreateDashboard(CurrentDashboard);

                // Create entry in ECNECODASHBOARD_USER to show the dashboard by default
                EcnecodashboardUser CurrentDashboardUser = new EcnecodashboardUser()
                {
                    Id = 0,
                    Dashboardid = CurrentDashboard.Dashboardid,
                    Userid = CurrentDashboard.Createdby,
                    Isshown = true,
                    Showcancelled = false,
                    Showinprogress = true,
                    Showresolved = false,
                    Showstatus01 = true,
                    Showstatus02 = false,
                    Showstatus03 = false,
                    Showstatus99 = true,
                    Showunderriview = true,
                    Columnsorder = ""
                };
                _ecnEcoFollowUpService.CreateDashboardUser(CurrentDashboardUser);

                // if not empty, add ECN/ECO from the main search
                if (!IsEmpty)
                {
                    if (CurrentEcnEcoFollowUpDataContext.EcnShownList != null)
                    {
                        int CurrentID = _ecnEcoFollowUpService.GetEcnEcoDetailLatestId() + 1;

                        foreach (var item in CurrentEcnEcoFollowUpDataContext.EcnShownList)
                        {
                            _ecnEcoFollowUpService.CreateEcnEcoDashboardDetail(new EcnecodashboardDetail()
                            {
                                Dashboardid = CurrentDashboard.Dashboardid,
                                Ecnecocomment = "-",
                                Ecnecodepartment = "TEXE",
                                Ecneconumber = item.EcnEcoFollowUp.Ecn_Number,
                                Id = CurrentID,
                                Ecnecopriority = "None"
                            });
                            CurrentID++;
                        }
                    }
                }

                AddOneDashboard(CurrentDashboard, CurrentDashboardUser);
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void DeleteDashboard(EcnEcoFollowUpDashboardViewModel Dashboard)
        {
            try
            {
                Dashboard.DashboardItem.IsActive = false;
                Dashboard.DashboardItem.IsShown = false;
                Dashboard.DashboardItem.DeactivatedOn = DateTime.Today;
                UpdateDashboardInDatabase(Dashboard.DashboardItem);
                CurrentEcnEcoFollowUpDataContext.DashboardList.Remove(Dashboard);
                CurrentEcnEcoFollowUpDataContext.RaiseDashboardListEvent();
                Dashboard.RaiseDashboardHideEvent();
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void RemoveDashboard(EcnEcoFollowUpDashboardViewModel dashboard)
        {
            try
            {
                if (dashboard != null)
                {
                    int nbDeleted = _ecnEcoFollowUpService.DeleteEcnEcoDashboardUser(Environment.UserName, dashboard.DashboardItem.EcnEcoDashboard.Dashboardid);
                    if (nbDeleted > 0)
                    {
                        dashboard.DashboardItem.IsShown = false;
                        CurrentEcnEcoFollowUpDataContext.DashboardList.Remove(dashboard);
                        CurrentEcnEcoFollowUpDataContext.RaiseDashboardListEvent();
                        dashboard.RaiseDashboardHideEvent();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void SearchActiveDashboard()
        {
            try
            {
                var ListDashboard = _ecnEcoFollowUpService.GetDashboardsForCurrentUser(Environment.UserName);

                //(from user in CreoEntities.ECNECODASHBOARD_USER
                //                     from dashboard in CreoEntities.ECNECODASHBOARD
                //                     where user.USERID == Environment.UserName
                //                     && user.DASHBOARDID == dashboard.DASHBOARDID
                //                     && (dashboard.CREATEDBY == Environment.UserName || (dashboard.CREATEDBY != Environment.UserName && dashboard.ISSHARED.Value))
                //                     orderby dashboard.DASHBOARDNAME
                //                     select new { Duser = user, Ddash = dashboard }).ToList();

                CurrentEcnEcoFollowUpDataContext.DashboardList.Clear();
                foreach (var item in ListDashboard)
                    AddOneDashboard(item.Dashboard, item.User);
                //AddOneDashboard(CreoEntities.ECNECODASHBOARD.FirstOrDefault((dashb) => dashb.DASHBOARDID == item.Duser.DASHBOARDID), item.Duser);

                CurrentEcnEcoFollowUpDataContext.RaiseDashboardListEvent();
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void AddOneSearchedDashboard(EFU_DashboardItem item)
        {
            try
            {

                int IdNum = Convert.ToInt32(item.Id);

                // check if dashboard not already shown
                var (dashboard, dashboardUser) = _ecnEcoFollowUpService.EnsureDashboardUserExists(IdNum);

                if (dashboard != null && dashboardUser != null)
                {
                    AddOneDashboard(dashboard, dashboardUser);
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgDashboardAlreadyAdded", new string[1] { item.Name }), McgWpfTools.GetStringResource("EFU_TitleSearchDashboard"), MessageBoxButton.OK, MessageBoxImage.Warning);

                //// check if dashboard not already shown
                //var CurrentDashboardUser = CreoEntities.ECNECODASHBOARD_USER.FirstOrDefault((dashb) => dashb.DASHBOARDID == IdNum && dashb.USERID == Environment.UserName);

                //if (CurrentDashboardUser == null)
                //{
                //    var CurrentDashboard = CreoEntities.ECNECODASHBOARD.FirstOrDefault((dashb) => dashb.DASHBOARDID == IdNum);

                //    if (CurrentDashboard != null)
                //    {
                //        // Create entry in ECNECODASHBOARD_USER to show the dashboard by default
                //        CurrentDashboardUser = new ECNECODASHBOARD_USER()
                //        {
                //            ID = CreoEntities.ECNECODASHBOARD_USER.Max((id) => id.ID) + 1,
                //            DASHBOARDID = CurrentDashboard.DASHBOARDID,
                //            USERID = Environment.UserName,
                //            ISSHOWN = true,
                //            SHOWCANCELLED = false,
                //            SHOWINPROGRESS = true,
                //            SHOWRESOLVED = false,
                //            SHOWSTATUS01 = true,
                //            SHOWSTATUS02 = false,
                //            SHOWSTATUS03 = false,
                //            SHOWSTATUS99 = true,
                //            SHOWUNDERRIVIEW = true,
                //            COLUMNSORDER = ""
                //        };
                //        CreoEntities.ECNECODASHBOARD_USER.Add(CurrentDashboardUser);
                //        CreoEntities.SaveChanges();

                //        AddOneDashboard(CurrentDashboard, CurrentDashboardUser);
                //    }
                //}
                //else
                //    MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgDashboardAlreadyAdded", new string[1] { item.Name }), McgWpfTools.GetStringResource("EFU_TitleSearchDashboard"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void AddOneDashboard(Ecnecodashboard CurrentDashboard, EcnecodashboardUser CurrentDashboardUser)
        {
            try
            {
                if (CurrentDashboard != null)
                {
                    EFU_DashboardItem DashboardItem;
                    DashboardItem = new EFU_DashboardItem()
                    {
                        IsActive = true,
                        IsShown = CurrentDashboardUser.Isshown.Value,
                        IsShared = CurrentDashboard.Isshared.Value,
                        IsReadOnly = CurrentDashboard.Isreadonly.Value,
                        IsCreator = CurrentDashboard.Createdby == CurrentDashboardUser.Userid,
                        Name = CurrentDashboard.Dashboardname,
                        CreatedBy = CurrentDashboard.Createdbyfullname,
                        CreatedOn = CurrentDashboard.Createdon.Value.ToDateTime(TimeOnly.MinValue),
                        EcnEcoDashboard = CurrentDashboard,
                        ParentApp = this,
                        Id = CurrentDashboard.Dashboardid.ToString("000000"),
                        GeneralComment = CurrentDashboard.Generalcomment
                    };

                    DashboardItem.CurrentDashboardConfiguration.Parent = DashboardItem;
                    DashboardItem.CurrentDashboardConfiguration.IsInProgress = CurrentDashboardUser.Showinprogress.Value;
                    DashboardItem.CurrentDashboardConfiguration.IsUnderReview = CurrentDashboardUser.Showunderriview.Value;
                    DashboardItem.CurrentDashboardConfiguration.IsResolved = CurrentDashboardUser.Showresolved.Value;
                    DashboardItem.CurrentDashboardConfiguration.IsCanceled = CurrentDashboardUser.Showcancelled.Value;
                    DashboardItem.CurrentDashboardConfiguration.IsStatus99 = CurrentDashboardUser.Showstatus99.Value;
                    DashboardItem.CurrentDashboardConfiguration.IsStatus01 = CurrentDashboardUser.Showstatus01.Value;
                    DashboardItem.CurrentDashboardConfiguration.IsStatus02 = CurrentDashboardUser.Showstatus02.Value;
                    DashboardItem.CurrentDashboardConfiguration.IsStatus03 = CurrentDashboardUser.Showstatus03.Value;

                    DashboardItem.CurrentDashboardConfiguration.UpdateColumnsOrder(CurrentDashboardUser.Columnsorder);

                    if (DashboardItem.IsShown) UpdateDashboardInformation(DashboardItem);

                    CurrentEcnEcoFollowUpDataContext.DashboardList.Add(_ecnEcoFollowUpWindowService.GetEcnEcoFollowUpDashboardViewModel(DashboardItem, this));

                    DashboardItem.CurrentDashboardConfiguration.IsUpdateFilterEvent += UpdateDashboardInDatabaseEvent;
                    DashboardItem.CurrentDashboardConfiguration.IsUpdateFilterEvent += ApplyFilterDashboardListEcnEco;
                    DashboardItem.CurrentDashboardConfiguration.IsUpdateColumsOrderUserEvent += UpdateDashboardInDatabaseEventAsynch;
                    DashboardItem.IsDashboardUpdateEvent += UpdateDashboardInDatabaseEvent;
                }
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateDashboardInDatabaseEvent(object sender, EventArgs e)
        {
            try
            {
                if (sender != null)
                    UpdateDashboardInDatabase((EFU_DashboardItem)sender);
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateDashboardInDatabaseEventAsynch(object sender, EventArgs e)
        {
            try
            {
                if (sender != null)
                {
                    Thread ThreadSearchPart = new Thread(() => UpdateDashboardInDatabase((EFU_DashboardItem)sender));
                    ThreadSearchPart.Start();
                }
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateDashboardInDatabase(EFU_DashboardItem dashboardItem)
        {
            try
            {


                if (dashboardItem is null)
                    throw new ArgumentNullException(nameof(dashboardItem));

                var dashboardId = dashboardItem.EcnEcoDashboard.Dashboardid;
                var userId = Environment.UserName;

                var currentDashboard = _ecnEcoFollowUpService.GetDashboardById(dashboardId);
                if (currentDashboard is null)
                    return;


                currentDashboard.Dashboardname = dashboardItem.Name;
                currentDashboard.Isactive = dashboardItem.IsActive;
                currentDashboard.Deactivatedon = DateOnly.FromDateTime(dashboardItem.DeactivatedOn ?? DateTime.MinValue);
                currentDashboard.Isreadonly = dashboardItem.IsReadOnly;
                currentDashboard.Isshared = dashboardItem.IsShared;
                currentDashboard.Generalcomment = dashboardItem.GeneralComment;


                if (!dashboardItem.IsActive)
                {
                    _ecnEcoFollowUpService.DeleteAllDashboardUsers(dashboardId);
                }
                else
                {
                    var currentDashboardUser = _ecnEcoFollowUpService.GetDashboardUserForUpdate(dashboardId, userId);
                    if (currentDashboardUser != null)
                    {
                        currentDashboardUser.Isshown = dashboardItem.IsShown;
                        currentDashboardUser.Showcancelled = dashboardItem.CurrentDashboardConfiguration.IsCanceled;
                        currentDashboardUser.Showinprogress = dashboardItem.CurrentDashboardConfiguration.IsInProgress;
                        currentDashboardUser.Showresolved = dashboardItem.CurrentDashboardConfiguration.IsResolved;
                        currentDashboardUser.Showstatus01 = dashboardItem.CurrentDashboardConfiguration.IsStatus01;
                        currentDashboardUser.Showstatus02 = dashboardItem.CurrentDashboardConfiguration.IsStatus02;
                        currentDashboardUser.Showstatus03 = dashboardItem.CurrentDashboardConfiguration.IsStatus03;
                        currentDashboardUser.Showstatus99 = dashboardItem.CurrentDashboardConfiguration.IsStatus99;
                        currentDashboardUser.Showunderriview = dashboardItem.CurrentDashboardConfiguration.IsUnderReview;
                        currentDashboardUser.Columnsorder = dashboardItem.CurrentDashboardConfiguration.ColumnsOrderStr;
                    }
                }

                _ecnEcoFollowUpService.SaveChanges();

                //using (EcnEcoFollowUpDataBaseEntities CreoEntities = GetDataBaseEntity(true))
                //{
                //    var CurrentDashboard = CreoEntities.ECNECODASHBOARD.FirstOrDefault((item) => item.DASHBOARDID == dashboardItem.EcnEcoDashboard.DASHBOARDID);
                //    if (CurrentDashboard != null)
                //    {
                //        CurrentDashboard.DASHBOARDNAME = dashboardItem.Name;
                //        CurrentDashboard.ISACTIVE = dashboardItem.IsActive;
                //        CurrentDashboard.DEACTIVATEDON = dashboardItem.DeactivatedOn;
                //        CurrentDashboard.ISREADONLY = dashboardItem.IsReadOnly;
                //        CurrentDashboard.ISSHARED = dashboardItem.IsShared;
                //        CurrentDashboard.GENERALCOMMENT = dashboardItem.GeneralComment;

                //        //CurrentDashboard.ISSHOWN = DashboardItem.IsShown.ToString().ToUpper();

                //        // Remove all user references to this dashboard if inactive
                //        if (!dashboardItem.IsActive)
                //        {
                //            var ListDashboardUser = CreoEntities.ECNECODASHBOARD_USER.Where((item) => item.DASHBOARDID == CurrentDashboard.DASHBOARDID).ToList();
                //            foreach (var item in ListDashboardUser)
                //                CreoEntities.ECNECODASHBOARD_USER.Remove(item);
                //        }
                //        else
                //        {
                //            var CurrentDashboardUser = CreoEntities.ECNECODASHBOARD_USER.FirstOrDefault((item) => item.DASHBOARDID == dashboardItem.EcnEcoDashboard.DASHBOARDID
                //                                                                                                  && item.USERID == Environment.UserName);
                //            if (CurrentDashboardUser != null)
                //            {
                //                CurrentDashboardUser.ISSHOWN = dashboardItem.IsShown;
                //                CurrentDashboardUser.SHOWCANCELLED = dashboardItem.CurrentDashboardConfiguration.IsCanceled;
                //                CurrentDashboardUser.SHOWINPROGRESS = dashboardItem.CurrentDashboardConfiguration.IsInProgress;
                //                CurrentDashboardUser.SHOWRESOLVED = dashboardItem.CurrentDashboardConfiguration.IsResolved;
                //                CurrentDashboardUser.SHOWSTATUS01 = dashboardItem.CurrentDashboardConfiguration.IsStatus01;
                //                CurrentDashboardUser.SHOWSTATUS02 = dashboardItem.CurrentDashboardConfiguration.IsStatus02;
                //                CurrentDashboardUser.SHOWSTATUS03 = dashboardItem.CurrentDashboardConfiguration.IsStatus03;
                //                CurrentDashboardUser.SHOWSTATUS99 = dashboardItem.CurrentDashboardConfiguration.IsStatus99;
                //                CurrentDashboardUser.SHOWUNDERRIVIEW = dashboardItem.CurrentDashboardConfiguration.IsUnderReview;
                //                CurrentDashboardUser.COLUMNSORDER = dashboardItem.CurrentDashboardConfiguration.ColumnsOrderStr;
                //            }
                //        }
                //    }
                //    CreoEntities.SaveChanges();
                //}
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        public void UpdateDashboardInformation(EFU_DashboardItem dashboardItem)
        {
            try
            {
                if (dashboardItem != null && dashboardItem.EcnEcoDashboard != null)
                {
                    int dashboardId = Convert.ToInt32(dashboardItem.Id);

                    var currentDashboard = _ecnEcoFollowUpService.GetDashboardById(dashboardId);

                    dashboardItem.RowListEcnEco.Clear();

                    if (currentDashboard != null)
                    {
                        dashboardItem.IsShared = currentDashboard.Isshared == true;
                        dashboardItem.IsReadOnly = currentDashboard.Isreadonly == true;
                    }

                    foreach (var ecnEco in _ecnEcoFollowUpService.GetDashboardDetailsByDashboardId(dashboardItem.EcnEcoDashboard.Dashboardid))
                        UpdateOneEcnEcoDashboardInformation(dashboardItem, ecnEco);

                    dashboardItem.RowListEcnEco = dashboardItem.RowListEcnEco.OrderBy(x => x.Priority).ToList();

                    ApplyFilterDashboardListEcnEco(dashboardItem);
                }
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateOneEcnEcoDashboardInformation(EFU_DashboardItem DashboardItem, EcnecodashboardDetail CurrentECNECODASHBOARD_DETAIL)
        {
            try
            {
                EFU_DashboardEcnEco CurrentEcnEco = new EFU_DashboardEcnEco()
                {
                    Information = CurrentECNECODASHBOARD_DETAIL.Encecoinformation,
                    Comment = CurrentECNECODASHBOARD_DETAIL.Ecnecocomment,
                    Department = CurrentECNECODASHBOARD_DETAIL.Ecnecodepartment,
                    SapOrder = CurrentECNECODASHBOARD_DETAIL.Ecnecosaporder,
                    Priority = CurrentECNECODASHBOARD_DETAIL.Ecnecopriority,
                    EcnEcoDasboardDetail = CurrentECNECODASHBOARD_DETAIL,
                    EcnEcoToShowEndUser = new EFU_EcnEcoToShowEndUser()
                    {
                        EcnEcoFollowUp = EFU_EcnEcoFollowUp.GetEFU_EcnEcoFollowUp(_ecnEcoFollowUpService.GetOneEcnEcoFollowUp(CurrentECNECODASHBOARD_DETAIL.Ecneconumber))
                    }
                };

                CurrentEcnEco.IsUpdateEvent += UpdateOneEcnEcoFromDashboardToDatabase;
                CurrentEcnEco.UpdateEcoTimeResolution();
                CurrentEcnEco.UpdateApprovalEcnStep();

                DashboardItem.RowListEcnEco.Add(CurrentEcnEco);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AddOneEcnEcoToDashboard(EcnEcoFollowUpDashboardViewModel currentDashboard, string ecnNumber)
        {
            try
            {
                if (currentDashboard == null)
                    throw new ArgumentNullException(nameof(currentDashboard));

                if (string.IsNullOrWhiteSpace(ecnNumber))
                    throw new ArgumentException("ECN number is required.", nameof(ecnNumber));

                int dashboardId = currentDashboard.DashboardItem.EcnEcoDashboard.Dashboardid;

                // 1) check if ECN already in the dashboard
                var existingDetail = _ecnEcoFollowUpService.GetDashboardDetail(ecnNumber, dashboardId);
                if (existingDetail != null)
                {
                    MessageBox.Show(
                        McgWpfTools.GetStringResource("EFU_MsgAddOneEcnEcoToDashboardAlreadyAdded", new[] { ecnNumber }),
                        McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // 2) check if ECN exists in follow-up table
                var currentEcnEco = _ecnEcoFollowUpService.GetOneEcnEcoFollowUp(ecnNumber);
                if (currentEcnEco == null)
                {
                    MessageBox.Show(
                        McgWpfTools.GetStringResource("EFU_MsgAddOneEcnEcoToDashboardNotFound", new[] { ecnNumber }),
                        McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // 3) create new dashboard detail
                int nextId = _ecnEcoFollowUpService.GetNextDashboardDetailId();

                var newDetail = new EcnecodashboardDetail
                {
                    Id = nextId,
                    Dashboardid = dashboardId,
                    Ecneconumber = ecnNumber,

                    Ecnecocomment = "",
                    Ecnecodepartment = "",
                    Ecnecopriority = "None",
                    Ecnecosaporder = "",
                    Encecoinformation = ""
                };

                // 4) insert + save
                _ecnEcoFollowUpService.CreateEcnEcoDashboardDetail(newDetail);

                // 5) update business/UI
                UpdateOneEcnEcoDashboardInformation(currentDashboard.DashboardItem, newDetail);
                ApplyFilterDashboardListEcnEco(currentDashboard.DashboardItem);
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        public void AddOneEcnEcoToDashboard(EcnEcoFollowUpDashboardViewModel currentDashboard, EFU_EcnEcoCopyPaste ecn)
        {
            try
            {
                if (currentDashboard == null)
                    throw new ArgumentNullException(nameof(currentDashboard));

                if (ecn == null)
                    throw new ArgumentNullException(nameof(ecn));

                int dashboardId = currentDashboard.DashboardItem.EcnEcoDashboard.Dashboardid;


                // 1) check if ECN already in the dashboard
                var existingDetail = _ecnEcoFollowUpService.GetDashboardDetail(ecn.EcnEcoNumber, dashboardId);
                if (existingDetail != null)
                {
                    MessageBox.Show(
                        McgWpfTools.GetStringResource("EFU_MsgAddOneEcnEcoToDashboardAlreadyAdded", new[] { ecn.EcnEcoNumber }),
                        McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // 2) check if ECN exists in follow-up table
                var currentEcnEco = _ecnEcoFollowUpService.GetOneEcnEcoFollowUp(ecn.EcnEcoNumber);
                if (currentEcnEco == null)
                {
                    MessageBox.Show(
                        McgWpfTools.GetStringResource("EFU_MsgAddOneEcnEcoToDashboardNotFound", new[] { ecn.EcnEcoNumber }),
                        McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // 3) create new dashboard detail
                int nextId = _ecnEcoFollowUpService.GetNextDashboardDetailId();

                var newDetail = new EcnecodashboardDetail
                {
                    Id = nextId,
                    Dashboardid = dashboardId,
                    Ecneconumber = ecn.EcnEcoNumber,
                    Ecnecocomment = ecn.Comment,
                    Ecnecodepartment = "",
                    Ecnecosaporder = ecn.SapOrder,
                    Encecoinformation = ecn.Information
                };
                if (ecn.Priority != null && (new Regex("^1$|^2$|^3$").IsMatch(ecn.Priority)))
                    newDetail.Ecnecopriority = ecn.Priority;
                else
                    newDetail.Ecnecopriority = "None";

                // 4) insert + save
                _ecnEcoFollowUpService.CreateEcnEcoDashboardDetail(newDetail);

                // 5) update business/UI
                UpdateOneEcnEcoDashboardInformation(currentDashboard.DashboardItem, newDetail);
                ApplyFilterDashboardListEcnEco(currentDashboard.DashboardItem);


                //using (EcnEcoFollowUpDataBaseEntities CreoEntities = GetDataBaseEntity(true))
                //{
                //    // check if ECN already in the dashboard
                //    EcnecodashboardDetail CurrentECNECODASHBOARD_DETAIL = CreoEntities.ECNECODASHBOARD_DETAIL.FirstOrDefault((item) => item.ECNECONUMBER == ecn.EcnEcoNumber && item.DASHBOARDID == currentDashboard.DashboardItem.EcnEcoDashboard.Dashboardid);

                //    if (CurrentECNECODASHBOARD_DETAIL == null)
                //    {
                //        Ecnecofollowup CurrentEcnEco = CreoEntities.ECNECOFOLLOWUP.FirstOrDefault((item) => item.ECN_NUMBER == ecn.EcnEcoNumber);
                //        if (CurrentEcnEco != null)
                //        {
                //            CurrentECNECODASHBOARD_DETAIL = new EcnecodashboardDetail()
                //            {
                //                Dashboardid = currentDashboard.DashboardItem.EcnEcoDashboard.Dashboardid,
                //                Ecnecocomment = ecn.Comment,
                //                Ecnecodepartment = "",
                //                Ecnecosaporder = ecn.SapOrder,
                //                Encecoinformation = ecn.Information,
                //                Ecneconumber = ecn.EcnEcoNumber,
                //                Id = CreoEntities.ECNECODASHBOARD_DETAIL.Max((item) => item.ID) + 1
                //            };
                //            if (ecn.Priority != null && (new Regex("^1$|^2$|^3$").IsMatch(ecn.Priority)))
                //                CurrentECNECODASHBOARD_DETAIL.Ecnecopriority = ecn.Priority;
                //            else
                //                CurrentECNECODASHBOARD_DETAIL.Ecnecopriority = "None";

                //            CreoEntities.ECNECODASHBOARD_DETAIL.Add(CurrentECNECODASHBOARD_DETAIL);

                //            CreoEntities.SaveChanges();
                //            UpdateOneEcnEcoDashboardInformation(currentDashboard.DashboardItem, CurrentECNECODASHBOARD_DETAIL);
                //            ApplyFilterDashboardListEcnEco(currentDashboard.DashboardItem);
                //        }
                //        else
                //            MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgAddOneEcnEcoToDashboardNotFound", new string[1] { ecn.EcnEcoNumber }), McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco"), MessageBoxButton.OK, MessageBoxImage.Warning);
                //    }
                //    else
                //        MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgAddOneEcnEcoToDashboardAlreadyAdded", new string[1] { ecn.EcnEcoNumber }), McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco"), MessageBoxButton.OK, MessageBoxImage.Warning);
                //}
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        public void AddEcnEcoToDashboardFromSearch(EcnEcoFollowUpDashboardViewModel currentDashboard, ObservableCollection<EFU_EcnEcoToShowEndUser> currentListEcnEco = null)
        {
            try
            {
                if (currentListEcnEco == null) currentListEcnEco = CurrentEcnEcoFollowUpDataContext.EcnShownList;

                if (currentListEcnEco == null || currentListEcnEco.Count == 0)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgAddEcnEcoToDashboardFromSearchNone"), McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Ecnecofollowup CurrentEcnEco;
                EcnecodashboardDetail CurrentEcnEcoDetail;

                int NbEcnEcoAdded = 0;
                int NbEcnEcoNotAdded = 0;

                int IdDb = _ecnEcoFollowUpService.GetNextDashboardDetailId();
                int dashboardId = currentDashboard.DashboardItem.EcnEcoDashboard.Dashboardid;


                //int IdDb = CreoEntities.ECNECODASHBOARD_DETAIL.Max((item) => item.ID) + 1;


                foreach (var ecnEco in currentListEcnEco)
                {
                    CurrentEcnEco = _ecnEcoFollowUpService.GetOneEcnEcoFollowUp(ecnEco.EcnEcoFollowUp.Ecn_Number);
                    CurrentEcnEcoDetail = _ecnEcoFollowUpService.GetDashboardDetail(ecnEco.EcnEcoFollowUp.Ecn_Number, dashboardId);

                    if (CurrentEcnEco != null && CurrentEcnEcoDetail == null)
                    {
                        CurrentEcnEcoDetail = new EcnecodashboardDetail()
                        {
                            Dashboardid = currentDashboard.DashboardItem.EcnEcoDashboard.Dashboardid,
                            Ecnecocomment = "",
                            Ecnecodepartment = "",
                            Ecnecopriority = "None",
                            Ecneconumber = ecnEco.EcnEcoFollowUp.Ecn_Number,
                            Id = IdDb
                        };

                        _ecnEcoFollowUpService.CreateEcnEcoDashboardDetail(CurrentEcnEcoDetail);

                        UpdateOneEcnEcoDashboardInformation(currentDashboard.DashboardItem, CurrentEcnEcoDetail);

                        NbEcnEcoAdded++;
                        IdDb++;
                    }
                    else
                        NbEcnEcoNotAdded++;
                }


                ApplyFilterDashboardListEcnEco(currentDashboard.DashboardItem);

                MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgAddEcnEcoToDashboardFromSearchResult", new string[2] { NbEcnEcoAdded.ToString(), NbEcnEcoNotAdded.ToString() }), McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        public void RemoveOneEcnEcoFromDashboard(EcnEcoFollowUpDashboardViewModel CurrentDashboard, EFU_DashboardEcnEco DashboardEcnEco)
        {
            try
            {
                int nbDeleted = _ecnEcoFollowUpService.DeleteDashboardDetail(DashboardEcnEco.EcnEcoToShowEndUser.EcnEcoFollowUp.Ecn_Number, CurrentDashboard.DashboardItem.EcnEcoDashboard.Dashboardid);
                if (nbDeleted > 0)
                {
                    CurrentDashboard.DashboardItem.ListEcnEco.Remove(DashboardEcnEco);
                    CurrentDashboard.DashboardItem.RowListEcnEco.Remove(DashboardEcnEco);
                }
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateOneEcnEcoFromDashboardToDatabase(object sender, EventArgs e)
        {
            try
            {
                EcnecodashboardDetail senderEcnEcoDetail = ((EFU_DashboardEcnEco)sender).EcnEcoDasboardDetail;
                EcnecodashboardDetail currentEcnEcoDetail = _ecnEcoFollowUpService.GetDashboardDetail(senderEcnEcoDetail.Ecneconumber, senderEcnEcoDetail.Dashboardid.Value);
                if (currentEcnEcoDetail != null)
                {
                    currentEcnEcoDetail.Ecnecocomment = ((EFU_DashboardEcnEco)sender).Comment;
                    currentEcnEcoDetail.Ecnecodepartment = ((EFU_DashboardEcnEco)sender).Department;
                    currentEcnEcoDetail.Ecnecopriority = ((EFU_DashboardEcnEco)sender).Priority;
                    currentEcnEcoDetail.Encecoinformation = ((EFU_DashboardEcnEco)sender).Information;
                    currentEcnEcoDetail.Ecnecosaporder = ((EFU_DashboardEcnEco)sender).SapOrder;
                }
                _ecnEcoFollowUpService.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdatePersonalDashboardEcnEco(EFU_DashboardItem DashboardItem)
        {
            try
            {
                if (DashboardItem != null)
                {
                    DashboardItem.ListEcnEco.Clear();
                    var ListEcnEco = _ecnEcoFollowUpService.GetEcnEcoFollowupsForUser(LoggedUser.GivenName.ToUpper(), LoggedUser.Surname.ToUpper().Trim(), Environment.UserName.ToUpper());
                    foreach (var item in ListEcnEco)
                        UpdateOneEcnEcoDashboardInformation(DashboardItem, new EcnecodashboardDetail() { Ecneconumber = item.EcnNumber });
                    ApplyFilterDashboardListEcnEco(DashboardItem);
                }
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void ApplyFilterDashboardListEcnEco(object sender, EventArgs e = null)
        {
            try
            {
                EFU_DashboardItem DashboardItem = (EFU_DashboardItem)sender;

                // Filter SAP Status
                List<string> ListSapFilter = new List<string>();

                if (DashboardItem.CurrentDashboardConfiguration.IsStatusNotCreated)
                    ListSapFilter.Add("ECOTOBECREATED");

                if (DashboardItem.CurrentDashboardConfiguration.IsStatus99)
                    ListSapFilter.Add("ECOSTATUS99");

                if (DashboardItem.CurrentDashboardConfiguration.IsStatus01)
                {
                    ListSapFilter.Add("ECOSTATUS01");
                    ListSapFilter.Add("ECOSTATUS01_6MONTHS");
                }

                if (DashboardItem.CurrentDashboardConfiguration.IsStatus02)
                    ListSapFilter.Add("ECOSTATUS02");

                if (DashboardItem.CurrentDashboardConfiguration.IsStatus03)
                    ListSapFilter.Add("ECOSTATUS03");


                // Apply SAP filter
                var TempList = DashboardItem.RowListEcnEco.Where((item) => item.EcnEcoToShowEndUser.EcnEcoFollowUp.Ecn_State != null
                        && ListSapFilter.Contains(item.EcnEcoToShowEndUser.Status.ToString().Trim())).ToList();

                // Apply Ecn State filter "Under review" 
                List<string> ListPdmState = new List<string>();
                ListPdmState.Add("Open");
                ListPdmState.Add("Implementation");

                if (DashboardItem.CurrentDashboardConfiguration.IsUnderReview)
                {
                    var TempListUnderReview = DashboardItem.RowListEcnEco.Where((item) => item.EcnEcoToShowEndUser.EcnEcoFollowUp.Designer_Start_App_Date != null
                                && item.EcnEcoToShowEndUser.EcnEcoFollowUp.Designer_Start_App_Date.ToString().Trim() != ""
                                && ListPdmState.Contains(item.EcnEcoToShowEndUser.EcnEcoFollowUp.Ecn_State)).ToList();
                    foreach (var item in TempListUnderReview)
                        if (TempList.FirstOrDefault((eco) => eco.GetHashCode() == item.GetHashCode()) == null) TempList.Add(item);
                }

                // Apply Ecn State filter "In progress"
                if (DashboardItem.CurrentDashboardConfiguration.IsInProgress)
                {
                    var TempListInProgress = DashboardItem.RowListEcnEco.Where((item) => item.EcnEcoToShowEndUser.EcnEcoFollowUp.Designer_Start_App_Date == null
                                || item.EcnEcoToShowEndUser.EcnEcoFollowUp.Designer_Start_App_Date.ToString().Trim() == ""
                                && ListPdmState.Contains(item.EcnEcoToShowEndUser.EcnEcoFollowUp.Ecn_State)).ToList();
                    foreach (var item in TempListInProgress)
                        if (TempList.FirstOrDefault((eco) => eco.GetHashCode() == item.GetHashCode()) == null) TempList.Add(item);
                }

                ListPdmState.Clear();
                if (DashboardItem.CurrentDashboardConfiguration.IsResolved)
                    ListPdmState.Add("Resolved");

                if (DashboardItem.CurrentDashboardConfiguration.IsCanceled)
                    ListPdmState.Add("Canceled");

                // Apply Ecn State filter "Resolved" and "Cancelled"
                if (DashboardItem.CurrentDashboardConfiguration.IsInProgress || DashboardItem.CurrentDashboardConfiguration.IsResolved)
                {
                    var TempListResolvedCancelled = DashboardItem.RowListEcnEco.Where((item) => ListPdmState.Contains(item.EcnEcoToShowEndUser.EcnEcoFollowUp.Ecn_State)).ToList();
                    foreach (var item in TempListResolvedCancelled)
                        if (TempList.FirstOrDefault((eco) => eco.GetHashCode() == item.GetHashCode()) == null) TempList.Add(item);
                }

                DashboardItem.ListEcnEco.Clear();
                foreach (var item in TempList)
                    DashboardItem.ListEcnEco.Add(item);
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
