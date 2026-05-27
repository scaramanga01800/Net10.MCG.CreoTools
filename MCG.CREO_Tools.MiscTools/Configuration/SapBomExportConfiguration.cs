using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.MiscTools.ViewModel.SapBomExport;

namespace MCG.CREO_Tools.MiscTools.ViewModel.Configuration
{
    [Obsolete("Raplace by SapConfiguration")]
    public class SapBomExportConfiguration
    {
        public List<SapBomExportApplicationItem> ListBomApplication {  get; set; }

        public List<SapPlant> ListSapPlant { get; set; }

    }
}
