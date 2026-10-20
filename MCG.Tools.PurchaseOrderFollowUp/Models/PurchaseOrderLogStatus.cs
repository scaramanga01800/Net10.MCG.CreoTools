namespace MCG.Tools.PurchaseOrderFollowUp.Models
{
    /// <summary>
    /// Statuts utilisés dans les lignes de log synthétiques "PO|RequestId=...|PR=...|PO=...|Status=...".
    /// </summary>
    public static class PurchaseOrderLogStatus
    {
        public const string Created = "CREATED";
        public const string Updated = "UPDATED";
        public const string AlreadyExists = "ALREADY_EXISTS";
        public const string Failed = "FAILED";
        public const string Skipped = "SKIPPED";
    }
}
