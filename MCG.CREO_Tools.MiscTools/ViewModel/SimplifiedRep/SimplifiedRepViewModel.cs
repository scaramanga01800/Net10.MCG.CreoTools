using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.SimplifiedRep;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.MiscTools.ViewModel.SimplifiedRep
{
    public class SimplifiedRepViewModel : ObservableObject, ISimplifiedRepViewModel
    {
        #region [REGION] Properties from Interface
        public SimplifiedRepDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private Dispatcher MainDispatcher { get; set; }
        #endregion

        #region [REGION] Commands
        public ICommand CommandReadAsm { get => new RelayCommand(() => ExecuteReadAsm()); }
        public ICommand CommandCreateSimpRep { get => new RelayCommand(() => ExecuteCreateSimpRep()); }
        public ICommand CommandCopySimpRep { get => new RelayCommand(() => ExecuteCopySimpRep()); }
        public ICommand CommandUpdateSimpRep { get => new RelayCommand(() => ExecuteUpdateSimpRep()); }
        public ICommand CommandDeleteSimpRep { get => new RelayCommand(() => ExecuteDeleteSimpRep()); }
        public ICommand CommandActivateSimpRep { get => new RelayCommand(() => ExecuteActivateSimpRep()); }
        public ICommand CommandSaveModel { get => new RelayCommand(() => ExecuteSaveModel()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;

        public SimplifiedRepViewModel(ICreoSessionProvider creoSessionProvider,
                                      ICreoModelService creoModelService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;

                CurrentDataContext = new SimplifiedRepDataContext();
                MainDispatcher = Dispatcher.CurrentDispatcher;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDataContext.IsCreoConnected = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDataContext.IsCreoConnected = e;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteReadAsm()
        {
            try
            {
                CurrentDataContext.ListItem.Clear();
                CurrentDataContext.ListSimpRepName.Clear();
                CurrentDataContext.ActiveModelName = string.Empty;
                CurrentDataContext.IsAssemblyLoaded = false;
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreateSimpRep()
        {
            try
            {
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCopySimpRep()
        {
            try
            {
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateSimpRep()
        {
            try
            {
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDeleteSimpRep()
        {
            try
            {
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteActivateSimpRep()
        {
            try
            {
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSaveModel()
        {
            try
            {
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenHelp()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("SRP_UserGuide"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
