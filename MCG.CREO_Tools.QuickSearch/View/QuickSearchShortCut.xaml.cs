using MCG.CREO_Tools.QuickSearch.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MCG.CREO_Tools.QuickSearch.View
{
    public partial class QuickSearchShortCut : UserControl
    {
        public QuickSearchShortCutViewModel CurrentQuickSearchShortCutViewModel { get; set; }
        public static readonly DependencyProperty MainAppProperty =  DependencyProperty.Register("MainApp", typeof(QuickSearchViewModel), typeof(QuickSearchShortCut));


        public QuickSearchViewModel MainApp
        {
            get
            {
                return (QuickSearchViewModel)GetValue(MainAppProperty);
            }
            set
            {
                SetValue(MainAppProperty, value);
            }
        }

        public QuickSearchShortCut()
        {
            InitializeComponent();
            this.DataContext = CurrentQuickSearchShortCutViewModel;
        }
    }
}
