using MCG.CREO_Tools.CadDocQualityCheck.ViewModel;

namespace MCG.CREO_Tools.CadDocQualityCheck.View
{
    public interface ICadDocQualityCheckResultItem
    {
        CadDocQualityCheckItem ParentQualityCheckItem { get; set; }

        string Comments { get; set; }

        CadDocCheckStatus Status { get; set; }

    }
}
