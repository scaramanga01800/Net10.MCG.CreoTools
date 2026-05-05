using Fluent;
using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using System.Windows;

namespace MCG.Tools.EcnEcoFollowUp.View
{
    public partial class EcoWorkFlowTasksView : RibbonWindow
    {
        public EcoWorkFlowTasksViewModel CurrentEcoWorkFlowTasksViewModel { get; set; }


        public EcoWorkFlowTasksView(EcoWorkFlowTasksViewModel currentVm)
        {
            InitializeComponent();
            CurrentEcoWorkFlowTasksViewModel = currentVm;
        }

        public void SetEcoWorkFlowTasksViewProperties(EFU_EcnEcoToShowEndUser EcnEco, List<EFU_SapHupOracle_DmEcoTasks> ListAllTask)
        {
            CurrentEcoWorkFlowTasksViewModel.SetEcoWorkFlowTasksViewModelProperties(EcnEco, ListAllTask);
            DataContext = CurrentEcoWorkFlowTasksViewModel;
        }

        private void BtOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
