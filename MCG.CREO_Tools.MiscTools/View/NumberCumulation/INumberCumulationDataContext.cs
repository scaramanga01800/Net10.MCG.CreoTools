using MCG.CREO_Tools.MiscTools.ViewModel.NumberCumulation;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.NumberCumulation
{
    internal interface INumberCumulationDataContext
    {
        string CumulNumberOnly { get; set; }
        string CumulNumberSuf { get; set; }
        string CumulNumberPre { get; set; }
        string CumulNumberSufPre { get; set; }

        ObservableCollection<NumberCumulationItem> ListNumbers { get; set; }
        NumberCumulationItem SelectedItem { get; set; }
    }
}
