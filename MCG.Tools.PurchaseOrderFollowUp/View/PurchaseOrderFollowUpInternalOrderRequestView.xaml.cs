using Fluent;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using System.Windows;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    public partial class PurchaseOrderFollowUpInternalOrderRequestView : RibbonWindow
    {
        public PurchaseOrderFollowUpInternalOrderRequestView()
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

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = true;
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult=false;
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
