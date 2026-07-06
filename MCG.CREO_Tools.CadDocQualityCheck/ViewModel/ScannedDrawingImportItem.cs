namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class ScannedDrawingImportItem
    {
        public string NUMBER { get; set; }

        public override string ToString()
        {
            if (NUMBER != null)
                return NUMBER;
            else
                return base.ToString();
        }
    }
}
