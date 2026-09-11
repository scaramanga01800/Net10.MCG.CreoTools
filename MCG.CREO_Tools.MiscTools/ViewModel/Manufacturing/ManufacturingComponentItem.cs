using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.Manufacturing;
using System;

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
    ///
    /// DESCRIPTION_MTH beneficie en plus d'un suivi dedie : valeur initiale trouvee dans Creo,
    /// derniere valeur calculee par <see cref="DescriptionMthCalculationService"/>, indicateur de
    /// modification manuelle et statut explicite (voir <see cref="DescriptionMthStatus"/>). Une
    /// modification manuelle reste prioritaire : elle n'est jamais ecrasee silencieusement par un
    /// recalcul (voir <see cref="ApplyCalculatedDescriptionMth"/>).
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

        private string _ModelKey = string.Empty;
        /// <summary>
        /// Identite du modele Creo reference par ce composant (voir
        /// <c>CreoSimpRepComponentInfo.ModelKey</c>). Sert a identifier les composants identiques
        /// afin de propager une modification manuelle de DESCRIPTION_MTH quel que soit le niveau
        /// de nomenclature.
        /// </summary>
        public string ModelKey
        {
            get { return _ModelKey; }
            set
            {
                if (this._ModelKey != value)
                {
                    this._ModelKey = value;
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
        private bool _SuppressManualFlagOnNextDescriptionMthSet;

        /// <summary>
        /// Parametre DESCRIPTION_MTH affiche dans la grille : modifiable manuellement. Toute
        /// saisie utilisateur (differente de la valeur actuellement affichee) marque la ligne
        /// comme modifiee manuellement (<see cref="IsManualDescriptionMth"/> = true), sauf lors du
        /// chargement initial depuis Creo (voir <see cref="LoadDescriptionMthFromCreo"/>) ou de
        /// l'application d'un calcul automatique (voir
        /// <see cref="ApplyCalculatedDescriptionMth"/>), qui n'affectent pas cet indicateur.
        ///
        /// Une saisie manuelle reelle de l'utilisateur declenche en outre
        /// <see cref="ManualDescriptionMthChangedEvent"/>, afin que le ViewModel puisse propager
        /// la meme valeur a tous les composants partageant le meme <see cref="ModelKey"/>.
        /// </summary>
        public string DescriptionMth
        {
            get { return _DescriptionMth; }
            set
            {
                if (this._DescriptionMth != value)
                {
                    this._DescriptionMth = value;
                    OnPropertyChanged();

                    if (_SuppressManualFlagOnNextDescriptionMthSet)
                    {
                        _SuppressManualFlagOnNextDescriptionMthSet = false;
                    }
                    else
                    {
                        IsManualDescriptionMth = true;
                        Status = DescriptionMthStatus.ManuallyModified;

                        if (!_SuppressManualPropagationOnNextDescriptionMthSet)
                        {
                            try
                            {
                                ManualDescriptionMthChangedEvent?.Invoke(this, value);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }

                    NotifyPendingChange();
                }
            }
        }

        private bool _SuppressManualPropagationOnNextDescriptionMthSet;

        /// <summary>
        /// Declenche lorsque l'utilisateur modifie manuellement DESCRIPTION_MTH sur cette ligne
        /// (saisie reelle, hors chargement initial et hors propagation programmatique).
        /// </summary>
        public event EventHandler<string>? ManualDescriptionMthChangedEvent;

        /// <summary>
        /// Applique une valeur DESCRIPTION_MTH provenant de la propagation d'une modification
        /// manuelle faite sur un autre composant identique (meme <see cref="ModelKey"/>). Marque
        /// la ligne comme modifiee manuellement, comme une saisie directe, mais sans redeclencher
        /// <see cref="ManualDescriptionMthChangedEvent"/> (evite toute boucle de propagation).
        /// </summary>
        public void ApplyPropagatedManualDescriptionMth(string value)
        {
            _SuppressManualPropagationOnNextDescriptionMthSet = true;
            DescriptionMth = value;
            _SuppressManualPropagationOnNextDescriptionMthSet = false;
        }

        /// <summary>
        /// Affecte DESCRIPTION_MTH telle que lue dans Creo, sans marquer la ligne comme modifiee
        /// manuellement. A utiliser uniquement lors de la construction de la ligne, avant tout
        /// calcul automatique.
        /// </summary>
        public void LoadDescriptionMthFromCreo(string value)
        {
            _SuppressManualFlagOnNextDescriptionMthSet = true;
            DescriptionMth = value;
            _SuppressManualFlagOnNextDescriptionMthSet = false;
            InitialDescriptionMth = value;
        }

        private string _InitialDescriptionMth = string.Empty;
        /// <summary>
        /// Valeur de DESCRIPTION_MTH telle que trouvee dans Creo lors de la derniere lecture reelle
        /// (avant tout calcul ou toute saisie manuelle). Reference immuable jusqu'a la prochaine
        /// lecture (<see cref="CaptureBaseline"/>).
        /// </summary>
        public string InitialDescriptionMth
        {
            get { return _InitialDescriptionMth; }
            set
            {
                if (this._InitialDescriptionMth != value)
                {
                    this._InitialDescriptionMth = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _CalculatedDescriptionMth;
        /// <summary>
        /// Derniere valeur produite par <see cref="DescriptionMthCalculationService"/> pour cette
        /// ligne, independamment de ce qui est effectivement affiche (null si aucune regle n'a pu
        /// produire de valeur exploitable).
        /// </summary>
        public string? CalculatedDescriptionMth
        {
            get { return _CalculatedDescriptionMth; }
            set
            {
                if (this._CalculatedDescriptionMth != value)
                {
                    this._CalculatedDescriptionMth = value;
                    OnPropertyChanged();
                }
            }
        }

        private DescriptionMthRule _CalculatedDescriptionMthRule = DescriptionMthRule.None;
        /// <summary>Regle ayant produit <see cref="CalculatedDescriptionMth"/>.</summary>
        public DescriptionMthRule CalculatedDescriptionMthRule
        {
            get { return _CalculatedDescriptionMthRule; }
            set
            {
                if (this._CalculatedDescriptionMthRule != value)
                {
                    this._CalculatedDescriptionMthRule = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsManualDescriptionMth;
        /// <summary>
        /// Vrai si la valeur actuellement affichee dans <see cref="DescriptionMth"/> provient d'une
        /// saisie manuelle de l'utilisateur (et non du dernier calcul automatique). Une valeur
        /// manuelle est prioritaire : elle ne doit jamais etre ecrasee silencieusement par un
        /// recalcul (voir <see cref="ApplyCalculatedDescriptionMth"/>).
        /// </summary>
        public bool IsManualDescriptionMth
        {
            get { return _IsManualDescriptionMth; }
            set
            {
                if (this._IsManualDescriptionMth != value)
                {
                    this._IsManualDescriptionMth = value;
                    OnPropertyChanged();
                }
            }
        }

        private DescriptionMthStatus _Status = DescriptionMthStatus.Unchanged;
        /// <summary>Statut explicite de la ligne vis-a-vis du calcul/de la saisie de DESCRIPTION_MTH.</summary>
        public DescriptionMthStatus Status
        {
            get { return _Status; }
            set
            {
                if (this._Status != value)
                {
                    this._Status = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Applique un resultat de calcul automatique a la ligne, en respectant la priorite des
        /// modifications manuelles : si une valeur manuelle est deja presente et differente du
        /// nouveau calcul, la valeur affichee n'est PAS ecrasee ; la ligne est seulement marquee en
        /// <see cref="DescriptionMthStatus.UpdateRequired"/> pour que l'appelant puisse demander
        /// confirmation avant, eventuellement, un remplacement explicite via
        /// <see cref="AcceptCalculatedDescriptionMth"/>.
        /// </summary>
        public void ApplyCalculatedDescriptionMth(DescriptionMthCalculationResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            CalculatedDescriptionMthRule = result.Rule;
            CalculatedDescriptionMth = result.Value;

            if (result.IsWebtermError)
            {
                Status = DescriptionMthStatus.WebtermError;
                return;
            }

            if (IsManualDescriptionMth
                && !string.IsNullOrEmpty(_DescriptionMth)
                && result.Value != null
                && !string.Equals(_DescriptionMth, result.Value, StringComparison.Ordinal))
            {
                // Une valeur manuelle existe et differe du recalcul : elle reste affichee tant
                // qu'aucune confirmation explicite n'a ete donnee par l'utilisateur.
                Status = DescriptionMthStatus.UpdateRequired;
                return;
            }

            if (result.Value == null)
            {
                // Regle de secours : aucune regle n'a produit de resultat. La valeur existante
                // (manuelle ou deja calculee) n'est jamais remplacee automatiquement par du vide.
                if (!IsManualDescriptionMth && string.IsNullOrEmpty(_DescriptionMth))
                    Status = DescriptionMthStatus.CalculationImpossible;

                return;
            }

            if (!IsManualDescriptionMth)
            {
                SetDescriptionMthFromCalculation(result.Value);
            }
        }

        /// <summary>
        /// Remplace explicitement la valeur affichee par le dernier resultat calcule, apres
        /// confirmation de l'utilisateur suite a un changement de donnee source
        /// (<see cref="DescriptionMthStatus.UpdateRequired"/>).
        /// </summary>
        public void AcceptCalculatedDescriptionMth()
        {
            if (CalculatedDescriptionMth == null) return;

            SetDescriptionMthFromCalculation(CalculatedDescriptionMth);
        }

        /// <summary>
        /// Affecte la valeur affichee de DESCRIPTION_MTH suite a un calcul automatique (accepte ou
        /// initial), sans marquer la ligne comme modifiee manuellement.
        /// </summary>
        private void SetDescriptionMthFromCalculation(string value)
        {
            if (!string.Equals(_DescriptionMth, value, StringComparison.Ordinal))
            {
                _DescriptionMth = value;
                OnPropertyChanged(nameof(DescriptionMth));
                NotifyPendingChange();
            }

            IsManualDescriptionMth = false;
            Status = DescriptionMthStatus.Calculated;
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
        /// A appeler juste apres chaque lecture reelle depuis Creo (et apres application du
        /// calcul automatique). Ne modifie jamais <see cref="Status"/> : le statut refletant le
        /// resultat du calcul (ou la saisie manuelle) reste inchange par cette capture.
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
