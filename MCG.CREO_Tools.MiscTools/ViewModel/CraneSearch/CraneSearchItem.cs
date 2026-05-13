using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.CREO_Tools.MiscTools.View.CraneSearch;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.CraneSearch
{
    public class CraneSearchItem: ObservableObject, ICraneSearchItem
    {
        private string _Plant;
        public string Plant
        {
            get { return _Plant; }
            set
            {
                if (this._Plant != value)
                {
                    this._Plant = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _PlantCrane;
        public string PlantCrane
        {
            get { return _PlantCrane; }
            set
            {
                if (this._PlantCrane != value)
                {
                    this._PlantCrane = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _CraneName;
        public string CraneName
        {
            get { return _CraneName; }
            set
            {
                if (this._CraneName != value)
                {
                    this._CraneName = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<SapGenericObject> PartList { get; set; }
    }
}
