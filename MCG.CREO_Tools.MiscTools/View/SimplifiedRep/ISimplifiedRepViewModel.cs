using MCG.CREO_Tools.MiscTools.ViewModel.SimplifiedRep;

namespace MCG.CREO_Tools.MiscTools.View.SimplifiedRep
{
    internal interface ISimplifiedRepViewModel
    {
        SimplifiedRepDataContext CurrentDataContext { get; set; }
    }
}
