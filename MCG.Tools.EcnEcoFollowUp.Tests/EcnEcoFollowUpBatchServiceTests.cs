using System.IO;
using System.Reflection;
using MCG.CommonLib.DataBaseAccess.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.EcnEcoBatch;
using MCG.Tools.EcnEcoFollowUp.Models.EcnFollowUp;
using MCG.Tools.EcnEcoFollowUp.Models.EcoSapHup;
using MCG.Tools.EcnEcoFollowUp.Models.PdmContext;
using MCG.Tools.EcnEcoFollowUp.Services.EcnEcoBatch;
using Moq;
using Xunit;

namespace MCG.Tools.EcnEcoFollowUp.Tests
{
    public class EcnEcoFollowUpBatchServiceTests
    {
        private readonly List<string> _calls = new();
        private readonly Mock<IPdmContextSyncService> _pdm = new(MockBehavior.Strict);
        private readonly Mock<IEcnFollowUpSyncService> _ecn = new(MockBehavior.Strict);
        private readonly Mock<IEcoSapHupSyncService> _eco = new(MockBehavior.Strict);
        private int _active;

        private EcnEcoFollowUpBatchService Sut() => new(_pdm.Object, _ecn.Object, _eco.Object);

        private T Track<T>(string name, Func<T> body)
        {
            Assert.Equal(0, Interlocked.Increment(ref _active) - 1);
            try { _calls.Add(name); return body(); }
            finally { Interlocked.Decrement(ref _active); }
        }

        private void Setup(Func<PdmContextSyncResult>? pdm = null, Func<EcnFollowUpSyncResult>? ecn = null, Func<EcoSapHupSyncResult>? eco = null)
        {
            _pdm.Setup(s => s.Synchronize(It.IsAny<CancellationToken>())).Returns(() => Track("pdm", pdm ?? (() => new PdmContextSyncResult())));
            _ecn.Setup(s => s.Synchronize(It.IsAny<CancellationToken>())).Returns(() => Track("ecn", ecn ?? (() => new EcnFollowUpSyncResult())));
            _eco.Setup(s => s.Synchronize(It.IsAny<CancellationToken>())).Returns(() => Track("eco", eco ?? (() => new EcoSapHupSyncResult())));
        }

        // Invokes the existing internal Add* method of a child result with a default-built item.
        private static T WithItem<T>(T result, string method) where T : class
        {
            var m = typeof(T).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!;
            var p = m.GetParameters()[0].ParameterType;
            object arg;
            if (p == typeof(string)) arg = "w";
            else if (p.IsGenericType) // IEnumerable<TError>
            {
                var item = Build(p.GetGenericArguments()[0]);
                var arr = Array.CreateInstance(item.GetType(), 1); arr.SetValue(item, 0); arg = arr;
            }
            else arg = Build(p);
            m.Invoke(result, new[] { arg });
            return result;
        }

        private static object Build(Type t)
        {
            var c = t.GetConstructors().OrderByDescending(x => x.GetParameters().Length).First();
            var args = c.GetParameters().Select(p =>
                p.ParameterType == typeof(string) ? "x" :
                p.ParameterType == typeof(Exception) ? new InvalidOperationException("x") :
                p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null).ToArray();
            return c.Invoke(args);
        }

        private static PdmContextSyncResult PdmFailed() => WithItem(new PdmContextSyncResult(), "AddErrors");
        private static EcnFollowUpSyncResult EcnFailed() => WithItem(new EcnFollowUpSyncResult(), "AddError");
        private static EcoSapHupSyncResult EcoFailed() => WithItem(new EcoSapHupSyncResult(), "AddError");

        // ---------- Ordre ----------

        [Fact]
        public void Run_ExecutesEachServiceOnce_InDependencyOrder_NotConcurrently()
        {
            Setup();
            Sut().Run();
            Assert.Equal(new[] { "pdm", "ecn", "eco" }, _calls);
            _pdm.Verify(s => s.Synchronize(It.IsAny<CancellationToken>()), Times.Once);
            _ecn.Verify(s => s.Synchronize(It.IsAny<CancellationToken>()), Times.Once);
            _eco.Verify(s => s.Synchronize(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Run_ForwardsSameToken_ToEveryService()
        {
            using var cts = new CancellationTokenSource();
            var seen = new List<CancellationToken>();
            _pdm.Setup(s => s.Synchronize(It.IsAny<CancellationToken>())).Returns((CancellationToken t) => { seen.Add(t); return new PdmContextSyncResult(); });
            _ecn.Setup(s => s.Synchronize(It.IsAny<CancellationToken>())).Returns((CancellationToken t) => { seen.Add(t); return new EcnFollowUpSyncResult(); });
            _eco.Setup(s => s.Synchronize(It.IsAny<CancellationToken>())).Returns((CancellationToken t) => { seen.Add(t); return new EcoSapHupSyncResult(); });
            Sut().Run(cts.Token);
            Assert.Equal(3, seen.Count);
            Assert.All(seen, t => Assert.Equal(cts.Token, t));
        }

        // ---------- SuccÃ¨s ----------

        [Fact]
        public void Run_AllSucceed_AggregatesTypedResults()
        {
            var pdm = new PdmContextSyncResult(); var ecn = new EcnFollowUpSyncResult(); var eco = new EcoSapHupSyncResult();
            Setup(() => pdm, () => ecn, () => eco);
            var r = Sut().Run();
            Assert.Same(pdm, r.PdmContext); Assert.Same(ecn, r.EcnFollowUp); Assert.Same(eco, r.EcoSapHup);
            Assert.True(r.IsSuccess); Assert.False(r.HasFailures); Assert.False(r.HasWarnings);
            Assert.Equal(3, r.CompletedStepCount); Assert.Null(r.FailedStep);
            Assert.Equal(EcnEcoBatchStepStatus.Completed, r.PdmContextStatus);
            Assert.Equal(EcnEcoBatchStepStatus.Completed, r.EcnFollowUpStatus);
            Assert.Equal(EcnEcoBatchStepStatus.Completed, r.EcoSapHupStatus);
        }

        [Fact]
        public void Run_ChildWarnings_AreReflected_WithoutFailure()
        {
            Setup(ecn: () => WithItem(new EcnFollowUpSyncResult(), "AddWarning"));
            var r = Sut().Run();
            Assert.True(r.HasWarnings); Assert.False(r.HasFailures); Assert.True(r.IsSuccess);
        }

        [Fact]
        public void Run_EcoWarnings_AreReflected()
        {
            Setup(eco: () => WithItem(new EcoSapHupSyncResult(), "AddWarning"));
            Assert.True(Sut().Run().HasWarnings);
        }

        // ---------- Echecs partiels : continuation ----------

        [Fact]
        public void PdmPartialFailure_ContinuesWithEcnAndEco()
        {
            Setup(pdm: PdmFailed);
            var r = Sut().Run();
            Assert.Equal(new[] { "pdm", "ecn", "eco" }, _calls);
            Assert.Equal(EcnEcoBatchStepStatus.CompletedWithFailures, r.PdmContextStatus);
            Assert.True(r.PdmContext!.HasFailures);
            Assert.True(r.HasFailures); Assert.False(r.IsSuccess); Assert.Equal(3, r.CompletedStepCount);
        }

        [Fact]
        public void EcnPartialFailure_ContinuesWithEco()
        {
            Setup(ecn: EcnFailed);
            var r = Sut().Run();
            Assert.Equal(new[] { "pdm", "ecn", "eco" }, _calls);
            Assert.Equal(EcnEcoBatchStepStatus.CompletedWithFailures, r.EcnFollowUpStatus);
            Assert.Equal(EcnEcoBatchStepStatus.Completed, r.EcoSapHupStatus);
            Assert.True(r.HasFailures);
        }

        [Fact]
        public void EcoPartialFailure_IsRetained()
        {
            Setup(eco: EcoFailed);
            var r = Sut().Run();
            Assert.Equal(EcnEcoBatchStepStatus.CompletedWithFailures, r.EcoSapHupStatus);
            Assert.True(r.EcoSapHup!.HasFailures); Assert.True(r.HasFailures);
        }

        [Fact]
        public void AllPartialFailures_AllStepsRun()
        {
            Setup(PdmFailed, EcnFailed, EcoFailed);
            var r = Sut().Run();
            Assert.Equal(3, _calls.Count); Assert.Equal(3, r.CompletedStepCount); Assert.True(r.HasFailures);
        }

        // ---------- Echecs globaux ----------

        [Fact]
        public void PdmGlobalFailure_SkipsEcnAndEco()
        {
            var inner = new PdmContextSyncException("boom", new IOException());
            Setup(pdm: () => throw inner);
            var ex = Assert.Throws<EcnEcoFollowUpBatchException>(() => Sut().Run());
            Assert.Equal(EcnEcoBatchStep.PdmContext, ex.FailedStep);
            Assert.Same(inner, ex.InnerException);
            Assert.Equal(new[] { "pdm" }, _calls);
            Assert.Equal(EcnEcoBatchStepStatus.Failed, ex.PartialResult.PdmContextStatus);
            Assert.Equal(EcnEcoBatchStepStatus.SkippedDueToDependencyFailure, ex.PartialResult.EcnFollowUpStatus);
            Assert.Equal(EcnEcoBatchStepStatus.SkippedDueToDependencyFailure, ex.PartialResult.EcoSapHupStatus);
            Assert.False(ex.IsConfigurationError);
        }

        [Fact]
        public void EcnGlobalFailure_SkipsEco_KeepsPdmResult()
        {
            var pdm = new PdmContextSyncResult();
            var inner = new InvalidOperationException("ecn");
            Setup(pdm: () => pdm, ecn: () => throw inner);
            var ex = Assert.Throws<EcnEcoFollowUpBatchException>(() => Sut().Run());
            Assert.Equal(EcnEcoBatchStep.EcnFollowUp, ex.FailedStep);
            Assert.Same(inner, ex.InnerException);
            Assert.Equal(new[] { "pdm", "ecn" }, _calls);
            Assert.Same(pdm, ex.PartialResult.PdmContext);
            Assert.Equal(EcnEcoBatchStepStatus.Completed, ex.PartialResult.PdmContextStatus);
            Assert.Equal(EcnEcoBatchStepStatus.Failed, ex.PartialResult.EcnFollowUpStatus);
            Assert.Equal(EcnEcoBatchStepStatus.SkippedDueToDependencyFailure, ex.PartialResult.EcoSapHupStatus);
            Assert.Equal(1, ex.PartialResult.CompletedStepCount);
        }

        [Fact]
        public void EcoGlobalFailure_KeepsPreviousResults()
        {
            var inner = new EcoSapHupSyncException("eco", new IOException());
            Setup(eco: () => throw inner);
            var ex = Assert.Throws<EcnEcoFollowUpBatchException>(() => Sut().Run());
            Assert.Equal(EcnEcoBatchStep.EcoSapHup, ex.FailedStep);
            Assert.Same(inner, ex.InnerException);
            Assert.Equal(2, ex.PartialResult.CompletedStepCount);
            Assert.Equal(EcnEcoBatchStepStatus.Failed, ex.PartialResult.EcoSapHupStatus);
        }

        // ---------- Configuration ----------

        [Fact]
        public void ConfigurationException_FromEcn_PropagatesUnchanged_AndSkipsEco()
        {
            var cfg = Cfg();
            Setup(ecn: () => throw cfg);
            var ex = Assert.Throws<WindchillBatchCredentialConfigurationException>(() => Sut().Run());
            Assert.Same(cfg, ex);
            Assert.Equal(new[] { "pdm", "ecn" }, _calls);
        }

        [Fact]
        public void ConfigurationException_FromPdm_PropagatesUnchanged()
        {
            var cfg = Cfg();
            Setup(pdm: () => throw cfg);
            Assert.Same(cfg, Assert.Throws<WindchillBatchCredentialConfigurationException>(() => Sut().Run()));
            Assert.Equal(new[] { "pdm" }, _calls);
        }

        [Fact]
        public void ConfigurationException_WrappedByPdmService_IsDetectable()
        {
            var wrapped = new PdmContextSyncException("creds", Cfg());
            Setup(pdm: () => throw wrapped);
            var ex = Assert.Throws<EcnEcoFollowUpBatchException>(() => Sut().Run());
            Assert.True(ex.IsConfigurationError);
        }

        private static WindchillBatchCredentialConfigurationException Cfg() =>
            new(default(WindchillBatchCredentialFailureReason), "cfg", "Windchill:Batch:Key", null);

        // ---------- Annulation ----------

        [Fact]
        public void Cancel_BeforeExecution_NoServiceCalled()
        {
            Setup();
            using var cts = new CancellationTokenSource(); cts.Cancel();
            Assert.Throws<OperationCanceledException>(() => Sut().Run(cts.Token));
            Assert.Empty(_calls);
        }

        [Theory]
        [InlineData("pdm", 1)]
        [InlineData("ecn", 2)]
        [InlineData("eco", 3)]
        public void Cancel_DuringStep_PropagatesUnchanged_StopsChain(string step, int expectedCalls)
        {
            var oce = new OperationCanceledException();
            Setup(step == "pdm" ? () => throw oce : null, step == "ecn" ? () => throw oce : null, step == "eco" ? () => throw oce : null);
            Assert.Same(oce, Assert.Throws<OperationCanceledException>(() => Sut().Run()));
            Assert.Equal(expectedCalls, _calls.Count);
        }

        [Theory]
        [InlineData("pdm", 1)]
        [InlineData("ecn", 2)]
        [InlineData("eco", 3)]
        public void Cancel_AfterStep_StopsBeforeNext_NoResult(string step, int expectedCalls)
        {
            using var cts = new CancellationTokenSource();
            Setup(
                () => { if (step == "pdm") cts.Cancel(); return new PdmContextSyncResult(); },
                () => { if (step == "ecn") cts.Cancel(); return new EcnFollowUpSyncResult(); },
                () => { if (step == "eco") cts.Cancel(); return new EcoSapHupSyncResult(); });
            Assert.ThrowsAny<OperationCanceledException>(() => Sut().Run(cts.Token));
            Assert.Equal(expectedCalls, _calls.Count);
        }

        [Fact]
        public void Cancel_AfterPartialFailure_StopsBeforeNext()
        {
            using var cts = new CancellationTokenSource();
            Setup(pdm: () => { cts.Cancel(); return PdmFailed(); });
            Assert.ThrowsAny<OperationCanceledException>(() => Sut().Run(cts.Token));
            Assert.Equal(new[] { "pdm" }, _calls);
        }

        // ---------- Statelessness ----------

        [Fact]
        public void TwoRuns_ProduceIndependentResults()
        {
            Setup(pdm: () => new PdmContextSyncResult());
            var sut = Sut();
            var a = sut.Run(); var b = sut.Run();
            Assert.NotSame(a, b); Assert.NotSame(a.PdmContext, b.PdmContext);
            Assert.Equal(6, _calls.Count);
        }

        [Fact]
        public void Service_HasNoStaticOrMutableInstanceState()
        {
            var fields = typeof(EcnEcoFollowUpBatchService).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.DoesNotContain(fields, f => f.IsStatic);
            Assert.All(fields, f => Assert.True(f.IsInitOnly, f.Name));
            Assert.All(fields, f => Assert.True(f.FieldType.IsInterface, f.Name));
        }

        [Fact]
        public void Service_ConstructorDependsOnlyOnThreeDomainServices()
        {
            var ctor = typeof(EcnEcoFollowUpBatchService).GetConstructors().Single();
            Assert.Equal(new[] { typeof(IPdmContextSyncService), typeof(IEcnFollowUpSyncService), typeof(IEcoSapHupSyncService) },
                ctor.GetParameters().Select(p => p.ParameterType));
        }

        // ---------- Analyse statique ----------

        private static string SourceOf(string relative)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "MCG.Tools.EcnEcoFollowUp", "MCG.Tools.EcnEcoFollowUp.csproj")))
                dir = dir.Parent;
            Assert.NotNull(dir);
            return File.ReadAllText(Path.Combine(dir!.FullName, "MCG.Tools.EcnEcoFollowUp", relative));
        }

        [Theory]
        [InlineData("Pdmcontext")] [InlineData("Pdmallcontext")] [InlineData("Ecnecofollowup")] [InlineData("EcnecofollowupUpdate")]
        [InlineData("DmEcoViewStatusDate")] [InlineData("DmEcoViewWfEcoTask")] [InlineData("EcnWorkflowTaskItem")]
        [InlineData("WindchillContextTeamMember")] [InlineData("NetworkCredential")] [InlineData("DbContext")] [InlineData("DateOnly")]
        [InlineData(".Where(")] [InlineData("SaveChanges")] [InlineData("IEcnWorkflowDateCalculator")]
        [InlineData("EcnEcoFollowUpViewModel")] [InlineData("IWindchillCredentialService")] [InlineData("IWindchillBatchCredentialProvider")]
        [InlineData("ShowDialog")] [InlineData("MessageBox")] [InlineData("Window")] [InlineData("Dispatcher")] [InlineData("ICommand")]
        [InlineData("CurrentDataContext")] [InlineData("IConfiguration")] [InlineData("SELECT ")] [InlineData("ConnectionString")]
        [InlineData("Task.Run")] [InlineData("Thread")] [InlineData("Parallel")]
        public void Orchestrator_DoesNotReference_Forbidden(string token)
        {
            var src = SourceOf(Path.Combine("Services", "EcnEcoBatch", "EcnEcoFollowUpBatchService.cs"));
            Assert.DoesNotContain(token, src, StringComparison.Ordinal);
        }
    }
}

