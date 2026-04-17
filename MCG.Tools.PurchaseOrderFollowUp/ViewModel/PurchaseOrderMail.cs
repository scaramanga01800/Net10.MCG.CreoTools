
namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderMail
    {
        public string Email { get; set; } = string.Empty;
        public string EmailCC { get; set; } = string.Empty;
        public PurchaseOrderType TypeRequest { get; set; }
    }
}
