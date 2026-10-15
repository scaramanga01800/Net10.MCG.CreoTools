using MCG.Tools.PurchaseOrderFollowUp.ViewModel;

namespace MCG.Tools.PurchaseOrderFollowUp.Models
{
    /// <summary>
    /// Données de référence nécessaires à la synchronisation SAP HUP des demandes d'achat.
    /// Contexte explicite, indépendant de l'interface utilisateur (aucun DataContext, aucune collection graphique).
    /// </summary>
    public sealed class PurchaseOrderReferenceData
    {
        public PurchaseOrderReferenceData(
            IEnumerable<PurchaseOrderLocation>? deliveryLocations,
            IEnumerable<PurchaseOrderCostCenter>? costCenters,
            IEnumerable<PurchaseOrderMaterial>? dienNlagMaterials)
        {
            DeliveryLocations = (deliveryLocations ?? Enumerable.Empty<PurchaseOrderLocation>()).ToList().AsReadOnly();
            CostCenters = (costCenters ?? Enumerable.Empty<PurchaseOrderCostCenter>()).ToList().AsReadOnly();
            DienNlagMaterials = (dienNlagMaterials ?? Enumerable.Empty<PurchaseOrderMaterial>()).ToList().AsReadOnly();
        }

        /// <summary>Lieux de livraison (PurchaseOrderFollowUpConfiguration.xml, ListDeliveryLocation).</summary>
        public IReadOnlyList<PurchaseOrderLocation> DeliveryLocations { get; }

        /// <summary>Centres de coût (PurchaseOrderFollowUpConfiguration.xml, ListCostCenter).</summary>
        public IReadOnlyList<PurchaseOrderCostCenter> CostCenters { get; }

        /// <summary>Matériaux DIEN/NLAG (IPurchaseOrderService.GetAllPoMatDienNlags).</summary>
        public IReadOnlyList<PurchaseOrderMaterial> DienNlagMaterials { get; }
    }
}
