using MCG.Tools.EcnDataCheck.Interfaces;
using MCG.Tools.EcnDataCheck.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCG.Tools.EcnDataCheck.Services
{
    internal class AiExplanationService : IAiExplanationService
    {

        private readonly IPdfKnowledgeService _pdfKnowledgeService;

        public AiExplanationService(IPdfKnowledgeService pdfKnowledgeService)
        {
            _pdfKnowledgeService = pdfKnowledgeService;
        }

        public async Task<string> GetExplanationAsync(EcnDataCheckResultItem item)
        {
            var documentationContent = await _pdfKnowledgeService.GetRelevantContentAsync(item);

            var prompt =
                    $"""
                    You are an ECN Data Check expert.

                    Issue:
                    {item.Comments}

                    Part Number:
                    {item.ParentEcnDataCheckItem?.EcnWtPart?.Number}

                    Document:
                    {item.LinkedObjNumber}

                    Documentation:
                    {documentationContent}

                    Generate:

                    1. Summary
                    2. Cause
                    3. Impact
                    4. Corrective Actions

                    Limit to 10 lines.
                    """;

            // GPT call ici plus tard

            return prompt;
        }
    }
}
