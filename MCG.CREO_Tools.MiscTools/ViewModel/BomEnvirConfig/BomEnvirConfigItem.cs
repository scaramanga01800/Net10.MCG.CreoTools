using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.BomEnvirConfig;
using pfcls;

namespace MCG.CREO_Tools.MiscTools.ViewModel.BomEnvirConfig
{
    public class BomEnvirConfigItem: ObservableObject, IBomEnvirConfigItem
    {
        #region [REGION] Properties from Interface
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

        private string _AsmName;
        public string AsmName
        {
            get { return _AsmName; }
            set
            {
                if (this._AsmName != value)
                {
                    this._AsmName = value;
                    OnPropertyChanged();
                }
            }
        }   

        private string _OldAsmName;
        public string OldAsmName
        {
            get { return _OldAsmName; }
            set
            {
                if (this._OldAsmName != value)
                {
                    this._OldAsmName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Rep;
        public string Rep
        {
            get { return _Rep; }
            set
            {
                if (this._Rep != value)
                {
                    this._Rep = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _RepFct;
        public string RepFct
        {
            get { return _RepFct; }
            set
            {
                if (this._RepFct != value)
                {
                    this._RepFct = value;
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

        private int _Level;
        public int Level
        {
            get { return _Level; }
            set
            {
                if (this._Level != value)
                {
                    this._Level = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _CompOrder;
        public int CompOrder
        {
            get { return _CompOrder; }
            set
            {
                if (this._CompOrder != value)
                {
                    this._CompOrder = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region [REGION] Internal variables
        public IpfcFeature CreoFeature { get; set; }
        #endregion

        #region [REGION] Miscellaneous methods
        #endregion
    }
}
