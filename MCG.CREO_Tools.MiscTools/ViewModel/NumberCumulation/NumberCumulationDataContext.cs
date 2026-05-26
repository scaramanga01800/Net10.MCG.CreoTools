using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.NumberCumulation;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.NumberCumulation
{
    public class NumberCumulationDataContext : ObservableObject, INumberCumulationDataContext
    {
        #region [REGION] Properties from Interface
        private string _CumulNumberOnly;
        public string CumulNumberOnly
        {
            get { return _CumulNumberOnly; }
            set
            {
                if (this._CumulNumberOnly != value)
                {
                    this._CumulNumberOnly = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _CumulNumberSuf;
        public string CumulNumberSuf
        {
            get { return _CumulNumberSuf; }
            set
            {
                if (this._CumulNumberSuf != value)
                {
                    this._CumulNumberSuf = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _CumulNumberPre;
        public string CumulNumberPre
        {
            get { return _CumulNumberPre; }
            set
            {
                if (this._CumulNumberPre != value)
                {
                    this._CumulNumberPre = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _CumulNumberSufPre;
        public string CumulNumberSufPre
        {
            get { return _CumulNumberSufPre; }
            set
            {
                if (this._CumulNumberSufPre != value)
                {
                    this._CumulNumberSufPre = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<NumberCumulationItem> ListNumbers { get; set; } =  new ObservableCollection<NumberCumulationItem>();

        private NumberCumulationItem _SelectedItem;
        public NumberCumulationItem SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (this._SelectedItem != value)
                {
                    this._SelectedItem = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion
    }
}
