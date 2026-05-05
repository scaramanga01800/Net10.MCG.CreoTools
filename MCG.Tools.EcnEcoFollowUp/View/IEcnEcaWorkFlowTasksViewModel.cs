using MCG.Tools.EcnEcoFollowUp.Models;
using System.Collections.ObjectModel;

namespace MCG.Tools.EcnEcoFollowUp.View
{
    public interface IEcnEcaWorkFlowTasksViewModel
    {
        EFU_EcnEcoToShowEndUser EcnEcoToShowEndUser { get; set; }
        ObservableCollection<EFU_EcnEcoWorkflowItem> ListEcnWfTask { get; set; }
        ObservableCollection<EFU_EcnEcoWorkflowItem> ListEcaWfTask { get; set; }
    }
}
