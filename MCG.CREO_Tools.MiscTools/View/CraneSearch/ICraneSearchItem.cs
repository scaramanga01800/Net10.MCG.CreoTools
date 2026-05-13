using MCG.CommonLib.SapTools.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.CraneSearch
{
    public interface ICraneSearchItem
    {
        string Plant { get; set; }
        string PlantCrane { get; set; }
        string CraneName { get; set; }

        ObservableCollection<SapGenericObject> PartList { get; set; }
    }
}
