using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.ViewModel.BomComparison;
using MCG.WindchillRequestTool.Exceptions;
using System.Windows;
using System.Windows.Controls;

namespace MCG.CREO_Tools.MiscTools.View.BomComparison
{
    public partial class BomComparisonTabContentView : UserControl
    {
        private bool IsAlreadyInit { get; set; } = false;
        private BomComparisonViewModel CurrenBomComparisonViewModel { get; set; }

        public BomComparisonTabContentView()
        {
            try
            {
                DataContextChanged += BomComparisonTabMainView_DataContextChanged;
                InitializeComponent();
            }
            catch (MiscToolsException ex)
            {
                WindchillRequestException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void BomComparisonTabMainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(BomComparisonViewModel))
                {
                    CurrenBomComparisonViewModel = DataContext as BomComparisonViewModel;
                    IsAlreadyInit = true;
                }
            }
            catch (Exception ex)
            {
                WindchillRequestException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
