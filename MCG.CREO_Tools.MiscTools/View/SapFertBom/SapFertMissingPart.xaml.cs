using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.WindchillRequestTool.Model.BomComparison;
using System.Collections.ObjectModel;
using System.Windows;

namespace MCG.CREO_Tools.MiscTools.View.SapFertBom
{
    public partial class SapFertMissingPart : Window
    {
        private List<BomMissingComponentItem> _ListPart;

        public List<BomMissingComponentItem> ListPart
        {
            get { return _ListPart; }
            set { _ListPart = value; UpdateList(); }
        }

        public ObservableCollection<BomMissingComponentItem> ListPartShow { get; set; } = new ObservableCollection<BomMissingComponentItem>();

        public SapFertMissingPart()
        {
            try
            {

                InitializeComponent();
                DataContext = this;
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }

        }

        private void BtClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UpdateList()
        {
            try
            {
                if (ListPart != null)
                {
                    ListPartShow.Clear();
                    foreach (var item in ListPart)
                        ListPartShow.Add(item);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

    }
}
