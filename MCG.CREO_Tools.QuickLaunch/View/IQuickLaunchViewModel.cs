using MCG.CommonLib.WpfComponent;
using System.Windows.Input;

namespace MCG.CREO_Tools.QuickLaunch.View
{
    public interface IQuickLaunchViewModel
    {
        CREOToolsAppAvailability AppAvailable { get; set; }
        CREOToolsAppAvailability AppVisible { get; set; }
        ICommand CommandBackUpCadDocument { get; }
        ICommand CommandBomComparison { get; }
        ICommand CommandBomEnvirConfig { get; }
        ICommand CommandCadAutoColor { get; }
        ICommand CommandCadDocumentRename { get; }
        ICommand CommandConnectCreoSession { get; }
        ICommand CommandCraneSearch { get; }
        ICommand CommandCreateUpdateWtDocPart { get; }
        ICommand CommandDxfDwgDrawingExport { get; }
        ICommand CommandEngTime { get; }
        ICommand CommandExportBom { get; }
        ICommand CommandKillCreoProcesses { get; }
        ICommand CommandMcgHelpOnline { get; }
        ICommand CommandMechanismAnalysis { get; }
        ICommand CommandNewCadDocument { get; }
        ICommand CommandNumberCumulation { get; }
        ICommand CommandOpenCall { get; }
        ICommand CommandPartNumberCreator { get; }
        ICommand CommandPartNumberGenerator { get; }
        ICommand CommandQuickChange { get; }
        ICommand CommandSapBomExportAllLevel { get; }
        ICommand CommandSapExportBom { get; }
        ICommand CommandSapFertBom { get; }
        ICommand CommandUpdateRelationsParameters { get; }
        ICommand CommandWebterm { get; }
        ICommand CommandWebtermRequest { get; }
        string HtmlPage { get; set; }
        bool IsCreoConnected { get; set; }
        bool IsCreoConnectionInProgress { get; set; }

        event EventHandler HtmlPageChangedEvent;

        void RaiseHtmlPageChangedEvent();
    }
}