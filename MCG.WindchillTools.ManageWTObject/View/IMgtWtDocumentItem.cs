using MCG.CommonLib.Models.Enums;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.WindchillTools.ManageWTObject.View
{
    public interface IMgtWtDocumentItem
    {
        bool IsSelected { get; set; }

        string Number { get; set; }
        McgRevisionSchemaEnum Revision { get; set; }
        McgRevisionSchemaEnum? LastWtDocumentRevision { get; set; }
        McgRevisionSchemaEnum? LastPartRevision { get; set; }
        string WindchillDocumentType { get; set; }
        string WindchillPartType { get; set; }

        MgtWtObject WtDocumentObject { get; set; }
        MgtWtObject WtPartObject { get; set; }

        bool PartFound { get; set; }
        bool WtDocumentFound { get; set; }
        bool PartRevisionFound { get; set; }
        bool WtDocumentRevisionFound { get; set; }

        bool PartSearchDone { get; set; }
        bool WtDocumentSearchDone { get; set; }

        bool IsNewRevision { get; set; }
        ObservableCollection<MgtContentItem> ListContentItem { get; set; }

        string StatusWtDocument { get; set; }
        string StatusPart { get; set; }
        string StatusWtDocumentPart { get; set; }

        MgtRequiredActionEnum RequiredActionWtDocument { get; set; }
        MgtRequiredActionEnum RequiredActionPart { get; set; }
        MgtRequiredActionEnum RequiredActionWtDocumentPart { get; set; }

        WindchillObjectLinkType LinkWtDocumentWtPart { get; set; }

        ObjectState LinkStatus { get; set; }
    }
}
