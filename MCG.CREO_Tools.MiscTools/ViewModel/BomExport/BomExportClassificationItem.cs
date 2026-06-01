using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.BomExport;
using MCG.WindchillRequestTool.Model.Windchill;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.BomExport
{
    public class BomExportClassificationItem : ObservableObject, IBomExportClassificationItem
    {
        private double _CumulativeMass;
        public double CumulativeMass
        {
            get { return _CumulativeMass; }
            set
            {
                if (this._CumulativeMass != value)
                {
                    this._CumulativeMass = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _PtcCommonName;
        public string PtcCommonName
        {
            get { return _PtcCommonName; }
            set
            {
                if (this._PtcCommonName != value)
                {
                    this._PtcCommonName = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Material;
        public string Material
        {
            get { return _Material; }
            set
            {
                if (this._Material != value)
                {
                    this._Material = value;
                    OnPropertyChanged();
                }

            }
        }

        private double _CumulativeQuantity;
        public double CumulativeQuantity
        {
            get { return _CumulativeQuantity; }
            set
            {
                if (this._CumulativeQuantity != value)
                {
                    this._CumulativeQuantity = value;
                    OnPropertyChanged();
                }

            }
        }


        public ObservableCollection<WindchillObjStructureComponent> ListItem { get; set; } = new ObservableCollection<WindchillObjStructureComponent>();

    }
}
