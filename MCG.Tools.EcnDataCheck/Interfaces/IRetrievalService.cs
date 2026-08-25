using MCG.Tools.EcnDataCheck.Models;

namespace MCG.Tools.EcnDataCheck.Interfaces
{
    public interface IRetrievalService
    {
        Task<RetrievalResponse> SearchAsync(string query);
    }
}
