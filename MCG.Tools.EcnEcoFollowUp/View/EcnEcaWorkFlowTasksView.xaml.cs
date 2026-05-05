using Fluent;
using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using System.Windows;

namespace MCG.Tools.EcnEcoFollowUp.View
{
    /// <summary>
    /// Logique d'interaction pour EcnEcaWorkFlowTasksView.xaml
    /// </summary>
    public partial class EcnEcaWorkFlowTasksView : RibbonWindow
    {
        public EcnEcaWorkFlowTasksViewModel CurrentEcnEcaWorkFlowTasksViewModel { get; set; }
        public EcnEcaWorkFlowTasksView()
        {
            InitializeComponent();
        }

        public EcnEcaWorkFlowTasksView(EcnEcaWorkFlowTasksViewModel currentVm)
        {
            InitializeComponent();
            CurrentEcnEcaWorkFlowTasksViewModel = currentVm;
        }

        public void SetEcnEcaWorkFlowTasksViewProperties(EFU_EcnEcoToShowEndUser EcnEco, List<EFU_EcnEcoWorkflowItem> ListAllTask)
        {
            InitializeComponent();
            CurrentEcnEcaWorkFlowTasksViewModel.SetEcnEcaWorkFlowTasksViewModelProperties(EcnEco, ListAllTask);
            DataContext = CurrentEcnEcaWorkFlowTasksViewModel;
        }

        private void BtOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
