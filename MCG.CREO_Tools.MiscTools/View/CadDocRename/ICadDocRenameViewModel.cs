using System.Windows.Input;
using MCG.CREO_Tools.MiscTools.ViewModel.CadDocRename;

namespace MCG.CREO_Tools.MiscTools.View.CadDocRename
{
    internal interface ICadDocRenameViewModel
    {
        CadDocRenameDataContext CurrentDataContext { get; set; }

        ICommand CommandReadAsm { get; }
        ICommand CommandRenameCadDoc { get; }
        ICommand CommandOpenHelp { get; }
    }
}
