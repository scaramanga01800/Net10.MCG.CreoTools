namespace MCG.Tools.EcnEcoFollowUp.Models.Workflow
{
    /// <summary>
    /// One workflow task (ECN or ECA) as consumed by the historical ExtractPdmWfTaskDate.
    /// Only the fields read by the historical algorithm are kept
    /// (EFU_EcnEcoWorkflowItem: WfTaskName, Vote, WfTaskCompletedOn, EcaNumber, EcaSate, EcnState).
    /// Vote and WfTaskCompletedOn are expected to be already derived by EFU_EcnEcoWorkflowItem.GetEFU_EcnEcoWorkflowItem.
    /// </summary>
    public sealed record EcnWorkflowTaskItem
    {
        public string? WfTaskName { get; init; }
        public string? Vote { get; init; }
        public DateTime? WfTaskCompletedOn { get; init; }
        public string? EcaNumber { get; init; }
        public string? EcaState { get; init; }
        public string? EcnState { get; init; }
    }

    /// <summary>
    /// Result of the pure workflow-date calculation. Dates keep their full DateTime precision;
    /// DateOnly conversion is deferred to the persistence boundary.
    /// </summary>
    public sealed record EcnWorkflowDateResult
    {
        /// <summary>False when the task list is empty (historical EFU_Status.ECN_NOT_IN_PDM).</summary>
        public bool IsEcnFoundInPdm { get; init; }

        /// <summary>True when an in-progress "Submit Change Notice" task forced every date to null.</summary>
        public bool IsSubmitChangeNoticeInProgress { get; init; }

        public DateTime? DesignerStartAppDate { get; init; }
        public DateTime? FirstApprovalDate { get; init; }
        public DateTime? QualCheckApprovalDate { get; init; }
        public DateTime? CaiiiApprovalDate { get; init; }
    }
}
