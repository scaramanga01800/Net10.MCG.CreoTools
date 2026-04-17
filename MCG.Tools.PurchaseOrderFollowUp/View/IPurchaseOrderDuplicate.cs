using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    public interface IPurchaseOrderDuplicate
    {
        ObservableCollection<PurchaseOrderRequest> ListeDuplicateOrder {  get; set; }
        int NumberItem { get; set; }
    }
}
