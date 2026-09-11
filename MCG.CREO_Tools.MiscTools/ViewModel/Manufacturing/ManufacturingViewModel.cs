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
        /// </summary>
        private void ExecuteUpdateParameters()
        {
            try
            {
                if (!EnsureActiveModelIsModifiable()) return;

                ShowInformation("MFG_MsgUpdateNotYetImplemented");
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        /// <summary>
        /// Commande "Creation du PVZ" : la commande, son activation et son raccordement au ruban
        /// sont en place. L'export sera realise dans une etape dediee.
        /// </summary>
        private void ExecuteCreatePvz()
        {
            try
            {
                if (!EnsureActiveModelIsModifiable()) return;

                ShowInformation("MFG_MsgCreatePvzNotYetImplemented");
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
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
