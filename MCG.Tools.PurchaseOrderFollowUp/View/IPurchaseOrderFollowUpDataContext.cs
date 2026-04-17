using LiveCharts;
using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    public interface IPurchaseOrderFollowUpDataContext
    {
        ObservableCollection<PurchaseOrderRequest> ListRequest { get; set; }
        ObservableCollection<PurchaseOrderRequest> ListMyRequest { get; set; }
        ObservableCollection<PurchaseOrderRequest> ListShownMyRequest { get; set; }
        ObservableCollection<PurchaseOrderRequest> ListSearchedRequest { get; set; }
        ObservableCollection<PurchaseOrderRequest> ListShownRequest { get; set; }
        PurchaseOrderRequest SelectedRequest { get; set; }
        PurchaseOrderRequest CurrentRequest { get; set; }

        ObservableCollection<PurchaseOrderType> ListPurchaseType { get; set; }
        //PurchaseOrderType SelectedPurchaseType { get; set; }

        ObservableCollection<PurchaseOrderCostCenter> ListCostCenter { get; set; }
        //PurchaseOrderCostCenter SelectedCostCenter { get; set; }

        ObservableCollection<PurchaseOrderInternalOrder> ListAllInternalOrder { get; set; }
        PurchaseOrderInternalOrder SelectedInternalOrder { get; set; }

        ObservableCollection<PurchaseOrderInternalOrder> ListInternalOrder { get; set; }
        //PurchaseOrderInternalOrder SelectedInternalOrder { get; set; }

        ObservableCollection<PurchaseOrderMaterial> ListDienNlagMaterial { get; set; }

        ObservableCollection<PurchaseOrderVendor> ListVendor { get; set; }
        PurchaseOrderVendor CurrentVendor { get; set; }

        ObservableCollection<PurchaseOrderLocation> ListDeliveryLocation { get; set; }

        string GeneralDescription { get; set; }

        bool IsDienNlagMaterial { get; set; }

        Object ListItem { get; set; }
        Object SelectedItem { get; set; }
        bool IsCloseBtShown { get; set; }

        string NumberSearchField { get; set; }
        string DescriptionSearchField { get; set; }
        bool HasLocationProperty { get; set; }
        bool IsAllBtRequestTypeShown { get; set; }

        string BtRequestTypeQuestion { get; set; }
        string BtRequestType1 { get; set; }
        string BtRequestType2 { get; set; }
        string BtRequestType3 { get; set; }
        string BtRequestType4 { get; set; }
        string BtRequestType5 { get; set; }
        string BtRequestType6 { get; set; }

        ObservableCollection<PurchaseOrderMaterialGroup> ListMaterialGroup { get; set; }

        string InternalOrderDescription { get; set; }

        bool IsRoleAdmin { get; set; }
        bool IsRoleSuperviser { get; set; }
        bool IsRoleSapCreator { get; set; }

        DateOnly? PoCreatedAfter { get; set; }
        DateOnly? PoCreatedBefore { get; set; }

        bool StatusNewSelected { get; set; }
        bool StatusSentSelected { get; set; }
        bool StatusCreatedSelected { get; set; }
        bool StatusGoodsReceiptSelected { get; set; }
        bool StatusInvoiceReceiptSelected { get; set; }
        bool StatusClosedSelected { get; set; }
        bool IsNoActionInProgress { get; set; }

        int CurrentStep { get; set; }
        int TotalStep { get; set; }

        // For charts
        bool ChartStatusNewSelected { get; set; }
        bool ChartStatusSentSelected { get; set; }
        bool ChartStatusCreatedSelected { get; set; }
        bool ChartStatusGoodsReceiptSelected { get; set; }
        bool ChartStatusInvoiceReceiptSelected { get; set; }
        bool ChartStatusClosedSelected { get; set; }
        bool IsAllCostCenterSelected { get; set; }

        DateOnly? ChartPoCreatedAfter { get; set; }
        DateOnly? ChartPoCreatedBefore { get; set; }

        SeriesCollection AllPurchasePieSeriesCost { get; set; }
        SeriesCollection AllPurchasePieSeriesNumber { get; set; }
        SeriesCollection AllPurchasePieSeriesVendorCost { get; set; }

        double MinVendorRatio { get; set; }
        Func<double, string> PieSeriesCostFormatter { get; set; }

        bool IsPleaseWaitShown { get; set; }

        ObservableCollection<PurchaseOrderDuplicate> ListDuplicateRequest { get; set; }

    }
}
