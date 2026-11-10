using MCG.CommonLib.Services.Statics;
using Microsoft.Web.WebView2.Core;
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
        private bool _isInitializing;
        private CoreWebView2Environment? _environment;

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
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized || _isInitializing)
            {
                return;
            }

            _isInitializing = true;
            try
            {
                // Même dossier de données que l'onglet Accueil (QuickLaunch) : la session SSO SharePoint est partagée.
                string userDataFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KioskBrowser");
                _environment ??= await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await PdfWebView.EnsureCoreWebView2Async(_environment);

                PdfWebView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
                _isInitialized = true;
                Navigate();
            }
            catch (Exception ex) when (ex is WebView2RuntimeNotFoundException
                                        or InvalidOperationException
                                        or System.Runtime.InteropServices.COMException
                                        or ArgumentException)
            {
                // Détails techniques conservés dans la trace ; l'utilisateur reçoit un message simple.
                TraceLog.AddTraceLog($"PdfViewerView : initialisation WebView2 impossible : {ex}");
                ShowError();
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void Navigate()
        {
            if (Source == null || !Source.IsAbsoluteUri || string.IsNullOrWhiteSpace(Source.OriginalString))
            {
                TraceLog.AddTraceLog($"PdfViewerView : source PDF absente ou invalide : '{Source}'");
                ShowError();
                return;
            }

            ErrorPanel.Visibility = Visibility.Collapsed;
            PdfWebView.Visibility = Visibility.Visible;
            PdfWebView.CoreWebView2.Navigate(Source.AbsoluteUri);
        }

        private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess && e.HttpStatusCode < 400)
            {
                return;
            }

            // OperationCanceled : navigation remplacée par une autre, pas une erreur.
            if (e.WebErrorStatus == CoreWebView2WebErrorStatus.OperationCanceled)
            {
                return;
            }

            TraceLog.AddTraceLog($"PdfViewerView : échec de navigation. WebErrorStatus={e.WebErrorStatus}, HttpStatusCode={e.HttpStatusCode}, Source={Source}");
            ShowError();
        }

        private void ShowError()
        {
            PdfWebView.Visibility = Visibility.Collapsed;
            ErrorPanel.Visibility = Visibility.Visible;
        }

        private async void OnRetryClick(object sender, RoutedEventArgs e)
        {
            ErrorPanel.Visibility = Visibility.Collapsed;
            PdfWebView.Visibility = Visibility.Visible;

            if (_isInitialized)
            {
                Navigate();
            }
            else
            {
                await InitializeAsync();
            }
        }
    }
}
