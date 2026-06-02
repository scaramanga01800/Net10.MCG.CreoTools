using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.BomExport;
using MCG.CREO_Tools.MiscTools.ViewModel.BomExport;
using MCG.WindchillRequestTool.Model.Windchill;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.BomExport
{
    public class BomExportWindowDataContext : ObservableObject, IBomExportWindowDataContext
    {
        #region [REGION] Properties from Interface
        private bool _IsPartChecked = true;
        public bool IsPartChecked
        {
            get { return this._IsPartChecked; }
            set
            {
                if (this._IsPartChecked != value)
                {
                    this._IsPartChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsAssemblyChecked = false;
        public bool IsAssemblyChecked
        {
            get { return this._IsAssemblyChecked; }
            set
            {
                if (this._IsAssemblyChecked != value)
                {
                    this._IsAssemblyChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsLatestRevision = true;
        public bool IsLatestRevision
        {
            get { return this._IsLatestRevision; }
            set
            {
                if (this._IsLatestRevision != value)
                {
                    this._IsLatestRevision = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _ShowSapCostVolumeInfo;
        public bool ShowSapCostVolumeInfo
        {
            get { return this._ShowSapCostVolumeInfo; }
            set
            {
                if (this._ShowSapCostVolumeInfo != value)
                {
                    this._ShowSapCostVolumeInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsMsgSearchSap;
        public bool IsMsgSearchSap
        {
            get { return this._IsMsgSearchSap; }
            set
            {
                if (this._IsMsgSearchSap != value)
                {
                    this._IsMsgSearchSap = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsSearchBomDone = true;
        public bool IsSearchBomDone
        {
            get { return _IsSearchBomDone; }
            set
            {
                if (this._IsSearchBomDone != value)
                {
                    this._IsSearchBomDone = value;
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

        private string _Number;
        public string Number
        {
            get { return this._Number; }
            set
            {
                if (this._Number != value)
                {
                    this._Number = value.ToUpper().Trim();
                    OnPropertyChanged();
                }
            }
        }

        private string _Revision;
        public string Revision
        {
            get { return this._Revision; }
            set
            {
                if (this._Revision != value)
                {
                    this._Revision = value.ToUpper().Trim();
                    OnPropertyChanged();
                }
            }
        }

        private double _MainSapCost = 0;
        public double MainSapCost
        {
            get { return _MainSapCost; }
            set
            {
                if (this._MainSapCost != value)
                {
                    this._MainSapCost = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _MainSapProvider = "Unknown";
        public string MainSapProvider
        {
            get { return _MainSapProvider; }
            set
            {
                if (this._MainSapProvider != value)
                {
                    this._MainSapProvider = value;
                    OnPropertyChanged();
                }

            }
        }

        public List<int> BomLevelList { get; set; }
        private int _BomLevel;
        public int BomLevel
        {
            get { return this._BomLevel; }
            set
            {
                if (this._BomLevel != value)
                {
                    this._BomLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _MaxBomLevel;
        public int MaxBomLevel
        {
            get { return this._MaxBomLevel; }
            set
            {
                if (this._MaxBomLevel != value)
                {
                    this._MaxBomLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<BomExportParameter> ListAvailableParameters { get; set; } = new ObservableCollection<BomExportParameter>();
        public ObservableCollection<BomExportParameter> ListSelectedParameters { get; set; } = new ObservableCollection<BomExportParameter>();
        public ObservableCollection<BomExportParameter> ListAllParameters { get; set; } = new ObservableCollection<BomExportParameter>();
        public ObservableCollection<BomExportParameter> ListAllParametersAuthorized { get; set; } = new ObservableCollection<BomExportParameter>();

        public ObservableCollection<SapPlant> ListSapPlant { get; set; } = new ObservableCollection<SapPlant>();

        private SapPlant _SelectedSapPlant;
        public SapPlant SelectedSapPlant
        {
            get { return this._SelectedSapPlant; }
            set
            {
                if (this._SelectedSapPlant != value)
                {
                    this._SelectedSapPlant = value;
                    OnPropertyChanged();
                }
                RaiseSapPlantChangeEvent();
            }
        }

        private BomExportParameter _SelectedParameterAvailable;
        public BomExportParameter SelectedParameterAvailable
        {
            get { return this._SelectedParameterAvailable; }
            set
            {
                if (this._SelectedParameterAvailable != value)
                {
                    this._SelectedParameterAvailable = value;
                    OnPropertyChanged();
                }
            }
        }
        private BomExportParameter _SelectedParameter;
        public BomExportParameter SelectedParameter
        {
            get { return this._SelectedParameter; }
            set
            {
                if (this._SelectedParameter != value)
                {
                    this._SelectedParameter = value;
                    OnPropertyChanged();
                }
            }
        }

        private char _FieldSeparator;
        public char FieldSeparator
        {
            get { return this._FieldSeparator; }
            set
            {
                if (this._FieldSeparator != value)
                {
                    this._FieldSeparator = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<BomExportOutputFormat> ListOutputFormat { get; set; } = new ObservableCollection<BomExportOutputFormat>();

        private BomExportOutputFormat _SelectedOutputFormat;
        public BomExportOutputFormat SelectedOutputFormat
        {
            get { return this._SelectedOutputFormat; }
            set
            {
                if (this._SelectedOutputFormat != value)
                {
                    this._SelectedOutputFormat = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsNamingConvention = true;
        public bool IsNamingConvention
        {
            get { return this._IsNamingConvention; }
            set
            {
                if (this._IsNamingConvention != value)
                {
                    this._IsNamingConvention = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<WindchillObjStructureComponent> MainBom { get; set; } = new ObservableCollection<WindchillObjStructureComponent>();
        private WindchillObjStructureComponent _SelectedBomItem;
        public WindchillObjStructureComponent SelectedBomItem
        {
            get { return this._SelectedBomItem; }
            set
            {
                if (this._SelectedBomItem != value)
                {
                    this._SelectedBomItem = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<WindchillObjStructureComponent> AllComponents { get; set; } = new ObservableCollection<WindchillObjStructureComponent>();

        private WindchillObjStructureComponent _SelectedComponent;
        public WindchillObjStructureComponent SelectedComponent
        {
            get { return _SelectedComponent; }
            set
            {
                if (this._SelectedComponent != value)
                {
                    this._SelectedComponent = value;
                    OnPropertyChanged();
                }

            }
        }

        private BomExportParameter _BomColumnNumber;
        public BomExportParameter BomColumnNumber
        {
            get { return this._BomColumnNumber; }
            set
            {
                if (this._BomColumnNumber != value)
                {
                    this._BomColumnNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumnLevel;
        public BomExportParameter BomColumnLevel
        {
            get { return this._BomColumnLevel; }
            set
            {
                if (this._BomColumnLevel != value)
                {
                    this._BomColumnLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn1 = new BomExportParameter();
        public BomExportParameter BomColumn1
        {
            get { return this._BomColumn1; }
            set
            {
                if (this._BomColumn1 != value)
                {
                    this._BomColumn1 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn2 = new BomExportParameter();
        public BomExportParameter BomColumn2
        {
            get { return this._BomColumn2; }
            set
            {
                if (this._BomColumn2 != value)
                {
                    this._BomColumn2 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn3 = new BomExportParameter();
        public BomExportParameter BomColumn3
        {
            get { return this._BomColumn3; }
            set
            {
                if (this._BomColumn3 != value)
                {
                    this._BomColumn3 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn4 = new BomExportParameter();
        public BomExportParameter BomColumn4
        {
            get { return this._BomColumn4; }
            set
            {
                if (this._BomColumn4 != value)
                {
                    this._BomColumn4 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn5 = new BomExportParameter();
        public BomExportParameter BomColumn5
        {
            get { return this._BomColumn5; }
            set
            {
                if (this._BomColumn5 != value)
                {
                    this._BomColumn5 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn6 = new BomExportParameter();
        public BomExportParameter BomColumn6
        {
            get { return this._BomColumn6; }
            set
            {
                if (this._BomColumn6 != value)
                {
                    this._BomColumn6 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn7 = new BomExportParameter();
        public BomExportParameter BomColumn7
        {
            get { return this._BomColumn7; }
            set
            {
                if (this._BomColumn7 != value)
                {
                    this._BomColumn7 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn8 = new BomExportParameter();
        public BomExportParameter BomColumn8
        {
            get { return this._BomColumn8; }
            set
            {
                if (this._BomColumn8 != value)
                {
                    this._BomColumn8 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn9 = new BomExportParameter();
        public BomExportParameter BomColumn9
        {
            get { return this._BomColumn9; }
            set
            {
                if (this._BomColumn9 != value)
                {
                    this._BomColumn9 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn10 = new BomExportParameter();
        public BomExportParameter BomColumn10
        {
            get { return this._BomColumn10; }
            set
            {
                if (this._BomColumn10 != value)
                {
                    this._BomColumn10 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn11 = new BomExportParameter();
        public BomExportParameter BomColumn11
        {
            get { return this._BomColumn11; }
            set
            {
                if (this._BomColumn11 != value)
                {
                    this._BomColumn11 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn12 = new BomExportParameter();
        public BomExportParameter BomColumn12
        {
            get { return this._BomColumn12; }
            set
            {
                if (this._BomColumn12 != value)
                {
                    this._BomColumn12 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn13 = new BomExportParameter();
        public BomExportParameter BomColumn13
        {
            get { return this._BomColumn13; }
            set
            {
                if (this._BomColumn13 != value)
                {
                    this._BomColumn13 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn14 = new BomExportParameter();
        public BomExportParameter BomColumn14
        {
            get { return this._BomColumn14; }
            set
            {
                if (this._BomColumn14 != value)
                {
                    this._BomColumn14 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn15 = new BomExportParameter();
        public BomExportParameter BomColumn15
        {
            get { return this._BomColumn15; }
            set
            {
                if (this._BomColumn15 != value)
                {
                    this._BomColumn15 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn16 = new BomExportParameter();
        public BomExportParameter BomColumn16
        {
            get { return this._BomColumn16; }
            set
            {
                if (this._BomColumn16 != value)
                {
                    this._BomColumn16 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn17 = new BomExportParameter();
        public BomExportParameter BomColumn17
        {
            get { return this._BomColumn17; }
            set
            {
                if (this._BomColumn17 != value)
                {
                    this._BomColumn17 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn18 = new BomExportParameter();
        public BomExportParameter BomColumn18
        {
            get { return this._BomColumn18; }
            set
            {
                if (this._BomColumn18 != value)
                {
                    this._BomColumn18 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn19 = new BomExportParameter();
        public BomExportParameter BomColumn19
        {
            get { return this._BomColumn19; }
            set
            {
                if (this._BomColumn19 != value)
                {
                    this._BomColumn19 = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomExportParameter _BomColumn20 = new BomExportParameter();
        public BomExportParameter BomColumn20
        {
            get { return this._BomColumn20; }
            set
            {
                if (this._BomColumn20 != value)
                {
                    this._BomColumn20 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _StatusBarMsg;
        public string StatusBarMsg
        {
            get { return this._StatusBarMsg; }
            set
            {
                if (this._StatusBarMsg != value)
                {
                    this._StatusBarMsg = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsShowOccurrences = false;
        public bool IsShowOccurrences
        {
            get { return this._IsShowOccurrences; }
            set
            {
                if (this._IsShowOccurrences != value)
                {
                    this._IsShowOccurrences = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsLevelIndented = false;
        public bool IsLevelIndented
        {
            get { return _IsLevelIndented; }
            set
            {
                if (this._IsLevelIndented != value)
                {
                    this._IsLevelIndented = value;
                    OnPropertyChanged();
                }

            }
        }


        public ObservableCollection<int> NumericalLineNumberDigitList { get; set; } = new ObservableCollection<int>();

        private int _NumericalLineNumberDigit = 4;
        public int NumericalLineNumberDigit
        {
            get { return _NumericalLineNumberDigit; }
            set
            {
                if (this._NumericalLineNumberDigit != value)
                {
                    this._NumericalLineNumberDigit = value;
                    OnPropertyChanged();
                    RaiseNumericalLineNumberDigitEvent();
                }
            }
        }

        public ObservableCollection<BomExportClassificationItem> ClassificationItemList { get; set; } = new ObservableCollection<BomExportClassificationItem>();

        private bool _IsColNameShown;
        public bool IsColNameShown
        {
            get { return _IsColNameShown; }
            set
            {
                if (this._IsColNameShown != value)
                {
                    this._IsColNameShown = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsColMaterialShown;
        public bool IsColMaterialShown
        {
            get { return _IsColMaterialShown; }
            set
            {
                if (this._IsColMaterialShown != value)
                {
                    this._IsColMaterialShown = value;
                    OnPropertyChanged();
                }

            }
        }

        private double _CumulativeEndItemMass;
        public double CumulativeEndItemMass
        {
            get { return _CumulativeEndItemMass; }
            set
            {
                if (this._CumulativeEndItemMass != value)
                {
                    this._CumulativeEndItemMass = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsCreateZip = false;
        public bool IsCreateZip
        {
            get { return _IsCreateZip; }
            set
            {
                if (this._IsCreateZip != value)
                {
                    this._IsCreateZip = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsStateInWork = true;
        public bool IsStateInWork
        {
            get { return _IsStateInWork; }
            set
            {
                if (this._IsStateInWork != value)
                {
                    this._IsStateInWork = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsStateUnderReview = true;
        public bool IsStateUnderReview
        {
            get { return _IsStateUnderReview; }
            set
            {
                if (this._IsStateUnderReview != value)
                {
                    this._IsStateUnderReview = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsStatePreReleased = false;
        public bool IsStatePreReleased
        {
            get { return _IsStatePreReleased; }
            set
            {
                if (this._IsStatePreReleased != value)
                {
                    this._IsStatePreReleased = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsStatePrototype = false;
        public bool IsStatePrototype
        {
            get { return _IsStatePrototype; }
            set
            {
                if (this._IsStatePrototype != value)
                {
                    this._IsStatePrototype = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsStateReleased = false;
        public bool IsStateReleased
        {
            get { return _IsStateReleased; }
            set
            {
                if (this._IsStateReleased != value)
                {
                    this._IsStateReleased = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsStateObsolete = false;
        public bool IsStateObsolete
        {
            get { return _IsStateObsolete; }
            set
            {
                if (this._IsStateObsolete != value)
                {
                    this._IsStateObsolete = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsStateSuperseded = false;
        public bool IsStateSuperseded
        {
            get { return _IsStateSuperseded; }
            set
            {
                if (this._IsStateSuperseded != value)
                {
                    this._IsStateSuperseded = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsStateRework = true;
        public bool IsStateRework
        {
            get { return _IsStateRework; }
            set
            {
                if (this._IsStateRework != value)
                {
                    this._IsStateRework = value;
                    OnPropertyChanged();
                }

            }
        }

        #endregion

        #region [REGION] Event
        public event EventHandler SapPlantChangeEvent;
        public void RaiseSapPlantChangeEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                SapPlantChangeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler NumericalLineNumberDigitEvent;
        public void RaiseNumericalLineNumberDigitEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                NumericalLineNumberDigitEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }


        public event EventHandler IsParameterUpdateEvent;
        public void RaiseIsParameterUpdateEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                IsParameterUpdateEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        public BomExportWindowDataContext()
        {
        }

        public void SubscribeListAllParametersEvents()
        {
            try
            {
                if (ListAllParameters != null)
                    foreach (var param in ListAllParameters)
                    {
                        param.IsSelectedParameterEvent -= RaiseIsParameterUpdateEvent;
                        param.IsSelectedParameterEvent += RaiseIsParameterUpdateEvent;
                    }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);

            }

        }
    }
}
