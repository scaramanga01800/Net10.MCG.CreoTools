using MCG.CREO_Tools.ProfileApp.Configuration;
using MCG.CREO_Tools.ProfileApp.ViewModel;
using System.Windows;

namespace MCG.CREO_Tools.ProfileApp.Interfaces
{
    public interface IProfileAppWindowService
    {
        void ShowProfileUpdateProfileView(
            ProfileGenericItem currentProfile,
            ProfileConfiguration currentAppProfileConfiguration,
            bool isAlreadyCreated = false);

        MessageBoxResult ShowDialogProfileUpdateProfileView(
            ProfileGenericItem currentProfile,
            ProfileConfiguration currentAppProfileConfiguration);

        void CloseProfileUpdateProfileView();
    }
}