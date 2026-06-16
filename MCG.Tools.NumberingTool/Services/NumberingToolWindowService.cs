using System.Windows;
using MCG.Tools.NumberingTool.Exceptions;
using MCG.Tools.NumberingTool.Interfaces;
using MCG.Tools.NumberingTool.View;
using MCG.Tools.NumberingTool.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.Tools.NumberingTool.Services
{
    public class NumberingToolWindowService : INumberingToolWindowService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _NumberingToolCreateSeveralFluentView;
        private Window _NumberingToolUpdateCreateFluentView;
        private Window _NumberingToolFluentMainView;


        public event EventHandler? CreateNumberRequested;
        public event EventHandler? UseNumberRequested;

        private bool _isMainViewSubscribed;

        public NumberingToolWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        private void OnCreateNumber(object? sender, EventArgs e)
            => CreateNumberRequested?.Invoke(sender, e);
        private void OnUseNumber(object? sender, EventArgs e)
            => UseNumberRequested?.Invoke(sender, e);


        private void SubscribeMainViewEvents(NumberingToolFluentMainView view)
        {
            if (_isMainViewSubscribed) return;

            view.CreateNumberEvent += OnCreateNumber;
            view.UseNumberEvent += OnUseNumber;

            // Nettoyage automatique à la fermeture
            view.Closed += OnMainViewClosed;

            _isMainViewSubscribed = true;
        }

        private void OnMainViewClosed(object? sender, EventArgs e)
        {
            if (sender is NumberingToolFluentMainView view)
            {
                view.CreateNumberEvent -= OnCreateNumber;
                view.UseNumberEvent -= OnUseNumber;
                view.Closed -= OnMainViewClosed;
            }

            _NumberingToolFluentMainView = null;
            _isMainViewSubscribed = false;
        }


        public void ShowNumberingToolCreateSeveralFluentView(NumberingToolViewModel currentVm)
        {
            try
            {
                if (_NumberingToolCreateSeveralFluentView == null || !_NumberingToolCreateSeveralFluentView.IsLoaded)
                {
                    _NumberingToolCreateSeveralFluentView = _serviceProvider.GetRequiredService<NumberingToolCreateSeveralFluentView>();
                    ((NumberingToolCreateSeveralFluentView)_NumberingToolCreateSeveralFluentView).SetNumberingToolCreateSeveralFluentViewProperties(currentVm);
                    _NumberingToolCreateSeveralFluentView.Show();
                }
                else
                {
                    _NumberingToolCreateSeveralFluentView.Activate();
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ShowNumberingToolFluentMainView(bool pNoRangeAuthorized = false, bool isAlreadyCreated = false)
        {
            try
            {
                if (isAlreadyCreated)
                {
                    if (_NumberingToolFluentMainView != null && _NumberingToolFluentMainView.IsVisible)
                    {
                        _NumberingToolFluentMainView.Activate();
                        return;
                    }
                }


                var view = _serviceProvider.GetRequiredService<NumberingToolFluentMainView>();
                _NumberingToolFluentMainView = view;
                view.SetNumberingToolFluentMainViewProperties(pNoRangeAuthorized);

                SubscribeMainViewEvents(view);

                _NumberingToolFluentMainView.Show();
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ShowNumberingToolUpdateCreateFluentView(bool CurrentIsNewNumber, NumberingToolTemplate CurrentSelectedNumberingTemplate, List<string> CurrentSearchProductList, List<string> CurrentListFormat, NumberingToolItem AlreadyCreatedItem = null, bool CurrentIsDetailShown = true)
        {
            try
            {
                if (_NumberingToolUpdateCreateFluentView == null || !_NumberingToolUpdateCreateFluentView.IsLoaded)
                {
                    _NumberingToolUpdateCreateFluentView = _serviceProvider.GetRequiredService<NumberingToolUpdateCreateFluentView>();
                    ((NumberingToolUpdateCreateFluentView)_NumberingToolUpdateCreateFluentView).SetNumberingToolUpdateCreateFluentViewProperties(CurrentIsNewNumber, CurrentSelectedNumberingTemplate, CurrentSearchProductList, CurrentListFormat, AlreadyCreatedItem, CurrentIsDetailShown);
                    _NumberingToolUpdateCreateFluentView.Show();
                }
                else
                {
                    _NumberingToolUpdateCreateFluentView.Activate();
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ShowDialogNumberingToolCreateSeveralFluentView(NumberingToolViewModel currentVm)
        {
            try
            {
                if (_NumberingToolCreateSeveralFluentView == null || !_NumberingToolCreateSeveralFluentView.IsLoaded)
                {
                    _NumberingToolCreateSeveralFluentView = _serviceProvider.GetRequiredService<NumberingToolCreateSeveralFluentView>();
                    ((NumberingToolCreateSeveralFluentView)_NumberingToolCreateSeveralFluentView).SetNumberingToolCreateSeveralFluentViewProperties(currentVm);
                    _NumberingToolCreateSeveralFluentView.ShowDialog();
                }
                else
                {
                    _NumberingToolCreateSeveralFluentView.Activate();
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ShowDialogNumberingToolFluentMainView(bool pNoRangeAuthorized = false)
        {
            try
            {
                if (_NumberingToolFluentMainView == null || !_NumberingToolFluentMainView.IsLoaded)
                {

                    var view = _serviceProvider.GetRequiredService<NumberingToolFluentMainView>();
                    _NumberingToolFluentMainView = view;

                    view.SetNumberingToolFluentMainViewProperties(pNoRangeAuthorized);

                    SubscribeMainViewEvents(view);

                    _NumberingToolFluentMainView.ShowDialog();
                }
                else
                {
                    _NumberingToolFluentMainView.Activate();
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ShowDialogNumberingToolUpdateCreateFluentView(bool CurrentIsNewNumber, NumberingToolTemplate CurrentSelectedNumberingTemplate, List<string> CurrentSearchProductList, List<string> CurrentListFormat, NumberingToolItem AlreadyCreatedItem = null, bool CurrentIsDetailShown = true)
        {
            try
            {
                if (_NumberingToolUpdateCreateFluentView == null || !_NumberingToolUpdateCreateFluentView.IsLoaded)
                {
                    _NumberingToolUpdateCreateFluentView = _serviceProvider.GetRequiredService<NumberingToolUpdateCreateFluentView>();
                    ((NumberingToolUpdateCreateFluentView)_NumberingToolUpdateCreateFluentView).SetNumberingToolUpdateCreateFluentViewProperties(CurrentIsNewNumber, CurrentSelectedNumberingTemplate, CurrentSearchProductList, CurrentListFormat, AlreadyCreatedItem, CurrentIsDetailShown);
                    _NumberingToolUpdateCreateFluentView.ShowDialog();
                }
                else
                {
                    _NumberingToolUpdateCreateFluentView.Activate();
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void CloseNumberingToolCreateSeveralFluentView()
        {
            try
            {
                if (_NumberingToolCreateSeveralFluentView != null && _NumberingToolCreateSeveralFluentView.IsLoaded)
                {
                    _NumberingToolCreateSeveralFluentView.Close();
                    _NumberingToolCreateSeveralFluentView = null;
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void CloseNumberingToolFluentMainView()
        {
            try
            {
                if (_NumberingToolFluentMainView != null && _NumberingToolFluentMainView.IsLoaded)
                {
                    _NumberingToolFluentMainView.Close();
                    _NumberingToolFluentMainView = null;
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void CloseNumberingToolUpdateCreateFluentView()
        {
            try
            {
                if (_NumberingToolUpdateCreateFluentView != null && _NumberingToolUpdateCreateFluentView.IsLoaded)
                {
                    _NumberingToolUpdateCreateFluentView.Close();
                    _NumberingToolUpdateCreateFluentView = null;
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

    }
}
