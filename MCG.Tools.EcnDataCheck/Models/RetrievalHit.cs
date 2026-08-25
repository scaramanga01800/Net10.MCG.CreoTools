namespace MCG.Tools.EcnDataCheck.Models
{
    public class RetrievalHit
    {
        public string WebUrl
        {
            get;
            set;
        }

        public List<ExtractItem> Extracts
        {
            get;
            set;
        }
    }
}
