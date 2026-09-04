using MCG.Tools.EcnDataCheck.View;
using System.Windows;

namespace MCG.Tools.EcnDataCheck.Interfaces
{
    public interface IEcnDataCheckWindchillService
    {
        void CloseEcnDataCheckEcaSelection();
        MessageBoxResult ShowDialogEcnDataCheckEcaSelection(IEcnDataCheckDataContext dataContext);
        void ShowEcnDataCheckEcaSelection(IEcnDataCheckDataContext dataContext);
    }
}