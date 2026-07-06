using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.CadDocQualityCheck.View;

namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class CadDocQualityCheckResultItem: ObservableObject, ICadDocQualityCheckResultItem
    {
        #region [REGION] Properties from Interface
        private CadDocQualityCheckItem _ParentQualityCheckItem;
        public CadDocQualityCheckItem ParentQualityCheckItem
        {
            get { return _ParentQualityCheckItem; }
            set
            {
                if (this._ParentQualityCheckItem != value)
                {
                    this._ParentQualityCheckItem = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Comments;
        public string Comments
        {
            get { return _Comments; }
            set
            {
                if (this._Comments != value)
                {
                    this._Comments = value;
                    OnPropertyChanged();
                }

            }
        }

        private CadDocCheckStatus _Status;
        public CadDocCheckStatus Status
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
        #endregion

        #region [REGION] Internal variables
        public string KeyString { get; set; }
        public string[] ParamString { get; set; }
        #endregion

        #region [REGION] Misc
        #endregion
    }
}
