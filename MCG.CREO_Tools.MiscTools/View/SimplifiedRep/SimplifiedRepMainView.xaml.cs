using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.ViewModel.SimplifiedRep;
using System.IO;

namespace MCG.CREO_Tools.MiscTools.View.SimplifiedRep
{
    public partial class SimplifiedRepMainView : RibbonWindow
    {
        public SimplifiedRepViewModel CurrentDataContext { get; set; }

        public SimplifiedRepMainView(SimplifiedRepViewModel currentViewModel, ISharedAppContext sharedAppContext)
        {
            try
            {
                TraceLog.AddTraceLog("Create SimplifiedRepMainView");
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                TraceLog.AddTraceLog($"SimplifiedRepMainView: Local App Directory {MainAppFolder}");

                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);
                CurrentDataContext = currentViewModel;
                DataContext = CurrentDataContext;

                InitializeComponent();
                McgWpfTools.UpdateMergeDictionaries(sharedAppContext.CurrentLanguage?.Language?.CultureInfo?.Substring(0, 2));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
