namespace MCG.CREO_Tools.CadDocQualityCheck.View
{
    public interface ICadDocRelationLineItem
    {
        string Relation { get; set; }
        bool IsExtra { get; set; }
        bool IsMissing { get; set; }
        bool IsOk { get; set; }
    }
}
