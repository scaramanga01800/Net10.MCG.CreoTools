using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CREO_Tools.DxfExport.Interfaces;
using MCG.CREO_Tools.MassUpdateAttribute.Interfaces;
using MCG.CREO_Tools.MiscTools.Interfaces;
using MCG.CREO_Tools.QuickLaunch.Configuration;
using MCG.CREO_Tools.QuickLaunch.Exceptions;
using MCG.CREO_Tools.QuickLaunch.View;
using MCG.Tools.NumberingTool.Interfaces;
using MCG.WindchillTools.ManageWTObject.Interfaces;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.QuickLaunch.ViewModel
{
    public class QuickLaunchViewModel : ObservableObject, IQuickLaunchViewModel
    {
        private string MainAppFolder { get; set; }

        #region [REGION] Properties From Interface
        private CREOToolsAppAvailability _AppVisible;
        public CREOToolsAppAvailability AppVisible
        {
            get { return this._AppVisible; }
            set
            {
                if (this._AppVisible != value)
                {
                    this._AppVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        private CREOToolsAppAvailability _AppAvailable;
        public CREOToolsAppAvailability AppAvailable
        {
            get { return this._AppAvailable; }
            set
            {
                if (this._AppAvailable != value)
                {
                    this._AppAvailable = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsCreoConnected;
        public bool IsCreoConnected
        {
            get { return this._IsCreoConnected; }
            set
            {
                if (this._IsCreoConnected != value)
                {
                    this._IsCreoConnected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsCreoConnectionInProgress;
        public bool IsCreoConnectionInProgress
        {
            get { return this._IsCreoConnectionInProgress; }
            set
            {
                if (this._IsCreoConnectionInProgress != value)
                {
                    this._IsCreoConnectionInProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _HtmlPage;
        public string HtmlPage
        {
            get { return _HtmlPage; }
            set
            {
                if (this._HtmlPage != value)
                {
                    this._HtmlPage = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region [REGION] Events From Interface
        public event EventHandler HtmlPageChangedEvent;

        public void RaiseHtmlPageChangedEvent()
        {
            try
            {
                HtmlPageChangedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Commands
        public ICommand CommandNewCadDocument { get => new RelayCommand(() => ExecuteNewCadDocument()); }
        public ICommand CommandWebterm { get => new RelayCommand<bool>((param) => ExecuteWebterm(param)); }
        public ICommand CommandPartNumberGenerator { get => new RelayCommand(() => ExecutePartNumberGenerator()); }
        public ICommand CommandConnectCreoSession { get => new RelayCommand(() => ExecuteConnectCreoSession()); }
        public ICommand CommandExportBom { get => new RelayCommand(() => ExecuteExportBom()); }
        public ICommand CommandPartNumberCreator { get => new RelayCommand(() => ExecutePartNumberCreator()); }
        public ICommand CommandDxfDwgDrawingExport { get => new RelayCommand(() => ExecuteDxfDwgDrawingExport()); }
        public ICommand CommandBackUpCadDocument { get => new RelayCommand(() => ExecuteBackUpCadDocument()); }
        public ICommand CommandMcgHelpOnline { get => new RelayCommand<bool>((param) => ExecuteMcgHelpOnline(param)); }
        public ICommand CommandEngTime { get => new RelayCommand<bool>((param) => ExecuteEngTime(param)); }
        public ICommand CommandMechanismAnalysis { get => new RelayCommand(() => ExecuteMechanismAnalysis()); }
        public ICommand CommandCreateUpdateWtDocPart { get => new RelayCommand(() => ExecuteCreateUpdateWtDocPart()); }
        public ICommand CommandKillCreoProcesses { get => new RelayCommand(() => ExecuteKillCreoProcesses()); }
        public ICommand CommandCadAutoColor { get => new RelayCommand(() => ExecuteCadAutoColor()); }
        public ICommand CommandNumberCumulation { get => new RelayCommand(() => ExecuteNumberCumulation()); }
        public ICommand CommandBomComparison { get => new RelayCommand(() => ExecuteBomComparison()); }
        public ICommand CommandSapExportBom { get => new RelayCommand(() => ExecuteSapExportBom()); }
        public ICommand CommandSapBomExportAllLevel { get => new RelayCommand(() => ExecuteSapBomExportAllLevel()); }
        public ICommand CommandSapFertBom { get => new RelayCommand(() => ExecuteSapFertBom()); }
        public ICommand CommandWebtermRequest { get => new RelayCommand(() => ExecuteWebtermRequest()); }
        public ICommand CommandCraneSearch { get => new RelayCommand(() => ExecuteCraneSearch()); }
        public ICommand CommandQuickChange { get => new RelayCommand(() => ExecuteQuickChange()); }
        public ICommand CommandCadDocumentRename { get => new RelayCommand(() => ExecuteCadDocumentRename()); }
        public ICommand CommandBomEnvirConfig { get => new RelayCommand(() => ExecuteBomEnvirConfig()); }
        public ICommand CommandUpdateRelationsParameters { get => new RelayCommand(() => ExecuteUpdateRelationsParameters()); }
        public ICommand CommandOpenCall { get => new RelayCommand<bool>((param) => ExecuteOpenCall(param)); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly IMassUpdateAttributeWindowService _massUpdateAttributeWindowService;
        private readonly IHtmlTools _htmlTools;
        private readonly IMiscToolsWindchillService _miscToolsWindchillService;
        private readonly INumberingToolWindowService _numberingToolWindowService;
        private readonly IDxfExportWindchillService _dxfExportWindchillService;
        private readonly IMcgWindchillToolsManageWTObjectWindowService _mcgWindchillToolsManageWTObjectWindowService;
        private readonly ISharedAppContext _sharedAppContext;

        public QuickLaunchViewModel(ICreoSessionProvider creoSessionProvider,
                                    IMassUpdateAttributeWindowService massUpdateAttributeWindowService,
                                    IHtmlTools htmlTools,
                                    IMiscToolsWindchillService miscToolsWindchillService,
                                    INumberingToolWindowService numberingToolWindowService,
                                    IDxfExportWindchillService dxfExportWindchillService,
                                    IMcgWindchillToolsManageWTObjectWindowService mcgWindchillToolsManageWTObjectWindowService,
                                    ISharedAppContext sharedAppContext)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _massUpdateAttributeWindowService = massUpdateAttributeWindowService;
                _htmlTools = htmlTools;
                _miscToolsWindchillService = miscToolsWindchillService;
                _numberingToolWindowService = numberingToolWindowService;
                _dxfExportWindchillService = dxfExportWindchillService;
                _mcgWindchillToolsManageWTObjectWindowService = mcgWindchillToolsManageWTObjectWindowService;
                _sharedAppContext = sharedAppContext;


                //var creoConnectionStatus = _creoSessionProvider.Connect(false);
                //IsCreoConnected = creoConnectionStatus == CreoConnectionStatus.OK;

                _creoSessionProvider.ConnectionStateChanged += (sender, e) => IsCreoConnected = e;

                _creoSessionProvider.ConnectionStart += (sender, e) => IsCreoConnectionInProgress = true;
                _creoSessionProvider.ConnectionEnd += (sender, e) => IsCreoConnectionInProgress = false;

                HtmlPage = QuickLaunchConstants.HtmlLinkMcgDocumentation;

                AppVisible = _sharedAppContext.AppVisible;
                AppAvailable = _sharedAppContext.AppAvailable;
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public async Task ConnectToCreoAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    var status = _creoSessionProvider.Connect(false);
                    IsCreoConnected = status == CreoConnectionStatus.OK;
                }
                catch (Exception ex)
                {
                    TraceLog.AddTraceLog($"CREO connection failed: {ex.Message}");
                }
            });
        }

        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteNewCadDocument()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter CreateNewCadDocumentViewModel App");
                _massUpdateAttributeWindowService.ShowCreateNewCadDocumentFluentWindow(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteWebterm(bool StartNewBrowser = true)
        {
            try
            {
                HtmlPage = QuickLaunchConstants.HtmlLinkWebterm;
                RaiseHtmlPageChangedEvent();
                if (StartNewBrowser)
                    _htmlTools.OpenUrlInIternetExplorer(HtmlPage);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecutePartNumberGenerator()
        {
            try
            {
                _htmlTools.OpenUrlInIternetExplorer(QuickLaunchConstants.HtmlLinkPatNumberGenerator);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteConnectCreoSession()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter StartConnectCreoSessionAsynch App");
                new Thread(new ThreadStart(StartConnectCreoSessionAsynch)).Start();
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteExportBom()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter CurrentBomExportWindowView App");
                _miscToolsWindchillService.ShowBomExportFluentWindowView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecutePartNumberCreator()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecutePartNumberCreator App");
                _numberingToolWindowService.ShowNumberingToolFluentMainView(false);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDxfDwgDrawingExport()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteDxfDwgDrawingExport App");
                _dxfExportWindchillService.ShowDxfDwgDrawingExportMainView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteBackUpCadDocument()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteBackUpCadDocument App");
                _dxfExportWindchillService.ShowBackUpCadDocumentView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMcgHelpOnline(bool StartNewBrowser = true)
        {
            try
            {
                HtmlPage = QuickLaunchConstants.HtmlLinkMcgDocumentation;
                RaiseHtmlPageChangedEvent();
                if (StartNewBrowser)
                    _htmlTools.OpenUrlInIternetExplorer(HtmlPage);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteEngTime(bool StartNewBrowser = true)
        {
            try
            {
                HtmlPage = QuickLaunchConstants.HtmlLinkEngTime;
                RaiseHtmlPageChangedEvent();
                if (StartNewBrowser)
                    _htmlTools.OpenUrlInIternetExplorer(HtmlPage);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMechanismAnalysis()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteMechanismAnalysis App");
                _miscToolsWindchillService.ShowMechanismAnalysisMainView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreateUpdateWtDocPart()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteMechanismAnalysis App");
                _mcgWindchillToolsManageWTObjectWindowService.ShowDialogCreateUpdateWtDocumentWtPartViewModel(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteKillCreoProcesses()
        {
            try
            {

                TraceLog.AddTraceLog($"Enter ExecuteKillCreoProcesses App");
                if (MessageBox.Show(McgWpfTools.GetStringResource("QL_KillCreo"), McgWpfTools.GetStringResource("QL_KillCreoMsgTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    McgFileAndSystemTools.KillAllProcesses("zbcefr");
                    McgFileAndSystemTools.KillAllProcesses("xtop");
                    McgFileAndSystemTools.KillAllProcesses("pro_comm_msg");
                    McgFileAndSystemTools.KillAllProcesses("pfclscom");
                    McgFileAndSystemTools.KillAllProcesses("nmsd");
                    McgFileAndSystemTools.KillAllProcesses("creoagent");
                }
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCadAutoColor()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteCadAutoColor App");
                _miscToolsWindchillService.ShowCadAutoColorMainView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteNumberCumulation()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteNumberCumulation App");
                _miscToolsWindchillService.ShowNumberCumulationMainView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteBomComparison()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteBomComparison App");
                _miscToolsWindchillService.ShowBomComparisonView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSapExportBom()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteSapExportBom App");
                _miscToolsWindchillService.ShowSapBomExportMainView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSapBomExportAllLevel()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteSapBomExportAllLevel App");
                _miscToolsWindchillService.ShowSapBomExportAllLevelMainView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSapFertBom()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteSapFertExport App");
                _miscToolsWindchillService.ShowSapFertBomMainView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteWebtermRequest()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteWebtermRequest App");
                _miscToolsWindchillService.ShowWebtermRequestMainView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCraneSearch()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteCraneSearch App");
                _miscToolsWindchillService.ShowCraneSearchMainView(null, true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteQuickChange()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteQuickChange App");
                _miscToolsWindchillService.ShowQuickChangeMainView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCadDocumentRename()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteCadDocumentRename App");
                _miscToolsWindchillService.ShowCadDocRenameMainView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteBomEnvirConfig()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteBomEnvirConfig App");
                _miscToolsWindchillService.ShowBomEnvirConfigMainView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateRelationsParameters()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteUpdateRelationsParameters App");
                _massUpdateAttributeWindowService.ShowUpdateRelationsParametersMainView(true);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenCall(bool StartNewBrowser = true)
        {
            try
            {
                HtmlPage = QuickLaunchConstants.HtmlLinkOpenCall;
                RaiseHtmlPageChangedEvent();
                if (StartNewBrowser)
                    _htmlTools.OpenUrlInIternetExplorer(HtmlPage);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        private void StartConnectCreoSessionAsynch()
        {
            try
            {
                CreoConnectionStatus CreoCnxStatus = _creoSessionProvider.Connect(true);
                switch (CreoCnxStatus)
                {
                    case CreoConnectionStatus.OK:
                        break;
                    case CreoConnectionStatus.NO_CREO:
                        MessageBox.Show(McgWpfTools.GetStringResource("QL_CreoCnxNoCREO"), McgWpfTools.GetStringResource("QL_CreoCnxMsgTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    case CreoConnectionStatus.SEVERAL_CREO:
                        MessageBox.Show(McgWpfTools.GetStringResource("QL_CreoCnxSeveralCREO"), McgWpfTools.GetStringResource("QL_CreoCnxMsgTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    case CreoConnectionStatus.NO_NMSD:
                        MessageBox.Show(McgWpfTools.GetStringResource("QL_CreoCnxNoNmsd"), McgWpfTools.GetStringResource("QL_CreoCnxMsgTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    case CreoConnectionStatus.NO_PRO_COMM_MSG_EXE:
                        MessageBox.Show(McgWpfTools.GetStringResource("QL_CreoCnxNoEnvProCom"), McgWpfTools.GetStringResource("QL_CreoCnxMsgTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    case CreoConnectionStatus.START_CREO_ISSUE:
                        MessageBox.Show(McgWpfTools.GetStringResource("QL_CreoCnxCreoStartIssue"), McgWpfTools.GetStringResource("QL_CreoCnxMsgTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    case CreoConnectionStatus.UNKNOWN_ERROR:
                        MessageBox.Show(McgWpfTools.GetStringResource("QL_CreoCnxUnknownError"), McgWpfTools.GetStringResource("QL_CreoCnxMsgTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    case CreoConnectionStatus.API_NOT_FOUND:
                        MessageBox.Show(McgWpfTools.GetStringResource("QL_CreoCnxNoApi"), McgWpfTools.GetStringResource("QL_CreoCnxMsgTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    default:
                        MessageBox.Show(McgWpfTools.GetStringResource("QL_CreoCnxUnknownError"), McgWpfTools.GetStringResource("QL_CreoCnxMsgTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new QuickLaunchException(this.GetType().Name, ex);
            }
        }
    }
}
