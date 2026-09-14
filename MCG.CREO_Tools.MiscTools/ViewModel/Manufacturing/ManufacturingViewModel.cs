using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WebtermLib.Services.Interfaces;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.Manufacturing;
using pfcls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing
{
    /// <summary>
    /// ViewModel de l'application "Manufacturing View" : lecture hierarchique de la nomenclature
    /// de l'assemblage actif, avec les parametres REFERENCE, PTC_COMMON_NAME, DESCRIPTION_2,
    /// DESCRIPTION2_1, DESCRIPTION2_2 et DESCRIPTION_MTH de chaque composant.
    ///
    /// Reprend l'architecture de <see cref="SimplifiedRep.SimplifiedRepViewModel"/> : lecture et
    /// sauvegarde en tache de fond via <see cref="Thread"/>, controle prealable du statut
    /// modifiable de l'assemblage, suivi des modifications en attente par ligne.
    /// </summary>
    public class ManufacturingViewModel : ObservableObject, IManufacturingViewModel
    {
        #region [REGION] Properties from Interface
        public ManufacturingDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private Dispatcher MainDispatcher { get; set; }
        #endregion

        #region [REGION] Commands
        public ICommand CommandReadAsm { get => new RelayCommand(() => ExecuteReadAsm()); }
        public ICommand CommandSaveModel { get => new RelayCommand(() => ExecuteSaveModel()); }

        /// <summary>
        /// Commande "Mise a jour" : ecrit dans Creo les valeurs REFERENCE / DESCRIPTION_MTH
        /// modifiees dans la grille. L'implementation metier est traitee dans une etape dediee ;
        /// seuls la commande, son etat d'activation et son raccordement au ruban sont en place.
        /// </summary>
        public ICommand CommandUpdateParameters { get => new RelayCommand(() => ExecuteUpdateParameters()); }

        /// <summary>
        /// Commande "Creation du PVZ". L'implementation metier est traitee dans une etape dediee ;
        /// seuls la commande, son etat d'activation et son raccordement au ruban sont en place.
        /// </summary>
        public ICommand CommandCreatePvz { get => new RelayCommand(() => ExecuteCreatePvz()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        private readonly ICreoSimpRepService _creoSimpRepService;
        private readonly ICreoParameterService _creoParameterService;

        /// <summary>
        /// Service pur de calcul de DESCRIPTION_MTH (regles 1/2/3/secours), instancie une fois par
        /// lecture d'assemblage pour beneficier du cache Webterm interne le temps de la lecture.
        /// </summary>
        private readonly IWebtermTools _webtermTools;

        private IpfcModel? _activeModel;

        /// <summary>Statut Creo/Windchill de l'assemblage actif, evalue a chaque lecture.</summary>
        private CREOModelStatus _activeModelStatus = CREOModelStatus.UNKNOWNERROR;

        /// <summary>
        /// Vrai lorsque l'assemblage actif peut reellement etre modifie en session
        /// (extrait, nouveau en session ou modifie localement). Sinon toute ecriture de
        /// parametre ou toute sauvegarde est refusee par Creo.
        /// </summary>
        private bool _isActiveModelModifiable;

        /// <summary>Compteur global d'index d'arbre, incremente a chaque composant lu.</summary>
        private int _treeIndexCounter;

        /// <summary>
        /// Correspondance ModelKey -> modele Creo reel, capturee pendant le parcours de la
        /// nomenclature (voir <see cref="ReadAllLevels"/>). Un meme ModelKey n'est jamais
        /// reaffecte : la premiere occurrence rencontree fait foi, ce qui suffit puisque tous les
        /// composants partageant un ModelKey referencent le meme modele Creo par definition.
        /// Reinitialisee a chaque lecture d'assemblage (voir <see cref="ResetContext"/>).
        /// </summary>
        private readonly Dictionary<string, IpfcModel> _modelsByKey = new(StringComparer.OrdinalIgnoreCase);

        public ManufacturingViewModel(ICreoSessionProvider creoSessionProvider,
                                       ICreoModelService creoModelService,
                                       ICreoSimpRepService creoSimpRepService,
                                       ICreoParameterService creoParameterService,
                                       IWebtermTools webtermTools)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;
                _creoSimpRepService = creoSimpRepService;
                _creoParameterService = creoParameterService;
                _webtermTools = webtermTools;

                CurrentDataContext = new ManufacturingDataContext();
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
        /// Lecture hierarchique de l'assemblage actif en tache de fond : les interactions Creo
        /// sont longues, l'interface reste ainsi reactive et le gif d'attente est visible.
        /// </summary>
        private void ReadAsmAsynch()
        {
            try
            {
                _activeModel = _creoModelService.GetActiveModel();
                if (_activeModel == null)
                {
                    System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("MFG_MsgNoActiveModel"),
                                                   McgWpfTools.GetStringResource("MFG_WindowTitle"),
                                                   System.Windows.MessageBoxButton.OK,
                                                   System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var activeFileName = _creoModelService.GetActiveModelFileName();
                if (string.IsNullOrWhiteSpace(activeFileName)
                    || !activeFileName.EndsWith(".asm", StringComparison.OrdinalIgnoreCase))
                {
                    _activeModel = null;
                    System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("MFG_MsgNotAnAssembly"),
                                                   McgWpfTools.GetStringResource("MFG_WindowTitle"),
                                                   System.Windows.MessageBoxButton.OK,
                                                   System.Windows.MessageBoxImage.Warning);
                    return;
                }

                CurrentDataContext.ActiveModelName = activeFileName;

                CheckActiveModelIsModifiable(showMessage: false);

                _treeIndexCounter = 0;
                ReadAllLevels(_activeModel);

                CurrentDataContext.IsAssemblyLoaded = true;

                // Certaines lignes peuvent porter une valeur manuelle qui differe du nouveau
                // calcul (donnee source modifiee depuis la derniere lecture) : une confirmation
                // est demandee avant tout remplacement, conformement a la regle de priorite des
                // modifications manuelles.
                MainDispatcher.Invoke(PromptUpdateRequiredConfirmations);

                TraceLog.AddTraceLog($"Manufacturing View : assemblage '{activeFileName}' lu " +
                                     $"({CurrentDataContext.ListItem.Count} composants).");
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
        /// Noeud opaque du parcours de nomenclature : associe un composant Creo verifie
        /// (<see cref="CreoSimpRepComponentInfo"/>) au modele qu'il reference, deja resolu via
        /// <see cref="ICreoSimpRepService.GetComponentModel(IpfcComponentFeat)"/>. Permet au
        /// service de parcours generique (<see cref="ManufacturingBomTraversalService"/>) de
        /// rester totalement independant de Creo.
        /// </summary>
        private sealed class ManufacturingBomNode
        {
            public required CreoSimpRepComponentInfo Component { get; init; }
            public required IpfcModel? ComponentModel { get; init; }
        }

        /// <summary>
        /// Lit l'integralite des niveaux de la nomenclature de <paramref name="assemblyModel"/> en
        /// s'appuyant sur <see cref="ManufacturingBomTraversalService"/> pour la numerotation, la
        /// detection de cycle et la detection de doublon. Compose exclusivement des methodes deja
        /// verifiees de <see cref="ICreoSimpRepService"/> (ListTopLevelComponents, GetComponentModel) :
        /// aucune API de parcours recursif n'est invoquee, la recursion reste geree par le service
        /// de parcours pur, independant de Creo.
        /// </summary>
        private void ReadAllLevels(IpfcModel assemblyModel)
        {
            var rootChildren = GetChildNodes(assemblyModel);

            var visitResults = ManufacturingBomTraversalService.Traverse(
                rootChildren,
                getChildren: node => node.ComponentModel != null
                    ? GetChildNodes(node.ComponentModel)
                    : Array.Empty<ManufacturingBomNode>(),
                getModelKey: node => node.Component.ModelKey,
                isExpandable: node => node.ComponentModel != null && IsAssemblyModelKey(node.Component.ModelKey),
                maxLevel: MiscToolsConstants.MaxBomLevel);

            // Instancie le service de calcul une fois par lecture d'assemblage : le cache Webterm
            // interne evite ainsi plusieurs appels identiques pour un meme PTC_COMMON_NAME au fil
            // du parcours de toute la nomenclature.
            var descriptionMthCalculationService = new DescriptionMthCalculationService(_webtermTools);

            foreach (var visit in visitResults)
            {
                _treeIndexCounter++;

                var component = visit.Node.Component;
                var componentModel = visit.Node.ComponentModel;

                if (componentModel != null && !string.IsNullOrWhiteSpace(component.ModelKey))
                {
                    // La premiere occurrence rencontree pour un ModelKey donne fait foi : tous
                    // les composants partageant ce ModelKey referencent le meme modele Creo.
                    if (!_modelsByKey.ContainsKey(component.ModelKey))
                        _modelsByKey[component.ModelKey] = componentModel;
                }

                var reference = componentModel != null ? GetModelParameter(componentModel, "REFERENCE") : string.Empty;
                var ptcCommonName = componentModel != null ? GetModelParameter(componentModel, "PTC_COMMON_NAME") : string.Empty;
                var description2 = componentModel != null ? GetModelParameter(componentModel, "DESCRIPTION_2") : string.Empty;
                var description2_1 = componentModel != null ? GetModelParameter(componentModel, "DESCRIPTION2_1") : string.Empty;
                var description2_2 = componentModel != null ? GetModelParameter(componentModel, "DESCRIPTION2_2") : string.Empty;
                var descriptionMthFromCreo = componentModel != null ? GetModelParameter(componentModel, "DESCRIPTION_MTH") : string.Empty;

                var item = new ManufacturingComponentItem
                {
                    TreeIndex = _treeIndexCounter,
                    Level = visit.Level,
                    HierarchicalNumber = visit.HierarchicalNumber,
                    ComponentId = component.Id,
                    Name = component.Name,
                    ModelKey = component.ModelKey,
                    Reference = reference,
                    PtcCommonName = ptcCommonName,
                    Description2 = description2,
                    Description2_1 = description2_1,
                    Description2_2 = description2_2,
                    IsDuplicateModel = visit.IsDuplicateModel,
                    IsCycleDetected = visit.IsCycleDetected,
                };

                // La valeur DESCRIPTION_MTH existante dans Creo est chargee d'abord (sans marquer
                // la ligne comme modifiee manuellement), puis le calcul automatique est applique :
                // s'il n'y a pas de valeur manuelle preexistante, la proposition calculee devient
                // la valeur affichee ; sinon la valeur lue est conservee et le statut refletera le
                // resultat du calcul (Calcule, CalculImpossible ou ErreurWebterm).
                item.LoadDescriptionMthFromCreo(descriptionMthFromCreo);

                var calculationInput = new DescriptionMthCalculationInput
                {
                    PtcCommonName = ptcCommonName,
                    Description2 = description2,
                    Description2_1 = description2_1,
                    Description2_2 = description2_2,
                };

                var calculationResult = descriptionMthCalculationService.Calculate(calculationInput);
                item.ApplyCalculatedDescriptionMth(calculationResult);

                // L'etat lu/calcule devient la reference : tant qu'aucune saisie manuelle ne s'en
                // ecarte, la ligne n'est pas consideree comme modifiee.
                item.CaptureBaseline();

                MainDispatcher.Invoke(() =>
                {
                    SubscribeToPendingChange(item);
                    CurrentDataContext.ListItem.Add(item);
                    CurrentDataContext.NbModels++;
                    CurrentDataContext.NbModelsInProgress++;
                });
            }
        }

        /// <summary>
        /// Liste les composants de premier niveau de <paramref name="assemblyModel"/> et resout,
        /// pour chacun, le modele reference (couteux mais deja fait ainsi avant cette evolution).
        /// Un composant dont le modele ne peut pas etre resolu est neanmoins liste (traite comme
        /// une feuille), conformement a la regle : un composant illisible n'interrompt pas le
        /// parcours du reste de la nomenclature.
        /// </summary>
        private List<ManufacturingBomNode> GetChildNodes(IpfcModel assemblyModel)
        {
            var components = _creoSimpRepService.ListTopLevelComponents(assemblyModel);

            var nodes = new List<ManufacturingBomNode>(components.Count);

            foreach (var component in components)
            {
                IpfcModel? componentModel = null;

                try
                {
                    componentModel = component.ComponentFeature != null
                        ? _creoSimpRepService.GetComponentModel(component.ComponentFeature)
                        : null;
                }
                catch
                {
                    // Modele non resolvable (reference cassee, composant inaccessible, ...) :
                    // le composant reste liste, sans descendance ni parametres.
                }

                nodes.Add(new ManufacturingBomNode
                {
                    Component = component,
                    ComponentModel = componentModel
                });
            }

            return nodes;
        }

        private static bool IsAssemblyModelKey(string modelKey)
        {
            return !string.IsNullOrWhiteSpace(modelKey)
                && modelKey.EndsWith(".ASM", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reparcourt integralement <paramref name="assemblyModel"/> (nouvellement reouvert depuis
        /// le dossier de travail local) et remplace, pour CHAQUE ModelKey rencontre, le handle
        /// Creo dans <see cref="_modelsByKey"/> par le handle valide de la session courante.
        ///
        /// A la difference de la mise a jour ponctuelle faite dans <see cref="UpdateSingleModel"/>
        /// (qui ne rafraichit que les modeles effectivement modifies), cette methode couvre TOUS
        /// les composants de la nomenclature : apres <see cref="CloseAssemblyAndClearSession"/>,
        /// l'integralite des handles non affiches est invalidee par Creo, qu'ils fassent partie du
        /// plan de mise a jour du cycle courant ou non.
        /// </summary>
        private void RefreshModelsByKey(IpfcModel reopenedAssemblyModel)
        {
            try
            {
                var rootChildren = GetChildNodes(reopenedAssemblyModel);

                var visitResults = ManufacturingBomTraversalService.Traverse(
                    rootChildren,
                    getChildren: node => node.ComponentModel != null
                        ? GetChildNodes(node.ComponentModel)
                        : Array.Empty<ManufacturingBomNode>(),
                    getModelKey: node => node.Component.ModelKey,
                    isExpandable: node => node.ComponentModel != null && IsAssemblyModelKey(node.Component.ModelKey),
                    maxLevel: MiscToolsConstants.MaxBomLevel);

                foreach (var visit in visitResults)
                {
                    var component = visit.Node.Component;
                    var componentModel = visit.Node.ComponentModel;

                    if (componentModel != null && !string.IsNullOrWhiteSpace(component.ModelKey))
                    {
                        _modelsByKey[component.ModelKey] = componentModel;
                    }
                }
            }
            catch (Exception ex)
            {
                // Non bloquant : au pire, un modele non rafraichi ici echouera plus loin avec un
                // message explicite (NotFound / Error) sans masquer le traitement des autres.
                TraceLog.AddTraceLog($"Manufacturing View : echec du rafraichissement complet de _modelsByKey apres reouverture locale : {ex.Message}.");
            }
        }

        private void ExecuteSaveModel()
        {
            try
            {
                if (!EnsureActiveModelIsModifiable()) return;

                if (_activeModel == null)
                {
                    ShowWarning("MFG_MsgNoActiveModel");
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

        /// <summary>Sauvegarde Creo executee en tache de fond : l'ecriture du modele peut etre longue.</summary>
        private void SaveModelAsynch()
        {
            try
            {
                if (_activeModel == null) return;

                if (!_creoSimpRepService.SaveOwnerModel(_activeModel))
                {
                    ShowWarning("MFG_MsgSaveFailed");
                    return;
                }

                TraceLog.AddTraceLog($"Manufacturing View : modele '{CurrentDataContext.ActiveModelName}' sauvegarde.");

                System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("MFG_MsgSaveSuccess"),
                                               McgWpfTools.GetStringResource("MFG_WindowTitle"),
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

        /// <summary>
        /// Commande "Mise a jour" : la commande, son activation et son raccordement au ruban sont
        /// en place. L'ecriture effective des parametres Creo sera realisee dans une etape dediee.
        ///
        /// A la difference de "Creation du PVZ", cette action ne necessite pas que l'assemblage
        /// actif soit deja modifiable (CHECKEDOUT) en session : la mise a jour ne modifie qu'une
        /// copie de travail locale (backup/reload via <see cref="ICreoModelService"/>), jamais le
        /// modele extrait/gere dans Windchill. Le controle habituel via
        /// <see cref="EnsureActiveModelIsModifiable"/> est donc volontairement ignore ici.
        /// </summary>
        private void ExecuteUpdateParameters()
        {
            try
            {
                if (CurrentDataContext.IsUpdateRunning)
                {
                    ShowWarning("MFG_MsgUpdateAlreadyRunning");
                    return;
                }

                if (!CurrentDataContext.IsAssemblyLoaded || _activeModel == null)
                {
                    ShowWarning("MFG_MsgNoActiveModel");
                    return;
                }

                // ------------------------------------------------------------
                // 1) Construction des candidats purs (sans Creo) et planification
                // ------------------------------------------------------------
                var candidates = CurrentDataContext.ListItem.Select(item => new ManufacturingUpdateCandidate
                {
                    ModelKey = item.ModelKey,
                    ComponentName = item.Name,
                    HasReferenceChanged = item.HasReferenceChanged,
                    ReferenceValue = item.Reference,
                    HasDescriptionMthChanged = item.HasDescriptionMthChanged,
                    DescriptionMthValue = item.DescriptionMth
                }).ToList();

                var plan = ManufacturingUpdatePlanningService.BuildPlan(candidates);

                if (plan.IsEmpty)
                {
                    ShowInformation("MFG_MsgNoPendingChanges");
                    return;
                }

                if (plan.Unresolved.Count > 0)
                {
                    var unresolvedNames = string.Join(", ", plan.Unresolved.Select(u => u.ComponentName));
                    TraceLog.AddTraceLog($"Manufacturing View : {plan.Unresolved.Count} ligne(s) exclue(s) (modele non identifie) : {unresolvedNames}.");

                    System.Windows.MessageBox.Show(
                        string.Format(McgWpfTools.GetStringResource("MFG_MsgUpdateUnresolvedDetected"), plan.Unresolved.Count),
                        McgWpfTools.GetStringResource("MFG_WindowTitle"),
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                }

                if (plan.HasConflicts)
                {
                    var conflictDetail = string.Join(Environment.NewLine, plan.Conflicts.Select(c =>
                        $"- {string.Join(", ", c.ComponentNames)} : REFERENCE=[{string.Join(" / ", c.ConflictingReferenceValues)}] " +
                        $"DESCRIPTION_MTH=[{string.Join(" / ", c.ConflictingDescriptionMthValues)}]"));

                    TraceLog.AddTraceLog($"Manufacturing View : mise a jour bloquee, {plan.Conflicts.Count} conflit(s) detecte(s).");

                    System.Windows.MessageBox.Show(
                        string.Format(McgWpfTools.GetStringResource("MFG_MsgUpdateConflictsDetected"), plan.Conflicts.Count, conflictDetail),
                        McgWpfTools.GetStringResource("MFG_WindowTitle"),
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);

                    return;
                }

                if (plan.Entries.Count == 0)
                {
                    ShowInformation("MFG_MsgNoPendingChanges");
                    return;
                }

                // ------------------------------------------------------------
                // 2) Confirmation globale avant toute action destructive
                // ------------------------------------------------------------
                var confirmation = System.Windows.MessageBox.Show(
                    string.Format(McgWpfTools.GetStringResource("MFG_MsgUpdateConfirmGlobal"), plan.Entries.Count),
                    McgWpfTools.GetStringResource("MFG_WindowTitle"),
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (confirmation != System.Windows.MessageBoxResult.Yes)
                    return;

                // ------------------------------------------------------------
                // 3) Lancement en tache de fond : desactive les commandes concurrentes
                // ------------------------------------------------------------
                CurrentDataContext.IsUpdateRunning = true;

                Thread updateThread = new Thread(() => UpdateParametersAsynch(plan));
                updateThread.IsBackground = true;
                updateThread.Start();
            }
            catch (Exception ex)
            {
                CurrentDataContext.IsUpdateRunning = false;
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Execute le plan de mise a jour (option B) en tache de fond :
        /// 1) prepare et verifie un dossier de travail local dedie ;
        /// 2) sauvegarde localement l'assemblage actif et chaque modele concerne (Backup Creo),
        ///    sans jamais toucher a la session active tant que la copie n'est pas verifiee ;
        /// 3) ferme la fenetre source et vide les modeles non affiches (meme mecanisme deja
        ///    utilise par <see cref="ICreoModelService.OpenBackupReloadAndPurgeTempDetailed"/>) ;
        /// 4) rouvre l'assemblage principal puis chaque modele depuis le dossier local ;
        /// 5) ecrit REFERENCE/DESCRIPTION_MTH et sauvegarde chaque modele modifie, une seule fois
        ///    par <see cref="ManufacturingUpdatePlanEntry.ModelKey"/> ;
        /// 6) sauvegarde l'assemblage principal (meme mecanisme que SimplifiedRep) ;
        /// 7) affiche un bilan detaille par modele.
        /// </summary>
        private void UpdateParametersAsynch(ManufacturingUpdatePlan plan)
        {
            var outcomes = new List<ManufacturingUpdateOutcome>();
            string? mainAssemblyFileName = CurrentDataContext.ActiveModelName;

            try
            {
                // --------------------------------------------------------------
                // Etape 1 : dossier de travail local dedie, verifie avant toute
                // action destructive sur la session Creo active.
                // --------------------------------------------------------------
                var workFolder = ManufacturingUpdateWorkFolder.PrepareWorkFolder();
                if (!workFolder.Success)
                {
                    MainDispatcher.Invoke(() => System.Windows.MessageBox.Show(
                        string.Format(McgWpfTools.GetStringResource("MFG_MsgUpdateWorkFolderFailed"), workFolder.Detail),
                        McgWpfTools.GetStringResource("MFG_WindowTitle"),
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error));

                    TraceLog.AddTraceLog($"Manufacturing View : echec preparation dossier de travail ({workFolder.Detail}).");
                    return;
                }

                TraceLog.AddTraceLog(string.Format(McgWpfTools.GetStringResource("MFG_MsgUpdateWorkFolderPath"), workFolder.FolderPath));

                // --------------------------------------------------------------
                // Etape 2 : copie locale (Backup) de l'assemblage principal ET de
                // chaque modele du plan, AVANT tout effacement de la session.
                // --------------------------------------------------------------
                if (_activeModel == null || string.IsNullOrWhiteSpace(mainAssemblyFileName))
                    return;

                if (!TryBackupModel(_activeModel, workFolder.FolderPath, out var mainBackupFileName, out var backupError))
                {
                    ReportBackupFailure(mainAssemblyFileName, backupError);
                    return;
                }

                var backedUpFileNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var entry in plan.Entries)
                {
                    if (!_modelsByKey.TryGetValue(entry.ModelKey, out var model) || model == null)
                    {
                        outcomes.Add(new ManufacturingUpdateOutcome
                        {
                            ModelKey = entry.ModelKey,
                            ComponentNames = entry.ComponentNames,
                            Status = ManufacturingUpdateOutcomeStatus.NotFound,
                            Detail = "Modele introuvable dans la correspondance ModelKey capturee lors de la lecture."
                        });
                        continue;
                    }

                    // Le modele principal est deja sauvegarde ci-dessus ; ne pas le sauvegarder deux fois.
                    if (string.Equals(model.FileName, _activeModel.FileName, StringComparison.OrdinalIgnoreCase))
                    {
                        backedUpFileNames[entry.ModelKey] = mainBackupFileName;
                        continue;
                    }

                    if (!TryBackupModel(model, workFolder.FolderPath, out var backupFileName, out var modelBackupError))
                    {
                        ReportBackupFailure(model.FileName, modelBackupError);
                        return;
                    }

                    backedUpFileNames[entry.ModelKey] = backupFileName;
                }

                // --------------------------------------------------------------
                // Etape 3 : la copie locale est verifiee (tous les Backup ont
                // reussi) : on peut maintenant vider completement la session Creo
                // en toute securite (assemblages, pieces, dessins, affiches ou non).
                // En cas d'echec de la purge, le traitement est interrompu : on ne
                // rouvre jamais depuis une session partiellement videe.
                // --------------------------------------------------------------
                if (!ClearSessionCompletelyOrReportFailure())
                    return;

                // --------------------------------------------------------------
                // Etape 4 : reouverture de l'assemblage principal puis de chaque
                // modele depuis le dossier de travail local. Utilise la surcharge
                // (dossier, fichier) deja eprouvee ailleurs dans l'application
                // (JpgExportViewModel, DxfExportViewModel, CadDocRenameViewModel) :
                // elle combine le chemin complet et le passe via IpfcModelDescriptor.Path
                // avec Instance = null, ce qui evite pfcExceptions::XToolkitNotFound
                // rencontre avec la surcharge a 3 parametres (Instance = nom de fichier).
                // --------------------------------------------------------------
                var reopenedMainAssembly = _creoModelService.RetrieveModelFromLocalDir(workFolder.FolderPath, mainBackupFileName);

                if (reopenedMainAssembly == null)
                {
                    MainDispatcher.Invoke(() => ShowWarning("MFG_MsgUpdateReopenFailed"));
                    TraceLog.AddTraceLog("Manufacturing View : echec de reouverture de l'assemblage principal depuis le dossier de travail local.");
                    return;
                }

                try
                {
                    reopenedMainAssembly.Display();
                }
                catch
                {
                    // Non bloquant : la sauvegarde ne necessite pas d'affichage.
                }

                // Important : CloseAssemblyAndClearSession() (EraseUndisplayedModels) invalide
                // TOUS les handles Creo non affiches de la session, y compris ceux des composants
                // qui ne font pas partie du plan traite ce cycle-ci. Sans ce rafraichissement
                // complet, un composant modifie pour la premiere fois lors d'un cycle ulterieur
                // utiliserait un handle perime capture lors de la lecture initiale (ou d'un cycle
                // precedent) et echouerait avec pfcExceptions::XToolkitGeneralError.
                RefreshModelsByKey(reopenedMainAssembly);

                // --------------------------------------------------------------
                // Etape 5 : ecriture des parametres et sauvegarde de chaque modele.
                // --------------------------------------------------------------
                foreach (var entry in plan.Entries)
                {
                    if (!backedUpFileNames.TryGetValue(entry.ModelKey, out var backupFileName))
                        continue; // Deja consigne en NotFound ci-dessus.

                    UpdateSingleModel(entry, workFolder.FolderPath, backupFileName, reopenedMainAssembly, outcomes);
                }

                // --------------------------------------------------------------
                // Etape 6 : sauvegarde de l'assemblage principal (meme mecanisme
                // que SimplifiedRep).
                // --------------------------------------------------------------
                bool mainAssemblySaved;
                try
                {
                    mainAssemblySaved = _creoSimpRepService.SaveOwnerModel(reopenedMainAssembly);
                }
                catch (Exception ex)
                {
                    mainAssemblySaved = false;
                    TraceLog.AddTraceLog($"Manufacturing View : exception a la sauvegarde de l'assemblage principal : {ex.Message}.");
                }

                if (!mainAssemblySaved)
                {
                    MainDispatcher.Invoke(() => ShowWarning("MFG_MsgUpdateAssemblySaveFailed"));
                    TraceLog.AddTraceLog("Manufacturing View : echec de la sauvegarde de l'assemblage principal.");
                }

                _activeModel = reopenedMainAssembly;

                // --------------------------------------------------------------
                // Etape 7 : bilan detaille, uniquement apres confirmation reelle
                // de chaque sauvegarde.
                // --------------------------------------------------------------
                MainDispatcher.Invoke(() =>
                {
                    ApplyOutcomesToGrid(outcomes);
                    ShowUpdateSummary(outcomes, mainAssemblySaved);
                });
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsUpdateRunning = false;
            }
        }

        /// <summary>
        /// Sauvegarde (Backup Creo) <paramref name="model"/> dans <paramref name="workFolder"/>.
        /// Retourne le nom de fichier reellement utilise pour la copie (celui du modele).
        /// N'ecrase jamais un fichier existant dans le dossier de travail : celui-ci vient
        /// d'etre cree par <see cref="ManufacturingUpdateWorkFolder.PrepareWorkFolder"/> et est
        /// donc garanti vide.
        /// </summary>
        private bool TryBackupModel(IpfcModel model, string workFolder, out string backupFileName, out string errorDetail)
        {
            backupFileName = model.FileName;
            errorDetail = string.Empty;

            try
            {
                var backupDescriptor = new CCpfcModelDescriptor().Create(model.Type, backupFileName, null);
                backupDescriptor.Path = workFolder;

                model.Backup(backupDescriptor);

                TraceLog.AddTraceLog($"Manufacturing View : copie locale de '{backupFileName}' creee dans '{workFolder}'.");
                return true;
            }
            catch (Exception ex)
            {
                errorDetail = ex.Message;
                return false;
            }
        }

        private void ReportBackupFailure(string modelFileName, string errorDetail)
        {
            MainDispatcher.Invoke(() => System.Windows.MessageBox.Show(
                string.Format(McgWpfTools.GetStringResource("MFG_MsgUpdateBackupFailed"), modelFileName, errorDetail),
                McgWpfTools.GetStringResource("MFG_WindowTitle"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error));

            TraceLog.AddTraceLog($"Manufacturing View : echec de la copie locale de '{modelFileName}' : {errorDetail}. Session non videe.");
        }

        /// <summary>
        /// Vide completement la session Creo via <see cref="ICreoModelService.ClearSessionCompletely"/> :
        /// toutes les fenetres sont fermees puis les modeles non affiches sont erases (deux
        /// passages de securite), avant verification finale que la session est reellement vide
        /// (assemblages, pieces, dessins, affiches ou non). Chaque etape est journalisee.
        ///
        /// Limitation Creo documentee : si un modele reste affiche au moment de l'appel,
        /// <c>Erase()</c>/<c>EraseUndisplayedModels()</c> ne l'efface pas immediatement (report a
        /// la reprise de main de l'IHM Creo). C'est pourquoi toutes les fenetres sont fermees en
        /// premier lieu. Aucune API Creo VB ne garantit un vidage total en un seul appel ; en cas
        /// d'echec de la verification finale, le traitement est interrompu explicitement plutot
        /// que de poursuivre sur une session partiellement videe.
        /// </summary>
        /// <returns>True si la session est verifiee vide ; false si la purge a echoue.</returns>
        private bool ClearSessionCompletelyOrReportFailure()
        {
            TraceLog.AddTraceLog("Manufacturing View : debut du videment complet de la session Creo.");

            SessionClearResult result;

            try
            {
                result = _creoModelService.ClearSessionCompletely();
            }
            catch (Exception ex)
            {
                TraceLog.AddTraceLog($"Manufacturing View : echec du videment de la session Creo ({ex.Message}).");

                MainDispatcher.Invoke(() => System.Windows.MessageBox.Show(
                    string.Format(McgWpfTools.GetStringResource("MFG_MsgSessionClearFailed"), ex.Message),
                    McgWpfTools.GetStringResource("MFG_WindowTitle"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error));

                return false;
            }

            foreach (var logLine in result.Logs)
            {
                TraceLog.AddTraceLog($"Manufacturing View : {logLine}");
            }

            if (!result.SessionIsEmpty)
            {
                var remainingNames = string.Join(", ", result.RemainingModelFileNames);

                TraceLog.AddTraceLog(
                    $"Manufacturing View : videment de session incomplet, {result.RemainingModelsCount} " +
                    $"modele(s) restant(s) : {remainingNames}.");

                MainDispatcher.Invoke(() => System.Windows.MessageBox.Show(
                    string.Format(McgWpfTools.GetStringResource("MFG_MsgSessionNotEmpty"), result.RemainingModelsCount, remainingNames),
                    McgWpfTools.GetStringResource("MFG_WindowTitle"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error));

                return false;
            }

            TraceLog.AddTraceLog(
                $"Manufacturing View : session Creo videe avec succes ({result.ClosedWindowsCount} fenetre(s) fermee(s)).");

            return true;
        }

        /// <summary>
        /// Retrouve un modele deja mis a jour (option B) dans la nouvelle session locale,
        /// ecrit REFERENCE/DESCRIPTION_MTH selon <paramref name="entry"/> et sauvegarde le
        /// modele. Consigne un resultat detaille dans <paramref name="outcomes"/> dans tous les
        /// cas (succes ou echec), sans jamais interrompre le traitement des autres modeles.
        /// </summary>
        private void UpdateSingleModel(
            ManufacturingUpdatePlanEntry entry,
            string workFolder,
            string backupFileName,
            IpfcModel reopenedMainAssembly,
            List<ManufacturingUpdateOutcome> outcomes)
        {
            try
            {
                IpfcModel? model;

                // Le modele principal a deja ete rouvert par la reouverture de l'assemblage.
                if (string.Equals(backupFileName, reopenedMainAssembly.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    model = reopenedMainAssembly;
                }
                else
                {
                    // Surcharge (dossier, fichier) deja eprouvee ailleurs dans l'application :
                    // combine le chemin complet et le passe via IpfcModelDescriptor.Path avec
                    // Instance = null, evitant pfcExceptions::XToolkitNotFound.
                    model = _creoModelService.RetrieveModelFromLocalDir(workFolder, backupFileName);
                }

                if (model == null)
                {
                    outcomes.Add(new ManufacturingUpdateOutcome
                    {
                        ModelKey = entry.ModelKey,
                        ComponentNames = entry.ComponentNames,
                        Status = ManufacturingUpdateOutcomeStatus.NotFound,
                        Detail = $"Modele '{backupFileName}' introuvable apres reouverture locale."
                    });
                    return;
                }

                // Important : la session a ete videe (EraseUndisplayedModels) puis rouverte
                // depuis le dossier local. Les handles COM captures dans _modelsByKey lors de la
                // derniere lecture (ReadAllLevels) sont donc perimes pour ce ModelKey. Sans cette
                // mise a jour, un second lancement de "Mise a jour" dans la meme session
                // utiliserait un handle invalide et echouerait avec pfcExceptions::XToolkitGeneralError.
                _modelsByKey[entry.ModelKey] = model;

                if (entry.UpdateReference)
                {
                    var referenceStatus = _creoParameterService.SetParameter(model, "REFERENCE", entry.ReferenceValue, false);
                    if (referenceStatus != CREOModelStatus.OK)
                    {
                        outcomes.Add(new ManufacturingUpdateOutcome
                        {
                            ModelKey = entry.ModelKey,
                            ComponentNames = entry.ComponentNames,
                            Status = ManufacturingUpdateOutcomeStatus.Error,
                            Detail = $"Echec ecriture REFERENCE (statut {referenceStatus})."
                        });
                        return;
                    }
                }

                if (entry.UpdateDescriptionMth)
                {
                    var descriptionStatus = _creoParameterService.SetParameter(model, "DESCRIPTION_MTH", entry.DescriptionMthValue, false);
                    if (descriptionStatus != CREOModelStatus.OK)
                    {
                        outcomes.Add(new ManufacturingUpdateOutcome
                        {
                            ModelKey = entry.ModelKey,
                            ComponentNames = entry.ComponentNames,
                            Status = ManufacturingUpdateOutcomeStatus.Error,
                            Detail = $"Echec ecriture DESCRIPTION_MTH (statut {descriptionStatus})."
                        });
                        return;
                    }
                }

                // Le modele principal sera sauvegarde une seule fois, apres tous les composants
                // (voir etape 6 de UpdateParametersAsynch) : ne pas le sauvegarder ici en double.
                if (!ReferenceEquals(model, reopenedMainAssembly))
                {
                    bool saved;
                    try
                    {
                        saved = _creoSimpRepService.SaveOwnerModel(model);
                    }
                    catch (Exception ex)
                    {
                        outcomes.Add(new ManufacturingUpdateOutcome
                        {
                            ModelKey = entry.ModelKey,
                            ComponentNames = entry.ComponentNames,
                            Status = ManufacturingUpdateOutcomeStatus.Error,
                            Detail = $"Exception a la sauvegarde : {ex.Message}."
                        });
                        return;
                    }

                    if (!saved)
                    {
                        outcomes.Add(new ManufacturingUpdateOutcome
                        {
                            ModelKey = entry.ModelKey,
                            ComponentNames = entry.ComponentNames,
                            Status = ManufacturingUpdateOutcomeStatus.Error,
                            Detail = "La sauvegarde Creo a echoue (SaveOwnerModel a retourne false)."
                        });
                        return;
                    }
                }

                outcomes.Add(new ManufacturingUpdateOutcome
                {
                    ModelKey = entry.ModelKey,
                    ComponentNames = entry.ComponentNames,
                    Status = ManufacturingUpdateOutcomeStatus.Updated,
                    Detail = string.Empty
                });
            }
            catch (Exception ex)
            {
                // Une erreur sur ce modele ne doit jamais masquer les resultats des autres.
                outcomes.Add(new ManufacturingUpdateOutcome
                {
                    ModelKey = entry.ModelKey,
                    ComponentNames = entry.ComponentNames,
                    Status = ManufacturingUpdateOutcomeStatus.Error,
                    Detail = ex.Message
                });
            }
        }

        /// <summary>
        /// N'actualise la grille que pour les modeles reellement mis a jour avec succes : les
        /// lignes correspondantes redeviennent la nouvelle baseline (plus de surbrillance).
        /// </summary>
        private void ApplyOutcomesToGrid(IEnumerable<ManufacturingUpdateOutcome> outcomes)
        {
            var updatedModelKeys = outcomes
                .Where(o => o.Status == ManufacturingUpdateOutcomeStatus.Updated)
                .Select(o => o.ModelKey)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (updatedModelKeys.Count == 0) return;

            foreach (var item in CurrentDataContext.ListItem)
            {
                if (updatedModelKeys.Contains(item.ModelKey))
                {
                    // La nouvelle valeur vient d'etre ecrite avec succes dans Creo : elle devient
                    // la valeur "telle que trouvee dans Creo" de reference. Sans cette
                    // resynchronisation, CaptureBaseline() comparerait toujours a l'ancienne
                    // valeur lue avant la mise a jour et la ligne resterait surlignee a tort.
                    item.InitialDescriptionMth = item.DescriptionMth;
                    item.CaptureBaseline();
                }
            }

            RefreshPendingChangesState();
        }

        private static void ShowUpdateSummary(IReadOnlyList<ManufacturingUpdateOutcome> outcomes, bool mainAssemblySaved)
        {
            var lines = outcomes.Select(o => string.Format(
                McgWpfTools.GetStringResource("MFG_UpdateOutcomeLine"),
                string.Join(", ", o.ComponentNames),
                McgWpfTools.GetStringResource($"MFG_UpdateOutcome_{o.Status}"),
                o.Detail,
                string.IsNullOrEmpty(o.Detail) ? string.Empty : Environment.NewLine));

            var summary = string.Join(Environment.NewLine, lines);

            TraceLog.AddTraceLog($"Manufacturing View : bilan de mise a jour - {outcomes.Count(o => o.Status == ManufacturingUpdateOutcomeStatus.Updated)} mis a jour, " +
                                  $"{outcomes.Count(o => o.Status == ManufacturingUpdateOutcomeStatus.Error)} en erreur, " +
                                  $"{outcomes.Count(o => o.Status == ManufacturingUpdateOutcomeStatus.NotFound)} introuvable(s), " +
                                  $"assemblage principal sauvegarde : {mainAssemblySaved}.");

            System.Windows.MessageBox.Show(
                summary,
                McgWpfTools.GetStringResource("MFG_MsgUpdateSummaryTitle"),
                System.Windows.MessageBoxButton.OK,
                mainAssemblySaved ? System.Windows.MessageBoxImage.Information : System.Windows.MessageBoxImage.Warning);
        }

        /// <summary>
        /// Commande "Creation du PVZ" : exporte l'assemblage actif au format ProductView (.pvz)
        /// via l'API VB Creo officielle (<see cref="ICreoModelService.ExportModelToPvz"/>).
        ///
        /// L'export est realise a partir d'une copie locale de travail (Backup), jamais du
        /// modele extrait/gere directement dans Windchill : le mecanisme de copie locale suit le
        /// meme pattern deja verifie dans <see cref="ExecuteUpdateParameters"/> / <see cref="TryBackupModel"/>.
        /// </summary>
        private void ExecuteCreatePvz()
        {
            try
            {
                if (CurrentDataContext.IsPvzRunning)
                {
                    ShowWarning("MFG_MsgPvzAlreadyRunning");
                    return;
                }

                if (!EnsureActiveModelIsModifiable()) return;

                if (_activeModel == null)
                {
                    ShowWarning("MFG_MsgNoActiveModel");
                    return;
                }

                string modelFileName = _activeModel.FileName;
                string suggestedName = Path.GetFileNameWithoutExtension(modelFileName);

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = McgWpfTools.GetStringResource("MFG_MsgPvzSelectDestination"),
                    FileName = suggestedName,
                    DefaultExt = ".pvz",
                    Filter = "ProductView PVZ (*.pvz)|*.pvz",
                    AddExtension = true,
                    OverwritePrompt = true
                };

                bool? dialogResult = saveDialog.ShowDialog();
                if (dialogResult != true)
                    return;

                string destinationFullPath = saveDialog.FileName;
                string destinationFolder = Path.GetDirectoryName(destinationFullPath) ?? string.Empty;
                string destinationFileNameWithoutExtension = Path.GetFileNameWithoutExtension(destinationFullPath);

                if (string.IsNullOrWhiteSpace(destinationFolder) || !Directory.Exists(destinationFolder))
                {
                    ShowWarning("MFG_MsgUpdateWorkFolderFailed");
                    return;
                }

                CurrentDataContext.IsPvzRunning = true;
                CurrentDataContext.IsPleaseWaitShown = true;

                var pvzModel = _activeModel;
                var pvzModelFileName = modelFileName;

                Thread pvzThread = new Thread(() => CreatePvzAsynch(pvzModel, pvzModelFileName, destinationFolder, destinationFileNameWithoutExtension));
                pvzThread.IsBackground = true;
                pvzThread.Start();
            }
            catch (Exception ex)
            {
                CurrentDataContext.IsPvzRunning = false;
                CurrentDataContext.IsPleaseWaitShown = false;
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Execution en tache de fond de la creation du PVZ : copie locale (Backup) de
        /// l'assemblage actif puis export ProductView (.pvz) via
        /// <see cref="ICreoModelService.ExportModelToPvz"/>. La reussite n'est declaree que si le
        /// fichier PVZ existe reellement sur disque a l'issue de l'export.
        /// </summary>
        private void CreatePvzAsynch(IpfcModel model, string modelFileName, string destinationFolder, string destinationFileNameWithoutExtension)
        {
            string operationStart = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            TraceLog.AddTraceLog($"Manufacturing View : debut creation PVZ pour '{modelFileName}' -> '{destinationFolder}\\{destinationFileNameWithoutExtension}.pvz' ({operationStart}).");

            try
            {
                // --------------------------------------------------------------
                // Etape 1 : dossier de travail local dedie (meme mecanisme que la
                // mise a jour des parametres), verifie avant toute action Creo.
                // --------------------------------------------------------------
                var workFolder = ManufacturingUpdateWorkFolder.PrepareWorkFolder();
                if (!workFolder.Success)
                {
                    TraceLog.AddTraceLog($"Manufacturing View : creation PVZ annulee, echec preparation dossier de travail ({workFolder.Detail}).");

                    MainDispatcher.Invoke(() => System.Windows.MessageBox.Show(
                        string.Format(McgWpfTools.GetStringResource("MFG_MsgPvzBackupFailed"), workFolder.Detail),
                        McgWpfTools.GetStringResource("MFG_WindowTitle"),
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error));

                    return;
                }

                // --------------------------------------------------------------
                // Etape 2 : copie locale (Backup) du modele source avant tout export,
                // afin de ne jamais operer directement sur le modele gere en session.
                // --------------------------------------------------------------
                if (!TryBackupModel(model, workFolder.FolderPath, out var backupFileName, out var backupError))
                {
                    TraceLog.AddTraceLog($"Manufacturing View : creation PVZ annulee, echec de la copie locale de '{modelFileName}' : {backupError}.");

                    MainDispatcher.Invoke(() => System.Windows.MessageBox.Show(
                        string.Format(McgWpfTools.GetStringResource("MFG_MsgPvzBackupFailed"), backupError),
                        McgWpfTools.GetStringResource("MFG_WindowTitle"),
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error));

                    return;
                }

                var backupModel = _creoModelService.RetrieveModelFromLocalDir(workFolder.FolderPath, backupFileName);
                if (backupModel == null)
                {
                    TraceLog.AddTraceLog("Manufacturing View : creation PVZ annulee, impossible de recharger la copie locale.");

                    MainDispatcher.Invoke(() => ShowWarning("MFG_MsgUpdateReopenFailed"));
                    return;
                }

                // --------------------------------------------------------------
                // Etape 3 : export ProductView (.pvz) via l'API VB Creo officielle.
                // --------------------------------------------------------------
                string exportedFullPath;
                try
                {
                    exportedFullPath = _creoModelService.ExportModelToPvz(backupModel, destinationFolder, destinationFileNameWithoutExtension);
                }
                catch (Exception ex)
                {
                    TraceLog.AddTraceLog($"Manufacturing View : echec de l'export PVZ pour '{modelFileName}' : {ex.Message}.");

                    MainDispatcher.Invoke(() => System.Windows.MessageBox.Show(
                        string.Format(McgWpfTools.GetStringResource("MFG_MsgPvzExportFailed"), ex.Message),
                        McgWpfTools.GetStringResource("MFG_WindowTitle"),
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error));

                    return;
                }

                // --------------------------------------------------------------
                // Etape 4 : la reussite n'est declaree que si le fichier existe
                // reellement sur disque a l'issue de l'appel Export().
                // --------------------------------------------------------------
                if (!File.Exists(exportedFullPath))
                {
                    TraceLog.AddTraceLog($"Manufacturing View : creation PVZ en echec, fichier attendu introuvable ('{exportedFullPath}').");

                    MainDispatcher.Invoke(() => ShowWarning("MFG_MsgPvzFileNotCreated"));
                    return;
                }

                string operationEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                TraceLog.AddTraceLog($"Manufacturing View : creation PVZ reussie pour '{modelFileName}' -> '{exportedFullPath}' ({operationEnd}).");

                MainDispatcher.Invoke(() => System.Windows.MessageBox.Show(
                    string.Format(McgWpfTools.GetStringResource("MFG_MsgPvzExportSuccess"), exportedFullPath),
                    McgWpfTools.GetStringResource("MFG_WindowTitle"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information));
            }
            catch (Exception ex)
            {
                TraceLog.AddTraceLog($"Manufacturing View : exception non geree pendant la creation PVZ : {ex.Message}.");
                MainDispatcher.Invoke(() => MiscToolsException.SendMessageBox(this.GetType().Name, ex));
            }
            finally
            {
                MainDispatcher.Invoke(() =>
                {
                    CurrentDataContext.IsPvzRunning = false;
                    CurrentDataContext.IsPleaseWaitShown = false;
                });
            }
        }
        #endregion

        #region [REGION] Private Methods
        private bool EnsureActiveModelIsModifiable()
        {
            return CheckActiveModelIsModifiable(showMessage: true);
        }

        /// <summary>
        /// Evalue si l'assemblage actif est reellement modifiable en session.
        /// Un modele extrait (CHECKEDOUT), nouveau en session (NEWINSESSION) ou modifie
        /// localement (LOCALLYMODIFIED) peut etre modifie. Tout autre statut correspond a un
        /// modele en lecture seule.
        /// </summary>
        private bool CheckActiveModelIsModifiable(bool showMessage)
        {
            try
            {
                if (_activeModel == null)
                {
                    _activeModelStatus = CREOModelStatus.UNKNOWNERROR;
                    _isActiveModelModifiable = false;
                }
                else
                {
                    _activeModelStatus = _creoModelService.GetModelStatus(_activeModel);

                    _isActiveModelModifiable =
                        _activeModelStatus == CREOModelStatus.CHECKEDOUT
                        || _activeModelStatus == CREOModelStatus.NEWINSESSION
                        || _activeModelStatus == CREOModelStatus.LOCALLYMODIFIED;
                }
            }
            catch
            {
                _activeModelStatus = CREOModelStatus.UNKNOWNERROR;
                _isActiveModelModifiable = false;
            }

            CurrentDataContext.IsActiveModelModifiable = _isActiveModelModifiable;

            if (!_isActiveModelModifiable)
            {
                TraceLog.AddTraceLog($"Manufacturing View : assemblage non modifiable (statut {_activeModelStatus}).");

                if (showMessage)
                    MainDispatcher.Invoke(() => ShowWarning("MFG_MsgModelNotModifiable"));
            }

            return _isActiveModelModifiable;
        }

        /// <summary>
        /// Parcourt les lignes fraichement lues et propose, pour chacune de celles marquees
        /// <see cref="DescriptionMthStatus.UpdateRequired"/> (valeur manuelle existante qui differe
        /// du nouveau calcul), une confirmation explicite avant tout remplacement. Reutilise le
        /// mecanisme de dialogue deja en place dans MiscTools (MessageBox.Show / YesNo), comme dans
        /// <c>SimplifiedRepViewModel.ConfirmPendingChangesLoss</c>.
        /// </summary>
        private void PromptUpdateRequiredConfirmations()
        {
            foreach (var item in CurrentDataContext.ListItem)
            {
                if (item.Status != DescriptionMthStatus.UpdateRequired || item.CalculatedDescriptionMth == null)
                    continue;

                var message = string.Format(
                    McgWpfTools.GetStringResource("MFG_MsgConfirmDescriptionMthUpdate"),
                    item.Name,
                    item.DescriptionMth,
                    item.CalculatedDescriptionMth);

                var confirmation = System.Windows.MessageBox.Show(
                    message,
                    McgWpfTools.GetStringResource("MFG_WindowTitle"),
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (confirmation == System.Windows.MessageBoxResult.Yes)
                    item.AcceptCalculatedDescriptionMth();
            }

            RefreshPendingChangesState();
        }

        private static void ShowWarning(string resourceKey)
        {
            System.Windows.MessageBox.Show(McgWpfTools.GetStringResource(resourceKey),
                                           McgWpfTools.GetStringResource("MFG_WindowTitle"),
                                           System.Windows.MessageBoxButton.OK,
                                           System.Windows.MessageBoxImage.Warning);
        }

        private static void ShowInformation(string resourceKey)
        {
            System.Windows.MessageBox.Show(McgWpfTools.GetStringResource(resourceKey),
                                           McgWpfTools.GetStringResource("MFG_WindowTitle"),
                                           System.Windows.MessageBoxButton.OK,
                                           System.Windows.MessageBoxImage.Information);
        }

        private void SubscribeToPendingChange(ManufacturingComponentItem item)
        {
            item.PendingChangeEvent -= OnItemPendingChanged;
            item.PendingChangeEvent += OnItemPendingChanged;

            item.ManualDescriptionMthChangedEvent -= OnItemManualDescriptionMthChanged;
            item.ManualDescriptionMthChangedEvent += OnItemManualDescriptionMthChanged;
        }

        private void OnItemPendingChanged(object? sender, EventArgs e)
        {
            RefreshPendingChangesState();
        }

        /// <summary>
        /// Propage une modification manuelle de DESCRIPTION_MTH a tous les autres composants
        /// partageant le meme modele Creo (<see cref="ManufacturingComponentItem.ModelKey"/>),
        /// quel que soit leur niveau dans la nomenclature. Les composants sans identite de
        /// modele connue (ModelKey vide) ne sont jamais propages.
        /// </summary>
        private void OnItemManualDescriptionMthChanged(object? sender, string newValue)
        {
            if (sender is not ManufacturingComponentItem changedItem) return;
            if (string.IsNullOrEmpty(changedItem.ModelKey)) return;

            foreach (var otherItem in CurrentDataContext.ListItem)
            {
                if (ReferenceEquals(otherItem, changedItem)) continue;
                if (!string.Equals(otherItem.ModelKey, changedItem.ModelKey, StringComparison.Ordinal)) continue;
                if (string.Equals(otherItem.DescriptionMth, newValue, StringComparison.Ordinal)) continue;

                otherItem.ApplyPropagatedManualDescriptionMth(newValue);
            }
        }

        private void RefreshPendingChangesState()
        {
            CurrentDataContext.HasPendingChanges =
                CurrentDataContext.ListItem.Any(item => item.HasPendingChange);
        }

        private void ResetContext()
        {
            _activeModel = null;
            _modelsByKey.Clear();
            CurrentDataContext.ListItem.Clear();
            CurrentDataContext.ActiveModelName = string.Empty;
            CurrentDataContext.IsAssemblyLoaded = false;
            CurrentDataContext.NbModels = 0;
            CurrentDataContext.NbModelsInProgress = 0;
            CurrentDataContext.HasPendingChanges = false;
            CurrentDataContext.IsActiveModelModifiable = false;
            _isActiveModelModifiable = false;
            _activeModelStatus = CREOModelStatus.UNKNOWNERROR;
            _treeIndexCounter = 0;
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
