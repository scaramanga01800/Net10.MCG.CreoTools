using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.CadDocRename;
using pfcls;

namespace MCG.CREO_Tools.MiscTools.ViewModel.CadDocRename
{
    public class CadDocRenameItem: ObservableObject, ICadDocRenameItem
    {
        #region [REGION] Properties from Interface
        private string _OldNumber;
        public string OldNumber
        {
            get { return _OldNumber; }
            set
            {
                if (this._OldNumber != value)
                {
                    this._OldNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _NewNumber;
        public string NewNumber
        {
            get { return _NewNumber; }
            set
            {
                if (this._NewNumber != value)
                {
                    this._NewNumber = value;
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
        #endregion

        #region [REGION] Internal variables
        public IpfcModel CreoModel { get; set; }
        public bool IsRenamed { get; set; } = false;
        #endregion

        #region [REGION] Misc Methods
        #endregion
    }
}
