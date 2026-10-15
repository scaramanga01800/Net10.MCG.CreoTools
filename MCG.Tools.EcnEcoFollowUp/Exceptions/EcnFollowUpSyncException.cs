namespace MCG.Tools.EcnEcoFollowUp.Exceptions
{
    /// <summary>
    /// Global failure of the ECN follow-up synchronization (credentials, LASTPDMECNSEARCH tracking state,
    /// new-ECN discovery query, persistence infrastructure). The original exception is kept in InnerException
    /// when one exists. Never contains credentials or connection information.
    /// </summary>
    public sealed class EcnFollowUpSyncException : Exception
    {
        public EcnFollowUpSyncException(string message)
            : base(message)
        {
        }

        public EcnFollowUpSyncException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
