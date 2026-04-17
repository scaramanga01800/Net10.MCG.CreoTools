using MCG.WindchillTools.ManageWTObject.ViewModel;
using System.Windows.Input;

namespace MCG.WindchillTools.ManageWTObject.View
{
    public interface IMassWtDocumentUpdateViewModel
    {
        MassWtDocumentUpdateDataContext CurrentDataContext { get; set; }
        bool IsSingleWtDocumentDragDropInProgress { get; set; }

        ICommand CommandPaste { get; }
        ICommand CommandDragAndDrop { get; }
        ICommand CommandCheckUncheckAll { get; }
        ICommand CommandDragAndDropWtDocument { get; }
        ICommand CommandAddWtDocument { get; }
        ICommand CommandCheckWtDocument { get; }
        ICommand CommandCheckPart { get; }
        ICommand CommandRemoveWtDocument { get; }
        ICommand CommandApplyWtDocumentType { get; }
        ICommand CommandApplyWtPartType { get; }
        ICommand CommandApplyContext { get; }
        ICommand CommandCreateUpdateWtDocument { get; }
        ICommand CommandCreateUpdateWtPart { get; }
        ICommand CommandCreateUpdateLink { get; }
        ICommand CommandOpenHelp { get; }
        ICommand CommandReviseItem { get; }
        ICommand CommandRemoveItem { get; }
        ICommand CommandDragAndDropXls { get; }
        ICommand CommandDragAndDropSecondaryContent { get; }
        ICommand CommandRemoveContent { get; }
        ICommand CommandChangeContext { get; }
        ICommand CommandApplyWebterm { get; }
        ICommand CommandRenameWtDocument { get; }
        ICommand CommandRenameUpdateWtPart { get; }
    }
}
