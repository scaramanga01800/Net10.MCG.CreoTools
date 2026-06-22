using MCG.CREO_Tools.QuickSearch.Exceptions;
using MCG.CREO_Tools.QuickSearch.ViewModel;
using System.Windows.Controls;

namespace MCG.CREO_Tools.QuickSearch.View
{
    public partial class QuickSearchColumnHeaderSearch : UserControl
    {
        public QuickSearchColumnHeaderSearchViewModel CurrentDataContext { get; set; }
        
        public QuickSearchColumnHeaderSearch()
        {
            try
            {
                CurrentDataContext = new QuickSearchColumnHeaderSearchViewModel();
                DataContext = CurrentDataContext;
                InitializeComponent();
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void SetProperties(string AttributeName, object DataContextCommand, int MinWidth, QuickSearchPartSubClassParam RefObject)
        {
            CurrentDataContext.AttributeName = AttributeName;
            CurrentDataContext.DataContextCommand = DataContextCommand;
            CurrentDataContext.MinWidth = MinWidth;
            CurrentDataContext.RefObject = RefObject;
        }
    }
}
