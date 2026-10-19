using MCG.CommonLib.DataBaseAccess.Exceptions;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.PdmContext;
using MCG.Tools.EcnEcoFollowUp.Services.PdmContext;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.IO;
using System.Net;
using System.Net.Http;

namespace MCG.Tools.EcnEcoFollowUp.Tests
{
    internal static class Pdm
    {
        public static WindchillContextTeamMember M(string? ctx, string? id, string? role = "DESIGNER", string? name = "N", string? type = "WTUser") =>
            new() { ContextName = ctx!, ParticipantId = id!, TeamRole = role!, ParticipantName = name!, ParticipantType = type! };

        public static List<WindchillContextTeamMember> L(params WindchillContextTeamMember[] items) => items.ToList();
        public static readonly List<WindchillContextTeamMember> Empty = new();

        public static PdmContextMemberSnapshot S(string ctx, string? id = "u1", string? role = "CHANGE ADMINISTRATOR III", string type = "PRODUCT", string product = "STANDARD") =>
            new() { ContextName = ctx, ContextType = type, ParticipantId = id, ParticipantName = "N", ParticipantType = "WTUser", TeamRole = role, Product = product };
    }

    public class PdmContextSyncPlannerTests
    {
        private static PdmContextSyncPlan Build(List<WindchillContextTeamMember>? dp = null, List<WindchillContextTeamMember>? dl = null,
            List<WindchillContextTeamMember>? cp = null, List<WindchillContextTeamMember>? cl = null) =>
            PdmContextSyncPlanner.Build(dp ?? Pdm.Empty, dl ?? Pdm.Empty, cp ?? Pdm.Empty, cl ?? Pdm.Empty, CancellationToken.None);

        [Fact]
        public void ProductDesigner_IsTypedProduct_AndMatchedWithCaiii()
        {
            var plan = Build(dp: Pdm.L(Pdm.M("TWR_A", "d1")), cp: Pdm.L(Pdm.M("TWR_A", "c1", "CHANGE ADMINISTRATOR III", "Caiii", "WTGroup")));
            var s = Assert.Single(plan.AllContexts);
            Assert.Equal("PRODUCT", s.ContextType);
            Assert.Equal("c1", s.ParticipantId);
            Assert.Equal("Caiii", s.ParticipantName);
            Assert.Equal("WTGroup", s.ParticipantType);
            Assert.Equal("CHANGE ADMINISTRATOR III", s.TeamRole);
            Assert.Equal("A", s.Product);
            Assert.Single(plan.PdmContexts);
        }

        [Fact]
        public void LibraryDesigner_IsTypedLibrary_AndCaiiiLibraryMatches()
        {
            var plan = Build(dl: Pdm.L(Pdm.M("LIB_X", "d1")), cl: Pdm.L(Pdm.M("LIB_X", "c1", "CHANGE ADMINISTRATOR III")));
            var s = Assert.Single(plan.AllContexts);
            Assert.Equal("LIBRARY", s.ContextType);
            Assert.Equal("c1", s.ParticipantId);
        }

        [Fact]
        public void ProductAndLibrary_AreUnited()
        {
            var plan = Build(dp: Pdm.L(Pdm.M("P1", "d")), dl: Pdm.L(Pdm.M("L1", "d")));
            Assert.Equal(new[] { "P1", "L1" }, plan.AllContexts.Select(c => c.ContextName));
        }

        [Theory]
        [InlineData("FR_ECN_Coordinators")]
        [InlineData("CHI_TWR_ECN_Coordinators")]
        public void CoordinatorGroups_AreExcludedFromCaiii(string group)
        {
            var plan = Build(dp: Pdm.L(Pdm.M("C", "d")), cp: Pdm.L(Pdm.M("C", group, "CHANGE ADMINISTRATOR III")));
            Assert.Equal(1, plan.ExcludedCoordinatorAssignments);
            Assert.Equal(string.Empty, plan.AllContexts[0].ParticipantId);
            Assert.Empty(plan.PdmContexts);
        }

        [Theory]
        [InlineData("fr_ecn_coordinators")]
        [InlineData("FR_ECN_Coordinators ")]
        [InlineData("FR_ECN_Coordinators2")]
        public void SimilarParticipant_IsNotExcluded(string group)
        {
            var plan = Build(dp: Pdm.L(Pdm.M("C", "d")), cp: Pdm.L(Pdm.M("C", group, "CHANGE ADMINISTRATOR III")));
            Assert.Equal(0, plan.ExcludedCoordinatorAssignments);
            Assert.Equal(group, plan.AllContexts[0].ParticipantId);
        }

        [Fact]
        public void CoordinatorExclusion_NotAppliedToDesigner()
        {
            var plan = Build(dp: Pdm.L(Pdm.M("C", "FR_ECN_Coordinators")));
            Assert.Single(plan.AllContexts);
            Assert.Equal(0, plan.ExcludedCoordinatorAssignments);
        }

        [Fact]
        public void DesignerWithoutCaiii_IsKept_WithEmptyParticipantAndHistoricalRole()
        {
            var plan = Build(dp: Pdm.L(Pdm.M("C", "d", "DESIGNER")));
            var s = Assert.Single(plan.AllContexts);
            Assert.Equal(string.Empty, s.ParticipantId);
            Assert.Equal(string.Empty, s.ParticipantName);
            Assert.Equal(string.Empty, s.ParticipantType);
            Assert.Equal("CHANGE ADMINISTRATOR III", s.TeamRole);
            Assert.Equal(1, plan.ContextsWithoutChangeAdministrator);
            Assert.Equal(1, plan.SkippedMissingParticipantId);
            Assert.Empty(plan.PdmContexts);
        }

        [Fact]
        public void DuplicateDesignerAssignments_AreProcessedOnce()
        {
            var plan = Build(dp: Pdm.L(Pdm.M("C", "d1"), Pdm.M("C", "d2")), dl: Pdm.L(Pdm.M("C", "d3")));
            var s = Assert.Single(plan.AllContexts);
            Assert.Equal("PRODUCT", s.ContextType); // Product first wins (historical Union order)
            Assert.Equal(2, plan.DuplicateDesignerAssignments);
        }

        [Fact]
        public void MultipleCaiii_FirstLibraryThenProductWins()
        {
            var plan = Build(dp: Pdm.L(Pdm.M("C", "d")),
                cp: Pdm.L(Pdm.M("C", "prod", "CHANGE ADMINISTRATOR III")),
                cl: Pdm.L(Pdm.M("C", "lib1", "CHANGE ADMINISTRATOR III"), Pdm.M("C", "lib2", "CHANGE ADMINISTRATOR III")));
            Assert.Equal("lib1", plan.AllContexts[0].ParticipantId);
            Assert.Equal(2, plan.DuplicateChangeAdministratorAssignments);
        }

        [Fact]
        public void MatchingIsExactOrdinal()
        {
            var plan = Build(dp: Pdm.L(Pdm.M("Ctx", "d")), cp: Pdm.L(Pdm.M("ctx", "c", "CHANGE ADMINISTRATOR III"), Pdm.M("Ctx ", "c2", "CHANGE ADMINISTRATOR III")));
            Assert.Equal(string.Empty, plan.AllContexts[0].ParticipantId);
        }

        [Fact]
        public void MissingContextName_IsRecordedAsItemError()
        {
            var plan = Build(dp: Pdm.L(Pdm.M(null, "d"), Pdm.M("OK", "d")));
            Assert.Single(plan.AllContexts);
            var e = Assert.Single(plan.Errors);
            Assert.Equal(PdmContextSyncPhase.Normalization, e.Phase);
            Assert.NotNull(e.Exception);
        }

        [Fact]
        public void NullSourceValues_DoNotThrow()
        {
            var plan = Build(dp: Pdm.L(Pdm.M("C", null, null, null, null)), cp: Pdm.L(Pdm.M(null, null, null, null, null)));
            Assert.Single(plan.AllContexts);
            Assert.Empty(plan.PdmContexts);
        }

        [Fact]
        public void SourceObjects_AreNotMutated()
        {
            var d = Pdm.M("C", "d", "DESIGNER");
            Build(dp: Pdm.L(d), cp: Pdm.L(Pdm.M("C", "c", "CHANGE ADMINISTRATOR III")));
            Assert.Equal("d", d.ParticipantId);
            Assert.Equal("DESIGNER", d.TeamRole);
        }

        [Theory]
        [InlineData("GMK_5150", "5150")]
        [InlineData("OTHER", "STANDARD")]
        [InlineData("LIBRARY ETUDES", "ETUDES")]
        [InlineData("LIBRARY CHINA", "CHINA")]
        [InlineData("LIBRARY_LEGACY_ECN_FR", "LEGACY_ECN_FR")]
        [InlineData("LIBRARY ADMIN DOCUMENTATION", "ADMIN DOCUMENTATION")]
        [InlineData("LIBRARY TIA-20090608", "TIA-20090608")]
        [InlineData("LIBRARY  DOUBLE", "DOUBLE")]
        [InlineData("LIBRARY __X", "__X")]
        [InlineData("library etudes", "etudes")]
        [InlineData("LIBRARY", "")]
        [InlineData("TWR_MDT1609", "MDT1609")]
        [InlineData("TWR_A_B", "A_B")]
        [InlineData("WI_ MY", " MY")]
        public void Product_UsesExtractProduct(string ctx, string expected)
        {
            var plan = Build(dp: Pdm.L(Pdm.M(ctx, "d")));
            Assert.Equal(expected, plan.AllContexts[0].Product);
        }

        [Fact]
        public void Cancellation_Propagates()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.Throws<OperationCanceledException>(() =>
                PdmContextSyncPlanner.Build(Pdm.L(Pdm.M("C", "d")), Pdm.Empty, Pdm.Empty, Pdm.Empty, cts.Token));
        }
    }

    public class PdmContextRowSynchronizerTests
    {
        private int _allocatorCalls;
        private Func<PdmContextIdAllocator> Alloc(int? max) => () => { _allocatorCalls++; return new PdmContextIdAllocator(max); };

        [Fact]
        public void UnchangedRow_IsCountedUnchanged()
        {
            var row = new Pdmcontext { Id = 1, PdmContext = "C", ParticipantId = "u1", ParticipantName = "N", ParticipantType = "WTUser", TeamRole = "CHANGE ADMINISTRATOR III", Product = "STANDARD", Type = "PRODUCT", ContextLink = "-" };
            var r = new PdmContextTableSyncResult(PdmContextSyncTable.Pdmcontext);
            var inserted = PdmContextRowSynchronizer.SyncPdmContexts(new[] { row }, new[] { Pdm.S("C") }, Alloc(1), r, CancellationToken.None);
            Assert.Empty(inserted);
            Assert.Equal(1, r.Unchanged);
            Assert.False(r.HasWrites);
            Assert.Equal(0, _allocatorCalls);
        }

        [Fact]
        public void ExistingRow_UpdatesAllFieldsIncludingTeamRole_ButNotContextLink()
        {
            var row = new Pdmallcontext { Id = 1, PdmContext = "C", ParticipantId = "old", TeamRole = "OLD", Product = "X", Type = "LIBRARY", ContextLink = "custom" };
            var r = new PdmContextTableSyncResult(PdmContextSyncTable.Pdmallcontext);
            PdmContextRowSynchronizer.SyncPdmAllContexts(new[] { row }, new[] { Pdm.S("C") }, Alloc(1), r, CancellationToken.None);
            Assert.Equal(1, r.Updated);
            Assert.Equal("u1", row.ParticipantId);
            Assert.Equal("CHANGE ADMINISTRATOR III", row.TeamRole);
            Assert.Equal("STANDARD", row.Product);
            Assert.Equal("PRODUCT", row.Type);
            Assert.Equal("custom", row.ContextLink);
        }

        [Fact]
        public void NewRows_UseSingleAllocation_SequentialIds()
        {
            var r = new PdmContextTableSyncResult(PdmContextSyncTable.Pdmcontext);
            var inserted = PdmContextRowSynchronizer.SyncPdmContexts(Array.Empty<Pdmcontext>(), new[] { Pdm.S("A"), Pdm.S("B") }, Alloc(41), r, CancellationToken.None);
            Assert.Equal(new[] { 42, 43 }, inserted.Select(i => i.Id));
            Assert.All(inserted, i => Assert.Equal("-", i.ContextLink));
            Assert.Equal(2, r.Inserted);
            Assert.Equal(1, _allocatorCalls);
        }

        [Fact]
        public void EmptyTable_StartsAtOne()
        {
            var r = new PdmContextTableSyncResult(PdmContextSyncTable.Pdmcontext);
            var inserted = PdmContextRowSynchronizer.SyncPdmContexts(Array.Empty<Pdmcontext>(), new[] { Pdm.S("A") }, Alloc(null), r, CancellationToken.None);
            Assert.Equal(1, inserted[0].Id);
        }

        [Fact]
        public void PdmAllContext_KeepsCandidateWithoutParticipant()
        {
            var r = new PdmContextTableSyncResult(PdmContextSyncTable.Pdmallcontext);
            var inserted = PdmContextRowSynchronizer.SyncPdmAllContexts(Array.Empty<Pdmallcontext>(), new[] { Pdm.S("A", id: "") }, Alloc(0), r, CancellationToken.None);
            Assert.Equal(string.Empty, Assert.Single(inserted).ParticipantId);
        }

        [Fact]
        public void ObsoleteRows_AreDetected_NotDeleted()
        {
            var existing = new List<Pdmcontext> { new() { Id = 1, PdmContext = "GONE" } };
            var r = new PdmContextTableSyncResult(PdmContextSyncTable.Pdmcontext);
            PdmContextRowSynchronizer.SyncPdmContexts(existing, new[] { Pdm.S("A") }, Alloc(1), r, CancellationToken.None);
            Assert.Equal(1, r.ObsoleteRowsDetected);
            Assert.Single(existing);
        }

        [Fact]
        public void DuplicateExistingRows_LowestIdWins()
        {
            var a = new Pdmcontext { Id = 5, PdmContext = "C" };
            var b = new Pdmcontext { Id = 2, PdmContext = "C" };
            var r = new PdmContextTableSyncResult(PdmContextSyncTable.Pdmcontext);
            PdmContextRowSynchronizer.SyncPdmContexts(new[] { a, b }, new[] { Pdm.S("C") }, Alloc(5), r, CancellationToken.None);
            Assert.Equal("u1", b.ParticipantId);
            Assert.Null(a.ParticipantId);
            Assert.Equal(1, r.DuplicateExistingRows);
        }

        [Fact]
        public void Cancellation_Propagates()
        {
            using var cts = new CancellationTokenSource(); cts.Cancel();
            Assert.Throws<OperationCanceledException>(() => PdmContextRowSynchronizer.SyncPdmContexts(
                Array.Empty<Pdmcontext>(), new[] { Pdm.S("A") }, Alloc(0), new PdmContextTableSyncResult(PdmContextSyncTable.Pdmcontext), cts.Token));
        }

        [Fact]
        public void SourceCode_HasNoMaxPlusOneInLoop_NorDelete()
        {
            var dir = FindSourceDir();
            foreach (var file in Directory.GetFiles(dir, "*.cs"))
            {
                var text = File.ReadAllText(file);
                Assert.DoesNotContain("Remove(", text);
                Assert.DoesNotContain("RemoveRange(", text);
                Assert.DoesNotContain("ExecuteDelete", text);
                Assert.DoesNotContain("ShowDialog", text);
                Assert.DoesNotContain("MessageBox", text);
                Assert.DoesNotContain("Task.Run", text);
            }
            var sync = File.ReadAllText(Path.Combine(dir, "PdmContextRowSynchronizer.cs"));
            Assert.DoesNotContain(".Max(", sync);
        }

        private static string FindSourceDir()
        {
            var d = new DirectoryInfo(AppContext.BaseDirectory);
            while (d != null && !Directory.Exists(Path.Combine(d.FullName, "MCG.Tools.EcnEcoFollowUp", "Services", "PdmContext")))
                d = d.Parent;
            Assert.NotNull(d);
            return Path.Combine(d!.FullName, "MCG.Tools.EcnEcoFollowUp", "Services", "PdmContext");
        }
    }

    public class PdmContextSyncServiceTests
    {
        private const string Secret = "S3cr3t-P@ss";
        private readonly Mock<IWindchillBatchCredentialProvider> _cred = new(MockBehavior.Strict);
        private readonly Mock<IWindchillReportingManagementService> _wc = new(MockBehavior.Strict);
        private readonly Mock<IPdmContextSyncRepository> _repo = new(MockBehavior.Strict);
        private readonly List<string> _calls = new();
        private IReadOnlyList<PdmContextMemberSnapshot>? _pdm, _all;

        public PdmContextSyncServiceTests()
        {
            _cred.Setup(c => c.GetCredential()).Returns(() => { _calls.Add("cred"); return new NetworkCredential("svc_user", Secret); });
            SetupWc(true, "DESIGNER", Pdm.L(Pdm.M("TWR_A", "d"), Pdm.M("TWR_B", "d")));
            SetupWc(false, "DESIGNER", Pdm.L(Pdm.M("LIB_C", "d")));
            SetupWc(true, "CHANGE ADMINISTRATOR III", Pdm.L(Pdm.M("TWR_A", "caA", "CHANGE ADMINISTRATOR III"), Pdm.M("TWR_B", "FR_ECN_Coordinators", "CHANGE ADMINISTRATOR III")));
            SetupWc(false, "CHANGE ADMINISTRATOR III", Pdm.L(Pdm.M("LIB_C", "caC", "CHANGE ADMINISTRATOR III")));
            _repo.Setup(r => r.Synchronize(It.IsAny<IReadOnlyList<PdmContextMemberSnapshot>>(), It.IsAny<IReadOnlyList<PdmContextMemberSnapshot>>(), It.IsAny<CancellationToken>()))
                .Returns((IReadOnlyList<PdmContextMemberSnapshot> p, IReadOnlyList<PdmContextMemberSnapshot> a, CancellationToken _) =>
                {
                    _calls.Add("db"); _pdm = p; _all = a;
                    return new PdmContextPersistenceResult
                    {
                        PdmContext = new PdmContextTableSyncResult(PdmContextSyncTable.Pdmcontext) { },
                        PdmAllContext = new PdmContextTableSyncResult(PdmContextSyncTable.Pdmallcontext),
                        Errors = Array.Empty<PdmContextSyncError>()
                    };
                });
        }

        private void SetupWc(bool product, string role, List<WindchillContextTeamMember> data, Exception? ex = null, Action? onCall = null)
        {
            Func<List<WindchillContextTeamMember>> f = () => { _calls.Add($"{(product ? "P" : "L")}:{role}"); onCall?.Invoke(); if (ex != null) throw ex; return data; };
            if (product)
                _wc.Setup(w => w.GetQueryBuilderUserProductFromTeamRole<WindchillContextTeamMember>(It.IsAny<NetworkCredential>(), role, null, null, null, "Default")).Returns(f);
            else
                _wc.Setup(w => w.GetQueryBuilderUserLibraryFromTeamRole<WindchillContextTeamMember>(It.IsAny<NetworkCredential>(), role, null, null, null, "Default")).Returns(f);
        }

        private PdmContextSyncService Svc() => new(_cred.Object, _wc.Object, _repo.Object);

        [Fact]
        public void NominalFlow_RetrievesFourPopulations_ThenPersists()
        {
            var r = Svc().Synchronize();
            _cred.Verify(c => c.GetCredential(), Times.Once);
            Assert.Equal(new[] { "cred", "P:DESIGNER", "L:DESIGNER", "P:CHANGE ADMINISTRATOR III", "L:CHANGE ADMINISTRATOR III", "db" }, _calls);
            Assert.Equal(2, r.DesignerProductAssignments);
            Assert.Equal(1, r.DesignerLibraryAssignments);
            Assert.Equal(2, r.ChangeAdministratorProductAssignments);
            Assert.Equal(1, r.ChangeAdministratorLibraryAssignments);
            Assert.Equal(1, r.ExcludedCoordinatorAssignments);
            Assert.Equal(3, r.NormalizedContexts);
            Assert.Equal(1, r.ContextsWithoutChangeAdministrator);
            Assert.Equal(1, r.SkippedMissingParticipantId);
            Assert.Equal(new[] { "TWR_A", "LIB_C" }, _pdm!.Select(s => s.ContextName));
            Assert.Equal(new[] { "TWR_A", "TWR_B", "LIB_C" }, _all!.Select(s => s.ContextName));
            Assert.False(r.HasFailures);
        }

        [Fact]
        public void CredentialConfigurationError_PropagatesUnchanged()
        {
            var ex = new WindchillBatchCredentialConfigurationException(default, "k", "missing");
            _cred.Setup(c => c.GetCredential()).Throws(ex);
            Assert.Same(ex, Assert.Throws<WindchillBatchCredentialConfigurationException>(() => Svc().Synchronize()));
            _repo.VerifyNoOtherCalls();
        }

        [Fact]
        public void OtherCredentialError_IsWrapped()
        {
            var inner = new InvalidOperationException("x");
            _cred.Setup(c => c.GetCredential()).Throws(inner);
            Assert.Same(inner, Assert.Throws<PdmContextSyncException>(() => Svc().Synchronize()).InnerException);
        }

        [Fact]
        public void WindchillFailure_PreventsDatabaseWrites()
        {
            var inner = new HttpRequestException("down");
            SetupWc(false, "CHANGE ADMINISTRATOR III", Pdm.Empty, inner);
            var ex = Assert.Throws<PdmContextSyncException>(() => Svc().Synchronize());
            Assert.Same(inner, ex.InnerException);
            Assert.DoesNotContain("db", _calls);
            Assert.DoesNotContain(Secret, ex.ToString());
            Assert.DoesNotContain("svc_user", ex.Message);
        }

        [Fact]
        public void PersistenceFailure_IsWrappedWithOriginal()
        {
            var inner = new InvalidOperationException("pk");
            _repo.Setup(r => r.Synchronize(It.IsAny<IReadOnlyList<PdmContextMemberSnapshot>>(), It.IsAny<IReadOnlyList<PdmContextMemberSnapshot>>(), It.IsAny<CancellationToken>())).Throws(inner);
            Assert.Same(inner, Assert.Throws<PdmContextSyncException>(() => Svc().Synchronize()).InnerException);
        }

        [Fact]
        public void ItemError_IsRecorded_AndSyncContinues()
        {
            SetupWc(false, "DESIGNER", Pdm.L(Pdm.M(null, "d"), Pdm.M("LIB_C", "d")));
            var r = Svc().Synchronize();
            Assert.True(r.HasFailures);
            Assert.Single(r.Errors);
            Assert.Contains("db", _calls);
        }

        private void SetupAll(List<WindchillContextTeamMember> dp, List<WindchillContextTeamMember> dl, List<WindchillContextTeamMember> cp, List<WindchillContextTeamMember> cl)
        {
            SetupWc(true, "DESIGNER", dp); SetupWc(false, "DESIGNER", dl);
            SetupWc(true, "CHANGE ADMINISTRATOR III", cp); SetupWc(false, "CHANGE ADMINISTRATOR III", cl);
        }

        private static List<WindchillContextTeamMember> NoContext(int n) =>
            Enumerable.Range(0, n).Select(i => Pdm.M(string.Empty, "u" + i)).ToList();

        [Fact]
        public void Run1Case_AllDesignerLibraryWithoutContextName_ThrowsExplicitDiagnostic_NoWrite()
        {
            SetupAll(Pdm.Empty, NoContext(643), Pdm.Empty, NoContext(168));
            var ex = Assert.Throws<PdmContextSyncException>(() => Svc().Synchronize());
            Assert.Contains("Library DESIGNER: received=643, withoutContextName=643, withContextName=0", ex.Message);
            Assert.DoesNotContain("db", _calls);
            _repo.Verify(r => r.Synchronize(It.IsAny<IReadOnlyList<PdmContextMemberSnapshot>>(), It.IsAny<IReadOnlyList<PdmContextMemberSnapshot>>(), It.IsAny<CancellationToken>()), Times.Never);
            Assert.DoesNotContain(Secret, ex.ToString());
            Assert.DoesNotContain("svc_user", ex.Message);
        }

        [Fact]
        public void BothDesignerPopulationsEmpty_ThrowsWithoutWrite()
        {
            SetupAll(Pdm.Empty, Pdm.Empty, Pdm.Empty, Pdm.Empty);
            var ex = Assert.Throws<PdmContextSyncException>(() => Svc().Synchronize());
            Assert.Contains("aucune affectation", ex.Message);
            Assert.DoesNotContain("db", _calls);
        }

        [Fact]
        public void ProductEmpty_WithValidLibrary_ProceedsNormally()
        {
            SetupAll(Pdm.Empty, Pdm.L(Pdm.M("LIB_C", "d")), Pdm.Empty, Pdm.L(Pdm.M("LIB_C", "caC", "CHANGE ADMINISTRATOR III")));
            var r = Svc().Synchronize();
            Assert.False(r.HasFailures);
            Assert.Contains("db", _calls);
        }

        [Fact]
        public void ProductDesignerWithContextName_IsMapped()
        {
            SetupAll(Pdm.L(Pdm.M("TWR_A", "d")), Pdm.Empty, Pdm.Empty, Pdm.Empty);
            Svc().Synchronize();
            Assert.Equal("TWR_A", Assert.Single(_all!).ContextName);
        }

        [Fact]
        public void LibraryDesignerWithContextName_IsMapped()
        {
            SetupAll(Pdm.Empty, Pdm.L(Pdm.M("LIB_C", "d")), Pdm.Empty, Pdm.Empty);
            Svc().Synchronize();
            var s = Assert.Single(_all!);
            Assert.Equal("LIB_C", s.ContextName);
            Assert.Equal("LIBRARY", s.ContextType);
        }

        [Theory]
        [InlineData("P:DESIGNER")]
        [InlineData("L:DESIGNER")]
        [InlineData("P:CHANGE ADMINISTRATOR III")]
        [InlineData("L:CHANGE ADMINISTRATOR III")]
        public void AnyOfFourQueriesFailing_PreventsWrites(string failing)
        {
            var inner = new HttpRequestException("down");
            bool product = failing.StartsWith("P:");
            SetupWc(product, failing[2..], Pdm.Empty, inner);
            var ex = Assert.Throws<PdmContextSyncException>(() => Svc().Synchronize());
            Assert.Same(inner, ex.InnerException);
            Assert.DoesNotContain("db", _calls);
        }

        [Fact]
        public void PartiallyMissingContextNames_ErrorsAreSampled_NotDuplicated()
        {
            var items = NoContext(50); items.Add(Pdm.M("LIB_C", "d"));
            SetupAll(Pdm.Empty, items, Pdm.Empty, Pdm.Empty);
            var r = Svc().Synchronize();
            Assert.True(r.HasFailures);
            Assert.Contains("db", _calls);
        }

        [Fact]
        public void ServiceAndRepositorySources_ContainNoDeleteOperation()
        {
            var dir = Path.GetDirectoryName(typeof(PdmContextSyncService).Assembly.Location)!;
            var src = typeof(PdmContextSyncService).Assembly.GetTypes().Where(t => t.Namespace == "MCG.Tools.EcnEcoFollowUp.Services.PdmContext")
                .SelectMany(t => t.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly))
                .Select(m => m.Name);
            Assert.DoesNotContain(src, n => n.Contains("Delete", StringComparison.OrdinalIgnoreCase) || n.Contains("Remove", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(dir);
        }

        [Fact]
        public void CancelledBeforeCredentials()
        {
            using var cts = new CancellationTokenSource(); cts.Cancel();
            Assert.Throws<OperationCanceledException>(() => Svc().Synchronize(cts.Token));
            Assert.Empty(_calls);
        }

        [Fact]
        public void CancelledBetweenWindchillCalls_NoDatabase()
        {
            using var cts = new CancellationTokenSource();
            SetupWc(false, "DESIGNER", Pdm.Empty, onCall: cts.Cancel);
            Assert.Throws<OperationCanceledException>(() => Svc().Synchronize(cts.Token));
            Assert.DoesNotContain("P:CHANGE ADMINISTRATOR III", _calls);
            Assert.DoesNotContain("db", _calls);
        }

        [Fact]
        public void CancelledBeforePersistence_NoDatabase()
        {
            using var cts = new CancellationTokenSource();
            SetupWc(false, "CHANGE ADMINISTRATOR III", Pdm.Empty, onCall: cts.Cancel);
            Assert.Throws<OperationCanceledException>(() => Svc().Synchronize(cts.Token));
            Assert.DoesNotContain("db", _calls);
        }

        [Fact]
        public void Stateless_TwoCalls_SameResult()
        {
            var a = Svc(); var r1 = a.Synchronize(); var r2 = a.Synchronize();
            Assert.Equal(r1.ToString(), r2.ToString());
            _cred.Verify(c => c.GetCredential(), Times.Exactly(2));
        }

        [Fact]
        public void DependencyInjection_ResolvesHeadlessGraph()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_cred.Object);
            services.AddSingleton(_wc.Object);
            services.AddTransient<IPdmContextSyncRepository, PdmContextSyncRepository>();
            services.AddTransient<IPdmContextSyncService, PdmContextSyncService>();
            using var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
            Assert.IsType<PdmContextSyncService>(sp.GetRequiredService<IPdmContextSyncService>());
        }
    }
}
