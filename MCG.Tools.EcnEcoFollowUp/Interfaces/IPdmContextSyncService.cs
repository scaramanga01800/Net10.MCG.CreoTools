using MCG.Tools.EcnEcoFollowUp.Models.PdmContext;

namespace MCG.Tools.EcnEcoFollowUp.Interfaces
{
    /// <summary>
    /// Headless PDM context synchronization (historical BatchModeUpdatePdmContext).
    /// Stateless: every call is independent.
    /// </summary>
    public interface IPdmContextSyncService
    {
        /// <exception cref="MCG.CommonLib.DataBaseAccess.Exceptions.WindchillBatchCredentialConfigurationException">Batch credential configuration missing or invalid (propagated unchanged).</exception>
        /// <exception cref="Exceptions.PdmContextSyncException">Global failure (credentials, Windchill retrieval, planning, persistence).</exception>
        /// <exception cref="OperationCanceledException">Cancellation requested.</exception>
        PdmContextSyncResult Synchronize(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Persistence boundary for PDMCONTEXT / PDMALLCONTEXT. One isolated DbContext and one transaction per call.
    /// No Windchill access, no credentials, no business matching.
    /// </summary>
    public interface IPdmContextSyncRepository
    {
        PdmContextPersistenceResult Synchronize(
            IReadOnlyList<PdmContextMemberSnapshot> pdmContextCandidates,
            IReadOnlyList<PdmContextMemberSnapshot> pdmAllContextCandidates,
            CancellationToken cancellationToken);
    }
}
