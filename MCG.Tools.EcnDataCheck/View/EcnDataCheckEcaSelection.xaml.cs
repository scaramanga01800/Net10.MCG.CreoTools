
using Fluent;
using System.Windows;

namespace MCG.Tools.EcnDataCheck.View
{
    /// <summary>
    /// Logique d'interaction pour EcnDataCheckEcaSelection.xaml
    /// </summary>
    public partial class EcnDataCheckEcaSelection : RibbonWindow
    {
        public MessageBoxResult ReturnValue { get; set; }
       
        public EcnDataCheckEcaSelection()
        {
            InitializeComponent();
        }

        private void BtOK_Click(object sender, RoutedEventArgs e)
        {
            ReturnValue = MessageBoxResult.OK;
            this.DialogResult = true;
            this.Close();
        }

        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            ReturnValue = MessageBoxResult.Cancel;
            this.DialogResult = false;
            this.Close();
        }
    }
}
