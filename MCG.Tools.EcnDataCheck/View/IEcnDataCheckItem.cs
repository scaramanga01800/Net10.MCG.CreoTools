using MCG.WindchillRequestTool;
using MCG.Tools.EcnDataCheck.Models;
using System.Collections.ObjectModel;
using MCG.WindchillRequestTool.Model.Windchill;

namespace MCG.Tools.EcnDataCheck.View
{
    /// <summary>
    /// Interface to define requirement for the View for a Ecn Data Check Item
    /// </summary>
    public interface IEcnDataCheckItem
    {
        DataCheckStatus PartMissingCheck { get; set; }
        DataCheckStatus MetaDataStatus { get; set; }
        DataCheckStatus BomPdmComparisonStatus { get; set; }
        DataCheckStatus BomErpComparisonStatus { get; set; }
        DataCheckStatus ContextStatus { get; set; }
        DataCheckStatus Desc1EnStatus { get; set; }
        DataCheckStatus Desc2EnStatus { get; set; }
        DataCheckStatus Desc1LocalStatus { get; set; }
        DataCheckStatus Desc2LocalStatus { get; set; }
        DataCheckStatus GroupCreatorStatus { get; set; }
        DataCheckStatus MassStatus { get; set; }
        DataCheckStatus QualInspGrpStatus { get; set; }
        DataCheckStatus DefaultUnitStatus { get; set; }
        DataCheckStatus MaterialStatus { get; set; }
        DataCheckStatus BrandStatus { get; set; }
        DataCheckStatus GroupStatus { get; set; }
        DataCheckStatus SubGroupStatus { get; set; }
        DataCheckStatus OptionStatus { get; set; }
        DataCheckStatus RevisionStatus { get; set; }

        bool IsPdmBomComparison { get; set; }

        bool IsErpBomComparison { get; set; }

        WindchillObjectWtPart EcnWtPart { get; set; }

        string NewName { get; set; }
        ObservableCollection<WindchillContext> WindchillContextList { get; set; }
        string NewContextName { get; set; }
        string NewFolderName { get; set; }

        BomComparisonItem PdmBomComparison { get; set; }
        BomComparisonItem ErpBomComparison { get; set; }

        ObservableCollection<IEcnDataCheckResultItem> ListDataCheckResultShown { get; set; }
        EcnDataCheckResultItem SelectedDataCheckResultItem { get; set; }
        bool IsResultItem { get; set; }

        bool IsFirstRowDetailShow { get; set; }
    }

}
