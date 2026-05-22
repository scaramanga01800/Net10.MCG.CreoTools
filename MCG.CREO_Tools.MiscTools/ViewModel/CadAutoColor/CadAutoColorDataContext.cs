using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.CadAutoColor;
using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColor;
using pfcls;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColr
{
    public class CadAutoColorDataContext : ObservableObject, ICadAutoColorDataContext
    {
        #region [REGION] Properties from Interface
        public ObservableCollection<CadAutoColorCreoColor> ListCreoColor { get; set; } = new ObservableCollection<CadAutoColorCreoColor>();
        public ObservableCollection<CadAutoColorItem> ListItem { get; set; } = new ObservableCollection<CadAutoColorItem> { };
        public ObservableCollection<CadAutoColorItem> ListItemName { get; set; } = new ObservableCollection<CadAutoColorItem> { };
        public ObservableCollection<CadAutoColorItem> ListItemPart { get; set; } = new ObservableCollection<CadAutoColorItem> { };

        private string _SelectedCadDoc;
        public string SelectedCadDoc
        {
            get { return _SelectedCadDoc; }
            set
            {
                if (this._SelectedCadDoc != value)
                {
                    this._SelectedCadDoc = value;
                    OnPropertyChanged();
                }

            }
        }

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

        private int _NbModels = 1;
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

        private bool _IsAllPartSelected = false;
        public bool IsAllPartSelected
        {
            get { return _IsAllPartSelected; }
            set
            {
                if (this._IsAllPartSelected != value)
                {
                    this._IsAllPartSelected = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsAllPartSelectedName = false;
        public bool IsAllPartSelectedName
        {
            get { return _IsAllPartSelectedName; }
            set
            {
                if (this._IsAllPartSelectedName != value)
                {
                    this._IsAllPartSelectedName = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsAllPartSelectedPart = false;
        public bool IsAllPartSelectedPart
        {
            get { return _IsAllPartSelectedPart; }
            set
            {
                if (this._IsAllPartSelectedPart != value)
                {
                    this._IsAllPartSelectedPart = value;
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


        private TabItem _SelectedTab;
        public TabItem SelectedTab
        {
            get { return _SelectedTab; }
            set
            {
                if (this._SelectedTab != value)
                {
                    this._SelectedTab = value;
                    OnPropertyChanged();
                }
                UpdateCurrentList();
            }
        }

        #endregion

        #region [REGION] Internal variables
        public List<IpfcModel> AllCadModels { get; set; }
        public ObservableCollection<CadAutoColorItem> CurrentList { get; set; }
        #endregion

        #region [REGION] Misc
        private void UpdateCurrentList()
        {
            try
            {
                if (SelectedTab!=null)
                {
                    if (SelectedTab.Header.ToString() == McgWpfTools.GetStringResource("CAC_TabTitleMaterial"))
                        CurrentList = ListItem;
                    else if (SelectedTab.Header.ToString() == McgWpfTools.GetStringResource("CAC_TabTitlePtcCommonName"))
                        CurrentList = ListItemName;
                    else if (SelectedTab.Header.ToString() == McgWpfTools.GetStringResource("CAC_TabTitlePart"))
                        CurrentList = ListItemPart;
                    else
                        CurrentList = ListItem;
                }
            }            
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion


    }
}
