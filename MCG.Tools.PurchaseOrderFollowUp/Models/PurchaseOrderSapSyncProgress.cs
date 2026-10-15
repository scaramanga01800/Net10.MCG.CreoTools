namespace MCG.Tools.PurchaseOrderFollowUp.Models
{
    /// <summary>
    /// Avancement neutre (sans dépendance UI) de la mise à jour des demandes ouvertes.
    /// TotalStep inclut les demandes PO sans PR chargées mais non traitées (comportement historique).
    /// </summary>
    public readonly record struct PurchaseOrderSapSyncProgress(int CurrentStep, int TotalStep);
}
