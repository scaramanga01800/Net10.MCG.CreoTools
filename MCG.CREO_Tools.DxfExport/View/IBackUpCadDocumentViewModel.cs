using MCG.CREO_Tools.DxfExport.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.DxfExport.View
{
    public interface IBackUpCadDocumentViewModel
    {
        BackUpCadDocumentViewDataContext CurrentDatacontext { get; set; }
        ICommand CommandCadDocumentBackup { get; }
        ICommand CommandOpenFolder { get; }
        ICommand CommandPaste { get; }
        ICommand CommandResetList { get; }
    }
}
