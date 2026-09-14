using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.Manufacturing;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing
{
    /// <summary>Etat UI de la fenetre Manufacturing View.</summary>
    public class ManufacturingDataContext : ObservableObject, IManufacturingDataContext
    {
        #region [REGION] Properties from Interface
        private bool _IsCreoConnected;
        public bool IsCreoConnected
        {
            get { return _IsCreoConnected; }
            set
            {
                if (this._IsCreoConnected != value)
                {
                    this._IsCreoConnected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsPleaseWaitShown;
        public bool IsPleaseWaitShown
        {
            get { return _IsPleaseWaitShown; }
            set
            {
                if (this._IsPleaseWaitShown != value)
                {
                    this._IsPleaseWaitShown = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        private bool _IsAssemblyLoaded;
        public bool IsAssemblyLoaded
        {
            get { return _IsAssemblyLoaded; }
            set
            {
                if (this._IsAssemblyLoaded != value)
                {
                    this._IsAssemblyLoaded = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsActiveModelModifiable;
        /// <summary>
        /// Vrai lorsque l'assemblage actif est modifiable en session (extrait, nouveau en
        /// session ou modifie localement). Sinon Creo refuse toute ecriture de parametre ou
        /// toute sauvegarde et les actions correspondantes du ruban restent desactivees.
        /// </summary>
        public bool IsActiveModelModifiable
        {
            get { return _IsActiveModelModifiable; }
            set
            {
                if (this._IsActiveModelModifiable != value)
                {
                    this._IsActiveModelModifiable = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _HasPendingChanges;
        /// <summary>Vrai des qu'au moins une ligne de la grille porte une modification non sauvegardee.</summary>
        public bool HasPendingChanges
        {
            get { return _HasPendingChanges; }
            set
            {
                if (this._HasPendingChanges != value)
                {
                    this._HasPendingChanges = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsUpdateRunning;
        /// <summary>
        /// Vrai pendant l'execution de l'action "Mise a jour" (copie locale, vidage de session,
        /// reouverture, ecriture des parametres, sauvegardes). Empeche un double lancement de la
        /// commande et desactive les commandes incompatibles (lecture, sauvegarde, PVZ) le temps
        /// de l'operation.
        /// </summary>
        public bool IsUpdateRunning
        {
            get { return _IsUpdateRunning; }
            set
            {
                if (this._IsUpdateRunning != value)
                {
                    this._IsUpdateRunning = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        /// <summary>
        /// Vrai lorsque la fenetre est occupee par une operation en cours (lecture, sauvegarde ou
        /// mise a jour) : combine <see cref="IsPleaseWaitShown"/> et <see cref="IsUpdateRunning"/>
        /// afin de desactiver les commandes incompatibles pendant toute operation longue.
        /// </summary>
        public bool IsBusy => _IsPleaseWaitShown || _IsUpdateRunning || _IsPvzRunning;

        private bool _IsPvzRunning;
        /// <summary>
        /// Vrai pendant l'execution de l'action "Creation du PVZ" (copie locale, export
        /// ProductView). Empeche un double lancement de la commande et desactive les commandes
        /// incompatibles (lecture, sauvegarde, mise a jour) le temps de l'operation.
        /// </summary>
        public bool IsPvzRunning
        {
            get { return _IsPvzRunning; }
            set
            {
                if (this._IsPvzRunning != value)
                {
                    this._IsPvzRunning = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        private string _ActiveModelName = string.Empty;
        public string ActiveModelName
        {
            get { return _ActiveModelName; }
            set
            {
                if (this._ActiveModelName != value)
                {
                    this._ActiveModelName = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbModels;
        public int NbModels
        {
            get { return _NbModels; }
            set
            {
                if (this._NbModels != value)
                {
                    this._NbModels = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _NbModelsInProgress;
        public int NbModelsInProgress
        {
            get { return _NbModelsInProgress; }
            set
            {
                if (this._NbModelsInProgress != value)
                {
                    this._NbModelsInProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<ManufacturingComponentItem> _ListItem = new ObservableCollection<ManufacturingComponentItem>();
        /// <summary>Composants de la nomenclature, a plat, dans l'ordre hierarchique de l'arbre.</summary>
        public ObservableCollection<ManufacturingComponentItem> ListItem
        {
            get { return _ListItem; }
            set
            {
                if (this._ListItem != value)
                {
                    this._ListItem = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion
    }
}
