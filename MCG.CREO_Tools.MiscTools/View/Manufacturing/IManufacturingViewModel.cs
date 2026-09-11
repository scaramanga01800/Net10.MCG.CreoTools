using MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing;

namespace MCG.CREO_Tools.MiscTools.View.Manufacturing
{
    internal interface IManufacturingViewModel
    {
        ManufacturingDataContext CurrentDataContext { get; set; }
    }
}
