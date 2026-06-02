using MCG.WindchillRequestTool.Exceptions;
using System.Windows.Controls;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.BomExport
{
    public partial class BomExportTabContentView : UserControl
    {
        private bool IsAlreadyInit { get; set; } = false;
        public BomExportTabContentView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                WindchillRequestException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void TreeView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ScrollHori.ScrollToLeftEnd();
        }
    }
}
