using MCG.CREO_Tools.QuickSearch.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.QuickSearch.View
{
    internal interface IQuickSearchWindowClassSubClassFromNumberViewModel
    {
        string Number { get; set; }
        QuickSearchShortCutViewModel ClassSubClass { get; set; }
        bool IsClassSubFound { get; set; }

        ICommand CommandOpenClassSubClass { get; }
        ICommand CommandSearchClassSubClass { get; }
        ICommand CommandClose { get; }
    }
}
