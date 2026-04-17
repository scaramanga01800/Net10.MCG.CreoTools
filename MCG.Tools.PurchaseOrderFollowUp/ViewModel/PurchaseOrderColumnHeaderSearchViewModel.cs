using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.WpfComponent.ViewModel;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderColumnHeaderSearchViewModel:ObservableObject
    {
        private string _AttributeName = string.Empty;
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

        private object _DataContextCommand = default!;
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

        private McgColumnData _CurrentCommandParameter = new McgColumnData();
        public McgColumnData CurrentCommandParameter
        {
            get { return _CurrentCommandParameter; }
            set
            {
                if (this._CurrentCommandParameter != value)
                {
                    this._CurrentCommandParameter = value;
                    OnPropertyChanged();
                }

            }
        }
    }
}
