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

        public NumberingToolWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

        public void ShowNumberingToolFluentMainView(bool pNoRangeAuthorized = false)
        {
            try
            {
                if (_NumberingToolFluentMainView == null || !_NumberingToolFluentMainView.IsLoaded)
                {
                    _NumberingToolFluentMainView = _serviceProvider.GetRequiredService<NumberingToolFluentMainView>();
                    ((NumberingToolFluentMainView)_NumberingToolFluentMainView).SetNumberingToolFluentMainViewProperties(pNoRangeAuthorized);
                    _NumberingToolFluentMainView.Show();
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
                    _NumberingToolFluentMainView = _serviceProvider.GetRequiredService<NumberingToolFluentMainView>();
                    ((NumberingToolFluentMainView)_NumberingToolFluentMainView).SetNumberingToolFluentMainViewProperties(pNoRangeAuthorized);
                    _NumberingToolFluentMainView.Show();
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
