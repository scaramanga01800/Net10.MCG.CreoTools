using MCG.CREO_Tools.JpgExport.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.JpgExport.View
{
    interface IJpgExportViewModel
    {
        JpgExportDataContext CurrentJpgExportDataContext { get; set; }

        ICommand CommandBtHelpMouseLeftButtonUpEvent { get; }
        ICommand CommandOpenFolder { get; }
        ICommand CommandOpenFile { get; }
        ICommand CommandExportJpg { get; }
        ICommand CommandOpenModelInCreo { get; }
        ICommand CommandPaste { get; }
        ICommand CommandMenuItemPasteCodes { get; }
    }
}
