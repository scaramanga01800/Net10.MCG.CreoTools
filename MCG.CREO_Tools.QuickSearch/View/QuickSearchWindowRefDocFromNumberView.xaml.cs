using MCG.CREO_Tools.QuickSearch.ViewModel;
using System.Windows;

namespace MCG.CREO_Tools.QuickSearch.View
{
    public partial class QuickSearchWindowRefDocFromNumberView : Window
    {
        public QuickSearchWindowRefDocFromNumberView(QuickSearchWindowRefDocFromNumberViewModel currentViewModel)
        {
            InitializeComponent();
            DataContext = currentViewModel;
            currentViewModel.CloseEvent += (s, e) => this.Close();
        }
    }
}
