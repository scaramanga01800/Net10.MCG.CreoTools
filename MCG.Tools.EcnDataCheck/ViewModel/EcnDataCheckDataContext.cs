using MCG.Tools.EcnDataCheck.View;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.Tools.EcnDataCheck.Models;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Models.SAP;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;

namespace MCG.Tools.EcnDataCheck.ViewModel
{
    class EcnDataCheckDataContext : ObservableObject, IEcnDataCheckDataContext
    {
        #region [REGION] Properties from Interface
        private string _EcnNumber;
        public string EcnNumber
        {
            get { return this._EcnNumber; }
            set
            {
                if (this._EcnNumber != value)
                {
                    this._EcnNumber = value.Trim().ToUpper();
                    OnPropertyChanged();
                }
            }
        }

        public WindchillChangeNotice CurrentWindchillChangeNotice { get; set; }

        private WindchillChangeActivity _EcaNumber;
        public WindchillChangeActivity EcaNumber
        {
            get { return this._EcaNumber; }
            set
            {
                if (this._EcaNumber != value)
                {
                    this._EcaNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<WindchillChangeActivity> EcaList { get; set; } = new ObservableCollection<WindchillChangeActivity>();

        private DataCheckStatus _GlobalStatus = DataCheckStatus.NONE;
        public DataCheckStatus GlobalStatus
        {
            get { return this._GlobalStatus; }
            set
            {
                if (this._GlobalStatus != value)
                {
                    this._GlobalStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _EcnDataCheckInProgress = false;
        public bool EcnDataCheckInProgress
        {
            get { return this._EcnDataCheckInProgress; }
            set
            {
                if (this._EcnDataCheckInProgress != value)
                {
                    this._EcnDataCheckInProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _TotalStep = 10;
        public int TotalStep
        {
            get { return this._TotalStep; }
            set
            {
                if (this._TotalStep != value)
                {
                    this._TotalStep = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _CurrentStep = 5;
        public int CurrentStep
        {
            get { return this._CurrentStep; }
            set
            {
                if (this._CurrentStep != value)
                {
                    this._CurrentStep = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _ShowActionButton = true;
        public bool ShowActionButton
        {
            get { return this._ShowActionButton; }
            set
            {
                if (this._ShowActionButton != value)
                {
                    this._ShowActionButton = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _ShowRenameTab = false;
        public bool ShowRenameTab
        {
            get { return this._ShowRenameTab; }
            set
            {
                if (this._ShowRenameTab != value)
                {
                    this._ShowRenameTab = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _ShowMoveTab = false;
        public bool ShowMoveTab
        {
            get { return this._ShowMoveTab; }
            set
            {
                if (this._ShowMoveTab != value)
                {
                    this._ShowMoveTab = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<MCGLanguage> ListLanguage { get; set; } = new ObservableCollection<MCGLanguage>();
        private MCGLanguage _SelectedLanguage;
        public MCGLanguage SelectedLanguage
        {
            get { return this._SelectedLanguage; }
            set
            {
                if (this._SelectedLanguage != value)
                {
                    this._SelectedLanguage = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ErpSystem;
        public string ErpSystem
        {
            get { return this._ErpSystem; }
            set
            {
                if (this._ErpSystem != value)
                {
                    this._ErpSystem = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<string> ErpList { get; set; } = new ObservableCollection<string>();

        private IEcnDataCheckItem _SelectedDataCheckItem;
        public IEcnDataCheckItem SelectedDataCheckItem
        {
            get { return this._SelectedDataCheckItem; }
            set
            {
                if (this._SelectedDataCheckItem != value)
                {
                    this._SelectedDataCheckItem = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<IEcnDataCheckItem> DataCheckItemList { get; set; } = new ObservableCollection<IEcnDataCheckItem>();

        private IEcnDataCheckResultItem _SelectedDataCheckResultItem;
        public IEcnDataCheckResultItem SelectedDataCheckResultItem
        {
            get { return this._SelectedDataCheckResultItem; }
            set
            {
                if (this._SelectedDataCheckResultItem != value)
                {
                    this._SelectedDataCheckResultItem = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<IEcnDataCheckResultItem> DataCheckResultItemList { get; set; } = new ObservableCollection<IEcnDataCheckResultItem>();

        // Properties for the Move Tab
        private string _SelectedLocation;
        public string SelectedLocation
        {
            get { return this._SelectedLocation; }
            set
            {
                if (this._SelectedLocation != value)
                {
                    this._SelectedLocation = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<string> ListLocation { get; set; } = new ObservableCollection<string>();

        private bool _IsCheckBoxProductSelected;
        public bool IsCheckBoxProductSelected
        {
            get { return this._IsCheckBoxProductSelected; }
            set
            {
                if (this._IsCheckBoxProductSelected != value)
                {
                    this._IsCheckBoxProductSelected = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _IsCheckBoxLibraySelected;
        public bool IsCheckBoxLibraySelected
        {
            get { return this._IsCheckBoxLibraySelected; }
            set
            {
                if (this._IsCheckBoxLibraySelected != value)
                {
                    this._IsCheckBoxLibraySelected = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _ContextFilter;
        public string ContextFilter
        {
            get { return this._ContextFilter; }
            set
            {
                if (this._ContextFilter != value)
                {
                    this._ContextFilter = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<IEcnDataCheckItem> MoveItemList { get; set; } = new ObservableCollection<IEcnDataCheckItem>();
        private IEcnDataCheckItem _SelectedMoveItem;
        public IEcnDataCheckItem SelectedMoveItem
        {
            get { return this._SelectedMoveItem; }
            set
            {
                if (this._SelectedMoveItem != value)
                {
                    this._SelectedMoveItem = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<WindchillContext> WindchillContextList { get; set; } = new ObservableCollection<WindchillContext>();

        private WindchillContext _SelectedContext;
        public WindchillContext SelectedContext
        {
            get { return _SelectedContext; }
            set
            {
                if (this._SelectedContext != value)
                {
                    this._SelectedContext = value;
                    OnPropertyChanged();
                }

            }
        }

        // Properties for the Rename tab
        public ObservableCollection<string> WebTermList { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<IEcnDataCheckItem> RenameItemList { get; set; } = new ObservableCollection<IEcnDataCheckItem>();
        private IEcnDataCheckItem _SelectedRenameItem;
        public IEcnDataCheckItem SelectedRenameItem
        {
            get { return this._SelectedRenameItem; }
            set
            {
                if (this._SelectedRenameItem != value)
                {
                    this._SelectedRenameItem = value;
                    OnPropertyChanged();
                }
            }
        }

        // Property for the Status Bar
        private string _ExtraStatusBarMsg = "-";
        public string ExtraStatusBarMsg
        {
            get { return this._ExtraStatusBarMsg; }
            set
            {
                if (this._ExtraStatusBarMsg != value)
                {
                    this._ExtraStatusBarMsg = value;
                    OnPropertyChanged();
                }
            }
        }

        // Properties SAP Menu
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
            }
        }
        public ObservableCollection<SapPlant> ListSapPlant { get; set; } = new ObservableCollection<SapPlant>();

        private int _NumericalLineNumberDigit;
        public int NumericalLineNumberDigit
        {
            get { return this._NumericalLineNumberDigit; }
            set
            {
                if (this._NumericalLineNumberDigit != value)
                {
                    this._NumericalLineNumberDigit = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<int> NumericalLineNumberDigitList { get; set; } = new ObservableCollection<int>();

        // Properties for StatusBar

        private string _StatusBarMsg1;
        public string StatusBarMsg1
        {
            get { return this._StatusBarMsg1; }
            set
            {
                if (this._StatusBarMsg1 != value)
                {
                    this._StatusBarMsg1 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _StatusBarMsg2;
        public string StatusBarMsg2
        {
            get { return this._StatusBarMsg2; }
            set
            {
                if (this._StatusBarMsg2 != value)
                {
                    this._StatusBarMsg2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<SapGenericObject> SapCraneList { get; set; } = new ObservableCollection<SapGenericObject>();
        #endregion

        #region [REGION] Events
        public event EventHandler EcnPartExtractedEvent;
        public void RaiseEcnPartExtractedEvent()
        {
            try
            {
                EcnPartExtractedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler EcnPdmBomExtractedEvent;
        public void RaiseEcnPdmBomExtractedEvent()
        {
            try
            {
                EcnPdmBomExtractedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler EcnSapBomExtractedEvent;
        public void RaiseEcnSapBomExtractedEvent()
        {
            try
            {
                EcnSapBomExtractedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion


        #region [REGION] Properties not from Intrerface
        public List<WindchillContext> AllWindchillContextList { get; set; } = new List<WindchillContext>();
        public List<Webterm> AllWebtermList { get; set; } = new List<Webterm>();
        public List<EcnDataCheckItem> MissingWtPartInEcnList { get; set; } = new List<EcnDataCheckItem>();

        public List<EcnDataCheckItem> OtherCheckItemList { get; set; } = new List<EcnDataCheckItem>();

        public bool IsSapBomExtracted { get; set; } = false;
        public bool IsPdmBomExtracted { get; set; } = false;
        #endregion


    }
}
