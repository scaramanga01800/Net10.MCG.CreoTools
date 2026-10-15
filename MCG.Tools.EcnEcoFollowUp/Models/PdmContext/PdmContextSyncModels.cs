namespace MCG.Tools.EcnEcoFollowUp.Models.PdmContext
{
    /// <summary>
    /// Normalized final PDM context (DESIGNER context merged with its CHANGE ADMINISTRATOR III participant).
    /// Historical BatchModeUpdatePdmContext representation, without mutating the Windchill source objects.
    /// </summary>
    public sealed record PdmContextMemberSnapshot
    {
        public required string ContextName { get; init; }

        /// <summary>WindchillContextType.ToString() of the DESIGNER source (PRODUCT or LIBRARY).</summary>
        public required string ContextType { get; init; }

        public string? TeamRole { get; init; }
        public string? ParticipantType { get; init; }
        public string? ParticipantId { get; init; }
        public string? ParticipantName { get; init; }

        /// <summary>EFU_PdmContext.ExtractProduct(ContextName).</summary>
        public required string Product { get; init; }

        public bool HasParticipant => !string.IsNullOrWhiteSpace(ParticipantId);
    }

    public enum PdmContextSyncTable
    {
        None = 0,
        Pdmcontext = 1,
        Pdmallcontext = 2
    }

    public enum PdmContextSyncPhase
    {
        Normalization = 0,
        Matching = 1,
        ProductExtraction = 2,
        Persistence = 3
    }

    /// <summary>Item-level error. Never contains credential data.</summary>
    public sealed record PdmContextSyncError(
        string? ContextName,
        PdmContextSyncTable Table,
        PdmContextSyncPhase Phase,
        string Message,
        Exception Exception);

    /// <summary>Pure planning output: candidates for both tables plus normalization counters.</summary>
    public sealed class PdmContextSyncPlan
    {
        public required IReadOnlyList<PdmContextMemberSnapshot> AllContexts { get; init; }
        public required IReadOnlyList<PdmContextMemberSnapshot> PdmContexts { get; init; }
        public required IReadOnlyList<PdmContextSyncError> Errors { get; init; }

        public int ExcludedCoordinatorAssignments { get; init; }
        public int DuplicateDesignerAssignments { get; init; }
        public int DuplicateChangeAdministratorAssignments { get; init; }
        public int ContextsWithoutChangeAdministrator { get; init; }
        public int SkippedMissingParticipantId { get; init; }
    }

    /// <summary>Per-table persistence counters.</summary>
    public sealed class PdmContextTableSyncResult
    {
        public PdmContextTableSyncResult(PdmContextSyncTable table) => Table = table;

        public PdmContextSyncTable Table { get; }
        public int Candidates { get; internal set; }
        public int Inserted { get; internal set; }
        public int Updated { get; internal set; }
        public int Unchanged { get; internal set; }
        public int Failed { get; internal set; }
        public int DuplicateExistingRows { get; internal set; }
        public int ObsoleteRowsDetected { get; internal set; }

        public bool HasWrites => Inserted > 0 || Updated > 0;

        public override string ToString() =>
            $"{Table}: candidates={Candidates}, inserted={Inserted}, updated={Updated}, unchanged={Unchanged}, failed={Failed}, duplicateExisting={DuplicateExistingRows}, obsolete={ObsoleteRowsDetected}";
    }

    /// <summary>Result of the repository transaction.</summary>
    public sealed class PdmContextPersistenceResult
    {
        public required PdmContextTableSyncResult PdmContext { get; init; }
        public required PdmContextTableSyncResult PdmAllContext { get; init; }
        public required IReadOnlyList<PdmContextSyncError> Errors { get; init; }
        public bool SaveChangesCalled { get; init; }
    }

    /// <summary>Complete synchronization result.</summary>
    public sealed class PdmContextSyncResult
    {
        public int DesignerProductAssignments { get; internal set; }
        public int DesignerLibraryAssignments { get; internal set; }
        public int ChangeAdministratorProductAssignments { get; internal set; }
        public int ChangeAdministratorLibraryAssignments { get; internal set; }
        public int ExcludedCoordinatorAssignments { get; internal set; }
        public int DuplicateDesignerAssignments { get; internal set; }
        public int DuplicateChangeAdministratorAssignments { get; internal set; }
        public int NormalizedContexts { get; internal set; }
        public int ContextsWithoutChangeAdministrator { get; internal set; }
        public int SkippedMissingParticipantId { get; internal set; }

        public PdmContextTableSyncResult PdmContext { get; internal set; } = new(PdmContextSyncTable.Pdmcontext);
        public PdmContextTableSyncResult PdmAllContext { get; internal set; } = new(PdmContextSyncTable.Pdmallcontext);

        private readonly List<PdmContextSyncError> _errors = new();
        public IReadOnlyList<PdmContextSyncError> Errors => _errors;
        public bool HasFailures => _errors.Count > 0;

        internal void AddErrors(IEnumerable<PdmContextSyncError> errors) => _errors.AddRange(errors);

        public override string ToString() =>
            $"designer(product={DesignerProductAssignments}, library={DesignerLibraryAssignments}), " +
            $"caiii(product={ChangeAdministratorProductAssignments}, library={ChangeAdministratorLibraryAssignments}), " +
            $"excludedCoordinators={ExcludedCoordinatorAssignments}, duplicateDesigner={DuplicateDesignerAssignments}, " +
            $"duplicateCaiii={DuplicateChangeAdministratorAssignments}, normalized={NormalizedContexts}, " +
            $"withoutCaiii={ContextsWithoutChangeAdministrator}, skippedNoParticipant={SkippedMissingParticipantId}, " +
            $"{PdmContext}, {PdmAllContext}, errors={_errors.Count}";
    }
}
