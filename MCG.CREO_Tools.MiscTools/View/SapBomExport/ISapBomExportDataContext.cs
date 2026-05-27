using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.MiscTools.ViewModel.SapBomExport;
using MCG.WindchillRequestTool.Model.BomComparison;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.SapBomExport
{
    internal interface ISapBomExportDataContext
    {
        string PartNumber { get; set; }
        string EcoNumber { get; set; }
        ObservableCollection<SapPlant> AllSapPlants { get; set; }
        SapPlant Plant { get; set; }
        ObservableCollection<string> AllAlternativeBom { get; set; }
        string AlternativeBom { get; set; }
        ObservableCollection<SapBomExportApplicationItem> AllBomApplication { get; set; }
        SapBomExportApplicationItem BomApplication { get; set; }
        DateTime DateValidity { get; set; }
        DateTime DateValidityCost { get; set; }
        ObservableCollection<string> AllRevision { get; set; }
        string Revision { get; set; }
        bool Is_CB_RLT_Selected { get; set; }
        bool Is_CB_PUR_Selected { get; set; }
        bool Is_RB_SC_Selected { get; set; }
        bool Is_RB_RT_Selected { get; set; }
        bool Is_RB_ALL_Selected { get; set; }
        bool Is_RB_MRT_Selected { get; set; }

        ObservableCollection<int> SizeColumns { get; set; }

        int MaxBomLevel { get; set; }

        ObservableCollection<BomComponent> MainStructure { get; set; }

        bool IsPleaseWaitShown { get; set; }

    }
}
