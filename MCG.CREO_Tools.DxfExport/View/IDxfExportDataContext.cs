using MCG.CREO_Tools.DxfExport.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.DxfExport.View
{
    public interface IDxfExportDataContext
    {
        string CurrentFolder { get; set; }
        string CurrentFileName { get; set; }
        string StatusBarMessage { get; set; }
        ObservableCollection<DxfExportItem> ListItems { get; set; }
        bool IsCreoEnable { get; set; }
        DxfExportItem SelectedItem { get; set; }
        bool IsFlatSelected { get; set; }
    }
}
