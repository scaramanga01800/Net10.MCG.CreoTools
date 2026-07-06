using MCG.CREO_Tools.CadDocQualityCheck.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.CadDocQualityCheck.View
{
    internal interface IRunCreoMacroViewModel
    {
        RunCreoMacroDataContext CurrentDataContext { get; set; }

        ICommand CommandPaste { get; }
        ICommand CommandMenuPaste { get; }
        ICommand CommandStart { get; }
        ICommand CommandDeleteItem { get; }
    }
}
