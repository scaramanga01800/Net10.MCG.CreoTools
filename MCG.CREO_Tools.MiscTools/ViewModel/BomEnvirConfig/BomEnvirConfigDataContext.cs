using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.BomEnvirConfig;
using pfcls;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.BomEnvirConfig
{
    public class BomEnvirConfigDataContext: ObservableObject, IBomEnvirConfigDataContext
    {
        #region [REGION] Properties from Interface
        private string _ActiveModelFileName;
        public string ActiveModelFileName
        {
            get { return _ActiveModelFileName; }
            set
            {
                if (this._ActiveModelFileName != value)
                {
                    this._ActiveModelFileName = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _CadDocType;
        public string CadDocType
        {
            get { return _CadDocType; }
            set
            {
                if (this._CadDocType != value)
                {
                    this._CadDocType = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsCreoEnable = false;
        public bool IsCreoEnable
        {
            get { return _IsCreoEnable; }
            set
            {
                if (this._IsCreoEnable != value)
                {
                    this._IsCreoEnable = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsPleaseWaitShown = false;
        public bool IsPleaseWaitShown
        {
            get { return _IsPleaseWaitShown; }
            set
            {
                if (this._IsPleaseWaitShown != value)
                {
                    this._IsPleaseWaitShown = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbModels = 0;
        public int NbModels
        {
            get { return _NbModels; }
            set
            {
                if (this._NbModels != value)
                {
                    this._NbModels = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbModelsInProgress = 0;
        public int NbModelsInProgress
        {
            get { return _NbModelsInProgress; }
            set
            {
                if (this._NbModelsInProgress != value)
                {
                    this._NbModelsInProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _AsmNameValue;
        public string AsmNameValue
        {
            get { return _AsmNameValue; }
            set
            {
                if (this._AsmNameValue != value)
                {
                    this._AsmNameValue = value;
                    OnPropertyChanged();
                    RaiseAsmNameChangedEvent();
                }
            }
        }

        public ObservableCollection<BomEnvirConfigItem> ListItem { get; set; } = new ObservableCollection<BomEnvirConfigItem>();
        #endregion

        #region [REGION] Internal variables
        public IpfcModel ActiveModel { get; set; }
        public List<IpfcModel> AllCadModels { get; set; }
        #endregion

        #region [REGION] Events
        public event EventHandler AsmNameChangedEvent;

        public void RaiseAsmNameChangedEvent()
        {
            try
            {
                AsmNameChangedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}
