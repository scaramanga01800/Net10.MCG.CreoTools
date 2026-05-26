using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool
{
    public class AnalysisAreaItem: ObservableObject 
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

        public ObservableCollection<AnalysisStateItem> AllStates { get; set; } = new ObservableCollection<AnalysisStateItem>();

        public override string ToString()
        {
            return Name;
        }
    }
}
