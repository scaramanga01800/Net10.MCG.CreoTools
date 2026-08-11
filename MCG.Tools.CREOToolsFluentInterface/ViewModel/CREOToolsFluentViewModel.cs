using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.Models;
using MCG.CREO_Tools.CutLengthApp.ViewModel;
using MCG.CREO_Tools.DxfExport.ViewModel;
using MCG.CREO_Tools.JpgExport.ViewModel;
using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.ProfileApp.ViewModel;
using MCG.CREO_Tools.QuickLaunch.ViewModel;
using MCG.CREO_Tools.QuickSearch.ViewModel;
using MCG.CREO_Tools.ShearedTube.ViewModel;
using MCG.Tools.CREOToolsFluentInterface.Configuration;
using MCG.Tools.EcnDataCheck.ViewModel;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using MCG.Tools.VisualizationLib.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MCG.Tools.CREOToolsFluentInterface.ViewModel
{
    public partial class CREOToolsFluentViewModel : ObservableObject
    {
        #region [REGION] Dependencies
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ISharedAppContext _sharedAppContext;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly IServiceProvider _serviceProvider;
        #endregion

        #region [REGION] Properties
        [ObservableProperty] private CREOToolsDataContext _currentDataContext = new();

        private CREOToolsConfiguration _appConfiguration;
        private CREOToolsUserConfiguration _userConfiguration;

        private string MainAppFolder { get; }
        private string UserConfigPath = CREOToolsConstants.CreoToolsUserConfigXmlFile;
        #endregion

        #region [REGION] ViewModel Properties
        private QuickLaunchViewModel _quickLaunchViewModel;
        private EcnEcoFollowUpViewModel _ecnEcoFollowUpViewModel;
        private PurchaseOrderFollowUpViewModel _purchaseOrderFollowUpViewModel;
        private ConvertToPdfViewModel _convertToPdfViewModel;
        private DownloadVisualizationFileViewModel _downloadVisualizationFileViewModel;
        private EcnDataCheckViewModel _ecnDataCheckViewModel;
        private CutLengthViewModel _cutLengthViewModel;
        private DxfExportViewModel _dxfExportViewModel;
        private JpgExportViewModel _jpgExportViewModel;
        private MassUpdateAttributeViewModel _massUpdateAttributeViewModel;
        private ProfileViewModel _profileViewModel;
        private QuickSearchViewModel _quickSearchViewModel;
        private ShearedTubeViewModel _shearedTubeViewModel;

        public QuickLaunchViewModel QuickLaunchViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_quickLaunchViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _quickLaunchViewModel = _serviceProvider.GetRequiredService<QuickLaunchViewModel>();
                }
                return _quickLaunchViewModel;
            }
        }

        public CutLengthViewModel CutLengthViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_cutLengthViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _cutLengthViewModel = _serviceProvider.GetRequiredService<CutLengthViewModel>();
                }
                return _cutLengthViewModel;
            }
        }

        public DxfExportViewModel DxfExportViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_dxfExportViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _dxfExportViewModel = _serviceProvider.GetRequiredService<DxfExportViewModel>();
                }
                return _dxfExportViewModel;
            }
        }

        public JpgExportViewModel JpgExportViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_jpgExportViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _jpgExportViewModel = _serviceProvider.GetRequiredService<JpgExportViewModel>();
                }
                return _jpgExportViewModel;
            }
        }

        public MassUpdateAttributeViewModel MassUpdateAttributeViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_massUpdateAttributeViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _massUpdateAttributeViewModel = _serviceProvider.GetRequiredService<MassUpdateAttributeViewModel>();
                }
                return _massUpdateAttributeViewModel;
            }
        }

        public ProfileViewModel ProfileViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_profileViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _profileViewModel = _serviceProvider.GetRequiredService<ProfileViewModel>();
                }
                return _profileViewModel;
            }
        }

        public QuickSearchViewModel QuickSearchViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_quickSearchViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _quickSearchViewModel = _serviceProvider.GetRequiredService<QuickSearchViewModel>();
                }
                return _quickSearchViewModel;
            }
        }

        public ShearedTubeViewModel ShearedTubeViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_shearedTubeViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _shearedTubeViewModel = _serviceProvider.GetRequiredService<ShearedTubeViewModel>();
                }
                return _shearedTubeViewModel;
            }
        }

        public EcnDataCheckViewModel EcnDataCheckViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_ecnDataCheckViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _ecnDataCheckViewModel = _serviceProvider.GetRequiredService<EcnDataCheckViewModel>();
                }
                return _ecnDataCheckViewModel;
            }
        }

        public EcnEcoFollowUpViewModel EcnEcoFollowUpViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_ecnEcoFollowUpViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _ecnEcoFollowUpViewModel = _serviceProvider.GetRequiredService<EcnEcoFollowUpViewModel>();
                }
                return _ecnEcoFollowUpViewModel;
            }
        }

        public PurchaseOrderFollowUpViewModel PurchaseOrderFollowUpViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_purchaseOrderFollowUpViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _purchaseOrderFollowUpViewModel = _serviceProvider.GetRequiredService<PurchaseOrderFollowUpViewModel>();
                }
                return _purchaseOrderFollowUpViewModel;
            }
        }

        public ConvertToPdfViewModel ConvertToPdfViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_convertToPdfViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _convertToPdfViewModel = _serviceProvider.GetRequiredService<ConvertToPdfViewModel>();
                }
                return _convertToPdfViewModel;
            }
        }

        public DownloadVisualizationFileViewModel DownloadVisualizationFileViewModelVM
        {
            get
            {
                // Si c'est la 1ère fois qu'on clique sur l'onglet :
                if (_downloadVisualizationFileViewModel == null)
                {
                    // Le système crée le ViewModel ET lui injecte ISapHupService tout seul !
                    _downloadVisualizationFileViewModel = _serviceProvider.GetRequiredService<DownloadVisualizationFileViewModel>();
                }
                return _downloadVisualizationFileViewModel;
            }
        }
        #endregion

        #region [REGION] Constructor
        public CREOToolsFluentViewModel(IXmlSerializeTools xmlSerializeTools,
                                        ICreoSessionProvider creoSessionProvider,
                                        ISharedAppContext sharedAppContext,
                                        IWindchillCredentialService windchillCredentialService,
                                        IServiceProvider serviceProvider)
        {
            _xmlSerializeTools = xmlSerializeTools;
            _creoSessionProvider = creoSessionProvider;
            _sharedAppContext = sharedAppContext;
            _windchillCredentialService = windchillCredentialService;
            _serviceProvider = serviceProvider;

            MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
            if (MainAppFolder == null || MainAppFolder == "")
                MainAppFolder = CommonLibConstants.MainAppFolder;
        }
        #endregion

        #region [REGION] Commands
        public ICommand CommandShowHelp { get => new RelayCommand(() => ExecuteShowHelp()); }
        public ICommand CommandAboutCreoTools { get => new RelayCommand(() => ExecuteAboutCreoTools()); }
        public ICommand CommandExit { get => new RelayCommand(() => Application.Current.Shutdown()); }
        public ICommand CommandChangeWindchillCredential { get => new RelayCommand(() => ExecuteChangeWindchillCredential()); }
        public ICommand CommandUpdateUserSetting { get => new RelayCommand(() => UpdateUserConfigXmlFile()); }
        public ICommand CommandUpdateScrollingText { get => new RelayCommand(() => ExecuteUpdateScrollingText()); }
        public ICommand CommandReleaseNotes { get => new RelayCommand(() => ExecuteReleaseNotes()); }
        public ICommand CommandOpenUtilitiesOverview { get => new RelayCommand(() => ExecuteOpenUtilitiesOverview()); }
        #endregion

        #region [REGION] Initialization
        public async Task InitializeAsync()
        {
            try
            {
                LoadConfigurations();
                InitAppAvailability();
                InitTheme();
                InitFonts();
                InitLanguages();
                PublishToSharedContext();

                // Init au démarrage
                _sharedAppContext.CurrentLanguage = _userConfiguration.CurrentLang;
                _sharedAppContext.AppAvailable = CurrentDataContext.AppAvailable;
                _sharedAppContext.AppVisible = CurrentDataContext.AppVisible;

                _creoSessionProvider.ConnectionEnd += _creoSessionProvider_ConnectionEnd;

                CurrentDataContext.ColorInterfaceChangeEvent += (sender, e) => UpdateUserConfigXmlFile();
                CurrentDataContext.FontInterfaceChangeEvent += (sender, e) => UpdateUserConfigXmlFile();

                CurrentDataContext.CurrentUser = McgActiveDirectoryTools.GetWindowsSessionUserFullName();
                CurrentDataContext.MachineName = System.Environment.MachineName;
                CurrentDataContext.CurrentLang = _userConfiguration.CurrentLang;
                // 🟢 Application initiale du thème
                //UpdateThemeAndAccent();

                await ConnectToCreoAsync();
            }
            catch (Exception ex)
            {
                TraceLog.AddTraceLog($"InitializeAsync failed: {ex.Message}");
                CREOToolsException.SendMessageBox(nameof(InitializeAsync), ex);
            }
        }

        private void _creoSessionProvider_ConnectionEnd(object? sender, bool e)
        {
            CurrentDataContext.IsCreoConnected = _creoSessionProvider.IsConnected;

        }

        private void LoadConfigurations()
        {
            try
            {
                _appConfiguration = _xmlSerializeTools.GetDeserializedXml<CREOToolsConfiguration>(Path.Combine(MainAppFolder, CommonLibConstants.ResourcesFolder, "CREOToolsConfiguration.xml"));
                _userConfiguration = _xmlSerializeTools.GetDeserializedXmlFromAppData<CREOToolsUserConfiguration>(CREOToolsConstants.CreoToolsUserConfigXmlFile) ?? new CREOToolsUserConfiguration();

                // Langue Windows
                if (_userConfiguration.CurrentLang == null) 
                {
                    _userConfiguration.CurrentLang = GetDefaultLanguage(_appConfiguration); 
                }

                // Police par défaut
                if (string.IsNullOrWhiteSpace(_userConfiguration.DefaultFont))
                {
                    _userConfiguration.DefaultFont = "Segoe UI";
                }

                // Theme par défaut
                if (!_userConfiguration.IsDark && !_userConfiguration.IsLight)
                {
                    _userConfiguration.IsLight = true;
                }

                // Version config
                if (string.IsNullOrWhiteSpace(_userConfiguration.ConfigVersion))
                {
                    _userConfiguration.ConfigVersion = "12.10";
                }

                // Couleur
                if (string.IsNullOrWhiteSpace(_userConfiguration.ColorScheme))
                {
                    _userConfiguration.ColorScheme = "Blue";
                }

                // AppVisible => toutes les apps disponibles
                if (_userConfiguration.AppVisible == null )
                {
                    _userConfiguration.AppVisible = _appConfiguration.AppAvailable ;
                }
            }
            catch (Exception ex)
            {
                TraceLog.AddTraceLog($"LoadConfigurations failed: {ex.Message}");
                throw;
            }
        }

        private CREOToolsLanguageSelection GetDefaultLanguage(CREOToolsConfiguration _appConfiguration)
        {
            string currentCulture =
                CultureInfo.CurrentUICulture.Name.ToLowerInvariant();

            // Chinois
            if (currentCulture.StartsWith("zh"))
                return _appConfiguration.LangCn ?? _appConfiguration.LangEn;

            // Allemand
            if (currentCulture.StartsWith("de"))
                return _appConfiguration.LangDe ?? _appConfiguration.LangEn;

            // Français
            if (currentCulture.StartsWith("fr"))
                return _appConfiguration.LangFr ?? _appConfiguration.LangEn;

            // Anglais
            if (currentCulture.StartsWith("en"))
                return _appConfiguration.LangEn ?? _appConfiguration.LangEn;

            // Fallback
            return _appConfiguration.LangEn ?? _appConfiguration.LangEn;
        }


        private void InitAppAvailability()
        {
            CurrentDataContext.AppAvailable = _appConfiguration.AppAvailable.GetCREOToolsAppAvailability();
            CurrentDataContext.AppVisible = _userConfiguration.AppVisible.GetCREOToolsAppAvailability()
                                              ?? _appConfiguration.AppAvailable.GetCREOToolsAppAvailability();

        }

        private void InitTheme()
        {
            CurrentDataContext.IsDark = _userConfiguration.IsDark;
            CurrentDataContext.IsLight = !_userConfiguration.IsDark;
            CurrentDataContext.SelectedColorScheme = _userConfiguration.ColorScheme ?? CurrentDataContext.ListColorSchemes.FirstOrDefault();
        }

        private void InitFonts()
        {
            var fonts = Fonts.SystemFontFamilies
                .Select(f => f.Source)
                .OrderBy(name => name)
                .ToList();

            if (fonts.Count == 0)
                fonts.Add("Segoe UI");

            foreach (var f in fonts)
                CurrentDataContext.ListFont.Add(f);

            CurrentDataContext.SelectedFont =
                !string.IsNullOrEmpty(_userConfiguration.DefaultFont)
                    ? _userConfiguration.DefaultFont
                    : "Segoe UI";
        }

        private void InitLanguages()
        {
            // 1️⃣ Langues récupérées depuis la config App
            CurrentDataContext.LangCn = _appConfiguration.LangCn;
            CurrentDataContext.LangEn = _appConfiguration.LangEn;
            CurrentDataContext.LangFr = _appConfiguration.LangFr;
            CurrentDataContext.LangDe = _appConfiguration.LangDe;

            var langs = new[]
            {
                CurrentDataContext.LangCn,
                CurrentDataContext.LangEn,
                CurrentDataContext.LangFr,
                CurrentDataContext.LangDe
            };

            // 2️⃣ Branchement direct du handler unique
            foreach (var l in langs)
                l.IsSelectedEvent += UpdateLanguageInterface;

            // 3️⃣ Sélection initiale
            var userLang = _userConfiguration?.CurrentLang;

            if (userLang != null)
            {
                UpdateLanguageInterface(userLang, EventArgs.Empty);

                foreach (var l in langs)
                    l.IsSelected = false;

                var match = langs.FirstOrDefault(l =>
                    l.Language.Language == userLang.Language.Language);

                if (match != null)
                    match.IsSelected = true;
            }
            else
            {
                var preselected = langs.FirstOrDefault(l => l.IsSelected);
                if (preselected != null)
                    UpdateLanguageInterface(preselected, EventArgs.Empty);
            }
        }

        private void PublishToSharedContext()
        {
            _sharedAppContext.AppAvailable = CurrentDataContext.AppAvailable;
            _sharedAppContext.AppVisible = CurrentDataContext.AppVisible;
            _sharedAppContext.CurrentLanguage = CurrentDataContext.CurrentLang;
        }

        private async Task ConnectToCreoAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    var status = _creoSessionProvider.Connect(false);
                    CurrentDataContext.IsCreoConnected = status == CreoConnectionStatus.OK;
                }
                catch (Exception ex)
                {
                    TraceLog.AddTraceLog($"CREO connection failed: {ex.Message}");
                }
            });
        }
        #endregion

        #region [REGION] Save user config
        private void UpdateLanguageInterface(object sender, EventArgs e)
        {
            try
            {
                TraceLog.AddTraceLog("Enter UpdateLanguageInterface");
                if (sender != null && sender.GetType() == typeof(CREOToolsLanguageSelection))
                {
                    CREOToolsLanguageSelection TempLanguage = (CREOToolsLanguageSelection)sender;
                    if (TempLanguage.IsSelected)
                    {
                        CurrentDataContext.CurrentLang = TempLanguage;
                        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(TempLanguage.Language.CultureInfo);
                        McgWpfTools.UpdateMergeDictionaries();
                        UpdateUserConfigXmlFile();
                        CurrentDataContext.RaiseColorInterfaceChangeEvent();
                        // CurrentMCGLanguage.RaiseChangeLanguageInterfaceEvent();
                        _sharedAppContext.CurrentLanguage = _userConfiguration.CurrentLang;
                    }
                }
                TraceLog.AddTraceLog("End UpdateLanguageInterface");
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
                TraceLog.AddTraceLog("Issue UpdateLanguageInterface");
            }
        }

        public void UpdateUserConfigXmlFile()
        {
            try
            {
                _userConfiguration.IsDark = CurrentDataContext.IsDark;
                _userConfiguration.ColorScheme = CurrentDataContext.SelectedColorScheme;
                _userConfiguration.DefaultFont = CurrentDataContext.SelectedFont;
                _userConfiguration.CurrentLang = CurrentDataContext.CurrentLang;
                _userConfiguration.AppVisible = CurrentDataContext.AppVisible.GetCREOToolsAppAvailability();

                _xmlSerializeTools.SerializedXmlInAppData<CREOToolsUserConfiguration>(_userConfiguration, UserConfigPath);
            }
            catch (Exception ex)
            {
                TraceLog.AddTraceLog($"UpdateUserConfigXmlFile failed: {ex.Message}");
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteShowHelp()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("CT_LinkHelpCreoTools"));
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAboutCreoTools()
        {
            try
            {
                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("CT_AboutText"), "\n", CREOToolsConstants.Version, CREOToolsConstants.Year),
                                McgWpfTools.GetStringResource("CT_AboutTextTitle"),
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteChangeWindchillCredential()
        {
            try
            {
                _windchillCredentialService.UpdateWindchillCredential($"{CommonLibConstants.WindchillUrl}/");
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateScrollingText()
        {
            try
            {
                CurrentDataContext.ScrollingText = $@"Welcome to CREO Tools. {DateTime.Today,0:dd/MM/yyyy} at {DateTime.Now.TimeOfDay.Hours.ToString("00")}:{DateTime.Now.TimeOfDay.Minutes.ToString("00")}|[Google link<http:\\google.fr>]|[Consulter le nouvelle méthodologie<file:\\O:\CREO_Config\TWR\application\ProE_Methodologies\PDM\PDM000EN.pdf>]";
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteReleaseNotes()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("CT_LinkReleaseNotesCreoTools"));
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenUtilitiesOverview()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("CT_LinkUtilitiesOverviews"));
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
