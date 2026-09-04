using MCG.Tools.EcnDataCheck.Interfaces;
using MCG.Tools.EcnDataCheck.View;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MCG.Tools.EcnDataCheck.Services
{
    public class EcnDataCheckWindchillService : IEcnDataCheckWindchillService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _EcnDataCheckEcaSelection;

        public EcnDataCheckWindchillService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowEcnDataCheckEcaSelection(IEcnDataCheckDataContext dataContext)
        {
            _EcnDataCheckEcaSelection = _serviceProvider.GetRequiredService<EcnDataCheckEcaSelection>();
            _EcnDataCheckEcaSelection.DataContext = dataContext;
            _EcnDataCheckEcaSelection.Show();
        }

        public MessageBoxResult ShowDialogEcnDataCheckEcaSelection(IEcnDataCheckDataContext dataContext)
        {
            _EcnDataCheckEcaSelection = _serviceProvider.GetRequiredService<EcnDataCheckEcaSelection>();
            _EcnDataCheckEcaSelection.DataContext = dataContext;
            var DialogResult = _EcnDataCheckEcaSelection.ShowDialog();
            if (DialogResult.HasValue)
            {
                return DialogResult.Value ? MessageBoxResult.OK : MessageBoxResult.Cancel;
            }
            else
            {
                return MessageBoxResult.None;
            }
        }

        public void CloseEcnDataCheckEcaSelection()
        {
            _EcnDataCheckEcaSelection.Close();
            _EcnDataCheckEcaSelection = null;
        }
    }
}
