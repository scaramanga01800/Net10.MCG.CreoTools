using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.ShearedTube.Exceptions;
using MCG.CREO_Tools.ShearedTube.View;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.ShearedTube.ViewModel
{
    public class ShearedTubeDataContext : ObservableObject, IShearedTubeDataContext
    {
        #region [REGION] Properties from Interface
        private bool _IsCreoEnable = false;
        public bool IsCreoEnable
        {
            get { return this._IsCreoEnable; }
            set
            {
                if (this._IsCreoEnable != value)
                {
                    this._IsCreoEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsHoleSelected = false;
        public bool IsHoleSelected
        {
            get { return this._IsHoleSelected; }
            set
            {
                if (this._IsHoleSelected != value)
                {
                    this._IsHoleSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _HoleDiameter = "7";
        public string HoleDiameter
        {
            get { return this._HoleDiameter; }
            set
            {
                if (this._HoleDiameter != value)
                {
                    this._HoleDiameter = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _HoleLength = "12";
        public string HoleLength
        {
            get { return this._HoleLength; }
            set
            {
                if (this._HoleLength != value)
                {
                    this._HoleLength = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ExtremityAngle = "0";
        public string ExtremityAngle
        {
            get { return this._ExtremityAngle; }
            set
            {
                if (this._ExtremityAngle != value)
                {
                    this._ExtremityAngle = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<double> ListThickness { get; set; } = new ObservableCollection<double>();
        public ObservableCollection<double> ListDiameter { get; set; } = new ObservableCollection<double>();

        private double _SelectedThickness;
        public double SelectedThickness
        {
            get { return this._SelectedThickness; }
            set
            {
                if (this._SelectedThickness != value)
                {
                    this._SelectedThickness = value;
                }
                OnPropertyChanged();
                UpdateDescription();
            }
        }

        private double _SelectedDiameter;
        public double SelectedDiameter
        {
            get { return this._SelectedDiameter; }
            set
            {
                if (this._SelectedDiameter != value)
                {
                    this._SelectedDiameter = value;
                    OnPropertyChanged();
                    UpdateListThickness();
                }
                UpdateDescription();
            }
        }

        private string _LeftAngle = "45";
        public string LeftAngle
        {
            get { return this._LeftAngle; }
            set
            {
                if (this._LeftAngle != value)
                {
                    this._LeftAngle = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _RightAngle = "45";
        public string RightAngle
        {
            get { return this._RightAngle; }
            set
            {
                if (this._RightAngle != value)
                {
                    this._RightAngle = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _TotalLength = "100";
        public string TotalLength
        {
            get { return this._TotalLength; }
            set
            {
                if (this._TotalLength != value)
                {
                    this._TotalLength = value;
                    OnPropertyChanged();
                }
                UpdateDescription();
            }
        }

        private string _Number;
        public string Number
        {
            get { return this._Number; }
            set
            {
                if (this._Number != value)
                {
                    this._Number = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _PtcCommonName = "TUBE";
        public string PtcCommonName
        {
            get { return this._PtcCommonName; }
            set
            {
                if (this._PtcCommonName != value)
                {
                    this._PtcCommonName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Description_2;
        public string Description_2
        {
            get { return this._Description_2; }
            set
            {
                if (this._Description_2 != value)
                {
                    this._Description_2 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Description2_1 = "TUBE ROND";
        public string Description2_1
        {
            get { return this._Description2_1; }
            set
            {
                if (this._Description2_1 != value)
                {
                    this._Description2_1 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Description2_2;
        public string Description2_2
        {
            get { return this._Description2_2; }
            set
            {
                if (this._Description2_2 != value)
                {
                    this._Description2_2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> ListGroupCreator { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListQualInspGroup { get; set; } = new ObservableCollection<string>();

        private string _SelectedGroupCreator;
        public string SelectedGroupCreator
        {
            get { return this._SelectedGroupCreator; }
            set
            {
                if (this._SelectedGroupCreator != value)
                {
                    this._SelectedGroupCreator = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _SelectedQualInspGroup;
        public string SelectedQualInspGroup
        {
            get { return this._SelectedQualInspGroup; }
            set
            {
                if (this._SelectedQualInspGroup != value)
                {
                    this._SelectedQualInspGroup = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region [REGION] Properties not from interface
        private List<ShearedTubeItem> _CompleteListTube;
        public List<ShearedTubeItem> CompleteListTube
        {
            get { return _CompleteListTube; }
            set
            {
                _CompleteListTube = value;
                UpdateListDiameter();
            }
        }
        #endregion

        #region [REGION] Misc Methods
        private void UpdateListDiameter()
        {
            try
            {
                ListDiameter.Clear();
                if (CompleteListTube != null)
                    foreach (double value in CompleteListTube.Select((elem) => elem.Diameter).Distinct().OrderBy((elem) => elem))
                        ListDiameter.Add(value);
            }
            catch (Exception ex)
            {
                throw new ShearedTubeException(this.GetType().Name, ex);
            }
        }

        private void UpdateListThickness()
        {
            try
            {
                ListThickness.Clear();
                foreach (double value in CompleteListTube.Where((elem) => elem.Diameter == SelectedDiameter).Select((elem) => elem.Thickness).Distinct().OrderBy((elem) => elem))
                    ListThickness.Add(value);
                SelectedThickness = ListThickness.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new ShearedTubeException(this.GetType().Name, ex);
            }
        }

        private void UpdateDescription()
        {
            try
            {
                Description_2 = $"D{SelectedDiameter}X{SelectedThickness} LG.{TotalLength}";
                Description2_2 = Description_2;
            }
            catch (Exception ex)
            {
                throw new ShearedTubeException(this.GetType().Name, ex);
            }
        }
        #endregion

    }
}
