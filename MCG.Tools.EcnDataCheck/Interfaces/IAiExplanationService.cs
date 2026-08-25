using MCG.Tools.EcnDataCheck.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCG.Tools.EcnDataCheck.Interfaces
{
    public interface IAiExplanationService
    {
        Task<string> GetExplanationAsync(EcnDataCheckResultItem item);
    }
}
