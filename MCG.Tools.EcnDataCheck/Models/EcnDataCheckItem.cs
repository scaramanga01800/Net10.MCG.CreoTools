
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.EcnDataCheck.View;
using MCG.WindchillRequestTool;
using MCG.WindchillRequestTool.Model.Windchill;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MCG.Tools.EcnDataCheck.Models
{
    public class EcnDataCheckItem : ObservableObject, IEcnDataCheckItem
    {
        #region [REGION] Properties for all Check Status
        private DataCheckStatus _PartMissingCheck = DataCheckStatus.UNKNOWN;
        public DataCheckStatus PartMissingCheck
        {
            get { return this._PartMissingCheck; }
            set
            {
                if (this._PartMissingCheck != value)
                {
                    this._PartMissingCheck = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _MetaDataStatus = DataCheckStatus.OK;
        public DataCheckStatus MetaDataStatus
        {
            get { return this._MetaDataStatus; }
            set
            {
                if (this._MetaDataStatus != value)
                {
                    this._MetaDataStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _BomPdmComparisonStatus = DataCheckStatus.UNKNOWN;
        public DataCheckStatus BomPdmComparisonStatus
        {
            get { return this._BomPdmComparisonStatus; }
            set
            {
                if (this._BomPdmComparisonStatus != value)
                {
                    this._BomPdmComparisonStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _BomErpComparisonStatus = DataCheckStatus.UNKNOWN;
        public DataCheckStatus BomErpComparisonStatus
        {
            get { return this._BomErpComparisonStatus; }
            set
            {
                if (this._BomErpComparisonStatus != value)
                {
                    this._BomErpComparisonStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _ContextStatus = DataCheckStatus.OK;
        public DataCheckStatus ContextStatus
        {
            get { return this._ContextStatus; }
            set
            {
                if (this._ContextStatus != value)
                {
                    this._ContextStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _Desc1EnStatus = DataCheckStatus.OK;
        public DataCheckStatus Desc1EnStatus
        {
            get { return this._Desc1EnStatus; }
            set
            {
                if (this._Desc1EnStatus != value)
                {
                    this._Desc1EnStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _Desc2EnStatus = DataCheckStatus.OK;
        public DataCheckStatus Desc2EnStatus
        {
            get { return this._Desc2EnStatus; }
            set
            {
                if (this._Desc2EnStatus != value)
                {
                    this._Desc2EnStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _Desc1LocalStatus = DataCheckStatus.OK;
        public DataCheckStatus Desc1LocalStatus
        {
            get { return this._Desc1LocalStatus; }
            set
            {
                if (this._Desc1LocalStatus != value)
                {
                    this._Desc1LocalStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _Desc2LocalStatus = DataCheckStatus.OK;
        public DataCheckStatus Desc2LocalStatus
        {
            get { return this._Desc2LocalStatus; }
            set
            {
                if (this._Desc2LocalStatus != value)
                {
                    this._Desc2LocalStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _GroupCreatorStatus = DataCheckStatus.OK;
        public DataCheckStatus GroupCreatorStatus
        {
            get { return this._GroupCreatorStatus; }
            set
            {
                if (this._GroupCreatorStatus != value)
                {
                    this._GroupCreatorStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _MassStatus = DataCheckStatus.OK;
        public DataCheckStatus MassStatus
        {
            get { return this._MassStatus; }
            set
            {
                if (this._MassStatus != value)
                {
                    this._MassStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _QualInspGrpStatus = DataCheckStatus.OK;
        public DataCheckStatus QualInspGrpStatus
        {
            get { return this._QualInspGrpStatus; }
            set
            {
                if (this._QualInspGrpStatus != value)
                {
                    this._QualInspGrpStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _DefaultUnitStatus = DataCheckStatus.OK;
        public DataCheckStatus DefaultUnitStatus
        {
            get { return this._DefaultUnitStatus; }
            set
            {
                if (this._DefaultUnitStatus != value)
                {
                    this._DefaultUnitStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _PartRepresentationFromLegacy = DataCheckStatus.OK;
        public DataCheckStatus PartRepresentationFromLegacy
        {
            get { return _PartRepresentationFromLegacy; }
            set
            {
                if (this._PartRepresentationFromLegacy != value)
                {
                    this._PartRepresentationFromLegacy = value;
                    OnPropertyChanged();
                }

            }
        }

        private DataCheckStatus _MaterialStatus = DataCheckStatus.OK;
        public DataCheckStatus MaterialStatus
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

        private DataCheckStatus _BrandStatus = DataCheckStatus.OK;
        public DataCheckStatus BrandStatus
        {
            get { return _BrandStatus; }
            set
            {
                if (this._BrandStatus != value)
                {
                    this._BrandStatus = value;
                    OnPropertyChanged();
                }

            }
        }

        private DataCheckStatus _GroupStatus = DataCheckStatus.OK;
        public DataCheckStatus GroupStatus
        {
            get { return _GroupStatus; }
            set
            {
                if (this._GroupStatus != value)
                {
                    this._GroupStatus = value;
                    OnPropertyChanged();
                }

            }
        }

        private DataCheckStatus _SubGroupStatus = DataCheckStatus.OK;
        public DataCheckStatus SubGroupStatus
        {
            get { return _SubGroupStatus; }
            set
            {
                if (this._SubGroupStatus != value)
                {
                    this._SubGroupStatus = value;
                    OnPropertyChanged();
                }

            }
        }

        private DataCheckStatus _OptionStatus = DataCheckStatus.OK;
        public DataCheckStatus OptionStatus
        {
            get { return _OptionStatus; }
            set
            {
                if (this._OptionStatus != value)
                {
                    this._OptionStatus = value;
                    OnPropertyChanged();
                }

            }
        }

        private DataCheckStatus _RevisionStatus = DataCheckStatus.OK;
        public DataCheckStatus RevisionStatus
        {
            get { return _RevisionStatus; }
            set
            {
                if (this._RevisionStatus != value)
                {
                    this._RevisionStatus = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region [REGION] Other properties from Interface
        private bool _IsPdmBomComparison;
        public bool IsPdmBomComparison
        {
            get { return this._IsPdmBomComparison; }
            set
            {
                if (this._IsPdmBomComparison != value)
                {
                    this._IsPdmBomComparison = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsErpBomComparison;
        public bool IsErpBomComparison
        {
            get { return this._IsErpBomComparison; }
            set
            {
                if (this._IsErpBomComparison != value)
                {
                    this._IsErpBomComparison = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPartRepresentationFromLegacy { get; set; } = false;

        private WindchillObjectWtPart _EcnWtPart;
        public WindchillObjectWtPart EcnWtPart
        {
            get { return this._EcnWtPart; }
            set
            {
                if (this._EcnWtPart != value)
                {
                    this._EcnWtPart = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _NewName;
        public string NewName
        {
            get { return this._NewName; }
            set
            {
                if (this._NewName != value)
                {
                    this._NewName = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<WindchillContext> WindchillContextList { get; set; } = new ObservableCollection<WindchillContext>();

        private string _NewContextName;
        public string NewContextName
        {
            get { return this._NewContextName; }
            set
            {
                if (this._NewContextName != value)
                {
                    this._NewContextName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _NewFolderName;
        public string NewFolderName
        {
            get { return this._NewFolderName; }
            set
            {
                if (this._NewFolderName != value)
                {
                    this._NewFolderName = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomComparisonItem _PdmBomComparison;
        public BomComparisonItem PdmBomComparison
        {
            get { return this._PdmBomComparison; }
            set
            {
                if (this._PdmBomComparison != value)
                {
                    this._PdmBomComparison = value;
                    OnPropertyChanged();
                }
            }
        }

        private BomComparisonItem _ErpBomComparison;
        public BomComparisonItem ErpBomComparison
        {
            get { return this._ErpBomComparison; }
            set
            {
                if (this._ErpBomComparison != value)
                {
                    this._ErpBomComparison = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<IEcnDataCheckResultItem> ListDataCheckResultShown { get; set; } = new ObservableCollection<IEcnDataCheckResultItem>();

        private EcnDataCheckResultItem _SelectedDataCheckResultItem;
        public EcnDataCheckResultItem SelectedDataCheckResultItem
        {
            get { return _SelectedDataCheckResultItem; }
            set
            {
                if (this._SelectedDataCheckResultItem != value)
                {
                    this._SelectedDataCheckResultItem = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsResultItem =false;
        public bool IsResultItem
        {
            get { return _IsResultItem; }
            set
            {
                if (this._IsResultItem != value)
                {
                    this._IsResultItem = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsFirstRowDetailShow = true;
        public bool IsFirstRowDetailShow
        {
            get { return _IsFirstRowDetailShow; }
            set
            {
                if (this._IsFirstRowDetailShow != value)
                {
                    this._IsFirstRowDetailShow = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Properties defined only for the ViewModel
        public List<IEcnDataCheckResultItem> ListDataCheckResult { get; set; } = new List<IEcnDataCheckResultItem>();

        public List<WindchillObjectLink> LinkWtPartEpmDocumentOwner { get; set; } = new List<WindchillObjectLink>();
        public List<WindchillObjectLink> LinkWtPartEpmDocumentDescribe { get; set; } = new List<WindchillObjectLink>();
        public List<WindchillObjectLink> LinkWtPartWtDocumentDescribe { get; set; } = new List<WindchillObjectLink>();
        public List<WindchillObjectLink> LinkWtPartWtDocumentReference { get; set; } = new List<WindchillObjectLink>();
        public List<WindchillObjectEpmDocument> ListEpmDocument { get; set; } = new List<WindchillObjectEpmDocument>();
        public List<WindchillObjectWtDocument> ListWtDocument { get; set; } = new List<WindchillObjectWtDocument>();
        public List<WindchillObjectEpmDocument> ListSearchedEpmDocument { get; set; } = new List<WindchillObjectEpmDocument>();
        public WindchillObjectStructure PartStructure { get; set; }
        public WindchillObjectStructure EpmDocStructure { get; set; }
        public BomItem EprStructure { get; set; }
        public string TempMessage { get; set; }
        #endregion
    }
}
