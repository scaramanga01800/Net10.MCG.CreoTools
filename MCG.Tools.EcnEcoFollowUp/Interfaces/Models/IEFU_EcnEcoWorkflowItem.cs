using MCG.Tools.EcnEcoFollowUp.Models;

namespace MCG.Tools.EcnEcoFollowUp.Interfaces.Models
{
    public interface IEFU_EcnEcoWorkflowItem
    {
        EFU_Status Status { get; set; }

        string WfTaskName { get; set; }
        string WfTaskOwner { get; set; }
        string Vote { get; set; }

        DateTime? WfTaskCreatedOn { get; set; }
        DateTime? WfTaskCompletedOn { get; set; }

        string EcaNumber { get; set; }
        string EcaSate { get; set; }

    }
}
