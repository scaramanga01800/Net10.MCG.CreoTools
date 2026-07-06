namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class CadDocTemplate
    {
        public string FileName { get; set; }
        public string Template { get; set; }
        public string CadDocType { get; set; }
        public string PreRegenRelations { get; set; }
        public string PostRegenRelations { get; set; }
        public List<string> PurgedPreRegenRelations { get; set; } = null;
        public List<string> PurgedPostRegenRelations { get; set; } = null;
        public List<CadDocAttributeItem> Attributes { get; set; } = null;
        public bool IsDefaultAsm { get; set; } = false;
        public bool IsDefaultPrt { get; set; } = false;
        public bool IsDefaultSheetMetal { get; set; } = false;
        public bool IsDefaultBulk { get; set; } = false;
        public List<string> MainRefPlans { get; set; }
        public string MainCoordSystem { get; set; }
        public List<CadDocLayerItemConfig> MandatoryLayers { get; set; }
        public bool IsLoaded { get; set; } = false;
        public bool ShouldBeLoaded { get; set; } = false;

        public override string ToString()
        {
            return Template; 
        }
    }
}
