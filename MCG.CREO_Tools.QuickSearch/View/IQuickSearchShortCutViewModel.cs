using MCG.CREO_Tools.QuickSearch.ViewModel;

namespace MCG.CREO_Tools.QuickSearch.View
{
    interface IQuickSearchShortCutViewModel
    {
        string Class { get; set; }
        string SubClass { get; set; }
        QuickSearchViewModel MainApp { get; set; }

    }
}
