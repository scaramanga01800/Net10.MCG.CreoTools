using MCG.CREO_Tools.DxfExport.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.DxfExport.View
{
    interface IDxfExportViewModel
    {
        DxfExportDataContext CurrentDxfExportDataContext { get; set; }

        ICommand CommandBtHelpMouseLeftButtonUpEvent { get; }
        ICommand CommandOpenFolder { get; }
        ICommand CommandOpenFile { get; }
        ICommand CommandExportDxf { get; }
        ICommand CommandOpenModelInCreo { get; }
    }
}
