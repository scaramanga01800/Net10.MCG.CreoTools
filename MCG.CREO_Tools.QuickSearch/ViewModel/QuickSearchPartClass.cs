
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
    public class QuickSearchPartClass : ObservableObject
    {
        private Partclass _CurrentPartClass;
        public Partclass CurrentPartClass
        {
            get { return this._CurrentPartClass; }
            set
            {
                if (this._CurrentPartClass != value)
                {
                    this._CurrentPartClass = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Name;
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
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
