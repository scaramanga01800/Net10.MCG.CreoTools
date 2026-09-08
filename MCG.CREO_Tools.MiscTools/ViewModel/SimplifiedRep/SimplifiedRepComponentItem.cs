using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.CreoInteractionTools.Models;
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
                    OnPropertyChanged();
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

        private string _CurrentAction = string.Empty;
        public string CurrentAction
        {
            get { return _CurrentAction; }
            set
            {
                if (this._CurrentAction != value)
                {
                    this._CurrentAction = value;
                    OnPropertyChanged();
                }
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
                }
            }
        }

        public ObservableCollection<string> ListComponentSimpRep { get; set; } = new ObservableCollection<string>();
        #endregion

        #region [REGION] Internal variables
        public CreoSimpRepComponentInfo ComponentInfo { get; set; }
        #endregion
    }
}
