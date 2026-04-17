using MCG.CommonLib.Models.Enums;
using MCG.WindchillRequestTool.Model.Windchill;

namespace MCG.WindchillTools.ManageWTObject.View
{
    public interface IMgtWtObject
    {
        string NUMBER { get; set; }
        string REVISION { get; set; }
        string PTCCOMMONNAME { get; set; }
        string DESCRIPTION2 { get; set; }
        string DESCRIPTION21 { get; set; }
        string DESCRIPTION22 { get; set; }
        string GROUPCREATOR { get; set; }
        string QUALINSPGRP { get; set; }
        double MASS { get; set; }
        string MATERIAL { get; set; }
        string GROUP { get; set; }
        string SUB_GROUP { get; set; }
        string BRAND { get; set; }
        string MODEL { get; set; }
        string OPTION { get; set; }
        WindchillContext SelectedWindchillContext { get; set; }
        string State { get; set; }
        string Unit { get; set; }
        ObjectState Status { get; set; }
        bool IsObjectEditable { get; set; }
        bool IsWtNonVersionAttributeEditable { get; set; }
        bool IsWtVersionAttributeEditable { get; set; }
        bool IsWtCommonAttributeEditable { get; set; }
    }
}
