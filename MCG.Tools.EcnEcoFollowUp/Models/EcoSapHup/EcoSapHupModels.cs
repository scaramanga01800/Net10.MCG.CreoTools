namespace MCG.Tools.EcnEcoFollowUp.Models.EcoSapHup
{
    /// <summary>
    /// ECO candidate selected from ECNECOFOLLOWUP. Carries explicitly the group membership and the stored
    /// closed date used for the rolling-year comparison (replaces the historical ListEco02ToUpdate).
    /// </summary>
    public sealed record EcoSapHupCandidate(
        int Id,
        string? EcnNumber,
        DateOnly? StoredEcoClosedOn,
        bool IsRollingYearCandidate,
        bool IsPendingSapUpdateCandidate);

    /// <summary>
    /// Result of the pure workflow-step calculation (historical SearchSapHupDmEcoViewStatusFromList, WF part).
    /// </summary>
    public sealed record EcoSapHupStepResult(
        bool HasTasks,
        string MainNextStep,
        DateTime? MainLastWiCreatedOn,
        string SecondaryNextStep,
        DateTime? SecondaryLastWiCreatedOn,
        string SecondaryPlants)
    {
        public static EcoSapHupStepResult NoTask { get; } = new(false, string.Empty, null, string.Empty, null, string.Empty);
    }

    /// <summary>
    /// Explicit SAP HUP information needed to update one ECO. No EF entity, no ViewModel.
    /// </summary>
    public sealed record EcoSapHupSnapshot
    {
        public required string Eco { get; init; }
        public required string MainPlant { get; init; }
        public required string MainPlantDesc { get; init; }
        public DateOnly? CreatedOn { get; init; }
        public DateOnly? EffectivityDate { get; init; }
        public required string Categ { get; init; }
        public required string Project { get; init; }
        public required string SubLine { get; init; }
        public required string Urge { get; init; }
        public byte? CurrentStatus { get; init; }
        public DateOnly? Stat1Date { get; init; }
        public DateOnly? Stat2Date { get; init; }
        public DateOnly? Stat3Date { get; init; }
        public DateOnly? LastChange { get; init; }
        public bool LastChangeDiffersFromStoredClosedOn { get; init; }
        public required EcoSapHupStepResult Steps { get; init; }
    }

    public enum EcoSapHupSyncPhase
    {
        MainStatusRetrieval,
        WorkflowTaskRetrieval,
        Persistence
    }

    public enum EcoSapHupSyncWarningKind
    {
        MissingEcoNumber,
        NotFoundInSapHup,
        EmptySapHupDescription
    }

    /// <summary>Item-level failure. The original exception is kept unchanged (stack trace, InnerException).</summary>
    public sealed record EcoSapHupSyncError(int EcoId, string? EcoNumber, EcoSapHupSyncPhase Phase, string Message, Exception Exception);

    /// <summary>Item-level non-fatal situation (not found / skipped). The database record is left untouched.</summary>
    public sealed record EcoSapHupSyncWarning(int EcoId, string? EcoNumber, EcoSapHupSyncWarningKind Kind, string Message);

    /// <summary>Detailed result of one SAP HUP ECO synchronization.</summary>
    public sealed class EcoSapHupSyncResult
    {
        private readonly List<EcoSapHupSyncError> _errors = new();
        private readonly List<EcoSapHupSyncWarning> _warnings = new();

        /// <summary>Distinct ECO records selected (union of both groups, deduplicated by Id).</summary>
        public int CandidatesSelected { get; internal set; }

        /// <summary>Candidates coming from the rolling-year group (status 02/03 closed during the last year).</summary>
        public int RollingYearCandidates { get; internal set; }

        /// <summary>Candidates coming from the pending SAP update group.</summary>
        public int PendingSapUpdateCandidates { get; internal set; }

        /// <summary>ECO for which a SAP HUP main status row was found.</summary>
        public int FoundInSapHup { get; internal set; }

        /// <summary>ECO without SAP HUP main status row (database left untouched).</summary>
        public int NotFoundInSapHup { get; internal set; }

        /// <summary>ECO skipped (missing ECO number or empty SAP HUP description, database left untouched).</summary>
        public int Skipped { get; internal set; }

        /// <summary>Rolling-year candidates whose SAP HUP last-change date differs from the stored closed date.</summary>
        public int LastChangeDifferences { get; internal set; }

        /// <summary>Rolling-year-only candidates without last-change difference: no update required (historical behavior).</summary>
        public int RollingYearCandidatesWithoutDifference { get; internal set; }

        /// <summary>ECO whose database row was modified and saved.</summary>
        public int Updated { get; internal set; }

        /// <summary>ECO mapped but whose database values were already identical.</summary>
        public int Unchanged { get; internal set; }

        public int Failures => _errors.Count;
        public bool HasFailures => _errors.Count > 0;
        public IReadOnlyList<EcoSapHupSyncError> Errors => _errors.AsReadOnly();
        public IReadOnlyList<EcoSapHupSyncWarning> Warnings => _warnings.AsReadOnly();

        internal void AddError(EcoSapHupSyncError error) => _errors.Add(error);
        internal void AddWarning(EcoSapHupSyncWarning warning) => _warnings.Add(warning);

        public override string ToString() =>
            $"candidats={CandidatesSelected} (annee glissante={RollingYearCandidates}, maj SAP en attente={PendingSapUpdateCandidates}), " +
            $"trouves={FoundInSapHup}, non trouves={NotFoundInSapHup}, ignores={Skipped}, ecarts last-change={LastChangeDifferences}, " +
            $"sans ecart={RollingYearCandidatesWithoutDifference}, mis a jour={Updated}, inchanges={Unchanged}, echecs={Failures}";
    }
}
