using MCG.CREO_Tools.QuickSearch.ViewModel;
using System.Windows;

namespace MCG.CREO_Tools.QuickSearch.Interfaces
{
    public interface IQuickSearchWindchillService
    {
        void ShowQuickSearchUpdatePartView(QuickSearchPart selectedPartItem, bool isAlreadyCreated = false);
        MessageBoxResult ShowDialogQuickSearchUpdatePartView(QuickSearchPart selectedPartItem);
        void CloseQuickSearchUpdatePartView();

        void ShowQuickSearchWindowClassSubClassFromNumberView(List<string> listStdShown, bool isAlreadyCreated = false);
        MessageBoxResult ShowDialogQuickSearchWindowClassSubClassFromNumberView(List<string> listStdShown);
        void CloseQuickSearchWindowClassSubClassFromNumberView();

        void ShowQuickSearchWindowRefDocFromNumberView(bool isAlreadyCreated = false);
        MessageBoxResult ShowDialogQuickSearchWindowRefDocFromNumberView();
        void CloseQuickSearchWindowRefDocFromNumberView();

        Task<QuickSearchShortCutViewModel?> ShowDialogQuickSearchWindowClassSubClassFromNumberViewAsync(List<string> listStdShown);
        Task<QuickSearchShortCutViewModel?> ShowQuickSearchWindowClassSubClassFromNumberViewAsync(List<string> listStdShown, bool isAlreadyCreated = false);
    }
}
