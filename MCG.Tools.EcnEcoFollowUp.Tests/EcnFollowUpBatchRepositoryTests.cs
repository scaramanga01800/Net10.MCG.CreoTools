using MCG.CommonLib.DataBaseAccess.Infrastructure;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.EcnEcoFollowUp.Services.EcnFollowUp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.Tools.EcnEcoFollowUp.Tests
{
    /// <summary>
    /// Équivalence EcnExists / FindPdmContext (unitaires) vs GetExistingEcnNumbers / FindPdmContextsBatch,
    /// sur une base CreoToolsDb InMemory (aucune base réelle).
    /// </summary>
    public class EcnFollowUpBatchRepositoryTests
    {
        private static (EcnFollowUpSyncRepository Single, EcnFollowUpBatchRepository Batch) Create()
        {
            var dbName = "ecn-batch-" + Guid.NewGuid();
            var services = new ServiceCollection();
            services.AddDbContext<CreoToolsDbContext>(o => o.UseInMemoryDatabase(dbName));
            var provider = services.BuildServiceProvider();

            using (var scope = provider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CreoToolsDbContext>();
                db.Ecnecofollowups.AddRange(
                    new Ecnecofollowup { Id = 1, EcnNumber = "ECN-1" },
                    new Ecnecofollowup { Id = 2, EcnNumber = "ECN-2" },
                    new Ecnecofollowup { Id = 3, EcnNumber = "ECN-2" });
                db.Pdmcontexts.AddRange(
                    new Pdmcontext { Id = 10, PdmContext = "CTX-A", Product = "P-late", ParticipantId = "U2" },
                    new Pdmcontext { Id = 5, PdmContext = "CTX-A", Product = "P-first", ParticipantId = "U1" },
                    new Pdmcontext { Id = 6, PdmContext = "CTX-B", Product = null, ParticipantId = "U3" });
                db.SaveChanges();
            }
            var factory = provider.GetRequiredService<IServiceScopeFactory>();
            return (new EcnFollowUpSyncRepository(factory), new EcnFollowUpBatchRepository(factory));
        }

        [Fact]
        public void GetExistingEcnNumbers_EqualsEcnExists_ForEachKey()
        {
            var (single, batch) = Create();
            var keys = new[] { "ECN-1", "ECN-2", "ECN-3", "ecn-1", "ECN-1" };
            var existing = batch.GetExistingEcnNumbers(keys, CancellationToken.None);
            foreach (var key in keys.Distinct())
                Assert.Equal(single.EcnExists(key, CancellationToken.None), existing.Contains(key));
        }

        [Fact]
        public void FindPdmContextsBatch_EqualsFindPdmContext_ForEachKey()
        {
            var (single, batch) = Create();
            var keys = new[] { "CTX-A", "CTX-B", "CTX-X", "CTX-A" };
            var found = batch.FindPdmContextsBatch(keys, CancellationToken.None);
            foreach (var key in keys.Distinct())
            {
                var expected = single.FindPdmContext(key, CancellationToken.None);
                if (expected is null)
                    Assert.False(found.ContainsKey(key));
                else
                    Assert.Equal(expected, found[key]);
            }
            Assert.Equal("P-first", found["CTX-A"].Product);
        }

        [Fact]
        public void Batch_NullEmptyAndSmallBatchSize_AreSafe()
        {
            var (_, batch) = Create();
            Assert.Empty(batch.GetExistingEcnNumbers(new string?[] { null, "" }, CancellationToken.None));
            Assert.Empty(batch.FindPdmContextsBatch(Array.Empty<string?>(), CancellationToken.None));
            var found = batch.GetExistingEcnNumbers(new[] { "ECN-1", "ECN-2" }, CancellationToken.None, batchSize: 1);
            Assert.Equal(2, found.Count);
        }

        [Fact]
        public void Batch_Cancelled_Throws()
        {
            var (_, batch) = Create();
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.Throws<OperationCanceledException>(() => batch.GetExistingEcnNumbers(new[] { "ECN-1" }, cts.Token));
        }
    }
}
