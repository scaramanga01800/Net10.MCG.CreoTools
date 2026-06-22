using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.SapTools.ViewModel;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
    public class QuickSearchPartSubClass : ObservableObject
    {
        private Partsubclass _CurrentPartSubClass;
        public Partsubclass CurrentPartSubClass
        {
            get { return this._CurrentPartSubClass; }
            set
            {
                if (this._CurrentPartSubClass != value)
                {
                    this._CurrentPartSubClass = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<QuickSearchPartSubClassParam> AllPartSubClassParam { get; set; }

        public List<QuickSearchPartSubClassParam> ShownPartSubClassParam { get; set; }

        private string _Name;
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSapSearchDone { get; set; } = false;
        public List<SapCostVolumeInfo> CurrentAllCostVolume { get; set; } = null;

        public override string ToString()
        {
            return Name;
        }
    }
}
