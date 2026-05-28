using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.WebtermRequest;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.WebtermRequest
{
    public class WebtermRequestItem: ObservableObject, IWebtermRequestItem
    {
        private WebtermRequestClass _SelectedClass;
        public WebtermRequestClass SelectedClass
        {
            get { return _SelectedClass; }
            set
            {
                if (this._SelectedClass != value)
                {
                    this._SelectedClass = value;
                    OnPropertyChanged();
                }

            }
        }

        private int _SelectedClassIndex = 0;
        public int SelectedClassIndex
        {
            get { return _SelectedClassIndex; }
            set
            {
                if (this._SelectedClassIndex != value)
                {
                    this._SelectedClassIndex = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _QualInspGrp;
        public string QualInspGrp
        {
            get { return _QualInspGrp; }
            set
            {
                if (this._QualInspGrp != value)
                {
                    this._QualInspGrp = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _DefaulUnit;
        public string DefaulUnit
        {
            get { return _DefaulUnit; }
            set
            {
                if (this._DefaulUnit != value)
                {
                    this._DefaulUnit = value;
                    OnPropertyChanged();
                }

            }
        }

        private double _MinMass = 0.001;
        public double MinMass
        {
            get { return _MinMass; }
            set
            {
                if (this._MinMass != value)
                {
                    this._MinMass = value;
                    OnPropertyChanged();
                }

            }
        }

        private double _MaxMass = 100000;
        public double  MaxMass
        {
            get { return _MaxMass; }
            set
            {
                if (this._MaxMass != value)
                {
                    this._MaxMass = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _TermEn;
        public string TermEn
        {
            get { return _TermEn; }
            set
            {
                if (this._TermEn != value)
                {
                    this._TermEn = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _DescriptionEn;
        public string DescriptionEn
        {
            get { return _DescriptionEn; }
            set
            {
                if (this._DescriptionEn != value)
                {
                    this._DescriptionEn = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _TermUpperCaseEn;
        public string TermUpperCaseEn
        {
            get { return _TermUpperCaseEn; }
            set
            {
                if (this._TermUpperCaseEn != value)
                {
                    this._TermUpperCaseEn = value?.ToUpper();
                    OnPropertyChanged();
                }

            }
        }

        private string _TermAbbrevitationEn;
        public string TermAbbrevitationEn
        {
            get { return _TermAbbrevitationEn; }
            set
            {
                if (this._TermAbbrevitationEn != value)
                {
                    this._TermAbbrevitationEn = value?.ToUpper();
                    OnPropertyChanged();
                }

            }
        }

        private string _AttributeEn;
        public string AttributeEn
        {
            get { return _AttributeEn; }
            set
            {
                if (this._AttributeEn != value)
                {
                    this._AttributeEn = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _AttributeExampleEn;
        public string AttributeExampleEn
        {
            get { return _AttributeExampleEn; }
            set
            {
                if (this._AttributeExampleEn != value)
                {
                    this._AttributeExampleEn = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _TermFr;
        public string TermFr
        {
            get { return _TermFr; }
            set
            {
                if (this._TermFr != value)
                {
                    this._TermFr = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _DescriptionFr;
        public string DescriptionFr
        {
            get { return _DescriptionFr; }
            set
            {
                if (this._DescriptionFr != value)
                {
                    this._DescriptionFr = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _TermUpperCaseFr;
        public string TermUpperCaseFr
        {
            get { return _TermUpperCaseFr; }
            set
            {
                if (this._TermUpperCaseFr != value)
                {
                    this._TermUpperCaseFr = value?.ToUpper();
                    OnPropertyChanged();
                }

            }
        }

        private string _TermAbbrevitationFr;
        public string TermAbbrevitationFr
        {
            get { return _TermAbbrevitationFr; }
            set
            {
                if (this._TermAbbrevitationFr != value)
                {
                    this._TermAbbrevitationFr = value?.ToUpper();
                    OnPropertyChanged();
                }

            }
        }

        private string _AttributeFr;
        public string AttributeFr
        {
            get { return _AttributeFr; }
            set
            {
                if (this._AttributeFr != value)
                {
                    this._AttributeFr = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _AttributeExampleFr;
        public string AttributeExampleFr
        {
            get { return _AttributeExampleFr; }
            set
            {
                if (this._AttributeExampleFr != value)
                {
                    this._AttributeExampleFr = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _TermDe;
        public string TermDe
        {
            get { return _TermDe; }
            set
            {
                if (this._TermDe != value)
                {
                    this._TermDe = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _DescriptionDe;
        public string DescriptionDe
        {
            get { return _DescriptionDe; }
            set
            {
                if (this._DescriptionDe != value)
                {
                    this._DescriptionDe = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _TermUpperCaseDe;
        public string TermUpperCaseDe
        {
            get { return _TermUpperCaseDe; }
            set
            {
                if (this._TermUpperCaseDe != value)
                {
                    this._TermUpperCaseDe = value?.ToUpper();
                    OnPropertyChanged();
                }

            }
        }

        private string _TermAbbrevitationDe;
        public string TermAbbrevitationDe
        {
            get { return _TermAbbrevitationDe; }
            set
            {
                if (this._TermAbbrevitationDe != value)
                {
                    this._TermAbbrevitationDe = value?.ToUpper();
                    OnPropertyChanged();
                }

            }
        }

        private string _AttributeDe;
        public string AttributeDe
        {
            get { return _AttributeDe; }
            set
            {
                if (this._AttributeDe != value)
                {
                    this._AttributeDe = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _AttributeExampleDe;
        public string AttributeExampleDe
        {
            get { return _AttributeExampleDe; }
            set
            {
                if (this._AttributeExampleDe != value)
                {
                    this._AttributeExampleDe = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> ListImage { get; set; } = new ObservableCollection<string>();

        private string _SelectedImage = null;
        public string SelectedImage
        {
            get { return _SelectedImage; }
            set
            {
                if (this._SelectedImage != value)
                {
                    this._SelectedImage = value;
                    OnPropertyChanged();
                }

            }
        }

    }
}
