using MCG.CommonLib.DataBaseAccess.Infrastructure;
using MCG.Tools.EcnEcoFollowUp.Models.EcnFollowUp;

namespace MCG.Tools.EcnEcoFollowUp.Interfaces
{
    /// <summary>
    /// Équivalents batch (clause IN) de IEcnFollowUpSyncRepository.EcnExists et FindPdmContext.
    /// Interface séparée : IEcnFollowUpSyncRepository et ses fakes ne sont pas modifiés.
    /// Clés nulles ou vides ignorées, doublons ignorés, comparaison ordinale (comme l'égalité Oracle sensible à la casse).
    /// </summary>
    public interface IEcnFollowUpBatchRepository
    {
        /// <summary>
        /// Sous-ensemble des numéros ECN demandés présents dans ECNECOFOLLOWUP
        /// (équivalent de EcnExists(n) == true pour chaque élément retourné).
        /// </summary>
        HashSet<string> GetExistingEcnNumbers(IEnumerable<string?> ecnNumbers, CancellationToken cancellationToken, int batchSize = OracleBatchHelper.RecommendedBatchSize);

        /// <summary>
        /// Pour chaque nom de contexte trouvé, la première ligne PDMCONTEXT par ID (équivalent de FindPdmContext(name) != null).
        /// Les noms absents de la base n'ont pas d'entrée (équivalent de FindPdmContext(name) == null).
        /// </summary>
        Dictionary<string, PdmContextLookup> FindPdmContextsBatch(IEnumerable<string?> contextNames, CancellationToken cancellationToken, int batchSize = OracleBatchHelper.RecommendedBatchSize);
    }
}
