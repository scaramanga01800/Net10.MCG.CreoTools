using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.QuickChange;
using pfcls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCG.CREO_Tools.MiscTools.ViewModel.QuickChange
{
    public class QuickChangeItem : ObservableObject, IQuickChangeItem
    {
        #region [REGION] Properties from Interface
        private string _CurrentNumber;
        public string CurrentNumber
        {
            get { return _CurrentNumber; }
            set
            {
                if (this._CurrentNumber != value)
                {
                    this._CurrentNumber = value;
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

        private int _Level = 1;
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

        private int _NbInstance = 1;
        public int NbInstance
        {
            get { return _NbInstance; }
            set
            {
                if (this._NbInstance != value)
                {
                    this._NbInstance = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _ParentNumber;
        public string ParentNumber
        {
            get { return _ParentNumber; }
            set
            {
                if (this._ParentNumber != value)
                {
                    this._ParentNumber = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public IpfcModel CadModel { get; set; }
        public IpfcModel ParentCadModel { get; set; }
        public List<IpfcFeature> ListFeature { get; set; }
        #endregion
    }
}
