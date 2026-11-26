using Fluent;
using System.Windows;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    public partial class PurchaseOrderFollowUpAddInternalOrderView : RibbonWindow
    {
        public PurchaseOrderFollowUpAddInternalOrderView()
        {
            InitializeComponent();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
