using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.EcnDataCheck.Exceptions;
using MCG.Tools.EcnDataCheck.View;
using MCG.WindchillRequestTool.Model.Windchill;

namespace MCG.Tools.EcnDataCheck.Models
{
    public class EcnDataCheckResultItem : ObservableObject, IEcnDataCheckResultItem
    {
        #region [REGION] Properties from Interface
        private IEcnDataCheckItem _ParentEcnDataCheckItem;
        public IEcnDataCheckItem ParentEcnDataCheckItem
        {
            get { return this._ParentEcnDataCheckItem; }
            set
            {
                if (this._ParentEcnDataCheckItem != value)
                {
                    this._ParentEcnDataCheckItem = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _LinkedObjNumber;
        public string LinkedObjNumber
        {
            get { return this._LinkedObjNumber; }
            set
            {
                if (this._LinkedObjNumber != value)
                {
                    this._LinkedObjNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _LinkedObjRevision;
        public string LinkedObjRevision
        {
            get { return this._LinkedObjRevision; }
            set
            {
                if (this._LinkedObjRevision != value)
                {
                    this._LinkedObjRevision = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _CurrentLink;
        public string CurrentLink
        {
            get { return this._CurrentLink; }
            set
            {
                if (this._CurrentLink != value)
                {
                    this._CurrentLink = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataCheckStatus _Status;
        public DataCheckStatus Status
        {
            get { return this._Status; }
            set
            {
                if (this._Status != value)
                {
                    this._Status = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Comments;
        public string Comments
        {
            get { return this._Comments; }
            set
            {
                if (this._Comments != value)
                {
                    this._Comments = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _IssueDocumentationPath;
        public string IssueDocumentationPath
        {
            get { return _IssueDocumentationPath; }
            set
            {
                if (this._IssueDocumentationPath != value)
                {
                    this._IssueDocumentationPath = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _IssueDocumentation = "None";
        public string IssueDocumentation
        {
            get { return _IssueDocumentation; }
            set
            {
                if (this._IssueDocumentation != value)
                {
                    this._IssueDocumentation = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Properties not from Interface
        public string KeyStringResource { get; set; }
        public string[] ParamString { get; set; }
        public WindchillObject LinkedObj { get; set; }
        public string KeyString { get; set; }
        public EcnDataCheckItem CurrentDataCheckItem { get; set; }
        #endregion

        public string GetFullString()
        {
            try
            {
                return $"{ParentEcnDataCheckItem.EcnWtPart.Number} - {ParentEcnDataCheckItem.EcnWtPart.Revision} - {LinkedObjNumber} - {LinkedObjRevision} - {CurrentLink} - {Comments}";
            }
            catch (Exception ex)
            {
                throw new EcnDataCheckException(this.GetType().Name, ex);
            }
        }
    }
}
