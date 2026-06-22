using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
    public class QuickSearchPartSubClassParam : ObservableObject
    {
        private Partsubclassparam _CurrentPartSubClassParam;
        public Partsubclassparam CurrentPartSubClassParam
        {
            get { return this._CurrentPartSubClassParam; }
            set
            {
                if (this._CurrentPartSubClassParam != value)
                {
                    this._CurrentPartSubClassParam = value;
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

        public string IdParam { get { return CurrentPartSubClassParam.Idparamtable; } }

        public string FilterValue { get; set; } = "";
    }
}
