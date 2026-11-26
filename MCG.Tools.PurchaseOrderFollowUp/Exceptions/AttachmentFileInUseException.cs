namespace MCG.Tools.PurchaseOrderFollowUp.Exceptions
{
    /// <summary>
    /// Levée lorsqu'un fichier à joindre est verrouillé par une autre application (ex: fichier Excel ouvert).
    /// Cas fonctionnel : ne doit pas déclencher l'envoi d'un log à l'administrateur.
    /// </summary>
    public class AttachmentFileInUseException : Exception
    {
        public string CompleteFilename { get; }

        public string FileName => System.IO.Path.GetFileName(CompleteFilename);

        public AttachmentFileInUseException(string completeFilename, Exception innerException = null)
            : base($"The file '{completeFilename}' is in use by another process.", innerException)
        {
            CompleteFilename = completeFilename ?? string.Empty;
        }
    }
}
