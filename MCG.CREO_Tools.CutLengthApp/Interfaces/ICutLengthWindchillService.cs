using MCG.CREO_Tools.CutLengthApp.ViewModel;
using System.Windows;

namespace MCG.CREO_Tools.CutLengthApp.Interfaces
{
    public interface ICutLengthWindchillService
    {
        void closeCutLengthBulkQuantity();
        void ShowCutLengthBulkQuantity(double currentQuantity, bool isAlreadyCreated = false);
        (MessageBoxResult ReturnValue, double Quantity) ShowDialogCutLengthBulkQuantity(double currentQuantity);

        void closeCutLengthCutUpdatePartView();
        void ShowBomCutLengthCutUpdatePartView(CutLengthCutPart CurrentPart, bool isAlreadyCreated = false);
        MessageBoxResult ShowDialogCutLengthCutUpdatePartView(CutLengthCutPart CurrentPart);
    }
}