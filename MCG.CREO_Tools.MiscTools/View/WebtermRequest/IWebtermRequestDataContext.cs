using MCG.CREO_Tools.MiscTools.ViewModel.WebtermRequest;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.WebtermRequest
{
    public interface IWebtermRequestDataContext
    {

        WebtermRequestItem CurrentRequest { get; set; }
        ObservableCollection<WebtermRequestClass> ListClass { get; set; }
    }
}
