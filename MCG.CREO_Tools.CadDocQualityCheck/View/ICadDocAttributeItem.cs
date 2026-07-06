using MCG.CREO_Tools.CadDocQualityCheck.ViewModel;

namespace MCG.CREO_Tools.CadDocQualityCheck.View
{
    public interface ICadDocAttributeItem
    {
        string Name { get; set; }
        string Type { get; set; }
        bool IsDesignated { get; set; }

        bool IsMissing { get; set; }
        bool IsDesignatedOk { get; set; }
        bool IsUpdated { get; set; }

        CadDocCheckStatus AttributeStatus { get; set; }
    }
}
