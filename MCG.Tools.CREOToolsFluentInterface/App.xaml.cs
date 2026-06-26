using MCG.CREO_Tools.MiscTools.Services;
using MCG.CREO_Tools.QuickLaunch.Services;
using MCG.CommonLib.CreoInteractionTools.Services;
using MCG.CommonLib.DataBaseAccess.Services;
using MCG.CommonLib.SapTools.Services;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WebtermLib.Services;
using MCG.CommonLib.WpfComponent.Services;
using MCG.CREO_Tools.CutLengthApp.Services;
using MCG.CREO_Tools.DxfExport.Services;
using MCG.CREO_Tools.JpgExport.Services;
using MCG.CREO_Tools.MassUpdateAttribute.Services;
using MCG.CREO_Tools.ProfileApp.Services;
using MCG.CREO_Tools.QuickSearch.Services;
using MCG.CREO_Tools.ShearedTube.Services;
using MCG.Tools.EcnDataCheck.Services;
using MCG.Tools.EcnEcoFollowUp.Services;
using MCG.Tools.NumberingTool.Services;
using MCG.Tools.PurchaseOrderFollowUp.Services;
using MCG.Tools.VisualizationLib.Services;
using MCG.WindchillRequestTool.Services;
using MCG.WindchillTools.ManageWTObject.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using MCG.Tools.CREOToolsFluentInterface.Configuration;
using MCG.Tools.CREOToolsFluentInterface.View;
using MCG.Tools.CREOToolsFluentInterface.Interfaces;
using MCG.Tools.CREOToolsFluentInterface.ViewModel;
using MCG.Tools.CREOToolsFluentInterface.Services;

namespace MCG.Tools.CREOToolsFluentInterface
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            TraceLog.InitTraceLog("CreoToolsLogFile.log", 5);
            TraceLog.AddTraceLog("******************************************************************");
            TraceLog.AddTraceLog("Start Main Application CREO Tools - Engineering Hub");
            TraceLog.AddTraceLog($"CREO Tools Version:{CREOToolsConstants.Version}");
            TraceLog.AddTraceLog("******************************************************************");

            SetupGlobalExceptionHandling();

            _host = Host.CreateDefaultBuilder()
             .ConfigureServices((context, services) =>
             {
                 services.AddSingleton<CREOToolsFluentMainView>();
                 services.AddSingleton<CREOToolsFluentViewModel>();
                 services.AddSingleton<ISharedAppContext, SharedAppContext>();

                 // Service for MCG Common Lib (Json, PDF, TIFF, HTML, Oracle Tools, etc.)
                 services.AddMCGCommonLibServices();

                 // For CREO
                 services.AddCreoIntegrationServices();

                 // For Quick Launch
                 services.AddQuickLaunchServices();

                 // Services pour l'accès aux données (bases de données)
                 services.AddMcgDataBaseAccessServices();

                 // For SAP Tools
                 services.AddMCGCommonLibSapToolsServices();

                 // For WebtermTools
                 services.AddMCGCommonLibWebtermServices();

                 // For McgCommonlib.WpfComponents
                 services.AddMCGCommonLibWpfComponentServices();

                 // For MCG Tools Cut Length
                 services.AddCutLengthServices();

                 // For MCG Tools Dxf Export
                 services.AddDxfExportServices();

                 // For MCG Tools Jpg Export
                 services.AddJpgExportServices();

                 // for Mass Update Attribute Tool
                 services.AddMassUpdateAttributeServices();

                 // Misc Tools Services
                 services.AddMiscToolsServices();

                 // For Profile Tools Services
                 services.AddProfileAppServices();

                 // For Quick Search Services
                 services.AddQuickSearchServices();

                 // For Sheared Tube Services
                 services.AddShearedTubeServices();

                 // Ecn Data Check Services
                 services.AddEcnDataCheckServices();

                 // EcnEcoFollowUp Services
                 services.AddEcnEcoFollowUpServices();

                 // Numbering Tool Services
                 services.AddNumberingToolServices();

                 // Purchase Order Follow Up Services
                 services.AddPurchaseOrderFollowUpServices();

                 // Visualization Lib Services
                 services.AddMCGToolsVisualizationLibServices();

                 // MCG Windchill Request Tool Services
                 services.AddMCGWindchillRequestToolServices();

                 // MCG Windchill Tools Manage WT Object Services
                 services.AddMCGWindchillToolsManageWTObjectServices();


                 // Register your services here
             })
             .Build();
        }

        private void SetupGlobalExceptionHandling()
        {
            string msgEx = string.Empty;
            this.DispatcherUnhandledException += (sender, args) =>
            {
                msgEx = $"An unexpected error occurred. Please contact support..\n\n{args.Exception.Message}\n\nFrom :{args.Exception.StackTrace}";
                //MessageBox.Show(msgEx, "Erreur Critique", MessageBoxButton.OK, MessageBoxImage.Error);
                TraceLog.Error(args.Exception, msgEx);
                CREOToolsException.SendMessageBox("App", args.Exception, "DispatcherUnhandledException");
                args.Handled = true;
            };
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                msgEx = $"An unexpected error occurred. Please contact support..\n\n{args.Exception.Message}\n\nFrom :{args.Exception.StackTrace}";
                //MessageBox.Show(msgEx, "Erreur Critique", MessageBoxButton.OK, MessageBoxImage.Error);
                TraceLog.Error(args.Exception, msgEx);
                CREOToolsException.SendMessageBox("App", args.Exception, "DispatcherUnhandledException");
                // Empêche le crash du système asynchrone
                args.SetObserved();
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                // Ici, on ne peut généralement plus sauver l'application (e.Handled n'existe pas), 
                // mais on a au moins le temps d'afficher un message avant la fermeture.
                Exception ex = e.ExceptionObject as Exception;
                msgEx = $"An unexpected fatal error occurred, the app . Please contact support..\n\n{ex?.Message}";
                //MessageBox.Show(msgEx, "Erreur Critique", MessageBoxButton.OK, MessageBoxImage.Error);
                TraceLog.Error(ex, msgEx);
                CREOToolsException.SendMessageBox("App", ex, "DispatcherUnhandledException");
            };
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            // On demande la MainWindow au système d'injection
            var mainWindow = _host.Services.GetRequiredService<CREOToolsFluentMainView>();

            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }
            TraceLog.Close();
            base.OnExit(e);
        }
    }
}
