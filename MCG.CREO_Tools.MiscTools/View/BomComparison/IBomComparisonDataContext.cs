using MCG.WindchillRequestTool;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.BomComparison
{
    internal interface IBomComparisonDataContext
    {
        bool IsActionProgress { get; set; }
        bool IsPartChecked { get; set; }
        bool IsAssemblyChecked { get; set; }
        bool IsLatestRevisionL { get; set; }
        bool IsLatestIterationL { get; set; }
        bool IsLatestRevisionR { get; set; }
        bool IsLatestIterationR { get; set; }

        string NumberL { get; set; }
        string RevisionL { get; set; }
        string NumberR { get; set; }
        string RevisionR { get; set; }

        string StatusBarMsgL { get; set; }
        string StatusBarMsgR { get; set; }

        BomItem BomL { get; set; }
        BomItem BomR { get; set; }

        int BomLevel { get; set; }
        int MaxBomLevel { get; set; }
        bool IsShowOccurrences { get; set; }
        int NumericalLineNumberDigit { get; set; }

        BomComparisonItem BomComparison { get; set; }

        string SelectedBomFromL { get; set; }
        string SelectedBomFromR { get; set; }
        ObservableCollection<string> ListBomFrom { get; set; }
        string SelectedSapPlantL { get; set; }
        string SelectedSapPlantR { get; set; }
        ObservableCollection<string> ListSapPlant { get; set; }
        DateTime? ValidityDateL { get; set; }
        DateTime? ValidityDateR { get; set; }

        bool ShowPdmFieldsL { get; set; }
        bool ShowPdmFieldsR { get; set; }
        bool ShowSapFieldsL { get; set; }
        bool ShowSapFieldsR { get; set; }
        bool ShowAsmRadioButton { get; set; }

    }
}
