using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.View;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using System.Windows;

namespace MCG.Tools.EcnEcoFollowUp.Interfaces
{
    public interface IEcnEcoFollowUpWindowService
    {
        void CloseEcnEcaWorkFlowTasksView();
        void CloseEcnEcoFollowUpDashboardSearchWindow();
        void CloseEcoWorkFlowTasksView();
        EcnEcoFollowUpDashboardView GetEcnEcoFollowUpDashboardView(EFU_DashboardItem dashboardItem);
        EcnEcoFollowUpDashboardViewModel GetEcnEcoFollowUpDashboardViewModel(EFU_DashboardItem dashboardItem, EcnEcoFollowUpViewModel parentApp);
        void ShowDialogEcnEcaWorkFlowTasksView(EFU_EcnEcoToShowEndUser currentEcn, List<EFU_EcnEcoWorkflowItem> listAllTask);
        (MessageBoxResult DialogValue, List<EFU_DashboardItem> SelectedDashboards) ShowDialogEcnEcoFollowUpDashboardSearchWindow();
        void ShowDialogEcoWorkFlowTasksView(EFU_EcnEcoToShowEndUser currentEcn, List<EFU_SapHupOracle_DmEcoTasks> listAllTask);
        void ShowEcnEcaWorkFlowTasksView(EFU_EcnEcoToShowEndUser currentEcn, List<EFU_EcnEcoWorkflowItem> listAllTask);
        void ShowEcnEcoFollowUpDashboardSearchWindow();
        void ShowEcoWorkFlowTasksView(EFU_EcnEcoToShowEndUser currentEcn, List<EFU_SapHupOracle_DmEcoTasks> listAllTask);
    }
}