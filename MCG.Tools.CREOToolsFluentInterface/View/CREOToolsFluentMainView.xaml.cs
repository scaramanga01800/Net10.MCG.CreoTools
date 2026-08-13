using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.CutLengthApp.View;
using MCG.CREO_Tools.DxfExport.View;
using MCG.CREO_Tools.JpgExport.View;
using MCG.CREO_Tools.MassUpdateAttribute.View;
using MCG.CREO_Tools.ProfileApp.View;
using MCG.CREO_Tools.QuickSearch.View;
using MCG.CREO_Tools.ShearedTube.View;
using MCG.Tools.CREOToolsFluentInterface.Configuration;
using MCG.Tools.CREOToolsFluentInterface.ViewModel;
using MCG.Tools.EcnDataCheck.View;
using MCG.Tools.EcnEcoFollowUp.View;
using MCG.Tools.PurchaseOrderFollowUp.View;
using MCG.Tools.VisualizationLib.View;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MCG.Tools.CREOToolsFluentInterface.View
{
    public partial class CREOToolsFluentMainView : RibbonWindow
    {
        private CREOToolsFluentViewModel CurrentDataContext { get; set; }
        private string MainAppFolder { get; set; }

        public object? CurrentTabContent
        {
            get => GetValue(CurrentTabContentProperty);
            set => SetValue(CurrentTabContentProperty, value);
        }

        public static readonly DependencyProperty CurrentTabContentProperty =
            DependencyProperty.Register(
                nameof(CurrentTabContent),
                typeof(object),
                typeof(CREOToolsFluentMainView),
                new PropertyMetadata(null));

        private readonly IServiceProvider _serviceProvider;

        public CREOToolsFluentMainView(CREOToolsFluentViewModel currentViewModel, IServiceProvider serviceProvider)
        {
            try
            {
                TraceLog.AddTraceLog($"Start CREOToolsFluentMainView");
                _serviceProvider = serviceProvider;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{CREOToolsConstants.MainDictionary}", UriKind.Absolute);

                TraceLog.StartTimer(nameof(InitializeComponent));
                InitializeComponent();
                TraceLog.StopTimer(nameof(InitializeComponent));

                CurrentDataContext = currentViewModel;
                DataContext = CurrentDataContext;
                Loaded += async (s, e) => await currentViewModel.InitializeAsync();

                CurrentDataContext.CurrentDataContext.ColorInterfaceChangeEvent += UpdateColorInterface;
                UpdateColorInterface(null, null);

                CurrentDataContext.CurrentDataContext.FontInterfaceChangeEvent += CurrentCREOToolsDataContext_FontInterfaceChangeEvent;
                CurrentCREOToolsDataContext_FontInterfaceChangeEvent(null, null);

                McgWpfTools.UpdateMergeDictionaries();

                MainRibbon.SelectedTabChanged += MainRibbon_SelectionChanged;
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void MainRibbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (MainRibbon.SelectedTabItem == TabCutLengthApp)
                {
                    EnsureCutLengthLoaded();
                }
                else if (MainRibbon.SelectedTabItem == TabQuickSearchApp)
                {
                    EnsureQuickSearchLoaded();
                }
                else if (MainRibbon.SelectedTabItem == TabDownloadVisualizationFileApp)
                {
                    EnsureDownloadVisuFileLoaded();
                }
                else if (MainRibbon.SelectedTabItem == TabEcnDataCheckApp)
                {
                    EnsureEcnDataCheckLoaded();
                }
                else if (MainRibbon.SelectedTabItem == TabMassUpdateAttributeApp)
                {
                    EnsureMassUpdateAttributeLoaded();
                }
                else if (MainRibbon.SelectedTabItem == TabEcnEcoFollowUpApp)
                {
                    EnsureEcnEcoFollowUpLoaded();
                }
                else if (MainRibbon.SelectedTabItem == TabPurchaseOrderFollowUpApp)
                {
                    EnsurePurchaseOrderViewLoaded();
                }
                else if (MainRibbon.SelectedTabItem == TabProfileApp)
                {
                    EnsureProfileViewLoaded();
                }
                else if (MainRibbon.SelectedTabItem == TabConvertToPdfApp)
                {
                    EnsureConvertToPdfViewLoaded();
                }
                else if (MainRibbon.SelectedTabItem == TabShearedTubeApp)
                {
                    EnsureShearedTubeViewLoaded();
                }
                else if (MainRibbon.SelectedTabItem == TabDxfExportApp)         
                {
                    EnsureDxfExportViewLoaded();
                }
                else if (MainRibbon.SelectedTabItem == TabJpgExportApp)
                {
                    EnsureJpgExportViewLoaded();
                }
            }
            catch (Exception ex)
            {
                TraceLog.AddTraceLog(ex.ToString());
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            TraceLog.StartTimer(nameof(ContentRendered));
            base.OnContentRendered(e);
            TraceLog.AddTraceLog("End OnContentRendered");
            TraceLog.StopTimer(nameof(ContentRendered));
        }
        private void Globe_LanguageSelected(object? sender, string culture)
        {
            // DataContext est le VM parent ; on récupère "CurrentDataContext"
            if (DataContext is not { } vm) return;

            // Récupère l'objet CurrentDataContext par reflection ou cast fort
            dynamic ctx = vm;
            var current = ctx.CurrentDataContext;

            // Reset tout le monde puis coche la bonne langue
            current.LangCn.IsSelected = (culture == "zh-CN");
            current.LangEn.IsSelected = (culture == "en-US");
            current.LangFr.IsSelected = (culture == "fr-FR");
            current.LangDe.IsSelected = (culture == "de-DE");
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMax_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Double-clic = maximize / restore
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
                return;
            }

            // Simple drag
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CurrentCREOToolsDataContext_FontInterfaceChangeEvent(object sender, EventArgs e)
        {
            try
            {
                if (CurrentDataContext.CurrentDataContext.SelectedFont != null && CurrentDataContext.CurrentDataContext.SelectedFont.Trim() != "")
                    UpdateFontInterface(CurrentDataContext.CurrentDataContext.SelectedFont);
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateFontInterface(string NewFont)
        {
            try
            {
                this.FontFamily = new FontFamily(NewFont);
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateColorInterface(object sender, EventArgs e)
        {
            try
            {
                TraceLog.AddTraceLog("Start CREOToolsFluentMainView.UpdateColorInterface");
                if (CurrentDataContext.CurrentDataContext.IsDark)
                {
                    ControlzEx.Theming.ThemeManager.Current.ChangeTheme(Application.Current, $"Dark.{CurrentDataContext.CurrentDataContext.SelectedColorScheme}");
                    McgWpfTools.MergeDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{CommonLibConstants.InterfaceDictionaryDark}", UriKind.Absolute);
                }
                else
                {
                    ControlzEx.Theming.ThemeManager.Current.ChangeTheme(Application.Current, $"Light.{CurrentDataContext.CurrentDataContext.SelectedColorScheme}");
                    McgWpfTools.MergeDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{CommonLibConstants.InterfaceDictionaryLight}", UriKind.Absolute);
                }
                TraceLog.AddTraceLog("End CREOToolsFluentMainView.UpdateColorInterface");
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateColorInterfaceOnFly(string Tempcolor)
        {
            try
            {
                if (Tempcolor != null)
                    if (CurrentDataContext.CurrentDataContext.IsDark)
                        ControlzEx.Theming.ThemeManager.Current.ChangeTheme(Application.Current, $"Dark.{Tempcolor}");
                    else
                        ControlzEx.Theming.ThemeManager.Current.ChangeTheme(Application.Current, $"Light.{Tempcolor}");
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void DisplayEvent_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender != null && sender.GetType() == typeof(TextBlock) && ((TextBlock)sender).DataContext != null && ((TextBlock)sender).DataContext.GetType() == typeof(string))
                {
                    String TempColor = ((TextBlock)sender).DataContext.ToString();
                    UpdateColorInterfaceOnFly(TempColor);
                }
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void DisplayEvent_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                UpdateColorInterface(null, null);
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void DisplayEvent_MouseEnterFont(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender != null && sender.GetType() == typeof(TextBlock) && ((TextBlock)sender).DataContext != null && ((TextBlock)sender).DataContext.GetType() == typeof(string))
                {
                    string TempFont = ((TextBlock)sender).DataContext.ToString();
                    UpdateFontInterface(TempFont);
                }
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void DisplayEvent_MouseLeaveFont(object sender, MouseEventArgs e)
        {
            try
            {
                UpdateFontInterface(CurrentDataContext.CurrentDataContext.SelectedFont);
            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }


        private TTab LoadRibbonTab<TTab>(RibbonTabItem placeHolder, ref TTab? loadedTab, object viewModel) where TTab : RibbonTabItem
        {
            if (loadedTab != null)
            {
                CurrentTabContent = viewModel;
                return loadedTab;
            }

            var sw = Stopwatch.StartNew();

            TraceLog.AddTraceLog(
                $"Lazy loading {typeof(TTab).Name}...");

            loadedTab =
                _serviceProvider.GetRequiredService<TTab>();

            loadedTab.DataContext = viewModel;

            CurrentTabContent = viewModel;

            int index =
                MainRibbon.Tabs.IndexOf(placeHolder);

            placeHolder.Visibility =
                Visibility.Collapsed;

            MainRibbon.Tabs.Insert(
                index + 1,
                loadedTab);

            loadedTab.IsSelected = true;

            TraceLog.AddTraceLog(
                $"Lazy loading {typeof(TTab).Name} : {sw.ElapsedMilliseconds} ms");

            return loadedTab;
        }

        // ECN Data Check Tab
        private EcnDataCheckRibbonTabView? _ecnDataCheckView;
        private bool _ecnDataCheckLoaded;

        private void EnsureEcnDataCheckLoaded()
        {
            if (_ecnDataCheckLoaded)
                return;
            _ecnDataCheckLoaded = true;
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var vm =
                        ((CREOToolsFluentViewModel)DataContext)
                        .EcnDataCheckViewModelVM;
                    LoadRibbonTab(
                        TabEcnDataCheckApp,
                        ref _ecnDataCheckView,
                        vm);
                }),
                DispatcherPriority.Background);
        }

        // Mass Update Attribute Tab
        private MassUpdateAttributeFluentTabMainView? _massUpdateAttributeView;
        private bool _massUpdateAttributeLoaded;

        private void EnsureMassUpdateAttributeLoaded()
        {
            if (_massUpdateAttributeLoaded)
                return;
            _massUpdateAttributeLoaded = true;
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var vm =
                        ((CREOToolsFluentViewModel)DataContext)
                        .MassUpdateAttributeViewModelVM;
                    LoadRibbonTab(
                        TabMassUpdateAttributeApp,
                        ref _massUpdateAttributeView,
                        vm);
                }),
                DispatcherPriority.Background);
        }

        // Cut Lenght Tab
        private CutLengthMainView? _cutLengthView;
        private bool _cutLengthLoaded;

        private void EnsureCutLengthLoaded()
        {
            if (_cutLengthLoaded)
                return;

            _cutLengthLoaded = true;

            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var vm =
                        ((CREOToolsFluentViewModel)DataContext)
                        .CutLengthViewModelVM;

                    LoadRibbonTab(
                        TabCutLengthApp,
                        ref _cutLengthView,
                        vm);
                }),
                DispatcherPriority.Background);
        }

        // Quick Search Tab
        private QuickSearchFluentRibbonTabView? _quickSearchView;
        private bool _quickSearchLoaded;

        private void EnsureQuickSearchLoaded()
        {
            if (_quickSearchLoaded)
                return;

            _quickSearchLoaded = true;

            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var vm =
                        ((CREOToolsFluentViewModel)DataContext)
                        .QuickSearchViewModelVM;

                    LoadRibbonTab(
                        TabQuickSearchApp,
                        ref _quickSearchView,
                        vm);
                }),
                DispatcherPriority.Background);
        }

        // Ecn Eco Follow Up Tab
        private EcnEcoFollowUpFluentTabView? _ecnEcoFollowUpView;
        private bool _ecnEcoFollowUpLoaded;

        private void EnsureEcnEcoFollowUpLoaded()
        {
            if (_ecnEcoFollowUpLoaded)
                return;
            _ecnEcoFollowUpLoaded = true;
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var vm =
                        ((CREOToolsFluentViewModel)DataContext)
                        .EcnEcoFollowUpViewModelVM;
                    LoadRibbonTab(
                        TabEcnEcoFollowUpApp,
                        ref _ecnEcoFollowUpView,
                        vm);
                }),
                DispatcherPriority.Background);
        }

        // Purchase Order Tab
        private PurchaseOrderFollowUpTabMainView? _purchaseOrderView;
        private bool _purchaseOrderViewLoaded;

        private void EnsurePurchaseOrderViewLoaded()
        {
            if (_purchaseOrderViewLoaded)
                return;
            _purchaseOrderViewLoaded = true;
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var vm =
                        ((CREOToolsFluentViewModel)DataContext)
                        .PurchaseOrderFollowUpViewModelVM;
                    LoadRibbonTab(
                        TabPurchaseOrderFollowUpApp,
                        ref _purchaseOrderView,
                        vm);
                }),
                DispatcherPriority.Background);
        }

        // Profile tab
        private ProfileFluentTabMainView? _profileView;
        private bool _profileViewLoaded;

        private void EnsureProfileViewLoaded()
        {
            if (_profileViewLoaded)
                return;
            _profileViewLoaded = true;
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var vm =
                        ((CREOToolsFluentViewModel)DataContext)
                        .ProfileViewModelVM;
                    LoadRibbonTab(
                        TabProfileApp,
                        ref _profileView,
                        vm);
                }),
                DispatcherPriority.Background);
        }

        // Convert To Pdf Tab
        private ConvertToPdfTabMainView? _convertToPdfView;
        private bool _convertToPdfViewLoaded;

        private void EnsureConvertToPdfViewLoaded()
        {
            if (_convertToPdfViewLoaded)
                return;
            _convertToPdfViewLoaded = true;
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var vm =
                        ((CREOToolsFluentViewModel)DataContext)
                        .ConvertToPdfViewModelVM;
                    LoadRibbonTab(
                        TabConvertToPdfApp,
                        ref _convertToPdfView,
                        vm);
                }),
                DispatcherPriority.Background);
        }

        // Sheared Tube Tab
        private ShearedTubeFluentTabMainView? _shearedTubeView;
        private bool _shearedTubeViewLoaded;

        private void EnsureShearedTubeViewLoaded()
        {
            if (_shearedTubeViewLoaded)
                return;
            _shearedTubeViewLoaded = true;
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var vm =
                        ((CREOToolsFluentViewModel)DataContext)
                        .ShearedTubeViewModelVM;
                    LoadRibbonTab(
                        TabShearedTubeApp,
                        ref _shearedTubeView,
                        vm);
                }),
                DispatcherPriority.Background);
        }

        // DXF Export Tab
        private DxfExportFluentTabMainView? _dxfExportView;
        private bool _dxfExportViewLoaded;

        private void EnsureDxfExportViewLoaded()
        {
            if (_dxfExportViewLoaded)
                return;
            _dxfExportViewLoaded = true;
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var vm =
                        ((CREOToolsFluentViewModel)DataContext)
                        .DxfExportViewModelVM;
                    LoadRibbonTab(
                        TabDxfExportApp,
                        ref _dxfExportView,
                        vm);
                }),
                DispatcherPriority.Background);
        }

        // JPG Export Tab
        private JpgExportFluentTabMainView? _jpgExportView;
        private bool _jpgExportViewLoaded;

        private void EnsureJpgExportViewLoaded()
        {
            if (_jpgExportViewLoaded)
                return;
            _jpgExportViewLoaded = true;
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var vm =
                        ((CREOToolsFluentViewModel)DataContext)
                        .JpgExportViewModelVM;
                    LoadRibbonTab(
                        TabJpgExportApp,
                        ref _jpgExportView,
                        vm);
                }),
                DispatcherPriority.Background);
        }

        // Download Visualization File Tab
        private DownloadVisualizationFileMainView? _downloadVisuFileView;
        private bool _downloadVisuFileLoaded;

        private void EnsureDownloadVisuFileLoaded()
        {
            if (_downloadVisuFileLoaded)
                return;
            _downloadVisuFileLoaded = true;
            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    var vm =
                        ((CREOToolsFluentViewModel)DataContext)
                        .DownloadVisualizationFileViewModelVM;
                    LoadRibbonTab(
                        TabDownloadVisualizationFileApp,
                        ref _downloadVisuFileView,
                        vm);
                }),
                DispatcherPriority.Background);
        }
    }
}
