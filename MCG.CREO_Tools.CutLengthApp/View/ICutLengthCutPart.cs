using MCG.CREO_Tools.CutLengthApp.ViewModel;

namespace MCG.CREO_Tools.CutLengthApp.View
{
    public interface ICutLengthCutPart
    {
        string PartNumber { get; set; }
        string PartName { get; set; }
        string CadDocType { get; set; }
        CutLengthCutPart UpdatedPart { get; set; }
    }
}
