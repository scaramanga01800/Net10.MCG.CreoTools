using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColr;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.CadAutoColor
{
    public interface ICadAutoColorViewModel
    {
        CadAutoColorDataContext CurrentDataContext { get; set; }
        ICommand CommandReadAsm { get; }
        ICommand CommandUpdateColor { get; }
        ICommand CommandStartExcelExport { get; }
        ICommand CommandOpenCadDoc { get; }
        ICommand CommandOpenHelp { get; }
        ICommand CommandUpdateColorPalette { get; }
        ICommand CommandCheckUncheckAll { get; }
        ICommand CommandCheckUncheckAllName { get; }
        ICommand CommandCheckUncheckAllPart { get; }
        ICommand CommandMultiAssignColor { get; }
        ICommand CommandRemoveColor { get; }
    }
}
