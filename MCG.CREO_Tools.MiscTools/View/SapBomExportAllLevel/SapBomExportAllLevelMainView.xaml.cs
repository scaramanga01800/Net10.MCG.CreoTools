using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.ViewModel.SapBomExportAllLevel;
using System.IO;

namespace MCG.CREO_Tools.MiscTools.View.SapBomExportAllLevel
{
    public partial class SapBomExportAllLevelMainView : RibbonWindow
    {
        public SapBomExportAllLevelViewModel CurrentDataContext { get; set; }

        public SapBomExportAllLevelMainView(SapBomExportAllLevelViewModel currentViewModel)
        {
            try
            {
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                TraceLog.AddTraceLog($"CadDocRenameMainView: Local App Directory {MainAppFolder}");

                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);
                CurrentDataContext = currentViewModel;
                DataContext = currentViewModel;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
