using MCG.Tools.VisualizationLib.Interfaces;
using MCG.Tools.VisualizationLib.View;
using MCG.Tools.VisualizationLib.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MCG.Tools.VisualizationLib.Services
{
    public class McgToolsVisualizationLibWindowService : IMcgToolsVisualizationLibWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        private Window _ConvertToPdfMergeWindowView;

        public McgToolsVisualizationLibWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public (MessageBoxResult ResultDialog, string FileName) ShowDialogConvertToPdfMergeWindowView(List<ConvertToPdfItem> pListFiles, string defaultFileName)
        {
            _ConvertToPdfMergeWindowView = _serviceProvider.GetRequiredService<ConvertToPdfMergeWindowView>();
            ((ConvertToPdfMergeWindowViewModel)_ConvertToPdfMergeWindowView.DataContext).SetConvertToPdfMergeWindowViewModelProperties(pListFiles, defaultFileName);

            _ConvertToPdfMergeWindowView.ShowDialog();

            var resultDialog = ((ConvertToPdfMergeWindowViewModel)_ConvertToPdfMergeWindowView.DataContext).Return;
            string fileName = ((ConvertToPdfMergeWindowView)_ConvertToPdfMergeWindowView).CurrentDataContext.FileName.Split('.').FirstOrDefault(); ;

            return (resultDialog, fileName);
        }

        public void CloseConvertToPdfMergeWindowView()
        {
            if (_ConvertToPdfMergeWindowView != null)
            {
                _ConvertToPdfMergeWindowView.Close();
                _ConvertToPdfMergeWindowView = null;
            }
        }
    }
}
