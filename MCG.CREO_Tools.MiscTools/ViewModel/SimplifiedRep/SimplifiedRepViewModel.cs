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
        public ICommand CommandIncludeSelection { get => new RelayCommand(() => ExecuteApplySelectionInclusion(true)); }
        public ICommand CommandExcludeSelection { get => new RelayCommand(() => ExecuteApplySelectionInclusion(false)); }
        public ICommand CommandApplyCommonSimpRep { get => new RelayCommand(() => ExecuteApplyCommonSimpRep()); }
        public ICommand CommandApplyAllInstances { get => new RelayCommand(() => ExecuteApplyToAllInstances()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        private readonly ICreoSimpRepService _creoSimpRepService;
        private readonly ICreoParameterService _creoParameterService;

        private IpfcModel? _activeModel;

        /// <summary>Evite la reentrance lors de l'application de la case "tout cocher".</summary>
        private bool _isApplyingAllIncluded;

        /// <summary>
        /// Derniere representation reellement chargee dans la grille.
        /// Permet de restaurer la selection du combo si l'utilisateur refuse de perdre
        /// ses modifications en cours.
        /// </summary>
        private string _lastLoadedSimpRepName = string.Empty;

        /// <summary>
        /// Vrai lorsque le changement de representation est pilote par le code
        /// (creation, copie, suppression, restauration) : aucune confirmation n'est demandee.
        /// </summary>
        private bool _isSelectionChangeInternal;

        /// <summary>
        /// Cache des representations simplifiees disponibles par modele composant.
        /// Evite de reinterroger Creo a chaque rechargement de la grille.
        /// Vide a chaque nouvelle lecture de l'assemblage.
        /// </summary>
        private readonly Dictionary<string, List<string>> _componentSimpRepCache =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

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
                // La lecture repart de zero : les modifications en cours seraient perdues.
                if (!ConfirmPendingChangesLoss()) return;

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
                // Nouvelle lecture de l'assemblage : le cache des representations
                // des composants n'est plus fiable.
                _componentSimpRepCache.Clear();

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

                // La creation active la nouvelle representation et recharge la grille.
                if (!ConfirmPendingChangesLoss()) return;

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

                // La copie active la nouvelle representation et recharge la grille.
                if (!ConfirmPendingChangesLoss()) return;

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
                // Seules les lignes reellement modifiees sont envoyees a Creo : chaque appel COM
                // est couteux, il est inutile de reappliquer un etat deja en place.
                var pendingChanges = CurrentDataContext.ListItem
                    .Where(item => item.HasPendingChange)
                    .Select(item => new SimplifiedRepPendingChange
                    {
                        ComponentPath = item.ComponentInfo?.ComponentPath is { Count: > 0 } path
                            ? new List<int>(path)
                            : new List<int> { item.ComponentId },
                        ComponentInfo = item.ComponentInfo,
                        IsIncluded = item.IsIncluded,
                        SubstitutedSimpRepName = item.EffectiveSubstitution
                    })
                    .ToList();

                // Rien n'a change : inutile de solliciter Creo ni de recharger la grille.
                if (pendingChanges.Count == 0)
                {
                    ShowInformation("SRP_MsgNoChangeToApply");
                    return;
                }

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

                // Creo a applique exactement ce qui vient d'etre demande : inutile de relire
                // l'ensemble des composants (operation tres couteuse). On recale simplement
                // l'etat de reference des lignes qui viennent d'etre envoyees.
                MainDispatcher.Invoke(() =>
                {
                    foreach (var item in CurrentDataContext.ListItem)
                    {
                        if (item.HasPendingChange)
                            item.CaptureBaseline();
                    }

                    RefreshPendingChangesState();
                });

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

                // La suppression repasse la grille en etat neutre.
                if (!ConfirmPendingChangesLoss()) return;

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
                    // Changement pilote par le code : pas de nouvelle confirmation.
                    try
                    {
                        _isSelectionChangeInternal = true;
                        CurrentDataContext.SelectedSimpRepName = string.Empty;
                    }
                    finally
                    {
                        _isSelectionChangeInternal = false;
                    }
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

                // L'activation relit l'etat reel des composants depuis Creo.
                if (!ConfirmPendingChangesLoss()) return;

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

                // La sauvegarde ecrit dans le .asm ce qui a deja ete applique par "Mettre a jour".
                // Les modifications encore en attente dans la grille n'ont pas ete envoyees a Creo :
                // elles doivent rester actives et surlignees, sinon "Mettre a jour" n'aurait plus
                // rien a appliquer. Aucun recalage de reference n'est donc effectue ici.

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

                // Changement pilote par le code : la perte des modifications a deja ete confirmee.
                try
                {
                    _isSelectionChangeInternal = true;
                    CurrentDataContext.SelectedSimpRepName = matchingName ?? newSimpRepName;
                }
                finally
                {
                    _isSelectionChangeInternal = false;
                }
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

        #region [REGION] Multi selection actions
        /// <summary>
        /// Lignes actuellement selectionnees dans la grille.
        /// Alimentee par la vue a chaque changement de selection.
        /// </summary>
        private readonly List<SimplifiedRepComponentItem> _selectedItems = new List<SimplifiedRepComponentItem>();

        /// <summary>
        /// Vrai pendant l'application d'une action de masse : les evenements de selection
        /// emis par la grille en reaction aux modifications sont alors ignores.
        /// </summary>
        private bool _isApplyingSelectionAction;

        /// <summary>
        /// Prend en compte la nouvelle selection de la grille et recalcule la liste des
        /// representations simplifiees communes a tous les composants selectionnes.
        /// </summary>
        public void UpdateSelection(IEnumerable<SimplifiedRepComponentItem> selectedItems)
        {
            try
            {
                if (_isApplyingSelectionAction) return;

                _selectedItems.Clear();

                if (selectedItems != null)
                    _selectedItems.AddRange(selectedItems);

                CurrentDataContext.IsMultiSelectionActive = _selectedItems.Count > 0;

                RefreshCommonSimpRepList();
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Ne conserve que les representations presentes sur TOUS les composants selectionnes.
        /// La representation maitresse est toujours disponible.
        /// </summary>
        private void RefreshCommonSimpRepList()
        {
            var masterLabel = SimplifiedRepComponentItem.MasterRepLabel;
            var previousSelection = CurrentDataContext.SelectedCommonSimpRep;

            // Intersection successive des listes de chaque ligne selectionnee.
            List<string>? common = null;

            foreach (var item in _selectedItems)
            {
                var names = item.ListComponentSimpRep
                    .Where(n => !string.Equals(n, masterLabel, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (common == null)
                {
                    common = names;
                    continue;
                }

                common = common
                    .Where(n => names.Contains(n, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (common.Count == 0) break;
            }

            var wanted = new List<string> { masterLabel };

            if (common != null)
                wanted.AddRange(common.OrderBy(n => n, StringComparer.OrdinalIgnoreCase));

            if (CurrentDataContext.ListCommonSimpRep.SequenceEqual(wanted, StringComparer.OrdinalIgnoreCase))
                return;

            CurrentDataContext.ListCommonSimpRep.Clear();

            foreach (var name in wanted)
                CurrentDataContext.ListCommonSimpRep.Add(name);

            // La selection precedente est conservee si elle reste commune a la nouvelle selection.
            CurrentDataContext.SelectedCommonSimpRep =
                wanted.Contains(previousSelection, StringComparer.OrdinalIgnoreCase)
                    ? previousSelection
                    : masterLabel;
        }

        /// <summary>
        /// Applique "Inclure" ou "Exclure" a toutes les lignes selectionnees.
        /// Aucun appel Creo : seule la grille est mise a jour, la mise a jour reelle
        /// reste declenchee par le bouton "Mettre a jour".
        /// </summary>
        private void ExecuteApplySelectionInclusion(bool isIncluded)
        {
            try
            {
                if (_selectedItems.Count == 0)
                {
                    ShowWarning("SRP_MsgNoSelection");
                    return;
                }

                // Copie de travail : modifier les lignes fait reagir la grille, qui renvoie
                // un SelectionChanged et reconstruit _selectedItems en cours d'iteration.
                _isApplyingSelectionAction = true;

                try
                {
                    foreach (var item in _selectedItems.ToList())
                        item.IsIncluded = isIncluded;
                }
                finally
                {
                    _isApplyingSelectionAction = false;
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Applique a toutes les lignes selectionnees la representation choisie dans le ruban.
        /// Choisir la representation maitresse revient a supprimer la substitution.
        /// </summary>
        private void ExecuteApplyCommonSimpRep()
        {
            try
            {
                if (_selectedItems.Count == 0)
                {
                    ShowWarning("SRP_MsgNoSelection");
                    return;
                }

                var wantedSimpRep = CurrentDataContext.SelectedCommonSimpRep;

                if (string.IsNullOrWhiteSpace(wantedSimpRep))
                {
                    ShowWarning("SRP_MsgNoCommonSimpRepSelected");
                    return;
                }

                // Copie de travail : modifier les lignes fait reagir la grille, qui renvoie
                // un SelectionChanged et reconstruit _selectedItems en cours d'iteration.
                _isApplyingSelectionAction = true;

                try
                {
                    foreach (var item in _selectedItems.ToList())
                    {
                        if (!item.ListComponentSimpRep.Contains(wantedSimpRep, StringComparer.OrdinalIgnoreCase))
                            continue;

                        item.SelectedComponentSimpRep = wantedSimpRep;
                    }
                }
                finally
                {
                    _isApplyingSelectionAction = false;
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Propage l'action de la ligne courante a toutes les lignes qui referencent
        /// le MEME modele Creo (.PRT / .ASM).
        ///
        /// La correspondance repose exclusivement sur l'identite du modele Creo
        /// (SimplifiedRepComponentItem.ModelKey, construite a partir du descripteur de
        /// modele Creo) et jamais sur le chemin d'assemblage, le numero d'occurrence,
        /// la position dans l'arbre ou l'assemblage parent.
        ///
        /// Les trois actions supportees sont propagees :
        /// - Inclure   : IsIncluded = true, sans substitution
        /// - Exclure   : IsIncluded = false, sans substitution
        /// - Remplacer : IsIncluded de la source + nom de la representation substituee
        ///
        /// Aucun appel Creo n'est effectue : comme pour les actions de masse existantes,
        /// seule la grille est modifiee, la mise a jour reelle restant declenchee par
        /// le bouton "Mettre a jour".
        /// </summary>
        private void ExecuteApplyToAllInstances()
        {
            try
            {
                if (_selectedItems.Count == 0)
                {
                    ShowWarning("SRP_MsgNoSelection");
                    return;
                }

                // Le clic droit selectionne la ligne : la premiere ligne selectionnee
                // sert de reference pour l'action a propager.
                var source = _selectedItems[0];
                var modelKey = source.ModelKey;

                if (string.IsNullOrWhiteSpace(modelKey))
                {
                    ShowInformation("SRP_MsgNoOtherInstance");
                    return;
                }

                // Etat a propager, capture AVANT toute modification des autres lignes.
                var isIncluded = source.IsIncluded;
                var substitution = source.EffectiveSubstitution;

                var targets = CurrentDataContext.ListItem
                    .Where(i => !ReferenceEquals(i, source))
                    .Where(i => string.Equals(i.ModelKey, modelKey, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (targets.Count == 0)
                {
                    ShowInformation("SRP_MsgNoOtherInstance");
                    return;
                }

                // Modifier les lignes fait reagir la grille, qui renvoie un SelectionChanged
                // et reconstruirait _selectedItems en cours d'iteration.
                _isApplyingSelectionAction = true;

                try
                {
                    foreach (var item in targets)
                    {
                        // IsIncluded est applique en premier : cocher / decocher la case
                        // remet la ligne sur la representation maitresse.
                        item.IsIncluded = isIncluded;

                        if (string.IsNullOrWhiteSpace(substitution)) continue;

                        // Remplacement par une representation simplifiee : la representation
                        // existe par construction sur toutes les occurrences du meme modele,
                        // le test evite simplement une valeur hors liste.
                        if (item.ListComponentSimpRep.Contains(substitution, StringComparer.OrdinalIgnoreCase))
                            item.SelectedComponentSimpRep = substitution;
                    }
                }
                finally
                {
                    _isApplyingSelectionAction = false;
                }

                // Les setters ont deja notifie chaque ligne (action, surlignage) ;
                // on reconsolide l'etat global des modifications en attente.
                RefreshPendingChangesState();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        /// <summary>Affiche un message d'information localise.</summary>
        private static void ShowInformation(string resourceKey)
        {
            System.Windows.MessageBox.Show(McgWpfTools.GetStringResource(resourceKey),
                                           McgWpfTools.GetStringResource("SRP_WindowTitle"),
                                           System.Windows.MessageBoxButton.OK,
                                           System.Windows.MessageBoxImage.Information);
        }

        /// <summary>
        /// Recharge l'etat des composants lorsque l'utilisateur change de representation simplifiee.
        /// </summary>
        private void OnDataContextPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Case d'en-tete : applique la meme valeur a toutes les lignes de la grille.
            if (e.PropertyName == nameof(SimplifiedRepDataContext.IsAllIncluded))
            {
                if (_isApplyingAllIncluded) return;

                try
                {
                    _isApplyingAllIncluded = true;

                    foreach (var item in CurrentDataContext.ListItem)
                        item.IsIncluded = CurrentDataContext.IsAllIncluded;
                }
                finally
                {
                    _isApplyingAllIncluded = false;
                }

                return;
            }

            if (e.PropertyName != nameof(SimplifiedRepDataContext.SelectedSimpRepName)) return;
            if (!CurrentDataContext.IsAssemblyLoaded) return;

            // Changement pilote par le code (creation, copie, suppression, restauration) :
            // la confirmation a deja ete traitee en amont.
            if (!_isSelectionChangeInternal)
            {
                if (!ConfirmPendingChangesLoss())
                {
                    // L'utilisateur refuse de perdre ses modifications : la selection
                    // precedente est restauree sans declencher de nouveau chargement.
                    RestoreSelectedSimpRepName();
                    return;
                }
            }

            _lastLoadedSimpRepName = CurrentDataContext.SelectedSimpRepName;

            CurrentDataContext.IsPleaseWaitShown = true;

            Thread loadStatesThread = new Thread(new ThreadStart(LoadComponentStates));
            loadStatesThread.IsBackground = true;
            loadStatesThread.Start();
        }

        /// <summary>
        /// Remet le combo des representations sur la derniere valeur reellement chargee.
        /// </summary>
        private void RestoreSelectedSimpRepName()
        {
            try
            {
                _isSelectionChangeInternal = true;
                CurrentDataContext.SelectedSimpRepName = _lastLoadedSimpRepName;
            }
            finally
            {
                _isSelectionChangeInternal = false;
            }
        }

        /// <summary>
        /// Recalcule l'indicateur global de modifications en attente a partir des lignes de la grille.
        /// </summary>
        private void RefreshPendingChangesState()
        {
            CurrentDataContext.HasPendingChanges =
                CurrentDataContext.ListItem.Any(item => item.HasPendingChange);
        }

        /// <summary>
        /// Demande confirmation lorsqu'une action va recharger ou remplacer les donnees
        /// affichees alors que des modifications ne sont pas sauvegardees.
        /// Retourne vrai si l'operation demandee peut se poursuivre.
        /// </summary>
        private bool ConfirmPendingChangesLoss()
        {
            RefreshPendingChangesState();

            if (!CurrentDataContext.HasPendingChanges) return true;

            var answer = System.Windows.MessageBox.Show(
                McgWpfTools.GetStringResource("SRP_MsgConfirmLosePendingChanges"),
                McgWpfTools.GetStringResource("SRP_WindowTitle"),
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            return answer == System.Windows.MessageBoxResult.Yes;
        }

        /// <summary>
        /// Abonne la ligne au suivi des modifications afin de maintenir a jour
        /// l'indicateur global <see cref="SimplifiedRepDataContext.HasPendingChanges"/>.
        /// </summary>
        private void SubscribeToPendingChange(SimplifiedRepComponentItem item)
        {
            item.PendingChangeEvent -= OnItemPendingChanged;
            item.PendingChangeEvent += OnItemPendingChanged;
        }

        private void OnItemPendingChanged(object? sender, EventArgs e)
        {
            RefreshPendingChangesState();
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

                // Lecture Creo des representations propres a chaque composant : tres couteux.
                // Ces listes ne dependent que du modele composant, pas de la representation de
                // l'assemblage : elles sont donc mises en cache et mutualisees entre les
                // occurrences repetees d'un meme modele.
                var componentSimpReps = new Dictionary<int, List<string>>();

                foreach (var item in CurrentDataContext.ListItem)
                {
                    var cacheKey = item.Name ?? string.Empty;

                    if (!_componentSimpRepCache.TryGetValue(cacheKey, out var names))
                    {
                        names = new List<string>();

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

                        _componentSimpRepCache[cacheKey] = names;
                    }

                    componentSimpReps[item.ComponentId] = names;
                }

                // Indexation des etats : evite une recherche lineaire par ligne (cout N x N).
                var statesById = new Dictionary<int, CreoSimpRepComponentState>();

                foreach (var state in states)
                    statesById[state.Component.Id] = state;

                // Seules les mises a jour des collections liees passent par le thread UI.
                MainDispatcher.Invoke(() =>
                {
                    foreach (var item in CurrentDataContext.ListItem)
                    {
                        statesById.TryGetValue(item.ComponentId, out var state);

                        // Action reellement appliquee : item explicite, sinon regle par defaut.
                        var effectiveAction = state != null
                                              && state.EffectiveAction != EpfcSimpRepActionType.EpfcSimpRepActionType_nil
                            ? state.EffectiveAction
                            : defaultAction;

                        item.IsExplicit = state?.IsExplicit ?? false;

                        // CurrentAction est calculee : elle reflete ce qui sera applique a la mise a jour.
                        item.IsIncluded = IsIncludedAction(effectiveAction, defaultAction);

                        componentSimpReps.TryGetValue(item.ComponentId, out var names);
                        names ??= new List<string>();

                        var substituted = state?.SubstitutedSimpRepName ?? string.Empty;

                        // La representation maitresse est toujours proposee en tete : elle
                        // permet de revenir explicitement a "pas de substitution".
                        var wantedNames = new List<string> { SimplifiedRepComponentItem.MasterRepLabel };
                        wantedNames.AddRange(names);

                        if (!string.IsNullOrEmpty(substituted)
                            && !wantedNames.Contains(substituted, StringComparer.OrdinalIgnoreCase))
                        {
                            wantedNames.Add(substituted);
                        }

                        // La collection n'est reconstruite que si son contenu change reellement :
                        // chaque Clear/Add declenche un rafraichissement complet du ComboBox lie.
                        if (!item.ListComponentSimpRep.SequenceEqual(wantedNames, StringComparer.OrdinalIgnoreCase))
                        {
                            item.ListComponentSimpRep.Clear();

                            foreach (var name in wantedNames)
                                item.ListComponentSimpRep.Add(name);
                        }

                        // Sans substitution, la ligne se positionne sur la representation maitresse.
                        item.SelectedComponentSimpRep = string.IsNullOrEmpty(substituted)
                            ? SimplifiedRepComponentItem.MasterRepLabel
                            : substituted;

                        // L'etat lu dans Creo devient la reference : seules les modifications
                        // ulterieures de l'utilisateur seront renvoyees a Creo.
                        item.CaptureBaseline();
                    }

                    RefreshPendingChangesState();
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
                item.SelectedComponentSimpRep = string.Empty;
                item.CaptureBaseline();
            }

            RefreshPendingChangesState();
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
            CurrentDataContext.HasPendingChanges = false;
            _lastLoadedSimpRepName = string.Empty;
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
                    // Parametres TYPE et SUB_TYPE portes par le modele du composant.
                    Type = componentModel != null ? GetModelParameter(componentModel, "TYPE") : string.Empty,
                    SubType = componentModel != null ? GetModelParameter(componentModel, "SUB_TYPE") : string.Empty,
                    IsIncluded = true,
                    IsExplicit = false,
                    ComponentInfo = component
                };

                MainDispatcher.Invoke(() =>
                {
                    SubscribeToPendingChange(item);
                    CurrentDataContext.ListItem.Add(item);
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
