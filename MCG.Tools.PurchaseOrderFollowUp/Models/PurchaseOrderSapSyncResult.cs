namespace MCG.Tools.PurchaseOrderFollowUp.Models
{
    /// <summary>
    /// Erreur non fatale survenue pendant le traitement d'une demande. L'exception d'origine est conservée.
    /// </summary>
    public sealed record PurchaseOrderSapSyncError(
        string Operation,
        int? RequestId,
        string? SapPurchaseRequest,
        string? SapPurchaseOrder,
        Exception Exception);

    /// <summary>
    /// Compteurs vérifiables d'une synchronisation SAP HUP.
    /// </summary>
    public sealed class PurchaseOrderSapSyncResult
    {
        private readonly List<PurchaseOrderSapSyncError> _errors = new();

        /// <summary>Nombre de demandes (regroupées par numéro de PO SAP) retournées par SAP HUP sur la plage de dates.</summary>
        public int SapRequestsAnalyzed { get; internal set; }

        /// <summary>Nouvelles demandes SAP enregistrées pour lesquelles UpsertRequest a signalé une création.</summary>
        public int RequestsCreated { get; internal set; }

        /// <summary>Nouvelles demandes SAP enregistrées pour lesquelles UpsertRequest a signalé une mise à jour.</summary>
        public int NewSapRequestsUpsertedAsExisting { get; internal set; }

        /// <summary>Demandes SAP ignorées car déjà présentes en base (par PO SAP ou PR SAP).</summary>
        public int ExistingRequestsSkipped { get; internal set; }

        /// <summary>Demandes ouvertes avec PR SAP chargées depuis la base (GetOpenSapPrRequests).</summary>
        public int OpenRequestsAnalyzed { get; internal set; }

        /// <summary>Demandes ouvertes avec PR SAP mises à jour avec succès.</summary>
        public int OpenRequestsUpdated { get; internal set; }

        /// <summary>
        /// Demandes ouvertes avec PO SAP sans PR SAP chargées mais non traitées (comportement historique conservé, dette technique).
        /// </summary>
        public int OpenRequestsWithSapPoWithoutSapPrNotProcessed { get; internal set; }

        /// <summary>Nombre d'échecs de traitement par demande.</summary>
        public int Failures => _errors.Count;

        public bool HasFailures => _errors.Count > 0;

        public IReadOnlyList<PurchaseOrderSapSyncError> Errors => _errors.AsReadOnly();

        internal void AddError(PurchaseOrderSapSyncError error) => _errors.Add(error);

        private readonly List<ViewModel.PurchaseOrderRequest> _savedNewSapRequests = new();

        /// <summary>Nouvelles demandes SAP enregistrées (création ou upsert), pour la mise à jour des listes d'affichage.</summary>
        public IReadOnlyList<ViewModel.PurchaseOrderRequest> SavedNewSapRequests => _savedNewSapRequests.AsReadOnly();

        internal void AddSavedNewSapRequest(ViewModel.PurchaseOrderRequest request) => _savedNewSapRequests.Add(request);

        /// <summary>
        /// Résumé final de log, calculé exclusivement à partir des compteurs internes.
        /// Updated = upserts signalés comme existants + demandes ouvertes mises à jour ;
        /// Skipped = PO sans PR non traitées ; Failed = échecs.
        /// </summary>
        public string ToSummaryLog() =>
            "Purchase Order summary | " +
            $"RequestsAnalyzed={SapRequestsAnalyzed + OpenRequestsAnalyzed} | Created={RequestsCreated} | " +
            $"Updated={NewSapRequestsUpsertedAsExisting + OpenRequestsUpdated} | AlreadyExists={ExistingRequestsSkipped} | " +
            $"Skipped={OpenRequestsWithSapPoWithoutSapPrNotProcessed} | Failed={Failures}";

        public override string ToString() =>
            $"SAP analysées={SapRequestsAnalyzed}, créées={RequestsCreated}, upsert existantes={NewSapRequestsUpsertedAsExisting}, " +
            $"existantes ignorées={ExistingRequestsSkipped}, ouvertes analysées={OpenRequestsAnalyzed}, ouvertes mises à jour={OpenRequestsUpdated}, " +
            $"PO sans PR non traitées={OpenRequestsWithSapPoWithoutSapPrNotProcessed}, échecs={Failures}";
    }
}
