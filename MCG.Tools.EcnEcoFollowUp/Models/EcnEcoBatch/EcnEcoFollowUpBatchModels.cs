using MCG.Tools.EcnEcoFollowUp.Models.EcnFollowUp;
using MCG.Tools.EcnEcoFollowUp.Models.EcoSapHup;
using MCG.Tools.EcnEcoFollowUp.Models.PdmContext;

namespace MCG.Tools.EcnEcoFollowUp.Models.EcnEcoBatch
{
    /// <summary>Orchestration steps in their verified execution order.</summary>
    public enum EcnEcoBatchStep
    {
        PdmContext = 1,
        EcnFollowUp = 2,
        EcoSapHup = 3
    }

    public enum EcnEcoBatchStepStatus
    {
        NotStarted,
        Completed,
        CompletedWithFailures,
        Failed,
        SkippedDueToDependencyFailure
    }

    /// <summary>Aggregate result. Child results are kept typed; no cross-domain counter is summed.</summary>
    public sealed class EcnEcoFollowUpBatchResult
    {
        public EcnEcoBatchStepStatus PdmContextStatus { get; internal set; } = EcnEcoBatchStepStatus.NotStarted;
        public PdmContextSyncResult? PdmContext { get; internal set; }

        public EcnEcoBatchStepStatus EcnFollowUpStatus { get; internal set; } = EcnEcoBatchStepStatus.NotStarted;
        public EcnFollowUpSyncResult? EcnFollowUp { get; internal set; }

        public EcnEcoBatchStepStatus EcoSapHupStatus { get; internal set; } = EcnEcoBatchStepStatus.NotStarted;
        public EcoSapHupSyncResult? EcoSapHup { get; internal set; }

        /// <summary>Step that threw a global exception, if any.</summary>
        public EcnEcoBatchStep? FailedStep { get; internal set; }

        public int CompletedStepCount =>
            Statuses.Count(s => s is EcnEcoBatchStepStatus.Completed or EcnEcoBatchStepStatus.CompletedWithFailures);

        /// <summary>Item-level failures reported by at least one completed step.</summary>
        public bool HasFailures => Statuses.Any(s => s == EcnEcoBatchStepStatus.CompletedWithFailures);

        /// <summary>Warnings of ECN follow-up and ECO SAP HUP (PdmContextSyncResult exposes no warnings).</summary>
        public bool HasWarnings => (EcnFollowUp?.Warnings.Count ?? 0) > 0 || (EcoSapHup?.Warnings.Count ?? 0) > 0;

        /// <summary>True when all three steps completed without item-level failure.</summary>
        public bool IsSuccess => Statuses.All(s => s == EcnEcoBatchStepStatus.Completed);

        private IEnumerable<EcnEcoBatchStepStatus> Statuses => new[] { PdmContextStatus, EcnFollowUpStatus, EcoSapHupStatus };

        public override string ToString() =>
            $"PdmContext={PdmContextStatus}, EcnFollowUp={EcnFollowUpStatus}, EcoSapHup={EcoSapHupStatus}, completed={CompletedStepCount}/3";
    }
}
