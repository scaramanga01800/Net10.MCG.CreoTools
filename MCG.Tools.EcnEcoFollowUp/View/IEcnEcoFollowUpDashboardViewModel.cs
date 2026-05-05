using MCG.Tools.EcnEcoFollowUp.Models;
using System.Windows.Input;

namespace MCG.Tools.EcnEcoFollowUp.View
{
    public interface IEcnEcoFollowUpDashboardViewModel
    {
        EFU_DashboardItem DashboardItem { get; set; }
        bool IsTabCreated { get; set; }

        ICommand CommandAddOneEcn { get; }
        ICommand CommandAddEcnFromSearch { get; }
        ICommand CommandDeleteSelectedEcn { get; }
        ICommand CommandExportXls { get; }
        ICommand CommandHideDashboard { get; }
        ICommand CommandRefreshDashboard { get; }

        ICommand CommandMenuItemOpenEcn { get; }
        ICommand CommandMenuItemOpenEcnDocs { get; }
        ICommand CommandMenutItemSearchEcnWfTask { get; }
        ICommand CommandMenutItemSearchEcoWfTask { get; }
        ICommand CommandMenutItemRemoveEcnEco { get; }

        ICommand CommandCheckAllDashboard { get; }
        ICommand CommandUncheckAllDashboard { get; }
        ICommand CommandMenutItemAddEcnEcoToDashboard { get; }

        event EventHandler DashboardHideEvent;
        event EventHandler DashboardShowEvent;
    }
}
