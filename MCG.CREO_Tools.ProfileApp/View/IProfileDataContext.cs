using MCG.CREO_Tools.ProfileApp.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace MCG.CREO_Tools.ProfileApp.View
{
    public interface IProfileDataContext
    {
        ObservableCollection<ProfileTypeItem> ListProfileType { get; set; }
        ProfileTypeItem CurrentProfileType { get; set; }
        BitmapImage ProfileTypeImage { get; set; }
        byte[] ProfileTypeImageFromDb { get; set; }

        ObservableCollection<string> ListMaterial { get; set; }
        string SelectedMaterial { get; set; }

        ObservableCollection<string> ListGrpCreator { get; set; }
        string SelectedGrpCreator { get; set; }

        ObservableCollection<ProfileDrwLocation> ListDrwLocation { get; set; }
        ProfileDrwLocation SelectedDrwLocation { get; set; }

        string CurrentPartNumber { get; set; }
        double CurrentLength { get; set; }

        bool IsDrwBrokenView { get; set; }

        ObservableCollection<ProfileGenericItem> ListProfileShown { get; set; }
        ProfileGenericItem SelectedItem { get; set; }

        bool IsCreoEnable { get; set; }
        bool ActionInProgress { get; set; }
        bool IsEditMode { get; set; }
        bool IsAdminToolsEnabled { get; set; }

    }
}
