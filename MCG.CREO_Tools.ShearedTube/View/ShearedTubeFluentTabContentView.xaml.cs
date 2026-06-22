using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MCG.CREO_Tools.ShearedTube.View
{
    public partial class ShearedTubeFluentTabContentView : UserControl
    {
        public ShearedTubeFluentTabContentView()
        {
            InitializeComponent();
        }

        #region [REGION] Misc
        private void Double_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex StartPointRegex = new Regex(@"^\.");
            if (StartPointRegex.IsMatch(((TextBox)sender).Text))
                ((TextBox)sender).Text = $"0{((TextBox)sender).Text}";

            Regex WholeTextRegex = new Regex(@"^$|^[0-9]+\.?$|^[0-9]+\.?[0-9]+$");
            Regex SingleCharRegex = new Regex("[0-9.]");
            e.Handled = !(WholeTextRegex.IsMatch(((TextBox)sender).Text) && SingleCharRegex.IsMatch(e.Text) && WholeTextRegex.IsMatch($"{((TextBox)sender).Text}{e.Text}"));
        }

        private void Double_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text == null || ((TextBox)sender).Text.Trim() == "")
                ((TextBox)sender).Text = "0";
        }
        #endregion
    }
}
