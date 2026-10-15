using MCG.Tools.PurchaseOrderFollowUp.Models;

namespace MCG.Tools.PurchaseOrderFollowUp.Interfaces
{
    /// <summary>
    /// Synchronisation SAP HUP des demandes d'achat, sans interface utilisateur.
    /// Les API SAP HUP / base utilisées sont synchrones : le service est volontairement synchrone.
    /// </summary>
    public interface IPurchaseOrderSapSyncService
    {
        /// <summary>
        /// Crée en base les demandes SAP HUP (PO créés entre fromDate et toDate) qui n'existent pas encore.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">fromDate &gt; toDate.</exception>
        /// <exception cref="Exceptions.PurchaseOrderSapSyncException">Échec de la recherche SAP HUP.</exception>
        PurchaseOrderSapSyncResult CreateRequestsFromSap(DateTime fromDate, DateTime toDate, PurchaseOrderReferenceData referenceData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Met à jour depuis SAP HUP les demandes ouvertes possédant une PR SAP.
        /// </summary>
        /// <exception cref="Exceptions.PurchaseOrderSapSyncException">Échec du chargement des demandes ouvertes.</exception>
        PurchaseOrderSapSyncResult UpdateOpenRequestsFromSap(PurchaseOrderReferenceData referenceData, CancellationToken cancellationToken = default, IProgress<PurchaseOrderSapSyncProgress>? progress = null);

        /// <summary>
        /// Recherche (sans enregistrement) les demandes SAP HUP des PO créés entre fromDate et toDate.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">fromDate &gt; toDate.</exception>
        List<ViewModel.PurchaseOrderRequest> SearchRequestsFromSap(DateTime fromDate, DateTime toDate, PurchaseOrderReferenceData referenceData);

        /// <summary>
        /// Charge depuis la base les articles et pièces jointes d'une demande (articles sans fournisseur / ordre interne tolérés).
        /// </summary>
        void LoadRequestDetails(ViewModel.PurchaseOrderRequest request, PurchaseOrderReferenceData referenceData);

        /// <summary>
        /// Met à jour une seule demande depuis SAP HUP, sans l'enregistrer.
        /// includePurchaseRequest = true : PR puis, si un PO existe, PO et réceptions ; false : PO et réceptions uniquement.
        /// </summary>
        void RefreshRequestFromSap(ViewModel.PurchaseOrderRequest request, PurchaseOrderReferenceData referenceData, bool includePurchaseRequest);

        /// <summary>
        /// Crée ou met à jour une demande en base. Retourne true si la demande a été créée.
        /// </summary>
        bool SaveRequest(ViewModel.PurchaseOrderRequest request);

        /// <summary>
        /// Séquence historique du batch : création depuis SAP puis mise à jour des demandes ouvertes.
        /// </summary>
        PurchaseOrderSapSyncResult Synchronize(DateTime fromDate, DateTime toDate, PurchaseOrderReferenceData referenceData, CancellationToken cancellationToken = default);
    }
}
