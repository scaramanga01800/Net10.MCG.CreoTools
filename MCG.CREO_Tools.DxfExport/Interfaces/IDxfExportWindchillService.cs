using System.Windows;

namespace MCG.CREO_Tools.DxfExport.Interfaces
{
    public interface IDxfExportWindchillService
    {
        void CloseBackUpCadDocumentView();
        void CloseDxfDwgDrawingExportMainView();
        void ShowBackUpCadDocumentView(bool isAlreadyCreated = false);
        MessageBoxResult ShowDialogBackUpCadDocumentView();
        MessageBoxResult ShowDialogDxfDwgDrawingExportMainView();
        void ShowDxfDwgDrawingExportMainView(bool isAlreadyCreated = false);
    }
}