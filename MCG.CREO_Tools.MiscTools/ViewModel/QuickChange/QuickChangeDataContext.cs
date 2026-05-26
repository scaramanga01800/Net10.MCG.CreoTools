using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.QuickChange;
using pfcls;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.QuickChange
{
    public class QuickChangeDataContext: ObservableObject, IQuickChangeDataContext
    {
        #region [REGION] Properties from Interface
        private bool _IsCreoEnable;
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

        private bool _AllLevel = false;
        public bool AllLevel
        {
            get { return _AllLevel; }
            set
            {
                if (this._AllLevel != value)
                {
                    this._AllLevel = value;
                    OnPropertyChanged();
                }

            }
        }
        public ObservableCollection<QuickChangeItem> ListItem { get; set; } = new ObservableCollection<QuickChangeItem>();
        #endregion

        #region [REGION] Internal variables
        public List<IpfcModel> AllCadModels { get; set; }
        #endregion
    }
}
