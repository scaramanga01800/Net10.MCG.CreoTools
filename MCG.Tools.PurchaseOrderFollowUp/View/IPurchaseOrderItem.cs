using MCG.CommonLib.Models.SAP;
using MCG.Tools.PurchaseOrderFollowUp.ViewModel;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    public interface IPurchaseOrderItem
    {
        int Number { get; set; }
        string Description { get; set; }
        string AccountAssignementCategory { get; set; }
        string Material { get; set; }
        PurchaseOrderMaterial SelectedMaterial { get; set; }
        double Quantity { get; set; }
        DateOnly? DeliveryDate { get; set; }
        DateTime? DeliveryDateDateTime   { get; set; }
        double Price { get; set; }
        PurchaseOrderInternalOrder InternalOrder { get; set; }
        PurchaseOrderCostCenter CostCenter { get; set; }
        string RequestedBy { get; set; }
        PurchaseOrderVendor Vendor { get; set; }
        string Detail { get; set; }
        PurchaseOrderLocation DeliveryAdress { get; set; }
        SapPlant DeliveryPlant { get; set; }
        PurchaseOrderStatus GoodReceiptStatus { get; set; }
        double Total_Ordered { get; set; }
        double Total_Goods { get; set; }
        double Total_Invoice { get; set; }
        double Total_Real_Goods { get; set; }
        bool Closed_Check { get; set; }
        bool IsExtended { get; set; }

        PurchaseOrderStatus PurchasingViewStatus { get; set; }
        PurchaseOrderStatus MrpViewStatus { get; set; }
        PurchaseOrderStatus StorageViewStatus { get; set; }
        PurchaseOrderStatus QualityViewStatus { get; set; }
    }
}
