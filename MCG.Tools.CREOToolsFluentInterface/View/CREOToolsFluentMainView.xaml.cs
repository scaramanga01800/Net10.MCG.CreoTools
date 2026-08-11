using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.CREOToolsFluentInterface.Configuration;
using MCG.Tools.CREOToolsFluentInterface.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MCG.Tools.CREOToolsFluentInterface.View
{
    public partial class CREOToolsFluentMainView : RibbonWindow
    {
        private CREOToolsFluentViewModel CurrentDataContext { get; set; }
        private string MainAppFolder { get; set; }

        public CREOToolsFluentMainView(CREOToolsFluentViewModel currentViewModel)
        {
            try
            {
                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{CREOToolsConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();
                CurrentDataContext = currentViewModel;
                DataContext = CurrentDataContext;
                Loaded += async (s, e) => await currentViewModel.InitializeAsync();

                CurrentDataContext.CurrentDataContext.ColorInterfaceChangeEvent += UpdateColorInterface;
                UpdateColorInterface(null, null);

                CurrentDataContext.CurrentDataContext.FontInterfaceChangeEvent += CurrentCREOToolsDataContext_FontInterfaceChangeEvent;
                CurrentCREOToolsDataContext_FontInterfaceChangeEvent(null, null);

                McgWpfTools.UpdateMergeDictionaries();

            }
            catch (Exception ex)
            {
                CREOToolsException.SendMessageBox(this.GetType().Name, ex);
            }
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
    }
}
