using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.CREO_Tools.MassUpdateAttribute.View;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.WindchillRequestTool.Model.Windchill;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class CreateNewCadDocumentDataContext: ObservableObject, ICreateNewCadDocumentDataContext
    {
        #region [REGION] Properties from Interface

        /// <summary>
        /// The  property
        /// </summary>
        private bool _CreoIsEnable;
        /// <summary>
        /// Gets or sets the property
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public bool CreoIsEnable
        {
            get { return this._CreoIsEnable; }
            set
            {
                if (this._CreoIsEnable != value)
                {
                    this._CreoIsEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The  property
        /// </summary>
        private bool _PrtSelected = true;
        /// <summary>
        /// Gets or sets the property
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public bool PrtSelected
        {
            get { return this._PrtSelected; }
            set
            {
                if (this._PrtSelected != value)
                {
                    this._PrtSelected = value;
                    if (value) SelectedCadDocumentType = WindchillObjectType.PHYSICAL_PART;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The  property
        /// </summary>
        private bool _PrtSmSelected = false;
        /// <summary>
        /// Gets or sets the property
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public bool PrtSmSelected
        {
            get { return this._PrtSmSelected; }
            set
            {
                if (this._PrtSmSelected != value)
                {
                    this._PrtSmSelected = value;
                    if (value) SelectedCadDocumentType = WindchillObjectType.SHEETMETAL;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The  property
        /// </summary>
        private bool _AsmSelected = false;
        /// <summary>
        /// Gets or sets the property
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public bool AsmSelected
        {
            get { return this._AsmSelected; }
            set
            {
                if (this._AsmSelected != value)
                {
                    this._AsmSelected = value;
                    if (value) SelectedCadDocumentType = WindchillObjectType.ASM;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The  property
        /// </summary>
        private bool _DrwSelected = false;
        /// <summary>
        /// Gets or sets the property
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public bool DrwSelected
        {
            get { return this._DrwSelected; }
            set
            {
                if (this._DrwSelected != value)
                {
                    this._DrwSelected = value;
                    if (value) SelectedCadDocumentType = WindchillObjectType.DRW;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The  property
        /// </summary>
        private string _PartNumber;
        /// <summary>
        /// Gets or sets the property
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
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

        /// <summary>
        /// Gets or sets the list webterm.
        /// </summary>
        /// <value>
        /// The list webterm.
        /// </value>
        public ObservableCollection<string> ListWebterm { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets the list webterm local.
        /// </summary>
        /// <value>
        /// The list webterm local.
        /// </value>
        public ObservableCollection<string> ListWebtermLocal { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// The  property
        /// </summary>
        private string _SelectedWebterm;
        /// <summary>
        /// Gets or sets the property
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public string SelectedWebterm
        {
            get { return this._SelectedWebterm; }
            set
            {
                if (this._SelectedWebterm != value)
                {
                    this._SelectedWebterm = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The  property
        /// </summary>
        private int _SelectedIndexLanguage;
        /// <summary>
        /// Gets or sets the property
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public int SelectedIndexLanguage
        {
            get { return this._SelectedIndexLanguage; }
            set
            {
                if (this._SelectedIndexLanguage != value)
                {
                    this._SelectedIndexLanguage = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The  property
        /// </summary>
        private Image _CurrentLanguage;
        /// <summary>
        /// Gets or sets the property
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public Image CurrentLanguage
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

        /// <summary>
        /// The  property
        /// </summary>
        private string _Description2_1;
        /// <summary>
        /// Gets or sets the property
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
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

        /// <summary>
        /// Gets or sets the list other attributes.
        /// </summary>
        /// <value>
        /// The list other attributes.
        /// </value>
        public List<McgAttributeColumnHeaderInfo> ListOtherAttributes { get; set; }
        #endregion

        #region [REGION] Properties not from interface
        /// <summary>
        /// Gets or sets the current language text.
        /// </summary>
        /// <value>
        /// The current language text.
        /// </value>
        public string CurrentLanguageText { get; set; } = "FRENCH";

        /// <summary>
        /// Gets or sets the type of the selected cad document.
        /// </summary>
        /// <value>
        /// The type of the selected cad document.
        /// </value>
        public WindchillObjectType SelectedCadDocumentType { get; set; } = WindchillObjectType.PHYSICAL_PART;
        #endregion
    }
}
