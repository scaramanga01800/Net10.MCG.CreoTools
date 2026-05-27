using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.SapBomExport;

namespace MCG.CREO_Tools.MiscTools.ViewModel.SapBomExport
{
    public class SapBomExportApplicationItem:ObservableObject, ISapBomExportApplicationItem
    {
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

        private string _Description;
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

        public override string ToString()
        {
            return Name;
        }
    }
}
