using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Services.Statics;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.ViewModel;
using MCG.WindchillTools.ManageWTObject.Exceptions;
using MCG.WindchillTools.ManageWTObject.View;

namespace MCG.WindchillTools.ManageWTObject.ViewModel
{
    public class MgtContentItem : ObservableObject, IMgtContentItem
    {
        #region [REGION] Properties from Interface
        private string _CompleteFilename;
        public string CompleteFilename
        {
            get { return _CompleteFilename; }
            set
            {
                if (this._CompleteFilename != value)
                {
                    this._CompleteFilename = value;
                    OnPropertyChanged();
                    UpdateMainInformation();
                }

            }
        }

        private FileExtensionEnum _Type;
        public FileExtensionEnum Type
        {
            get { return _Type; }
            set
            {
                if (this._Type != value)
                {
                    this._Type = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Filename;
        public string Filename
        {
            get { return _Filename; }
            set
            {
                if (this._Filename != value)
                {
                    this._Filename = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _ItemId;
        public string ItemId
        {
            get { return _ItemId; }
            set
            {
                if (this._ItemId != value)
                {
                    this._ItemId = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsPrimaryContent = false;
        public bool IsPrimaryContent
        {
            get { return _IsPrimaryContent; }
            set
            {
                RaisePreviousIsPrimaryContentEvent();
                if (this._IsPrimaryContent != value)
                {
                    this._IsPrimaryContent = value;
                    OnPropertyChanged();
                }
                if (value)
                    ContentType = WindchillContentType.PRIMARY_CONTENT;
                else
                    ContentType = WindchillContentType.SECONDARY_CONTENT;
                RaiseIsPrimaryContentEvent();
            }
        }

        private WindchillContentType _ContentType = WindchillContentType.PRIMARY_CONTENT;
        public WindchillContentType ContentType
        {
            get { return _ContentType; }
            set
            {
                if (this._ContentType != value)
                {
                    this._ContentType = value;
                    OnPropertyChanged();
                }

            }
        }

        private MgtWtDocumentItem _ParentWtDocument;
        public MgtWtDocumentItem ParentWtDocument
        {
            get { return _ParentWtDocument; }
            set
            {
                if (this._ParentWtDocument != value)
                {
                    this._ParentWtDocument = value;
                    OnPropertyChanged();
                }

            }
        }

        private ObjectState _State = ObjectState.NEW;
        public ObjectState State
        {
            get { return _State; }
            set
            {
                if (this._State != value)
                {
                    this._State = value;
                    OnPropertyChanged();
                }
                if (value == ObjectState.REMOVED)
                {
                    IsActive = false;
                    IsCanbeDownloaded = false;
                }
                else
                    IsActive = true;
            }
        }

        private bool _IsActive = true;
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                if (this._IsActive != value)
                {
                    this._IsActive = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsCanbeDownloaded = true;
        public bool IsCanbeDownloaded
        {
            get { return _IsCanbeDownloaded; }
            set
            {
                if (this._IsCanbeDownloaded != value)
                {
                    this._IsCanbeDownloaded = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Properties not from Interface
        public MgtWtDocumentItem ParentDocument { get; set; }

        public WindchillObjectViewableItemDownload Filecontent { get; set; }

        #endregion


        #region [REGION] Event Methods
        public event EventHandler IsPrimaryContentEvent;

        public void RaiseIsPrimaryContentEvent()
        {
            try
            {
                IsPrimaryContentEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler PreviousIsPrimaryContentEvent;

        public void RaisePreviousIsPrimaryContentEvent()
        {
            try
            {
                PreviousIsPrimaryContentEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion


        #region [REGION] Misc Methods
        private void UpdateMainInformation()
        {
            try
            {
                if (CompleteFilename != null && CompleteFilename.Trim() != "")
                {
                    var listExt = McgReflectionTools.GetEnumValues<FileExtensionEnum>();
                    Filename = CompleteFilename.Split('\\').LastOrDefault();
                    if (Filename != null)
                    {
                        string ext = Filename.ToUpper().Split('.').LastOrDefault();
                        Type = listExt.FirstOrDefault((item) => item.ToString() == ext);
                    }
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
