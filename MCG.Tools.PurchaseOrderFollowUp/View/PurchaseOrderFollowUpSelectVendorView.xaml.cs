using Fluent;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using System.Windows;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    /// <summary>
    /// Logique d'interaction pour PurchaseOrderFollowUpSelectVendorView.xaml
    /// </summary>
    public partial class PurchaseOrderFollowUpSelectVendorView : RibbonWindow
    {
        public PurchaseOrderFollowUpSelectVendorView()
        {
            try
            {
                this.InitializeComponent();
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ButtonCheck_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
