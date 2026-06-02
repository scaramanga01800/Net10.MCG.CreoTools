using Fluent;
using MCG.CREO_Tools.MiscTools.ViewModel.BomExport;

namespace MCG.CREO_Tools.MiscTools.View.BomExport
{
    /// <summary>
    /// Logique d'interaction pour BomExportCumulativeView.xaml
    /// </summary>
    public partial class BomExportCumulativeView : RibbonWindow
    {
        public BomExportCumulativeView()
        {
            InitializeComponent();
        }

        public void SetDataContext(BomExportWindowViewModel dataContext)
        {
            this.DataContext = dataContext;
        }
    }
}
