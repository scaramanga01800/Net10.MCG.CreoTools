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

                // Les dictionnaires de ressources ne sont pas encore fusionnes a la construction du
                // view model : la liste des regles par defaut est donc alimentee au premier usage.
                EnsureDefaultRuleList();

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

        /// <summary>
        /// Cree une nouvelle representation simplifiee avec la regle par defaut choisie dans le ruban
        /// ("Inclus" ou "Exclu"). Seuls les composants dont l'etat de la grille contredit cette regle
        /// sont listes explicitement, puis la nouvelle representation est activee.
        /// </summary>
        private void ExecuteCreateSimpRep()
        {
            try
            {
                if (!ValidateNewSimpRepName(out var newName)) return;

                var defaultAction = GetSelectedDefaultAction();

                CurrentDataContext.IsPleaseWaitShown = true;

                Thread createThread = new Thread(() => CreateSimpRepAsynch(newName, defaultAction));
                createThread.IsBackground = true;
                createThread.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Creation Creo executee en tache de fond : l'interface reste reactive et le gif d'attente est anime.
        /// La regle par defaut est appliquee telle quelle a l'ensemble des composants ; aucun item
        /// explicite n'est cree tant que l'utilisateur n'a pas differencie l'etat d'un composant
        /// via la mise a jour.
        /// </summary>
        private void CreateSimpRepAsynch(string newSimpRepName, EpfcSimpRepActionType defaultAction)
        {
            try
            {
                if (_activeModel == null) return;

                var simpRep = _creoSimpRepService.CreateSimpRep(_activeModel,
                                                                newSimpRepName,
                                                                defaultAction,
                                                                null,
                                                                false);

                _creoSimpRepService.ActivateSimpRep(_activeModel, simpRep);

                FinalizeSimpRepCreation(newSimpRepName);

                TraceLog.AddTraceLog($"SimplifiedRep : representation '{newSimpRepName}' creee et activee " +
                                     $"(regle par defaut {defaultAction}).");
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

        /// <summary>
        /// Cree une nouvelle representation simplifiee par copie de la representation selectionnee.
        /// </summary>
        private void ExecuteCopySimpRep()
        {
            try
            {
                var sourceName = CurrentDataContext.SelectedSimpRepName;

                EnsureDefaultRuleList();

                if (string.IsNullOrWhiteSpace(sourceName))
                {
                    ShowWarning("SRP_MsgNoSourceSimpRep");
                    return;
                }

                if (!ValidateNewSimpRepName(out var newName)) return;

                CurrentDataContext.IsPleaseWaitShown = true;

                Thread copyThread = new Thread(() => CopySimpRepAsynch(sourceName, newName));
                copyThread.IsBackground = true;
                copyThread.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Copie Creo executee en tache de fond.
        /// </summary>
        private void CopySimpRepAsynch(string sourceSimpRepName, string newSimpRepName)
        {
            try
            {
                if (_activeModel == null) return;

                var simpRep = _creoSimpRepService.CopySimpRep(_activeModel, sourceSimpRepName, newSimpRepName);

                _creoSimpRepService.ActivateSimpRep(_activeModel, simpRep);

                FinalizeSimpRepCreation(newSimpRepName);

                TraceLog.AddTraceLog($"SimplifiedRep : representation '{newSimpRepName}' creee " +
                                     $"par copie de '{sourceSimpRepName}'.");
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

        /// <summary>
        /// Applique sur la representation selectionnee l'etat courant de la grille :
        /// substitutions "definies par l'utilisateur" en priorite, sinon inclusion / exclusion.
        /// </summary>
        private void ExecuteUpdateSimpRep()
        {
            try
            {
                var simpRepName = CurrentDataContext.SelectedSimpRepName;

                if (_activeModel == null || string.IsNullOrWhiteSpace(simpRepName))
                {
                    ShowWarning("SRP_MsgNoSimpRepSelected");
                    return;
                }

                // Photo de l'etat de la grille prise sur le thread UI avant de basculer en tache de fond.
                var pendingChanges = CurrentDataContext.ListItem
                    .Select(item => new SimplifiedRepPendingChange
                    {
                        ComponentPath = item.ComponentInfo?.ComponentPath is { Count: > 0 } path
                            ? new List<int>(path)
                            : new List<int> { item.ComponentId },
                        ComponentInfo = item.ComponentInfo,
                        IsIncluded = item.IsIncluded,
                        SubstitutedSimpRepName = item.SelectedComponentSimpRep ?? string.Empty
                    })
                    .ToList();

                CurrentDataContext.IsPleaseWaitShown = true;

                Thread updateThread = new Thread(() => UpdateSimpRepAsynch(simpRepName, pendingChanges));
                updateThread.IsBackground = true;
                updateThread.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Mise a jour Creo executee en tache de fond, puis reactivation de la representation
        /// et rechargement de la grille.
        /// </summary>
        private void UpdateSimpRepAsynch(string simpRepName, List<SimplifiedRepPendingChange> pendingChanges)
        {
            try
            {
                if (_activeModel == null) return;

                var simpRep = _creoSimpRepService.GetSimpRep(_activeModel, simpRepName);
                if (simpRep == null)
                {
                    ShowWarning("SRP_MsgNoSimpRepSelected");
                    return;
                }

                var defaultAction = _creoSimpRepService.GetDefaultAction(simpRep);
                if (defaultAction == EpfcSimpRepActionType.EpfcSimpRepActionType_nil)
                    defaultAction = EpfcSimpRepActionType.EpfcSIMPREP_INCLUDE;

                var nbSubstituted = 0;
                var nbActions = 0;
                var nbRemoved = 0;

                foreach (var change in pendingChanges)
                {
                    // La substitution "definie par l'utilisateur" prime sur la case Inclus.
                    if (!string.IsNullOrWhiteSpace(change.SubstitutedSimpRepName)
                        && change.ComponentInfo != null)
                    {
                        _creoSimpRepService.SubstituteComponentBySimpRep(simpRep,
                                                                         change.ComponentInfo,
                                                                         change.SubstitutedSimpRepName);
                        nbSubstituted++;
                        continue;
                    }

                    var wantedAction = change.IsIncluded
                        ? EpfcSimpRepActionType.EpfcSIMPREP_INCLUDE
                        : EpfcSimpRepActionType.EpfcSIMPREP_EXCLUDE;

                    // Composant conforme a la regle par defaut : l'item explicite eventuellement
                    // present doit etre retire, sinon l'ancienne action resterait appliquee.
                    if (wantedAction == defaultAction)
                    {
                        if (_creoSimpRepService.RemoveComponentItem(simpRep, change.ComponentPath))
                            nbRemoved++;

                        continue;
                    }

                    _creoSimpRepService.SetComponentAction(simpRep, change.ComponentPath, wantedAction);
                    nbActions++;
                }

                _creoSimpRepService.ActivateSimpRep(_activeModel, simpRep);

                TraceLog.AddTraceLog($"SimplifiedRep : representation '{simpRepName}' mise a jour " +
                                     $"({nbActions} actions, {nbRemoved} retours a la regle par defaut, " +
                                     $"{nbSubstituted} substitutions).");
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsPleaseWaitShown = false;
            }

            // Relecture de l'etat reel depuis Creo pour refleter le resultat de la mise a jour.
            LoadComponentStates();
        }

        /// <summary>
        /// Supprime la representation selectionnee apres confirmation, puis reactive
        /// la representation maitre et vide la selection.
        /// </summary>
        private void ExecuteDeleteSimpRep()
        {
            try
            {
                var simpRepName = CurrentDataContext.SelectedSimpRepName;

                if (_activeModel == null || string.IsNullOrWhiteSpace(simpRepName))
                {
                    ShowWarning("SRP_MsgNoSimpRepSelected");
                    return;
                }

                var confirmation = System.Windows.MessageBox.Show(
                    string.Format(McgWpfTools.GetStringResource("SRP_MsgConfirmDelete"), simpRepName),
                    McgWpfTools.GetStringResource("SRP_WindowTitle"),
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (confirmation != System.Windows.MessageBoxResult.Yes) return;

                CurrentDataContext.IsPleaseWaitShown = true;

                Thread deleteThread = new Thread(() => DeleteSimpRepAsynch(simpRepName));
                deleteThread.IsBackground = true;
                deleteThread.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Suppression Creo executee en tache de fond, suivie du retour a la representation maitre.
        /// </summary>
        private void DeleteSimpRepAsynch(string simpRepName)
        {
            try
            {
                if (_activeModel == null) return;

                if (!_creoSimpRepService.DeleteSimpRep(_activeModel, simpRepName))
                {
                    ShowWarning("SRP_MsgDeleteFailed");
                    return;
                }

                // Le modele doit repasser sur la representation maitre : la representation
                // supprimee ne peut plus etre affichee dans Creo.
                _creoSimpRepService.ActivateMasterRep(_activeModel);

                var names = _creoSimpRepService.ListSimpRepNames(_activeModel);

                MainDispatcher.Invoke(() =>
                {
                    CurrentDataContext.ListSimpRepName.Clear();

                    foreach (var name in names)
                        CurrentDataContext.ListSimpRepName.Add(name);

                    // Vide la selection : declenche la remise a l'etat neutre de la grille.
                    CurrentDataContext.SelectedSimpRepName = string.Empty;
                });

                TraceLog.AddTraceLog($"SimplifiedRep : representation '{simpRepName}' supprimee, " +
                                     $"retour a la representation maitre.");
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

        /// <summary>
        /// Active dans Creo la representation selectionnee, puis recharge l'etat des composants.
        /// </summary>
        private void ExecuteActivateSimpRep()
        {
            try
            {
                var simpRepName = CurrentDataContext.SelectedSimpRepName;

                if (_activeModel == null || string.IsNullOrWhiteSpace(simpRepName))
                {
                    ShowWarning("SRP_MsgNoSimpRepSelected");
                    return;
                }

                CurrentDataContext.IsPleaseWaitShown = true;

                Thread activateThread = new Thread(() => ActivateSimpRepAsynch(simpRepName));
                activateThread.IsBackground = true;
                activateThread.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Activation Creo executee en tache de fond : la regeneration du modele peut etre longue.
        /// </summary>
        private void ActivateSimpRepAsynch(string simpRepName)
        {
            try
            {
                if (_activeModel == null) return;

                if (!_creoSimpRepService.ActivateSimpRep(_activeModel, simpRepName))
                {
                    ShowWarning("SRP_MsgActivateFailed");
                    return;
                }

                TraceLog.AddTraceLog($"SimplifiedRep : representation '{simpRepName}' activee.");
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsPleaseWaitShown = false;
            }

            // Relecture de l'etat reel : l'activation peut modifier l'affichage des composants.
            LoadComponentStates();
        }

        /// <summary>
        /// Sauvegarde l'assemblage actif : les representations simplifiees sont stockees dans le .asm.
        /// </summary>
        private void ExecuteSaveModel()
        {
            try
            {
                if (_activeModel == null)
                {
                    ShowWarning("SRP_MsgNoActiveModel");
                    return;
                }

                CurrentDataContext.IsPleaseWaitShown = true;

                Thread saveThread = new Thread(new ThreadStart(SaveModelAsynch));
                saveThread.IsBackground = true;
                saveThread.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Sauvegarde Creo executee en tache de fond : l'ecriture du modele peut etre longue.
        /// </summary>
        private void SaveModelAsynch()
        {
            try
            {
                if (_activeModel == null) return;

                if (!_creoSimpRepService.SaveOwnerModel(_activeModel))
                {
                    ShowWarning("SRP_MsgSaveFailed");
                    return;
                }

                TraceLog.AddTraceLog($"SimplifiedRep : modele '{CurrentDataContext.ActiveModelName}' sauvegarde.");

                System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("SRP_MsgSaveSuccess"),
                                               McgWpfTools.GetStringResource("SRP_WindowTitle"),
                                               System.Windows.MessageBoxButton.OK,
                                               System.Windows.MessageBoxImage.Information);
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
        /// Alimente la liste des regles par defaut et selectionne la premiere de la pile.
        /// Appelee tardivement car les dictionnaires de ressources ne sont pas disponibles
        /// au moment de la construction du view model.
        /// </summary>
        private void EnsureDefaultRuleList()
        {
            if (CurrentDataContext.ListDefaultRule.Count > 0) return;

            CurrentDataContext.ListDefaultRule.Add(McgWpfTools.GetStringResource("SRP_Action_Include"));
            CurrentDataContext.ListDefaultRule.Add(McgWpfTools.GetStringResource("SRP_Action_Exclude"));

            CurrentDataContext.SelectedDefaultRule = CurrentDataContext.ListDefaultRule[0];
        }

        /// <summary>
        /// Normalise le nom saisi selon la convention Creo : premiere lettre en majuscule,
        /// les suivantes en minuscules. Creo stocke ensuite le nom en majuscules,
        /// ce qui explique l'affichage en capitales dans la liste des representations.
        /// </summary>
        private static string NormalizeSimpRepName(string simpRepName)
        {
            if (string.IsNullOrWhiteSpace(simpRepName)) return string.Empty;

            var trimmedName = simpRepName.Trim();

            return char.ToUpperInvariant(trimmedName[0]) + trimmedName.Substring(1).ToLowerInvariant();
        }

        /// <summary>
        /// Convertit la regle par defaut choisie dans le ruban en action Creo.
        /// La comparaison porte sur la position dans la liste : l'index 1 correspond a "Exclu".
        /// </summary>
        private EpfcSimpRepActionType GetSelectedDefaultAction()
        {
            var ruleIndex = CurrentDataContext.ListDefaultRule.IndexOf(CurrentDataContext.SelectedDefaultRule);

            return ruleIndex == 1
                ? EpfcSimpRepActionType.EpfcSIMPREP_EXCLUDE
                : EpfcSimpRepActionType.EpfcSIMPREP_INCLUDE;
        }

        /// <summary>
        /// Controle la saisie du nouveau nom de representation : non vide et non deja utilise.
        /// </summary>
        private bool ValidateNewSimpRepName(out string newSimpRepName)
        {
            var candidateName = NormalizeSimpRepName(CurrentDataContext.NewSimpRepName ?? string.Empty);
            newSimpRepName = candidateName;

            if (!CurrentDataContext.IsAssemblyLoaded || _activeModel == null)
            {
                ShowWarning("SRP_MsgNoActiveModel");
                return false;
            }

            if (string.IsNullOrWhiteSpace(candidateName))
            {
                ShowWarning("SRP_MsgNewNameRequired");
                return false;
            }

            if (CurrentDataContext.ListSimpRepName.Any(n => string.Equals(n, candidateName, StringComparison.OrdinalIgnoreCase)))
            {
                ShowWarning("SRP_MsgNameAlreadyExists");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Rafraichit la liste des representations puis selectionne la nouvelle,
        /// ce qui declenche le rechargement des etats de composants.
        /// </summary>
        private void FinalizeSimpRepCreation(string newSimpRepName)
        {
            var names = _activeModel != null
                ? _creoSimpRepService.ListSimpRepNames(_activeModel)
                : new List<string>();

            MainDispatcher.Invoke(() =>
            {
                CurrentDataContext.ListSimpRepName.Clear();

                foreach (var name in names)
                    CurrentDataContext.ListSimpRepName.Add(name);

                CurrentDataContext.NewSimpRepName = string.Empty;

                // Creo renvoie le nom dans sa propre casse : la selection doit porter sur
                // l'instance reellement presente dans la liste, sinon le combo reste vide.
                var matchingName = CurrentDataContext.ListSimpRepName
                    .FirstOrDefault(n => string.Equals(n, newSimpRepName, StringComparison.OrdinalIgnoreCase));

                CurrentDataContext.SelectedSimpRepName = matchingName ?? newSimpRepName;
            });
        }

        /// <summary>Affiche un message d'avertissement localise.</summary>
        private static void ShowWarning(string resourceKey)
        {
            System.Windows.MessageBox.Show(McgWpfTools.GetStringResource(resourceKey),
                                           McgWpfTools.GetStringResource("SRP_WindowTitle"),
                                           System.Windows.MessageBoxButton.OK,
                                           System.Windows.MessageBoxImage.Warning);
        }

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
            // Retour a la premiere regle de la pile pour une future creation.
            EnsureDefaultRuleList();

            if (CurrentDataContext.ListDefaultRule.Count > 0)
                CurrentDataContext.SelectedDefaultRule = CurrentDataContext.ListDefaultRule[0];

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
