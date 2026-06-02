using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.BomExport
{
    public interface IBomExportWindowViewModel
    {
        ICommand CommandAddParameter { get; }
        ICommand CommandRemoveParameter { get; }
        ICommand CommandMoveUpParameter { get; }
        ICommand CommandMoveDownParameter { get; }
        ICommand CommandStartBomSearch { get; }
        ICommand CommandStartBomExport { get; }
        ICommand CommandSelectedBomItem { get; }
        ICommand CommandSortBom { get; }
        ICommand CommandSapPlantSelectionChanged { get; }
        ICommand CommandCopyPartNumber { get; }
        ICommand CommandStartPendingEcnSearch { get; }
        ICommand CommandShowOccurrencesChanged { get; }
        ICommand CommandClosing { get; }
        ICommand CommandBtHelpMouseLeftButtonUpEvent { get; }
        ICommand CommandHelpVisuTool { get; }
        ICommand CommandRemoveLine { get; }
        ICommand CommandResetBom { get; }
        ICommand CommandDownloadDrawing { get; }
        ICommand CommandStartCumulativeMaterial { get; }
        ICommand CommandStartCumulativeName { get; }
        ICommand CommandStartCumulativeBomExport { get; }
        ICommand CommandCloseCumulativeBomExport { get; }
        ICommand CommandCumulateInWorkNumber { get; }
        ICommand CommandToggleExpandCollapse { get; }
    }
}
