namespace MCG.Tools.EcnEcoFollowUp.Exceptions
{
    /// <summary>
    /// Global failure of the SAP HUP ECO synchronization. The original exception is kept in InnerException.
    /// No user interaction is triggered.
    /// </summary>
    public sealed class EcoSapHupSyncException : Exception
    {
        public EcoSapHupSyncException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
