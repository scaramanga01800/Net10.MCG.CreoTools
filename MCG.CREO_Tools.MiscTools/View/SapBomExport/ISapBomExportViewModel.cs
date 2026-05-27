using MCG.CREO_Tools.MiscTools.ViewModel.SapBomExport;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.SapBomExport
{
    public interface ISapBomExportViewModel
    {
        SapBomExportDataContext CurrentDataContext { get; set; }
        ICommand CommandStartSapBomExport { get; }
        ICommand CommandStartExportExcel { get; }
        ICommand CommandOpenHelp { get; }
        ICommand CommandExit { get; }
        ICommand CommandExpandAll { get; }
        ICommand CommandCollapseAll { get; }
        ICommand CommandToggleExpandCollapse { get; }
    }
}
