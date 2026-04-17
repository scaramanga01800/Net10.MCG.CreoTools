using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.Main;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.View;
using System.Collections.ObjectModel;

namespace MCG.WindchillTools.ManageWTObject.ViewModel
{
    public class CreateUpdateWtDocumentWtPartDataContext : ObservableObject, ICreateUpdateWtDocumentWtPartDataContext
    {

        #region [REGION] Properties from Interface
        private bool _ActionInProgress = false;
        public bool ActionInProgress
        {
            get { return _ActionInProgress; }
            set
            {
                if (this._ActionInProgress != value)
                {
                    this._ActionInProgress = value;
                    OnPropertyChanged();
                }

            }
        }

        private MgtWtDocumentItem _CurrentWtObject;
        public MgtWtDocumentItem CurrentWtObject
        {
            get { return _CurrentWtObject; }
            set
            {
                if (this._CurrentWtObject != value)
                {
                    this._CurrentWtObject = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _WtDocumentSelected;
        public bool WtDocumentSelected
        {
            get { return _WtDocumentSelected; }
            set
            {
                if (this._WtDocumentSelected != value)
                {
                    this._WtDocumentSelected = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _WtPartSelected;
        public bool WtPartSelected
        {
            get { return _WtPartSelected; }
            set
            {
                if (this._WtPartSelected != value)
                {
                    this._WtPartSelected = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<MgtContentItem> ListContentItem { get; set; } = new ObservableCollection<MgtContentItem>();

        public ObservableCollection<string> ListWindchillDocumentType { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> ListWindchillPartType { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<WindchillContext> WindchillContextList { get; set; } = new ObservableCollection<WindchillContext>();

        private WindchillContext _SelectedWindchillContext;
        public WindchillContext SelectedWindchillContext
        {
            get { return _SelectedWindchillContext; }
            set
            {
                if (this._SelectedWindchillContext != value)
                {
                    this._SelectedWindchillContext = value;
                    OnPropertyChanged();
                }

            }
        }

        /// <summary>
        /// Gets or sets the list webterm.
        /// </summary>
        /// <value>
        /// The list webterm.
        /// </value>
        public ObservableCollection<string> ListWebterm { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets the list webterm local.
        /// </summary>
        /// <value>
        /// The list webterm local.
        /// </value>
        public ObservableCollection<string> ListWebtermLocal { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets the list language.
        /// </summary>
        /// <value>
        /// The list language.
        /// </value>
        public ObservableCollection<MCGLanguage> ListLanguage { get; set; } = new ObservableCollection<MCGLanguage>();
        /// <summary>
        /// The  property
        /// </summary>
        private MCGLanguage _SelectedLanguage;
        /// <summary>
        /// Gets or sets the property
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
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
                RaiseChangeLanguageEvent();
            }
        }

        public ObservableCollection<WindchillContentType> ListContentType { get; set; } = new ObservableCollection<WindchillContentType>();

        private string _FilterNumber;
        public string FilterNumber
        {
            get { return _FilterNumber; }
            set
            {
                if (this._FilterNumber != value)
                {
                    this._FilterNumber = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<RestOdataWtDocument> ListSearchWtDocument { get; set; } = new ObservableCollection<RestOdataWtDocument>();

        public ObservableCollection<RestOdataWtPart> ListSearchWtPart { get; set; } = new ObservableCollection<RestOdataWtPart>();

        public ObservableCollection<string> AllUnits { get; set; } = new ObservableCollection<string>();

        private string _StatusBarText;
        public string StatusBarText
        {
            get { return _StatusBarText; }
            set
            {
                if (this._StatusBarText != value)
                {
                    this._StatusBarText = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<WindchillObjectLinkType> WtObjectLinkList { get; set; } = new ObservableCollection<WindchillObjectLinkType>();

        private bool _LinkWtDocumentWtPart = false;
        public bool LinkWtDocumentWtPart
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

        public ObservableCollection<string> MaterialList { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListGroup { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListSubGroup { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListBrand { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListOption { get; set; } = new ObservableCollection<string>();

        private string _SelectedBrand;
        public string SelectedBrand
        {
            get { return _SelectedBrand; }
            set
            {
                if (this._SelectedBrand != value)
                {
                    this._SelectedBrand = value;
                    OnPropertyChanged();
                    RaiseUpdateBrandEvent();
                }

            }
        }

        private string _SelectedGroup;
        public string SelectedGroup
        {
            get { return _SelectedGroup; }
            set
            {
                if (this._SelectedGroup != value)
                {
                    this._SelectedGroup = value;
                    OnPropertyChanged();
                    RaiseUpdateGroupEvent();
                }

            }
        }

        private string _SelectedSubGroup;
        public string SelectedSubGroup
        {
            get { return _SelectedSubGroup; }
            set
            {
                if (this._SelectedSubGroup != value)
                {
                    this._SelectedSubGroup = value;
                    OnPropertyChanged();
                    RaiseUpdateSubGroupEvent();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public List<WindchillContext> AllWindchillContextList { get; set; } = new List<WindchillContext>();
        #endregion

        #region [REGION] Event
        public event EventHandler ChangeLanguageEvent;
        public void RaiseChangeLanguageEvent()
        {
            try
            {
                ChangeLanguageEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler ChangeWebtermEvent;
        public void RaiseChangeWebtermEvent()
        {
            try
            {
                ChangeWebtermEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler ChangeLocalWebtermEvent;
        public void RaiseChangeLocalWebtermEvent()
        {
            try
            {
                ChangeLocalWebtermEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UpdateBrandEvent;
        public void RaiseUpdateBrandEvent()
        {
            try
            {
                UpdateBrandEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UpdateGroupEvent;
        public void RaiseUpdateGroupEvent()
        {
            try
            {
                UpdateGroupEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UpdateSubGroupEvent;
        public void RaiseUpdateSubGroupEvent()
        {
            try
            {
                UpdateSubGroupEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Init
        public CreateUpdateWtDocumentWtPartDataContext()
        {
            try
            {
                ListWindchillDocumentType.Add("PLAN Tif");
                ListWindchillDocumentType.Add("Reference Document");
                ListWindchillDocumentType.Add("Technical Document");
                ListWindchillDocumentType.Add("Illustration iso");

                ListWindchillPartType.Add("MCPart");

                ListContentType.Add(WindchillContentType.PRIMARY_CONTENT);
                ListContentType.Add(WindchillContentType.SECONDARY_CONTENT);

                WtObjectLinkList.Add(WindchillObjectLinkType.DESCRIPTOR);
                WtObjectLinkList.Add(WindchillObjectLinkType.REFERENCE);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
