using System.Windows;
using System.Windows.Controls;

namespace MCG.Tools.CREOToolsFluentInterface.View
{
    /// <summary>
    /// Vue d'affichage d'un PDF via Microsoft Edge WebView2 (viewer PDF natif d'Edge).
    /// </summary>
    public partial class PdfViewerView : UserControl
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(Uri), typeof(PdfViewerView),
                new PropertyMetadata(null, OnSourceChanged));

        private bool _isInitialized;

        public PdfViewerView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        /// <summary>Adresse (fichier local ou URL) du PDF à afficher.</summary>
        public Uri? Source
        {
            get => (Uri?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PdfViewerView view && view._isInitialized)
            {
                view.Navigate();
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                return;
            }

            try
            {
                // Même dossier de données que l'onglet Accueil (QuickLaunch) : la session SSO SharePoint est partagée.
                string userDataFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KioskBrowser");
                var environment = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await PdfWebView.EnsureCoreWebView2Async(environment);
                _isInitialized = true;
                Navigate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Initialisation WebView2 impossible : {ex.Message}");
            }
        }

        private void Navigate()
        {
            if (Source != null)
            {
                PdfWebView.CoreWebView2.Navigate(Source.AbsoluteUri);
            }
        }
    }
}
