using MCG.Tools.PurchaseOrderFollowUp.Models;

namespace MCG.Tools.PurchaseOrderFollowUp.Interfaces
{
    public interface IPurchaseOrderReferenceDataProvider
    {
        /// <summary>
        /// Charge les données de référence depuis MainAppFolder\Resources\PurchaseOrderFollowUpConfiguration.xml
        /// et la base (matériaux DIEN/NLAG), sans dépendance à l'interface utilisateur.
        /// </summary>
        PurchaseOrderReferenceData Load(CancellationToken cancellationToken = default);
    }
}
