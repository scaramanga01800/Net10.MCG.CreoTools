using MCG.CommonLib.Models.Enums;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    public interface IPurchaseOrderMaterial
    {
        string Number { get; set; }
        string Description { get; set; }
        SapMaterialType Type { get; set; }
    }
}
