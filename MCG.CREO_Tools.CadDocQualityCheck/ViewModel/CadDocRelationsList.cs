namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class CadDocRelationsList
    {
        public List<CadDocRelationLineItem> ExtraRelations { get; set; }
        public List<CadDocRelationLineItem> MissingRelations { get; set; }
        public bool IsRelationsOK { get; set; }
    }
}
