using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.Enums;
using MCG.CREO_Tools.CadDocQualityCheck.View;
using pfcls;

namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class CadDocLayerItem : ObservableObject, ICadDocLayerItem
    {
        #region [REGION] Properties from Interface
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    OnPropertyChanged();
                }

            }
        }

        private ObjectState _State = ObjectState.UNKNOWN;
        public ObjectState State
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

        private bool _IsDisplayed = true;
        public bool IsDisplayed
        {
            get { return _IsDisplayed; }
            set
            {
                if (this._IsDisplayed != value)
                {
                    this._IsDisplayed = value;
                    OnPropertyChanged();
                }

            }
        }

        private CadDocCheckStatus _LayerStatus = CadDocCheckStatus.UNKNOWN;
        public CadDocCheckStatus LayerStatus
        {
            get { return _LayerStatus; }
            set
            {
                if (this._LayerStatus != value)
                {
                    this._LayerStatus = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion


        #region [REGION] Internal variables
        public IpfcLayer LayerItem { get; set; } = null;

        public EpfcDisplayStatus DisplayStatus { get; set; } = EpfcDisplayStatus.EpfcDisplayStatus_nil;

        public List<IpfcModelItem> ListModelItems { get; set; }
        #endregion

        public override string ToString()
        {
            return Name;
        }
    }
}
