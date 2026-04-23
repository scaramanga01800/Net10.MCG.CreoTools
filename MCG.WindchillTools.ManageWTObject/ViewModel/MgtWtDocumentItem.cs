using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Services.Statics;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.Exceptions;
using MCG.WindchillTools.ManageWTObject.View;
using System.Collections.ObjectModel;

namespace MCG.WindchillTools.ManageWTObject.ViewModel
{
    public class MgtWtDocumentItem : ObservableObject, IMgtWtDocumentItem
    {
        #region [REGION] Properties from Interface
        private bool _IsSelected = false;
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

        private McgRevisionSchemaEnum _Revision;
        public McgRevisionSchemaEnum Revision
        {
            get { return _Revision; }
            set
            {
                if (this._Revision != value)
                {
                    this._Revision = value;
                    OnPropertyChanged();
                    PartSearchDone = false;
                    WtDocumentSearchDone = false;
                    //PartFound = false;
                    //WtDocumentFound = false;
                    //PartRevisionFound = false;
                    //WtDocumentRevisionFound = false;
                }

            }
        }

        private McgRevisionSchemaEnum? _LastWtDocumentRevision = null;
        public McgRevisionSchemaEnum? LastWtDocumentRevision
        {
            get { return _LastWtDocumentRevision; }
            set
            {
                if (this._LastWtDocumentRevision != value)
                {
                    this._LastWtDocumentRevision = value;
                    OnPropertyChanged();
                }

            }
        }

        private McgRevisionSchemaEnum? _LastPartRevision = null;
        public McgRevisionSchemaEnum? LastPartRevision
        {
            get { return _LastPartRevision; }
            set
            {
                if (this._LastPartRevision != value)
                {
                    this._LastPartRevision = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _WindchillDocumentType;
        public string WindchillDocumentType
        {
            get { return _WindchillDocumentType; }
            set
            {
                if (this._WindchillDocumentType != value)
                {
                    this._WindchillDocumentType = value;
                    OnPropertyChanged();
                    UpdateDocumentType();
                }

            }
        }

        private string _WindchillPartType;
        public string WindchillPartType
        {
            get { return _WindchillPartType; }
            set
            {
                if (this._WindchillPartType != value)
                {
                    this._WindchillPartType = value;
                    OnPropertyChanged();
                    UpdatePartType();
                }

            }
        }

        private bool _Partfound = false;
        public bool PartFound
        {
            get { return _Partfound; }
            set
            {
                if (this._Partfound != value)
                {
                    this._Partfound = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _WtDocumentFound = false;
        public bool WtDocumentFound
        {
            get { return _WtDocumentFound; }
            set
            {
                if (this._WtDocumentFound != value)
                {
                    this._WtDocumentFound = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _PartRevisionFound = false;
        public bool PartRevisionFound
        {
            get { return _PartRevisionFound; }
            set
            {
                if (this._PartRevisionFound != value)
                {
                    this._PartRevisionFound = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _WtDocumentRevisionFound = false;
        public bool WtDocumentRevisionFound
        {
            get { return _WtDocumentRevisionFound; }
            set
            {
                if (this._WtDocumentRevisionFound != value)
                {
                    this._WtDocumentRevisionFound = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _PartSearchDone = false;
        public bool PartSearchDone
        {
            get { return _PartSearchDone; }
            set
            {
                if (this._PartSearchDone != value)
                {
                    this._PartSearchDone = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _WtDocumentSearchDone = false;
        public bool WtDocumentSearchDone
        {
            get { return _WtDocumentSearchDone; }
            set
            {
                if (this._WtDocumentSearchDone != value)
                {
                    this._WtDocumentSearchDone = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsNewRevision = false;
        public bool IsNewRevision
        {
            get { return _IsNewRevision; }
            set
            {
                if (this._IsNewRevision != value)
                {
                    this._IsNewRevision = value;
                    OnPropertyChanged();
                }

            }
        }

        private MgtWtObject _WtDocumentObject;
        public MgtWtObject WtDocumentObject
        {
            get { return _WtDocumentObject; }
            set
            {
                if (this._WtDocumentObject != value)
                {
                    this._WtDocumentObject = value;
                    OnPropertyChanged();
                }

            }
        }

        private MgtWtObject _WtPartObject;
        public MgtWtObject WtPartObject
        {
            get { return _WtPartObject; }
            set
            {
                if (this._WtPartObject != value)
                {
                    this._WtPartObject = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<MgtContentItem> ListContentItem { get; set; } = new ObservableCollection<MgtContentItem>();

        private string _StatusWtDocument = "UNKNOWN";
        public string StatusWtDocument
        {
            get { return _StatusWtDocument; }
            set
            {
                if (this._StatusWtDocument != value)
                {
                    this._StatusWtDocument = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _StatusPart = "UNKNOWN";
        public string StatusPart
        {
            get { return _StatusPart; }
            set
            {
                if (this._StatusPart != value)
                {
                    this._StatusPart = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _StatusWtDocumentPart = "UNKNOWN";
        public string StatusWtDocumentPart
        {
            get { return _StatusWtDocumentPart; }
            set
            {
                if (this._StatusWtDocumentPart != value)
                {
                    this._StatusWtDocumentPart = value;
                    OnPropertyChanged();
                }

            }
        }

        private MgtRequiredActionEnum _RequiredActionWtDocument = MgtRequiredActionEnum.DO_NOTHING;
        public MgtRequiredActionEnum RequiredActionWtDocument
        {
            get { return _RequiredActionWtDocument; }
            set
            {
                if (this._RequiredActionWtDocument != value)
                {
                    this._RequiredActionWtDocument = value;
                    OnPropertyChanged();
                }

            }
        }

        private MgtRequiredActionEnum _RequiredActionPart = MgtRequiredActionEnum.DO_NOTHING;
        public MgtRequiredActionEnum RequiredActionPart
        {
            get { return _RequiredActionPart; }
            set
            {
                if (this._RequiredActionPart != value)
                {
                    this._RequiredActionPart = value;
                    OnPropertyChanged();
                }

            }
        }

        private MgtRequiredActionEnum _RequiredActionWtDocumentPart = MgtRequiredActionEnum.LINK;
        public MgtRequiredActionEnum RequiredActionWtDocumentPart
        {
            get { return _RequiredActionWtDocumentPart; }
            set
            {
                if (this._RequiredActionWtDocumentPart != value)
                {
                    this._RequiredActionWtDocumentPart = value;
                    OnPropertyChanged();
                }

            }
        }

        private WindchillObjectLinkType _LinkWtDocumentWtPart = WindchillObjectLinkType.DESCRIPTOR;
        public WindchillObjectLinkType LinkWtDocumentWtPart
        {
            get { return _LinkWtDocumentWtPart; }
            set
            {
                if (this._LinkWtDocumentWtPart != value)
                {
                    this._LinkWtDocumentWtPart = value;
                    OnPropertyChanged();
                }

            }
        }

        private ObjectState _LinkStatus = ObjectState.UNLINKED;
        public ObjectState LinkStatus
        {
            get { return _LinkStatus; }
            set
            {
                if (this._LinkStatus != value)
                {
                    this._LinkStatus = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public WindchillWtDocumentOdataTypeEnum WtDocumentOdataType { get; set; } = WindchillWtDocumentOdataTypeEnum.UNKNOWN;

        public WindchillWtPartOdataTypeEnum WtPartOdataType { get; set; } = WindchillWtPartOdataTypeEnum.PTC_ProdMgmt_MCPart;

        public RestOdataWtDocument WindchillWtDocument { get; set; }

        public RestOdataWtPart WindchillWtPart { get; set; }
        #endregion

        #region [REGION] Init
        public MgtWtDocumentItem()
        {
            try
            {
                if (ListContentItem == null) ListContentItem = new ObservableCollection<MgtContentItem>();
                ListContentItem.CollectionChanged += ListContentItem_CollectionChanged;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void ListContentItem_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (ListContentItem != null && ListContentItem.Count > 0)
                {
                    ListContentItem.Last().IsPrimaryContentEvent += MgtWtDocumentItem_IsPrimaryContentEvent;
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void MgtWtDocumentItem_IsPrimaryContentEvent(object sender, EventArgs e)
        {
            try
            {
                MgtContentItem currentContent = null;
                if (sender != null && sender.GetType() == typeof(MgtContentItem))
                {
                    currentContent = (MgtContentItem)sender;
                    if (currentContent.IsPrimaryContent)
                    {
                        foreach (var item in ListContentItem.Where((content) => content.GetHashCode() != currentContent.GetHashCode()))
                            item.IsPrimaryContent = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

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

        #region [REGION] Misc Methods
        public void UpdateMainInformation(string CompleteFilename)
        {
            try
            {
                if (CompleteFilename != null && CompleteFilename.Trim() != "")
                {
                    var listExt = McgReflectionTools.GetEnumValues<FileExtensionEnum>();
                    string Filename = CompleteFilename.Split('\\').LastOrDefault();
                    if (Filename != null)
                    {
                        string[] filemaneSplit = Filename.ToUpper().Split('.').FirstOrDefault().Split('_');

                        Number = filemaneSplit[0];
                        if (filemaneSplit.Length > 1)
                        {
                            if (filemaneSplit[1] == "#") filemaneSplit[1] = "BLANK";
                            Revision = McgReflectionTools.GetEnumValue<McgRevisionSchemaEnum>(filemaneSplit[1]);
                        }
                        else
                            Revision = McgRevisionSchemaEnum.BLANK;
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateDocumentType()
        {
            try
            {
                switch (WindchillDocumentType.ToUpper())
                {
                    case "ANALYSE ESSAI":
                        WtDocumentOdataType = WindchillWtDocumentOdataTypeEnum.AnalyseEssai;
                        break;
                    case "ANNEXE ESSAI":
                        WtDocumentOdataType = WindchillWtDocumentOdataTypeEnum.AnnexeEssai;
                        break;
                    case "DEMANDE ESSAI":
                        WtDocumentOdataType = WindchillWtDocumentOdataTypeEnum.DemandeEssai;
                        break;
                    case "RAPPORT ESSAI":
                        WtDocumentOdataType = WindchillWtDocumentOdataTypeEnum.RapportEssai;
                        break;
                    case "ILLUSTRATION ISO":
                        WtDocumentOdataType = WindchillWtDocumentOdataTypeEnum.ILLUSTRATIONISO;
                        break;
                    case "PLAN TIF":
                        WtDocumentOdataType = WindchillWtDocumentOdataTypeEnum.PLANTIF;
                        break;
                    case "REFERENCE DOCUMENT":
                        WtDocumentOdataType = WindchillWtDocumentOdataTypeEnum.ReferenceDocument;
                        break;
                    case "TECHNICAL DOCUMENT":
                        WtDocumentOdataType = WindchillWtDocumentOdataTypeEnum.Technical_Document;
                        break;

                    default:
                        WtDocumentOdataType = WindchillWtDocumentOdataTypeEnum.Technical_Document;
                        break;
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        public void UpdateDocumentType(WindchillWtDocumentOdataTypeEnum CurrentDocType)
        {
            try
            {
                switch (CurrentDocType)
                {
                    case WindchillWtDocumentOdataTypeEnum.AnalyseEssai:
                        WindchillDocumentType = "Analyse Essai";
                        break;
                    case WindchillWtDocumentOdataTypeEnum.AnnexeEssai:
                        WindchillDocumentType = "Annexe Essai";
                        break;
                    case WindchillWtDocumentOdataTypeEnum.DemandeEssai:
                        WindchillDocumentType = "Demande Essai";
                        break;
                    case WindchillWtDocumentOdataTypeEnum.RapportEssai:
                        WindchillDocumentType = "Rapport Essai";
                        break;
                    case WindchillWtDocumentOdataTypeEnum.ILLUSTRATIONISO:
                        WindchillDocumentType = "Illustration ISO";
                        break;
                    case WindchillWtDocumentOdataTypeEnum.PLANTIF:
                        WindchillDocumentType = "PLAN Tif";
                        break;
                    case WindchillWtDocumentOdataTypeEnum.ReferenceDocument:
                        WindchillDocumentType = "Reference Document";
                        break;
                    case WindchillWtDocumentOdataTypeEnum.Technical_Document:
                        WindchillDocumentType = "Technical Document";
                        break;
                    default:
                        WindchillDocumentType = "Unknown";
                        break;
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdatePartType()
        {
            try
            {
                switch (WindchillPartType.ToUpper())
                {
                    case "MCPART":
                        WtPartOdataType = WindchillWtPartOdataTypeEnum.PTC_ProdMgmt_MCPart;
                        break;
                    default:
                        WtPartOdataType = WindchillWtPartOdataTypeEnum.PTC_ProdMgmt_MCPart;
                        break;
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void UpdatePartType(WindchillWtPartOdataTypeEnum CurrentPartType)
        {
            try
            {
                switch (CurrentPartType)
                {
                    case WindchillWtPartOdataTypeEnum.PTC_ProdMgmt_MCPart:
                        WindchillPartType = "MCPart";
                        break;
                    case WindchillWtPartOdataTypeEnum.PTC_ProdMgmt_WtPart:
                        WindchillPartType = "WtPart";
                        break;
                    default:
                        WindchillPartType = "Unknown";
                        break;
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

    }
}
