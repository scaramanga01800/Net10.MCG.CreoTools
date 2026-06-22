using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.QuickSearch.Configuration;
using MCG.CREO_Tools.QuickSearch.Exceptions;
using MCG.CREO_Tools.QuickSearch.View;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
    public class QuickSearchDataContext : ObservableObject, IQuickSearchDataContext
    {
        #region [REGION] Properties from Interface
        private bool _IsCreoEnable = false;
        public bool IsCreoEnable
        {
            get { return this._IsCreoEnable; }
            set
            {
                if (this._IsCreoEnable != value)
                {
                    this._IsCreoEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _CRWLocalShown;
        public bool CRWLocalShown
        {
            get { return this._CRWLocalShown; }
            set
            {
                if (this._CRWLocalShown != value)
                {
                    this._CRWLocalShown = value;
                    OnPropertyChanged();
                    UpdateListStandardShown();
                    RaiseListStandardSelectionChanged();
                }
            }
        }

        private bool _DGLocalShown;
        public bool DGLocalShown
        {
            get { return this._DGLocalShown; }
            set
            {
                if (this._DGLocalShown != value)
                {
                    this._DGLocalShown = value;
                    OnPropertyChanged();
                    UpdateListStandardShown();
                    RaiseListStandardSelectionChanged();
                }
            }
        }

        private bool _SGLocalShown;
        public bool SGLocalShown
        {
            get { return this._SGLocalShown; }
            set
            {
                if (this._SGLocalShown != value)
                {
                    this._SGLocalShown = value;
                    OnPropertyChanged();
                    UpdateListStandardShown();
                    RaiseListStandardSelectionChanged();
                }
            }
        }

        private bool _TWRLocalShown;
        public bool TWRLocalShown
        {
            get { return this._TWRLocalShown; }
            set
            {
                if (this._TWRLocalShown != value)
                {
                    this._TWRLocalShown = value;
                    OnPropertyChanged();
                    UpdateListStandardShown();
                    RaiseListStandardSelectionChanged();
                }
            }
        }

        private bool _MFGTWRLocalShown;
        public bool MFGTWRLocalShown
        {
            get { return this._MFGTWRLocalShown; }
            set
            {
                if (this._MFGTWRLocalShown != value)
                {
                    this._MFGTWRLocalShown = value;
                    OnPropertyChanged();
                    UpdateListStandardShown();
                    RaiseListStandardSelectionChanged();
                }
            }
        }

        private bool _STDGlobalShown;
        public bool STDGlobalShown
        {
            get { return this._STDGlobalShown; }
            set
            {
                if (this._STDGlobalShown != value)
                {
                    this._STDGlobalShown = value;
                    OnPropertyChanged();
                    UpdateListStandardShown();
                    RaiseListStandardSelectionChanged();
                }
            }
        }

        private bool _CRWLocalEnabled;
        public bool CRWLocalEnabled
        {
            get { return this._CRWLocalEnabled; }
            set
            {
                if (this._CRWLocalEnabled != value)
                {
                    this._CRWLocalEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _DGLocalEnabled;
        public bool DGLocalEnabled
        {
            get { return this._DGLocalEnabled; }
            set
            {
                if (this._DGLocalEnabled != value)
                {
                    this._DGLocalEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _SGLocalEnabled;
        public bool SGLocalEnabled
        {
            get { return this._SGLocalEnabled; }
            set
            {
                if (this._SGLocalEnabled != value)
                {
                    this._SGLocalEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _TWRLocalEnabled;
        public bool TWRLocalEnabled
        {
            get { return this._TWRLocalEnabled; }
            set
            {
                if (this._TWRLocalEnabled != value)
                {
                    this._TWRLocalEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _MFGTWRLocalEnabled;
        public bool MFGTWRLocalEnabled
        {
            get { return this._MFGTWRLocalEnabled; }
            set
            {
                if (this._MFGTWRLocalEnabled != value)
                {
                    this._MFGTWRLocalEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _STDGlobalEnabled;
        public bool STDGlobalEnabled
        {
            get { return this._STDGlobalEnabled; }
            set
            {
                if (this._STDGlobalEnabled != value)
                {
                    this._STDGlobalEnabled = value;
                    OnPropertyChanged();
                }
            }
        }


        public ObservableCollection<QuickSearchPartClass> ListClass { get; set; } = new ObservableCollection<QuickSearchPartClass>();

        private QuickSearchPartClass _SelectedClassItem;
        public QuickSearchPartClass SelectedClassItem
        {
            get { return this._SelectedClassItem; }
            set
            {
                if (this._SelectedClassItem != value)
                {
                    this._SelectedClassItem = value;
                    OnPropertyChanged();
                    RaiseListClassChanged();
                }
            }
        }

        public ObservableCollection<QuickSearchPartSubClass> ListSubClass { get; set; } = new ObservableCollection<QuickSearchPartSubClass>();

        private QuickSearchPartSubClass _SelectedSubClassItem;
        public QuickSearchPartSubClass SelectedSubClassItem
        {
            get { return this._SelectedSubClassItem; }
            set
            {
                if (this._SelectedSubClassItem != value)
                {
                    this._SelectedSubClassItem = value;
                    OnPropertyChanged();
                    RaiseListSubClassChanging();
                }
            }
        }

        private string _RefDocument;
        public string RefDocument
        {
            get { return this._RefDocument; }
            set
            {
                if (this._RefDocument != value)
                {
                    this._RefDocument = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<QuickSearchExtraCompMenu> _ListExtraMenu;
        public List<QuickSearchExtraCompMenu> ListExtraMenu
        {
            get { return this._ListExtraMenu; }
            set
            {
                if (this._ListExtraMenu != value)
                {
                    this._ListExtraMenu = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsExtraComponentShown = false;
        public bool IsExtraComponentShown
        {
            get { return this._IsExtraComponentShown; }
            set
            {
                if (this._IsExtraComponentShown != value)
                {
                    this._IsExtraComponentShown = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsExtraComponentPossible = false;
        public bool IsExtraComponentPossible
        {
            get { return this._IsExtraComponentPossible; }
            set
            {
                if (this._IsExtraComponentPossible != value)
                {
                    this._IsExtraComponentPossible = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsPartPictureShown = false;
        public bool IsPartPictureShown
        {
            get { return this._IsPartPictureShown; }
            set
            {
                if (this._IsPartPictureShown != value)
                {
                    this._IsPartPictureShown = value;
                    OnPropertyChanged();
                }
            }
        }

        private BitmapImage _MainPictureShown;
        public BitmapImage MainPictureShown
        {
            get { return this._MainPictureShown; }
            set
            {
                if (this._MainPictureShown != value)
                {
                    this._MainPictureShown = value;
                    OnPropertyChanged();
                }
            }
        }

        private BitmapImage _ExtraPictureShown;
        public BitmapImage ExtraPictureShown
        {
            get { return this._ExtraPictureShown; }
            set
            {
                if (this._ExtraPictureShown != value)
                {
                    this._ExtraPictureShown = value;
                    OnPropertyChanged();
                }
            }
        }

        private BitmapImage _PartPictureShown;
        public BitmapImage PartPictureShown
        {
            get { return this._PartPictureShown; }
            set
            {
                if (this._PartPictureShown != value)
                {
                    this._PartPictureShown = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<QuickSearchPart> ListPartItemShown { get; set; } = new ObservableCollection<QuickSearchPart>();
        private QuickSearchPart _SelectedPartItem;
        public QuickSearchPart SelectedPartItem
        {
            get { return this._SelectedPartItem; }
            set
            {
                if (this._SelectedPartItem != value)
                {
                    this._SelectedPartItem = value;
                    OnPropertyChanged();
                }
            }
        }

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
                    RaiseListSubClassChanging();
                    RaiseSapPlantChangeEvent();
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

        public bool ShowSapCostVolumeInfo { get; set; }

        public ObservableCollection<QuickSearchShortCutViewModel> ListShortCut { get; set; } = new ObservableCollection<QuickSearchShortCutViewModel>();

        private bool _IsEditMode = false;
        public bool IsEditMode
        {
            get { return _IsEditMode; }
            set
            {
                if (this._IsEditMode != value)
                {
                    this._IsEditMode = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsAdminToolsEnabled =false;
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

        private bool _IsRefDocHtmlLink = false;
        public bool IsRefDocHtmlLink
        {
            get { return _IsRefDocHtmlLink; }
            set
            {
                if (this._IsRefDocHtmlLink != value)
                {
                    this._IsRefDocHtmlLink = value;
                    OnPropertyChanged();
                }

            }
        }

        #endregion

        #region [REGION] Properties not from interface
        public List<string> ListStandardShown { get; set; } = new List<string>();
        public List<QuickSearchPart> ListPartItemCurrentSubClass { get; set; }
        public List<QuickSearchShortCutData> ListShortCutData { get; set; }
        #endregion

        #region [REGION] Events
        public event EventHandler StandardSelectionChangedEvent;
        public event EventHandler ClassChangedEvent;
        public event EventHandler SubClassChangingEvent;
        public event EventHandler SubClassChangedEvent;
        public event EventHandler ShortCutChangedEvent;
        public event EventHandler SapPlantChangeEvent;

        public void RaiseListStandardSelectionChanged()
        {
            try
            {
                if (StandardSelectionChangedEvent != null)
                    StandardSelectionChangedEvent(this, new EventArgs());
            }
            catch (Exception ex)
            {
               QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void RaiseListClassChanged()
        {
            try
            {
                if (ClassChangedEvent != null)
                    ClassChangedEvent(this, new EventArgs());
            }
            catch (Exception ex)
            {
               QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void RaiseListSubClassChanging()
        {
            try
            {
                if (SubClassChangingEvent != null)
                    SubClassChangingEvent(this, new EventArgs());
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void RaiseListSubClassChanged()
        {
            try
            {
                if (SubClassChangedEvent != null)
                    SubClassChangedEvent(this, new EventArgs());
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void RaiseShortCutChangedEvent()
        {
            try
            {
                if (ShortCutChangedEvent != null)
                    ShortCutChangedEvent(this, new EventArgs());
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void RaiseSapPlantChangeEvent()
        {
            try
            {
                SapPlantChangeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Misc
        public void UpdateListStandardShown()
        {
            ListStandardShown.Clear();
            if (CRWLocalShown) ListStandardShown.Add(QuickSearchConstants.SubClassCrawler);
            if (DGLocalShown) ListStandardShown.Add(QuickSearchConstants.SubClassDG);
            if (SGLocalShown) ListStandardShown.Add(QuickSearchConstants.SubClassSG);
            if (TWRLocalShown) ListStandardShown.Add(QuickSearchConstants.SubClassTower);
            if (MFGTWRLocalShown) ListStandardShown.Add(QuickSearchConstants.SubClassTowerMfg);
            if (STDGlobalShown) ListStandardShown.Add(QuickSearchConstants.SubClassGlobal);
        }
        #endregion

    }
}
