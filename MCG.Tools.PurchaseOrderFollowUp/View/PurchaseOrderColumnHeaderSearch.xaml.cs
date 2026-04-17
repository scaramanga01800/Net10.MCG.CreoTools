using MCG.CommonLib.WpfComponent.ViewModel.AttributeColumn;
using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System.Windows.Controls;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    /// <summary>
    /// Logique d'interaction pour PurchaseOrderColumnHeaderSearch.xaml
    /// </summary>
    public partial class PurchaseOrderColumnHeaderSearch : UserControl
    {

        public PurchaseOrderColumnHeaderSearchViewModel CurrentDataContext { get; set; }

        public PurchaseOrderColumnHeaderSearch()
        {
            CurrentDataContext= new PurchaseOrderColumnHeaderSearchViewModel();
            DataContext = CurrentDataContext;
            InitializeComponent();
        }

        public PurchaseOrderColumnHeaderSearch(string AttributeName, object DataContextCommand, McgColumnData CurrentCommandParameter)
        {
            CurrentDataContext = new PurchaseOrderColumnHeaderSearchViewModel()
            {
                AttributeName = AttributeName,
                DataContextCommand = DataContextCommand,
                CurrentCommandParameter = CurrentCommandParameter
            };
            DataContext = CurrentDataContext;
            InitializeComponent();
        }
    }
}
