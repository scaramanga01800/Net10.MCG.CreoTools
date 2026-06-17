using MCG.CREO_Tools.ProfileApp.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.ProfileApp.View
{
    public interface IProfileUpdateProfileViewModel
    {
        ProfileGenericItem ProfileItem { get; set; }

        ObservableCollection<string> ListAllMaterial { get; set; }
        ObservableCollection<string> ListGeneric3D { get; set; }
        ObservableCollection<string> ListGenericDrwComplete { get; set; }
        ObservableCollection<string> ListGenericDrwBroken { get; set; }
        ObservableCollection<string> ListStdType { get; set; }

        MessageBoxResult Return { get; set; }

        ICommand CommandCreateUpdatePart { get; }
    }
}
