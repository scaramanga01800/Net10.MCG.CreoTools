using MCG.Tools.EcnEcoFollowUp.Models.EcnEcoBatch;

namespace MCG.Tools.EcnEcoFollowUp.Exceptions
{
    /// <summary>
    /// Global failure of one orchestration step. InnerException is the original child exception;
    /// PartialResult keeps already completed step results and marks later steps as skipped. No secrets.
    /// </summary>
    public sealed class EcnEcoFollowUpBatchException : Exception
    {
        public EcnEcoBatchStep FailedStep { get; }
        public EcnEcoFollowUpBatchResult PartialResult { get; }

        /// <summary>True when a WindchillBatchCredentialConfigurationException exists in the inner-exception chain
        /// (e.g. wrapped by PdmContextSyncException).</summary>
        public bool IsConfigurationError
        {
            get
            {
                for (var e = InnerException; e != null; e = e.InnerException)
                    if (e is MCG.CommonLib.DataBaseAccess.Exceptions.WindchillBatchCredentialConfigurationException)
                        return true;
                return false;
            }
        }

        public EcnEcoFollowUpBatchException(EcnEcoBatchStep failedStep, EcnEcoFollowUpBatchResult partialResult, Exception innerException)
            : base($"Echec global de l'etape ECN/ECO '{failedStep}'. Etapes suivantes non executees.", innerException)
        {
            FailedStep = failedStep;
            PartialResult = partialResult ?? throw new ArgumentNullException(nameof(partialResult));
        }
    }
}
