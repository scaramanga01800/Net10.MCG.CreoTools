using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.DataBaseAccess.Models.SapHupDb;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.EcoSapHup;
using MCG.Tools.EcnEcoFollowUp.Services.EcoSapHup;
using Moq;
using System.Reflection;

namespace MCG.Tools.EcnEcoFollowUp.Tests
{
    public class EcoSapHupSyncServiceTests
    {
        private sealed class FixedTimeProvider : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => new(2025, 3, 7, 12, 0, 0, TimeSpan.Zero);
            public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
        }

        private readonly Mock<IEcoSapHupSyncRepository> _repo = new(MockBehavior.Strict);
        private readonly Mock<ISapHupService> _sap = new(MockBehavior.Strict);
        private readonly Dictionary<int, Ecnecofollowup> _db = new();

        private EcoSapHupSyncService CreateService()
        {
            _repo.Setup(r => r.UpdateEco(It.IsAny<int>(), It.IsAny<Action<Ecnecofollowup>>()))
                .Returns((int id, Action<Ecnecofollowup> apply) =>
                {
                    var e = _db[id];
                    var before = Snapshot(e);
                    apply(e);
                    return before != Snapshot(e);
                });
            return new EcoSapHupSyncService(_repo.Object, _sap.Object, new EcoSapHupStepCalculator(), new FixedTimeProvider());
        }

        private static string Snapshot(Ecnecofollowup e) => string.Join("|",
            typeof(Ecnecofollowup).GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string)).Select(p => p.GetValue(e)?.ToString()));

        private void Candidates(params EcoSapHupCandidate[] c)
            => _repo.Setup(r => r.GetCandidates(new DateOnly(2024, 3, 7))).Returns(c);

        private EcoSapHupCandidate AddEco(int id, string eco, bool rolling, bool pending, DateOnly? closedOn = null)
        {
            _db[id] = new Ecnecofollowup { Id = id, EcnNumber = eco, EcoClosedOn = closedOn, EcoTmlpseWiClose = 7 };
            return new EcoSapHupCandidate(id, eco, closedOn, rolling, pending);
        }

        private void SapFound(string eco, byte status = 2, DateTime? lastChange = null)
        {
            _sap.Setup(s => s.GetEcoViewStatusData(eco)).Returns(new List<DmEcoViewStatusDate>
            {
                new()
                {
                    Eco = eco, EcoDesc = "d", EcoCoord = "P1", EcoCurrentStatus = status,
                    EcoCreationDate = new DateTime(2024, 1, 1), EcoValidFrom = new DateTime(2024, 1, 2),
                    EcoStat1Date = new DateTime(2024, 1, 3), EcoStat2Date = new DateTime(2024, 1, 4),
                    EcoLastChange = lastChange ?? new DateTime(2024, 1, 4)
                }
            });
            _sap.Setup(s => s.GetEcoWfTaskData(eco)).Returns(new List<DmEcoViewWfEcoTask>
            {
                EcoSapHupStepCalculatorTests.Task("P2", "P1", "S", created: new DateTime(2024, 2, 1))
            });
        }

        [Fact]
        public void NoCandidate()
        {
            Candidates();
            var r = CreateService().Synchronize();
            Assert.Equal(0, r.CandidatesSelected);
            Assert.False(r.HasFailures);
        }

        [Fact]
        public void RollingYearOnly_WithDifference_UpdatesClosedOnWithLastChange()
        {
            Candidates(AddEco(1, "E-1", rolling: true, pending: false, closedOn: new DateOnly(2024, 1, 4)));
            SapFound("E-1", lastChange: new DateTime(2024, 9, 9, 10, 0, 0));
            var r = CreateService().Synchronize();
            Assert.Equal(1, r.LastChangeDifferences);
            Assert.Equal(1, r.Updated);
            Assert.Equal(new DateOnly(2024, 9, 9), _db[1].EcoClosedOn);
        }

        [Fact]
        public void RollingYearOnly_WithoutDifference_NotUpdated_NoWfQuery()
        {
            Candidates(AddEco(1, "E-1", true, false, new DateOnly(2024, 1, 4)));
            SapFound("E-1");
            var r = CreateService().Synchronize();
            Assert.Equal(1, r.RollingYearCandidatesWithoutDifference);
            Assert.Equal(0, r.Updated);
            _sap.Verify(s => s.GetEcoWfTaskData(It.IsAny<string>()), Times.Never);
            _repo.Verify(s => s.UpdateEco(It.IsAny<int>(), It.IsAny<Action<Ecnecofollowup>>()), Times.Never);
        }

        [Fact]
        public void PendingOnly_ValidData_UpdatesEntity()
        {
            Candidates(AddEco(1, "E-1", false, true));
            SapFound("E-1");
            var r = CreateService().Synchronize();
            Assert.Equal(1, r.FoundInSapHup);
            Assert.Equal(1, r.Updated);
            Assert.Equal("02", _db[1].EcoStatus);
            Assert.Equal("P2", _db[1].EcoWiSecondaryPlants);
            Assert.Equal(new DateOnly(2024, 1, 4), _db[1].EcoClosedOn);
            Assert.Equal(EcoSapHupMapper.NoUpdateRequired, _db[1].SapUpdateStatus);
            Assert.Equal(7, _db[1].EcoTmlpseWiClose);
        }

        [Fact]
        public void CandidateInBothGroups_ProcessedOnce()
        {
            Candidates(AddEco(1, "E-1", true, true, new DateOnly(2020, 1, 1)));
            SapFound("E-1");
            var r = CreateService().Synchronize();
            Assert.Equal(1, r.CandidatesSelected);
            Assert.Equal(1, r.RollingYearCandidates);
            Assert.Equal(1, r.PendingSapUpdateCandidates);
            _sap.Verify(s => s.GetEcoViewStatusData("E-1"), Times.Once);
            _repo.Verify(s => s.UpdateEco(1, It.IsAny<Action<Ecnecofollowup>>()), Times.Once);
        }

        [Fact]
        public void MainRowNotFound_PreservesEntity_AndWarns()
        {
            Candidates(AddEco(1, "E-1", false, true));
            _sap.Setup(s => s.GetEcoViewStatusData("E-1")).Returns(new List<DmEcoViewStatusDate>());
            var r = CreateService().Synchronize();
            Assert.Equal(1, r.NotFoundInSapHup);
            Assert.Single(r.Warnings, w => w.Kind == EcoSapHupSyncWarningKind.NotFoundInSapHup);
            _sap.Verify(s => s.GetEcoWfTaskData(It.IsAny<string>()), Times.Never);
            _repo.Verify(s => s.UpdateEco(It.IsAny<int>(), It.IsAny<Action<Ecnecofollowup>>()), Times.Never);
        }

        [Fact]
        public void EmptyDescription_Skipped()
        {
            Candidates(AddEco(1, "E-1", false, true));
            _sap.Setup(s => s.GetEcoViewStatusData("E-1")).Returns(new List<DmEcoViewStatusDate> { new() { Eco = "E-1", EcoDesc = "" } });
            var r = CreateService().Synchronize();
            Assert.Equal(1, r.Skipped);
            _repo.Verify(s => s.UpdateEco(It.IsAny<int>(), It.IsAny<Action<Ecnecofollowup>>()), Times.Never);
        }

        [Fact]
        public void IdenticalValues_CountedUnchanged()
        {
            Candidates(AddEco(1, "E-1", false, true));
            SapFound("E-1");
            var service = CreateService();
            service.Synchronize();
            var r = service.Synchronize();
            Assert.Equal(1, r.Unchanged);
            Assert.Equal(0, r.Updated);
        }

        [Fact]
        public void SapMainFailure_Recorded_NextProcessed_OriginalExceptionKept()
        {
            var boom = new InvalidOperationException("sap");
            Candidates(AddEco(1, "E-1", false, true), AddEco(2, "E-2", false, true));
            _sap.Setup(s => s.GetEcoViewStatusData("E-1")).Throws(boom);
            SapFound("E-2");
            var r = CreateService().Synchronize();
            Assert.Equal(1, r.Failures);
            Assert.Same(boom, r.Errors[0].Exception);
            Assert.Equal(EcoSapHupSyncPhase.MainStatusRetrieval, r.Errors[0].Phase);
            Assert.Equal("E-1", r.Errors[0].EcoNumber);
            Assert.Equal(1, r.Updated);
        }

        [Fact]
        public void WorkflowFailure_Recorded_NextProcessed()
        {
            Candidates(AddEco(1, "E-1", false, true), AddEco(2, "E-2", false, true));
            SapFound("E-1");
            _sap.Setup(s => s.GetEcoWfTaskData("E-1")).Throws(new TimeoutException());
            SapFound("E-2");
            var r = CreateService().Synchronize();
            Assert.Equal(EcoSapHupSyncPhase.WorkflowTaskRetrieval, Assert.Single(r.Errors).Phase);
            Assert.Equal(1, r.Updated);
            Assert.Null(_db[1].EcoStatus);
        }

        [Fact]
        public void PersistenceFailure_Recorded_NextProcessed()
        {
            var boom = new InvalidOperationException("db");
            Candidates(AddEco(1, "E-1", false, true), AddEco(2, "E-2", false, true));
            SapFound("E-1"); SapFound("E-2");
            var service = CreateService();
            _repo.Setup(r => r.UpdateEco(1, It.IsAny<Action<Ecnecofollowup>>())).Throws(boom);
            var r = service.Synchronize();
            Assert.Same(boom, Assert.Single(r.Errors).Exception);
            Assert.Equal(EcoSapHupSyncPhase.Persistence, r.Errors[0].Phase);
            Assert.Equal(1, r.Updated);
        }

        [Fact]
        public void SelectionFailure_ThrowsContextualException_WithInner()
        {
            var boom = new InvalidOperationException("sel");
            _repo.Setup(r => r.GetCandidates(It.IsAny<DateOnly>())).Throws(boom);
            var ex = Assert.Throws<EcoSapHupSyncException>(() => CreateService().Synchronize());
            Assert.Same(boom, ex.InnerException);
        }

        [Fact]
        public void CancellationBeforeSelection()
        {
            var service = CreateService();
            Assert.Throws<OperationCanceledException>(() => service.Synchronize(new CancellationToken(true)));
            _repo.Verify(r => r.GetCandidates(It.IsAny<DateOnly>()), Times.Never);
        }

        [Fact]
        public void CancellationBetweenItems_Propagates_NoErrorEntry()
        {
            using var cts = new CancellationTokenSource();
            Candidates(AddEco(1, "E-1", false, true), AddEco(2, "E-2", false, true));
            SapFound("E-1"); SapFound("E-2");
            var service = CreateService();
            _repo.Setup(r => r.UpdateEco(1, It.IsAny<Action<Ecnecofollowup>>())).Returns(true).Callback(() => cts.Cancel());
            Assert.Throws<OperationCanceledException>(() => service.Synchronize(cts.Token));
            _sap.Verify(s => s.GetEcoViewStatusData("E-2"), Times.Never);
        }

        [Fact]
        public void MissingEcoNumber_Skipped()
        {
            Candidates(new EcoSapHupCandidate(1, null, null, false, true));
            var r = CreateService().Synchronize();
            Assert.Equal(1, r.Skipped);
        }

        [Fact]
        public void Service_HasNoMutableStateNorUiDependency()
        {
            var fields = typeof(EcoSapHupSyncService).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.All(fields, f => Assert.True(f.IsInitOnly));
            var deps = typeof(EcoSapHupSyncService).GetConstructors().Single().GetParameters().Select(p => p.ParameterType.FullName!).ToList();
            Assert.DoesNotContain(deps, d => d.Contains("ViewModel") || d.Contains("Windows") || d.Contains("Windchill") || d.Contains("SapEcoService"));
            Assert.DoesNotContain(fields, f => typeof(System.Collections.IEnumerable).IsAssignableFrom(f.FieldType));
        }
    }
}
