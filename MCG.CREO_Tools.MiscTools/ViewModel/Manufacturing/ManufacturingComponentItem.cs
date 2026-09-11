using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.Manufacturing;

namespace MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing
{
    /// <summary>
    /// Ligne de la grille Manufacturing View : un composant de la nomenclature de l'assemblage
    /// actif, avec son niveau de profondeur.
    ///
    /// REFERENCE et DESCRIPTION_MTH sont modifiables manuellement par l'utilisateur ; les autres
    /// colonnes proviennent des parametres Creo du composant et restent en lecture seule.
    /// La comparaison de modification se fait contre l'etat lu lors de la derniere lecture Creo
    /// (baseline), pas uniquement contre l'etat courant du controle : un retour manuel a la valeur
    /// d'origine supprime donc la surbrillance si aucune autre donnee de la ligne n'a change.
    /// </summary>
    public class ManufacturingComponentItem : ObservableObject, IManufacturingComponentItem
    {
        #region [REGION] Properties from Interface
        private int _TreeIndex;
        public int TreeIndex
        {
            get { return _TreeIndex; }
            set
            {
                if (this._TreeIndex != value)
                {
                    this._TreeIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _Level;
        /// <summary>Profondeur du composant dans la nomenclature (0 = premier niveau).</summary>
        public int Level
        {
            get { return _Level; }
            set
            {
                if (this._Level != value)
                {
                    this._Level = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _HierarchicalNumber = string.Empty;
        /// <summary>Numerotation hierarchique du composant (ex : 1, 1.1, 1.1.2).</summary>
        public string HierarchicalNumber
        {
            get { return _HierarchicalNumber; }
            set
            {
                if (this._HierarchicalNumber != value)
                {
                    this._HierarchicalNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _ComponentId;
        public int ComponentId
        {
            get { return _ComponentId; }
            set
            {
                if (this._ComponentId != value)
                {
                    this._ComponentId = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Name = string.Empty;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Reference = string.Empty;
        /// <summary>Parametre REFERENCE : modifiable manuellement dans la grille.</summary>
        public string Reference
        {
            get { return _Reference; }
            set
            {
                if (this._Reference != value)
                {
                    this._Reference = value;
                    OnPropertyChanged();
                    NotifyPendingChange();
                }
            }
        }

        private string _PtcCommonName = string.Empty;
        /// <summary>Parametre PTC_COMMON_NAME lu sur le modele du composant (lecture seule).</summary>
        public string PtcCommonName
        {
            get { return _PtcCommonName; }
            set
            {
                if (this._PtcCommonName != value)
                {
                    this._PtcCommonName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Description2 = string.Empty;
        /// <summary>Parametre DESCRIPTION_2 lu sur le modele du composant (lecture seule).</summary>
        public string Description2
        {
            get { return _Description2; }
            set
            {
                if (this._Description2 != value)
                {
                    this._Description2 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Description2_1 = string.Empty;
        /// <summary>Parametre DESCRIPTION2_1 lu sur le modele du composant (lecture seule).</summary>
        public string Description2_1
        {
            get { return _Description2_1; }
            set
            {
                if (this._Description2_1 != value)
                {
                    this._Description2_1 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Description2_2 = string.Empty;
        /// <summary>Parametre DESCRIPTION2_2 lu sur le modele du composant (lecture seule).</summary>
        public string Description2_2
        {
            get { return _Description2_2; }
            set
            {
                if (this._Description2_2 != value)
                {
                    this._Description2_2 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _DescriptionMth = string.Empty;
        /// <summary>Parametre DESCRIPTION_MTH : modifiable manuellement dans la grille.</summary>
        public string DescriptionMth
        {
            get { return _DescriptionMth; }
            set
            {
                if (this._DescriptionMth != value)
                {
                    this._DescriptionMth = value;
                    OnPropertyChanged();
                    NotifyPendingChange();
                }
            }
        }

        private bool _IsDuplicateModel;
        /// <summary>
        /// Vrai si le modele reference par ce composant a deja ete rencontre ailleurs dans la
        /// nomenclature (a un autre endroit de l'arbre, hors ascendance directe). Purement
        /// informatif : n'empeche pas l'expansion du composant.
        /// </summary>
        public bool IsDuplicateModel
        {
            get { return _IsDuplicateModel; }
            set
            {
                if (this._IsDuplicateModel != value)
                {
                    this._IsDuplicateModel = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsCycleDetected;
        /// <summary>
        /// Vrai si ce composant a ete ecarte de l'expansion car son modele figure deja parmi ses
        /// propres ancetres (cycle reel dans la structure de l'assemblage).
        /// </summary>
        public bool IsCycleDetected
        {
            get { return _IsCycleDetected; }
            set
            {
                if (this._IsCycleDetected != value)
                {
                    this._IsCycleDetected = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region [REGION] Baseline / Modification tracking
        private string _BaselineReference = string.Empty;
        private string _BaselineDescriptionMth = string.Empty;

        /// <summary>
        /// Memorise l'etat courant de REFERENCE et DESCRIPTION_MTH comme etat de reference.
        /// A appeler juste apres chaque lecture reelle depuis Creo.
        /// </summary>
        public void CaptureBaseline()
        {
            _BaselineReference = _Reference;
            _BaselineDescriptionMth = _DescriptionMth;

            // L'etat de reference vient d'etre recale : la ligne n'est plus consideree
            // comme modifiee et le surlignage doit disparaitre.
            NotifyPendingChange();
        }

        /// <summary>
        /// Indique si la ligne a ete modifiee par l'utilisateur depuis la derniere lecture Creo.
        /// La comparaison se fait contre la baseline, pas uniquement contre l'etat courant :
        /// un retour manuel a la valeur d'origine supprime donc la surbrillance si aucune autre
        /// donnee de la ligne n'a change.
        /// </summary>
        public bool HasPendingChange
        {
            get
            {
                return !string.Equals(_Reference, _BaselineReference, StringComparison.Ordinal)
                    || !string.Equals(_DescriptionMth, _BaselineDescriptionMth, StringComparison.Ordinal);
            }
        }

        /// <summary>Expose <see cref="HasPendingChange"/> a la vue : la ligne est surlignee en jaune pale.</summary>
        public bool IsModified => HasPendingChange;

        private void NotifyPendingChange()
        {
            OnPropertyChanged(nameof(IsModified));
            OnPropertyChanged(nameof(HasPendingChange));

            try
            {
                PendingChangeEvent?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>Declenche des que l'etat "modifie" de la ligne est susceptible d'avoir change.</summary>
        public event EventHandler? PendingChangeEvent;
        #endregion
    }
}
