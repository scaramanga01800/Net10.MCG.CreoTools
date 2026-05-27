using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.MiscTools.ViewModel.SapBomExport;

namespace MCG.CREO_Tools.MiscTools.Configuration
{
    public class SapConfiguration
    {
        public List<SapBomExportApplicationItem> ListBomApplication { get; set; }

        public List<SapPlant> ListSapPlant { get; set; }

        public List<SapPlant> AllSapPlant { get; set; }

    }
}
