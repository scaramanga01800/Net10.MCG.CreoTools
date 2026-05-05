using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.EcnEcoFollowUp.Interfaces.Models;

namespace MCG.Tools.EcnEcoFollowUp.Models
{
    public class EFU_SapHupOracle_DmEcoTasks: ObservableObject, IEFU_SapHupOracle_DmEcoTasks
    {
        #region [REGION] Properties from Interface
        private string _TYPE_ITEM = string.Empty;
        public string TYPE_ITEM
        {
            get { return this._TYPE_ITEM; }
            set
            {
                if (this._TYPE_ITEM != value)
                {
                    this._TYPE_ITEM = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _WI_STATUS = string.Empty;
        public string WI_STATUS
        {
            get { return this._WI_STATUS; }
            set
            {
                if (this._WI_STATUS != value)
                {
                    this._WI_STATUS = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _CALCULATED_PLANT_DESC = string.Empty;
        public string CALCULATED_PLANT_DESC
        {
            get { return this._CALCULATED_PLANT_DESC; }
            set
            {
                if (this._CALCULATED_PLANT_DESC != value)
                {
                    this._CALCULATED_PLANT_DESC = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _WI_ACTUAL_AGENT = string.Empty;
        public string WI_ACTUAL_AGENT
        {
            get { return this._WI_ACTUAL_AGENT; }
            set
            {
                if (this._WI_ACTUAL_AGENT != value)
                {
                    this._WI_ACTUAL_AGENT = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _WI_CREATION_DATE;
        public DateTime? WI_CREATION_DATE
        {
            get { return this._WI_CREATION_DATE; }
            set
            {
                if (this._WI_CREATION_DATE != value)
                {
                    this._WI_CREATION_DATE = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _WI_END_DATE;
        public DateTime? WI_END_DATE
        {
            get { return this._WI_END_DATE; }
            set
            {
                if (this._WI_END_DATE != value)
                {
                    this._WI_END_DATE = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region [REGION] Properties not from interface
        public string ECO { get; set; } = string.Empty;
        public string ECO_COORD { get; set; } = string.Empty;
        public string ECO_COORD_DESC { get; set; } = string.Empty;
        public string WI_TEXT { get; set; } = string.Empty;
        public string CALCULATED_PLANT { get; set; } = string.Empty;
        #endregion
    }
}
