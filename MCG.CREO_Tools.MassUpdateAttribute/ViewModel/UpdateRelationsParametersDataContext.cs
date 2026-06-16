using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MassUpdateAttribute.View;
using pfcls;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class UpdateRelationsParametersDataContext : ObservableObject, IUpdateRelationsParametersDataContext
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

        private bool _IsUpperLevelSelected = true;
        public bool IsUpperLevelSelected
        {
            get { return _IsUpperLevelSelected; }
            set
            {
                if (this._IsUpperLevelSelected != value)
                {
                    this._IsUpperLevelSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsOneLevelSelected = false;
        public bool IsOneLevelSelected
        {
            get { return _IsOneLevelSelected; }
            set
            {
                if (this._IsOneLevelSelected != value)
                {
                    this._IsOneLevelSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsAllLevelsSelected = false;
        public bool IsAllLevelsSelected
        {
            get { return _IsAllLevelsSelected; }
            set
            {
                if (this._IsAllLevelsSelected != value)
                {
                    this._IsAllLevelsSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<UpdateRelationsParametersItem> ListItem { get; set; } = new ObservableCollection<UpdateRelationsParametersItem>();
        #endregion

        #region [REGION] Internal variables
        public IpfcModel ActiveModel { get; set; }
        #endregion

        #region [REGION] Misc functions
        #endregion
    }
}