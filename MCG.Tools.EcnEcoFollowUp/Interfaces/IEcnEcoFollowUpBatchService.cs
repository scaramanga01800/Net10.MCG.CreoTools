using MCG.Tools.EcnEcoFollowUp.Models.EcnEcoBatch;

namespace MCG.Tools.EcnEcoFollowUp.Interfaces
{
    /// <summary>
    /// Headless top-level ECN/ECO batch orchestrator. Coordinates only IPdmContextSyncService,
    /// IEcnFollowUpSyncService and IEcoSapHupSyncService, sequentially, each at most once.
    /// </summary>
    public interface IEcnEcoFollowUpBatchService
    {
        /// <exception cref="Exceptions.EcnEcoFollowUpBatchException">Global failure of one step; later steps were skipped.</exception>
        /// <exception cref="MCG.CommonLib.DataBaseAccess.Exceptions.WindchillBatchCredentialConfigurationException">Propagated unchanged.</exception>
        /// <exception cref="OperationCanceledException">Propagated unchanged.</exception>
        EcnEcoFollowUpBatchResult Run(CancellationToken cancellationToken = default);
    }
}
