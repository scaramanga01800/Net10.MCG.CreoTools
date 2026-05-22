using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.CadAutoColor;
using pfcls;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColor
{
    public class CadAutoColorItem: ObservableObject, ICadAutoColorItem
    {
        #region [REGION] Properties from Interface
        private string _Material = "Unknown";
        public string Material
        {
            get { return _Material; }
            set
            {
                if (this._Material != value)
                {
                    this._Material = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Ptc_Common_Name;
        public string Ptc_Common_Name
        {
            get { return _Ptc_Common_Name; }
            set
            {
                if (this._Ptc_Common_Name != value)
                {
                    this._Ptc_Common_Name = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Number;
        public string Number
        {
            get { return _Number; }
            set
            {
                if (this._Number != value)
                {
                    this._Number = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _AsssignedMaterial;
        public string AsssignedMaterial
        {
            get { return _AsssignedMaterial; }
            set
            {
                if (this._AsssignedMaterial != value)
                {
                    this._AsssignedMaterial = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsSelected = false;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (this._IsSelected != value)
                {
                    this._IsSelected = value;
                    OnPropertyChanged();
                    RaiseIsSelectedEvent();
                }

            }
        }

        private CadAutoColorCreoColor _SelectedCreoColor;
        public CadAutoColorCreoColor SelectedCreoColor
        {
            get { return _SelectedCreoColor; }
            set
            {
                if (this._SelectedCreoColor != value)
                {
                    this._SelectedCreoColor = value;
                    OnPropertyChanged();
                    IsColorAssigned = false;
                }

            }
        }

        public ObservableCollection<string> ListCadDoc { get; set; } = new ObservableCollection<string>();
        #endregion

        #region [REGION] Internal variables
        public List<IpfcModel> CadModels { get; set; }

        public bool IsColorAssigned { get; set; } = false;
        #endregion

        #region [REGION] Events
        /// <summary>
        /// Occurs when [is selected event].
        /// </summary>
        public event EventHandler IsSelectedEvent;

        /// <summary>
        /// Raises the saved searches list event.
        /// </summary>
        public void RaiseIsSelectedEvent()
        {
            try
            {
                IsSelectedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion


    }
}
