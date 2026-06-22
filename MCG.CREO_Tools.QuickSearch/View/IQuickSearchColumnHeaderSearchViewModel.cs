using MCG.CREO_Tools.QuickSearch.ViewModel;
namespace MCG.CREO_Tools.QuickSearch.View
{
    public interface IQuickSearchColumnHeaderSearchViewModel
    {
        string AttributeName { get; set; }
        object DataContextCommand { get; set; }
        int MinWidth { get; set; }
        QuickSearchPartSubClassParam RefObject { get; set; }
    }
}
