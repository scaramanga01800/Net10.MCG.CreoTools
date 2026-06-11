using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.JpgExport.View;

namespace MCG.CREO_Tools.JpgExport.ViewModel
{
    public class JpgExportComboBoxValue : ObservableObject, IJpgExportComboBoxValue
    {
        private string _Value;
        public string Value
        {
            get { return this._Value; }
            set
            {
                if (this._Value != value)
                {
                    this._Value = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ValueShown;
        public string ValueShown
        {
            get { return this._ValueShown; }
            set
            {
                if (this._ValueShown != value)
                {
                    this._ValueShown = value;
                    OnPropertyChanged();
                }
            }
        }

        public override string ToString()
        {
            return ValueShown;
        }
    }
}
