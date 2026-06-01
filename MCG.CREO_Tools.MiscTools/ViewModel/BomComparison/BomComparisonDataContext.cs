using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.BomComparison;
using MCG.WindchillRequestTool;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.BomComparison
{
    public class BomComparisonDataContext : ObservableObject, IBomComparisonDataContext
    {
        #region [REGION] Properties from Interface
        private bool _IsActionProgress = false;
        public bool IsActionProgress
        {
            get { return _IsActionProgress; }
            set
            {
                if (this._IsActionProgress != value)
                {
                    this._IsActionProgress = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsPartChecked = true;
        public bool IsPartChecked
        {
            get { return _IsPartChecked; }
            set
            {
                if (this._IsPartChecked != value)
                {
                    this._IsPartChecked = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsAssemblyChecked = false;
        public bool IsAssemblyChecked
        {
            get { return _IsAssemblyChecked; }
            set
            {
                if (this._IsAssemblyChecked != value)
                {
                    this._IsAssemblyChecked = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsLatestRevisionL = true;
        public bool IsLatestRevisionL
        {
            get { return _IsLatestRevisionL; }
            set
            {
                if (this._IsLatestRevisionL != value)
                {
                    this._IsLatestRevisionL = value;
                    OnPropertyChanged();
                    if (value)
                        RevisionL = "";
                }

            }
        }

        private bool _IsLatestIterationL = true;
        public bool IsLatestIterationL
        {
            get { return _IsLatestIterationL; }
            set
            {
                if (this._IsLatestIterationL != value)
                {
                    this._IsLatestIterationL = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsLatestRevisionR = true;
        public bool IsLatestRevisionR
        {
            get { return _IsLatestRevisionR; }
            set
            {
                if (this._IsLatestRevisionR != value)
                {
                    this._IsLatestRevisionR = value;
                    OnPropertyChanged();
                    if (value)
                        RevisionR = "";
                }

            }
        }

        private bool _IsLatestIterationR = true;
        public bool IsLatestIterationR
        {
            get { return _IsLatestIterationR; }
            set
            {
                if (this._IsLatestIterationR != value)
                {
                    this._IsLatestIterationR = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _NumberL;
        public string NumberL
        {
            get { return _NumberL; }
            set
            {
                if (this._NumberL != value)
                {
                    this._NumberL = value.ToUpper();
                    OnPropertyChanged();
                }

            }
        }

        private string _RevisionL;
        public string RevisionL
        {
            get { return _RevisionL; }
            set
            {
                if (this._RevisionL != value)
                {
                    this._RevisionL = value.ToUpper();
                    OnPropertyChanged();
                }

            }
        }

        private string _NumberR;
        public string NumberR
        {
            get { return _NumberR; }
            set
            {
                if (this._NumberR != value)
                {
                    this._NumberR = value.ToUpper();
                    OnPropertyChanged();
                }

            }
        }

        private string _RevisionR;
        public string RevisionR
        {
            get { return _RevisionR; }
            set
            {
                if (this._RevisionR != value)
                {
                    this._RevisionR = value.ToUpper();
                    OnPropertyChanged();
                }

            }
        }

        private string _StatusBarMsgL;
        public string StatusBarMsgL
        {
            get { return _StatusBarMsgL; }
            set
            {
                if (this._StatusBarMsgL != value)
                {
                    this._StatusBarMsgL = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _StatusBarMsgR;
        public string StatusBarMsgR
        {
            get { return _StatusBarMsgR; }
            set
            {
                if (this._StatusBarMsgR != value)
                {
                    this._StatusBarMsgR = value;
                    OnPropertyChanged();
                }

            }
        }

        //public ObservableCollection<WindchillObjStructureComponent> BomL { get; set; } = new ObservableCollection<WindchillObjStructureComponent>();
        //public ObservableCollection<WindchillObjStructureComponent> BomR { get; set; } = new ObservableCollection<WindchillObjStructureComponent>();

        private BomItem _BomL;
        public BomItem BomL
        {
            get { return _BomL; }
            set
            {
                if (this._BomL != value)
                {
                    this._BomL = value;
                    OnPropertyChanged();
                }

            }
        }

        private BomItem _BomR;
        public BomItem BomR
        {
            get { return _BomR; }
            set
            {
                if (this._BomR != value)
                {
                    this._BomR = value;
                    OnPropertyChanged();
                }

            }
        }



        private int _BomLevel = 1;
        public int BomLevel
        {
            get { return _BomLevel; }
            set
            {
                if (this._BomLevel != value)
                {
                    this._BomLevel = value;
                    OnPropertyChanged();
                }

            }
        }

        private int _MaxBomLevel;
        public int MaxBomLevel
        {
            get { return this._MaxBomLevel; }
            set
            {
                if (this._MaxBomLevel != value)
                {
                    this._MaxBomLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsShowOccurrences = false;
        public bool IsShowOccurrences
        {
            get { return this._IsShowOccurrences; }
            set
            {
                if (this._IsShowOccurrences != value)
                {
                    this._IsShowOccurrences = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NumericalLineNumberDigit = 4;
        public int NumericalLineNumberDigit
        {
            get { return _NumericalLineNumberDigit; }
            set
            {
                if (this._NumericalLineNumberDigit != value)
                {
                    this._NumericalLineNumberDigit = value;
                    OnPropertyChanged();
                    RaiseNumericalLineNumberDigitEvent();
                }
            }
        }

        private BomComparisonItem _BomComparison;
        public BomComparisonItem BomComparison
        {
            get { return _BomComparison; }
            set
            {
                if (this._BomComparison != value)
                {
                    this._BomComparison = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _SelectedBomFroml;
        public string SelectedBomFromL
        {
            get { return _SelectedBomFroml; }
            set
            {
                if (this._SelectedBomFroml != value)
                {
                    this._SelectedBomFroml = value;
                    OnPropertyChanged();
                    if(value != null && value=="PDM")
                    {
                        ShowPdmFieldsL = true;
                        ShowSapFieldsL = false;
                        if(SelectedBomFromR != null && SelectedBomFromR == "PDM")
                            ShowAsmRadioButton = true;
                        else
                        {
                            ShowAsmRadioButton = false;
                            IsPartChecked = true;
                        }
                    }
                    else if (value != null && value == "SAP")
                    {
                        ShowPdmFieldsL = false;
                        ShowSapFieldsL = true;
                        ShowAsmRadioButton = false;
                        IsPartChecked = true;
                    }
                    else
                    {
                        ShowPdmFieldsL = true;
                        ShowSapFieldsL = false;
                        if (SelectedBomFromR != null && SelectedBomFromR == "PDM")
                            ShowAsmRadioButton = true;
                        else
                        {
                            ShowAsmRadioButton = false;
                            IsPartChecked = true;
                        }
                    }
                }

            }
        }

        private string _SelectedBomFromR;
        public string SelectedBomFromR
        {
            get { return _SelectedBomFromR; }
            set
            {
                if (this._SelectedBomFromR != value)
                {
                    this._SelectedBomFromR = value;
                    OnPropertyChanged();
                    if (value != null && value == "PDM")
                    {
                        ShowPdmFieldsR = true;
                        ShowSapFieldsR = false;
                        if (SelectedBomFromR != null && SelectedBomFromR == "PDM")
                        {
                            ShowAsmRadioButton = true;
                        }
                        else
                        {
                            ShowAsmRadioButton = false;
                            IsPartChecked = true;
                        }
                    }
                    else if (value != null && value == "SAP")
                    {
                        ShowPdmFieldsR = false;
                        ShowSapFieldsR = true;
                        ShowAsmRadioButton = false;
                        IsPartChecked = true;
                    }
                    else
                    {
                        ShowPdmFieldsR = true;
                        ShowSapFieldsR = false;
                        if (SelectedBomFromR != null && SelectedBomFromR == "PDM")
                            ShowAsmRadioButton = true;
                        else
                        { 
                            ShowAsmRadioButton = false;
                            IsPartChecked = true;
                        }
                    }
                }

            }
        }

        public ObservableCollection<string> ListBomFrom { get; set; } = new ObservableCollection<string>(); 

        private string _SelectedSapPlantL;
        public string SelectedSapPlantL
        {
            get { return _SelectedSapPlantL; }
            set
            {
                if (this._SelectedSapPlantL != value)
                {
                    this._SelectedSapPlantL = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _SelectedSapPlantR;
        public string SelectedSapPlantR
        {
            get { return _SelectedSapPlantR; }
            set
            {
                if (this._SelectedSapPlantR != value)
                {
                    this._SelectedSapPlantR = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> ListSapPlant { get; set; } = new ObservableCollection<string>();

        private DateTime? _ValidityDateL = new DateTime(2099,01,01);
        public DateTime? ValidityDateL
        {
            get { return _ValidityDateL; }
            set
            {
                if (this._ValidityDateL != value)
                {
                    this._ValidityDateL = value;
                    OnPropertyChanged();
                }

            }
        }

        private DateTime? _ValidityDateR = new DateTime(2099, 01, 01);
        public DateTime? ValidityDateR
        {
            get { return _ValidityDateR; }
            set
            {
                if (this._ValidityDateR != value)
                {
                    this._ValidityDateR = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ShowPdmFieldsL = true;
        public bool ShowPdmFieldsL
        {
            get { return _ShowPdmFieldsL; }
            set
            {
                if (this._ShowPdmFieldsL != value)
                {
                    this._ShowPdmFieldsL = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ShowPdmFieldsR = true;
        public bool ShowPdmFieldsR
        {
            get { return _ShowPdmFieldsR; }
            set
            {
                if (this._ShowPdmFieldsR != value)
                {
                    this._ShowPdmFieldsR = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ShowSapFieldsL = false;
        public bool ShowSapFieldsL
        {
            get { return _ShowSapFieldsL; }
            set
            {
                if (this._ShowSapFieldsL != value)
                {
                    this._ShowSapFieldsL = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ShowSapFieldsR = false;
        public bool ShowSapFieldsR
        {
            get { return _ShowSapFieldsR; }
            set
            {
                if (this._ShowSapFieldsR != value)
                {
                    this._ShowSapFieldsR = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ShowAsmRadioButton = true;
        public bool ShowAsmRadioButton
        {
            get { return _ShowAsmRadioButton; }
            set
            {
                if (this._ShowAsmRadioButton != value)
                {
                    this._ShowAsmRadioButton = value;
                    OnPropertyChanged();
                }

            }
        }

        #endregion

        #region [REGION] Internal variables
        public bool IsSearchBomDoneL { get; set; } = false;
        public bool IsSearchBomDoneR { get; set; } = false;
        #endregion

        #region [REGION] Event

        public event EventHandler NumericalLineNumberDigitEvent;
        public void RaiseNumericalLineNumberDigitEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                NumericalLineNumberDigitEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}
