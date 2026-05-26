using MCG.CREO_Tools.MiscTools.ViewModel.QuickChange;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.QuickChange
{
    public interface IQuickChangeViewModel
    {
        QuickChangeDataContext CurrentDataContext { get; set; }

        ICommand CommandReadAsm { get; }
        ICommand CommandStartExcelExport { get; }
        ICommand CommandReplaceComponent { get; }
        ICommand CommandOpenHelp { get; }
    }
}
