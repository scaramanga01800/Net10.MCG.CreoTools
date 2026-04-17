using MCG.CommonLib.Models.Enums;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.ViewModel;

namespace MCG.WindchillTools.ManageWTObject.View
{
    public interface IMgtContentItem
    {
        string CompleteFilename { get; set; }
        FileExtensionEnum Type { get; set; }
        string Filename { get; set; }
        string ItemId { get; set; }
        bool IsPrimaryContent { get; set; }
        WindchillContentType ContentType { get; set; }
        MgtWtDocumentItem ParentWtDocument { get; set; }
        ObjectState State { get; set; }
        bool IsActive { get; set; }
        bool IsCanbeDownloaded { get; set; }
    }
}
