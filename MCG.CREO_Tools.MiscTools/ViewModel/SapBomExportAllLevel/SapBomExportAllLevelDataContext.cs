using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.MiscTools.View.SapBomExportAllLevel;
using MCG.WindchillRequestTool.Model.BomComparison;
using System.Collections.ObjectModel;


namespace MCG.CREO_Tools.MiscTools.ViewModel.SapBomExportAllLevel
{
    public class SapBomExportAllLevelDataContext : ObservableObject, ISapBomExportAllLevelDataContext
    {
        #region [REGION] Properties from Interface
        private string _PartNumber;
        public string PartNumber
        {
            get { return _PartNumber; }
            set
            {
                if (this._PartNumber != value)
                {
                    this._PartNumber = value?.ToUpper().Trim();
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<SapPlant> AllSapPlants { get; set; } = new ObservableCollection<SapPlant>();

        private SapPlant _Plant;
        public SapPlant Plant
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

        public ObservableCollection<SapBomUsage> AllBomUsage { get; set; } = new ObservableCollection<SapBomUsage>();

        private SapBomUsage _BomUsage;
        public SapBomUsage BomUsage
        {
            get { return _BomUsage; }
            set
            {
                if (this._BomUsage != value)
                {
                    this._BomUsage = value;
                    OnPropertyChanged();
                }

            }
        }

        private DateTime _DateValidity = DateTime.Today;
        public DateTime DateValidity
        {
            get { return _DateValidity; }
            set
            {
                if (this._DateValidity != value)
                {
                    this._DateValidity = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<int> SizeColumns { get; set; } = new ObservableCollection<int>();


        private int _MaxBomLevel = 1;
        public int MaxBomLevel
        {
            get { return _MaxBomLevel; }
            set
            {
                if (this._MaxBomLevel != value)
                {
                    this._MaxBomLevel = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<BomComponent> MainStructure { get; set; } = new ObservableCollection<BomComponent>();
        public ObservableCollection<BomComponent> AllComponents { get; set; } = new ObservableCollection<BomComponent>();
        public ObservableCollection<BomComponent> FlatStructure { get; set; } = new ObservableCollection<BomComponent>();

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

        private int _SapSearchIndex;
        public int SapSearchIndex
        {
            get { return _SapSearchIndex; }
            set
            {
                if (this._SapSearchIndex != value)
                {
                    this._SapSearchIndex = value;
                    OnPropertyChanged();
                }

            }
        }

        #endregion

        #region [REGION] Internal variables
        public string MainDescription { get; set; }
        #endregion

    }
}
