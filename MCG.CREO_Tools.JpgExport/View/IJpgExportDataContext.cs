using MCG.CREO_Tools.JpgExport.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.JpgExport.View
{
    interface IJpgExportDataContext
    {
        string CurrentFolder { get; set; }
        string CurrentFileName { get; set; }
        string StatusBarMessage { get; set; }

        ObservableCollection<JpgExportComboBoxValue> ListDisplayStyle { get; set; }
        ObservableCollection<JpgExportComboBoxValue> ListView3D { get; set; }
        ObservableCollection<JpgExportComboBoxValue> ListResolution { get; set; }

        JpgExportComboBoxValue SelectedView3D { get; set; }
        JpgExportComboBoxValue SelectedDisplayStyle { get; set; }
        JpgExportComboBoxValue SelectedResolution { get; set; }

        ObservableCollection<JpgExportItem> ListItems { get; set; }
        bool IsCreoEnable { get; set; }
        JpgExportItem SelectedItem { get; set; }
        bool IsRemoveColor { get; set; }
    }
}
