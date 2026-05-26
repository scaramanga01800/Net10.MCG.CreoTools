using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool
{
    public class AnalysisItem : ObservableObject
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

        public ObservableCollection<AnalysisMeasureItem> AllMeasures { get; set; } = new ObservableCollection<AnalysisMeasureItem>();

        public List<string> AllAnalysisFiles { get; set; } = new List<string>();

    }
}
