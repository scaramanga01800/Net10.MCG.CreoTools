using MCG.WindchillRequestTool.Model.Windchill;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.BomExport
{
    public interface IBomExportClassificationItem
    {
        double CumulativeMass { get; set; }
        double CumulativeQuantity { get; set; }
        ObservableCollection<WindchillObjStructureComponent> ListItem { get; set; }
        string Material { get; set; }
        string PtcCommonName { get; set; }
    }
}