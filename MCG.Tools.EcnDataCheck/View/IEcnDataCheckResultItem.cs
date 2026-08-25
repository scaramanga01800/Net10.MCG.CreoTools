using MCG.Tools.EcnDataCheck.View;

namespace MCG.Tools.EcnDataCheck.Models
{
    public interface IEcnDataCheckResultItem
    {
        IEcnDataCheckItem ParentEcnDataCheckItem { get; set; }

        string LinkedObjNumber { get; set; }
        string LinkedObjRevision { get; set; }
        string CurrentLink { get; set; }
        string IssueDocumentationPath { get; set; }
        string IssueDocumentation { get; set; }

        DataCheckStatus Status { get; set; }
        string Comments { get; set; }

        string AiExplanation { get; set; }
        bool AiExplanationLoaded { get; set; }
    }
}
