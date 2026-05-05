using MCG.Tools.EcnEcoFollowUp.ViewModel;
using System.Windows.Input;

namespace MCG.Tools.EcnEcoFollowUp.View
{
    public interface IEcnEcoFollowUpViewModel
    {
        EcnEcoFollowUpDataContext CurrentEcnEcoFollowUpDataContext { get; set; }

        ICommand CommandExportXls { get; }
        ICommand CommandMenuItemCreateDashboardEmpty { get; }
        ICommand CommandMenuItemCreateDashboardFromSearch { get; }
        ICommand CommandMenuItemSaveSearch { get; }
        ICommand CommandSavedOrRecentSearch { get; }
        ICommand CommandShowHelp { get; }
        ICommand CommandSearchEcnEco { get; }
        ICommand CommandMenuItemOpenEcn { get; }
        ICommand CommandMenuItemOpenEco { get; }
        ICommand CommandMenuItemOpenEcnDocs { get; }
        ICommand CommandMenutItemSearchEcnWfTask { get; }
        ICommand CommandMenutItemSearchEcoWfTask { get; }
        ICommand CommandMenuItemOpenEcoDashboard { get; }
        ICommand CommandRenameSearch { get; }
        ICommand CommandUpdateSearch { get; }
        ICommand CommandDeleteSearch { get; }
        ICommand CommandExportSearch { get; }
        ICommand CommandMenutItemAddEcnEcoToDashboard { get; }
        ICommand CommandMenuItemSearchDashboard { get; }

        ICommand CommandDashBoardShow { get; }
        ICommand CommandDashBoardHide { get; }
        ICommand CommandDashBoardExport { get; }
        ICommand CommandDashBoardRename { get; }
        ICommand CommandDashBoardDelete { get; }
        ICommand CommandDashBoardRemove { get; }

        ICommand CommandAdmToolDeleteEcnEco { get; }
        ICommand CommandAdmToolSeachDeletedEcn { get; }

        ICommand CommandCheckAllMain { get; }
        ICommand CommandUncheckAllMain { get; }
    }
}
