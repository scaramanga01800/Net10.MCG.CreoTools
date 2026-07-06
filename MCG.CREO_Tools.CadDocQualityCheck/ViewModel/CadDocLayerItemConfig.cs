namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class CadDocLayerItemConfig
    {
        public string Name { get; set; }
        public bool IsDisplayed { get; set; }
        public bool ToBeCreatedIfMissing { get; set; } = false;
        public string RefType { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
