using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.CREO_Tools.MassUpdateAttribute.Exceptions;
using MCG.CREO_Tools.MassUpdateAttribute.View;
using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColor;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class MassUpdateAttributeDataContext : ObservableObject, IMassUpdateAttributeDataContext
    {
        #region [REGION] Properties from interface
        private bool _ShowActionButton = false;
        public bool ShowActionButton
        {
            get { return this._ShowActionButton; }
            set
            {
                if (this._ShowActionButton != value)
                {
                    this._ShowActionButton = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsSearchCadModelInProgress = false;
        public bool IsSearchCadModelInProgress
        {
            get { return this._IsSearchCadModelInProgress; }
            set
            {
                if (this._IsSearchCadModelInProgress != value)
                {
                    this._IsSearchCadModelInProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsOnlyDisplayedModels = false;
        public bool IsOnlyDisplayedModels
        {
            get { return this._IsOnlyDisplayedModels; }
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

        private bool _IsOnlyActiveModel = false;
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

        private bool _IsCheckedOutShown = true;
        public bool IsCheckedOutShown
        {
            get { return this._IsCheckedOutShown; }
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

        private bool _IsLocallyModifiedShown = false;
        public bool IsLocallyModifiedShown
        {
            get { return this._IsLocallyModifiedShown; }
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

        private bool _IsReadOnlyShown = false;
        public bool IsReadOnlyShown
        {
            get { return this._IsReadOnlyShown; }
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

        private bool _IsLoadedFromCreo = true;
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

        public ObservableCollection<MassUpdateAttributeItem> ShownCadModels { get; set; } = new ObservableCollection<MassUpdateAttributeItem>();

        private MassUpdateAttributeItem _SelectedItem;
        public MassUpdateAttributeItem SelectedItem
        {
            get { return this._SelectedItem; }
            set
            {
                if (this._SelectedItem != value)
                {
                    this._SelectedItem = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsAllSelected = false;
        public bool IsAllSelected
        {
            get { return this._IsAllSelected; }
            set
            {
                if (this._IsAllSelected != value)
                {
                    this._IsAllSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsAllSelectedRename = false;
        public bool IsAllSelectedRename
        {
            get { return _IsAllSelectedRename; }
            set
            {
                if (this._IsAllSelectedRename != value)
                {
                    this._IsAllSelectedRename = value;
                    OnPropertyChanged();
                }

            }
        }

        private int _SelectedIndex = 0;
        public int SelectedIndex
        {
            get { return this._SelectedIndex; }
            set
            {
                if (this._SelectedIndex != value)
                {
                    this._SelectedIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _TextStatusBar;
        public string TextStatusBar
        {
            get { return this._TextStatusBar; }
            set
            {
                if (this._TextStatusBar != value)
                {
                    this._TextStatusBar = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> ListLanguages { get; set; } = new ObservableCollection<string>();

        private string _CurrentLanguage;
        public string CurrentLanguage
        {
            get { return this._CurrentLanguage; }
            set
            {
                if (this._CurrentLanguage != value)
                {
                    this._CurrentLanguage = value;
                    OnPropertyChanged();
                }
            }
        }

        private long _NbModelsInSession;
        public long NbModelsInSession
        {
            get { return this._NbModelsInSession; }
            set
            {
                if (this._NbModelsInSession != value)
                {
                    this._NbModelsInSession = value;
                    OnPropertyChanged();
                }
            }
        }

        private long _NbModelsInSessionInProgress = 0;
        public long NbModelsInSessionInProgress
        {
            get { return this._NbModelsInSessionInProgress; }
            set
            {
                if (this._NbModelsInSessionInProgress != value)
                {
                    this._NbModelsInSessionInProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _MessageModelsInSessionInProgress;
        public string MessageModelsInSessionInProgress
        {
            get { return this._MessageModelsInSessionInProgress; }
            set
            {
                if (this._MessageModelsInSessionInProgress != value)
                {
                    this._MessageModelsInSessionInProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<McgAttributeColumnHeaderInfo> ListColumns { get; set; }

        public ObservableCollection<CadAutoColorCreoColor> ListCreoColor { get; set; } = new ObservableCollection<CadAutoColorCreoColor>();

        private CadAutoColorCreoColor _SelectedCreoColor;
        public CadAutoColorCreoColor SelectedCreoColor
        {
            get { return _SelectedCreoColor; }
            set
            {
                if (this._SelectedCreoColor != value)
                {
                    this._SelectedCreoColor = value;
                    OnPropertyChanged();
                }

            }
        }

        private CadAutoColorPalette _ColorPalette01;
        public CadAutoColorPalette ColorPalette01
        {
            get { return _ColorPalette01; }
            set
            {
                if (this._ColorPalette01 != value)
                {
                    this._ColorPalette01 = value;
                    OnPropertyChanged();
                }

            }
        }

        private CadAutoColorPalette _ColorPalette02;
        public CadAutoColorPalette ColorPalette02
        {
            get { return _ColorPalette02; }
            set
            {
                if (this._ColorPalette02 != value)
                {
                    this._ColorPalette02 = value;
                    OnPropertyChanged();
                }

            }
        }

        private CadAutoColorPalette _ColorPalette03;
        public CadAutoColorPalette ColorPalette03
        {
            get { return _ColorPalette03; }
            set
            {
                if (this._ColorPalette03 != value)
                {
                    this._ColorPalette03 = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<MassUpdateAttributeRenameItem> ListToBeRenamedObject { get; set; } = new ObservableCollection<MassUpdateAttributeRenameItem>();

        private MassUpdateAttributeRenameItem _SelectedRenameItem;
        public MassUpdateAttributeRenameItem SelectedRenameItem
        {
            get { return _SelectedRenameItem; }
            set
            {
                if (this._SelectedRenameItem != value)
                {
                    this._SelectedRenameItem = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> WebtermList { get; set; } = new ObservableCollection<string>();

        private string _NewTerm;
        public string NewTerm
        {
            get { return _NewTerm; }
            set
            {
                if (this._NewTerm != value)
                {
                    this._NewTerm = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> MaterialList { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListGroup { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListSubGroup { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListBrand { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListOption { get; set; } = new ObservableCollection<string>();

        private string _SelectedBrand;
        public string SelectedBrand
        {
            get { return _SelectedBrand; }
            set
            {
                if (this._SelectedBrand != value)
                {
                    this._SelectedBrand = value;
                    OnPropertyChanged();
                    RaiseUpdateBrandEvent();
                }

            }
        }

        private string _SelectedGroup;
        public string SelectedGroup
        {
            get { return _SelectedGroup; }
            set
            {
                if (this._SelectedGroup != value)
                {
                    this._SelectedGroup = value;
                    OnPropertyChanged();
                    RaiseUpdateGroupEvent();
                }

            }
        }

        private string _SelectedSubGroup;
        public string SelectedSubGroup
        {
            get { return _SelectedSubGroup; }
            set
            {
                if (this._SelectedSubGroup != value)
                {
                    this._SelectedSubGroup = value;
                    OnPropertyChanged();
                    RaiseUpdateSubGroupEvent();
                }

            }
        }

        private string _SelectedOption;
        public string SelectedOption
        {
            get { return _SelectedOption; }
            set
            {
                if (this._SelectedOption != value)
                {
                    this._SelectedOption = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Properties not from interface
        public List<MassUpdateAttributeItem> AllCadModels = new List<MassUpdateAttributeItem>();

        public bool IsTemplateInformationReaded { get; set; } = false;
        public List<string> TemplateMainRefPlans { get; set; }
        public List<string> TemplateMainCoordSystem { get; set; }

        public List<CadDocLayerItemConfig> MandatoryLayers { get; set; }
        #endregion

        #region [REGION] Event
        public event EventHandler UpdateBrandEvent;
        public void RaiseUpdateBrandEvent()
        {
            try
            {
                UpdateBrandEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UpdateGroupEvent;
        public void RaiseUpdateGroupEvent()
        {
            try
            {
                UpdateGroupEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UpdateSubGroupEvent;
        public void RaiseUpdateSubGroupEvent()
        {
            try
            {
                UpdateSubGroupEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        public void UpdateShownModelsList()
        {
            try
            {
                ShownCadModels.Clear();

                List<MassUpdateAttributeItem> ListToShow = new List<MassUpdateAttributeItem>();

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
                var ShownCadModelsSorted = ListToShow.OrderBy((item) => item.PartNumber).ToList();

                foreach (var elem in ShownCadModelsSorted)
                    ShownCadModels.Add(elem);
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }
    }
}
