using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.EcnEcoFollowUp.Interfaces.Models;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.Tools.EcnEcoFollowUp.Models
{
    public class EFU_DashboardItem : ObservableObject, IEFU_DashboardItem
    {
        #region [REGION] Properties from Interface
        private string _Name = string.Empty;
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

        private string _Id = string.Empty;
        public string Id
        {
            get { return this._Id; }
            set
            {
                if (this._Id != value)
                {
                    this._Id = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _CreatedBy = string.Empty;
        public string CreatedBy
        {
            get { return this._CreatedBy; }
            set
            {
                if (this._CreatedBy != value)
                {
                    this._CreatedBy = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _GeneralComment = string.Empty;
        public string GeneralComment
        {
            get { return _GeneralComment; }
            set
            {
                if (this._GeneralComment != value)
                {
                    this._GeneralComment = value;
                    OnPropertyChanged();
                    RaiseIsDashboardUpdateEvent();
                }

            }
        }

        private DateTime? _CreatedOn;
        public DateTime? CreatedOn
        {
            get { return this._CreatedOn; }
            set
            {
                if (this._CreatedOn != value)
                {
                    this._CreatedOn = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsShown;
        public bool IsShown
        {
            get { return this._IsShown; }
            set
            {
                if (this._IsShown != value)
                {
                    this._IsShown = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsActive;
        public bool IsActive
        {
            get { return this._IsActive; }
            set
            {
                if (this._IsActive != value)
                {
                    this._IsActive = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsCreator;
        public bool IsCreator
        {
            get { return this._IsCreator; }
            set
            {
                if (this._IsCreator != value)
                {
                    this._IsCreator = value;
                    OnPropertyChanged();
                }
                CheckUpdateAllowed();
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return this._IsSelected; }
            set
            {
                if (this._IsSelected != value)
                {
                    this._IsSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsShared = true;
        public bool IsShared
        {
            get { return _IsShared; }
            set
            {
                if (this._IsShared != value)
                {
                    this._IsShared = value;
                    OnPropertyChanged();
                    RaiseIsDashboardUpdateEvent();
                    //if (!value) IsReadOnly = false;
                }
            }
        }

        private bool _IsReadOnly = false;
        public bool IsReadOnly
        {
            get { return _IsReadOnly; }
            set
            {
                if (this._IsReadOnly != value)
                {
                    this._IsReadOnly = value;
                    OnPropertyChanged();
                    RaiseIsDashboardUpdateEvent();
                }
                CheckUpdateAllowed();
            }
        }

        private bool _UpdateAllowed;
        public bool UpdateAllowed
        {
            get { return _UpdateAllowed; }
            set
            {
                if (this._UpdateAllowed != value)
                {
                    this._UpdateAllowed = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<EFU_DashboardEcnEco> ListEcnEco { get; set; } = new ObservableCollection<EFU_DashboardEcnEco>();

        private EFU_DashboardEcnEco _SelectedEcnEco;
        public EFU_DashboardEcnEco SelectedEcnEco
        {
            get { return this._SelectedEcnEco; }
            set
            {
                if (this._SelectedEcnEco != value)
                {
                    this._SelectedEcnEco = value;
                    OnPropertyChanged();
                }
            }
        }

        public EcnEcoFollowUpViewModel ParentApp { get; set; }

        public ObservableCollection<string> ListPriority { get; set; } = new ObservableCollection<string>() { "None", "1", "2", "3" };

        private bool _IsAddDeletEcnEcoAllowed = true;
        public bool IsAddDeletEcnEcoAllowed
        {
            get { return this._IsAddDeletEcnEcoAllowed; }
            set
            {
                if (this._IsAddDeletEcnEcoAllowed != value)
                {
                    this._IsAddDeletEcnEcoAllowed = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsHideShowDashboardAllowed = true;
        public bool IsHideShowDashboardAllowed
        {
            get { return this._IsHideShowDashboardAllowed; }
            set
            {
                if (this._IsHideShowDashboardAllowed != value)
                {
                    this._IsHideShowDashboardAllowed = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsPersonalInfoShown = true;
        public bool IsPersonalInfoShown
        {
            get { return this._IsPersonalInfoShown; }
            set
            {
                if (this._IsPersonalInfoShown != value)
                {
                    this._IsPersonalInfoShown = value;
                    OnPropertyChanged();
                }
            }
        }

        private EFU_DashboardConfiguration _CurrentDashboardConfiguration = new EFU_DashboardConfiguration();
        public EFU_DashboardConfiguration CurrentDashboardConfiguration
        {
            get { return this._CurrentDashboardConfiguration; }
            set
            {
                if (this._CurrentDashboardConfiguration != value)
                {
                    this._CurrentDashboardConfiguration = value;
                    OnPropertyChanged();
                    this.CurrentDashboardConfiguration.RaiseIsUpdateEventEvent();
                    this.CurrentDashboardConfiguration.RaiseIsUpdateFilterEvent();
                }
            }
        }

        private bool _IsPersonalDashBoard = false;
        public bool IsPersonalDashBoard
        {
            get { return _IsPersonalDashBoard; }
            set
            {
                if (this._IsPersonalDashBoard != value)
                {
                    this._IsPersonalDashBoard = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Events
        public event EventHandler IsDashboardUpdateEvent;

        public void RaiseIsDashboardUpdateEvent()
        {
            try
            {
                IsDashboardUpdateEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Properties not from Interface
        public Ecnecodashboard EcnEcoDashboard { get; set; }
        public List<EFU_DashboardEcnEco> RowListEcnEco { get; set; } = new List<EFU_DashboardEcnEco>();
        public DateTime? DeactivatedOn { get; internal set; }
        #endregion

        #region [REGION] Misc
        public Ecnecodashboard GetEcnecodashboard()
        {
            return new Ecnecodashboard()
            {
                Dashboardid = 0,
                Createdbyfullname = CreatedBy,
                Isshown = IsShown.ToString(),
                Dashboardname = Name,
                Createdby = CreatedBy,
                Createdon = DateOnly.FromDateTime(CreatedOn ?? DateTime.MinValue),
                Generalcomment = GeneralComment,
                Isactive = IsActive,
                Isreadonly = IsReadOnly,
                Isshared = IsShared,
                Deactivatedon = DateOnly.FromDateTime(DeactivatedOn ?? DateTime.MinValue)
            };
        }

        private void CheckUpdateAllowed()
        {
            if (!IsReadOnly || IsCreator)
                UpdateAllowed = true;
            else
                UpdateAllowed = false;
        }
        #endregion

    }
}