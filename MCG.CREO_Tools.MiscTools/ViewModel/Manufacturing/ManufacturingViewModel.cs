using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.Manufacturing;
using pfcls;
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
                                       ICreoParameterService creoParameterService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;
                _creoSimpRepService = creoSimpRepService;
                _creoParameterService = creoParameterService;

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
                ReadComponentsRecursive(_activeModel, level: 0, parentNumber: string.Empty);

                CurrentDataContext.IsAssemblyLoaded = true;

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
        /// Parcourt recursivement les composants de premier niveau de <paramref name="assemblyModel"/>
        /// et, pour chaque sous-assemblage, reapplique le meme parcours sur son propre modele.
        /// Compose exclusivement des methodes deja verifiees de <see cref="ICreoSimpRepService"/>
        /// (ListTopLevelComponents, GetComponentModel) : aucune API de parcours recursif n'est
        /// invoquee, la recursion est geree cote ViewModel.
        /// </summary>
        private void ReadComponentsRecursive(IpfcModel assemblyModel, int level, string parentNumber)
        {
            // Garde-fou contre une boucle de reference anormale dans la structure Creo.
            if (level >= MiscToolsConstants.MaxBomLevel) return;

            var components = _creoSimpRepService.ListTopLevelComponents(assemblyModel);

            int localIndex = 0;

            foreach (var component in components)
            {
                localIndex++;
                _treeIndexCounter++;

                var hierarchicalNumber = string.IsNullOrEmpty(parentNumber)
                    ? localIndex.ToString()
                    : $"{parentNumber}.{localIndex}";

                var componentModel = component.ComponentFeature != null
                    ? _creoSimpRepService.GetComponentModel(component.ComponentFeature)
                    : null;

                var item = new ManufacturingComponentItem
                {
                    TreeIndex = _treeIndexCounter,
                    Level = level,
                    HierarchicalNumber = hierarchicalNumber,
                    ComponentId = component.Id,
                    Name = component.Name,
                    Reference = componentModel != null ? GetModelParameter(componentModel, "REFERENCE") : string.Empty,
                    PtcCommonName = componentModel != null ? GetModelParameter(componentModel, "PTC_COMMON_NAME") : string.Empty,
                    Description2 = componentModel != null ? GetModelParameter(componentModel, "DESCRIPTION_2") : string.Empty,
                    Description2_1 = componentModel != null ? GetModelParameter(componentModel, "DESCRIPTION2_1") : string.Empty,
                    Description2_2 = componentModel != null ? GetModelParameter(componentModel, "DESCRIPTION2_2") : string.Empty,
                    DescriptionMth = componentModel != null ? GetModelParameter(componentModel, "DESCRIPTION_MTH") : string.Empty,
                };

                // L'etat lu dans Creo devient la reference : tant qu'aucune saisie manuelle ne
                // s'en ecarte, la ligne n'est pas consideree comme modifiee.
                item.CaptureBaseline();

                MainDispatcher.Invoke(() =>
                {
                    SubscribeToPendingChange(item);
                    CurrentDataContext.ListItem.Add(item);
                    CurrentDataContext.NbModels++;
                    CurrentDataContext.NbModelsInProgress++;
                });

                // Un sous-assemblage est identifie par l'extension .ASM portee par l'identite du
                // modele Creo reference (CreoSimpRepComponentInfo.ModelKey), au meme titre que le
                // controle deja effectue sur le nom du modele actif lors de la lecture initiale.
                if (componentModel != null && IsAssemblyModelKey(component.ModelKey))
                {
                    ReadComponentsRecursive(componentModel, level + 1, hierarchicalNumber);
                }
            }
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
        }

        private void OnItemPendingChanged(object? sender, EventArgs e)
        {
            RefreshPendingChangesState();
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
