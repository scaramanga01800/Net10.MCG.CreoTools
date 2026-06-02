using MCG.CommonLib.Models.SAP;

namespace MCG.CREO_Tools.MiscTools.ViewModel.BomExport
{
    public class BomExportUserPreferences
    {
        public List<BomExportParameterData> ListSelectedParameter { get; set; }
        public char FieldSeparator { get; set; }
        public BomExportOutputFormat SelectedOutputFormat { get; set; }
        public SapPlant CurrentSapPlant { get; set; }
        public bool IsLevelIndented { get; set; }

        public bool IsStateInWork { get; set; } = true;
        public bool IsStateUnderReview { get; set; } = true;
        public bool IsStatePreReleased { get; set; } = false;
        public bool IsStatePrototype { get; set; } = false;
        public bool IsStateReleased { get; set; } = false;
        public bool IsStateObsolete { get; set; } = false;
        public bool IsStateSuperseded { get; set; } = false;
        public bool IsStateRework { get; set; } = true;
    }
}
