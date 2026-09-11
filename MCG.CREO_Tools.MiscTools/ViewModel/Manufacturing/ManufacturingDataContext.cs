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
