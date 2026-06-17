using MCG.CREO_Tools.ProfileApp.Configuration;
using MCG.CREO_Tools.ProfileApp.Interfaces;
using MCG.CREO_Tools.ProfileApp.View;
using MCG.CREO_Tools.ProfileApp.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MCG.CREO_Tools.ProfileApp.Services
{
    public class ProfileAppWindowService : IProfileAppWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        private Window _profileUpdateProfileView;

        public ProfileAppWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // ── ProfileUpdateProfileView ───────────────────────────────

        public void ShowProfileUpdateProfileView(
            ProfileGenericItem currentProfile,
            ProfileConfiguration currentAppProfileConfiguration,
            bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_profileUpdateProfileView != null && _profileUpdateProfileView.IsVisible)
                {
                    _profileUpdateProfileView.Activate();
                    return;
                }
            }

            _profileUpdateProfileView = _serviceProvider.GetRequiredService<ProfileUpdateProfileView>();
            ((ProfileUpdateProfileView)_profileUpdateProfileView)
                .Initialize(currentProfile, currentAppProfileConfiguration);
            _profileUpdateProfileView.Show();
        }


        public MessageBoxResult ShowDialogProfileUpdateProfileView(
                    ProfileGenericItem currentProfile,
                    ProfileConfiguration currentAppProfileConfiguration)
        {
            var view = _serviceProvider.GetRequiredService<ProfileUpdateProfileView>();
            view.Initialize(currentProfile, currentAppProfileConfiguration);

            _profileUpdateProfileView = view;
            _profileUpdateProfileView.ShowDialog();

            // Récupère le résultat depuis le ViewModel après fermeture
            return view.CurrentDataContext?.Return ?? MessageBoxResult.Cancel;
        }


        public void CloseProfileUpdateProfileView()
        {
            if (_profileUpdateProfileView != null && _profileUpdateProfileView.IsVisible)
            {
                _profileUpdateProfileView.Close();
                _profileUpdateProfileView = null;
            }
        }
    }
}