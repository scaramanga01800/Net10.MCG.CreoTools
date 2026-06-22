using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.QuickSearch.View;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
    public class QuickSearchColumnHeaderSearchViewModel: ObservableObject, IQuickSearchColumnHeaderSearchViewModel
    {
        private string _AttributeName;
        public string AttributeName
        {
            get { return this._AttributeName; }
            set
            {
                if (this._AttributeName != value)
                {
                    this._AttributeName = value;
                    OnPropertyChanged();
                }
            }
        }

        private object _DataContextCommand;
        public object DataContextCommand
        {
            get { return this._DataContextCommand; }
            set
            {
                if (this._DataContextCommand != value)
                {
                    this._DataContextCommand = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _MinWidth = 60;
        public int MinWidth
        {
            get { return this._MinWidth; }
            set
            {
                if (this._MinWidth != value)
                {
                    this._MinWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        private QuickSearchPartSubClassParam _RefObject;
        public QuickSearchPartSubClassParam RefObject
        {
            get { return this._RefObject; }
            set
            {
                if (this._RefObject != value)
                {
                    this._RefObject = value;
                    OnPropertyChanged();
                }
            }
        }

    }
}
