using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.ViewModel.BomComparison;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using System.IO;
using MCG.CommonLib.WpfComponent.Interfaces;

namespace MCG.CREO_Tools.MiscTools.View.BomComparison
{
    /// <summary>
    /// Logique d'interaction pour BomComparisonView.xaml
    /// </summary>
    public partial class BomComparisonView : RibbonWindow
    {
        private readonly ISharedAppContext _sharedAppContext;
        public BomComparisonView(BomComparisonViewModel currentViewModel,
                                 ISharedAppContext sharedAppContext)
        {
            try
            {
                TraceLog.AddTraceLog("Create BomComparisonView");
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                TraceLog.AddTraceLog($"CadDocRenameMainView: Local App Directory {MainAppFolder}");

                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);
                DataContext = currentViewModel;
                _sharedAppContext = sharedAppContext;

                InitializeComponent();

                McgWpfTools.UpdateMergeDictionaries(_sharedAppContext.CurrentLanguage?.Language?.CultureInfo?.Substring(0,2));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
