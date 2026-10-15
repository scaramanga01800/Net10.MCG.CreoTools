using MCG.Tools.EcnEcoFollowUp.Models.EcnFollowUp;

namespace MCG.Tools.EcnEcoFollowUp.Interfaces
{
    /// <summary>
    /// Headless reconstruction of BatchModeAddAllNewEcn followed by BatchModeUpdateEcnWfDate.
    /// No UI, no interactive credentials, no shared state between invocations.
    /// </summary>
    public interface IEcnFollowUpSyncService
    {
        /// <exception cref="Exceptions.EcnFollowUpSyncException">Global failure (tracking state, discovery, persistence infrastructure).</exception>
        /// <exception cref="OperationCanceledException">Cancellation; LASTPDMECNSEARCH is never advanced.</exception>
        EcnFollowUpSyncResult Synchronize(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Persistence boundary of the ECN follow-up synchronization. Every method uses its own short
    /// DI scope (fresh CreoToolsDbContext) so that a failed SaveChanges never poisons later items.
    /// No Windchill access, no credentials, no workflow-date calculation.
    /// </summary>
    public interface IEcnFollowUpSyncRepository
    {
        LastPdmEcnSearchMarker ReadLastPdmEcnSearch(CancellationToken cancellationToken);

        /// <summary>Writes STATUS of the LASTPDMECNSEARCH row. Throws when the row does not exist.</summary>
        void WriteLastPdmEcnSearch(string status, CancellationToken cancellationToken);

        /// <summary>ECN numbers where PDM_UPDATE_STATUS IS NULL OR PDM_UPDATE_STATUS &lt;&gt; 'NO UPDATE REQUIRED', ordered by ID.</summary>
        IReadOnlyList<string?> GetWorkflowCandidateEcnNumbers(CancellationToken cancellationToken);

        bool EcnExists(string ecnNumber, CancellationToken cancellationToken);

        /// <summary>First PDMCONTEXT row (by ID) whose PDM_CONTEXT equals the context name; null when absent.</summary>
        PdmContextLookup? FindPdmContext(string? contextName, CancellationToken cancellationToken);

        /// <summary>Current MAX(ID) of ECNECOFOLLOWUP, 0 when empty.</summary>
        int GetMaxEcnFollowUpId(CancellationToken cancellationToken);

        void InsertEcn(MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb.Ecnecofollowup row, CancellationToken cancellationToken);

        /// <summary>Applies the mutation to the first row (by ID) with this ECN number and saves. False when no row exists.</summary>
        bool UpdateExistingEcn(string ecnNumber, Action<MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb.Ecnecofollowup> apply, CancellationToken cancellationToken);
    }
}
