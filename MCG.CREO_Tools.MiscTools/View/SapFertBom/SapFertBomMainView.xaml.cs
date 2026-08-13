using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.ViewModel.SapFertBom;
using System.IO;

namespace MCG.CREO_Tools.MiscTools.View.SapFertBom
{
    public partial class SapFertBomMainView : RibbonWindow
    {
        public SapFertBomViewModel CurrentDataContext { get; set; }

        public SapFertBomMainView(SapFertBomViewModel currentViewModel, ISharedAppContext sharedAppContext)
        {
            try
            {
                TraceLog.AddTraceLog("Create SapFertBomMainView");
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                TraceLog.AddTraceLog($"CadAutoColorMainView: Local App Directory {MainAppFolder}");

                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);
                CurrentDataContext = currentViewModel;
                DataContext = currentViewModel;

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
