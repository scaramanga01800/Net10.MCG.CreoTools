using MCG.CommonLib.Models.SAP;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
   public class QuickSearchUserConfiguration
    {
        public List<QuickSearchShortCutData> ListShortCut { get; set; }

        public SapPlant CurrentSapPlant { get; set; }
    }
}
