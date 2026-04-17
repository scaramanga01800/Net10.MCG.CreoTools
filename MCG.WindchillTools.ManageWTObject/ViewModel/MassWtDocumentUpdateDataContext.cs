using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.Main;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.View;
using System.Collections.ObjectModel;

namespace MCG.WindchillTools.ManageWTObject.ViewModel
{
    public class MassWtDocumentUpdateDataContext : ObservableObject, IMassWtDocumentUpdateDataContext
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

        private bool _IsAllPartSelected = false;
        public bool IsAllPartSelected
        {
            get { return _IsAllPartSelected; }
            set
            {
                if (this._IsAllPartSelected != value)
                {
                    this._IsAllPartSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<MgtWtDocumentItem> WtDocumentList { get; set; } = new ObservableCollection<MgtWtDocumentItem>();

        public ObservableCollection<string> ListWindchillDocumentType { get; set; } = new ObservableCollection<string>();

        private string _SelectedWindchillDocumentType;
        public string SelectedWindchillDocumentType
        {
            get { return _SelectedWindchillDocumentType; }
            set
            {
                if (this._SelectedWindchillDocumentType != value)
                {
                    this._SelectedWindchillDocumentType = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> ListWindchillPartType { get; set; } = new ObservableCollection<string>();

        private string _SelectedWindchillPartType;
        public string SelectedWindchillPartType
        {
            get { return _SelectedWindchillPartType; }
            set
            {
                if (this._SelectedWindchillPartType != value)
                {
                    this._SelectedWindchillPartType = value;
                    OnPropertyChanged();
                }

            }
        }


        public ObservableCollection<WindchillContentType> ListContentType { get; set; } = new ObservableCollection<WindchillContentType>();

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

        private string _SelectedWebterm;
        public string SelectedWebterm
        {
            get { return _SelectedWebterm; }
            set
            {
                if (this._SelectedWebterm != value)
                {
                    this._SelectedWebterm = value;
                    OnPropertyChanged();
                }
                RaiseChangeWebtermEvent();
            }
        }

        private string _SelectedLocalWebterm;
        public string SelectedLocalWebterm
        {
            get { return _SelectedLocalWebterm; }
            set
            {
                if (this._SelectedLocalWebterm != value)
                {
                    this._SelectedLocalWebterm = value;
                    OnPropertyChanged();
                }
                RaiseChangeLocalWebtermEvent();
            }
        }

        private string _StatusBarTextRight;
        public string StatusBarTextRight
        {
            get { return _StatusBarTextRight; }
            set
            {
                if (this._StatusBarTextRight != value)
                {
                    this._StatusBarTextRight = value;
                    OnPropertyChanged();
                }

            }
        }

        private int _TotalStep=0;
        public int TotalStep
        {
            get { return _TotalStep; }
            set
            {
                if (this._TotalStep != value)
                {
                    this._TotalStep = value;
                    OnPropertyChanged();
                }

            }
        }

        private int _CurrentStep=0;
        public int CurrentStep
        {
            get { return _CurrentStep; }
            set
            {
                if (this._CurrentStep != value)
                {
                    this._CurrentStep = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> ListQualInspGrp { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListGroup { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListBrand { get; set; } = new ObservableCollection<string>();
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
        #endregion

        #region [REGION] Init
        public MassWtDocumentUpdateDataContext()
        {
            try
            {
                ListWindchillDocumentType.Add("PLAN Tif");
                ListWindchillDocumentType.Add("Reference Document");
                ListWindchillDocumentType.Add("Technical Document");
                SelectedWindchillDocumentType = ListWindchillDocumentType.First();

                ListWindchillPartType.Add("MCPart");
                SelectedWindchillPartType = ListWindchillPartType.First();

                ListContentType.Add(WindchillContentType.PRIMARY_CONTENT);
                ListContentType.Add(WindchillContentType.SECONDARY_CONTENT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [REGION] Misc Methods
        #endregion
    }
}
