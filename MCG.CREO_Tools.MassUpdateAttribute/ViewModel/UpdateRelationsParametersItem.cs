using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MassUpdateAttribute.View;
using pfcls;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    #region [REGION] Properties from Interface
    public class UpdateRelationsParametersItem : ObservableObject, IUpdateRelationsParametersItem
    {
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
        public IpfcModel CadModel { get; set; }
        public bool IsModifiable { get; set; } = true;
        #endregion

        #region [REGION] Misc functions
        #endregion
    }
}
