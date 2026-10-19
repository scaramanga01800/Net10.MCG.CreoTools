using MCG.CommonLib.DataBaseAccess.Infrastructure;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.EcnFollowUp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.Tools.EcnEcoFollowUp.Services.EcnFollowUp
{
    /// <summary>
    /// EF Core persistence of the ECN follow-up synchronization. One short DI scope (fresh CreoToolsDbContext)
    /// per call: no transaction is held during Windchill calls and a failed SaveChanges never poisons later items.
    /// </summary>
    public sealed class EcnFollowUpSyncRepository : IEcnFollowUpSyncRepository
    {
        public const string LastPdmEcnSearchAction = "LASTPDMECNSEARCH";

        private readonly IServiceScopeFactory _scopeFactory;

        public EcnFollowUpSyncRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        public LastPdmEcnSearchMarker ReadLastPdmEcnSearch(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreoToolsDbContext>();
            var row = db.EcnecofollowupUpdates.AsNoTracking()
                .Where(e => e.Action == LastPdmEcnSearchAction)
                .OrderBy(e => e.Id)
                .FirstOrDefault();
            return row is null ? new LastPdmEcnSearchMarker(false, null) : new LastPdmEcnSearchMarker(true, row.Status);
        }

        public void WriteLastPdmEcnSearch(string status, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(status);
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreoToolsDbContext>();
            var row = db.EcnecofollowupUpdates
                .Where(e => e.Action == LastPdmEcnSearchAction)
                .OrderBy(e => e.Id)
                .FirstOrDefault()
                ?? throw new InvalidOperationException("Ligne ECNECOFOLLOWUP_UPDATE 'LASTPDMECNSEARCH' introuvable.");
            row.Status = status;
            cancellationToken.ThrowIfCancellationRequested();
            db.SaveChanges();
        }

        public IReadOnlyList<string?> GetWorkflowCandidateEcnNumbers(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreoToolsDbContext>();
            return db.Ecnecofollowups.AsNoTracking()
                .Where(e => e.PdmUpdateStatus == null || e.PdmUpdateStatus != EcnFollowUpMapper.NoUpdateRequired)
                .OrderBy(e => e.Id)
                .Select(e => e.EcnNumber)
                .ToList();
        }

        public bool EcnExists(string ecnNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreoToolsDbContext>();
            return db.Ecnecofollowups.AsNoTracking().Any(e => e.EcnNumber == ecnNumber);
        }

        public PdmContextLookup? FindPdmContext(string? contextName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreoToolsDbContext>();
            return db.Pdmcontexts.AsNoTracking()
                .Where(e => e.PdmContext == contextName)
                .OrderBy(e => e.Id)
                .Select(e => new PdmContextLookup(e.Product, e.ParticipantId))
                .FirstOrDefault();
        }

        public int GetMaxEcnFollowUpId(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreoToolsDbContext>();
            return db.Ecnecofollowups.Max(e => (int?)e.Id) ?? 0;
        }

        public void InsertEcn(Ecnecofollowup row, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(row);
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreoToolsDbContext>();
            db.Ecnecofollowups.Add(row);
            db.SaveChanges();
        }

        public EcnUpdateOutcome UpdateExistingEcn(string ecnNumber, Action<Ecnecofollowup> apply, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(apply);
            cancellationToken.ThrowIfCancellationRequested();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreoToolsDbContext>();
            var row = db.Ecnecofollowups
                .Where(e => e.EcnNumber == ecnNumber)
                .OrderBy(e => e.Id)
                .FirstOrDefault();
            if (row is null)
                return EcnUpdateOutcome.NotFound;
            apply(row);
            cancellationToken.ThrowIfCancellationRequested();
            db.ChangeTracker.DetectChanges();
            if (!db.ChangeTracker.HasChanges())
                return EcnUpdateOutcome.Unchanged;
            db.SaveChanges();
            return EcnUpdateOutcome.Changed;
        }
    }
}
