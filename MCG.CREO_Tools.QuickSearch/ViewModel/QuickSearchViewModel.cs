using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Models.SAP;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WebtermLib.Services.Interfaces;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.WindchillCredential;
using MCG.CREO_Tools.QuickSearch.Configuration;
using MCG.CREO_Tools.QuickSearch.Exceptions;
using MCG.CREO_Tools.QuickSearch.Interfaces;
using MCG.CREO_Tools.QuickSearch.View;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Services.Interfaces;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
    public class QuickSearchViewModel : ObservableObject, IQuickSearchViewModel
    {
        #region [REGION] Properties from Interface
        public QuickSearchDataContext CurrentQuickSearchDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private QuickSearchConfiguration CurrentQuickSearchConfiguration { get; set; }
        private MCGLanguage CurrentMcgLanguage { get; set; }
        private WindchillCredentialItem WindchillNetworkCredential { get; set; }
        private Thread ThreadSearchSapInfo { get; set; }
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
        public ICommand CommandBtHelpMouseLeftButtonUpEvent { get => new RelayCommand<string>((doc) => ExecuteBtHelpMouseLeftButtonUpEvent(doc)); }
        public ICommand CommandOpenModelInCreo { get => new RelayCommand<bool>((isasynch) => ExecuteOpenModelInCreo(isasynch)); }
        public ICommand CommandAddModelInAssembly { get => new RelayCommand<bool>((isasynch) => ExecuteAddModelInAssembly(isasynch)); }
        public ICommand CommandOpenPartInPdm { get => new RelayCommand(() => ExecuteOpenPartInPdm()); }
        public ICommand CommandOpenRefDocInPdm { get => new RelayCommand<bool>((isasynch) => ExecuteOpenRefDocInPdm(isasynch)); }
        public ICommand CommandSearchColumnKeyWord { get => new RelayCommand<QuickSearchPartSubClassParam>((SubClassParam) => ExecuteSearchColumnKeyWord(SubClassParam)); }
        public ICommand CommandAddExtraComponent { get => new RelayCommand<QuickSearchExtraCompMenu>((menu) => ExecuteAddExtraComponent(menu)); }
        public ICommand CommandUpdateExtraCompMenu { get => new RelayCommand(() => ExecuteUpdateExtraCompMenu()); }
        public ICommand CommandPartSelectionChanged { get => new RelayCommand(() => ExecutePartSelectionChanged()); }
        public ICommand CommandSearchRefDocFromNumber { get => new RelayCommand(() => ExecuteSearchRefDocFromNumber()); }
        public ICommand CommandSearchClassSubClassFromNumber { get => new RelayCommand(() => ExecuteSearchClassSubClassFromNumber()); }
        public ICommand CommandCopyPartNumber { get => new RelayCommand(() => ExecuteCopyPartNumber()); }
        public ICommand CommandShortCutClassSubClass { get => new RelayCommand<QuickSearchShortCutViewModel>((shortcut) => ExecuteShortCutClassSubClass(shortcut)); }
        public ICommand CommandShortCutDelete { get => new RelayCommand<QuickSearchShortCutViewModel>((shortcut) => ExecuteShortCutDelete(shortcut)); }
        public ICommand CommandShortCutReOrderDown { get => new RelayCommand<QuickSearchShortCutViewModel>((shortcut) => ExecuteShortCutReOrderDown(shortcut)); }
        public ICommand CommandShortCutReOrderUp { get => new RelayCommand<QuickSearchShortCutViewModel>((shortcut) => ExecuteShortCutReOrderUp(shortcut)); }
        public ICommand CommandShortCutAdd { get => new RelayCommand(() => ExecuteShortCutAdd()); }
        public ICommand CommandShortCutReset { get => new RelayCommand(() => ExecuteShortCutReset()); }
        public ICommand CommandEditPartNumber { get => new RelayCommand(() => ExecuteEditPartNumber()); }
        public ICommand CommandAddNewPartNumber { get => new RelayCommand<bool>((b) => ExecuteAddNewPartNumber(b)); }
        public ICommand CommandOpenRefDocumentFromLink { get => new RelayCommand<bool>((b) => ExecuteOpenRefDocumentFromLink(b)); }
        #endregion

        #region [REGION] Init
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoMacroService _creoMacroService;
        private readonly IUserAuthorizationService _userAuthorizationService;
        private readonly IMcgToolDictionary _mcgToolDictionary;
        private readonly ICreoModelService _creoModelService;
        private readonly IHtmlTools _htmlTools;
        private readonly IWindchillPartManagementService _windchillPartManagementService;
        private readonly IWindchillNavigationService _windchillNavigationService;
        private readonly IWindchillDocumentManagementService _windchillDocumentManagementService;
        private readonly ISapHupService _sapHupService;
        private readonly IQuickSearchService _quickSearchService;
        private readonly IQuickSearchWindchillService _quickSearchWindchillService;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly ISharedAppContext _sharedAppContext;

        public QuickSearchViewModel(IXmlSerializeTools xmlSerializeTools,
                                    ICreoSessionProvider creoSessionProvider,
                                    IUserAuthorizationService userAuthorizationService,
                                    IMcgToolDictionary mcgToolDictionary,
                                    ICreoModelService creoModelService,
                                    ICreoMacroService creoMacroService,
                                    IHtmlTools htmlTools,
                                    IWindchillPartManagementService windchillPartManagementService,
                                    IWindchillNavigationService windchillNavigationService,
                                    IWindchillDocumentManagementService windchillDocumentManagementService,
                                    ISapHupService sapHupService,
                                    IQuickSearchService quickSearchService,
                                    IQuickSearchWindchillService quickSearchWindchillService,
                                    IWindchillCredentialService windchillCredentialService,
                                    ISharedAppContext sharedAppContext)
        {
            try
            {
                _xmlSerializeTools = xmlSerializeTools;
                _creoSessionProvider = creoSessionProvider;
                _userAuthorizationService = userAuthorizationService;
                _mcgToolDictionary = mcgToolDictionary;
                _creoModelService = creoModelService;
                _creoMacroService = creoMacroService;
                _htmlTools = htmlTools;
                _windchillPartManagementService = windchillPartManagementService;
                _windchillNavigationService = windchillNavigationService;
                _windchillDocumentManagementService = windchillDocumentManagementService;
                _sapHupService = sapHupService;
                _quickSearchService = quickSearchService;
                _quickSearchWindchillService = quickSearchWindchillService;
                _windchillCredentialService = windchillCredentialService;
                _sharedAppContext = sharedAppContext;

                CurrentQuickSearchDataContext = new QuickSearchDataContext();

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentQuickSearchDataContext.IsCreoEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentQuickSearchDataContext.IsCreoEnable = e;

                CurrentMcgLanguage = _sharedAppContext.CurrentLanguage?.Language;

                if (CurrentMcgLanguage != null)
                    CurrentMcgLanguage.ChangeLanguageInterface += UpdateInterfaceLanguage;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                CurrentQuickSearchConfiguration = _xmlSerializeTools.GetDeserializedXml<QuickSearchConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{QuickSearchConstants.ConfigurationFile}");

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{QuickSearchConstants.MainDictionary}", UriKind.Absolute);

                SetCurrentPicture($"{MainAppFolder}\\{CommonLibConstants.PictureFolder}\\{QuickSearchConstants.MainPicture}");

                CurrentQuickSearchDataContext.CRWLocalEnabled = CurrentQuickSearchConfiguration.CRWLocalEnabled;
                CurrentQuickSearchDataContext.DGLocalEnabled = CurrentQuickSearchConfiguration.DGLocalEnabled;
                CurrentQuickSearchDataContext.SGLocalEnabled = CurrentQuickSearchConfiguration.SGLocalEnabled;
                CurrentQuickSearchDataContext.TWRLocalEnabled = CurrentQuickSearchConfiguration.TWRLocalEnabled;
                CurrentQuickSearchDataContext.MFGTWRLocalEnabled = CurrentQuickSearchConfiguration.MFGTWRLocalEnabled;
                CurrentQuickSearchDataContext.STDGlobalEnabled = CurrentQuickSearchConfiguration.STDGlobalEnabled;

                CurrentQuickSearchDataContext.CRWLocalShown = CurrentQuickSearchConfiguration.CRWLocalShown;
                CurrentQuickSearchDataContext.DGLocalShown = CurrentQuickSearchConfiguration.DGLocalShown;
                CurrentQuickSearchDataContext.SGLocalShown = CurrentQuickSearchConfiguration.SGLocalShown;
                CurrentQuickSearchDataContext.TWRLocalShown = CurrentQuickSearchConfiguration.TWRLocalShown;
                CurrentQuickSearchDataContext.MFGTWRLocalShown = CurrentQuickSearchConfiguration.MFGTWRLocalShown;
                CurrentQuickSearchDataContext.STDGlobalShown = CurrentQuickSearchConfiguration.STDGlobalShown;

                CurrentQuickSearchDataContext.UpdateListStandardShown();

                ReadAllClass();

                CurrentQuickSearchDataContext.StandardSelectionChangedEvent += ReadAllClass;
                CurrentQuickSearchDataContext.ClassChangedEvent += ReadSubClass;
                CurrentQuickSearchDataContext.SubClassChangingEvent += ReadPartSubClass;

                // Update ShortCut
                QuickSearchUserConfiguration CurrentUserConfig = GetUserConfigFromXmlFile();
                CurrentQuickSearchDataContext.ListShortCutData = new List<QuickSearchShortCutData>();
                if (CurrentUserConfig != null && CurrentUserConfig.ListShortCut != null && CurrentUserConfig.ListShortCut.Count > 0)
                    CurrentQuickSearchDataContext.ListShortCutData.AddRange(CurrentUserConfig.ListShortCut);
                else if (CurrentQuickSearchConfiguration.ListShortCut != null)
                    CurrentQuickSearchDataContext.ListShortCutData.AddRange(CurrentQuickSearchConfiguration.ListShortCut);
                UpdateShortCutList(CurrentQuickSearchDataContext.ListShortCutData);

                // Update List Sap Plant
                if (CurrentQuickSearchConfiguration.ListSapPlant != null)
                    foreach (var plant in CurrentQuickSearchConfiguration.ListSapPlant)
                        CurrentQuickSearchDataContext.ListSapPlant.Add(plant);
                SapPlant UserSapPlant = null;
                if (CurrentUserConfig != null && CurrentUserConfig.CurrentSapPlant != null)
                    UserSapPlant = CurrentQuickSearchConfiguration.ListSapPlant.FirstOrDefault((item) => item.Name == CurrentUserConfig.CurrentSapPlant.Name);
                if (UserSapPlant != null)
                    CurrentQuickSearchDataContext.SelectedSapPlant = UserSapPlant;
                else
                    CurrentQuickSearchDataContext.SelectedSapPlant = CurrentQuickSearchDataContext.ListSapPlant.FirstOrDefault();
                CurrentQuickSearchDataContext.ShowSapCostVolumeInfo = CurrentQuickSearchConfiguration.ShowSapCostVolumeInfo;

                CurrentQuickSearchDataContext.SapPlantChangeEvent += UpdateUserConfigXmlFile;

                CurrentQuickSearchDataContext.IsAdminToolsEnabled = CheckUserAuthorization(QuickSearchConstants.KeyUserUpdateAppName);

            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public bool CheckUserAuthorization(string AppName)
        {
            try
            {
                if (AppName == null) AppName = "";
                return _userAuthorizationService.GetAppAuthorization(Environment.UserName.ToUpper(), AppName.ToUpper());
            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }

        private void UpdateShortCutList(List<QuickSearchShortCutData> ListShortCut)
        {
            try
            {
                if (ListShortCut != null)
                {
                    CurrentQuickSearchDataContext.ListShortCut.Clear();
                    int order = 1;
                    foreach (var shortcut in ListShortCut.OrderBy((sc) => sc.Order))
                    {
                        var DbClass = _quickSearchService.GetOnePartClass(shortcut.Class);
                        var DbSubClass = _quickSearchService.GetOnePartSubClass(shortcut.SubClass);
                        string StdClass, StdSubClass;
                        if (DbClass != null)
                            StdClass = _mcgToolDictionary.GetTerm(DbClass.Idclassname);
                        else
                            StdClass = _mcgToolDictionary.GetTerm(shortcut.Class);
                        if (DbSubClass != null)
                            StdSubClass = _mcgToolDictionary.GetTerm(DbSubClass.Subclassname);
                        else
                            StdSubClass = _mcgToolDictionary.GetTerm(shortcut.SubClass);

                        shortcut.Order = order;
                        CurrentQuickSearchDataContext.ListShortCut.Add(new QuickSearchShortCutViewModel()
                        {
                            Class = StdClass,
                            SubClass = StdSubClass,
                            Order = order,
                            MainApp = this,
                            ParentData = shortcut
                        });
                        order++;
                    }
                    CurrentQuickSearchDataContext.RaiseShortCutChangedEvent();
                    UpdateUserConfigXmlFile();
                }
            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }

        private QuickSearchUserConfiguration GetUserConfigFromXmlFile()
        {
            try
            {
                QuickSearchUserConfiguration CurrentConfig = _xmlSerializeTools.GetDeserializedXmlFromAppData<QuickSearchUserConfiguration>(QuickSearchConstants.QuickSearchUserShortCuts);
                return CurrentConfig;
            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }

        private void UpdateUserConfigXmlFile(object sender = null, EventArgs e = null)
        {
            try
            {
                if (CurrentQuickSearchDataContext.SelectedSapPlant != null)
                {
                    QuickSearchUserConfiguration CurrentConfig = new QuickSearchUserConfiguration()
                    {
                        ListShortCut = CurrentQuickSearchDataContext.ListShortCutData,
                        CurrentSapPlant = CurrentQuickSearchDataContext.SelectedSapPlant
                    };

                    _xmlSerializeTools.SerializedXmlInAppData<QuickSearchUserConfiguration>(CurrentConfig, QuickSearchConstants.QuickSearchUserShortCuts);
                }
            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }

        private void UpdateInterfaceLanguage(object sender = null, EventArgs e = null)
        {
            try
            {
                ReadAllClass();
                UpdateShortCutList(CurrentQuickSearchDataContext.ListShortCutData);
            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteBtHelpMouseLeftButtonUpEvent(string doc)
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("QS_LinkHelpQuickSearch"));
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenModelInCreo(bool InAsynch = false)
        {
            try
            {
                if (CurrentQuickSearchDataContext.SelectedPartItem != null)
                {
                    if (InAsynch)
                    {
                        RaiseActionInProgressEvent();

                        Thread aThread = new Thread(new ThreadStart(() =>
                        {
                            CurrentQuickSearchDataContext.SelectedPartItem.CurrentEpmDocument.OpenInCreo(_creoSessionProvider, _creoModelService);
                            RaiseActionDoneEvent();
                        }));
                        aThread.IsBackground = true;
                        aThread.Start();
                    }
                    else
                        CurrentQuickSearchDataContext.SelectedPartItem.CurrentEpmDocument.OpenInCreo(_creoSessionProvider, _creoModelService);
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddModelInAssembly(bool InAsynch = false)
        {
            try
            {
                if (CurrentQuickSearchDataContext.SelectedPartItem != null)
                    if (InAsynch)
                    {
                        RaiseActionInProgressEvent();

                        Thread aThread = new Thread(new ThreadStart(() =>
                        {
                            CurrentQuickSearchDataContext.SelectedPartItem.CurrentEpmDocument.AddInAssembly(_creoModelService, _creoMacroService, _creoSessionProvider);
                            RaiseActionDoneEvent();
                        }));
                        aThread.IsBackground = true;
                        aThread.Start();
                    }
                    else
                        CurrentQuickSearchDataContext.SelectedPartItem.CurrentEpmDocument.AddInAssembly(_creoModelService, _creoMacroService, _creoSessionProvider);
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenPartInPdm()
        {
            try
            {
                if (CurrentQuickSearchDataContext.SelectedPartItem != null)
                {
                    CheckwindchillCredential();
                    if (!_windchillNavigationService.OpenWtPartDetailPage(WindchillNetworkCredential.WindchillCredential, CurrentQuickSearchDataContext.SelectedPartItem.CurrentPart.Recpart, "Latest"))
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("QS_ErrorMsgPartSearch", new string[1] { CurrentQuickSearchDataContext.SelectedPartItem.CurrentPart.Recpart }), McgWpfTools.GetStringResource("QS_ErrorMsgTitleGlobal"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenRefDocInPdm(bool InAsynch = false)
        {
            try
            {
                if (CurrentQuickSearchDataContext.SelectedPartItem != null)
                {
                    CheckwindchillCredential();
                    if (InAsynch)
                    {
                        RaiseActionInProgressEvent();

                        Thread aThread = new Thread(new ThreadStart(() => SearchRefDocInPdm()));
                        aThread.IsBackground = true;
                        aThread.Start();
                    }
                    else
                        SearchRefDocInPdm();
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSearchColumnKeyWord(QuickSearchPartSubClassParam SubClassParam)
        {
            try
            {
                AppliAllColumnFilter();
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddExtraComponent(QuickSearchExtraCompMenu menu)
        {
            try
            {
                if (menu != null && menu.ExtraEpmDoc != null)
                {
                    // menu.ExtraEpmDoc.AddInAssembly();
                    RaiseActionInProgressEvent();

                    Thread aThread = new Thread(new ThreadStart(() =>
                    {
                        menu.ExtraEpmDoc.AddInAssembly(_creoModelService, _creoMacroService, _creoSessionProvider);
                        RaiseActionDoneEvent();
                    }));
                    aThread.IsBackground = true;
                    aThread.Start();
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateExtraCompMenu()
        {
            try
            {
                if (CurrentQuickSearchDataContext.SelectedPartItem != null)
                    CurrentQuickSearchDataContext.ListExtraMenu = CurrentQuickSearchDataContext.SelectedPartItem.ListExtraMenu;
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteExtraMenuMouseEnterEvent(object sender, EventArgs e)
        {
            try
            {
                if (sender != null)
                {
                    CurrentQuickSearchDataContext.ExtraPictureShown = ((QuickSearchExtraCompMenu)sender).ExtraCompImage;
                    CurrentQuickSearchDataContext.IsExtraComponentShown = true;
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteExtraMenuMouseLeaveEvent(object sender, EventArgs e)
        {
            try
            {
                CurrentQuickSearchDataContext.IsExtraComponentShown = false;
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecutePartSelectionChanged()
        {
            try
            {
                if (CurrentQuickSearchDataContext.SelectedPartItem != null)
                {
                    string pictureFileName = $"{MainAppFolder}\\{CommonLibConstants.PictureFolder}\\{CurrentQuickSearchDataContext.SelectedPartItem.CurrentPart.Partpicture}";
                    if (File.Exists(pictureFileName))
                        CurrentQuickSearchDataContext.PartPictureShown = McgWpfTools.GetBitmapImage(pictureFileName);
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSearchRefDocFromNumber()
        {
            try
            {
                _quickSearchWindchillService.ShowQuickSearchWindowRefDocFromNumberView(true);
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private async void ExecuteSearchClassSubClassFromNumber()
        {
            try
            {


                var selected = await _quickSearchWindchillService
                    .ShowDialogQuickSearchWindowClassSubClassFromNumberViewAsync(
                        CurrentQuickSearchDataContext.ListStandardShown);

                if (selected != null)
                    await ExecuteShortCutClassSubClassAsync(selected);

                //QuickSearchWindowClassSubClassFromNumberViewModel aQuickSearchWindowClassSubClassFromNumberViewModel = new QuickSearchWindowClassSubClassFromNumberViewModel(CurrentQuickSearchDataContext.ListStandardShown);

                //EventHandler handler = (s, e) => ExecuteShortCutClassSubClass(aQuickSearchWindowClassSubClassFromNumberViewModel.ClassSubClass);
                //aQuickSearchWindowClassSubClassFromNumberViewModel.OpenClassSubClassEvent += handler;

                //aQuickSearchWindowClassSubClassFromNumberViewModel.Show();
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCopyPartNumber()
        {
            try
            {
                if (CurrentQuickSearchDataContext.SelectedPartItem != null)
                {
                    McgMiscTools.CopyTextClipboard(CurrentQuickSearchDataContext.SelectedPartItem.CurrentPart.Recpart);
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteShortCutClassSubClass(QuickSearchShortCutViewModel ShortCut)
        {
            try
            {
                QuickSearchPartClass CurrentClass = CurrentQuickSearchDataContext.ListClass.FirstOrDefault((cl) => cl.CurrentPartClass.Idclass == ShortCut.ParentData.Class);
                if (CurrentClass != null)
                {
                    CurrentQuickSearchDataContext.SelectedClassItem = CurrentClass;

                    QuickSearchPartSubClass CurrentSubClass = CurrentQuickSearchDataContext.ListSubClass.FirstOrDefault((scl) => scl.CurrentPartSubClass.Idsubclass == ShortCut.ParentData.SubClass);
                    if (CurrentSubClass != null)
                        CurrentQuickSearchDataContext.SelectedSubClassItem = CurrentSubClass;
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private async Task ExecuteShortCutClassSubClassAsync(QuickSearchShortCutViewModel ShortCut)
        {
            try
            {
                ExecuteShortCutClassSubClass(ShortCut);
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteShortCutDelete(QuickSearchShortCutViewModel ShortCut)
        {
            try
            {
                if (MessageBox.Show(McgWpfTools.GetStringResource("QS_AskRemoveShortCut"), McgWpfTools.GetStringResource("QS_AskTitleRemoveShortCut"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    CurrentQuickSearchDataContext.ListShortCutData.Remove(ShortCut.ParentData);
                    UpdateShortCutList(CurrentQuickSearchDataContext.ListShortCutData);
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteShortCutReOrderDown(QuickSearchShortCutViewModel ShortCut)
        {
            try
            {
                SwitchShortCut(ShortCut.ParentData, +1);
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteShortCutReOrderUp(QuickSearchShortCutViewModel ShortCut)
        {
            try
            {
                SwitchShortCut(ShortCut.ParentData, -1);
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteShortCutAdd()
        {
            try
            {
                if (CurrentQuickSearchDataContext.SelectedClassItem != null && CurrentQuickSearchDataContext.SelectedSubClassItem != null)
                {
                    string StdClass = CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentPartSubClass.Idclass;
                    string StdSubClass = CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentPartSubClass.Idsubclass;
                    if (CurrentQuickSearchDataContext.ListShortCutData.FirstOrDefault((sc) => sc.Class == StdClass && sc.SubClass == StdSubClass) == null)
                    {
                        CurrentQuickSearchDataContext.ListShortCutData.Add(new QuickSearchShortCutData()
                        {
                            Class = StdClass,
                            SubClass = StdSubClass,
                            Order = 1000
                        });

                        UpdateShortCutList(CurrentQuickSearchDataContext.ListShortCutData);
                    }
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteShortCutReset()
        {
            try
            {
                if (MessageBox.Show(McgWpfTools.GetStringResource("QS_AskResetShortCut"), McgWpfTools.GetStringResource("QS_AskTitleResetShortCut"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    CurrentQuickSearchDataContext.ListShortCutData = new List<QuickSearchShortCutData>();
                    if (CurrentQuickSearchConfiguration.ListShortCut != null)
                        CurrentQuickSearchDataContext.ListShortCutData.AddRange(CurrentQuickSearchConfiguration.ListShortCut);
                    UpdateShortCutList(CurrentQuickSearchDataContext.ListShortCutData);
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteEditPartNumber()
        {
            try
            {
                QuickSearchPart CurrentPart = CurrentQuickSearchDataContext.SelectedPartItem;
                bool IsCrationOk = false;

                while (!IsCrationOk)
                {
                    var returnWindow = _quickSearchWindchillService.ShowDialogQuickSearchUpdatePartView(CurrentPart);

                    if (returnWindow == MessageBoxResult.Yes)
                    {
                        if (CurrentPart.UpdatedPart.Recpart != null && CurrentPart.UpdatedPart.Recpart.Trim() != "" && CurrentPart.UpdatedPart.Epmdoc != null && CurrentPart.UpdatedPart.Epmdoc.Trim() != "")
                        {
                            McgReflectionTools.UpdateInstanceFromObject<Part>(CurrentPart.CurrentPart, CurrentPart.UpdatedPart);

                            Part DbPart = _quickSearchService.GetOnePart(CurrentPart.OrigPartNumber, CurrentPart.CurrentPart.Idsubclass);
                            if (DbPart != null)
                            {
                                McgReflectionTools.UpdateInstanceFromObject<Part>(DbPart, CurrentPart.CurrentPart);
                                _quickSearchService.UpdateOnePart(CurrentPart.CurrentPart);
                            }

                            IsCrationOk = true;
                            ReadPartSubClass();
                        }
                        else
                        {
                            MessageBox.Show(McgWpfTools.GetStringResource("QS_ErrorMsgNumberBlank"), McgWpfTools.GetStringResource("QS_ErrorMsgTitleIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                        IsCrationOk = true;
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddNewPartNumber(bool FromSelected = false)
        {
            try
            {
                if (CurrentQuickSearchDataContext.SelectedSubClassItem != null)
                {
                    Part pART;
                    if (FromSelected)
                        pART = McgMiscTools.CopyInstanceFromObject<Part>(CurrentQuickSearchDataContext.SelectedPartItem.CurrentPart);
                    else
                        pART = new Part() { Idsubclass = CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentPartSubClass.Idsubclass };

                    QuickSearchPart CurrentPart = new QuickSearchPart()
                    {
                        UpdatedPart = pART,
                        SubClassItem = CurrentQuickSearchDataContext.SelectedSubClassItem
                    };
                    if (FromSelected)
                    {
                        CurrentPart.PartPicturePath = CurrentQuickSearchDataContext.SelectedPartItem.PartPicturePath;
                        CurrentPart.UpdatedImage = pART.Partpicturebin;
                    }

                    bool IsCrationOk = false;

                    while (!IsCrationOk)
                    {
                        var returnWindow = _quickSearchWindchillService.ShowDialogQuickSearchUpdatePartView(CurrentPart);

                        if (returnWindow == MessageBoxResult.Yes)
                        {
                            if (CurrentPart.UpdatedPart.Recpart != null && CurrentPart.UpdatedPart.Recpart.Trim() != "" && CurrentPart.UpdatedPart.Epmdoc != null && CurrentPart.UpdatedPart.Epmdoc.Trim() != "")
                            {
                                CurrentPart.CurrentPart = McgMiscTools.CopyInstanceFromObject<Part>(CurrentPart.UpdatedPart);

                                Part DbPart = _quickSearchService.GetOnePart(CurrentPart.CurrentPart.Recpart, CurrentPart.CurrentPart.Idsubclass);
                                if (DbPart != null)
                                    MessageBox.Show(McgWpfTools.GetStringResource("QS_ErrorMsgPartExist"), McgWpfTools.GetStringResource("QS_ErrorMsgTitlePartExist"), MessageBoxButton.OK, MessageBoxImage.Warning);
                                else
                                {
                                    DbPart = McgMiscTools.CopyInstanceFromObject<Part>(CurrentPart.CurrentPart);
                                    DbPart.Id = _quickSearchService.GetNextPartId();
                                    _quickSearchService.AddOnePart(DbPart);
                                    IsCrationOk = true;
                                    ReadPartSubClass();
                                }
                            }
                            else
                            {
                                MessageBox.Show(McgWpfTools.GetStringResource("QS_ErrorMsgNumberBlank"), McgWpfTools.GetStringResource("QS_ErrorMsgTitleIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                            IsCrationOk = true;
                    }
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("QS_ErrorMsgClassNotSelected"), McgWpfTools.GetStringResource("QS_ErrorMsgTitleIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);

            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenRefDocumentFromLink(bool InAsynch = false)
        {
            try
            {
                CheckwindchillCredential();
                if (InAsynch)
                {
                    RaiseActionInProgressEvent();

                    Thread aThread = new Thread(new ThreadStart(() => SearchRefDocInPdmFromDocNumber(CurrentQuickSearchDataContext.RefDocument)));
                    aThread.IsBackground = true;
                    aThread.Start();
                }
                else
                    SearchRefDocInPdmFromDocNumber(CurrentQuickSearchDataContext.RefDocument);
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void SwitchShortCut(QuickSearchShortCutData CurrentShortCut, int increment)
        {
            try
            {
                QuickSearchShortCutData TempShortCut = CurrentQuickSearchDataContext.ListShortCutData.FirstOrDefault((sc) => sc.Order == CurrentShortCut.Order + increment);
                if (TempShortCut != null)
                {
                    TempShortCut.Order = CurrentShortCut.Order;
                    CurrentShortCut.Order += increment;
                    UpdateShortCutList(CurrentQuickSearchDataContext.ListShortCutData);
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods Seach Data
        private void SearchRefDocInPdm()
        {
            try
            {
                RestOdataWtPart CurrentRestOdataWtPart = _windchillPartManagementService.GetOneWtPartWithWtDocumentAssociation(WindchillNetworkCredential.WindchillCredential, CurrentQuickSearchDataContext.SelectedPartItem.CurrentPart.Recpart);

                if (CurrentRestOdataWtPart != null && CurrentRestOdataWtPart.References != null && CurrentRestOdataWtPart.References.Count > 0)
                {
                    Regex RegexRefDoc = new Regex("^TDFC|^GEI|^PR-", RegexOptions.IgnoreCase);
                    RestOdataWtObjectAssociation CurrentDoc = CurrentRestOdataWtPart.References.Where((doc) => doc.References != null &&
                                                 RegexRefDoc.IsMatch(doc.References.Number) &&
                                                 doc.References.PrimaryContent != null &&
                                                 doc.References.PrimaryContent.Content != null).FirstOrDefault();
                    if (CurrentDoc != null)
                    {
                        string CompleteFileName = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\{CurrentDoc.References.PrimaryContent.Content.Label}";
                        _htmlTools.DownloadFileFromUrl(CurrentDoc.References.PrimaryContent.Content.URL, WindchillNetworkCredential.WindchillCredential, CompleteFileName);
                        McgFileAndSystemTools.OpenFile(CompleteFileName);
                    }
                    else
                        MessageBox.Show(McgWpfTools.GetStringResource("QS_ErrorMsgRefDocSearch"), McgWpfTools.GetStringResource("QS_ErrorMsgTitleGlobal"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("QS_ErrorMsgRefDocSearch"), McgWpfTools.GetStringResource("QS_ErrorMsgTitleGlobal"), MessageBoxButton.OK, MessageBoxImage.Warning);

            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void SearchRefDocInPdmFromDocNumber(string RefNumber)
        {
            try
            {
                RestOdataWtDocument CurrentRefDoc = _windchillDocumentManagementService.GetOneWtDocumentWithContent(WindchillNetworkCredential.WindchillCredential, RefNumber);

                if (CurrentRefDoc != null && CurrentRefDoc.PrimaryContent != null &&
                    CurrentRefDoc.PrimaryContent.Content != null &&
                    CurrentRefDoc.PrimaryContent.Content.URL != null)
                {
                    string CompleteFileName = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\{CurrentRefDoc.PrimaryContent.Content.Label}";
                    _htmlTools.DownloadFileFromUrl(CurrentRefDoc.PrimaryContent.Content.URL, WindchillNetworkCredential.WindchillCredential, CompleteFileName);
                    McgFileAndSystemTools.OpenFile(CompleteFileName);
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("QS_ErrorMsgRefDocSearch"), McgWpfTools.GetStringResource("QS_ErrorMsgTitleGlobal"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void ReadAllClass(object sender = null, EventArgs e = null)
        {
            try
            {

                CurrentQuickSearchDataContext.ListPartItemShown.Clear();
                CurrentQuickSearchDataContext.ListClass.Clear();
                if (CurrentQuickSearchDataContext.ListStandardShown != null && CurrentQuickSearchDataContext.ListStandardShown.Count > 0)
                {
                    var ListTempClass = _quickSearchService.GetPartClassesWithSubClassFilter(CurrentQuickSearchDataContext.ListStandardShown);

                    List<QuickSearchPartClass> SortedList = new List<QuickSearchPartClass>();
                    foreach (Partclass elem in ListTempClass)
                        SortedList.Add(new QuickSearchPartClass() { CurrentPartClass = elem, Name = _mcgToolDictionary.GetTerm(elem.Idclassname) });
                    SortedList = SortedList.OrderBy((elem) => elem.Name).ToList();

                    foreach (QuickSearchPartClass elem in SortedList)
                        CurrentQuickSearchDataContext.ListClass.Add(elem);
                }

            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }

        private void ReadSubClass(object sender = null, EventArgs e = null)
        {
            try
            {
                // created connection to SQL server and search all Classes

                CurrentQuickSearchDataContext.ListSubClass.Clear();
                if (CurrentQuickSearchDataContext.ListStandardShown != null && CurrentQuickSearchDataContext.ListStandardShown.Count > 0 && CurrentQuickSearchDataContext.SelectedClassItem != null)
                {
                    var ListTempSubClass = _quickSearchService.GetPartSubClasses(CurrentQuickSearchDataContext.SelectedClassItem.CurrentPartClass.Idclass, CurrentQuickSearchDataContext.ListStandardShown);

                    List<QuickSearchPartSubClass> SortedList = new List<QuickSearchPartSubClass>();
                    QuickSearchPartSubClass CurrentQuickSearchPartSubClass;
                    foreach (Partsubclass elem in ListTempSubClass)
                    {
                        CurrentQuickSearchPartSubClass = new QuickSearchPartSubClass()
                        {
                            CurrentPartSubClass = elem,
                            Name = _mcgToolDictionary.GetTerm(elem.Subclassname)
                        };
                        var AllPartSubClassParam = _quickSearchService.GetPartSubClassParams(elem.Idsubclass);

                        CurrentQuickSearchPartSubClass.AllPartSubClassParam = new List<QuickSearchPartSubClassParam>();
                        foreach (var SubClassParam in AllPartSubClassParam)
                            CurrentQuickSearchPartSubClass.AllPartSubClassParam.Add(new QuickSearchPartSubClassParam() { CurrentPartSubClassParam = SubClassParam, Name = _mcgToolDictionary.GetTerm(SubClassParam.Idparamname) });
                        CurrentQuickSearchPartSubClass.ShownPartSubClassParam = CurrentQuickSearchPartSubClass.AllPartSubClassParam.Where((subClass) => subClass.CurrentPartSubClassParam.Info.ToUpper() == "TRUE").ToList();

                        SortedList.Add(CurrentQuickSearchPartSubClass);
                    }

                    SortedList = SortedList.OrderBy((elem) => elem.Name).ToList();

                    foreach (QuickSearchPartSubClass elem in SortedList)
                        CurrentQuickSearchDataContext.ListSubClass.Add(elem);
                }
                CurrentQuickSearchDataContext.SelectedSubClassItem = CurrentQuickSearchDataContext.ListSubClass.FirstOrDefault();

            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }

        private void ReadPartSubClass(object sender = null, EventArgs e = null)
        {
            try
            {
                UpdateSubClassInformation();
                if (CurrentQuickSearchDataContext.ListStandardShown != null && CurrentQuickSearchDataContext.ListStandardShown.Count > 0 && CurrentQuickSearchDataContext.SelectedSubClassItem != null)
                {

                    var ListTempPartSubClass = _quickSearchService.GetPartsBySubClass(CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentPartSubClass.Idsubclass);
                    ListTempPartSubClass = ListTempPartSubClass.OrderBy((elem) => elem.Pars001).OrderBy((elem) => elem.Parr001).ToList();

                    CurrentQuickSearchDataContext.ListPartItemCurrentSubClass = new List<QuickSearchPart>();
                    string pictureFileName;
                    foreach (Part elem in ListTempPartSubClass)
                    {
                        pictureFileName = $"{MainAppFolder}\\{CommonLibConstants.PictureFolder}\\{elem.Partpicture}";
                        if (!File.Exists(pictureFileName))
                            pictureFileName = $"{MainAppFolder}\\{CommonLibConstants.PictureFolder}\\{QuickSearchConstants.PictureNotFound}";

                        QuickSearchPart CurrentQuickSearchPart = new QuickSearchPart()
                        {
                            CurrentPart = elem,
                            CurrentEpmDocument = new EPMDocument(elem.Epmdoc, elem.Epmdoc, elem.Epmdoc),
                            PartPicturePath = pictureFileName,
                            UpdatedPart = McgReflectionTools.CopyInstanceFromObject<Part>(elem),
                            SubClassItem = CurrentQuickSearchDataContext.SelectedSubClassItem,
                            OrigPartNumber = elem.Recpart
                        };
                        CurrentQuickSearchPart.UpdatedImage = CurrentQuickSearchPart.UpdatedPart.Partpicturebin;

                        // Init part if SAP info is shown
                        if (CurrentQuickSearchDataContext.ShowSapCostVolumeInfo)
                        {
                            CurrentQuickSearchPart.PlantVolume = 0;
                            CurrentQuickSearchPart.PlantMaxVolume = "None";
                        }

                        CurrentQuickSearchDataContext.ListPartItemCurrentSubClass.Add(CurrentQuickSearchPart);
                        if (CurrentQuickSearchDataContext.IsExtraComponentPossible)
                            SearchExtraComponent(CurrentQuickSearchPart);
                    }

                    if (CurrentQuickSearchDataContext.ShowSapCostVolumeInfo)
                    {
                        if (ThreadSearchSapInfo != null && ThreadSearchSapInfo.IsAlive)
                        {
                            ThreadSearchSapInfo.Abort();
                            ThreadSearchSapInfo = null;
                        }
                    }
                    AppliAllColumnFilter();
                }
            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }

        private void SearchExtraComponent(QuickSearchPart currentQuickSearchPart)
        {
            try
            {
                var ListExtraCompParam = CurrentQuickSearchDataContext.SelectedSubClassItem.AllPartSubClassParam.Where((param) => param.CurrentPartSubClassParam.Extracomp.ToUpper() == "TRUE").ToList();
                QuickSearchExtraCompMenu CurrentMenu;
                currentQuickSearchPart.ListExtraMenu = new List<QuickSearchExtraCompMenu>();
                foreach (var ExtraCompParam in ListExtraCompParam)
                {
                    var CurrentProperty = typeof(Part).GetProperty(ExtraCompParam.CurrentPartSubClassParam.Idparamtable);
                    if (CurrentProperty != null)
                    {
                        string CadDocName = (string)CurrentProperty.GetValue(currentQuickSearchPart.CurrentPart);
                        Partattribute CurrentPartAttrib = _quickSearchService.GetOnePartAttribute(CadDocName);
                        if (CurrentPartAttrib == null) CurrentPartAttrib = new Partattribute() { Epmdoc = CadDocName, Description = CadDocName, Picture = QuickSearchConstants.PictureNotFound };
                        CurrentMenu = new QuickSearchExtraCompMenu()
                        {
                            Header = $"{CadDocName} - {CurrentPartAttrib.Description}",
                            ExtraEpmDoc = new EPMDocument(CadDocName, CadDocName, CadDocName),
                            Description = CurrentPartAttrib.Description,

                            ExtraCompImage = McgWpfTools.GetBitmapImage(CurrentPartAttrib.Picturebin),
                            DataContext = this,
                            Command = CommandAddExtraComponent,
                        };
                        CurrentMenu.CommandParameter = CurrentMenu;
                        CurrentMenu.MouseEnter += ExecuteExtraMenuMouseEnterEvent;
                        CurrentMenu.MouseLeave += ExecuteExtraMenuMouseLeaveEvent;

                        currentQuickSearchPart.ListExtraMenu.Add(CurrentMenu);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }

        private void AppliAllColumnFilter()
        {
            try
            {
                CurrentQuickSearchDataContext.ListPartItemShown.Clear();
                List<QuickSearchPart> TempPartList = CurrentQuickSearchDataContext.ListPartItemCurrentSubClass.ToList();
                Regex keyWordRegex;
                PropertyInfo CurrentProperty;

                foreach (var column in CurrentQuickSearchDataContext.SelectedSubClassItem.ShownPartSubClassParam)
                {
                    if (column.FilterValue != null && column.FilterValue != "")
                    {
                        keyWordRegex = new Regex(column.FilterValue, RegexOptions.IgnoreCase);
                        CurrentProperty = typeof(Part).GetProperty(column.IdParam);
                        if (CurrentProperty != null)
                        {
                            TempPartList = TempPartList.Where((part) => keyWordRegex.IsMatch(CurrentProperty.GetValue(part.CurrentPart).ToString())).ToList();
                        }
                    }
                }

                foreach (var part in TempPartList)
                    CurrentQuickSearchDataContext.ListPartItemShown.Add(part);

                // Searche SAP Cost/Volume info
                if (CurrentQuickSearchDataContext.ShowSapCostVolumeInfo)
                {
                    if (ThreadSearchSapInfo == null || (ThreadSearchSapInfo != null && !ThreadSearchSapInfo.IsAlive))
                    {
                        ThreadSearchSapInfo = new Thread(() => UpdateSapCostVolumeInformation());
                        ThreadSearchSapInfo.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods search Sap Information
        private void UpdateSapCostVolumeInformation()
        {
            try
            {
                CurrentQuickSearchDataContext.IsMsgSearchSap = true;
                if (!CurrentQuickSearchDataContext.SelectedSubClassItem.IsSapSearchDone || CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentAllCostVolume == null)
                {
                    List<string> AllMaterial;
                    AllMaterial = CurrentQuickSearchDataContext.ListPartItemShown.Select((item) => GetPartNumber(item.CurrentPart.Recpart)).Distinct().ToList();

                    var tmpSapCostVolumeInfos = _sapHupService.GetListMaterialMasterCostVolumeInfo(AllMaterial);
                    CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentAllCostVolume = new List<SapCostVolumeInfo>();
                    if (tmpSapCostVolumeInfos != null)
                        CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentAllCostVolume.AddRange(tmpSapCostVolumeInfos.Select(x => new SapCostVolumeInfo(x)).ToList());
                }


                if (CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentAllCostVolume != null && CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentAllCostVolume.Count > 0)
                {
                    List<SapCostVolumeInfo> CurrentListMaterialCostVolume;
                    SapCostVolumeInfo CurrentMaterialCostVolume;
                    Regex EuropeRegex = new Regex(CommonLibConstants.SapEuropePlant);
                    Regex FranceRegex = new Regex(CommonLibConstants.SapFrenchPlant);
                    foreach (var part in CurrentQuickSearchDataContext.ListPartItemShown)
                    {
                        // Selected plant
                        part.PlantVolume = 0;
                        var SelectedPlant = CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentAllCostVolume.FirstOrDefault((item) => item.PlantNumber.Number == CurrentQuickSearchDataContext.SelectedSapPlant.Number && item.MaterialMasterNumber == GetPartNumber(part.CurrentPart.Recpart));
                        if (SelectedPlant != null)
                        {
                            part.PlantStdCost = Math.Round(McgBusinessTools.GetCurrencyFromUsd(McgBusinessTools.GetCurrencyToUsd(SelectedPlant.StdCost, GetPlantCurrency(SelectedPlant.PlantNumber)), CurrentQuickSearchDataContext.SelectedSapPlant.Currency), 2);
                            part.PlantVolume = Math.Round(SelectedPlant.Volume, 2);
                            part.PlantStdCostPerKg = Math.Round(McgBusinessTools.GetCurrencyFromUsd(McgBusinessTools.GetCurrencyToUsd(SelectedPlant.StdCostPerKg, GetPlantCurrency(SelectedPlant.PlantNumber)), CurrentQuickSearchDataContext.SelectedSapPlant.Currency), 2);
                            if (SelectedPlant.ProcurementType != null && (SelectedPlant.ProcurementType == "Purchased" || SelectedPlant.ProcurementType == "Manufactured"))
                                part.ProcurementType = McgWpfTools.GetStringResource($"QS_{SelectedPlant.ProcurementType}");
                            else
                                part.ProcurementType = "";
                        }
                        // World Average all plant
                        CurrentListMaterialCostVolume = CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentAllCostVolume.Where((item) => item.MaterialMasterNumber == GetPartNumber(part.CurrentPart.Recpart)).ToList();

                        if (CurrentListMaterialCostVolume != null && CurrentListMaterialCostVolume.Count > 0)
                        {
                            // Plant Max Volume
                            CurrentMaterialCostVolume = CurrentListMaterialCostVolume.FirstOrDefault((item) => item.Volume == CurrentListMaterialCostVolume.Max((max) => max.Volume));
                            if (CurrentMaterialCostVolume != null && CurrentMaterialCostVolume.Volume > 0)
                                part.PlantMaxVolume = CurrentMaterialCostVolume.PlantNumber.Number;
                            else
                                part.PlantMaxVolume = "None";

                            part.WorldAverageCost = Math.Round(CurrentListMaterialCostVolume.Where((item) => item.StdCost > 0).Average((item) => McgBusinessTools.GetCurrencyFromUsd(McgBusinessTools.GetCurrencyToUsd(item.StdCost, GetPlantCurrency(item.PlantNumber)), CurrentQuickSearchDataContext.SelectedSapPlant.Currency)), 2);
                            part.WorldAverageVolume = Math.Round(CurrentListMaterialCostVolume.Sum((item) => item.Volume), 2);

                            // Europe Average all plant
                            CurrentListMaterialCostVolume = CurrentListMaterialCostVolume.Where((item) => item.StdCost > 0 && EuropeRegex.IsMatch(item.PlantNumber.Number)).ToList();
                            if (CurrentListMaterialCostVolume != null && CurrentListMaterialCostVolume.Count > 0)
                            {
                                part.EuropeAverageCost = Math.Round(CurrentListMaterialCostVolume.Average((item) => McgBusinessTools.GetCurrencyFromUsd(McgBusinessTools.GetCurrencyToUsd(item.StdCost, GetPlantCurrency(item.PlantNumber)), CurrentQuickSearchDataContext.SelectedSapPlant.Currency)), 2);

                                // France Average all plant
                                CurrentListMaterialCostVolume = CurrentListMaterialCostVolume.Where((item) => item.StdCost > 0 && FranceRegex.IsMatch(item.PlantNumber.Number)).ToList();
                                if (CurrentListMaterialCostVolume != null && CurrentListMaterialCostVolume.Count > 0)
                                    part.FrenchAverageCost = Math.Round(CurrentListMaterialCostVolume.Average((item) => McgBusinessTools.GetCurrencyFromUsd(McgBusinessTools.GetCurrencyToUsd(item.StdCost, GetPlantCurrency(item.PlantNumber)), CurrentQuickSearchDataContext.SelectedSapPlant.Currency)), 2);
                            }
                        }
                    }
                }
                if (!CurrentQuickSearchDataContext.SelectedSubClassItem.IsSapSearchDone && CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentAllCostVolume != null && CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentAllCostVolume.Count > 0)
                    CurrentQuickSearchDataContext.SelectedSubClassItem.IsSapSearchDone = true;
            }
            catch (ThreadAbortException)
            {
                // Can happen when switching or applying filter
                // Seems not raise other issue so just catch to avoid error messages
            }
            catch (InvalidOperationException)
            {
                // Can happen when switching or applying filter
                // Seems not raise other issue so just catch to avoid error messages
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentQuickSearchDataContext.IsMsgSearchSap = false;
            }
        }

        private string GetPartNumber(string CompletePartNumber)
        {
            try
            {
                int index = CompletePartNumber.IndexOf(QuickSearchConstants.PartNumberSuffixSeparator);
                if (index > 1)
                    return CompletePartNumber.Substring(0, index);
                else
                    return CompletePartNumber;
            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }

        private McgCurrency GetPlantCurrency(SapPlant plantNumber)
        {
            try
            {
                var temp = CurrentQuickSearchConfiguration.ListSapPlant.Concat(CurrentQuickSearchConfiguration.ExtraListSapPlant).FirstOrDefault((item) => item.Number == plantNumber.Number);

                if (temp != null)
                    return temp.Currency;
                else
                    return McgCurrency.USD;
            }
            catch (Exception)
            {
                return McgCurrency.USD;
            }

        }
        #endregion

        #region [REGION] Misc Methods
        private void UpdateSubClassInformation()
        {
            try
            {
                if (CurrentQuickSearchDataContext.SelectedSubClassItem != null)
                {
                    if (CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentPartSubClass.Showpartpicture != null && CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentPartSubClass.Showpartpicture.ToUpper().Trim() == "TRUE")
                        CurrentQuickSearchDataContext.IsPartPictureShown = true;
                    else
                        CurrentQuickSearchDataContext.IsPartPictureShown = false;

                    var SubClass = CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentPartSubClass;
                    SetCurrentPicture($"{MainAppFolder}\\{CommonLibConstants.PictureFolder}\\{SubClass.Subclasspicture}");
                    CurrentQuickSearchDataContext.RefDocument = SubClass.Subclassrefdoc;
                    CurrentQuickSearchDataContext.IsExtraComponentPossible = SubClass.Showcontextmenu.ToUpper() == "TRUE";

                    if (CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentPartSubClass.Showhyperlink != null && CurrentQuickSearchDataContext.SelectedSubClassItem.CurrentPartSubClass.Showhyperlink.Value)
                        CurrentQuickSearchDataContext.IsRefDocHtmlLink = true;
                    else
                        CurrentQuickSearchDataContext.IsRefDocHtmlLink = false;
                }
                else
                {
                    CurrentQuickSearchDataContext.RefDocument = "";
                    SetCurrentPicture($"{MainAppFolder}\\{CommonLibConstants.PictureFolder}\\{QuickSearchConstants.MainPicture}");
                    CurrentQuickSearchDataContext.IsExtraComponentPossible = false;
                }
                CurrentQuickSearchDataContext.RaiseListSubClassChanged();
            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }

        private void SetCurrentPicture(string PictureCompleteFileName)
        {
            try
            {
                if (File.Exists(PictureCompleteFileName))
                    CurrentQuickSearchDataContext.MainPictureShown = McgWpfTools.GetBitmapImage(PictureCompleteFileName);
                else
                    CurrentQuickSearchDataContext.MainPictureShown = McgWpfTools.GetBitmapImage($"{MainAppFolder}\\{CommonLibConstants.PictureFolder}\\{QuickSearchConstants.PictureNotFound}");
            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }

        private void CheckwindchillCredential()
        {
            try
            {
                if (WindchillNetworkCredential == null || !WindchillNetworkCredential.IsCredentialOk)
                    WindchillNetworkCredential = _windchillCredentialService.GetWindchillCredential(CommonLibConstants.WindchillUrl, CommonLibConstants.WindchillUrl);
            }
            catch (Exception ex)
            {
                throw new QuickSearchException(this.GetType().Name, ex);
            }
        }
        #endregion

    }
}
