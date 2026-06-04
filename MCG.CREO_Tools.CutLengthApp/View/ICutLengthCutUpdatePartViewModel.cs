using MCG.CREO_Tools.CutLengthApp.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.CutLengthApp.View
{
    public interface ICutLengthCutUpdatePartViewModel
    {
        CutLengthCutPart PartItem { get; set; }

        MessageBoxResult Return { get; set; }

        ICommand CommandCreateUpdatePart { get; }
    }
}
