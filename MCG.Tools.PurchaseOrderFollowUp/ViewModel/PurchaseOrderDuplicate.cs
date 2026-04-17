using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.View;
using System.Collections.ObjectModel;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderDuplicate: ObservableObject, IPurchaseOrderDuplicate
    {
        public ObservableCollection<PurchaseOrderRequest> ListeDuplicateOrder {  get; set; } = new ObservableCollection<PurchaseOrderRequest>();
        public int NumberItem { get; set; }

        public PurchaseOrderDuplicate(List<PurchaseOrderRequest> listOrder, int number)
        {
            try
            {
                if (listOrder != null)
                {
                    foreach (var item in listOrder)
                    {
                        ListeDuplicateOrder.Add(item);
                    }
                }
                NumberItem = number;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }
    }
}
