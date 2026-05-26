using CommunityToolkit.Mvvm.ComponentModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool
{
    public class AnalysisResultItem: ObservableObject
    {
        private double _Position;
        public double Position
        {
            get { return _Position; }
            set
            {
                if (this._Position != value)
                {
                    this._Position = value;
                    OnPropertyChanged();
                }

            }
        }

        private double _Value;
        public double Value
        {
            get { return _Value; }
            set
            {
                if (this._Value != value)
                {
                    this._Value = value;
                    OnPropertyChanged();
                }

            }
        }

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

        public override string ToString()
        {
            return $"{Name} - Position {Position}:{Value}";
        }
    }
}
