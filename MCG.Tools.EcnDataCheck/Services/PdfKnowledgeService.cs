using MCG.Tools.EcnDataCheck.Interfaces;
using MCG.Tools.EcnDataCheck.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCG.Tools.EcnDataCheck.Services
{
    public class PdfKnowledgeService : IPdfKnowledgeService
    {
        public async Task<string> GetRelevantContentAsync(
            EcnDataCheckResultItem item)
        {
            await Task.Delay(100);

            return
                $"Documentation : {item.IssueDocumentation}\n\n" +
                $"{item.Comments}";
        }
    }
}
