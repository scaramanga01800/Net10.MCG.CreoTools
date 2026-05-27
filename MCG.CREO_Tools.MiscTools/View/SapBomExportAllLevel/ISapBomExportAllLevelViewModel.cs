using MCG.CREO_Tools.MiscTools.ViewModel.SapBomExportAllLevel;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.SapBomExportAllLevel
{
    public interface ISapBomExportAllLevelViewModel
    {
        SapBomExportAllLevelDataContext CurrentDataContext { get; set; }
        ICommand CommandStartSapBomExport { get; }
        ICommand CommandStartExportExcel { get; }
        ICommand CommandOpenHelp { get; }
        ICommand CommandExit { get; }
        ICommand CommandExpandAll { get; }
        ICommand CommandCollapseAll { get; }
        ICommand CommandToggleExpandCollapse { get; }
    }
}
