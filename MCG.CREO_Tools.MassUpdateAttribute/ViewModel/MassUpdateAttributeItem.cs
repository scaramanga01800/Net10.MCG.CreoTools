using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.Models.Main;
using MCG.CREO_Tools.MassUpdateAttribute.Exceptions;
using MCG.CREO_Tools.MassUpdateAttribute.View;
using MCG.WindchillRequestTool.Model.Windchill;
using pfcls;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class MassUpdateAttributeItem : ObservableObject, IMassUpdateAttributeItem
    {
        #region [REGION] Properties from Interface
        private bool _IsSelected;
        public bool IsSelected
        {
            get { return this._IsSelected; }
            set
            {
                if (this._IsSelected != value)
                {
                    this._IsSelected = value;
                    OnPropertyChanged();
                    RaiseIsSelectedEvent();
                }
            }
        }

        private string _PartNumber;
        public string PartNumber
        {
            get { return this._PartNumber; }
            set
            {
                if (this._PartNumber != value)
                {
                    this._PartNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _PTC_COMMON_NAME;
        public string PTC_COMMON_NAME
        {
            get { return this._PTC_COMMON_NAME; }
            set
            {
                if (this._PTC_COMMON_NAME != value)
                {
                    this._PTC_COMMON_NAME = value;
                    UpdatedStringMassUpdateAttributeValue("PTC_COMMON_NAME");
                    OnPropertyChanged();
                }
            }
        }

        private string _BasePartNumber;
        public string BasePartNumber
        {
            get { return this._BasePartNumber; }
            set
            {
                if (this._BasePartNumber != value)
                {
                    this._BasePartNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Status = "Not updated";
        public string Status
        {
            get { return _Status; }
            set
            {
                if (this._Status != value)
                {
                    this._Status = value;
                    OnPropertyChanged();
                }

            }
        }


        private bool _IsUpdated = false;
        public bool IsUpdated
        {
            get { return this._IsUpdated; }
            set
            {
                if (this._IsUpdated != value)
                {
                    this._IsUpdated = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsPtcCommonNameModifiable = false;
        public bool IsPtcCommonNameModifiable
        {
            get { return this._IsPtcCommonNameModifiable; }
            set
            {
                if (this._IsPtcCommonNameModifiable != value)
                {
                    this._IsPtcCommonNameModifiable = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsBasePartNumberFound = true;
        public bool IsBasePartNumberFound
        {
            get { return this._IsBasePartNumberFound; }
            set
            {
                if (this._IsBasePartNumberFound != value)
                {
                    this._IsBasePartNumberFound = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsCheckedIn = false;
        public bool IsCheckedIn
        {
            get { return this._IsCheckedIn; }
            set
            {
                if (this._IsCheckedIn != value)
                {
                    this._IsCheckedIn = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsCheckedOut = false;
        public bool IsCheckedOut
        {
            get { return this._IsCheckedOut; }
            set
            {
                if (this._IsCheckedOut != value)
                {
                    this._IsCheckedOut = value;
                    OnPropertyChanged();
                }
                IsCheckedIn = !IsCheckedOut;
            }
        }

        private bool _IsLocallyModified = false;
        public bool IsLocallyModified
        {
            get { return this._IsLocallyModified; }
            set
            {
                if (this._IsLocallyModified != value)
                {
                    this._IsLocallyModified = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsReadOnly = false;
        public bool IsReadOnly
        {
            get { return this._IsReadOnly; }
            set
            {
                if (this._IsReadOnly != value)
                {
                    this._IsReadOnly = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> WebtermList { get; set; } = new ObservableCollection<string>();

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
                //if (this._SelectedBrand != value)
                //{
                this._SelectedBrand = value;
                UpdatedStringMassUpdateAttributeValue("SelectedBrand");
                OnPropertyChanged();
                if (!IsUpdateInProgress)
                {
                    RaiseUpdateBrandEvent();
                    UpdateGroups();
                }
                //}

            }
        }

        private string _SelectedGroup = "-";
        public string SelectedGroup
        {
            get { return _SelectedGroup; }
            set
            {
                //if (this._SelectedGroup != value)
                //{
                this._SelectedGroup = value;
                UpdatedStringMassUpdateAttributeValue("SelectedGroup");
                OnPropertyChanged();
                if (!IsUpdateInProgress)
                {
                    RaiseUpdateGroupEvent();
                    UpdateSubGroups();
                }
                //}

            }
        }

        private string _SelectedSubGroup = "-";
        public string SelectedSubGroup
        {
            get { return _SelectedSubGroup; }
            set
            {
                //if (this._SelectedSubGroup != value)
                //{
                this._SelectedSubGroup = value;
                UpdatedStringMassUpdateAttributeValue("SelectedSubGroup");
                OnPropertyChanged();
                if (!IsUpdateInProgress)
                {
                    RaiseUpdateSubGroupEvent();
                    UpdateOptions();
                }
                //}

            }
        }

        private string _SelectedOption = "-";
        public string SelectedOption
        {
            get { return _SelectedOption; }
            set
            {
                //if (this._SelectedOption != value)
                //{
                this._SelectedOption = value;
                UpdatedStringMassUpdateAttributeValue("SelectedOption");
                OnPropertyChanged();
                //}

            }
        }

        #endregion

        #region [REGION] Events
        public event EventHandler IsSelectedEvent;
        public void RaiseIsSelectedEvent()
        {
            try
            {
                IsSelectedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

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

        #region [REGION] Properties not from Interface
        public IpfcModel CurrentCadModel { get; set; }

        public ObservableCollection<MassUpdateAttributeValue> ListAttribute = new ObservableCollection<MassUpdateAttributeValue>();

        public WindchillObjectNumberTemplate BasePartNumberTemplate { get; set; }

        public WindchillCheckedObject CurrentWindchillCheckedObject { get; set; }

        public bool IsModifiable { get; set; } = true;

        public bool FromExcelImport { get; set; } = false;

        public bool IsUpdateInProgress { get; set; } = false;

        private List<BrandGroupSubGroupItem> _ListBrandGroupSubGroup;
        public List<BrandGroupSubGroupItem> ListBrandGroupSubGroup
        {
            get { return _ListBrandGroupSubGroup; }
            set
            {
                if (this._ListBrandGroupSubGroup != value)
                {
                    this._ListBrandGroupSubGroup = value;
                    OnPropertyChanged();
                    //ListBrandGroupSubGroup = McgMiscTools.GetLIstBrandGroupSubGroup();
                    ListBrand.Clear();
                    var brands = ListBrandGroupSubGroup.Select(i => i.Brand).Distinct();
                    foreach (var brand in brands)
                    {
                        ListBrand.Add(brand);
                    }
                    SelectedBrand = ListBrand.FirstOrDefault();
                    UpdateGroups();
                }
            }
        }
        #endregion

        #region [REGION] Properties not from Interface
        private string _PARAMSTR01;
        public string PARAMSTR01
        {
            get
            {
                return _PARAMSTR01;
            }
            set
            {
                // if (value != null) value = value.Trim();
                _PARAMSTR01 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR01");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR02;
        public string PARAMSTR02
        {
            get
            {
                return _PARAMSTR02;
            }
            set
            {
                // if (value != null) value = value.Trim();
                _PARAMSTR02 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR02");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR03;
        public string PARAMSTR03
        {
            get
            {
                return _PARAMSTR03;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR03 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR03");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR04;
        public string PARAMSTR04
        {
            get
            {
                return _PARAMSTR04;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR04 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR04");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR05;
        public string PARAMSTR05
        {
            get
            {
                return _PARAMSTR05;
            }
            set
            {
                // if (value != null) value = value.Trim();
                _PARAMSTR05 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR05");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR06;
        public string PARAMSTR06
        {
            get
            {
                return _PARAMSTR06;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR06 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR06");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR07;
        public string PARAMSTR07
        {
            get
            {
                return _PARAMSTR07;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR07 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR07");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR08;
        public string PARAMSTR08
        {
            get
            {
                return _PARAMSTR08;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR08 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR08");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR09;
        public string PARAMSTR09
        {
            get
            {
                return _PARAMSTR09;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR09 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR09");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR10;
        public string PARAMSTR10
        {
            get
            {
                return _PARAMSTR10;
            }
            set
            {
                // if (value != null) value = value.Trim();
                _PARAMSTR10 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR10");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR11;
        public string PARAMSTR11
        {
            get
            {
                return _PARAMSTR11;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR11 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR11");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR12;
        public string PARAMSTR12
        {
            get
            {
                return _PARAMSTR12;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR12 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR12");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR13;
        public string PARAMSTR13
        {
            get
            {
                return _PARAMSTR13;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR13 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR13");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR14;
        public string PARAMSTR14
        {
            get
            {
                return _PARAMSTR14;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR14 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR14");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR15;
        public string PARAMSTR15
        {
            get
            {
                return _PARAMSTR15;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR15 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR15");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR16;
        public string PARAMSTR16
        {
            get
            {
                return _PARAMSTR16;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR16 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR16");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR17;
        public string PARAMSTR17
        {
            get
            {
                return _PARAMSTR17;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR17 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR17");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR18;
        public string PARAMSTR18
        {
            get
            {
                return _PARAMSTR18;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR18 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR18");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR19;
        public string PARAMSTR19
        {
            get
            {
                return _PARAMSTR19;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR19 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR19");
                OnPropertyChanged();
            }
        }

        private string _PARAMSTR20;
        public string PARAMSTR20
        {
            get
            {
                return _PARAMSTR20;
            }
            set
            {
                //if (value != null) value = value.Trim();
                _PARAMSTR20 = value;
                UpdatedStringMassUpdateAttributeValue("PARAMSTR20");
                OnPropertyChanged();
            }
        }

        public List<CadDocLayerItem> ListLayers { get; set; } = new List<CadDocLayerItem>();
        public List<CREOCadModelItem> ListRefPlans { get; set; }
        public List<CREOCadModelItem> ListRefPoints { get; set; }
        public List<CREOCadModelItem> ListRefAxis { get; set; }
        public List<CREOCadModelItem> ListRefCSys { get; set; }
        #endregion


        /// <summary>
        /// Updateds the string mass update attribute value.
        /// </summary>
        /// <param name="pPropertyName">Name of the p property.</param>
        private void UpdatedStringMassUpdateAttributeValue(string pPropertyName)
        {
            try
            {
                string currentValue = null;
                if (this.GetType().GetProperty(pPropertyName) != null)
                {
                    currentValue = ((string)this.GetType().GetProperty(pPropertyName)?.GetValue(this))?.Trim();
                    MassUpdateAttributeValue CurrentMassUpdateAttributeValue = ListAttribute.FirstOrDefault((item) => item.ParentAttribute.ClassAttributeID == pPropertyName);
                    if (CurrentMassUpdateAttributeValue != null)
                    {
                        if (CurrentMassUpdateAttributeValue.ParentAttribute != null && CurrentMassUpdateAttributeValue.ParentAttribute.MaxCharacters > 0 && currentValue.Length > CurrentMassUpdateAttributeValue.ParentAttribute.MaxCharacters)
                            this.GetType().GetProperty(pPropertyName).SetValue(this, currentValue.Substring(0, CurrentMassUpdateAttributeValue.ParentAttribute.MaxCharacters));
                        CurrentMassUpdateAttributeValue.NewValue = currentValue;
                    }
                }
                var ListAttribUpdated = ListAttribute.Where((item) => item.IsUpdated).ToList();
                if (ListAttribUpdated.Count > 0)
                    IsUpdated = true;
                else
                    IsUpdated = false;
                if (FromExcelImport) IsUpdated = true;
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateGroups()
        {
            try
            {
                ListGroup.Clear();
                var groups = ListBrandGroupSubGroup.Where(i => i.Brand == SelectedBrand).Select(i => i.Group).Distinct();
                foreach (var group in groups)
                {
                    ListGroup.Add(group);
                }
                SelectedGroup = ListGroup.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateSubGroups()
        {
            try
            {
                ListSubGroup.Clear();

                var subGroups = ListBrandGroupSubGroup.Where(i => i.Brand == SelectedBrand && i.Group == SelectedGroup).Select(i => i.SubGroup).Distinct();
                foreach (var subGroup in subGroups)
                {
                    ListSubGroup.Add(subGroup);
                }
                SelectedSubGroup = ListSubGroup.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateOptions()
        {
            try
            {
                ListOption.Clear();

                var options = ListBrandGroupSubGroup.FirstOrDefault(i => i.Brand == SelectedBrand && i.Group == SelectedGroup && i.SubGroup == SelectedSubGroup)?.OptionList;
                if (options != null)
                    foreach (var option in options)
                    {
                        ListOption.Add(option);
                    }
                SelectedOption = ListOption.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }
    }
}
