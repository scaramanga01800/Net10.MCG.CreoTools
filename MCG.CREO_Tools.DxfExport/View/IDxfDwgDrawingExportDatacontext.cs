using MCG.CREO_Tools.DxfExport.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.DxfExport.View
{
    public interface IDxfDwgDrawingExportDatacontext
    {
        string CurrentFolder { get; set; }
        string CurrentFileName { get; set; }
        string DrawingTemplate { get; set; }
        ObservableCollection<DxfExportItem> ListItems { get; set; }
        DxfExportItem SelectedItem { get; set; }
        string StatusBarMessage { get; set; }

        bool IsCreoEnable { get; set; }
        bool IsDxfSelected { get; set; }
        bool IsDwgSelected { get; set; }
    }
}
