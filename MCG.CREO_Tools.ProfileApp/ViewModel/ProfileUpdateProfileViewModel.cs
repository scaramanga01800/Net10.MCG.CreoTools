using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CREO_Tools.ProfileApp.Configuration;
using MCG.CREO_Tools.ProfileApp.Exceptions;
using MCG.CREO_Tools.ProfileApp.View;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.ProfileApp.ViewModel
{
    public class ProfileUpdateProfileViewModel : ObservableObject, IProfileUpdateProfileViewModel
    {
        #region [REGION] Properties from Interface
        private ProfileGenericItem _ProfileItem;
        public ProfileGenericItem ProfileItem
        {
            get { return _ProfileItem; }
            set
            {
                if (this._ProfileItem != value)
                {
                    this._ProfileItem = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> ListAllMaterial { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListGeneric3D { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListGenericDrwComplete { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListGenericDrwBroken { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ListStdType { get; set; } = new ObservableCollection<string>();
        #endregion

        #region [REGION] Internal variables
        public MessageBoxResult Return { get; set; }
        #endregion

        #region [REGION] Commands
        public ICommand CommandCreateUpdatePart { get => new RelayCommand(() => ExecuteCreateUpdatePart()); }
        #endregion

        #region [REGION] Init
        public ProfileUpdateProfileViewModel(ProfileGenericItem CurrentProfile, ProfileConfiguration CurrentAppProfileConfiguration)
        {
            try
            {

                if (CurrentAppProfileConfiguration.ListMaterial != null)
                {
                    foreach (var item in CurrentAppProfileConfiguration.ListMaterial)
                        ListAllMaterial.Add(item);
                }

                if (CurrentAppProfileConfiguration.ListGeneric3D != null)
                {
                    foreach (var item in CurrentAppProfileConfiguration.ListGeneric3D)
                        ListGeneric3D.Add(item);
                }

                if (CurrentAppProfileConfiguration.ListGenericDrwComplete != null)
                {
                    foreach (var item in CurrentAppProfileConfiguration.ListGenericDrwComplete)
                        ListGenericDrwComplete.Add(item);
                }

                if (CurrentAppProfileConfiguration.ListGenericDrwBroken != null)
                {
                    foreach (var item in CurrentAppProfileConfiguration.ListGenericDrwBroken)
                        ListGenericDrwBroken.Add(item);
                }

                if (CurrentAppProfileConfiguration.ListStdType != null)
                {
                    foreach (var item in CurrentAppProfileConfiguration.ListStdType)
                        ListStdType.Add(item);
                }

                ProfileItem = CurrentProfile;


            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteCreateUpdatePart(bool InAsynch = false)
        {
            try
            {
                Return = MessageBoxResult.Yes;
            }
            catch (Exception ex)
            {
                ProfileException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
