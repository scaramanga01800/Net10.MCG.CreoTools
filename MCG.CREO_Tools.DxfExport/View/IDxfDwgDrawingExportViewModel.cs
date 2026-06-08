using MCG.CREO_Tools.DxfExport.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.DxfExport.View
{
    public interface IDxfDwgDrawingExportViewModel
    {
        DxfDwgDrawingExportDatacontext CurrentDatacontext { get; set; }
        ICommand CommandDxfDwgDrawingExport { get; }
        ICommand CommandOpenFolder { get; }
        ICommand CommandPaste { get; }
        ICommand CommandResetList { get; }

    }
}
