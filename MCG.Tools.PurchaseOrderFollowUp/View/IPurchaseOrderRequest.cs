using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    public interface IPurchaseOrderRequest
    {
        int ID { get; set; }
        string CreatedBy { get; set; }
        DateOnly? CreatedOn { get; set; }
        string RequestedBy { get; set; }
        string SapCreatedBy { get; set; }
        DateOnly? SapCreatedOn { get; set; }
        PurchaseOrderType RequestType { get; set; }
        string Description { get; set; }
        string SapPurchaseOrder { get; set; }
        string SapPurchaseRequest { get; set; }
        PurchaseOrderStatus Status { get; set; }
        PurchaseOrderInternalOrder InternalOrder { get; set; }
        PurchaseOrderCostCenter CostCenter { get; set; }
        PurchaseOrderVendor Vendor { get; set; }

        ObservableCollection<PurchaseOrderItem> ListItem { get; set; }
        ObservableCollection<PurchaseOrderItem> UpdatedListItem { get; set; }
        PurchaseOrderItem SelectedItem { get; set; }

        ObservableCollection<PurchaseOrderAttachment> ListAttachment { get; set; }
        ObservableCollection<PurchaseOrderAttachment> UpdatedListAttachment { get; set; }
        PurchaseOrderAttachment SelectedAttachment { get; set; }

        bool IsUpdateAllowed { get; set; }
        bool IsAlreadyExist { get; set; }

        double Total_Ordered { get; set; }
        double Total_Goods { get; set; }
        double Total_Real_Goods { get; set; }

        string CurrencySymbol { get; set; }

        bool CanBeClosedWithoutSaving { get; set; }
        string WindowTitle { get; set; }
        bool IsAdminUpdate { get; set; }
    }
}
