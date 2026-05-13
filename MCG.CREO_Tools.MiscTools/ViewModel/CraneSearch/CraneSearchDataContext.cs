using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.MiscTools.View.CraneSearch;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.CraneSearch
{
    public class CraneSearchDataContext : ObservableObject, ICraneSearchDataContext
    {
        public ObservableCollection<string> PartList { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<CraneSearchItem> CraneList { get; set; } = new ObservableCollection<CraneSearchItem>();

        public ObservableCollection<SapPlant> PlantList { get; set; } = new ObservableCollection<SapPlant>();

        private ObservableCollection<KeyValuePair<string, string>> _EuropeEquivalent;
        public ObservableCollection<KeyValuePair<string, string>> EuropeEquivalent
        {
            get { return _EuropeEquivalent; }
            set
            {
                if (this._EuropeEquivalent != value)
                {
                    this._EuropeEquivalent = value;
                    OnPropertyChanged();
                }

            }
        }

        private ObservableCollection<KeyValuePair<string, string>> _AsiaEquivalent;
        public ObservableCollection<KeyValuePair<string, string>> AsiaEquivalent
        {
            get { return _AsiaEquivalent; }
            set
            {
                if (this._AsiaEquivalent != value)
                {
                    this._AsiaEquivalent = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsStandAlone = false;
        public bool IsStandAlone
        {
            get { return _IsStandAlone; }
            set
            {
                if (this._IsStandAlone != value)
                {
                    this._IsStandAlone = value;
                    OnPropertyChanged();
                }

            }
        }
    }
}
