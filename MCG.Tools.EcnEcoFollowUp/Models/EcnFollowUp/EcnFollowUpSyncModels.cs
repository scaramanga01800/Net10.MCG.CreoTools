namespace MCG.Tools.EcnEcoFollowUp.Models.EcnFollowUp
{
    /// <summary>Processing phase in which an item-level failure occurred.</summary>
    public enum EcnFollowUpSyncPhase
    {
        NewEcnChangeNoticeRetrieval,
        NewEcnPersistence,
        WorkflowTaskRetrieval,
        WorkflowCalculation,
        WorkflowChangeNoticeRetrieval,
        WorkflowPersistence
    }

    /// <summary>Item-level failure. Keeps the original exception; never contains credentials or EF entities.</summary>
    public sealed record EcnFollowUpSyncError(string? EcnNumber, EcnFollowUpSyncPhase Phase, string Message, Exception Exception);

    /// <summary>Content of the LASTPDMECNSEARCH tracking row (ECNECOFOLLOWUP_UPDATE).</summary>
    public sealed record LastPdmEcnSearchMarker(bool RowExists, string? Status);

    /// <summary>PDMCONTEXT values consumed by the ECN mapping (PRODUCT, PARTICIPANT_ID).</summary>
    public sealed record PdmContextLookup(string? Product, string? ParticipantId);

    /// <summary>Resulting-object counters of one Change Notice (historical NB_PART / NB_EPM_DOC / NB_WT_DOC / NB_DRW).</summary>
    public sealed record ResultingObjectCounts(int NbPart, int NbEpmDoc, int NbWtDoc, int NbDrw);

    /// <summary>Windchill metadata of one ECN, mapped from GetChangeNoticeWithAllResultingObject.</summary>
    public sealed record EcnChangeNoticeSnapshot
    {
        public required string EcnNumber { get; init; }
        public string? Name { get; init; }
        public string? State { get; init; }
        public string? CreatedBy { get; init; }
        public string? Description { get; init; }
        public string? PdmEcnId { get; init; }
        public string? PdmContext { get; init; }
        public DateOnly? CreatedOn { get; init; }
        public required ResultingObjectCounts Counts { get; init; }
    }

    /// <summary>Structured result of one ECN follow-up synchronization. A new instance per invocation.</summary>
    public sealed class EcnFollowUpSyncResult
    {
        private readonly List<EcnFollowUpSyncError> _errors = new();
        private readonly List<string> _warnings = new();

        // New ECN discovery
        public DateOnly DiscoveryStartDate { get; internal set; }
        public int EcnsReturnedByWindchill { get; internal set; }
        public int DuplicateCandidates { get; internal set; }
        public int InvalidCandidates { get; internal set; }
        public int EcnsAlreadyExisting { get; internal set; }
        public int EcnsInserted { get; internal set; }
        public int NewEcnsNotFoundInWindchill { get; internal set; }
        public int NewEcnFailures { get; internal set; }
        public bool LastPdmEcnSearchUpdated { get; internal set; }
        public string? LastPdmEcnSearchNewValue { get; internal set; }

        // Workflow update
        public int WorkflowDatabaseCandidates { get; internal set; }
        public int WorkflowDuplicateCandidates { get; internal set; }
        public int WorkflowInvalidCandidates { get; internal set; }
        public int WorkflowDeduplicatedEcns { get; internal set; }
        public int WorkflowCalculationsPerformed { get; internal set; }
        public int WorkflowEcnsWithoutWorkflowTasks { get; internal set; }
        public int WorkflowEcnsFoundInWindchill { get; internal set; }
        public int WorkflowEcnsNotFoundInWindchill { get; internal set; }
        /// <summary>ECN trouvees dans Windchill dont la ligne a reellement change (SaveChanges appele).</summary>
        public int WorkflowEcnsChanged { get; internal set; }
        /// <summary>ECN trouvees dans Windchill dont la ligne existe mais n'a pas change (aucune ecriture).</summary>
        public int WorkflowEcnsUnchanged { get; internal set; }
        public int WorkflowEcnsSetNoUpdateRequired { get; internal set; }
        public int WorkflowFailures { get; internal set; }

        public IReadOnlyList<EcnFollowUpSyncError> Errors => _errors;
        public IReadOnlyList<string> Warnings => _warnings;
        public bool HasFailures => _errors.Count > 0;

        internal void AddError(EcnFollowUpSyncError error) => _errors.Add(error);
        internal void AddWarning(string warning) => _warnings.Add(warning);

        public override string ToString() =>
            $"discovery[from={DiscoveryStartDate:yyyy-MM-dd}, returned={EcnsReturnedByWindchill}, duplicates={DuplicateCandidates}, existing={EcnsAlreadyExisting}, inserted={EcnsInserted}, notFound={NewEcnsNotFoundInWindchill}, failures={NewEcnFailures}, markerUpdated={LastPdmEcnSearchUpdated}] " +
            $"workflow[candidates={WorkflowDatabaseCandidates}, unique={WorkflowDeduplicatedEcns}, calculated={WorkflowCalculationsPerformed}, noTasks={WorkflowEcnsWithoutWorkflowTasks}, found={WorkflowEcnsFoundInWindchill}, notFound={WorkflowEcnsNotFoundInWindchill}, changed={WorkflowEcnsChanged}, unchanged={WorkflowEcnsUnchanged}, noUpdateRequired={WorkflowEcnsSetNoUpdateRequired}, failures={WorkflowFailures}]";
    }
}
