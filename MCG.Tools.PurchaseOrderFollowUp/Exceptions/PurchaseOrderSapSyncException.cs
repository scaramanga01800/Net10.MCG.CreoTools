namespace MCG.Tools.PurchaseOrderFollowUp.Exceptions
{
    /// <summary>
    /// Échec global de la synchronisation SAP HUP. L'exception d'origine est conservée dans InnerException
    /// (stack trace intacte). Aucune interaction utilisateur n'est déclenchée.
    /// </summary>
    public sealed class PurchaseOrderSapSyncException : Exception
    {
        public PurchaseOrderSapSyncException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
