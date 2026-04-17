using Fluent.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCG.Tools.PurchaseOrderFollowUp.Interfaces
{
    public interface IPurchaseOrderFollowWindowService
    {
        void ShowPurchaseOrderFollowCreateUpdateView(object currentDataContext);
        void ShowPurchaseOrderFollowListRequestView(object currentDataContext);
        void ShowPurchaseOrderFollowUpCreateUpdateVendorView(object currentDataContext);
        void ShowPurchaseOrderFollowUpDuplicate(object currentDataContext);
        void ShowPurchaseOrderFollowUpExtendedPartView(object currentDataContext);
        void ShowPurchaseOrderFollowUpInternalOrderRequestView(object currentDataContext);
        void ShowPurchaseOrderFollowUpSelectVendorView(object currentDataContext);

        void ShowDialogPurchaseOrderFollowCreateUpdateView(object currentDataContext);
        void ShowDialogPurchaseOrderFollowListRequestView(object currentDataContext);
        void ShowDialogPurchaseOrderFollowUpCreateUpdateVendorView(object currentDataContext);
        void ShowDialogPurchaseOrderFollowUpDuplicate(object currentDataContext);
        void ShowDialogPurchaseOrderFollowUpExtendedPartView(object currentDataContext);
        bool? ShowDialogPurchaseOrderFollowUpInternalOrderRequestView(object currentDataContext);
        bool? ShowDialogPurchaseOrderFollowUpSelectVendorView(object currentDataContext);

        void ClosePurchaseOrderFollowCreateUpdateView();
        void ClosePurchaseOrderFollowListRequestView();
        void ClosePurchaseOrderFollowUpCreateUpdateVendorView();
        void ClosePurchaseOrderFollowUpDuplicate();
        void ClosePurchaseOrderFollowUpExtendedPartView();
        void ClosePurchaseOrderFollowUpInternalOrderRequestView();
        void ClosePurchaseOrderFollowUpSelectVendorView();
    }
}
