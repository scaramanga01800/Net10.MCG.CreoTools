using System.Windows.Input;

namespace MCG.CREO_Tools.QuickSearch.View
{
    public interface IQuickSearchWindowRefDocFromNumberViewModel
    {
        string Number { get; set; }
        string RefDocNumber { get; set; }
        bool IsRefDocFound { get; set; }

        ICommand CommandOpenRefDoc { get; }
        ICommand CommandSearchRefDoc { get; }
        ICommand CommandClose { get; }
    }
}
