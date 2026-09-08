using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.SimplifiedRep;
using pfcls;
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
        private readonly ICreoSimpRepService _creoSimpRepService;
        private readonly ICreoParameterService _creoParameterService;

        private IpfcModel? _activeModel;

        public SimplifiedRepViewModel(ICreoSessionProvider creoSessionProvider,
                                      ICreoModelService creoModelService,
                                      ICreoSimpRepService creoSimpRepService,
                                      ICreoParameterService creoParameterService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;
                _creoSimpRepService = creoSimpRepService;
                _creoParameterService = creoParameterService;

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
                ResetContext();

                CurrentDataContext.IsPleaseWaitShown = true;

                _activeModel = _creoModelService.GetActiveModel();
                if (_activeModel == null)
                {
                    System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("SRP_MsgNoActiveModel"),
                                                   McgWpfTools.GetStringResource("SRP_WindowTitle"),
                                                   System.Windows.MessageBoxButton.OK,
                                                   System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var activeFileName = _creoModelService.GetActiveModelFileName();
                if (string.IsNullOrWhiteSpace(activeFileName)
                    || !activeFileName.EndsWith(".asm", StringComparison.OrdinalIgnoreCase))
                {
                    _activeModel = null;
                    System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("SRP_MsgNotAnAssembly"),
                                                   McgWpfTools.GetStringResource("SRP_WindowTitle"),
                                                   System.Windows.MessageBoxButton.OK,
                                                   System.Windows.MessageBoxImage.Warning);
                    return;
                }

                CurrentDataContext.ActiveModelName = activeFileName;

                LoadSimpRepNames();
                LoadTopLevelComponents();

                CurrentDataContext.IsAssemblyLoaded = true;

                TraceLog.AddTraceLog($"SimplifiedRep : assemblage '{activeFileName}' lu " +
                                     $"({CurrentDataContext.ListItem.Count} composants, " +
                                     $"{CurrentDataContext.ListSimpRepName.Count} representations).");
            }
            catch (Exception ex)
            {
                ResetContext();
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsPleaseWaitShown = false;
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

        #region [REGION] Private Methods
        /// <summary>Vide le contexte et repasse la fenetre a l'etat initial.</summary>
        private void ResetContext()
        {
            _activeModel = null;
            CurrentDataContext.ListItem.Clear();
            CurrentDataContext.ListSimpRepName.Clear();
            CurrentDataContext.ActiveModelName = string.Empty;
            CurrentDataContext.SelectedSimpRepName = string.Empty;
            CurrentDataContext.NewSimpRepName = string.Empty;
            CurrentDataContext.IsAssemblyLoaded = false;
            CurrentDataContext.NbModels = 0;
            CurrentDataContext.NbModelsInProgress = 0;
        }

        /// <summary>Charge la liste des representations simplifiees existantes du modele actif.</summary>
        private void LoadSimpRepNames()
        {
            CurrentDataContext.ListSimpRepName.Clear();

            if (_activeModel == null) return;

            foreach (var simpRepName in _creoSimpRepService.ListSimpRepNames(_activeModel))
                CurrentDataContext.ListSimpRepName.Add(simpRepName);
        }

        /// <summary>Charge les composants de premier niveau de l'assemblage actif.</summary>
        private void LoadTopLevelComponents()
        {
            CurrentDataContext.ListItem.Clear();

            if (_activeModel == null) return;

            var components = _creoSimpRepService.ListTopLevelComponents(_activeModel);

            CurrentDataContext.NbModels = components.Count;
            CurrentDataContext.NbModelsInProgress = 0;

            foreach (var component in components)
            {
                var componentModel = component.ComponentFeature != null
                    ? _creoSimpRepService.GetComponentModel(component.ComponentFeature)
                    : null;

                CurrentDataContext.ListItem.Add(new SimplifiedRepComponentItem
                {
                    TreeIndex = component.TreeIndex,
                    ComponentId = component.Id,
                    Name = component.Name,
                    Rep = GetRepParameter(component),
                    Description = BuildDescription(componentModel),
                    IsIncluded = true,
                    IsExplicit = false,
                    CurrentAction = string.Empty,
                    ComponentInfo = component
                });

                CurrentDataContext.NbModelsInProgress++;
            }
        }

        /// <summary>
        /// Lit le parametre REP porte par la feature composant dans l'assemblage parent.
        /// Il s'agit d'un parametre de relation entre l'assemblage et le composant.
        /// </summary>
        private string GetRepParameter(CreoSimpRepComponentInfo component)
        {
            try
            {
                if (component.ComponentFeature is not IpfcParameterOwner parameterOwner)
                    return string.Empty;

                var parameter = parameterOwner.GetParam("REP");
                if (parameter == null) return string.Empty;

                return _creoParameterService.GetParameterAsString(parameter);
            }
            catch
            {
                // le parametre REP n'existe pas sur ce composant
                return string.Empty;
            }
        }

        /// <summary>
        /// Construit la description du composant : PTC_COMMON_NAME + "|" + DESCRIPTION2.
        /// </summary>
        private string BuildDescription(IpfcModel? componentModel)
        {
            if (componentModel == null) return string.Empty;

            var commonName = GetModelParameter(componentModel, "PTC_COMMON_NAME");
            var description2 = GetModelParameter(componentModel, "DESCRIPTION_2");

            return $"{commonName}|{description2}";
        }

        /// <summary>Lit un parametre du modele et retourne une chaine vide s'il est absent.</summary>
        private string GetModelParameter(IpfcModel model, string parameterName)
        {
            try
            {
                return _creoParameterService.GetParameterAsString(model, parameterName) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion
    }
}
