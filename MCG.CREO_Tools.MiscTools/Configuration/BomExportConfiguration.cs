using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.MiscTools.ViewModel.BomExport;

namespace MCG.CREO_Tools.MiscTools.ViewModel.Configuration
{
    public class BomExportConfiguration
    {
        public List<BomExportParameterData> ListAvailableParameter { get; set; }
        public List<BomExportParameterData> ListSelectedParameter { get; set; }
        public char FieldSeparator { get; set; }
        public List<BomExportOutputFormat> ListOutputFormat { get; set; }
        public BomExportOutputFormat SelectedOutputFormat { get; set; }

        public List<SapPlant> ListSapPlant { get; set; }

        public List<string> UnapprovedState { get; set; }

        public bool ShowSapCostVolumeInfo { get; set; } = false;

        public bool IsLevelIndented { get; set; } = false;
    }
}
