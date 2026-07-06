using MCG.CREO_Tools.CadDocQualityCheck.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.CadDocQualityCheck.View
{
    internal interface IRunCreoMacroDataContext
    {
        bool IsEnabledActionButton { get; set; }
        bool IsEnabledCreo { get; set; }
        ObservableCollection<CadDocQualityCheckItem> ShownCadModels { get; set; }
        string Macro { get; set; }
    }
}
