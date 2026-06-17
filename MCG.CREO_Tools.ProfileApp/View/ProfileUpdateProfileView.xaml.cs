using Fluent;
using MCG.CREO_Tools.ProfileApp.Configuration;
using MCG.CREO_Tools.ProfileApp.Exceptions;
using MCG.CREO_Tools.ProfileApp.ViewModel;

namespace MCG.CREO_Tools.ProfileApp.View
{
    public partial class ProfileUpdateProfileView : RibbonWindow
    {
        public ProfileUpdateProfileViewModel CurrentDataContext { get; set; }

        public ProfileUpdateProfileView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                ProfileException.SendMessageBox(this.GetType().Name, ex);
            }
        }


        public void Initialize(
                    ProfileGenericItem currentProfile,
                    ProfileConfiguration currentAppProfileConfiguration)
        {
            try
            {
                CurrentDataContext = new ProfileUpdateProfileViewModel(
                    currentProfile,
                    currentAppProfileConfiguration);

                DataContext = CurrentDataContext;
            }
            catch (Exception ex)
            {
                ProfileException.SendMessageBox(this.GetType().Name, ex);
            }
        }


    }
}
