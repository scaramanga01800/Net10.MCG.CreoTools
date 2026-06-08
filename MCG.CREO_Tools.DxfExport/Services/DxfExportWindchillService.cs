using MCG.CREO_Tools.DxfExport.Interfaces;
using MCG.CREO_Tools.DxfExport.View;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MCG.CREO_Tools.DxfExport.Services
{
    public class DxfExportWindchillService : IDxfExportWindchillService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _BackUpCadDocumentView;
        private Window _DxfDwgDrawingExportMainView;

        public DxfExportWindchillService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowBackUpCadDocumentView(bool isAlreadyCreated = false)
        {

            if (isAlreadyCreated)
            {
                if (_BackUpCadDocumentView != null && _BackUpCadDocumentView.IsVisible)
                {
                    _BackUpCadDocumentView.Activate();
                    return;
                }
            }

            _BackUpCadDocumentView = _serviceProvider.GetRequiredService<BackUpCadDocumentView>();
            _BackUpCadDocumentView.Show();
        }
        public MessageBoxResult ShowDialogBackUpCadDocumentView()
        {
            _BackUpCadDocumentView = _serviceProvider.GetRequiredService<BackUpCadDocumentView>();
            var DialogResult = _BackUpCadDocumentView.ShowDialog();
            if (DialogResult.HasValue)
            {
                return DialogResult.Value ? MessageBoxResult.OK : MessageBoxResult.Cancel;
            }
            else
            {
                return MessageBoxResult.None;
            }
        }

        public void ShowDxfDwgDrawingExportMainView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_DxfDwgDrawingExportMainView != null && _DxfDwgDrawingExportMainView.IsVisible)
                {
                    _DxfDwgDrawingExportMainView.Activate();
                    return;
                }
            }
            _DxfDwgDrawingExportMainView = _serviceProvider.GetRequiredService<DxfDwgDrawingExportMainView>();
            _DxfDwgDrawingExportMainView.Show();
        }
        public MessageBoxResult ShowDialogDxfDwgDrawingExportMainView()
        {
            _DxfDwgDrawingExportMainView = _serviceProvider.GetRequiredService<DxfDwgDrawingExportMainView>();
            var DialogResult = _DxfDwgDrawingExportMainView.ShowDialog();
            if (DialogResult.HasValue)
            {
                return DialogResult.Value ? MessageBoxResult.OK : MessageBoxResult.Cancel;
            }
            else
            {
                return MessageBoxResult.None;
            }
        }

        public void CloseBackUpCadDocumentView()
        {
            _BackUpCadDocumentView.Close();
            _BackUpCadDocumentView = null;
        }
        public void CloseDxfDwgDrawingExportMainView()
        {
            _DxfDwgDrawingExportMainView.Close();
            _DxfDwgDrawingExportMainView = null;
        }
    }
}
