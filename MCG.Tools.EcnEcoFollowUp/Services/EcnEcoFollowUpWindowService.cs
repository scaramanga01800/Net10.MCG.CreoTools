using MCG.CommonLib.WpfComponent.View;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.View;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MCG.Tools.EcnEcoFollowUp.Services
{
    public class EcnEcoFollowUpWindowService : IEcnEcoFollowUpWindowService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _EcoWorkFlowTasksView;
        private Window _EcnEcaWorkFlowTasksView;
        private Window _EcnEcoFollowUpDashboardSearchWindow;

        public EcnEcoFollowUpWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public EcnEcoFollowUpDashboardView GetEcnEcoFollowUpDashboardView(EFU_DashboardItem dashboardItem)
        {
            var view = _serviceProvider.GetRequiredService<EcnEcoFollowUpDashboardView>();
            view.SetEcnEcoFollowUpDashboardViewProperties(dashboardItem);
            return view;
        }

        public EcnEcoFollowUpDashboardViewModel GetEcnEcoFollowUpDashboardViewModel(EFU_DashboardItem dashboardItem, EcnEcoFollowUpViewModel parentApp)
        {
            var viewModel = _serviceProvider.GetRequiredService<EcnEcoFollowUpDashboardViewModel>();
            viewModel.SetEcnEcoFollowUpDashboardViewModelProperties(dashboardItem, parentApp);
            return viewModel;
        }

        public void ShowEcoWorkFlowTasksView(EFU_EcnEcoToShowEndUser currentEcn, List<EFU_SapHupOracle_DmEcoTasks> listAllTask)
        {
            _EcoWorkFlowTasksView = _serviceProvider.GetRequiredService<EcoWorkFlowTasksView>();
            ((EcoWorkFlowTasksView)_EcoWorkFlowTasksView).SetEcoWorkFlowTasksViewProperties(currentEcn, listAllTask);
            _EcoWorkFlowTasksView.Show();
        }
        public void ShowEcnEcaWorkFlowTasksView(EFU_EcnEcoToShowEndUser currentEcn, List<EFU_EcnEcoWorkflowItem> listAllTask)
        {
            _EcnEcaWorkFlowTasksView = _serviceProvider.GetRequiredService<EcnEcaWorkFlowTasksView>();
            ((EcnEcaWorkFlowTasksView) _EcnEcaWorkFlowTasksView).SetEcnEcaWorkFlowTasksViewProperties(currentEcn, listAllTask);
            _EcnEcaWorkFlowTasksView.Show();
        }
        public void ShowEcnEcoFollowUpDashboardSearchWindow()
        {
            _EcnEcoFollowUpDashboardSearchWindow = _serviceProvider.GetRequiredService<EcnEcoFollowUpDashboardSearchWindow>();
            _EcnEcoFollowUpDashboardSearchWindow.Show();
        }

        public void ShowDialogEcoWorkFlowTasksView(EFU_EcnEcoToShowEndUser currentEcn, List<EFU_SapHupOracle_DmEcoTasks> listAllTask)
        {
            _EcoWorkFlowTasksView = _serviceProvider.GetRequiredService<EcoWorkFlowTasksView>();
            ((EcoWorkFlowTasksView)_EcoWorkFlowTasksView).SetEcoWorkFlowTasksViewProperties(currentEcn, listAllTask);
            _EcoWorkFlowTasksView.ShowDialog();
        }
        public void ShowDialogEcnEcaWorkFlowTasksView(EFU_EcnEcoToShowEndUser currentEcn, List<EFU_EcnEcoWorkflowItem> listAllTask)
        {
            _EcnEcaWorkFlowTasksView = _serviceProvider.GetRequiredService<EcnEcaWorkFlowTasksView>();
            ((EcnEcaWorkFlowTasksView) _EcnEcaWorkFlowTasksView).SetEcnEcaWorkFlowTasksViewProperties(currentEcn, listAllTask);
            _EcnEcaWorkFlowTasksView.ShowDialog();
        }
        public (MessageBoxResult DialogValue, List<EFU_DashboardItem> SelectedDashboards) ShowDialogEcnEcoFollowUpDashboardSearchWindow()
        {
            _EcnEcoFollowUpDashboardSearchWindow = _serviceProvider.GetRequiredService<EcnEcoFollowUpDashboardSearchWindow>();
            MessageBoxResult dialogReturn =
                _EcnEcoFollowUpDashboardSearchWindow.ShowDialog() == true
                    ? MessageBoxResult.OK
                    : MessageBoxResult.Cancel;
            var values = ((EcnEcoFollowUpDashboardSearchWindow)_EcnEcoFollowUpDashboardSearchWindow).CurrentEcnEcoFollowUpDashboardSearchWindowViewModel.ListSelectedDashboard;
            return (dialogReturn, values);
        }

        public void CloseEcoWorkFlowTasksView()
        {
            _EcoWorkFlowTasksView.Close();
            _EcoWorkFlowTasksView = null;
        }
        public void CloseEcnEcaWorkFlowTasksView()
        {
            _EcnEcaWorkFlowTasksView.Close();
            _EcnEcaWorkFlowTasksView = null;
        }
        public void CloseEcnEcoFollowUpDashboardSearchWindow()
        {
            _EcnEcoFollowUpDashboardSearchWindow.Close();
            _EcnEcoFollowUpDashboardSearchWindow = null;
        }
    }
}
