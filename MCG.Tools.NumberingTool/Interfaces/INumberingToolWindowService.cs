using MCG.Tools.NumberingTool.ViewModel;

namespace MCG.Tools.NumberingTool.Interfaces
{
    public interface INumberingToolWindowService
    {
        void CloseNumberingToolCreateSeveralFluentView();
        void CloseNumberingToolFluentMainView();
        void CloseNumberingToolUpdateCreateFluentView();
        void ShowDialogNumberingToolCreateSeveralFluentView(NumberingToolViewModel currentVm);
        void ShowDialogNumberingToolFluentMainView(bool pNoRangeAuthorized = false);
        void ShowDialogNumberingToolUpdateCreateFluentView(bool CurrentIsNewNumber, NumberingToolTemplate CurrentSelectedNumberingTemplate, List<string> CurrentSearchProductList, List<string> CurrentListFormat, NumberingToolItem AlreadyCreatedItem = null, bool CurrentIsDetailShown = true);
        void ShowNumberingToolCreateSeveralFluentView(NumberingToolViewModel currentVm);
        void ShowNumberingToolFluentMainView(bool pNoRangeAuthorized = false);
        void ShowNumberingToolUpdateCreateFluentView(bool CurrentIsNewNumber, NumberingToolTemplate CurrentSelectedNumberingTemplate, List<string> CurrentSearchProductList, List<string> CurrentListFormat, NumberingToolItem AlreadyCreatedItem = null, bool CurrentIsDetailShown = true);
    }
}