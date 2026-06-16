using MCG.WindchillRequestTool.Model.Windchill;

namespace MCG.CREO_Tools.MassUpdateAttribute.View
{
    public interface IMassUpdateAttributeRenameItem
    {
        string Number { get; set; }
        string OldName { get; set; }
        string NewName { get; set; }
        string ObjectId { get; set; }
        string State { get; set; }
        WindchillObjectType ObjectType { get; set; }
        bool ToBeRenamed { get; set; }
        bool IsSelected { get; set; }
    }
}
