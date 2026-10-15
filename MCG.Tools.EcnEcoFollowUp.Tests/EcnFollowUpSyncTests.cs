using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.EcnFollowUp;
using MCG.Tools.EcnEcoFollowUp.Models.Workflow;
using MCG.Tools.EcnEcoFollowUp.Services;
using MCG.Tools.EcnEcoFollowUp.Services.EcnFollowUp;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.IO;
using System.Net;
using System.Net.Http;

namespace MCG.Tools.EcnEcoFollowUp.Tests
{
    internal sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;
        public FixedTimeProvider(DateTimeOffset now) => _now = now;
        public override DateTimeOffset GetUtcNow() => _now.ToUniversalTime();
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    /// <summary>In-memory repository: controlled, no database.</summary>
    internal sealed class FakeEcnRepository : IEcnFollowUpSyncRepository
    {
        public LastPdmEcnSearchMarker Marker = new(true, "2024-05-01");
        public List<string> MarkerWrites = new();
        public List<Ecnecofollowup> Rows = new();
        public Dictionary<string, PdmContextLookup> Pdm = new(StringComparer.Ordinal);
        public HashSet<string> FailInsertFor = new(StringComparer.Ordinal);
        public HashSet<string> FailUpdateFor = new(StringComparer.Ordinal);
        public Exception? ReadMarkerException;
        public List<int> InsertedIds = new();

        public LastPdmEcnSearchMarker ReadLastPdmEcnSearch(CancellationToken ct)
        {
            if (ReadMarkerException != null) throw ReadMarkerException;
            return Marker;
        }

        public void WriteLastPdmEcnSearch(string status, CancellationToken ct) => MarkerWrites.Add(status);

        public IReadOnlyList<string?> GetWorkflowCandidateEcnNumbers(CancellationToken ct) =>
            Rows.Where(r => r.PdmUpdateStatus == null || r.PdmUpdateStatus != "NO UPDATE REQUIRED").OrderBy(r => r.Id).Select(r => r.EcnNumber).ToList();

        public bool EcnExists(string ecnNumber, CancellationToken ct) => Rows.Any(r => r.EcnNumber == ecnNumber);

        public PdmContextLookup? FindPdmContext(string? contextName, CancellationToken ct) =>
            contextName != null && Pdm.TryGetValue(contextName, out var p) ? p : null;

        public int GetMaxEcnFollowUpId(CancellationToken ct) => Rows.Count == 0 ? 0 : Rows.Max(r => r.Id);

        public void InsertEcn(Ecnecofollowup row, CancellationToken ct)
        {
            if (FailInsertFor.Contains(row.EcnNumber!)) throw new IOException("insert " + row.EcnNumber);
            InsertedIds.Add(row.Id);
            Rows.Add(row);
        }

        public bool UpdateExistingEcn(string ecnNumber, Action<Ecnecofollowup> apply, CancellationToken ct)
        {
            if (FailUpdateFor.Contains(ecnNumber)) throw new IOException("update " + ecnNumber);
            var row = Rows.Where(r => r.EcnNumber == ecnNumber).OrderBy(r => r.Id).FirstOrDefault();
            if (row == null) return false;
            var copy = Clone(row);
            apply(copy);
            Rows[Rows.IndexOf(row)] = copy;
            return true;
        }

        private static Ecnecofollowup Clone(Ecnecofollowup r) => (Ecnecofollowup)typeof(object)
            .GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(r, null)!;
    }

    internal sealed class EcnSyncHarness
    {
        public readonly NetworkCredential Credential = new("batch", "secret");
        public readonly Mock<IWindchillBatchCredentialProvider> Credentials = new();
        public readonly Mock<IWindchillReportingManagementService> Reporting = new();
        public readonly Mock<IWindchillChangeManagementService> ChangeMgmt = new();
        public readonly Mock<IWindchillRequestMiscService> Misc = new();
        public readonly Mock<IEcnWorkflowDateCalculator> Calculator = new();
        public readonly FakeEcnRepository Repo = new();
        public readonly List<string> Discovered = new();
        public readonly Dictionary<string, RestOdataChangeNotice> ChangeNotices = new(StringComparer.Ordinal);
        public readonly HashSet<string> ChangeNoticeFailures = new(StringComparer.Ordinal);
        public readonly HashSet<string> WorkflowFailures = new(StringComparer.Ordinal);
        public readonly Dictionary<string, List<WindchillObjectWorkflowTask>> EcnTasks = new(StringComparer.Ordinal);
        public readonly Dictionary<string, List<WindchillObjectWorkflowTask>> EcaTasks = new(StringComparer.Ordinal);
        public readonly List<IReadOnlyList<EcnWorkflowTaskItem>> CalculatorInputs = new();
        public EcnWorkflowDateResult CalcResult = new() { IsEcnFoundInPdm = true };
        public static readonly DateTimeOffset Now = new(2024, 6, 15, 10, 30, 0, TimeSpan.Zero);

        public EcnSyncHarness()
        {
            Credentials.Setup(c => c.GetCredential()).Returns(Credential);
            Reporting.Setup(r => r.GetQueryBuilderEcnCreatedFromDate<WindchillChangeNotice>(It.IsAny<NetworkCredential>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Discovered.Select(n => new WindchillChangeNotice { Number = n }).ToList());
            ChangeMgmt.Setup(c => c.GetChangeNoticeWithAllResultingObject(It.IsAny<NetworkCredential>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns((NetworkCredential _, string n, bool _, string _) =>
                {
                    if (ChangeNoticeFailures.Contains(n)) throw new HttpRequestException("cn " + n);
                    var upper = new RestOdataChangeNoticeUpper();
                    if (ChangeNotices.TryGetValue(n, out var cn)) upper.value.Add(cn);
                    return upper;
                });
            Misc.Setup(m => m.GetWindchillChangeNotice(It.IsAny<RestOdataChangeNotice>()))
                .Returns((RestOdataChangeNotice cn) => new WindchillChangeNotice
                {
                    Number = cn.Number, Name = cn.Name, State = "Resolved", CreatedBy = "creator", Description = "desc",
                    Id = "OR-" + cn.Number, Context = new WindchillContext { Name = "CTX" }, CreatedOn = new DateTime(2024, 5, 2, 23, 59, 0)
                });
            Reporting.Setup(r => r.GetQueryBuilderEcaWorkflowTask<WindchillObjectWorkflowTask>(It.IsAny<NetworkCredential>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((NetworkCredential _, string n, string _) =>
                {
                    if (WorkflowFailures.Contains(n)) throw new HttpRequestException("wf " + n);
                    return EcaTasks.TryGetValue(n, out var l) ? l : new List<WindchillObjectWorkflowTask>();
                });
            Reporting.Setup(r => r.GetQueryBuilderEcnWorkflowTask<WindchillObjectWorkflowTask>(It.IsAny<NetworkCredential>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((NetworkCredential _, string n, string _) => EcnTasks.TryGetValue(n, out var l) ? l : new List<WindchillObjectWorkflowTask>());
            Calculator.Setup(c => c.Calculate(It.IsAny<IReadOnlyList<EcnWorkflowTaskItem>>()))
                .Returns((IReadOnlyList<EcnWorkflowTaskItem> t) => { CalculatorInputs.Add(t); return CalcResult; });
        }

        public void AddChangeNotice(string number, int parts = 0, int wtDocs = 0, params WindchillObjectType[] epmTypes)
        {
            var cn = new RestOdataChangeNotice { Number = number, Name = "Name " + number };
            for (int i = 0; i < parts; i++) cn.ListWtPart.Add(new RestOdataWtPart());
            for (int i = 0; i < wtDocs; i++) cn.ListWtDocument.Add(new RestOdataWtDocument());
            foreach (var t in epmTypes) cn.ListEpmDocument.Add(new RestOdataEpmDocument { ObjectWindchillSubType = t });
            ChangeNotices[number] = cn;
        }

        public EcnFollowUpSyncService Create() => new(Credentials.Object, Reporting.Object, ChangeMgmt.Object, Misc.Object,
            Calculator.Object, Repo, new FixedTimeProvider(Now));

        public static WindchillObjectWorkflowTask Task(string name, string status = "Completed", string? eca = null) =>
            new() { Name = name, Status = status, Events = "", EcaNumber = eca!, EcaState = "Resolved", EcnState = "Open", LastModified = new DateTime(2024, 1, 1) };
    }

    public class EcnFollowUpMapperTests
    {
        [Fact]
        public void Counts_PartEpmWtDocAndDrw_DrwIsSubsetOfEpm()
        {
            var cn = new RestOdataChangeNotice();
            cn.ListWtPart.Add(new RestOdataWtPart()); cn.ListWtPart.Add(new RestOdataWtPart());
            cn.ListWtDocument.Add(new RestOdataWtDocument());
            cn.ListEpmDocument.Add(new RestOdataEpmDocument { ObjectWindchillSubType = WindchillObjectType.DRW });
            cn.ListEpmDocument.Add(new RestOdataEpmDocument { ObjectWindchillSubType = WindchillObjectType.PRT });
            cn.ListEpmDocument.Add(new RestOdataEpmDocument { ObjectWindchillSubType = WindchillObjectType.DRW });
            Assert.Equal(new ResultingObjectCounts(2, 3, 1, 2), EcnFollowUpMapper.CountResultingObjects(cn));
        }

        [Fact]
        public void Counts_EmptyLists_AreZero() =>
            Assert.Equal(new ResultingObjectCounts(0, 0, 0, 0), EcnFollowUpMapper.CountResultingObjects(new RestOdataChangeNotice()));

        [Fact]
        public void Counts_DuplicateResultingObjects_AreCountedAsHistorical()
        {
            var cn = new RestOdataChangeNotice();
            var part = new RestOdataWtPart();
            cn.ListWtPart.Add(part); cn.ListWtPart.Add(part);
            Assert.Equal(2, EcnFollowUpMapper.CountResultingObjects(cn).NbPart);
        }

        [Theory]
        [InlineData("2024-05-01", true)]
        [InlineData(" 2024-05-01 ", true)]
        [InlineData("01/05/2024", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("garbage", false)]
        public void LastPdmEcnSearch_Parse(string? status, bool ok) =>
            Assert.Equal(ok, EcnFollowUpMapper.TryParseLastPdmEcnSearch(status, out _));

        [Fact]
        public void LastPdmEcnSearch_Format_IsInvariantIso() =>
            Assert.Equal("2024-06-15", EcnFollowUpMapper.FormatLastPdmEcnSearch(new DateOnly(2024, 6, 15)));

        [Fact]
        public void PdmUpdateStatus_Format_IsDdMmYyyy() =>
            Assert.Equal("05/06/2024", EcnFollowUpMapper.FormatPdmUpdateStatus(new DateOnly(2024, 6, 5)));

        [Fact]
        public void ToDateOnly_ConvertsTypedAndPreservesNull()
        {
            Assert.Equal(new DateOnly(2024, 3, 4), EcnFollowUpMapper.ToDateOnly(new DateTime(2024, 3, 4, 23, 59, 59)));
            Assert.Null(EcnFollowUpMapper.ToDateOnly(null));
        }

        [Fact]
        public void PdmContext_NotFound_UsesHistoricalLiteral()
        {
            Assert.Equal("Not Found", EcnFollowUpMapper.ResolvePdmProduct(null));
            Assert.Equal("Not Found", EcnFollowUpMapper.ResolveCaiiiName(null));
            var p = new PdmContextLookup("TWR", "caiii");
            Assert.Equal("TWR", EcnFollowUpMapper.ResolvePdmProduct(p));
            Assert.Equal("caiii", EcnFollowUpMapper.ResolveCaiiiName(p));
        }

        [Fact]
        public void SentinelDates_TypedNormalization_OnOwnColumns()
        {
            var s = new DateOnly(9999, 1, 1);
            var valid = new DateOnly(2024, 1, 1);
            var row = new Ecnecofollowup { EcoCreatedOn = s, EcoWfStartedOn = s, EcoClosedOn = valid, EcoEffectivityDate = s, EcnCreatedOn = valid };
            EcnFollowUpMapper.NormalizeSapSentinelDates(row);
            Assert.Null(row.EcoCreatedOn);
            Assert.Null(row.EcoWfStartedOn);
            Assert.Equal(valid, row.EcoClosedOn);
            Assert.Null(row.EcoEffectivityDate);
            Assert.Equal(valid, row.EcnCreatedOn);
        }

        /// <summary>
        /// Characterization: historical AddUpdateOneEcnEcoFollowUp checked Eco_Created_On == 01/01/9999 but cleared ECN_CREATED_ON
        /// (and ECO_CREATED_ON kept the sentinel). Reproduced here as a documented model of the old behavior only.
        /// </summary>
        [Fact]
        public void Characterization_HistoricalSentinelRule_ClearedEcnCreatedOnInsteadOfEcoCreatedOn()
        {
            var s = new DateOnly(9999, 1, 1);
            var row = new Ecnecofollowup { EcoCreatedOn = s, EcnCreatedOn = new DateOnly(2024, 1, 1) };
            // historical line: if (Eco_Created_On is sentinel) CurrentEFU_EcnEcoFollowUp.ECN_CREATED_ON = null;
            if (EcnFollowUpMapper.IsSapSentinel(row.EcoCreatedOn)) row.EcnCreatedOn = null;
            Assert.Null(row.EcnCreatedOn);
            Assert.Equal(s, row.EcoCreatedOn);
        }

        [Fact]
        public void MigrationCorrection_SentinelOnEcoCreatedOn_ClearsEcoCreatedOn_AndKeepsEcnCreatedOn()
        {
            var row = new Ecnecofollowup { EcoCreatedOn = new DateOnly(9999, 1, 1) };
            var snapshot = Snapshot("B-1");
            EcnFollowUpMapper.ApplyWorkflowUpdate(row, snapshot, null, new EcnWorkflowDateResult { IsEcnFoundInPdm = true }, new DateOnly(2024, 6, 15));
            Assert.Null(row.EcoCreatedOn);
            Assert.Equal(snapshot.CreatedOn, row.EcnCreatedOn);
        }

        [Fact]
        public void NewRow_HistoricalDefaults()
        {
            var row = EcnFollowUpMapper.CreateNewRow(42, Snapshot("B-1"), new PdmContextLookup("P", "C"));
            Assert.Equal(42, row.Id);
            Assert.Equal("Not Created", row.EcoStatus);
            Assert.Null(row.PdmUpdateStatus);
            Assert.Null(row.DesignerStartAppDate);
            Assert.Null(row.FirstApprovalDate);
            Assert.Null(row.QualCheckApprovalDate);
            Assert.Null(row.CaiiiApprovalDate);
            Assert.Null(row.EcoCreatedOn);
            Assert.Null(row.EcoWfStartedOn);
            Assert.Null(row.EcoEffectivityDate);
            Assert.Equal("P", row.PdmProduct);
            Assert.Equal("C", row.CaiiiName);
            Assert.Equal("OR-1", row.PdmEcnId);
            Assert.Equal(new DateOnly(2024, 5, 2), row.EcnCreatedOn);
            Assert.Equal(3, row.NbPart);
        }

        [Fact]
        public void WorkflowUpdate_NullCalculatorDates_OverwriteExistingDates()
        {
            var d = new DateOnly(2023, 1, 1);
            var row = new Ecnecofollowup { DesignerStartAppDate = d, FirstApprovalDate = d, QualCheckApprovalDate = d, CaiiiApprovalDate = d };
            EcnFollowUpMapper.ApplyWorkflowUpdate(row, Snapshot("B-1"), null, new EcnWorkflowDateResult { IsEcnFoundInPdm = true }, new DateOnly(2024, 6, 15));
            Assert.Null(row.DesignerStartAppDate);
            Assert.Null(row.FirstApprovalDate);
            Assert.Null(row.QualCheckApprovalDate);
            Assert.Null(row.CaiiiApprovalDate);
            Assert.Equal("15/06/2024", row.PdmUpdateStatus);
        }

        [Fact]
        public void WorkflowUpdate_PreservesPdmEcnIdAndEcoStatus()
        {
            var row = new Ecnecofollowup { PdmEcnId = "keep", EcoStatus = "ECO OK", SapUpdateStatus = "S" };
            EcnFollowUpMapper.ApplyWorkflowUpdate(row, Snapshot("B-1"), null, new EcnWorkflowDateResult { IsEcnFoundInPdm = true }, new DateOnly(2024, 6, 15));
            Assert.Equal("keep", row.PdmEcnId);
            Assert.Equal("ECO OK", row.EcoStatus);
            Assert.Equal("S", row.SapUpdateStatus);
        }

        [Theory]
        [InlineData("Resolved", true, true)]
        [InlineData("Canceled", true, true)]
        [InlineData("resolved", true, false)]
        [InlineData("Open", true, false)]
        [InlineData("Resolved", false, false)]
        public void NoUpdateRequired_Rule(string state, bool allDates, bool expected)
        {
            var d = new DateTime(2024, 1, 1);
            var wf = new EcnWorkflowDateResult { IsEcnFoundInPdm = true, DesignerStartAppDate = d, FirstApprovalDate = d, QualCheckApprovalDate = d, CaiiiApprovalDate = allDates ? d : null };
            Assert.Equal(expected, EcnFollowUpMapper.IsNoUpdateRequired(state, wf));
        }

        [Fact]
        public void Deduplicate_OrdinalCaseSensitive_PreservesOrder()
        {
            var r = EcnFollowUpMapper.Deduplicate(new[] { "B-2", "B-1", "B-2", "b-1", null, " " }, out int dup, out int inv);
            Assert.Equal(new[] { "B-2", "B-1", "b-1" }, r);
            Assert.Equal(1, dup);
            Assert.Equal(2, inv);
        }

        [Fact]
        public void MapWorkflowTasks_EcnTasksFirstThenEca_NoSorting()
        {
            var ecn1 = EcnSyncHarness.Task("Submit Change Notice");
            var eca1 = EcnSyncHarness.Task("Complete Change Notice Task", eca: "A1");
            var eca2 = EcnSyncHarness.Task("Set Up Reviewer Participants", eca: "A2");
            var ecn2 = EcnSyncHarness.Task("Audit Change Notice");
            var mapped = EcnFollowUpMapper.MapWorkflowTasks(new[] { ecn1, ecn2 }, new[] { eca1, eca2 });
            Assert.Equal(new[] { "Submit Change Notice", "Audit Change Notice", "Complete Change Notice Task", "Set Up Reviewer Participants" }, mapped.Select(m => m.WfTaskName));
        }

        [Fact]
        public void MapWorkflowTasks_UsesHistoricalVoteAndCompletedOn()
        {
            var potential = EcnSyncHarness.Task("T1", "Potential");
            var rework = EcnSyncHarness.Task("T2"); rework.Events = "Rework";
            var done = EcnSyncHarness.Task("T3", eca: "A1");
            var m = EcnFollowUpMapper.MapWorkflowTasks(new[] { potential, rework }, new[] { done });
            Assert.Equal("In progress", m[0].Vote); Assert.Null(m[0].WfTaskCompletedOn);
            Assert.Equal("Rework", m[1].Vote); Assert.Equal(new DateTime(2024, 1, 1), m[1].WfTaskCompletedOn);
            Assert.Equal("Approved", m[2].Vote); Assert.Equal("A1", m[2].EcaNumber); Assert.Equal("Resolved", m[2].EcaState);
        }

        [Fact]
        public void MapWorkflowTasks_SameInstanceInBothLists_UnionKeepsOne()
        {
            var t = EcnSyncHarness.Task("T");
            Assert.Single(EcnFollowUpMapper.MapWorkflowTasks(new[] { t }, new[] { t }));
        }

        internal static EcnChangeNoticeSnapshot Snapshot(string n) => new()
        {
            EcnNumber = n, Name = "N", State = "Open", CreatedBy = "c", Description = "d", PdmEcnId = "OR-1", PdmContext = "CTX",
            CreatedOn = new DateOnly(2024, 5, 2), Counts = new ResultingObjectCounts(3, 2, 1, 1)
        };
    }

    public class EcnFollowUpSyncServiceTests
    {
        [Fact]
        public void ZeroNewEcn_MarkerAdvanced_QueryUsesMarkerAndFilter()
        {
            var h = new EcnSyncHarness();
            var r = h.Create().Synchronize();
            h.Reporting.Verify(x => x.GetQueryBuilderEcnCreatedFromDate<WindchillChangeNotice>(h.Credential, "2024-05-01", "B-*", It.IsAny<string>()), Times.Once);
            Assert.Equal(new DateOnly(2024, 5, 1), r.DiscoveryStartDate);
            Assert.True(r.LastPdmEcnSearchUpdated);
            Assert.Equal(new[] { "2024-06-15" }, h.Repo.MarkerWrites);
            Assert.False(r.HasFailures);
        }

        [Fact]
        public void CredentialsResolvedOncePerRun()
        {
            var h = new EcnSyncHarness();
            h.Discovered.AddRange(new[] { "B-1", "B-2" });
            h.AddChangeNotice("B-1"); h.AddChangeNotice("B-2");
            h.Create().Synchronize();
            h.Credentials.Verify(c => c.GetCredential(), Times.Once);
        }

        [Theory]
        [InlineData(false, "2024-05-01")]
        [InlineData(true, "")]
        [InlineData(true, null)]
        [InlineData(true, "01/05/2024")]
        public void InvalidMarker_GlobalFailure_NoWindchillQuery_NoWrite(bool exists, string? status)
        {
            var h = new EcnSyncHarness();
            h.Repo.Marker = new LastPdmEcnSearchMarker(exists, status);
            Assert.Throws<EcnFollowUpSyncException>(() => h.Create().Synchronize());
            h.Reporting.Verify(x => x.GetQueryBuilderEcnCreatedFromDate<WindchillChangeNotice>(It.IsAny<NetworkCredential>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.Empty(h.Repo.MarkerWrites);
        }

        [Fact]
        public void MarkerReadFailure_IsGlobal_OriginalKept()
        {
            var h = new EcnSyncHarness();
            var boom = new IOException("db");
            h.Repo.ReadMarkerException = boom;
            var ex = Assert.Throws<EcnFollowUpSyncException>(() => h.Create().Synchronize());
            Assert.Same(boom, ex.InnerException);
        }

        [Fact]
        public void DiscoveryQueryFailure_IsGlobal_MarkerNotAdvanced()
        {
            var h = new EcnSyncHarness();
            var boom = new HttpRequestException("wc");
            h.Reporting.Setup(x => x.GetQueryBuilderEcnCreatedFromDate<WindchillChangeNotice>(It.IsAny<NetworkCredential>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(boom);
            var ex = Assert.Throws<EcnFollowUpSyncException>(() => h.Create().Synchronize());
            Assert.Same(boom, ex.InnerException);
            Assert.Empty(h.Repo.MarkerWrites);
        }

        [Fact]
        public void CredentialFailure_Propagates()
        {
            var h = new EcnSyncHarness();
            h.Credentials.Setup(c => c.GetCredential()).Throws(new InvalidOperationException("cfg"));
            Assert.Throws<InvalidOperationException>(() => h.Create().Synchronize());
            Assert.Empty(h.Repo.MarkerWrites);
        }

        [Fact]
        public void OneNewEcn_Inserted_WithDefaultsAndPdmContext()
        {
            var h = new EcnSyncHarness();
            h.Discovered.Add("B-1");
            h.AddChangeNotice("B-1", parts: 2, wtDocs: 1, WindchillObjectType.DRW, WindchillObjectType.ASM);
            h.Repo.Pdm["CTX"] = new PdmContextLookup("TWR", "caiii");
            var r = h.Create().Synchronize();
            Assert.Equal(1, r.EcnsInserted);
            var row = h.Repo.Rows.Single(x => x.EcnNumber == "B-1");
            Assert.Equal("Not Created", row.EcoStatus);
            Assert.Equal("TWR", row.PdmProduct);
            Assert.Equal("caiii", row.CaiiiName);
            Assert.Equal((2, 2, 1, 1), (row.NbPart, row.NbEpmDoc, row.NbWtDoc, row.NbDrw));
        }

        [Fact]
        public void NewEcn_PdmContextMissing_NotFoundLiteral()
        {
            var h = new EcnSyncHarness();
            h.Discovered.Add("B-1"); h.AddChangeNotice("B-1");
            h.Create().Synchronize();
            Assert.Equal("Not Found", h.Repo.Rows[0].CaiiiName);
        }

        [Fact]
        public void SeveralNewEcn_IdsAllocatedSequentiallyFromMaxReadOnce()
        {
            var h = new EcnSyncHarness();
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 10, EcnNumber = "B-0", PdmUpdateStatus = "NO UPDATE REQUIRED" });
            h.Discovered.AddRange(new[] { "B-1", "B-2", "B-3" });
            foreach (var n in h.Discovered) h.AddChangeNotice(n);
            var r = h.Create().Synchronize();
            Assert.Equal(3, r.EcnsInserted);
            Assert.Equal(new[] { 11, 12, 13 }, h.Repo.InsertedIds);
        }

        [Fact]
        public void DuplicateCandidates_Reported_InsertedOnce()
        {
            var h = new EcnSyncHarness();
            h.Discovered.AddRange(new[] { "B-1", "B-1", "B-2" });
            h.AddChangeNotice("B-1"); h.AddChangeNotice("B-2");
            var r = h.Create().Synchronize();
            Assert.Equal(3, r.EcnsReturnedByWindchill);
            Assert.Equal(1, r.DuplicateCandidates);
            Assert.Equal(1, h.Repo.Rows.Count(x => x.EcnNumber == "B-1"));
        }

        [Fact]
        public void ExistingEcn_NotInsertedTwice_NotModifiedByDiscovery()
        {
            var h = new EcnSyncHarness();
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1", EcnName = "old", PdmUpdateStatus = "NO UPDATE REQUIRED" });
            h.Discovered.Add("B-1"); h.AddChangeNotice("B-1");
            var r = h.Create().Synchronize();
            Assert.Equal(1, r.EcnsAlreadyExisting);
            Assert.Equal(0, r.EcnsInserted);
            Assert.Equal("old", h.Repo.Rows.Single().EcnName);
            h.ChangeMgmt.Verify(c => c.GetChangeNoticeWithAllResultingObject(It.IsAny<NetworkCredential>(), "B-1", false, It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void NewEcn_ChangeNoticeEmpty_NotInserted_WarningOnly_MarkerAdvanced()
        {
            var h = new EcnSyncHarness();
            h.Discovered.Add("B-1");
            var r = h.Create().Synchronize();
            Assert.Equal(1, r.NewEcnsNotFoundInWindchill);
            Assert.Empty(h.Repo.Rows);
            Assert.True(r.LastPdmEcnSearchUpdated);
        }

        [Fact]
        public void NewEcn_ChangeNoticeFailure_Recorded_NextProcessed_MarkerNotAdvanced()
        {
            var h = new EcnSyncHarness();
            h.Discovered.AddRange(new[] { "B-1", "B-2" });
            h.AddChangeNotice("B-2");
            h.ChangeNoticeFailures.Add("B-1");
            var r = h.Create().Synchronize();
            var e = Assert.Single(r.Errors);
            Assert.Equal("B-1", e.EcnNumber);
            Assert.Equal(EcnFollowUpSyncPhase.NewEcnChangeNoticeRetrieval, e.Phase);
            Assert.IsType<HttpRequestException>(e.Exception);
            Assert.Equal(1, r.EcnsInserted);
            Assert.False(r.LastPdmEcnSearchUpdated);
            Assert.Empty(h.Repo.MarkerWrites);
        }

        [Fact]
        public void NewEcn_PersistenceFailure_Recorded_NextProcessed_IdReRead()
        {
            var h = new EcnSyncHarness();
            h.Discovered.AddRange(new[] { "B-1", "B-2" });
            h.AddChangeNotice("B-1"); h.AddChangeNotice("B-2");
            h.Repo.FailInsertFor.Add("B-1");
            var r = h.Create().Synchronize();
            Assert.Equal(EcnFollowUpSyncPhase.NewEcnPersistence, Assert.Single(r.Errors).Phase);
            Assert.IsType<IOException>(r.Errors[0].Exception);
            Assert.Equal(new[] { 1 }, h.Repo.InsertedIds);
            Assert.Empty(h.Repo.MarkerWrites);
        }

        [Fact]
        public void Cancellation_BeforeStart_Propagates_NoWrite()
        {
            var h = new EcnSyncHarness();
            using var cts = new CancellationTokenSource(); cts.Cancel();
            Assert.Throws<OperationCanceledException>(() => h.Create().Synchronize(cts.Token));
            Assert.Empty(h.Repo.MarkerWrites);
        }

        [Fact]
        public void Cancellation_DuringNewEcnLoop_MarkerNotAdvanced_NotRecordedAsError()
        {
            var h = new EcnSyncHarness();
            using var cts = new CancellationTokenSource();
            h.Discovered.AddRange(new[] { "B-1", "B-2" });
            h.AddChangeNotice("B-1"); h.AddChangeNotice("B-2");
            h.Misc.Setup(m => m.GetWindchillChangeNotice(It.IsAny<RestOdataChangeNotice>()))
                .Returns(() => { cts.Cancel(); return new WindchillChangeNotice { Context = new WindchillContext { Name = "CTX" } }; });
            Assert.ThrowsAny<OperationCanceledException>(() => h.Create().Synchronize(cts.Token));
            Assert.Empty(h.Repo.MarkerWrites);
        }

        [Fact]
        public void Workflow_ZeroCandidates()
        {
            var h = new EcnSyncHarness();
            var r = h.Create().Synchronize();
            Assert.Equal(0, r.WorkflowDatabaseCandidates);
            h.Calculator.Verify(c => c.Calculate(It.IsAny<IReadOnlyList<EcnWorkflowTaskItem>>()), Times.Never);
        }

        [Fact]
        public void Workflow_OneCandidate_EcnThenEcaOrder_PassedToCalculatorOnce()
        {
            var h = new EcnSyncHarness();
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1" });
            h.AddChangeNotice("B-1");
            h.EcaTasks["B-1"] = new() { EcnSyncHarness.Task("ECA-1", eca: "A1"), EcnSyncHarness.Task("ECA-2", eca: "A2") };
            h.EcnTasks["B-1"] = new() { EcnSyncHarness.Task("ECN-1") };
            var r = h.Create().Synchronize();
            var input = Assert.Single(h.CalculatorInputs);
            Assert.Equal(new[] { "ECN-1", "ECA-1", "ECA-2" }, input.Select(t => t.WfTaskName));
            Assert.Equal(1, r.WorkflowCalculationsPerformed);
            Assert.Equal(1, r.WorkflowEcnsUpdated);
        }

        [Fact]
        public void Workflow_NewEcnAlsoCandidate_ProcessedOnceByWorkflow()
        {
            var h = new EcnSyncHarness();
            h.Discovered.Add("B-1"); h.AddChangeNotice("B-1");
            var r = h.Create().Synchronize();
            Assert.Equal(1, r.EcnsInserted);
            Assert.Equal(1, r.WorkflowDeduplicatedEcns);
            Assert.Single(h.CalculatorInputs);
        }

        [Fact]
        public void Workflow_DuplicateRows_DeduplicatedByEcnNumber()
        {
            var h = new EcnSyncHarness();
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1" });
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 2, EcnNumber = "B-1" });
            h.AddChangeNotice("B-1");
            var r = h.Create().Synchronize();
            Assert.Equal(2, r.WorkflowDatabaseCandidates);
            Assert.Equal(1, r.WorkflowDuplicateCandidates);
            Assert.Single(h.CalculatorInputs);
        }

        [Fact]
        public void Workflow_DatesConvertedToDateOnly_AndNoUpdateRequiredWhenResolvedAndComplete()
        {
            var h = new EcnSyncHarness();
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1" });
            h.AddChangeNotice("B-1");
            var d = new DateTime(2024, 2, 3, 22, 15, 0);
            h.CalcResult = new EcnWorkflowDateResult { IsEcnFoundInPdm = true, DesignerStartAppDate = d, FirstApprovalDate = d, QualCheckApprovalDate = d, CaiiiApprovalDate = d };
            var r = h.Create().Synchronize();
            var row = h.Repo.Rows.Single();
            Assert.Equal(new DateOnly(2024, 2, 3), row.CaiiiApprovalDate);
            Assert.Equal("NO UPDATE REQUIRED", row.PdmUpdateStatus);
            Assert.Equal(1, r.WorkflowEcnsSetNoUpdateRequired);
        }

        [Fact]
        public void Workflow_MissingDate_ProcessingDateStatus()
        {
            var h = new EcnSyncHarness();
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1" });
            h.AddChangeNotice("B-1");
            h.CalcResult = new EcnWorkflowDateResult { IsEcnFoundInPdm = true, DesignerStartAppDate = new DateTime(2024, 1, 1) };
            h.Create().Synchronize();
            Assert.Equal("15/06/2024", h.Repo.Rows.Single().PdmUpdateStatus);
        }

        [Fact]
        public void Workflow_SubmitInProgress_DelegatedToCalculator_DatesNulled()
        {
            var h = new EcnSyncHarness();
            var old = new DateOnly(2020, 1, 1);
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1", FirstApprovalDate = old });
            h.AddChangeNotice("B-1");
            h.CalcResult = new EcnWorkflowDateResult { IsEcnFoundInPdm = true, IsSubmitChangeNoticeInProgress = true };
            h.Create().Synchronize();
            Assert.Null(h.Repo.Rows.Single().FirstApprovalDate);
        }

        [Fact]
        public void Workflow_EcnNotInWindchill_RowUntouched()
        {
            var h = new EcnSyncHarness();
            var row = new Ecnecofollowup { Id = 1, EcnNumber = "B-1", EcnName = "keep", PdmUpdateStatus = "01/01/2024", FirstApprovalDate = new DateOnly(2020, 1, 1) };
            h.Repo.Rows.Add(row);
            h.CalcResult = new EcnWorkflowDateResult { IsEcnFoundInPdm = false };
            var r = h.Create().Synchronize();
            Assert.Same(row, h.Repo.Rows.Single());
            Assert.Equal("keep", row.EcnName);
            Assert.Equal("01/01/2024", row.PdmUpdateStatus);
            Assert.Equal(1, r.WorkflowEcnsNotFoundInWindchill);
            Assert.Equal(1, r.WorkflowEcnsWithoutWorkflowTasks);
        }

        [Fact]
        public void Workflow_RetrievalFailure_Recorded_NextContinues()
        {
            var h = new EcnSyncHarness();
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1" });
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 2, EcnNumber = "B-2" });
            h.AddChangeNotice("B-1"); h.AddChangeNotice("B-2");
            h.WorkflowFailures.Add("B-1");
            var r = h.Create().Synchronize();
            var e = Assert.Single(r.Errors);
            Assert.Equal(EcnFollowUpSyncPhase.WorkflowTaskRetrieval, e.Phase);
            Assert.IsType<HttpRequestException>(e.Exception);
            Assert.Equal(1, r.WorkflowEcnsUpdated);
            Assert.True(r.HasFailures);
        }

        [Fact]
        public void Workflow_PersistenceFailure_Recorded_NextContinues()
        {
            var h = new EcnSyncHarness();
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 1, EcnNumber = "B-1" });
            h.Repo.Rows.Add(new Ecnecofollowup { Id = 2, EcnNumber = "B-2" });
            h.AddChangeNotice("B-1"); h.AddChangeNotice("B-2");
            h.Repo.FailUpdateFor.Add("B-1");
            var r = h.Create().Synchronize();
            Assert.Equal(EcnFollowUpSyncPhase.WorkflowPersistence, Assert.Single(r.Errors).Phase);
            Assert.IsType<IOException>(r.Errors[0].Exception);
            Assert.Equal(1, r.WorkflowEcnsUpdated);
            Assert.True(r.LastPdmEcnSearchUpdated); // workflow failures do not affect phase A marker
        }

        [Fact]
        public void Stateless_ConsecutiveCalls_DoNotShareCandidates_NewResult()
        {
            var h = new EcnSyncHarness();
            var svc = h.Create();
            h.Discovered.Add("B-1"); h.AddChangeNotice("B-1");
            var r1 = svc.Synchronize();
            h.Discovered.Clear();
            h.Repo.Rows.Clear();
            var r2 = svc.Synchronize();
            Assert.NotSame(r1, r2);
            Assert.Equal(0, r2.EcnsReturnedByWindchill);
            Assert.Equal(0, r2.WorkflowDatabaseCandidates);
            Assert.Single(h.CalculatorInputs); // only the first run had a workflow candidate
        }

        [Fact]
        public void Stateless_NoInstanceCollectionFields()
        {
            var fields = typeof(EcnFollowUpSyncService).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            Assert.DoesNotContain(fields, f => typeof(System.Collections.IEnumerable).IsAssignableFrom(f.FieldType) || f.FieldType == typeof(NetworkCredential));
        }

        [Fact]
        public void DI_RegistersServiceAndRepository_Transient()
        {
            var services = new ServiceCollection();
            services.AddEcnEcoFollowUpServices();
            Assert.Equal(ServiceLifetime.Transient, services.Single(d => d.ServiceType == typeof(IEcnFollowUpSyncService)).Lifetime);
            Assert.Equal(ServiceLifetime.Transient, services.Single(d => d.ServiceType == typeof(IEcnFollowUpSyncRepository)).Lifetime);
        }

        [Fact]
        public void StaticScan_NoForbiddenDependencies()
        {
            var dir = FindSourceDir();
            var forbidden = new[] { "EcnEcoFollowUpViewModel", "IWindchillCredentialService", "WindchillCredentialService", "ShowDialog", "MessageBox", "Window", "Dispatcher", "CurrentDataContext", "ICommand", "Task.Run", "NewEcnList", "FromSql", "ExecuteSql", "ConnectionString", ".ToString().IndexOf(" };
            foreach (var file in Directory.GetFiles(dir, "*.cs"))
            {
                var text = File.ReadAllText(file);
                foreach (var f in forbidden)
                    Assert.False(text.Contains(f, StringComparison.Ordinal), $"{Path.GetFileName(file)} contains {f}");
            }
        }

        private static string FindSourceDir()
        {
            var d = new DirectoryInfo(AppContext.BaseDirectory);
            while (d != null && !Directory.Exists(Path.Combine(d.FullName, "MCG.Tools.EcnEcoFollowUp", "Services", "EcnFollowUp")))
                d = d.Parent;
            Assert.NotNull(d);
            return Path.Combine(d!.FullName, "MCG.Tools.EcnEcoFollowUp", "Services", "EcnFollowUp");
        }
    }
}
