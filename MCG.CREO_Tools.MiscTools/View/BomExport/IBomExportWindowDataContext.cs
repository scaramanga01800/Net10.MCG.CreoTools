using MCG.CommonLib.Models.SAP;
using MCG.CREO_Tools.MiscTools.ViewModel.BomExport;
using MCG.WindchillRequestTool.Model.Windchill;
using System.Collections.ObjectModel;


namespace MCG.CREO_Tools.MiscTools.View.BomExport
{
    public interface IBomExportWindowDataContext
    {
        bool IsPartChecked { get; set; }
        bool IsAssemblyChecked { get; set; }
        bool IsLatestRevision { get; set; }

        bool ShowSapCostVolumeInfo { get; set; }
        bool IsMsgSearchSap { get; set; }
        bool IsSearchBomDone { get; set; }
        bool IsActionProgress { get; set; }
        string Number { get; set; }
        string Revision { get; set; }

        double MainSapCost { get; set; }
        string MainSapProvider { get; set; }

        List<int> BomLevelList { get; set; }
        int BomLevel { get; set; }
        int MaxBomLevel { get; set; }

        ObservableCollection<BomExportParameter> ListAvailableParameters { get; set; }
        ObservableCollection<BomExportParameter> ListSelectedParameters { get; set; }
        ObservableCollection<BomExportParameter> ListAllParameters { get; set; }
        ObservableCollection<BomExportParameter> ListAllParametersAuthorized { get; set; }

        ObservableCollection<SapPlant> ListSapPlant { get; set; }

        SapPlant SelectedSapPlant { get; set; }

        BomExportParameter SelectedParameterAvailable { get; set; }
        BomExportParameter SelectedParameter { get; set; }

        char FieldSeparator { get; set; }
        ObservableCollection<BomExportOutputFormat> ListOutputFormat { get; set; }
        BomExportOutputFormat SelectedOutputFormat { get; set; }

        bool IsNamingConvention { get; set; }

        ObservableCollection<WindchillObjStructureComponent> MainBom { get; set; }
        WindchillObjStructureComponent SelectedBomItem { get; set; }

        ObservableCollection<WindchillObjStructureComponent> AllComponents { get; set; }
        WindchillObjStructureComponent SelectedComponent { get; set; }

        BomExportParameter BomColumnNumber { get; set; }
        BomExportParameter BomColumnLevel { get; set; }

        BomExportParameter BomColumn1 { get; set; }
        BomExportParameter BomColumn2 { get; set; }
        BomExportParameter BomColumn3 { get; set; }
        BomExportParameter BomColumn4 { get; set; }
        BomExportParameter BomColumn5 { get; set; }
        BomExportParameter BomColumn6 { get; set; }
        BomExportParameter BomColumn7 { get; set; }
        BomExportParameter BomColumn8 { get; set; }
        BomExportParameter BomColumn9 { get; set; }
        BomExportParameter BomColumn10 { get; set; }
        BomExportParameter BomColumn11 { get; set; }
        BomExportParameter BomColumn12 { get; set; }
        BomExportParameter BomColumn13 { get; set; }
        BomExportParameter BomColumn14 { get; set; }
        BomExportParameter BomColumn15 { get; set; }
        BomExportParameter BomColumn16 { get; set; }
        BomExportParameter BomColumn17 { get; set; }
        BomExportParameter BomColumn18 { get; set; }
        BomExportParameter BomColumn19 { get; set; }
        BomExportParameter BomColumn20 { get; set; }

        string StatusBarMsg { get; set; }

        bool IsShowOccurrences { get; set; }
        bool IsLevelIndented { get; set; }

        int NumericalLineNumberDigit { get; set; }
        ObservableCollection<int> NumericalLineNumberDigitList { get; set; }

        ObservableCollection<BomExportClassificationItem> ClassificationItemList { get; set; }
        bool IsColNameShown { get; set; }
        bool IsColMaterialShown { get; set; }
        double CumulativeEndItemMass { get; set; }

        bool IsCreateZip { get; set; }

        bool IsStateInWork { get; set; }
        bool IsStateUnderReview { get; set; }
        bool IsStatePreReleased { get; set; }
        bool IsStatePrototype { get; set; }
        bool IsStateReleased { get; set; }
        bool IsStateObsolete { get; set; }
        bool IsStateSuperseded { get; set; }
        bool IsStateRework { get; set; }
    }
}

