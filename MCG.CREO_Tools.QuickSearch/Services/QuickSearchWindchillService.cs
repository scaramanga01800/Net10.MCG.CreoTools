using MCG.CREO_Tools.QuickSearch.Interfaces;
using MCG.CREO_Tools.QuickSearch.View;
using MCG.CREO_Tools.QuickSearch.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MCG.CREO_Tools.QuickSearch.Services
{
    public class QuickSearchWindchillService : IQuickSearchWindchillService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _quickSearchUpdatePartView;
        private Window _quickSearchWindowClassSubClassFromNumberView;
        private Window _quickSearchWindowRefDocFromNumberView;

        public QuickSearchWindchillService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowQuickSearchUpdatePartView(QuickSearchPart selectedPartItem, bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_quickSearchUpdatePartView != null && _quickSearchUpdatePartView.IsVisible)
                {
                    _quickSearchUpdatePartView.Activate();
                    return;
                }
            }

            _quickSearchUpdatePartView = _serviceProvider.GetRequiredService<QuickSearchUpdatePartView>();
            ((QuickSearchUpdatePartView)_quickSearchUpdatePartView)
                .SetProperties(selectedPartItem);

            _quickSearchUpdatePartView.Show();
        }
        public MessageBoxResult ShowDialogQuickSearchUpdatePartView(QuickSearchPart selectedPartItem)
        {
            var view = _serviceProvider.GetRequiredService<QuickSearchUpdatePartView>();
            view.SetProperties(selectedPartItem);

            _quickSearchUpdatePartView = view;
            _quickSearchUpdatePartView.ShowDialog();

            // Récupère le résultat depuis le ViewModel après fermeture
            return view.CurrentDataContext?.Return ?? MessageBoxResult.Cancel;
        }
        public void CloseQuickSearchUpdatePartView()
        {
            if (_quickSearchUpdatePartView != null && _quickSearchUpdatePartView.IsVisible)
            {
                _quickSearchUpdatePartView.Close();
                _quickSearchUpdatePartView = null;
            }
        }

        public void ShowQuickSearchWindowClassSubClassFromNumberView(List<string> listStdShown, bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_quickSearchWindowClassSubClassFromNumberView != null
                    && _quickSearchWindowClassSubClassFromNumberView.IsVisible)
                {
                    _quickSearchWindowClassSubClassFromNumberView.Activate();
                    return;
                }
            }

            _quickSearchWindowClassSubClassFromNumberView =
                _serviceProvider.GetRequiredService<QuickSearchWindowClassSubClassFromNumberView>();

            ((QuickSearchWindowClassSubClassFromNumberView)_quickSearchWindowClassSubClassFromNumberView)
                .SetProperties(listStdShown);

            _quickSearchWindowClassSubClassFromNumberView.Show();
        }
        public MessageBoxResult ShowDialogQuickSearchWindowClassSubClassFromNumberView(List<string> listStdShown)
        {
            var view = _serviceProvider.GetRequiredService<QuickSearchWindowClassSubClassFromNumberView>();
            view.SetProperties(listStdShown);

            _quickSearchWindowClassSubClassFromNumberView = view;
            _quickSearchWindowClassSubClassFromNumberView.ShowDialog();

            bool? dialogResult = _quickSearchWindowClassSubClassFromNumberView.ShowDialog();
            return dialogResult == true ? MessageBoxResult.OK : MessageBoxResult.Cancel;
        }
        public void CloseQuickSearchWindowClassSubClassFromNumberView()
        {
            if (_quickSearchWindowClassSubClassFromNumberView != null
                && _quickSearchWindowClassSubClassFromNumberView.IsVisible)
            {
                _quickSearchWindowClassSubClassFromNumberView.Close();
                _quickSearchWindowClassSubClassFromNumberView = null;
            }
        }

        public void ShowQuickSearchWindowRefDocFromNumberView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_quickSearchWindowRefDocFromNumberView != null
                    && _quickSearchWindowRefDocFromNumberView.IsVisible)
                {
                    _quickSearchWindowRefDocFromNumberView.Activate();
                    return;
                }
            }

            _quickSearchWindowRefDocFromNumberView =
                _serviceProvider.GetRequiredService<QuickSearchWindowRefDocFromNumberView>();

            _quickSearchWindowRefDocFromNumberView.Show();
        }
        public MessageBoxResult ShowDialogQuickSearchWindowRefDocFromNumberView()
        {
            var view = _serviceProvider.GetRequiredService<QuickSearchWindowRefDocFromNumberView>();

            _quickSearchWindowRefDocFromNumberView = view;
            bool? dialogResult = _quickSearchWindowRefDocFromNumberView.ShowDialog();

            return dialogResult == true ? MessageBoxResult.OK : MessageBoxResult.Cancel;
        }
        public void CloseQuickSearchWindowRefDocFromNumberView()
        {
            if (_quickSearchWindowRefDocFromNumberView != null
                && _quickSearchWindowRefDocFromNumberView.IsVisible)
            {
                _quickSearchWindowRefDocFromNumberView.Close();
                _quickSearchWindowRefDocFromNumberView = null;
            }
        }

        public Task<QuickSearchShortCutViewModel?> ShowDialogQuickSearchWindowClassSubClassFromNumberViewAsync(List<string> listStdShown)
        {
            var view = _serviceProvider
                .GetRequiredService<QuickSearchWindowClassSubClassFromNumberView>();

            view.SetProperties(listStdShown);

            _quickSearchWindowClassSubClassFromNumberView = view;

            view.CurrentDataContext.ActionOpenClassSubClassEvent += View_OpenClassSubClassEvent;
            bool? dialogResult = view.ShowDialog();
            view.CurrentDataContext.ActionOpenClassSubClassEvent -= View_OpenClassSubClassEvent;

            var selected = dialogResult == true
                ? view.CurrentDataContext?.ClassSubClass
                : null;

            return Task.FromResult(selected);
        }
        public Task<QuickSearchShortCutViewModel?> ShowQuickSearchWindowClassSubClassFromNumberViewAsync(List<string> listStdShown, bool isAlreadyCreated = false)
        {
            // Si déjà ouverte → on active simplement la fenêtre existante
            if (isAlreadyCreated
                && _quickSearchWindowClassSubClassFromNumberView != null
                && _quickSearchWindowClassSubClassFromNumberView.IsVisible)
            {
                _quickSearchWindowClassSubClassFromNumberView.Activate();
                return Task.FromResult<QuickSearchShortCutViewModel?>(null);
            }

            var view = _serviceProvider
                .GetRequiredService<QuickSearchWindowClassSubClassFromNumberView>();

            view.SetProperties(listStdShown);

            var tcs = new TaskCompletionSource<QuickSearchShortCutViewModel?>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            // Quand la fenêtre se ferme → on remonte le résultat
            void OnClosed(object? s, EventArgs e)
            {
                view.Closed -= OnClosed;   // ✅ désabonnement → évite memory leak
                var result = view.DialogResult == true
                    ? view.CurrentDataContext?.ClassSubClass
                    : null;

                tcs.TrySetResult(result);
            }

            view.Closed += OnClosed;

            _quickSearchWindowClassSubClassFromNumberView = view;
            view.Show();   // ✅ Non modale → fenêtre principale toujours utilisable

            return tcs.Task;
        }

        public event EventHandler<QuickSearchShortCutViewModel>? OpenClassSubClassEvent;
        private void View_OpenClassSubClassEvent(QuickSearchShortCutViewModel item)
        {
            OpenClassSubClassEvent?.Invoke(this, item);
        }
    }
}
