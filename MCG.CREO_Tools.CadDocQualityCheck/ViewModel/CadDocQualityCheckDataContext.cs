using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.CadDocQualityCheck.Exceptions;
using MCG.CREO_Tools.CadDocQualityCheck.View;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class CadDocQualityCheckDataContext : ObservableObject, ICadDocQualityCheckDataContext
    {
        #region [REGION] Properties from Interface
        private bool _ShowActionButton;
        public bool ShowActionButton
        {
            get { return _ShowActionButton; }
            set
            {
                if (this._ShowActionButton != value)
                {
                    this._ShowActionButton = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsSearchCadModelInProgress;
        public bool IsSearchCadModelInProgress
        {
            get { return _IsSearchCadModelInProgress; }
            set
            {
                if (this._IsSearchCadModelInProgress != value)
                {
                    this._IsSearchCadModelInProgress = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsOnlyDisplayedModels;
        public bool IsOnlyDisplayedModels
        {
            get { return _IsOnlyDisplayedModels; }
            set
            {
                if (this._IsOnlyDisplayedModels != value)
                {
                    this._IsOnlyDisplayedModels = value;
                    OnPropertyChanged();
                }
                if (value) IsOnlyActiveModel = false;

            }
        }

        private bool _IsOnlyActiveModel;
        public bool IsOnlyActiveModel
        {
            get { return _IsOnlyActiveModel; }
            set
            {
                if (this._IsOnlyActiveModel != value)
                {
                    this._IsOnlyActiveModel = value;
                    OnPropertyChanged();
                }
                if (value) IsOnlyDisplayedModels = false;

            }
        }

        private bool _IsLoadedFromCreo;
        public bool IsLoadedFromCreo
        {
            get { return _IsLoadedFromCreo; }
            set
            {
                if (this._IsLoadedFromCreo != value)
                {
                    this._IsLoadedFromCreo = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsCheckedOutShown = true;
        public bool IsCheckedOutShown
        {
            get { return _IsCheckedOutShown; }
            set
            {
                if (this._IsCheckedOutShown != value)
                {
                    this._IsCheckedOutShown = value;
                    OnPropertyChanged();
                    UpdateShownModelsList();
                }

            }
        }

        private bool _IsLocallyModifiedShown = true;
        public bool IsLocallyModifiedShown
        {
            get { return _IsLocallyModifiedShown; }
            set
            {
                if (this._IsLocallyModifiedShown != value)
                {
                    this._IsLocallyModifiedShown = value;
                    OnPropertyChanged();
                    UpdateShownModelsList();
                }

            }
        }

        private bool _IsReadOnlyShown = true;
        public bool IsReadOnlyShown
        {
            get { return _IsReadOnlyShown; }
            set
            {
                if (this._IsReadOnlyShown != value)
                {
                    this._IsReadOnlyShown = value;
                    OnPropertyChanged();
                    UpdateShownModelsList();
                }

            }
        }

        private bool _IsNoActionInProgress = true;
        public bool IsNoActionInProgress
        {
            get { return _IsNoActionInProgress; }
            set
            {
                if (this._IsNoActionInProgress != value)
                {
                    this._IsNoActionInProgress = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsCheckDone = false;
        public bool IsCheckDone
        {
            get { return _IsCheckDone; }
            set
            {
                if (this._IsCheckDone != value)
                {
                    this._IsCheckDone = value;
                    OnPropertyChanged();
                }

            }
        }


        public ObservableCollection<CadDocQualityCheckItem> ShownCadModels { get; set; } = new ObservableCollection<CadDocQualityCheckItem>();

        private CadDocQualityCheckItem _SelectedItem;
        public CadDocQualityCheckItem SelectedItem
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

        private int _SelectedIndex;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                if (this._SelectedIndex != value)
                {
                    this._SelectedIndex = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsAllSelected;
        public bool IsAllSelected
        {
            get { return _IsAllSelected; }
            set
            {
                if (this._IsAllSelected != value)
                {
                    this._IsAllSelected = value;
                    OnPropertyChanged();
                }

            }
        }


        private string _TextStatusBar;
        public string TextStatusBar
        {
            get { return _TextStatusBar; }
            set
            {
                if (this._TextStatusBar != value)
                {
                    this._TextStatusBar = value;
                    OnPropertyChanged();
                }

            }
        }

        private long _NbModelsInSession;
        public long NbModelsInSession
        {
            get { return _NbModelsInSession; }
            set
            {
                if (this._NbModelsInSession != value)
                {
                    this._NbModelsInSession = value;
                    OnPropertyChanged();
                }

            }
        }

        private long _NbModelsInSessionInProgress;
        public long NbModelsInSessionInProgress
        {
            get { return _NbModelsInSessionInProgress; }
            set
            {
                if (this._NbModelsInSessionInProgress != value)
                {
                    this._NbModelsInSessionInProgress = value;
                    OnPropertyChanged();
                }

            }
        }


        private bool _CheckUncheckedOutItem = false;
        public bool CheckUncheckedOutItem
        {
            get { return _CheckUncheckedOutItem; }
            set
            {
                if (this._CheckUncheckedOutItem != value)
                {
                    this._CheckUncheckedOutItem = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ForceTypeProeUpdate = false;
        public bool ForceTypeProeUpdate
        {
            get { return _ForceTypeProeUpdate; }
            set
            {
                if (this._ForceTypeProeUpdate != value)
                {
                    this._ForceTypeProeUpdate = value;
                    OnPropertyChanged();
                }

            }
        }

        #endregion

        #region [REGION] Internal variables
        public List<CadDocQualityCheckItem> AllCadModels = new List<CadDocQualityCheckItem>();

        public bool IsTemplateInformationReaded { get; set; } = false;
        public bool IsTemplateAttributesReaded { get; set; } = false;
        public bool IsTemplateMainRefReaded { get; set; } = false;

        public List<CadDocLayerItemConfig> MandatoryLayers { get; set; } 

        public List<CadDocTemplate> ListTemplate { get; set; }

        #endregion

        #region [REGION] Misc
        public void UpdateShownModelsList()
        {
            try
            {
                ShownCadModels.Clear();

                List<CadDocQualityCheckItem> ListToShow = new List<CadDocQualityCheckItem>();

                foreach (var elem in AllCadModels)
                {
                    if (elem.FromExcelImport)
                        ListToShow.Add(elem);
                    else
                    {
                        if (IsCheckedOutShown & elem.IsCheckedOut)
                            ListToShow.Add(elem);
                        else if (IsLocallyModifiedShown & elem.IsLocallyModified)
                            ListToShow.Add(elem);
                        else if (IsReadOnlyShown & elem.IsReadOnly)
                            ListToShow.Add(elem);
                    }
                }

                // Sort CadModel by partNumber
                var ShownCadModelsSorted = ListToShow.OrderBy((item) => item.Number).ToList();

                foreach (var elem in ShownCadModelsSorted)
                    ShownCadModels.Add(elem);
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        #endregion
    }
}
