using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.CadAutoColor;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColor
{
    public class CadAutoColorPalette: ObservableObject, ICadAutoColorPalette
    {
        #region [REGION] Properties from Interface
        private string _Name;
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

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (this._IsSelected != value)
                {
                    this._IsSelected = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<CadAutoColorCreoColor> ListColor { get; set; } = new ObservableCollection<CadAutoColorCreoColor>();

        public event EventHandler SelectedEvent;
        public void RaiseSelectedEvent()
        {
            try
            {
                SelectedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region [REGION] Internal variables
        public string ColorPaletteFile { get; set; }
        #endregion
    }
}
