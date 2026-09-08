using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.SimplifiedRep;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.SimplifiedRep
{
    public class SimplifiedRepDataContext : ObservableObject, ISimplifiedRepDataContext
    {
        #region [REGION] Properties from Interface
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

        private bool _IsAssemblyLoaded;
        public bool IsAssemblyLoaded
        {
            get { return _IsAssemblyLoaded; }
            set
            {
                if (this._IsAssemblyLoaded != value)
                {
                    this._IsAssemblyLoaded = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsSimpRepSelected;
        public bool IsSimpRepSelected
        {
            get { return _IsSimpRepSelected; }
            set
            {
                if (this._IsSimpRepSelected != value)
                {
                    this._IsSimpRepSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ActiveModelName = string.Empty;
        public string ActiveModelName
        {
            get { return _ActiveModelName; }
            set
            {
                if (this._ActiveModelName != value)
                {
                    this._ActiveModelName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _NewSimpRepName = string.Empty;
        public string NewSimpRepName
        {
            get { return _NewSimpRepName; }
            set
            {
                if (this._NewSimpRepName != value)
                {
                    this._NewSimpRepName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _SelectedSimpRepName = string.Empty;
        public string SelectedSimpRepName
        {
            get { return _SelectedSimpRepName; }
            set
            {
                if (this._SelectedSimpRepName != value)
                {
                    this._SelectedSimpRepName = value;
                    OnPropertyChanged();
                    IsSimpRepSelected = !string.IsNullOrWhiteSpace(value);
                }
            }
        }

        private string _SelectedDefaultRule = string.Empty;
        public string SelectedDefaultRule
        {
            get { return _SelectedDefaultRule; }
            set
            {
                if (this._SelectedDefaultRule != value)
                {
                    this._SelectedDefaultRule = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<string> _ListSimpRepName = new ObservableCollection<string>();
        public ObservableCollection<string> ListSimpRepName
        {
            get { return _ListSimpRepName; }
            set
            {
                if (this._ListSimpRepName != value)
                {
                    this._ListSimpRepName = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<string> _ListDefaultRule = new ObservableCollection<string>();
        public ObservableCollection<string> ListDefaultRule
        {
            get { return _ListDefaultRule; }
            set
            {
                if (this._ListDefaultRule != value)
                {
                    this._ListDefaultRule = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<SimplifiedRepComponentItem> _ListItem = new ObservableCollection<SimplifiedRepComponentItem>();
        public ObservableCollection<SimplifiedRepComponentItem> ListItem
        {
            get { return _ListItem; }
            set
            {
                if (this._ListItem != value)
                {
                    this._ListItem = value;
                    OnPropertyChanged();
                }
            }
        }

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
        #endregion
    }
}
