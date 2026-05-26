using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool
{
    public class AnalysisMeasureItem: ObservableObject
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

        private string _AxisPosition;
        public string AxisPosition
        {
            get { return _AxisPosition; }
            set
            {
                if (this._AxisPosition != value)
                {
                    this._AxisPosition = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<AnalysisAreaItem> AllAreas { get; set; } = new ObservableCollection<AnalysisAreaItem>();

        public override string ToString()
        {
            return Name;
        }
    }
}
