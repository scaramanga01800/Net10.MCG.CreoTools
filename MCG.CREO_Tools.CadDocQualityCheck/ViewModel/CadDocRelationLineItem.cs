using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.CadDocQualityCheck.View;

namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class CadDocRelationLineItem : ObservableObject, ICadDocRelationLineItem
    {
        #region [REGION] Properties from Interface
        private string _Relation;
        public string Relation
        {
            get { return _Relation; }
            set
            {
                if (this._Relation != value)
                {
                    this._Relation = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsExtra = false;
        public bool IsExtra
        {
            get { return _IsExtra; }
            set
            {
                if (this._IsExtra != value)
                {
                    this._IsExtra = value;
                    OnPropertyChanged();
                    if (value) IsOk = false;
                }

            }
        }

        private bool _IsMissing = false;
        public bool IsMissing
        {
            get { return _IsMissing; }
            set
            {
                if (this._IsMissing != value)
                {
                    this._IsMissing = value;
                    OnPropertyChanged();
                    if (value) IsOk = false;
                }

            }
        }

        private bool _IsOk = true;
        public bool IsOk
        {
            get { return _IsOk; }
            set
            {
                if (this._IsOk != value)
                {
                    this._IsOk = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        #endregion

        #region [REGION] Misc
        #endregion
    }
}
