using MCG.CREO_Tools.QuickSearch.ViewModel;
using System.Windows;

namespace MCG.CREO_Tools.QuickSearch.View
{
    public partial class QuickSearchWindowClassSubClassFromNumberView : Window
    {
        public QuickSearchWindowClassSubClassFromNumberViewModel CurrentDataContext { get; private set; }

        public QuickSearchWindowClassSubClassFromNumberView(QuickSearchWindowClassSubClassFromNumberViewModel currentViewModel)
        {
            CurrentDataContext = currentViewModel;
            DataContext = currentViewModel;
            CurrentDataContext.CloseEvent += (s, e) =>
            {
                DialogResult = true;
                Close();
            };

            InitializeComponent();
        }

        public void SetProperties(List<string> listStdShown)
        {
            CurrentDataContext.SetProperties(listStdShown);
        }
    }
}
