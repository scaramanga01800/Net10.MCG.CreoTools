using MCG.CREO_Tools.ProfileApp.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.ProfileApp.View
{
    public interface IProfileViewModel
    {
        ProfileDataContext CurrentDataContext { get; set; }
        ICommand CommandCreateProfile { get; }
        ICommand CommandOpenHelp { get; }
        ICommand CommandEditProfile { get; }
        ICommand CommandAddNewProfile { get; }
    }
}
