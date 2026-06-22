using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.QuickSearch.ViewModel;

namespace MCG.CREO_Tools.QuickSearch.Configuration
{
    public class QuickSearchConfiguration
    {
        public bool CRWLocalShown { get; set; }
        public bool DGLocalShown { get; set; }
        public bool SGLocalShown { get; set; }
        public bool TWRLocalShown { get; set; }
        public bool MFGTWRLocalShown { get; set; }
        public bool STDGlobalShown { get; set; }

        public bool CRWLocalEnabled { get; set; }
        public bool DGLocalEnabled { get; set; }
        public bool SGLocalEnabled { get; set; }
        public bool TWRLocalEnabled { get; set; }
        public bool MFGTWRLocalEnabled { get; set; }
        public bool STDGlobalEnabled { get; set; }

        public List<SapPlant> ListSapPlant { get; set; }
        public List<SapPlant> ExtraListSapPlant { get; set; }

        public bool ShowSapCostVolumeInfo { get; set; } = false;

        public List<QuickSearchShortCutData> ListShortCut { get; set; }
    }
}