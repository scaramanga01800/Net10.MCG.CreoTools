using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.View;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MCG.Tools.EcnEcoFollowUp.ViewModel
{
    public class EcnEcoFollowUpDataContext : ObservableObject, IEcnEcoFollowUpDataContext
    {
        #region [REGION] Properties from Interface
        private string _EcnNumber = string.Empty;
        public string EcnNumber
        {
            get { return this._EcnNumber; }
            set
            {
                if (this._EcnNumber != value)
                {
                    this._EcnNumber = value;
                    OnPropertyChanged();
                    IsOtherFieldEnable = !(EcnNumber != null && EcnNumber.Trim() != "");
                }
            }
        }

        private bool _IsOtherFieldEnable = true;
        public bool IsOtherFieldEnable
        {
            get { return this._IsOtherFieldEnable; }
            set
            {
                if (this._IsOtherFieldEnable != value)
                {
                    this._IsOtherFieldEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _CreatedAfter = null;
        public DateTime? CreatedAfter
        {
            get { return this._CreatedAfter; }
            set
            {
                if (this._CreatedAfter != value)
                {
                    this._CreatedAfter = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _CreatedBefore = null;
        public DateTime? CreatedBefore
        {
            get { return this._CreatedBefore; }
            set
            {
                if (this._CreatedBefore != value)
                {
                    this._CreatedBefore = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _ResolvedAfter = null;
        public DateTime? ResolvedAfter
        {
            get { return this._ResolvedAfter; }
            set
            {
                if (this._ResolvedAfter != value)
                {
                    this._ResolvedAfter = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _ResolvedBefore = null;
        public DateTime? ResolvedBefore
        {
            get { return this._ResolvedBefore; }
            set
            {
                if (this._ResolvedBefore != value)
                {
                    this._ResolvedBefore = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> PdmProductList { get; set; } = new ObservableCollection<string>();
        private string _PdmProduct = string.Empty;
        public string PdmProduct
        {
            get { return this._PdmProduct; }
            set
            {
                if (this._PdmProduct != value)
                {
                    this._PdmProduct = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> EcnStateList { get; set; } = new ObservableCollection<string>();
        private string _EcnState = string.Empty;
        public string EcnState
        {
            get { return this._EcnState; }
            set
            {
                if (this._EcnState != value)
                {
                    this._EcnState = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _KeyWords = string.Empty;
        public string KeyWords
        {
            get { return this._KeyWords; }
            set
            {
                if (this._KeyWords != value)
                {
                    this._KeyWords = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _EcnCreator = string.Empty;
        public string EcnCreator
        {
            get { return this._EcnCreator; }
            set
            {
                if (this._EcnCreator != value)
                {
                    this._EcnCreator = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _CreatedAfterSap;
        public DateTime? CreatedAfterSap
        {
            get { return this._CreatedAfterSap; }
            set
            {
                if (this._CreatedAfterSap != value)
                {
                    this._CreatedAfterSap = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _CreatedBeforeSap;
        public DateTime? CreatedBeforeSap
        {
            get { return this._CreatedBeforeSap; }
            set
            {
                if (this._CreatedBeforeSap != value)
                {
                    this._CreatedBeforeSap = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _KeyWordsProject = string.Empty;
        public string KeyWordsProject
        {
            get { return this._KeyWordsProject; }
            set
            {
                if (this._KeyWordsProject != value)
                {
                    this._KeyWordsProject = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _KeyWordsCategory = string.Empty;
        public string KeyWordsCategory
        {
            get { return this._KeyWordsCategory; }
            set
            {
                if (this._KeyWordsCategory != value)
                {
                    this._KeyWordsCategory = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsStatusNotCreated = true;
        public bool IsStatusNotCreated
        {
            get { return this._IsStatusNotCreated; }
            set
            {
                if (this._IsStatusNotCreated != value)
                {
                    this._IsStatusNotCreated = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsStatus99 = true;
        public bool IsStatus99
        {
            get { return this._IsStatus99; }
            set
            {
                if (this._IsStatus99 != value)
                {
                    this._IsStatus99 = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsStatus01 = true;
        public bool IsStatus01
        {
            get { return this._IsStatus01; }
            set
            {
                if (this._IsStatus01 != value)
                {
                    this._IsStatus01 = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsStatus02 = true;
        public bool IsStatus02
        {
            get { return this._IsStatus02; }
            set
            {
                if (this._IsStatus02 != value)
                {
                    this._IsStatus02 = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsStatus03 = true;
        public bool IsStatus03
        {
            get { return this._IsStatus03; }
            set
            {
                if (this._IsStatus03 != value)
                {
                    this._IsStatus03 = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbParts;
        public int NbParts
        {
            get { return this._NbParts; }
            set
            {
                if (this._NbParts != value)
                {
                    this._NbParts = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbPartsPdmApproved;
        public int NbPartsPdmApproved
        {
            get { return this._NbPartsPdmApproved; }
            set
            {
                if (this._NbPartsPdmApproved != value)
                {
                    this._NbPartsPdmApproved = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbPartsSapApproved;
        public int NbPartsSapApproved
        {
            get { return this._NbPartsSapApproved; }
            set
            {
                if (this._NbPartsSapApproved != value)
                {
                    this._NbPartsSapApproved = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbDrawings;
        public int NbDrawings
        {
            get { return this._NbDrawings; }
            set
            {
                if (this._NbDrawings != value)
                {
                    this._NbDrawings = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbDrawingsPdmApproved;
        public int NbDrawingsPdmApproved
        {
            get { return this._NbDrawingsPdmApproved; }
            set
            {
                if (this._NbDrawingsPdmApproved != value)
                {
                    this._NbDrawingsPdmApproved = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbDrawingsSapApproved;
        public int NbDrawingsSapApproved
        {
            get { return this._NbDrawingsSapApproved; }
            set
            {
                if (this._NbDrawingsSapApproved != value)
                {
                    this._NbDrawingsSapApproved = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbEpmDoc;
        public int NbEpmDoc
        {
            get { return this._NbEpmDoc; }
            set
            {
                if (this._NbEpmDoc != value)
                {
                    this._NbEpmDoc = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbEpmDocPdmApproved;
        public int NbEpmDocPdmApproved
        {
            get { return this._NbEpmDocPdmApproved; }
            set
            {
                if (this._NbEpmDocPdmApproved != value)
                {
                    this._NbEpmDocPdmApproved = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbEpmDocSapApproved;
        public int NbEpmDocSapApproved
        {
            get { return this._NbEpmDocSapApproved; }
            set
            {
                if (this._NbEpmDocSapApproved != value)
                {
                    this._NbEpmDocSapApproved = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbWtDoc;
        public int NbWtDoc
        {
            get { return this._NbWtDoc; }
            set
            {
                if (this._NbWtDoc != value)
                {
                    this._NbWtDoc = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbWtDocPdmApproved;
        public int NbWtDocPdmApproved
        {
            get { return this._NbWtDocPdmApproved; }
            set
            {
                if (this._NbWtDocPdmApproved != value)
                {
                    this._NbWtDocPdmApproved = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbWtDocSapApproved;
        public int NbWtDocSapApproved
        {
            get { return this._NbWtDocSapApproved; }
            set
            {
                if (this._NbWtDocSapApproved != value)
                {
                    this._NbWtDocSapApproved = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbEcn;
        public int NbEcn
        {
            get { return this._NbEcn; }
            set
            {
                if (this._NbEcn != value)
                {
                    this._NbEcn = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbEcnPdmApproved;
        public int NbEcnPdmApproved
        {
            get { return this._NbEcnPdmApproved; }
            set
            {
                if (this._NbEcnPdmApproved != value)
                {
                    this._NbEcnPdmApproved = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbEcnSapApproved;
        public int NbEcnSapApproved
        {
            get { return this._NbEcnSapApproved; }
            set
            {
                if (this._NbEcnSapApproved != value)
                {
                    this._NbEcnSapApproved = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _StatusBarText = string.Empty;
        public string StatusBarText
        {
            get { return this._StatusBarText; }
            set
            {
                if (this._StatusBarText != value)
                {
                    this._StatusBarText = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<EFU_EcnEcoToShowEndUser> EcnShownList { get; set; } = new ObservableCollection<EFU_EcnEcoToShowEndUser>();

        private EFU_EcnEcoToShowEndUser _SelectedEcn;
        public EFU_EcnEcoToShowEndUser SelectedEcn
        {
            get { return this._SelectedEcn; }
            set
            {
                if (this._SelectedEcn != value)
                {
                    this._SelectedEcn = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<EFU_SearchTemplate> SavedSearchesList { get; set; } = new List<EFU_SearchTemplate>();
        public List<EFU_SearchTemplate> RecentSearchesList { get; set; } = new List<EFU_SearchTemplate>();

        private EcnEcoFollowUpDashboardView _PersonalDashboard;
        public EcnEcoFollowUpDashboardView PersonalDashboard
        {
            get { return this._PersonalDashboard; }
            set
            {
                if (this._PersonalDashboard != value)
                {
                    this._PersonalDashboard = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<EcnEcoFollowUpDashboardViewModel> DashboardList { get; set; } = new List<EcnEcoFollowUpDashboardViewModel>();

        private bool _IsAdminToolsEnabled = false;
        public bool IsAdminToolsEnabled
        {
            get { return _IsAdminToolsEnabled; }
            set
            {
                if (this._IsAdminToolsEnabled != value)
                {
                    this._IsAdminToolsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private TabItem _SelectedTab;
        public TabItem SelectedTab
        {
            get { return _SelectedTab; }
            set
            {
                if (this._SelectedTab != value)
                {
                    this._SelectedTab = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Properties not from interface
        public string WindchillAccount { get; set; } = string.Empty;
        public string WindchillPass { get; set; } = string.Empty;
        #endregion

        #region [REGION] Events
        public event EventHandler SavedSearchesListEvent;
        public event EventHandler RecentSearchesListEvent;
        public event EventHandler DashboardListEvent;

        public void RaiseSavedSearchesListEvent()
        {
            try
            {
                SavedSearchesListEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public void RaiseRecentSearchesListEvent()
        {
            try
            {
                RecentSearchesListEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public void RaiseDashboardListEvent()
        {
            try
            {
                DashboardListEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}
