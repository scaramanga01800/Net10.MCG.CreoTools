using MCG.CommonLib.Models.SAP;
using MCG.WindchillRequestTool.Model.BomComparison;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.SapBomExportAllLevel
{
    public interface ISapBomExportAllLevelDataContext
    {
        string PartNumber { get; set; }
        ObservableCollection<SapPlant> AllSapPlants { get; set; }
        SapPlant Plant { get; set; }
        ObservableCollection<SapBomUsage> AllBomUsage { get; set; }
        SapBomUsage BomUsage { get; set; }
        DateTime DateValidity { get; set; }
        ObservableCollection<int> SizeColumns { get; set; }
        int MaxBomLevel { get; set; }
        ObservableCollection<BomComponent> MainStructure { get; set; }
        ObservableCollection<BomComponent> AllComponents { get; set; }
        ObservableCollection<BomComponent> FlatStructure { get; set; }
        bool IsPleaseWaitShown { get; set; }
        int SapSearchIndex { get; set; }
    }
}
