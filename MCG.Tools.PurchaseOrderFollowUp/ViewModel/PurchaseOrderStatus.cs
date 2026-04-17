namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public enum PurchaseOrderStatus
    {
        UNKNOWN=0,
        NEW=1,
        SENT=2,
        CREATED=3,
        UNDER_REVIEW=4,
        PR_CREATED = 5,
        PO_CREATED = 6,
        PR_APPROVED=7,
        PO_APPROVED = 8,
        PARTIAL_GOODS_RECEIPT=10,
        GOODS_RECEIPT=11,
        PARTIAL_INVOICE_RECEIPT = 12,
        INVOICE_RECEIPT=13,
        CLOSED=14,
        REWORK=15,
        DELIVERED=16,
        TO_BE_CREATED= 17,
        NOT_APPLICABLE= 18
    }
}
