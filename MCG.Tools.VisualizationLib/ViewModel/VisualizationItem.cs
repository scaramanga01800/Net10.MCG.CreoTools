using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MCG.CommonLib.Models.Enums;
using MCG.Tools.VisualizationLib.Exceptions;
using MCG.Tools.VisualizationLib.Interfaces;
using MCG.Tools.VisualizationLib.Messages;
using MCG.Tools.VisualizationLib.View;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using System.Collections.ObjectModel;

namespace MCG.Tools.VisualizationLib.ViewModel
{
    public partial class VisualizationItem : ObservableObject, IVisualizationItem
    {
        #region [REGION] Properties from Interface
        private string _PartNumber;
        public string PartNumber
        {
            get { return _PartNumber; }
            set
            {
                if (this._PartNumber != value)
                {
                    this._PartNumber = value;
                    OnPropertyChanged();
                }

            }
        }

        [ObservableProperty]
        private string _PartRevision;
        //public string PartRevision
        //{
        //    get { return _PartRevision; }
        //    set
        //    {
        //        if (this._PartRevision != value)
        //        {
        //            this._PartRevision = value;
        //            OnPropertyChanged();
        //            //_wtDownloadViewableTools.UpdateSelectedRevisionInformation(this);
        //        }
        //    }
        //}

        partial void OnPartRevisionChanged(string value)
        {
            OnPropertyChanged(nameof(IsLatestRevisionSelected));
            // On crie dans le vide : "Ma révision a changé !"
            WeakReferenceMessenger.Default.Send(new PartRevisionChangedMessage(this));
        }


        public ObservableCollection<string> AllPartRevision { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<VisualizationItemRevisionState> AllPartRevisionState { get; set; } = new ObservableCollection<VisualizationItemRevisionState>();

        private string _State;
        public string State
        {
            get { return _State; }
            set
            {
                if (this._State != value)
                {
                    this._State = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _DescriptionEng;
        public string DescriptionEng
        {
            get { return _DescriptionEng; }
            set
            {
                if (this._DescriptionEng != value)
                {
                    this._DescriptionEng = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _DescriptionLocal;
        public string DescriptionLocal
        {
            get { return _DescriptionLocal; }
            set
            {
                if (this._DescriptionLocal != value)
                {
                    this._DescriptionLocal = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _PdmContext;
        public string PdmContext
        {
            get { return _PdmContext; }
            set
            {
                if (this._PdmContext != value)
                {
                    this._PdmContext = value;
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

        private string _DetailComment;
        public string DetailComment
        {
            get { return _DetailComment; }
            set
            {
                if (this._DetailComment != value)
                {
                    this._DetailComment = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _AddedFrom;
        public string AddedFrom
        {
            get { return _AddedFrom; }
            set
            {
                if (this._AddedFrom != value)
                {
                    this._AddedFrom = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsDocumentFound = false;
        public bool IsDocumentFound
        {
            get { return _IsDocumentFound; }
            set
            {
                if (this._IsDocumentFound != value)
                {
                    this._IsDocumentFound = value;
                    OnPropertyChanged();
                }

            }
        }

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

        private bool _IsAllSelected = false;
        public bool IsAllSelected
        {
            get { return _IsAllSelected; }
            set
            {
                if (this._IsAllSelected != value)
                {
                    this._IsAllSelected = value;
                    OnPropertyChanged();
                }
                CheckUncheckAllDocument();

            }

        }

        public bool IsLatestRevisionSelected
        {
            get => PartRevision == LatestRevision;
        }

        private DocumentTypeEnum _ItemType = DocumentTypeEnum.UNKNOWN;
        public DocumentTypeEnum ItemType
        {
            get { return _ItemType; }
            set
            {
                if (this._ItemType != value)
                {
                    this._ItemType = value;
                    OnPropertyChanged();
                }

            }
        }

        private DocumentTypeEnum _ItemFrom = DocumentTypeEnum.UNKNOWN;
        public DocumentTypeEnum ItemFrom
        {
            get { return _ItemFrom; }
            set
            {
                if (this._ItemFrom != value)
                {
                    this._ItemFrom = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<VisualizationDocument> SearchedDocumentList { get; set; } = new ObservableCollection<VisualizationDocument>();
        #endregion

        #region [REGION] Internal variables
        public List<VisualizationDocument> SearchedCompleteDocumentList { get; set; } = new List<VisualizationDocument>();
        public bool IsDocumentSearched { get; set; } = false;

        public bool IsEcnInformationSearched { get; set; } = false;
        public string EcnNumber { get; set; } = "";

        public WindchillObjectWtPart WindchillPart { get; set; }
        public RestOdataChangeNotice WindchillEcn { get; set; }

        public List<RestOdataWtObject> AllOdataWtPartRevision { get; set; }

        public string LatestRevision { get; set; }
        #endregion

        public VisualizationItem() { }

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
        private void CheckUncheckAllDocument()
        {
            try
            {
                if (SearchedDocumentList != null)
                {
                    foreach (var item in SearchedDocumentList)
                        item.IsSelected = IsAllSelected;
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        [Obsolete("MIGRATION : N'utilisez plus VisualizationItem. Utilisez la nouvelle classe IWtDownloadViewableTools.UpdateSelectedRevisionInformation() à la place.", error: true)]
        private void UpdateSelectedRevisionInformation()
        {
            //try
            //{
            //    IsDocumentFound = false;
            //    IsDocumentSearched = false;
            //    IsAllSelected = false;
            //    SearchedDocumentList.Clear();
            //    SearchedCompleteDocumentList.Clear();

            //    if (AllOdataWtPartRevision != null)
            //    {
            //        RestOdataWtObject CurrentOdataPart = AllOdataWtPartRevision.FirstOrDefault((item) => item.Revision == PartRevision);
            //        if (CurrentOdataPart != null)
            //        {
            //            WindchillObjectWtPart CurrentWindchillPart = _windchillRequestMiscService.GetWindchillPart(CurrentOdataPart.GetWtPart());
            //            State = CurrentOdataPart.State.Display;
            //            DescriptionEng = $"{CurrentWindchillPart.Name}|{CurrentWindchillPart.DescriptionEn2}";
            //            DescriptionLocal = $"{CurrentWindchillPart.DescriptionLocal1}|{CurrentWindchillPart.DescriptionLocal2}";
            //            PdmContext = CurrentWindchillPart.Context.Name;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    VisualizationException.SendMessageBox(this.GetType().Name, ex);
            //}
        }
        #endregion
    }
}
