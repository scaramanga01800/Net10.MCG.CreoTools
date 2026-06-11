using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CREO_Tools.JpgExport.View;

namespace MCG.CREO_Tools.JpgExport.ViewModel
{
    public class JpgExportItem : ObservableObject, IJpgExportItem
    {
        #region [REGION] Properties from Interface
        private string _Number;
        public string Number
        {
            get { return this._Number; }
            set
            {
                if (this._Number != value)
                {
                    this._Number = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Status;
        public string Status
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

        private string _Comment;
        public string Comment
        {
            get { return this._Comment; }
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

        #region [REGION] Properties not from interface
        public EPMDocument CurrentEpmDocument { get; set; }

        public bool JpgCreated { get; set; } = false;
        #endregion
    }
}
