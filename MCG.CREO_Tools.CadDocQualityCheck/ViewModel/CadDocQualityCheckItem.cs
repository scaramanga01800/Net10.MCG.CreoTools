using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.Models.Enums;
using MCG.CREO_Tools.CadDocQualityCheck.View;
using pfcls;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class CadDocQualityCheckItem : ObservableObject, ICadDocQualityCheckItem
    {

        #region [REGION] Events
        /// <summary>
        /// Occurs when [is selected event].
        /// </summary>
        public event EventHandler IsSelectedEvent;

        /// <summary>
        /// Raises the saved searches list event.
        /// </summary>
        public void RaiseIsSelectedEvent()
        {
            try
            {
                IsSelectedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Properties from Interface
        private string _Number;
        public string Number
        {
            get { return _Number; }
            set
            {
                if (this._Number != value)
                {
                    this._Number = value;
                    OnPropertyChanged();
                }

            }
        }


        private bool _IsUpdated;
        public bool IsUpdated
        {
            get { return _IsUpdated; }
            set
            {
                if (this._IsUpdated != value)
                {
                    this._IsUpdated = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (this._IsSelected != value)
                {
                    this._IsSelected = value;
                    OnPropertyChanged();
                    RaiseIsSelectedEvent();
                }

            }
        }

        private bool _IsCheckedIn;
        public bool IsCheckedIn
        {
            get { return _IsCheckedIn; }
            set
            {
                if (this._IsCheckedIn != value)
                {
                    this._IsCheckedIn = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsCheckedOut;
        public bool IsCheckedOut
        {
            get { return _IsCheckedOut; }
            set
            {
                if (this._IsCheckedOut != value)
                {
                    this._IsCheckedOut = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsLocallyModified;
        public bool IsLocallyModified
        {
            get { return _IsLocallyModified; }
            set
            {
                if (this._IsLocallyModified != value)
                {
                    this._IsLocallyModified = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsReadOnly;
        public bool IsReadOnly
        {
            get { return _IsReadOnly; }
            set
            {
                if (this._IsReadOnly != value)
                {
                    this._IsReadOnly = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsFound = true;
        public bool IsFound
        {
            get { return _IsFound; }
            set
            {
                if (this._IsFound != value)
                {
                    this._IsFound = value;
                    OnPropertyChanged();
                }

            }
        }


        private string _Status;
        public string Status
        {
            get { return _Status; }
            set
            {
                if (this._Status != value)
                {
                    this._Status = value;
                    OnPropertyChanged();
                }

            }
        }


        private string _Comment;
        public string Comment
        {
            get { return _Comment; }
            set
            {
                if (this._Comment != value)
                {
                    this._Comment = value;
                    OnPropertyChanged();
                }

            }
        }



        private CadDocCheckStatus _LayersStatus = CadDocCheckStatus.UNKNOWN;
        public CadDocCheckStatus LayersStatus
        {
            get { return _LayersStatus; }
            set
            {
                if (this._LayersStatus != value)
                {
                    this._LayersStatus = value;
                    OnPropertyChanged();
                }

            }
        }

        private CadDocCheckStatus _RelationsStatus = CadDocCheckStatus.UNKNOWN;
        public CadDocCheckStatus RelationsStatus
        {
            get { return _RelationsStatus; }
            set
            {
                if (this._RelationsStatus != value)
                {
                    this._RelationsStatus = value;
                    OnPropertyChanged();
                }

            }
        }

        private CadDocCheckStatus _AttributesStatus = CadDocCheckStatus.UNKNOWN;
        public CadDocCheckStatus AttributesStatus
        {
            get { return _AttributesStatus; }
            set
            {
                if (this._AttributesStatus != value)
                {
                    this._AttributesStatus = value;
                    OnPropertyChanged();
                }

            }
        }

        private CadDocCheckStatus _ComponentStatus = CadDocCheckStatus.UNKNOWN;
        public CadDocCheckStatus ComponentStatus
        {
            get { return _ComponentStatus; }
            set
            {
                if (this._ComponentStatus != value)
                {
                    this._ComponentStatus = value;
                    OnPropertyChanged();
                }

            }
        }

        private CadDocCheckStatus _FeatureStatus = CadDocCheckStatus.UNKNOWN;
        public CadDocCheckStatus FeatureStatus
        {
            get { return _FeatureStatus; }
            set
            {
                if (this._FeatureStatus != value)
                {
                    this._FeatureStatus = value;
                    OnPropertyChanged();
                }

            }
        }


        private string _CurrentPreRegenRelations;
        public string CurrentPreRegenRelations
        {
            get { return _CurrentPreRegenRelations; }
            set
            {
                if (this._CurrentPreRegenRelations != value)
                {
                    this._CurrentPreRegenRelations = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _CurrentPostRegenRelations;
        public string CurrentPostRegenRelations
        {
            get { return _CurrentPostRegenRelations; }
            set
            {
                if (this._CurrentPostRegenRelations != value)
                {
                    this._CurrentPostRegenRelations = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _NewPreRegenRelations;
        public string NewPreRegenRelations
        {
            get { return _NewPreRegenRelations; }
            set
            {
                if (this._NewPreRegenRelations != value)
                {
                    this._NewPreRegenRelations = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _NewPostRegenRelations;
        public string NewPostRegenRelations
        {
            get { return _NewPostRegenRelations; }
            set
            {
                if (this._NewPostRegenRelations != value)
                {
                    this._NewPostRegenRelations = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsPostRegenRelationsOk = true;
        public bool IsPostRegenRelationsOk
        {
            get { return _IsPostRegenRelationsOk; }
            set
            {
                if (this._IsPostRegenRelationsOk != value)
                {
                    this._IsPostRegenRelationsOk = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsPreRegenRelationsOk = true;
        public bool IsPreRegenRelationsOk
        {
            get { return _IsPreRegenRelationsOk; }
            set
            {
                if (this._IsPreRegenRelationsOk != value)
                {
                    this._IsPreRegenRelationsOk = value;
                    OnPropertyChanged();
                }

            }
        }


        public ObservableCollection<CadDocRelationLineItem> ListCurrentPreRegenRelations { get; set; } = new ObservableCollection<CadDocRelationLineItem>();
        public ObservableCollection<CadDocRelationLineItem> ListCurrentPostRegenRelations { get; set; } = new ObservableCollection<CadDocRelationLineItem>();

        public ObservableCollection<CadDocLayerItem> ListLayers { get; set; } = new ObservableCollection<CadDocLayerItem>();

        public ObservableCollection<CadDocAttributeItem> ListAttributes { get; set; } = new ObservableCollection<CadDocAttributeItem>();


        private CadDocCheckStatus _MaterialStatus = CadDocCheckStatus.UNKNOWN;
        public CadDocCheckStatus MaterialStatus
        {
            get { return _MaterialStatus; }
            set
            {
                if (this._MaterialStatus != value)
                {
                    this._MaterialStatus = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsMaterialAssigned = true;
        public bool IsMaterialAssigned
        {
            get { return _IsMaterialAssigned; }
            set
            {
                if (this._IsMaterialAssigned != value)
                {
                    this._IsMaterialAssigned = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsNotDefaultMaterialAssigned = true;
        public bool IsNotDefaultMaterialAssigned
        {
            get { return _IsNotDefaultMaterialAssigned; }
            set
            {
                if (this._IsNotDefaultMaterialAssigned != value)
                {
                    this._IsNotDefaultMaterialAssigned = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsMaterialConditionDefined = true;
        public bool IsMaterialConditionDefined
        {
            get { return _IsMaterialConditionDefined; }
            set
            {
                if (this._IsMaterialConditionDefined != value)
                {
                    this._IsMaterialConditionDefined = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsUnitsOk = true;
        public bool IsUnitsOk
        {
            get { return _IsUnitsOk; }
            set
            {
                if (this._IsUnitsOk != value)
                {
                    this._IsUnitsOk = value;
                    OnPropertyChanged();
                }

            }
        }

        private EpmDocumentTypeEnum _CadDocSubType = EpmDocumentTypeEnum.UNKNOWN;
        public EpmDocumentTypeEnum CadDocSubType
        {
            get { return _CadDocSubType; }
            set
            {
                if (this._CadDocSubType != value)
                {
                    this._CadDocSubType = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<CadDocQualityCheckResultItem> ListQualityCheckResult { get; set; } = new ObservableCollection<CadDocQualityCheckResultItem>();

        private CadDocTemplate _Template;
        public CadDocTemplate Template
        {
            get { return _Template; }
            set
            {
                if (this._Template != value)
                {
                    this._Template = value;
                    OnPropertyChanged();
                }

            }
        }

        #endregion

        #region [REGION] Internal variables
        public bool IsModifiable { get; set; } = true;
        public IpfcModel CurrentCadModel { get; set; }
        public bool FromExcelImport { get; set; } = false;

        public List<string> PurgedPreRegenRelation { get; set; }
        public List<string> PurgedPostRegenRelation { get; set; }

        public IpfcModelItems LayerItems { get; set; }

        public EpmDocumentTypeEnum CadDocType { get; set; } = EpmDocumentTypeEnum.UNKNOWN;
        public CadDocRelationsList PreRelationsCheckResult { get; set; }
        public CadDocRelationsList PostRelationsCheckResult { get; set; }

        public List<CREOCadModelItem> ListRefPlans { get; set; }
        public List<CREOCadModelItem> ListRefPoints { get; set; }
        public List<CREOCadModelItem> ListRefAxis { get; set; }
        public List<CREOCadModelItem> ListRefCSys { get; set; }

        public IpfcUnitSystem DefaultUnits { get; set; }

        public bool IsExcluded { get; set; } = false;
        #endregion

        #region [REGION] Misc
        #endregion
    }
}
