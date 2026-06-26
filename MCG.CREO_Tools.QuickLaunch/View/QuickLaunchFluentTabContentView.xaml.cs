using MCG.CREO_Tools.QuickLaunch.Exceptions;
using MCG.CREO_Tools.QuickLaunch.ViewModel;
using Microsoft.Web.WebView2.Core;
using System.Windows;
using System.Windows.Controls;

namespace MCG.CREO_Tools.QuickLaunch.View
{
    /// <summary>
    /// Logique d'interaction pour QuickLaunchFluentTabContentView.xaml
    /// </summary>
    public partial class QuickLaunchFluentTabContentView : UserControl
    {
        private bool IsAppAlreadyInit = false;
        private readonly string _cacheFolderPath;
        private QuickLaunchViewModel CurrentQuickLaunchViewModel { get; set; }

        public QuickLaunchFluentTabContentView()
        {
            _cacheFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\KioskBrowser";
            DataContextChanged += QuickLaunchFluentTabContentView_DataContextChanged;

            InitializeComponent();
        }

        private void QuickLaunchFluentTabContentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAppAlreadyInit && DataContext != null && DataContext.GetType() == typeof(QuickLaunchViewModel))
                {
                    CurrentQuickLaunchViewModel =DataContext as QuickLaunchViewModel;
                    CurrentQuickLaunchViewModel.HtmlPageChangedEvent += UpdateUrl;
                    UpdateUrl();
                }
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);

            }
        }

        private void UpdateUrl(object sender = null, EventArgs e = null)
        {
            try
            {
                kioskBrowser.Source = new Uri(CurrentQuickLaunchViewModel.HtmlPage);
            }
            catch (Exception ex)
            {
                throw new QuickLaunchException(this.GetType().Name, ex);
            }
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Use to define the folder for WebView2 browser logs in the user local folder instead of the executable folder.
            try
            {
                var webView2Environment = await CoreWebView2Environment.CreateAsync(null, _cacheFolderPath);
                await kioskBrowser.EnsureCoreWebView2Async(webView2Environment);
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
