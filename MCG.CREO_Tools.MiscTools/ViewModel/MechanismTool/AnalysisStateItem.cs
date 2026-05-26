using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool
{
    public class AnalysisStateItem: ObservableObject
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<AnalysisResultItem> AllResults { get; set; } = new ObservableCollection<AnalysisResultItem>();

        public override string ToString()
        {
            return Name;
        }
    }
}
