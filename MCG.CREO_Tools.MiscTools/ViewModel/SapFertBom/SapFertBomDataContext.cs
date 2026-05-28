using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.SapFertBom;
using MCG.WindchillRequestTool;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.SapFertBom
{
    public class SapFertBomDataContext : ObservableObject, ISapFertBomDataContext
    {
        private string _FertNumber;
        public string FertNumber
        {
            get { return _FertNumber; }
            set
            {
                if (this._FertNumber != value)
                {
                    this._FertNumber = value;
                    OnPropertyChanged();
                    RaiseFertNumberUpsateEvent();
                }
            }
        }

        public ObservableCollection<string> AllSapPlants { get; set; } = new ObservableCollection<string>();

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
                    RaisePlantChangeEvent();
                }

            }
        }

        private BomComparisonItem _BomComparison;
        public BomComparisonItem BomComparison
        {
            get { return _BomComparison; }
            set
            {
                if (this._BomComparison != value)
                {
                    this._BomComparison = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsActionProgress = false;
        public bool IsActionProgress
        {
            get { return _IsActionProgress; }
            set
            {
                if (this._IsActionProgress != value)
                {
                    this._IsActionProgress = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsPleaseWaitShown = false;
        public bool IsPleaseWaitShown
        {
            get { return _IsPleaseWaitShown; }
            set
            {
                if (this._IsPleaseWaitShown != value)
                {
                    this._IsPleaseWaitShown = value;
                    OnPropertyChanged();
                }

            }
        }

        public event EventHandler FertNumberUpsateEvent;

        public void RaiseFertNumberUpsateEvent()
        {
            try
            {
                FertNumberUpsateEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler PlantChangeEvent;

        public void RaisePlantChangeEvent()
        {
            try
            {
                PlantChangeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

    }
}
