using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.DataBaseAccess.Models.SapHupDb;
using MCG.Tools.EcnEcoFollowUp.Models.EcoSapHup;

namespace MCG.Tools.EcnEcoFollowUp.Interfaces
{
    /// <summary>
    /// Headless SAP HUP ECO synchronization (historical SearchAllEcoFromLastYear + BatchModeUpdateEcoFromSapHup).
    /// Stateless: every call is independent.
    /// </summary>
    public interface IEcoSapHupSyncService
    {
        /// <exception cref="Exceptions.EcoSapHupSyncException">Global failure (candidate selection).</exception>
        /// <exception cref="OperationCanceledException">Cancellation requested.</exception>
        EcoSapHupSyncResult Synchronize(CancellationToken cancellationToken = default);
    }

    /// <summary>Pure calculator of the main and secondary SAP HUP workflow steps.</summary>
    public interface IEcoSapHupStepCalculator
    {
        EcoSapHupStepResult Calculate(IReadOnlyList<DmEcoViewWfEcoTask> tasks);
    }

    /// <summary>Focused persistence boundary for the ECO SAP HUP synchronization (one isolated DbContext per call).</summary>
    public interface IEcoSapHupSyncRepository
    {
        /// <summary>Returns the deduplicated candidates, ordered by ECN number.</summary>
        IReadOnlyList<EcoSapHupCandidate> GetCandidates(DateOnly closedAfter);

        /// <summary>Loads the ECO by Id in a fresh context, applies the mapping and saves. Returns true when values changed.</summary>
        bool UpdateEco(int id, Action<Ecnecofollowup> apply);
    }
}
