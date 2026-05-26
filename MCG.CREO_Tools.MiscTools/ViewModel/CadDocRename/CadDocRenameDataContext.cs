using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.CadDocRename;
using pfcls;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.CadDocRename
{
    public class CadDocRenameDataContext : ObservableObject, ICadDocRenameDataContext
    {
        #region [REGION] Properties from Interface
        private string _CadNumber;
        public string CadNumber
        {
            get { return _CadNumber; }
            set
            {
                if (this._CadNumber != value)
                {
                    this._CadNumber = value;
                    OnPropertyChanged();
                    RaiseCadNumberChangedEvent();
                }

            }
        }

        private bool _IsCreoConnected;
        public bool IsCreoConnected
        {
            get { return _IsCreoConnected; }
            set
            {
                if (this._IsCreoConnected != value)
                {
                    this._IsCreoConnected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsRenamedPossible = false;
        public bool IsRenamedPossible
        {
            get { return _IsRenamedPossible; }
            set
            {
                if (this._IsRenamedPossible != value)
                {
                    this._IsRenamedPossible = value;
                    OnPropertyChanged();
                }

            }
        }

        private int _SelectedLeadingZero = 3;
        public int SelectedLeadingZero
        {
            get { return _SelectedLeadingZero; }
            set
            {
                if (this._SelectedLeadingZero != value)
                {
                    this._SelectedLeadingZero = value;
                    OnPropertyChanged();
                    RaiseCadNumberChangedEvent();
                }
            }

        }

        public ObservableCollection<int> ListLeadingZero { get; set; } = new ObservableCollection<int>()
        {
            1,2,3,4,5
        };

        public ObservableCollection<CadDocRenameItem> ListItem { get; set; } = new ObservableCollection<CadDocRenameItem>();

        private int _NbModels;
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

        private int _NbModelsInProgress;
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

        private bool _IsPleaseWaitShown;
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
        #endregion

        #region [REGION] Internal variables
        public List<IpfcModel> AllCadModels { get; set; } = new List<IpfcModel>();
        #endregion

        #region [REGION] Events

        public event EventHandler CadNumberChangedEvent;

        public void RaiseCadNumberChangedEvent()
        {
            try
            {
                CadNumberChangedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}
