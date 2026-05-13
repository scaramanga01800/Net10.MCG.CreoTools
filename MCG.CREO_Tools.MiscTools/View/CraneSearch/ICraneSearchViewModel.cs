using MCG.CREO_Tools.MiscTools.ViewModel.CraneSearch;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.CraneSearch
{
    public interface ICraneSearchViewModel
    {
        CraneSearchDataContext CurrentDataContext { get; set; }
        List<string> PartList { get; set; }

        ICommand CommandCtrlPaste { get; }
        ICommand CommandPaste { get; }
        ICommand CommandSearchSapCrane { get; }
        ICommand CommandExportExcel { get; }
        ICommand CommandOpenHelp { get; }
        ICommand CommandClose { get; }
        ICommand CommandRemoveAll { get; }

    }
}
