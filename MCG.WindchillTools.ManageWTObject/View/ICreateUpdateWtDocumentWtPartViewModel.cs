using System.Windows.Input;

namespace MCG.WindchillTools.ManageWTObject.View
{
    public interface ICreateUpdateWtDocumentWtPartViewModel
    {
        ICommand CommandClosing { get; }
        ICommand CommandDrop { get; }
        ICommand CommandRemoveContent { get; }
        ICommand CommandDownloadContent { get; }
        ICommand CommandOpenHelp { get; }
        ICommand CommandCopyWtDocToPartNumber { get; }
        ICommand CommandCopyWtDocToPartContext { get; }
        ICommand CommandCopyWtDocToPartParam { get; }
        ICommand CommandCopyPartToWtDocNumber { get; }
        ICommand CommandCopyPartToWtDocContext { get; }
        ICommand CommandCopyPartToWtDocParam { get; }
        ICommand CommandSearchWtObject { get; }
        ICommand CommandChangeContext { get; }
        ICommand CommandSearchOk { get; }
        ICommand CommandSearchCancel { get; }
        ICommand CommandCheckWtDocument { get; }
        ICommand CommandCheckWtPart { get; }
        ICommand CommandCreateUpdateWtDocument { get; }
        ICommand CommandCreateUpdateWtPart { get; }
        ICommand CommandResetWtDocument { get; }
        ICommand CommandResetWtPart { get; }
        ICommand CommandCreateCreateLink { get; }

    }
}
