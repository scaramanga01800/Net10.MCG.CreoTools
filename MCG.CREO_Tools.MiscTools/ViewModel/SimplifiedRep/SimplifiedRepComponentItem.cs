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

        private string _Type = string.Empty;
        /// <summary>Parametre TYPE lu sur le modele du composant.</summary>
        public string Type
        {
            get { return _Type; }
            set
            {
                if (this._Type != value)
                {
                    this._Type = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _SubType = string.Empty;
        /// <summary>Parametre SUB_TYPE lu sur le modele du composant.</summary>
        public string SubType
        {
            get { return _SubType; }
            set
            {
                if (this._SubType != value)
                {
                    this._SubType = value;
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

                    // Cocher ou decocher la case annule la substitution : la combo revient
                    // sur la representation maitresse et l'action redevient "Inclure"/"Exclure".
                    var masterLabel = ListComponentSimpRep.Count > 0 ? MasterRepLabel : string.Empty;

                    if (!string.Equals(_SelectedComponentSimpRep, masterLabel, StringComparison.OrdinalIgnoreCase))
                    {
                        this._SelectedComponentSimpRep = masterLabel;
                        OnPropertyChanged(nameof(SelectedComponentSimpRep));
                    }

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentAction));
                    NotifyPendingChange();
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
                if (!string.IsNullOrWhiteSpace(EffectiveSubstitution))
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
                    NotifyPendingChange();
                }
            }
        }

        public ObservableCollection<string> ListComponentSimpRep { get; set; } = new ObservableCollection<string>();
        #endregion

        #region [REGION] Master representation
        /// <summary>
        /// Libelle localise de la representation maitresse, presente en tete de chaque liste.
        /// La selectionner signifie "aucune substitution" : le composant reste utilise
        /// dans sa representation complete.
        /// </summary>
        public static string MasterRepLabel => McgWpfTools.GetStringResource("SRP_MasterRep");

        /// <summary>
        /// Nom de la representation reellement substituee, ou chaine vide si la ligne
        /// est sur la representation maitresse.
        /// </summary>
        public string EffectiveSubstitution
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_SelectedComponentSimpRep)) return string.Empty;

                return string.Equals(_SelectedComponentSimpRep,
                                     MasterRepLabel,
                                     StringComparison.OrdinalIgnoreCase)
                    ? string.Empty
                    : _SelectedComponentSimpRep;
            }
        }
        #endregion

        #region [REGION] Internal variables
        public CreoSimpRepComponentInfo ComponentInfo { get; set; }

        /// <summary>
        /// Identite du modele Creo (.PRT / .ASM) reference par la ligne.
        /// Elle provient du descripteur de modele Creo (GetFullName + extension) et ne
        /// depend ni du chemin d'assemblage, ni du numero d'occurrence, ni de la position
        /// dans l'arbre. Deux lignes partageant cette valeur pointent sur le meme modele.
        ///
        /// Si le descripteur n'a pas pu etre exploite, le nom du composant sert de repli
        /// afin que la comparaison entre occurrences reste toujours possible.
        /// </summary>
        public string ModelKey
        {
            get
            {
                var key = ComponentInfo?.ModelKey ?? string.Empty;

                return string.IsNullOrWhiteSpace(key)
                    ? (Name ?? string.Empty).Trim().ToUpperInvariant()
                    : key;
            }
        }

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
            _BaselineSubstitutedSimpRepName = EffectiveSubstitution;

            // L'etat de reference vient d'etre recale : la ligne n'est plus consideree
            // comme modifiee et le surlignage doit disparaitre.
            NotifyPendingChange();
        }

        /// <summary>
        /// Indique si la ligne a ete modifiee par l'utilisateur depuis la derniere lecture Creo.
        /// Permet de n'envoyer a Creo que les composants reellement impactes.
        /// </summary>
        public bool HasPendingChange
        {
            get
            {
                var currentSubstitution = EffectiveSubstitution;

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

        /// <summary>
        /// Expose <see cref="HasPendingChange"/> a la vue : la ligne est surlignee
        /// tant que la modification n'est pas sauvegardee ou abandonnee.
        /// </summary>
        public bool IsModified => HasPendingChange;

        /// <summary>
        /// Signale a la vue et au view model que l'etat "modifie" de la ligne a pu changer.
        /// </summary>
        private void NotifyPendingChange()
        {
            OnPropertyChanged(nameof(IsModified));
            OnPropertyChanged(nameof(HasPendingChange));

            try
            {
                PendingChangeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Events
        /// <summary>
        /// Declenche a chaque changement de la case "Inclus".
        /// Permet a la vue de gerer la multiselection avec la touche MAJ.
        /// </summary>
        public event EventHandler? IsIncludedEvent;

        /// <summary>
        /// Declenche des que l'etat "modifie" de la ligne est susceptible d'avoir change.
        /// Permet au view model de recalculer l'indicateur global de modifications en attente.
        /// </summary>
        public event EventHandler? PendingChangeEvent;

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
