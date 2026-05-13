using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.MiscTools.ViewModel.CraneSearch;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.CraneSearch
{
    public interface ICraneSearchDataContext
    {
        ObservableCollection<string> PartList { get; set; }

        ObservableCollection<CraneSearchItem> CraneList { get; set; }

        ObservableCollection<SapPlant> PlantList { get; set; }

        ObservableCollection<KeyValuePair<string, string>> EuropeEquivalent { get; set; }

        ObservableCollection<KeyValuePair<string, string>> AsiaEquivalent { get; set; }

        bool IsStandAlone { get; set; }
    }
}
