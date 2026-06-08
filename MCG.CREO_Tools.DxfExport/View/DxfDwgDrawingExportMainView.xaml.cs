using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.DxfExport.Configuration;
using MCG.CREO_Tools.DxfExport.Exceptions;
using MCG.CREO_Tools.DxfExport.ViewModel;

namespace MCG.CREO_Tools.DxfExport.View
{
    public partial class DxfDwgDrawingExportMainView : RibbonWindow
    {
        private DxfDwgDrawingExportViewModel CurrentDxfDwgDrawingExportViewModel;
       
        public DxfDwgDrawingExportMainView(DxfDwgDrawingExportViewModel currentViewModel)
        {
            try
            {
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{DxfExportConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();
                CurrentDxfDwgDrawingExportViewModel = currentViewModel;
                this.DataContext = currentViewModel;

                currentViewModel.CurrentDatacontext.CurrentFolder = McgWpfTools.GetStringResource("DXF_TbExportFolder");
                currentViewModel.CurrentDatacontext.CurrentFileName = McgWpfTools.GetStringResource("DXF_TbExportFile");
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
