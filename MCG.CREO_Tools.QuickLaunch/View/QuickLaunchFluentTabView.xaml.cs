using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.QuickLaunch.Configuration;
using MCG.CREO_Tools.QuickLaunch.Exceptions;
using MCG.CREO_Tools.QuickLaunch.ViewModel;
using System.Diagnostics;

namespace MCG.CREO_Tools.QuickLaunch.View
{
    public partial class QuickLaunchFluentTabView : RibbonTabItem
    {
        private string MainAppFolder { get; set; }
        private bool IsAlreadyInit { get; set; } = false;
        public QuickLaunchViewModel CurrentQuickLaunchViewModel { get; set; }

        public QuickLaunchFluentTabView()
        {
            var sw = Stopwatch.StartNew();

            TraceLog.AddTraceLog("Create QuickLaunchFluentTabView");

            MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
            if (MainAppFolder == null || MainAppFolder == "")
                MainAppFolder = CommonLibConstants.MainAppFolder;

            TraceLog.AddTraceLog($"Before QuickLaunchFluentTabView MergeDictionary : {sw.ElapsedMilliseconds} ms");

            McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{QuickLaunchConstants.MainDictionary}", UriKind.Absolute);

            TraceLog.AddTraceLog($"After QuickLaunchFluentTabView MergeDictionary : {sw.ElapsedMilliseconds} ms");

            InitializeComponent();

            TraceLog.AddTraceLog($"After QuickLaunchFluentTabView InitializeComponent : {sw.ElapsedMilliseconds} ms");

            DataContextChanged += Initialize;
        }

        //public QuickLaunchFluentTabView()
        //{
        //    try
        //    {
        //        TraceLog.AddTraceLog("Create QuickLaunchFluentTabView");
        //        MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
        //        if (MainAppFolder == null || MainAppFolder == "")
        //            MainAppFolder = CommonLibConstants.MainAppFolder;
        //        McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{QuickLaunchConstants.MainDictionary}", UriKind.Absolute);

        //        InitializeComponent();

        //        DataContextChanged += Initialize;
        //    }
        //    catch (Exception ex)
        //    {
        //        QuickLaunchException.SendMessageBox(this.GetType().Name, ex);

        //    }
        //}


        private void Initialize(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(QuickLaunchViewModel))
                {
                    CurrentQuickLaunchViewModel = DataContext as QuickLaunchViewModel;
                    Loaded += async (s, e) => await CurrentQuickLaunchViewModel.ConnectToCreoAsync();
                }
            }
            catch (Exception ex)
            {
                QuickLaunchException.SendMessageBox(this.GetType().Name, ex);
            }
        }


    }
}
