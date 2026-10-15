using MCG.CommonLib.DataBaseAccess.Exceptions;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.EcnEcoBatch;

namespace MCG.Tools.EcnEcoFollowUp.Services.EcnEcoBatch
{
    /// <summary>
    /// Sequential ECN/ECO orchestration, order derived from data dependencies:
    /// PdmContext (writes PDMCONTEXT) -> EcnFollowUp (reads PDMCONTEXT, writes ECN_STATE / inserts ECNECOFOLLOWUP rows)
    /// -> EcoSapHup (selects candidates on ECN_STATE = 'Resolved' and ECO_STATUS of ECNECOFOLLOWUP).
    /// Item-level failures continue; a global failure stops the chain (linear dependency).
    /// No business rule, no credential, no database or external access here.
    /// </summary>
    public sealed class EcnEcoFollowUpBatchService : IEcnEcoFollowUpBatchService
    {
        private readonly IPdmContextSyncService _pdmContext;
        private readonly IEcnFollowUpSyncService _ecnFollowUp;
        private readonly IEcoSapHupSyncService _ecoSapHup;

        public EcnEcoFollowUpBatchService(
            IPdmContextSyncService pdmContext,
            IEcnFollowUpSyncService ecnFollowUp,
            IEcoSapHupSyncService ecoSapHup)
        {
            _pdmContext = pdmContext ?? throw new ArgumentNullException(nameof(pdmContext));
            _ecnFollowUp = ecnFollowUp ?? throw new ArgumentNullException(nameof(ecnFollowUp));
            _ecoSapHup = ecoSapHup ?? throw new ArgumentNullException(nameof(ecoSapHup));
        }

        public EcnEcoFollowUpBatchResult Run(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TraceLog.AddTraceLog("Start EcnEcoFollowUpBatch");
            var result = new EcnEcoFollowUpBatchResult();

            try
            {
                var pdm = Execute(EcnEcoBatchStep.PdmContext, result, () => _pdmContext.Synchronize(cancellationToken));
                result.PdmContext = pdm;
                result.PdmContextStatus = StatusOf(pdm.HasFailures);
                LogStep(EcnEcoBatchStep.PdmContext, result.PdmContextStatus);

                cancellationToken.ThrowIfCancellationRequested();
                var ecn = Execute(EcnEcoBatchStep.EcnFollowUp, result, () => _ecnFollowUp.Synchronize(cancellationToken));
                result.EcnFollowUp = ecn;
                result.EcnFollowUpStatus = StatusOf(ecn.HasFailures);
                LogStep(EcnEcoBatchStep.EcnFollowUp, result.EcnFollowUpStatus);

                cancellationToken.ThrowIfCancellationRequested();
                var eco = Execute(EcnEcoBatchStep.EcoSapHup, result, () => _ecoSapHup.Synchronize(cancellationToken));
                result.EcoSapHup = eco;
                result.EcoSapHupStatus = StatusOf(eco.HasFailures);
                LogStep(EcnEcoBatchStep.EcoSapHup, result.EcoSapHupStatus);

                cancellationToken.ThrowIfCancellationRequested();
                TraceLog.AddTraceLog($"End EcnEcoFollowUpBatch : {result}");
                return result;
            }
            catch (OperationCanceledException)
            {
                TraceLog.AddTraceLog("EcnEcoFollowUpBatch cancelled");
                throw;
            }
        }

        private static T Execute<T>(EcnEcoBatchStep step, EcnEcoFollowUpBatchResult result, Func<T> call)
        {
            TraceLog.AddTraceLog($"EcnEcoFollowUpBatch step {step} start");
            try
            {
                return call();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                MarkFailed(step, result);
                TraceLog.AddTraceLog($"EcnEcoFollowUpBatch step {step} global failure ({ex.GetType().Name}); following steps skipped");
                if (ex is WindchillBatchCredentialConfigurationException)
                    throw;
                throw new EcnEcoFollowUpBatchException(step, result, ex);
            }
        }

        private static void MarkFailed(EcnEcoBatchStep step, EcnEcoFollowUpBatchResult result)
        {
            result.FailedStep = step;
            switch (step)
            {
                case EcnEcoBatchStep.PdmContext:
                    result.PdmContextStatus = EcnEcoBatchStepStatus.Failed;
                    result.EcnFollowUpStatus = EcnEcoBatchStepStatus.SkippedDueToDependencyFailure;
                    result.EcoSapHupStatus = EcnEcoBatchStepStatus.SkippedDueToDependencyFailure;
                    break;
                case EcnEcoBatchStep.EcnFollowUp:
                    result.EcnFollowUpStatus = EcnEcoBatchStepStatus.Failed;
                    result.EcoSapHupStatus = EcnEcoBatchStepStatus.SkippedDueToDependencyFailure;
                    break;
                case EcnEcoBatchStep.EcoSapHup:
                    result.EcoSapHupStatus = EcnEcoBatchStepStatus.Failed;
                    break;
            }
        }

        private static EcnEcoBatchStepStatus StatusOf(bool hasFailures) =>
            hasFailures ? EcnEcoBatchStepStatus.CompletedWithFailures : EcnEcoBatchStepStatus.Completed;

        private static void LogStep(EcnEcoBatchStep step, EcnEcoBatchStepStatus status) =>
            TraceLog.AddTraceLog($"EcnEcoFollowUpBatch step {step} {status}");
    }
}
