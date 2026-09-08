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

                CurrentDataContext.PropertyChanged += OnDataContextPropertyChanged;
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

                Thread readAsmThread = new Thread(new ThreadStart(ReadAsmAsynch));
                readAsmThread.IsBackground = true;
                readAsmThread.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Lecture de l'assemblage actif en tache de fond : les interactions Creo sont longues,
        /// l'interface reste ainsi reactive et le gif d'attente est visible.
        /// </summary>
        private void ReadAsmAsynch()
        {
            try
            {
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

                // Traitements Creo executes hors du thread UI : chaque methode marshalle
                // elle-meme ses ajouts dans les collections liees a la grille.
                LoadSimpRepNames();
                LoadTopLevelComponents();

                CurrentDataContext.IsAssemblyLoaded = true;

                TraceLog.AddTraceLog($"SimplifiedRep : assemblage '{activeFileName}' lu " +
                                     $"({CurrentDataContext.ListItem.Count} composants, " +
                                     $"{CurrentDataContext.ListSimpRepName.Count} representations).");
            }
            catch (Exception ex)
            {
                MainDispatcher.Invoke(ResetContext);
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
        /// <summary>
        /// Recharge l'etat des composants lorsque l'utilisateur change de representation simplifiee.
        /// </summary>
        private void OnDataContextPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SimplifiedRepDataContext.SelectedSimpRepName)) return;
            if (!CurrentDataContext.IsAssemblyLoaded) return;

            CurrentDataContext.IsPleaseWaitShown = true;

            Thread loadStatesThread = new Thread(new ThreadStart(LoadComponentStates));
            loadStatesThread.IsBackground = true;
            loadStatesThread.Start();
        }

        /// <summary>
        /// Applique sur la grille l'etat consolide des composants pour la representation selectionnee.
        /// Si aucune representation n'est selectionnee, la grille repasse en etat neutre (tout inclus).
        /// Execute en tache de fond : les appels Creo sont couteux.
        /// </summary>
        private void LoadComponentStates()
        {
            try
            {
                CurrentDataContext.IsPleaseWaitShown = true;

                var simpRepName = CurrentDataContext.SelectedSimpRepName;

                if (_activeModel == null || string.IsNullOrWhiteSpace(simpRepName))
                {
                    MainDispatcher.Invoke(ResetComponentStates);
                    return;
                }

                var simpRep = _creoSimpRepService.GetSimpRep(_activeModel, simpRepName);
                if (simpRep == null)
                {
                    MainDispatcher.Invoke(ResetComponentStates);
                    return;
                }

                // La regle par defaut peut revenir "nil" selon la representation : dans ce cas
                // Creo considere les composants non listes comme inclus.
                var defaultAction = _creoSimpRepService.GetDefaultAction(simpRep);
                if (defaultAction == EpfcSimpRepActionType.EpfcSimpRepActionType_nil)
                    defaultAction = EpfcSimpRepActionType.EpfcSIMPREP_INCLUDE;

                CurrentDataContext.SelectedDefaultRule = FormatAction(defaultAction);

                var states = _creoSimpRepService.GetComponentStates(_activeModel, simpRep);

                // Lecture Creo des representations propres a chaque composant : couteux,
                // effectue hors du thread UI pour que le gif d'attente reste anime.
                var componentSimpReps = new Dictionary<int, List<string>>();

                foreach (var item in CurrentDataContext.ListItem)
                {
                    var names = new List<string>();

                    if (item.ComponentInfo?.ComponentFeature != null)
                    {
                        try
                        {
                            names = _creoSimpRepService.ListComponentSimpRepNames(item.ComponentInfo.ComponentFeature);
                        }
                        catch
                        {
                            // composant sans representation simplifiee exploitable : liste laissee vide
                        }
                    }

                    componentSimpReps[item.ComponentId] = names;
                }

                // Seules les mises a jour des collections liees passent par le thread UI.
                MainDispatcher.Invoke(() =>
                {
                    foreach (var item in CurrentDataContext.ListItem)
                    {
                        var state = states.FirstOrDefault(s => s.Component.Id == item.ComponentId);

                        // Action reellement appliquee : item explicite, sinon regle par defaut.
                        var effectiveAction = state != null
                                              && state.EffectiveAction != EpfcSimpRepActionType.EpfcSimpRepActionType_nil
                            ? state.EffectiveAction
                            : defaultAction;

                        item.IsExplicit = state?.IsExplicit ?? false;
                        item.CurrentAction = FormatAction(effectiveAction);
                        item.IsIncluded = IsIncludedAction(effectiveAction, defaultAction);

                        // La liste doit exister avant d'affecter la valeur selectionnee du combo.
                        item.ListComponentSimpRep.Clear();

                        if (componentSimpReps.TryGetValue(item.ComponentId, out var names))
                        {
                            foreach (var name in names)
                                item.ListComponentSimpRep.Add(name);
                        }

                        var substituted = state?.SubstitutedSimpRepName ?? string.Empty;

                        if (!string.IsNullOrEmpty(substituted)
                            && !item.ListComponentSimpRep.Contains(substituted))
                        {
                            item.ListComponentSimpRep.Add(substituted);
                        }

                        item.SelectedComponentSimpRep = substituted;
                    }
                });

                TraceLog.AddTraceLog($"SimplifiedRep : etats charges pour la representation '{simpRepName}'.");
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsPleaseWaitShown = false;
            }
        }

        /// <summary>Repasse toutes les lignes en etat neutre (aucune representation selectionnee).</summary>
        private void ResetComponentStates()
        {
            CurrentDataContext.SelectedDefaultRule = string.Empty;

            foreach (var item in CurrentDataContext.ListItem)
            {
                item.IsIncluded = true;
                item.IsExplicit = false;
                item.CurrentAction = string.Empty;
                item.SelectedComponentSimpRep = string.Empty;
            }
        }

        /// <summary>
        /// Determine si l'action effective laisse le composant present dans la representation.
        /// Aligne sur la logique interne du service Creo.
        /// </summary>
        private static bool IsIncludedAction(EpfcSimpRepActionType action, EpfcSimpRepActionType defaultAction)
        {
            return action switch
            {
                EpfcSimpRepActionType.EpfcSIMPREP_EXCLUDE => false,
                EpfcSimpRepActionType.EpfcSIMPREP_NONE => false,
                EpfcSimpRepActionType.EpfcSIMPREP_REVERSE =>
                    defaultAction != EpfcSimpRepActionType.EpfcSIMPREP_INCLUDE,
                EpfcSimpRepActionType.EpfcSimpRepActionType_nil => false,
                _ => true
            };
        }

        /// <summary>Traduit une action Creo en libelle affichable dans la grille.</summary>
        private static string FormatAction(EpfcSimpRepActionType action)
        {
            return action switch
            {
                EpfcSimpRepActionType.EpfcSIMPREP_INCLUDE => McgWpfTools.GetStringResource("SRP_Action_Include"),
                EpfcSimpRepActionType.EpfcSIMPREP_EXCLUDE => McgWpfTools.GetStringResource("SRP_Action_Exclude"),
                EpfcSimpRepActionType.EpfcSIMPREP_SUBSTITUTE => McgWpfTools.GetStringResource("SRP_Action_Substitute"),
                EpfcSimpRepActionType.EpfcSimpRepActionType_nil => string.Empty,
                _ => action.ToString()
            };
        }

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
            MainDispatcher.Invoke(() => CurrentDataContext.ListSimpRepName.Clear());

            if (_activeModel == null) return;

            // Appel Creo effectue hors du thread UI, seul l'ajout est marshalle.
            var names = _creoSimpRepService.ListSimpRepNames(_activeModel);

            foreach (var simpRepName in names)
                MainDispatcher.Invoke(() => CurrentDataContext.ListSimpRepName.Add(simpRepName));
        }

        /// <summary>Charge les composants de premier niveau de l'assemblage actif.</summary>
        private void LoadTopLevelComponents()
        {
            MainDispatcher.Invoke(() => CurrentDataContext.ListItem.Clear());

            if (_activeModel == null) return;

            var components = _creoSimpRepService.ListTopLevelComponents(_activeModel);

            CurrentDataContext.NbModels = components.Count;
            CurrentDataContext.NbModelsInProgress = 0;

            foreach (var component in components)
            {
                // Lecture des parametres du composant : couteux, reste hors du thread UI.
                var componentModel = component.ComponentFeature != null
                    ? _creoSimpRepService.GetComponentModel(component.ComponentFeature)
                    : null;

                var item = new SimplifiedRepComponentItem
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
                };

                MainDispatcher.Invoke(() => CurrentDataContext.ListItem.Add(item));

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
