using MCG.CREO_Tools.QuickSearch.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.QuickSearch.View
{
    public interface IQuickSearchViewModel
    {
        QuickSearchDataContext CurrentQuickSearchDataContext { get; set; }

        ICommand CommandBtHelpMouseLeftButtonUpEvent { get; }
        ICommand CommandOpenModelInCreo { get; }
        ICommand CommandAddModelInAssembly { get; }
        ICommand CommandOpenPartInPdm { get; }
        ICommand CommandOpenRefDocInPdm { get; }
        ICommand CommandSearchColumnKeyWord { get; }
        ICommand CommandAddExtraComponent { get; }
        ICommand CommandUpdateExtraCompMenu { get; }
        ICommand CommandPartSelectionChanged { get; }
        ICommand CommandSearchRefDocFromNumber { get; }
        ICommand CommandSearchClassSubClassFromNumber { get; }
        ICommand CommandCopyPartNumber { get; }

        ICommand CommandShortCutClassSubClass { get; }
        ICommand CommandShortCutDelete { get; }
        ICommand CommandShortCutReOrderDown { get; }
        ICommand CommandShortCutReOrderUp { get; }
        ICommand CommandShortCutAdd { get; }
        ICommand CommandShortCutReset { get; }
        ICommand CommandEditPartNumber { get; }
        ICommand CommandAddNewPartNumber { get; }
    }
}
