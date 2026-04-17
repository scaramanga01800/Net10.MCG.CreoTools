using MCG.Tools.PurchaseOrderFollowUp.ViewModel;

namespace MCG.Tools.PurchaseOrderFollowUp.Configuration
{
    public class PurchaseOrderFollowUpConfiguration
    {
        public List<PurchaseOrderCostCenter> ListCostCenter { get; set; }

        public List<PurchaseOrderType> ListOrderType { get; set; }

        public List<PurchaseOrderVendor> ListPlantVendor { get; set; }

        public List<PurchaseOrderLocation> ListDeliveryLocation { get; set; }

    }
}
