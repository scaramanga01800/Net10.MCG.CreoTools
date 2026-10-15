using MCG.CommonLib.DataBaseAccess.Infrastructure;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.EcoSapHup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.Tools.EcnEcoFollowUp.Services.EcoSapHup
{
    /// <summary>
    /// Persistence boundary: each call creates its own DI scope, hence its own CreoToolsDbContext,
    /// so a failed SaveChanges never poisons the processing of the next ECO.
    /// </summary>
    public sealed class EcoSapHupSyncRepository : IEcoSapHupSyncRepository
    {
        public const string ResolvedEcnState = "Resolved";
        private readonly IServiceScopeFactory _scopeFactory;

        public EcoSapHupSyncRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public IReadOnlyList<EcoSapHupCandidate> GetCandidates(DateOnly closedAfter)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreoToolsDbContext>();
            return SelectCandidates(db.Ecnecofollowups.AsNoTracking(), closedAfter);
        }

        /// <summary>
        /// Candidate selection: union of group A and group B, deduplicated by Id, ordered by EcnNumber then Id.
        /// </summary>
        public static IReadOnlyList<EcoSapHupCandidate> SelectCandidates(IQueryable<Ecnecofollowup> source, DateOnly closedAfter)
        {
            // Group A (SearchAllEcoFromLastYear): no condition on ECN state, as historical code.
            var rollingYear = source
                .Where(e => (e.EcoStatus == "02" || e.EcoStatus == "03")
                            && e.EcoClosedOn != null
                            && e.EcoClosedOn > closedAfter)
                .Select(e => new { e.Id, e.EcnNumber, e.EcoClosedOn })
                .ToList();

            // Group B (SearchAllEcoToUpdate(false)).
            var pending = source
                .Where(e => e.EcnState == ResolvedEcnState
                            && (e.SapUpdateStatus == null || e.SapUpdateStatus != EcoSapHupMapper.NoUpdateRequired))
                .Select(e => new { e.Id, e.EcnNumber, e.EcoClosedOn })
                .ToList();

            var rollingIds = rollingYear.Select(e => e.Id).ToHashSet();
            var pendingIds = pending.Select(e => e.Id).ToHashSet();

            return rollingYear.Concat(pending)
                .GroupBy(e => e.Id)
                .Select(g => g.First())
                .Select(e => new EcoSapHupCandidate(e.Id, e.EcnNumber, e.EcoClosedOn, rollingIds.Contains(e.Id), pendingIds.Contains(e.Id)))
                .OrderBy(c => c.EcnNumber, StringComparer.Ordinal)
                .ThenBy(c => c.Id)
                .ToList();
        }

        public bool UpdateEco(int id, Action<Ecnecofollowup> apply)
        {
            ArgumentNullException.ThrowIfNull(apply);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreoToolsDbContext>();

            var entity = db.Ecnecofollowups.FirstOrDefault(e => e.Id == id)
                ?? throw new InvalidOperationException($"ECNECOFOLLOWUP Id {id} introuvable.");

            apply(entity);

            if (!db.ChangeTracker.HasChanges())
                return false;

            db.SaveChanges();
            return true;
        }
    }
}
