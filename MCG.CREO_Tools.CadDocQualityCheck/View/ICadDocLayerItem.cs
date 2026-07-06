using MCG.CommonLib.Models.Enums;
using MCG.CREO_Tools.CadDocQualityCheck.ViewModel;

namespace MCG.CREO_Tools.CadDocQualityCheck.View
{
    public interface ICadDocLayerItem
    {
        string Name { get; set; }

        ObjectState State { get; set; }

        bool IsDisplayed { get; set; }
        CadDocCheckStatus LayerStatus { get; set; }
    }
}
