using MCG.Tools.EcnEcoFollowUp.Models;
using System.Collections.ObjectModel;

namespace MCG.Tools.EcnEcoFollowUp.View
{
    public interface IEcoWorkFlowTasksViewModel
    {
        EFU_EcnEcoToShowEndUser EcnEcoToShowEndUser { get; set; }

        ObservableCollection<EFU_SapHupOracle_DmEcoTasks> EcoWfTaskListMainPlant { get; set; }
        ObservableCollection<EFU_SapHupOracle_DmEcoTasks> EcoWfTaskListOtherPlants { get; set; }
    }
}
