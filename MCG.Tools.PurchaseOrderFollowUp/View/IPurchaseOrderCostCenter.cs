
namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    public interface IPurchaseOrderCostCenter
    {
        string Number { get; set; }
        string Description { get; set; }
        bool IsSelected { get; set; }
    }
}
