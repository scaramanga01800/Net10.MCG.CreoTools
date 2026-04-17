using Fluent;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    /// <summary>
    /// Logique d'interaction pour PurchaseOrderFollowListRequestView.xaml
    /// </summary>
    public partial class PurchaseOrderFollowListRequestView : RibbonWindow
    {
        public PurchaseOrderFollowListRequestView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
