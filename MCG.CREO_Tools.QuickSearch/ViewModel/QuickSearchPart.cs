using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using System.Windows.Media.Imaging;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
    public class QuickSearchPart : ObservableObject
    {
        private Part _CurrentPart;
        public Part CurrentPart
        {
            get { return this._CurrentPart; }
            set
            {
                if (this._CurrentPart != value)
                {
                    this._CurrentPart = value;
                    OnPropertyChanged();
                }
            }
        }

        public Part UpdatedPart { get; set; }

        private byte[] _UpdatedImage;
        public byte[] UpdatedImage
        {
            get { return _UpdatedImage; }
            set
            {
                if (this._UpdatedImage != value)
                {
                    this._UpdatedImage = value;
                    OnPropertyChanged();
                }

            }
        }

        public QuickSearchPartSubClass SubClassItem { get; set; }

        public EPMDocument CurrentEpmDocument { get; set; }

        public List<QuickSearchExtraCompMenu> ListExtraMenu { get; set; }

        private double _PlantStdCost;
        public double PlantStdCost
        {
            get { return this._PlantStdCost; }
            set
            {
                if (this._PlantStdCost != value)
                {
                    this._PlantStdCost = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _PlantStdCostPerKg;
        public double PlantStdCostPerKg
        {
            get { return this._PlantStdCostPerKg; }
            set
            {
                if (this._PlantStdCostPerKg != value)
                {
                    this._PlantStdCostPerKg = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _PlantVolume =1;
        public double PlantVolume
        {
            get { return this._PlantVolume; }
            set
            {
                if (this._PlantVolume != value)
                {
                    this._PlantVolume = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _WorldAverageCost;
        public double WorldAverageCost
        {
            get { return this._WorldAverageCost; }
            set
            {
                if (this._WorldAverageCost != value)
                {
                    this._WorldAverageCost = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _WorldAverageVolume;
        public double WorldAverageVolume
        {
            get { return this._WorldAverageVolume; }
            set
            {
                if (this._WorldAverageVolume != value)
                {
                    this._WorldAverageVolume = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _EuropeAverageCost;
        public double EuropeAverageCost
        {
            get { return this._EuropeAverageCost; }
            set
            {
                if (this._EuropeAverageCost != value)
                {
                    this._EuropeAverageCost = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _FrenchAverageCost;
        public double FrenchAverageCost
        {
            get { return this._FrenchAverageCost; }
            set
            {
                if (this._FrenchAverageCost != value)
                {
                    this._FrenchAverageCost = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _PlantMaxVolume = "NotUsed";
        public string PlantMaxVolume
        {
            get { return this._PlantMaxVolume; }
            set
            {
                if (this._PlantMaxVolume != value)
                {
                    this._PlantMaxVolume = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ProcurementType;
        public string ProcurementType
        {
            get { return this._ProcurementType; }
            set
            {
                if (this._ProcurementType != value)
                {
                    this._ProcurementType = value;
                    OnPropertyChanged();
                }
            }
        }

        private BitmapImage _PartPictureShown;
        public BitmapImage PartPictureShown
        {
            get { return this._PartPictureShown; }
            set
            {
                if (this._PartPictureShown != value)
                {
                    this._PartPictureShown = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _PartPicturePath;
        public string PartPicturePath
        {
            get { return this._PartPicturePath; }
            set
            {
                if (this._PartPicturePath != value)
                {
                    this._PartPicturePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string OrigPartNumber { get; set; }
    }
}
