using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.MiscTools.View.SapBomExport;
using MCG.WindchillRequestTool.Model.BomComparison;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.SapBomExport
{
    public class SapBomExportDataContext:ObservableObject, ISapBomExportDataContext
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
                    this._PartNumber = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _EcoNumber;
        public string EcoNumber
        {
            get { return _EcoNumber; }
            set
            {
                if (this._EcoNumber != value)
                {
                    this._EcoNumber = value;
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

        public ObservableCollection<string> AllAlternativeBom { get; set; } = new ObservableCollection<string>();

        private string _AlternativeBom;
        public string  AlternativeBom
        {
            get { return _AlternativeBom; }
            set
            {
                if (this._AlternativeBom != value)
                {
                    this._AlternativeBom = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<SapBomExportApplicationItem> AllBomApplication { get; set; } = new ObservableCollection<SapBomExportApplicationItem>();

        private SapBomExportApplicationItem _BomApplication;
        public SapBomExportApplicationItem BomApplication
        {
            get { return _BomApplication; }
            set
            {
                if (this._BomApplication != value)
                {
                    this._BomApplication = value;
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

        private DateTime _DateValidityCost = DateTime.Today;
        public DateTime DateValidityCost
        {
            get { return _DateValidityCost; }
            set
            {
                if (this._DateValidityCost != value)
                {
                    this._DateValidityCost = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> AllRevision { get; set; } = new ObservableCollection<string>();

        private string _Revision = "BLANK";
        public string Revision
        {
            get { return _Revision; }
            set
            {
                if (this._Revision != value)
                {
                    this._Revision = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _Is_CB_RLT_Selected;
        public bool Is_CB_RLT_Selected
        {
            get { return _Is_CB_RLT_Selected; }
            set
            {
                if (this._Is_CB_RLT_Selected != value)
                {
                    this._Is_CB_RLT_Selected = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _Is_CB_PUR_Selected;
        public bool Is_CB_PUR_Selected
        {
            get { return _Is_CB_PUR_Selected; }
            set
            {
                if (this._Is_CB_PUR_Selected != value)
                {
                    this._Is_CB_PUR_Selected = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _Is_RB_SC_Selected;
        public bool Is_RB_SC_Selected
        {
            get { return _Is_RB_SC_Selected; }
            set
            {
                if (this._Is_RB_SC_Selected != value)
                {
                    this._Is_RB_SC_Selected = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _Is_RB_RT_Selected;
        public bool Is_RB_RT_Selected
        {
            get { return _Is_RB_RT_Selected; }
            set
            {
                if (this._Is_RB_RT_Selected != value)
                {
                    this._Is_RB_RT_Selected = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _Is_RB_ALL_Selected = true;
        public bool Is_RB_ALL_Selected
        {
            get { return _Is_RB_ALL_Selected; }
            set
            {
                if (this._Is_RB_ALL_Selected != value)
                {
                    this._Is_RB_ALL_Selected = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _Is_RB_MRT_Selected;
        public bool Is_RB_MRT_Selected
        {
            get { return _Is_RB_MRT_Selected; }
            set
            {
                if (this._Is_RB_MRT_Selected != value)
                {
                    this._Is_RB_MRT_Selected = value;
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
        #endregion

        #region [REGION] Internal variables
        public string MainDescription { get; set; }
        #endregion

    }
}
