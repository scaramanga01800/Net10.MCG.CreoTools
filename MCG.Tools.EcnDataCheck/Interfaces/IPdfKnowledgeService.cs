using MCG.Tools.EcnDataCheck.Models;

namespace MCG.Tools.EcnDataCheck.Interfaces
{
    public interface IPdfKnowledgeService
    {
        Task<string> GetRelevantContentAsync(EcnDataCheckResultItem item);
    }
}
