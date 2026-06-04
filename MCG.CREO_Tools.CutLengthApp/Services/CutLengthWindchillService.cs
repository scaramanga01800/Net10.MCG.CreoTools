using MCG.CREO_Tools.CutLengthApp.Interfaces;
using MCG.CREO_Tools.CutLengthApp.View;
using MCG.CREO_Tools.CutLengthApp.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace MCG.CREO_Tools.CutLengthApp.Services
{
    public class CutLengthWindchillService : ICutLengthWindchillService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _CutLengthBulkQuantity;
        private Window _CutLengthCutUpdatePartView;

        public CutLengthWindchillService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowCutLengthBulkQuantity(double currentQuantity, bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_CutLengthBulkQuantity != null && _CutLengthBulkQuantity.IsVisible)
                {
                    _CutLengthBulkQuantity.Activate();
                    return;
                }
            }
            _CutLengthBulkQuantity = _serviceProvider.GetRequiredService<CutLengthBulkQuantity>();
            _CutLengthBulkQuantity.Show();
        }
        public (MessageBoxResult ReturnValue, double Quantity) ShowDialogCutLengthBulkQuantity(double currentQuantity)
        {
            var dialog = _serviceProvider.GetRequiredService<CutLengthBulkQuantity>();
            _CutLengthBulkQuantity = dialog;

            dialog.SetQuantity(currentQuantity);
            dialog.ShowDialog();

            return (dialog.ReturnValue, dialog.GetQuantity());
        }

        public void ShowBomCutLengthCutUpdatePartView(CutLengthCutPart CurrentPart, bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_CutLengthCutUpdatePartView != null && _CutLengthCutUpdatePartView.IsVisible)
                {
                    _CutLengthCutUpdatePartView.Activate();
                    return;
                }
            }
            _CutLengthCutUpdatePartView = _serviceProvider.GetRequiredService<CutLengthCutUpdatePartView>();
            ((CutLengthCutUpdatePartView) _CutLengthCutUpdatePartView).SetCurrentPart(CurrentPart);
            _CutLengthCutUpdatePartView.Show();
        }
        public MessageBoxResult ShowDialogCutLengthCutUpdatePartView(CutLengthCutPart CurrentPart)
        {
            _CutLengthCutUpdatePartView = _serviceProvider.GetRequiredService<CutLengthCutUpdatePartView>();
            ((CutLengthCutUpdatePartView) _CutLengthCutUpdatePartView).SetCurrentPart(CurrentPart);
            bool? dialogResult = _CutLengthCutUpdatePartView.ShowDialog();

            return dialogResult switch
            {
                true => MessageBoxResult.OK,
                false => MessageBoxResult.Cancel,
                null => MessageBoxResult.Cancel
            };
        }

        public void closeCutLengthBulkQuantity()
        {
            if (_CutLengthBulkQuantity != null)
            {
                _CutLengthBulkQuantity.Close();
                _CutLengthBulkQuantity = null;
            }
        }
        public void closeCutLengthCutUpdatePartView()
        {
            if (_CutLengthCutUpdatePartView != null)
            {
                _CutLengthCutUpdatePartView.Close();
                _CutLengthCutUpdatePartView = null;
            }
        }


    }
}
