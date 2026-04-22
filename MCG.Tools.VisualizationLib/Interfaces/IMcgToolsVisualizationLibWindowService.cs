using MCG.Tools.VisualizationLib.ViewModel;
using System.Windows;

namespace MCG.Tools.VisualizationLib.Interfaces
{
    public interface IMcgToolsVisualizationLibWindowService
    {
        void CloseConvertToPdfMergeWindowView();
        (MessageBoxResult ResultDialog, string FileName) ShowDialogConvertToPdfMergeWindowView(List<ConvertToPdfItem> pListFiles, string defaultFileName);
    }
}