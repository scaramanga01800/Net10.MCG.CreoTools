using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.QuickSearch.View;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
    public class QuickSearchShortCutViewModel : ObservableObject, IQuickSearchShortCutViewModel
    {
        private string _Class = "class";
        public string Class
        {
            get { return this._Class; }
            set
            {
                if (this._Class != value)
                {
                    this._Class = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _SubClass = "SubClass";
        public string SubClass
        {
            get { return this._SubClass; }
            set
            {
                if (this._SubClass != value)
                {
                    this._SubClass = value;
                    OnPropertyChanged();
                }
            }
        }

        public QuickSearchViewModel MainApp { get; set; }

        public int Order { get; set; }

        public QuickSearchShortCutData ParentData { get; set; }

        public override string ToString()
        {
            return $"{Class}/{SubClass}";
        }
    }
}
