using MCG.Tools.PurchaseOrderFollowUp.Interfaces;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MCG.Tools.PurchaseOrderFollowUp.View;

namespace MCG.Tools.PurchaseOrderFollowUp.Services
{
    public class PurchaseOrderFollowWindowService : IPurchaseOrderFollowWindowService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _PurchaseOrderFollowCreateUpdateView;
        private Window _PurchaseOrderFollowListRequestView;
        private Window _PurchaseOrderFollowUpCreateUpdateVendorView;
        private Window _PurchaseOrderFollowUpDuplicate;
        private Window _PurchaseOrderFollowUpExtendedPartView;
        private Window _PurchaseOrderFollowUpInternalOrderRequestView;
        private Window _PurchaseOrderFollowUpSelectVendorView;

        public PurchaseOrderFollowWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowDialogPurchaseOrderFollowCreateUpdateView(object currentDataContext)
        {
            _PurchaseOrderFollowCreateUpdateView = _serviceProvider.GetRequiredService<PurchaseOrderFollowCreateUpdateView>();
            _PurchaseOrderFollowCreateUpdateView.DataContext = currentDataContext; // Assurez-vous que votre fenêtre a une propriété DataContext
            // On l'affiche à l'écran
            _PurchaseOrderFollowCreateUpdateView.ShowDialog(); // (Ou .Show() si vous ne voulez pas bloquer la fenêtre principale)
        }

        public void ShowDialogPurchaseOrderFollowListRequestView(object currentDataContext)
        {
            _PurchaseOrderFollowListRequestView = _serviceProvider.GetRequiredService<PurchaseOrderFollowListRequestView>();
            _PurchaseOrderFollowListRequestView.DataContext = currentDataContext;
            // On l'affiche à l'écran
            _PurchaseOrderFollowListRequestView.ShowDialog(); // (Ou .Show() si vous ne voulez pas bloquer la fenêtre principale)
        }

        public void ShowDialogPurchaseOrderFollowUpCreateUpdateVendorView(object currentDataContext)
        {
            _PurchaseOrderFollowUpCreateUpdateVendorView = _serviceProvider.GetRequiredService<PurchaseOrderFollowUpCreateUpdateVendorView>();
            _PurchaseOrderFollowUpCreateUpdateVendorView.DataContext = currentDataContext;
            // On l'affiche à l'écran
            _PurchaseOrderFollowUpCreateUpdateVendorView.ShowDialog(); // (Ou .Show() si vous ne voulez pas bloquer la fenêtre principale)
        }

        public void ShowDialogPurchaseOrderFollowUpDuplicate(object currentDataContext)
        {
            _PurchaseOrderFollowUpDuplicate = _serviceProvider.GetRequiredService<PurchaseOrderFollowUpDuplicate>();
            _PurchaseOrderFollowUpDuplicate.DataContext = currentDataContext;
            // On l'affiche à l'écran
            _PurchaseOrderFollowUpDuplicate.ShowDialog(); // (Ou .Show() si vous ne voulez pas bloquer la fenêtre principale)
        }

        public void ShowDialogPurchaseOrderFollowUpExtendedPartView(object currentDataContext)
        {
            _PurchaseOrderFollowUpExtendedPartView = _serviceProvider.GetRequiredService<PurchaseOrderFollowUpExtendedPartView>();
            // On l'affiche à l'écran
            _PurchaseOrderFollowUpExtendedPartView.ShowDialog(); // (Ou .Show() si vous ne voulez pas bloquer la fenêtre principale)
        }

        public bool? ShowDialogPurchaseOrderFollowUpInternalOrderRequestView(object currentDataContext)
        {
            _PurchaseOrderFollowUpInternalOrderRequestView = _serviceProvider.GetRequiredService<PurchaseOrderFollowUpInternalOrderRequestView>();
            _PurchaseOrderFollowUpInternalOrderRequestView.DataContext = currentDataContext;
            // On l'affiche à l'écran
            _PurchaseOrderFollowUpInternalOrderRequestView.ShowDialog(); // (Ou .Show() si vous ne voulez pas bloquer la fenêtre principale)
            return _PurchaseOrderFollowUpInternalOrderRequestView.DialogResult;
        }

        public bool? ShowDialogPurchaseOrderFollowUpSelectVendorView(object currentDataContext)
        {
            _PurchaseOrderFollowUpSelectVendorView = _serviceProvider.GetRequiredService<PurchaseOrderFollowUpSelectVendorView>();
            _PurchaseOrderFollowUpSelectVendorView.DataContext = currentDataContext;
            // On l'affiche à l'écran
            _PurchaseOrderFollowUpSelectVendorView.ShowDialog(); // (Ou .Show() si vous ne voulez pas bloquer la fenêtre principale)
            return _PurchaseOrderFollowUpSelectVendorView.DialogResult;
        }


        public void ShowPurchaseOrderFollowCreateUpdateView(object currentDataContext)
        {
            _PurchaseOrderFollowCreateUpdateView = _serviceProvider.GetRequiredService<PurchaseOrderFollowCreateUpdateView>();

            // On l'affiche à l'écran
            _PurchaseOrderFollowCreateUpdateView.Show(); // (Ou .Show() si vous ne voulez pas bloquer la fenêtre principale)
        }

        public void ShowPurchaseOrderFollowListRequestView(object currentDataContext)
        {
            _PurchaseOrderFollowListRequestView = _serviceProvider.GetRequiredService<PurchaseOrderFollowListRequestView>();
            _PurchaseOrderFollowListRequestView.DataContext = currentDataContext;
            // On l'affiche à l'écran
            _PurchaseOrderFollowListRequestView.Show(); // (Ou .Show() si vous ne voulez pas bloquer
        }

        public void ShowPurchaseOrderFollowUpCreateUpdateVendorView(object currentDataContext)
        {
            _PurchaseOrderFollowUpCreateUpdateVendorView = _serviceProvider.GetRequiredService<PurchaseOrderFollowUpCreateUpdateVendorView>();
            _PurchaseOrderFollowUpCreateUpdateVendorView.DataContext = currentDataContext;
            // On l'affiche à l'écran
            _PurchaseOrderFollowUpCreateUpdateVendorView.Show(); // (Ou .Show() si vous ne voulez pas bloquer
        }

        public void ShowPurchaseOrderFollowUpDuplicate(object currentDataContext)
        {
            _PurchaseOrderFollowUpDuplicate = _serviceProvider.GetRequiredService<PurchaseOrderFollowUpDuplicate>();
            _PurchaseOrderFollowUpDuplicate.DataContext = currentDataContext;
            // On l'affiche à l'écran
            _PurchaseOrderFollowUpDuplicate.Show(); // (Ou .Show() si vous ne voulez pas bloquer
        }

        public void ShowPurchaseOrderFollowUpExtendedPartView(object currentDataContext)
        {
            _PurchaseOrderFollowUpExtendedPartView = _serviceProvider.GetRequiredService<PurchaseOrderFollowUpExtendedPartView>();
            _PurchaseOrderFollowUpExtendedPartView.DataContext = currentDataContext;
            // On l'affiche à l'écran
            _PurchaseOrderFollowUpExtendedPartView.Show(); // (Ou .Show() si vous ne voulez pas bloquer
        }

        public void ShowPurchaseOrderFollowUpInternalOrderRequestView(object currentDataContext)
        {
            _PurchaseOrderFollowUpInternalOrderRequestView = _serviceProvider.GetRequiredService<PurchaseOrderFollowUpInternalOrderRequestView>();
            _PurchaseOrderFollowUpInternalOrderRequestView.DataContext = currentDataContext;
            // On l'affiche à l'écran
            _PurchaseOrderFollowUpInternalOrderRequestView.Show(); // (Ou .Show() si vous ne voulez pas bloquer
        }

        public void ShowPurchaseOrderFollowUpSelectVendorView(object currentDataContext)
        {
            _PurchaseOrderFollowUpSelectVendorView = _serviceProvider.GetRequiredService<PurchaseOrderFollowUpSelectVendorView>();
            _PurchaseOrderFollowUpSelectVendorView.DataContext = currentDataContext;
            // On l'affiche à l'écran
            _PurchaseOrderFollowUpSelectVendorView.Show(); // (Ou .Show() si vous ne voulez pas bloquer  
        }


        public void ClosePurchaseOrderFollowCreateUpdateView()
        {
            if (_PurchaseOrderFollowCreateUpdateView != null)
            {
                _PurchaseOrderFollowCreateUpdateView.Close();
                _PurchaseOrderFollowCreateUpdateView = null;
            }
        }

        public void ClosePurchaseOrderFollowListRequestView()
        {
            if (_PurchaseOrderFollowListRequestView != null)
            {
                _PurchaseOrderFollowListRequestView.Close();
                _PurchaseOrderFollowListRequestView = null;
            }
        }

        public void ClosePurchaseOrderFollowUpCreateUpdateVendorView()
        {
            if (_PurchaseOrderFollowUpCreateUpdateVendorView != null)
            {
                _PurchaseOrderFollowUpCreateUpdateVendorView.Close();
                _PurchaseOrderFollowUpCreateUpdateVendorView = null;
            }
        }

        public void ClosePurchaseOrderFollowUpDuplicate()
        {
            if (_PurchaseOrderFollowUpDuplicate != null)
            {
                _PurchaseOrderFollowUpDuplicate.Close();
                _PurchaseOrderFollowUpDuplicate = null;
            }
        }

        public void ClosePurchaseOrderFollowUpExtendedPartView()
        {
            if (_PurchaseOrderFollowUpExtendedPartView != null)
            {
                _PurchaseOrderFollowUpExtendedPartView.Close();
                _PurchaseOrderFollowUpExtendedPartView = null;
            }
        }

        public void ClosePurchaseOrderFollowUpInternalOrderRequestView()
        {
            if (_PurchaseOrderFollowUpInternalOrderRequestView != null)
            {
                _PurchaseOrderFollowUpInternalOrderRequestView.Close();
                _PurchaseOrderFollowUpInternalOrderRequestView = null;
            }
        }

        public void ClosePurchaseOrderFollowUpSelectVendorView()
        {
            if (_PurchaseOrderFollowUpSelectVendorView != null)
            {
                _PurchaseOrderFollowUpSelectVendorView.Close();
                _PurchaseOrderFollowUpSelectVendorView = null;
            }
        }
    }
}
