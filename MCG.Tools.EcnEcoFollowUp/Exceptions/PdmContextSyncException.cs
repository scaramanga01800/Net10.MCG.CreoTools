namespace MCG.Tools.EcnEcoFollowUp.Exceptions
{
    /// <summary>
    /// Global failure of the PDM context synchronization (credential resolution, Windchill retrieval,
    /// planning or persistence). The original exception is kept in InnerException. Never contains credentials.
    /// </summary>
    public sealed class PdmContextSyncException : Exception
    {
        public PdmContextSyncException(string message)
            : base(message)
        {
        }

        public PdmContextSyncException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
