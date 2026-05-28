using MCG.CREO_Tools.MiscTools.ViewModel.SapFertBom;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.SapFertBom
{
    public interface ISapFertBomViewModel
    {
        SapFertBomDataContext CurrentDataContext { get; set; }
        ICommand CommandStartSapBomExport { get; }
        ICommand CommandStartImportExcel { get; }
        ICommand CommandStartCheckPartSap { get; }
        ICommand CommandStartUpdateBomSap { get; }
        ICommand CommandOpenHelp { get; }
        ICommand CommandPaste { get; }
    }
}
