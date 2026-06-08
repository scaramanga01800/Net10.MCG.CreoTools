using MCG.CREO_Tools.DxfExport.Exceptions;
using MCG.CREO_Tools.DxfExport.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MCG.CREO_Tools.DxfExport.View
{
    public partial class DxfExportFluentTabContentView : UserControl
    {
        private bool IsAlreadyInit { get; set; } = false;
        private DxfExportViewModel CurrentDataContext { get; set; }

        public DxfExportFluentTabContentView()
        {
            try
            {
                InitializeComponent();
                DataContextChanged += DxfExportFluentTabContentView_DataContextChanged;
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void DxfExportFluentTabContentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(DxfExportViewModel))
                {
                    CurrentDataContext = DataContext as DxfExportViewModel;
                    IsAlreadyInit = true;
                }
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
