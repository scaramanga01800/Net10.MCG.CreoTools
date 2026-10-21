using MCG.CommonLib.DataBaseAccess.Infrastructure;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.EcnFollowUp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.Tools.EcnEcoFollowUp.Services.EcnFollowUp
{
    /// <summary>
    /// Lectures batch CreoToolsDb pour ECN Follow Up. Non utilisé par le batch en production à ce stade.
    /// Même convention que EcnFollowUpSyncRepository : un scope DI court (DbContext neuf) par lecture.
    /// </summary>
    public sealed class EcnFollowUpBatchRepository : IEcnFollowUpBatchRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EcnFollowUpBatchRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        public HashSet<string> GetExistingEcnNumbers(IEnumerable<string?> ecnNumbers, CancellationToken cancellationToken, int batchSize = OracleBatchHelper.RecommendedBatchSize)
        {
            var keys = OracleBatchHelper.NormalizeKeys(ecnNumbers);
            var found = new HashSet<string>(StringComparer.Ordinal);
            if (keys.Count == 0)
                return found;

            cancellationToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MCG.CommonLib.DataBaseAccess.Infrastructure.CreoToolsDbContext>();

            // SELECT DISTINCT ECN_NUMBER FROM ECNECOFOLLOWUP WHERE ECN_NUMBER IN (...)
            foreach (var chunk in OracleBatchHelper.Chunk(keys, batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var rows = db.Ecnecofollowups.AsNoTracking()
                    .Where(e => chunk.Contains(e.EcnNumber!))
                    .Select(e => e.EcnNumber)
                    .Distinct()
                    .ToList();
                foreach (var number in rows)
                    if (number != null)
                        found.Add(number);
            }
            return found;
        }

        public Dictionary<string, PdmContextLookup> FindPdmContextsBatch(IEnumerable<string?> contextNames, CancellationToken cancellationToken, int batchSize = OracleBatchHelper.RecommendedBatchSize)
        {
            var keys = OracleBatchHelper.NormalizeKeys(contextNames);
            var result = new Dictionary<string, PdmContextLookup>(StringComparer.Ordinal);
            if (keys.Count == 0)
                return result;

            cancellationToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MCG.CommonLib.DataBaseAccess.Infrastructure.CreoToolsDbContext>();

            // SELECT ID, PDM_CONTEXT, PRODUCT, PARTICIPANT_ID FROM PDMCONTEXT WHERE PDM_CONTEXT IN (...) ORDER BY ID
            // Le premier ID par contexte est conservé (équivalent du OrderBy(Id).FirstOrDefault() unitaire).
            foreach (var chunk in OracleBatchHelper.Chunk(keys, batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var rows = db.Pdmcontexts.AsNoTracking()
                    .Where(e => chunk.Contains(e.PdmContext!))
                    .OrderBy(e => e.Id)
                    .Select(e => new { e.PdmContext, e.Product, e.ParticipantId })
                    .ToList();
                foreach (var row in rows)
                    if (row.PdmContext != null && !result.ContainsKey(row.PdmContext))
                        result[row.PdmContext] = new PdmContextLookup(row.Product, row.ParticipantId);
            }
            return result;
        }
    }
}
