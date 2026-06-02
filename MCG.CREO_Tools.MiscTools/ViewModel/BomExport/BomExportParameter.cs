using CommunityToolkit.Mvvm.ComponentModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.BomExport
{
    public class BomExportParameter: ObservableObject
    {
        private string _ParamNameShown;
        public string ParamNameShown
        {
            get { return this._ParamNameShown; }
            set
            {
                if (this._ParamNameShown != value)
                {
                    this._ParamNameShown = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ParamId;
        public string ParamId
        {
            get { return this._ParamId; }
            set
            {
                if (this._ParamId != value)
                {
                    this._ParamId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ParamName { get; set; }

        public int Order { get; set; }

        public string Source { get; set; }

        private bool _IsVisible = false;
        public bool IsVisible
        {
            get { return this._IsVisible; }
            set
            {
                if (this._IsVisible != value)
                {
                    this._IsVisible = value;
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
                    RaiseIsSelectedParameterEvent();
                }

            }
        }


        private int _Width;
        public int Width
        {
            get { return this._Width; }
            set
            {
                if (this._Width != value)
                {
                    this._Width = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsAuthorized = true;
        public bool IsAuthorized
        {
            get { return _IsAuthorized; }
            set
            {
                if (this._IsAuthorized != value)
                {
                    this._IsAuthorized = value;
                    OnPropertyChanged();
                }

            }
        }


        private bool _IsAPrice = false;
        public bool IsAPrice
        {
            get { return _IsAPrice; }
            set
            {
                if (this._IsAPrice != value)
                {
                    this._IsAPrice = value;
                    OnPropertyChanged();
                }

            }
        }


        public bool OrderByAscending { get; set; } = false;

        public BomExportWindowViewModel MainApp { get; set; }

        public BomExportParameterData GetBomExportParameterData()
        {
            return new BomExportParameterData() { ParamId = ParamId, ParamName = ParamName, Order = Order, Source = Source, IsSelected=IsSelected };
        }


        public event EventHandler IsSelectedParameterEvent;
        public void RaiseIsSelectedParameterEvent()
        {
            try
            {
                IsSelectedParameterEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }


        public override string ToString()
        {
            return ParamNameShown;
        }
    }
}
