using MCG.WindchillRequestTool;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.SapFertBom
{
    public interface ISapFertBomDataContext
    {
        string FertNumber { get; set; }
        ObservableCollection<string> AllSapPlants { get; set; }
        string Plant { get; set; }
        BomComparisonItem BomComparison { get; set; }

        bool IsActionProgress { get; set; }
        bool IsPleaseWaitShown { get; set; }
    }
}
