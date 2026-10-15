using System.Data;
using MCG.CommonLib.DataBaseAccess.Infrastructure;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.PdmContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.Tools.EcnEcoFollowUp.Services.PdmContext
{
    /// <summary>
    /// EF Core persistence of PDMCONTEXT and PDMALLCONTEXT.
    /// One DI scope (one CreoToolsDbContext) and one serializable transaction per call:
    /// both tables commit atomically, or nothing is written. No deletion.
    /// </summary>
    public sealed class PdmContextSyncRepository : IPdmContextSyncRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PdmContextSyncRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        public PdmContextPersistenceResult Synchronize(
            IReadOnlyList<PdmContextMemberSnapshot> pdmContextCandidates,
            IReadOnlyList<PdmContextMemberSnapshot> pdmAllContextCandidates,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(pdmContextCandidates);
            ArgumentNullException.ThrowIfNull(pdmAllContextCandidates);
            cancellationToken.ThrowIfCancellationRequested();

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CreoToolsDbContext>();

            cancellationToken.ThrowIfCancellationRequested();
            using var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);

            var pdmResult = new PdmContextTableSyncResult(PdmContextSyncTable.Pdmcontext);
            var allResult = new PdmContextTableSyncResult(PdmContextSyncTable.Pdmallcontext);

            var existingPdm = db.Pdmcontexts.ToList();
            var newPdm = PdmContextRowSynchronizer.SyncPdmContexts(
                existingPdm, pdmContextCandidates,
                () => new PdmContextIdAllocator(db.Pdmcontexts.Max(e => (int?)e.Id)),
                pdmResult, cancellationToken);
            db.Pdmcontexts.AddRange(newPdm);

            cancellationToken.ThrowIfCancellationRequested();
            var existingAll = db.Pdmallcontexts.ToList();
            var newAll = PdmContextRowSynchronizer.SyncPdmAllContexts(
                existingAll, pdmAllContextCandidates,
                () => new PdmContextIdAllocator(db.Pdmallcontexts.Max(e => (int?)e.Id)),
                allResult, cancellationToken);
            db.Pdmallcontexts.AddRange(newAll);

            bool saved = false;
            cancellationToken.ThrowIfCancellationRequested();
            if (pdmResult.HasWrites || allResult.HasWrites)
            {
                db.SaveChanges();
                saved = true;
            }

            cancellationToken.ThrowIfCancellationRequested();
            transaction.Commit();

            return new PdmContextPersistenceResult
            {
                PdmContext = pdmResult,
                PdmAllContext = allResult,
                Errors = Array.Empty<PdmContextSyncError>(),
                SaveChangesCalled = saved
            };
        }
    }
}
