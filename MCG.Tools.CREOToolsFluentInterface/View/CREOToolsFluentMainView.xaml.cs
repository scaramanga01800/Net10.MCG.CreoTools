using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.CutLengthApp.View;
using MCG.Tools.CREOToolsFluentInterface.Configuration;
using MCG.Tools.CREOToolsFluentInterface.ViewModel;
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
    }
}
