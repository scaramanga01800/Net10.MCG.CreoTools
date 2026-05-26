using CommunityToolkit.Mvvm.ComponentModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.NumberCumulation
{
    public class NumberCumulationItem:ObservableObject
    {
        private string _Number;
        public string Number
        {
            get { return _Number; }
            set
            {
                if (this._Number != value)
                {
                    this._Number = value;
                    OnPropertyChanged();
                    RaiseNumberUpdateEvent();
                }

            }
        }

        public event EventHandler NumberUpdateEvent;

        public void RaiseNumberUpdateEvent()
        {
            try
            {
                NumberUpdateEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
    }
}
