using MCG.CREO_Tools.CutLengthApp.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.CutLengthApp.View
{
    public interface ICutLenghtDataContext
    {
        ObservableCollection<CutLengthType> ListCutLengthType { get; set; }
        ObservableCollection<CutLengthCutPart> CurrentListPartNumber { get; set; }
        CutLengthType SelectedCutLengthType { get; set; }
        CutLengthCutPart SelectedCutLengthPart { get; set; }

        double Quantity { get; set; }
        string ActiveModelFileName { get; set; }

        bool BulkSelected { get; set; }
        bool ThreeDSelected { get; set; }
        bool ThreeDIsEnable { get; set; }
        bool IsCreoEnable { get; set; }
        bool IsEditMode { get; set; }
        bool IsAdminToolsEnabled { get; set; }

    }
}
