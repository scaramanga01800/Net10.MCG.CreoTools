namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderItemSapHubInformation
    {
        public string SapPurchaseOrder { get; set; } = string.Empty;
        public int Number { get; set; }
        public PurchaseOrderStatus OperationStatus { get; set; }
        public double Quantity_GR { get; set; }
        public double Price_GR { get; set; }
        public double Price_Paid { get; set; }
        public double Quantity_Ordered { get; set; }
        public double Price_Ordered { get; set; }
        public string CreditDebit_Info { get; set; } = string.Empty;
        public string Mouvement_Type { get; set; } = string.Empty;
        public double Canceled_Value { get; set; }
        public double Price_Gr_And_CanceledGr { get; set; }
        public bool Closed_Check {  get; set; }
    }
}
