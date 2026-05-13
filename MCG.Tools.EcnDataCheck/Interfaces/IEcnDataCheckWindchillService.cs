using System.Windows;

namespace MCG.Tools.EcnDataCheck.Interfaces
{
    public interface IEcnDataCheckWindchillService
    {
        void CloseEcnDataCheckEcaSelection();
        MessageBoxResult ShowDialogEcnDataCheckEcaSelection();
        void ShowEcnDataCheckEcaSelection();
    }
}