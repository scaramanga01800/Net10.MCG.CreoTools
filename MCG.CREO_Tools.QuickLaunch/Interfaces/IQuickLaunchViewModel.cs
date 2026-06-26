using MCG.CommonLib.WpfComponent;
using System.Windows.Input;

namespace MCG.CREO_Tools.QuickLaunch.View
{
    interface IQuickLaunchViewModel
    {
        CREOToolsAppAvailability AppVisible { get; set; }
        CREOToolsAppAvailability AppAvailable { get; set; }
        bool IsCreoConnected { get; set; }
        bool IsCreoConnectionInProgress { get; set; }
        string HtmlPage { get; set; }

        event EventHandler HtmlPageChangedEvent;

        // Icommand
        ICommand CommandNewCadDocument { get; }
        ICommand CommandWebterm { get; }
        ICommand CommandPartNumberGenerator { get; }
        ICommand CommandConnectCreoSession { get; }
        ICommand CommandExportBom { get; }
        ICommand CommandPartNumberCreator { get; }
        ICommand CommandDxfDwgDrawingExport { get; }
        ICommand CommandBackUpCadDocument { get; }
        ICommand CommandMcgHelpOnline { get; }
        ICommand CommandEngTime { get; }
        ICommand CommandMechanismAnalysis { get; }
        ICommand CommandCreateUpdateWtDocPart { get; }
        ICommand CommandKillCreoProcesses { get; }
        ICommand CommandCadAutoColor { get; }
        ICommand CommandNumberCumulation { get; }
        ICommand CommandBomComparison { get; }
        ICommand CommandSapExportBom { get; }
        ICommand CommandSapBomExportAllLevel { get; }
        ICommand CommandSapFertBom { get; }
        ICommand CommandWebtermRequest { get; }
        ICommand CommandCraneSearch { get; }
        ICommand CommandQuickChange { get; }
        ICommand CommandCadDocumentRename { get; }
        ICommand CommandBomEnvirConfig { get; }
        ICommand CommandUpdateRelationsParameters { get; }
    }
}
