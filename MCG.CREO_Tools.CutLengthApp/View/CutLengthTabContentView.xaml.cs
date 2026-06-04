using MCG.CREO_Tools.CutLengthApp.Exceptions;
using MCG.CREO_Tools.CutLengthApp.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MCG.CREO_Tools.CutLengthApp.View
{
    public partial class CutLengthTabContentView : UserControl
    {
        private bool IsAlreadyInit { get; set; } = false;
        public CutLengthViewModel CurrentDataContext { get; set; }

        public CutLengthTabContentView()
        {
            try
            {
                InitializeComponent();
                DataContextChanged += CutLengthMainView_DataContextChanged;
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void CutLengthMainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(CutLengthViewModel))
                {
                    CurrentDataContext = ((CutLengthViewModel)DataContext);
                    IsAlreadyInit = true;
                }
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }

    }
}
