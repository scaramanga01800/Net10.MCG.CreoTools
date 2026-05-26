using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.ViewModel.NumberCumulation;
using System.IO;

namespace MCG.CREO_Tools.MiscTools.View.NumberCumulation
{
    public partial class NumberCumulationMainView : RibbonWindow
    {
        public NumberCumulationViewModel CurrentDataContext { get; set; }

        public NumberCumulationMainView(NumberCumulationViewModel currentVm)
        {
            try
            {
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                TraceLog.AddTraceLog($"CraneSearchMainView: Local App Directory {MainAppFolder}");

                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);

                CurrentDataContext = currentVm;
                DataContext = currentVm;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
