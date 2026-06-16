using MCG.CREO_Tools.MassUpdateAttribute.Interfaces;
using MCG.CREO_Tools.MassUpdateAttribute.View;
using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MCG.CREO_Tools.MassUpdateAttribute.Services
{
    public class MassUpdateAttributeWindowService : IMassUpdateAttributeWindowService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _createNewCadDocumentFluentWindow;
        private Window _massUpdateAttributeChangeNameWindow;
        private Window _updateRelationsParametersMainView;

        public MassUpdateAttributeWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowCreateNewCadDocumentFluentWindow(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_createNewCadDocumentFluentWindow != null
                    && _createNewCadDocumentFluentWindow.IsVisible)
                {
                    _createNewCadDocumentFluentWindow.Activate();
                    return;
                }
            }

            _createNewCadDocumentFluentWindow =
                _serviceProvider.GetRequiredService<CreateNewCadDocumentFluentWindow>();
            _createNewCadDocumentFluentWindow.Show();
        }
        public void ShowDialogCreateNewCadDocumentFluentWindow()
        {
            _createNewCadDocumentFluentWindow =
                _serviceProvider.GetRequiredService<CreateNewCadDocumentFluentWindow>();
            _createNewCadDocumentFluentWindow.ShowDialog();
        }
        public void CloseCreateNewCadDocumentFluentWindow()
        {
            if (_createNewCadDocumentFluentWindow != null
                && _createNewCadDocumentFluentWindow.IsVisible)
            {
                _createNewCadDocumentFluentWindow.Close();
                _createNewCadDocumentFluentWindow = null;
            }
        }

        public void ShowMassUpdateAttributeChangeName(MassUpdateAttributeViewModel dataContext, bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_massUpdateAttributeChangeNameWindow != null
                    && _massUpdateAttributeChangeNameWindow.IsVisible)
                {
                    _massUpdateAttributeChangeNameWindow.Activate();
                    return;
                }
            }
            _massUpdateAttributeChangeNameWindow = _serviceProvider.GetRequiredService<MassUpdateAttributeChangeName>();
            ((MassUpdateAttributeChangeName)_massUpdateAttributeChangeNameWindow).SetDataContext(dataContext);
            _massUpdateAttributeChangeNameWindow.Show();
        }
        public void ShowDialogMassUpdateAttributeChangeName(MassUpdateAttributeViewModel dataContext)
        {
            _massUpdateAttributeChangeNameWindow = _serviceProvider.GetRequiredService<MassUpdateAttributeChangeName>();
            ((MassUpdateAttributeChangeName)_massUpdateAttributeChangeNameWindow).SetDataContext(dataContext);
            _massUpdateAttributeChangeNameWindow.ShowDialog();
        }
        public void CloseMassUpdateAttributeChangeName()
        {
            if (_massUpdateAttributeChangeNameWindow != null
                && _massUpdateAttributeChangeNameWindow.IsVisible)
            {
                _massUpdateAttributeChangeNameWindow.Close();
                _massUpdateAttributeChangeNameWindow = null;
            }
        }

        public void ShowUpdateRelationsParametersMainView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_updateRelationsParametersMainView != null
                    && _updateRelationsParametersMainView.IsVisible)
                {
                    _updateRelationsParametersMainView.Activate();
                    return;
                }
            }

            _updateRelationsParametersMainView =
                _serviceProvider.GetRequiredService<UpdateRelationsParametersMainView>();
            _updateRelationsParametersMainView.Show();
        }
        public void ShowDialogUpdateRelationsParametersMainView()
        {
            _updateRelationsParametersMainView =
                _serviceProvider.GetRequiredService<UpdateRelationsParametersMainView>();
            _updateRelationsParametersMainView.ShowDialog();
        }
        public void CloseUpdateRelationsParametersMainView()
        {
            if (_updateRelationsParametersMainView != null
                && _updateRelationsParametersMainView.IsVisible)
            {
                _updateRelationsParametersMainView.Close();
                _updateRelationsParametersMainView = null;
            }
        }
    }
}
