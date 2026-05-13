using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Models.SAP;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.Tools.EcnDataCheck.Models;
using MCG.WindchillRequestTool.Model.Windchill;
using System.Collections.ObjectModel;

namespace MCG.Tools.EcnDataCheck.View
{
    public interface IEcnDataCheckDataContext
    {
        string EcnNumber { get; set; }
        WindchillChangeNotice CurrentWindchillChangeNotice { get; set; }
        WindchillChangeActivity EcaNumber { get; set; }
        ObservableCollection<WindchillChangeActivity> EcaList { get; set; }

        DataCheckStatus GlobalStatus { get; set; }

        bool EcnDataCheckInProgress { get; set; }

        int TotalStep { get; set; }
        int CurrentStep { get; set; }


        bool ShowActionButton { get; set; }
        bool ShowRenameTab { get; set; }
        bool ShowMoveTab { get; set; }


        MCGLanguage SelectedLanguage { get; set; }
        ObservableCollection<MCGLanguage> ListLanguage { get; set; }

        string ErpSystem { get; set; }
        ObservableCollection<string> ErpList { get; set; }

        IEcnDataCheckItem SelectedDataCheckItem { get; set; }
        ObservableCollection<IEcnDataCheckItem> DataCheckItemList { get; set; }

        IEcnDataCheckResultItem SelectedDataCheckResultItem { get; set; }
        ObservableCollection<IEcnDataCheckResultItem> DataCheckResultItemList { get; set; }

        // Properties for the Move Tab
        string SelectedLocation { get; set; }
        ObservableCollection<string> ListLocation { get; set; }
        bool IsCheckBoxProductSelected { get; set; }
        bool IsCheckBoxLibraySelected { get; set; }
        string ContextFilter { get; set; }
        ObservableCollection<IEcnDataCheckItem> MoveItemList { get; set; }
        IEcnDataCheckItem SelectedMoveItem { get; set; }
        ObservableCollection<WindchillContext> WindchillContextList { get; set; }
        WindchillContext SelectedContext { get; set; }

        // Properties for the Rename tab
        ObservableCollection<string> WebTermList { get; set; }
        ObservableCollection<IEcnDataCheckItem> RenameItemList { get; set; }
        IEcnDataCheckItem SelectedRenameItem { get; set; }

        // Property for the Status Bar
        string ExtraStatusBarMsg { get; set; }

        // Properties SAP Menu
        SapPlant SelectedSapPlant { get; set; }
        ObservableCollection<SapPlant> ListSapPlant { get; set; }
        int NumericalLineNumberDigit { get; set; }
        ObservableCollection<int> NumericalLineNumberDigitList { get; set; }

        // Properties for StatusBar
        string StatusBarMsg1 { get; set; }
        string StatusBarMsg2 { get; set; }

        ObservableCollection<SapGenericObject> SapCraneList { get; set; }
    }
}
