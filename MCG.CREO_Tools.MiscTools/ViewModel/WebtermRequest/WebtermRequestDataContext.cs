using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.View.WebtermRequest;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.WebtermRequest
{
    public class WebtermRequestDataContext: ObservableObject, IWebtermRequestDataContext
    {
        private WebtermRequestItem _CurrentRequest;
        public WebtermRequestItem CurrentRequest
        {
            get { return _CurrentRequest; }
            set
            {
                if (this._CurrentRequest != value)
                {
                    this._CurrentRequest = value;
                    OnPropertyChanged();
                }

            }
        }
        public ObservableCollection<WebtermRequestClass> ListClass { get; set; }= new ObservableCollection<WebtermRequestClass>();
    }
}
