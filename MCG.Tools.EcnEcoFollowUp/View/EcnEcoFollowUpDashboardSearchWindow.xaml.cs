using Fluent;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MCG.Tools.EcnEcoFollowUp.View
{
    public partial class EcnEcoFollowUpDashboardSearchWindow : RibbonWindow
    {
        public EcnEcoFollowUpDashboardSearchWindowViewModel CurrentEcnEcoFollowUpDashboardSearchWindowViewModel { get; set; }


        public EcnEcoFollowUpDashboardSearchWindow(EcnEcoFollowUpDashboardSearchWindowViewModel currentVm)
        {
            try
            {
                CurrentEcnEcoFollowUpDashboardSearchWindowViewModel = currentVm;
                CurrentEcnEcoFollowUpDashboardSearchWindowViewModel.ParentWindow = this;
                DataContext = CurrentEcnEcoFollowUpDashboardSearchWindowViewModel;
                InitializeComponent();
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void Int_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex SingleCharRegex = new Regex("[0-9]");
            e.Handled = !SingleCharRegex.IsMatch(e.Text);
        }
    }
}
