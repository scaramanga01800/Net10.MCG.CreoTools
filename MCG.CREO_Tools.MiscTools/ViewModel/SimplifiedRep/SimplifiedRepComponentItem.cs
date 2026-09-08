using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.View.SimplifiedRep;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.SimplifiedRep
{
    public class SimplifiedRepComponentItem : ObservableObject, ISimplifiedRepComponentItem
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

        private string _Rep = string.Empty;
        /// <summary>Parametre REP : relation entre l'assemblage et le composant.</summary>
        public string Rep
        {
            get { return _Rep; }
            set
            {
                if (this._Rep != value)
                {
                    this._Rep = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Description = string.Empty;
        /// <summary>Concatenation de PTC_COMMON_NAME et DESCRIPTION_2 separes par "|".</summary>
        public string Description
        {
            get { return _Description; }
            set
            {
                if (this._Description != value)
                {
                    this._Description = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsIncluded = true;
        public bool IsIncluded
        {
            get { return _IsIncluded; }
            set
            {
                if (this._IsIncluded != value)
                {
                    this._IsIncluded = value;

                    // Cocher ou decocher la case annule la substitution :
                    // l'action redevient "Inclure" ou "Exclure".
                    if (!string.IsNullOrWhiteSpace(_SelectedComponentSimpRep))
                    {
                        this._SelectedComponentSimpRep = string.Empty;
                        OnPropertyChanged(nameof(SelectedComponentSimpRep));
                    }

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentAction));
                    RaiseIsIncludedEvent();
                }
            }
        }

        private bool _IsExplicit;
        public bool IsExplicit
        {
            get { return _IsExplicit; }
            set
            {
                if (this._IsExplicit != value)
                {
                    this._IsExplicit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Action qui sera reellement appliquee lors de la mise a jour de la representation :
        /// - une representation simplifiee choisie pour le composant est prioritaire => "Remplacer"
        /// - sinon la case "Inclus" determine "Inclure" ou "Exclure".
        /// </summary>
        public string CurrentAction
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_SelectedComponentSimpRep))
                    return McgWpfTools.GetStringResource("SRP_Action_Substitute");

                return _IsIncluded
                    ? McgWpfTools.GetStringResource("SRP_Action_Include")
                    : McgWpfTools.GetStringResource("SRP_Action_Exclude");
            }
        }

        private string _SelectedComponentSimpRep = string.Empty;
        public string SelectedComponentSimpRep
        {
            get { return _SelectedComponentSimpRep; }
            set
            {
                if (this._SelectedComponentSimpRep != value)
                {
                    this._SelectedComponentSimpRep = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentAction));
                }
            }
        }

        public ObservableCollection<string> ListComponentSimpRep { get; set; } = new ObservableCollection<string>();
        #endregion

        #region [REGION] Internal variables
        public CreoSimpRepComponentInfo ComponentInfo { get; set; }

        /// <summary>Etat d'inclusion tel que lu dans Creo lors du dernier chargement.</summary>
        private bool _BaselineIsIncluded = true;

        /// <summary>Substitution telle que lue dans Creo lors du dernier chargement.</summary>
        private string _BaselineSubstitutedSimpRepName = string.Empty;

        /// <summary>
        /// Memorise l'etat courant comme etat de reference.
        /// A appeler apres chaque lecture reelle depuis Creo.
        /// </summary>
        public void CaptureBaseline()
        {
            _BaselineIsIncluded = _IsIncluded;
            _BaselineSubstitutedSimpRepName = _SelectedComponentSimpRep ?? string.Empty;
        }

        /// <summary>
        /// Indique si la ligne a ete modifiee par l'utilisateur depuis la derniere lecture Creo.
        /// Permet de n'envoyer a Creo que les composants reellement impactes.
        /// </summary>
        public bool HasPendingChange
        {
            get
            {
                var currentSubstitution = _SelectedComponentSimpRep ?? string.Empty;

                if (!string.Equals(currentSubstitution,
                                   _BaselineSubstitutedSimpRepName,
                                   StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Tant qu'une substitution est active, la case "Inclus" n'a pas d'effet.
                if (!string.IsNullOrWhiteSpace(currentSubstitution)) return false;

                return _IsIncluded != _BaselineIsIncluded;
            }
        }
        #endregion

        #region [REGION] Events
        /// <summary>
        /// Declenche a chaque changement de la case "Inclus".
        /// Permet a la vue de gerer la multiselection avec la touche MAJ.
        /// </summary>
        public event EventHandler? IsIncludedEvent;

        /// <summary>Notifie la vue qu'une case a ete cochee ou decochee.</summary>
        public void RaiseIsIncludedEvent()
        {
            try
            {
                IsIncludedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}
