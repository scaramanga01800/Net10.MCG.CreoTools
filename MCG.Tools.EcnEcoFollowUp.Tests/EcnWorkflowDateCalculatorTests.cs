using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.Workflow;
using MCG.Tools.EcnEcoFollowUp.Services.Workflow;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace MCG.Tools.EcnEcoFollowUp.Tests
{
    /// <summary>
    /// Characterization tests of the historical ExtractPdmWfTaskDate (EcnEcoFollowUpManagement.cs).
    /// Branch ids (H0..H11, F0..F8) match the decision map documented in Docs\Rapport_EcnWorkflowDateCalculator.md.
    /// </summary>
    public class EcnWorkflowDateCalculatorTests
    {
        private static readonly DateTime D1 = new(2024, 1, 10, 8, 0, 0);
        private static readonly DateTime D2 = new(2024, 2, 10, 9, 0, 0);
        private static readonly DateTime D3 = new(2024, 3, 10, 10, 0, 0);
        private static readonly DateTime D4 = new(2024, 4, 10, 11, 0, 0);
        private static readonly DateTime D5 = new(2024, 5, 10, 12, 0, 0);
        private static readonly DateTime D6 = new(2024, 6, 10, 13, 0, 0);

        private readonly EcnWorkflowDateCalculator _calc = new();

        private static EcnWorkflowTaskItem T(string? name, string? vote = "Approved", DateTime? on = null,
            string? eca = "ECA-1", string? ecaState = "Resolved", string? ecnState = "Resolved") =>
            new() { WfTaskName = name, Vote = vote, WfTaskCompletedOn = on, EcaNumber = eca, EcaState = ecaState, EcnState = ecnState };

        /// <summary>Complete nominal workflow for one ECA, every flag set (no fallback).</summary>
        private static List<EcnWorkflowTaskItem> Nominal(string ecnState = "Resolved", string ecaState = "Resolved") => new()
        {
            T("Complete Change Notice Task", "Approved", D1, ecaState: ecaState, ecnState: ecnState),
            T("Set Up Reviewer Participants", "Approved", D2, ecaState: ecaState, ecnState: ecnState),
            T("Review Change Notice Task", "Approved", D3, ecaState: ecaState, ecnState: ecnState),
            T("Quality Check Approval", "Approved", D4, ecaState: ecaState, ecnState: ecnState),
            T("Audit Change Notice", "Approved", D5, ecaState: ecaState, ecnState: ecnState),
        };

        // ---------- A. Baseline ----------

        [Fact]
        public void H0_EmptyList_NotFoundInPdm_AllNull()
        {
            var r = _calc.Calculate(Array.Empty<EcnWorkflowTaskItem>());
            Assert.False(r.IsEcnFoundInPdm);
            AssertAllNull(r);
        }

        [Fact]
        public void NullList_Throws() => Assert.Throws<ArgumentNullException>(() => _calc.Calculate(null!));

        [Fact]
        public void NullElement_Throws() => Assert.Throws<ArgumentException>(() => _calc.Calculate(new EcnWorkflowTaskItem[] { null! }));

        [Fact]
        public void NoCompleteChangeNoticeTask_OpenEcn_AllNull()
        {
            // H2 false, F0 false (ECN not resolved).
            var r = _calc.Calculate(new[] { T("Set Up Reviewer Participants", on: D1, ecnState: "Open") });
            Assert.True(r.IsEcnFoundInPdm);
            AssertAllNull(r);
        }

        [Fact]
        public void TasksWithoutQualifyingTask_OpenEcn_AllNull()
        {
            var r = _calc.Calculate(new[] { T("Other", on: D1, ecnState: "Open"), T("Complete Change Notice Task", "In progress", ecnState: "Open") });
            AssertAllNull(r);
        }

        // ---------- B. Submit Change Notice ----------

        [Fact]
        public void H1_SubmitInProgress_AllNull_EvenWithValidApprovals()
        {
            var tasks = Nominal();
            tasks.Add(T("Submit Change Notice", "In progress"));
            var r = _calc.Calculate(tasks);
            Assert.True(r.IsSubmitChangeNoticeInProgress);
            AssertAllNull(r);
        }

        [Theory]
        [InlineData("submit change notice", "In progress")]
        [InlineData("Submit Change Notice ", "In progress")]
        [InlineData("Submit Change Notice", "in progress")]
        [InlineData("Submit Change Notice", "Approved")]
        public void H1_SimilarNameOrVote_DoesNotTrigger(string name, string vote)
        {
            var tasks = Nominal();
            tasks.Add(T(name, vote));
            var r = _calc.Calculate(tasks);
            Assert.False(r.IsSubmitChangeNoticeInProgress);
            Assert.Equal(D5, r.CaiiiApprovalDate);
        }

        // ---------- Nominal ----------

        [Fact]
        public void Nominal_AllFourDates()
        {
            var r = _calc.Calculate(Nominal());
            Assert.Equal(D2, r.DesignerStartAppDate);
            Assert.Equal(D3, r.FirstApprovalDate);
            Assert.Equal(D4, r.QualCheckApprovalDate);
            Assert.Equal(D5, r.CaiiiApprovalDate);
        }

        // ---------- C. Designer start ----------

        [Fact]
        public void H3_DesignerIsMinOfApprovedSetUpReviewer_AcrossEcas()
        {
            var tasks = Nominal();
            tasks.Add(T("Set Up Reviewer Participants", "Approved", D1.AddDays(-5), eca: "ECA-OTHER"));
            tasks.Add(T("Set Up Reviewer Participants", "Rework", D1.AddDays(-10)));
            Assert.Equal(D1.AddDays(-5), _calc.Calculate(tasks).DesignerStartAppDate);
        }

        [Fact]
        public void H3_NonApprovedSetUpReviewerOnly_OpenEcn_AllNull()
        {
            var r = _calc.Calculate(new[]
            {
                T("Complete Change Notice Task", "In progress", ecnState: "Open"),
                T("Set Up Reviewer Participants", "In progress", ecnState: "Open"),
            });
            AssertAllNull(r);
        }

        [Fact]
        public void H3_ApprovedSetUpReviewerWithNullDate_FlagDone_DateNull()
        {
            // Historical: ListDate.Count()>0 even if dates are null -> isStartAppDone=true, Min()=null; no fallback for designer.
            var tasks = Nominal();
            tasks[1] = tasks[1] with { WfTaskCompletedOn = null };
            var r = _calc.Calculate(tasks);
            Assert.Null(r.DesignerStartAppDate);
            Assert.Equal(D5, r.CaiiiApprovalDate);
        }

        [Fact]
        public void H3_MinIgnoresNullDates()
        {
            var tasks = Nominal();
            tasks.Add(T("Set Up Reviewer Participants", "Approved", null));
            Assert.Equal(D2, _calc.Calculate(tasks).DesignerStartAppDate);
        }

        // ---------- D. First approval ----------

        [Fact]
        public void H6_FirstIsMaxOfApprovedReviews()
        {
            var tasks = Nominal();
            tasks.Add(T("Review Change Notice Task", "Approved", D3.AddDays(2), eca: "ANY"));
            tasks.Add(T("Review Change Notice Task", "Rework", D6));
            Assert.Equal(D3.AddDays(2), _calc.Calculate(tasks).FirstApprovalDate);
        }

        [Fact]
        public void H4_MissingSetUpReviewerOnOneEca_OpenEcn_OnlyDesigner()
        {
            // ECA-2 has no approved Set Up Reviewer -> H4 false; ECN open -> no fallback.
            var tasks = Nominal("Open");
            tasks.Add(T("Complete Change Notice Task", "Approved", D1, eca: "ECA-2", ecnState: "Open"));
            tasks.Add(T("Quality Check Approval", "Approved", D4, eca: "ECA-2", ecnState: "Open"));
            var r = _calc.Calculate(tasks);
            Assert.Equal(D2, r.DesignerStartAppDate);
            Assert.Null(r.FirstApprovalDate);
            Assert.Null(r.QualCheckApprovalDate);
            Assert.Null(r.CaiiiApprovalDate);
        }

        [Fact]
        public void H4_SetUpReviewerOnOtherEcaDoesNotSatisfyPrerequisite()
        {
            var tasks = Nominal("Open");
            tasks.Add(T("Complete Change Notice Task", "Approved", D1, eca: "ECA-2", ecnState: "Open"));
            tasks.Add(T("Set Up Reviewer Participants", "In progress", null, eca: "ECA-2", ecnState: "Open"));
            Assert.Null(_calc.Calculate(tasks).FirstApprovalDate);
        }

        [Fact]
        public void H5_MissingQualityCheckOnOneEca_OpenEcn_OnlyDesigner()
        {
            var tasks = Nominal("Open");
            tasks.Add(T("Complete Change Notice Task", "Approved", D1, eca: "ECA-2", ecnState: "Open"));
            tasks.Add(T("Set Up Reviewer Participants", "Approved", D2, eca: "ECA-2", ecnState: "Open"));
            var r = _calc.Calculate(tasks);
            Assert.Equal(D2, r.DesignerStartAppDate);
            Assert.Null(r.FirstApprovalDate);
        }

        [Fact]
        public void H5_QualityCheckInProgress_IsEnoughAsPrerequisite()
        {
            var tasks = Nominal("Open", "Open");
            tasks[3] = tasks[3] with { Vote = "In progress", WfTaskCompletedOn = null };
            var r = _calc.Calculate(tasks);
            Assert.Equal(D3, r.FirstApprovalDate);
            Assert.Null(r.QualCheckApprovalDate);
        }

        [Fact]
        public void H4H5_AllEcasSatisfied_MultiEca()
        {
            var tasks = Nominal();
            tasks.Add(T("Complete Change Notice Task", "Approved", D1, eca: "ECA-2"));
            tasks.Add(T("Set Up Reviewer Participants", "Approved", D2, eca: "ECA-2"));
            tasks.Add(T("Quality Check Approval", "Approved", D6, eca: "ECA-2"));
            var r = _calc.Calculate(tasks);
            Assert.Equal(D3, r.FirstApprovalDate);
            Assert.Equal(D6, r.QualCheckApprovalDate);
        }

        [Fact]
        public void H6_NoApprovedReview_OpenEcn_FirstNull_QualStillComputed()
        {
            var tasks = Nominal("Open");
            tasks.RemoveAt(2);
            var r = _calc.Calculate(tasks);
            Assert.Null(r.FirstApprovalDate);
            Assert.Equal(D4, r.QualCheckApprovalDate);
            Assert.Null(r.CaiiiApprovalDate);
        }

        // ---------- E. Quality check ----------

        [Fact]
        public void H8_QualIsMaxOfApprovedQualityCheck()
        {
            var tasks = Nominal();
            tasks.Add(T("Quality Check Approval", "Approved", D6, eca: "ECA-X"));
            tasks.Add(T("Quality Check Approval", "Rework", D6.AddDays(1)));
            Assert.Equal(D6, _calc.Calculate(tasks).QualCheckApprovalDate);
        }

        [Fact]
        public void H7_OneEcaNotResolved_OpenEcn_NoQualNoCaiii()
        {
            var tasks = Nominal("Open");
            tasks.Add(T("Complete Change Notice Task", "In progress", null, eca: "ECA-2", ecaState: "Open", ecnState: "Open"));
            tasks.Add(T("Set Up Reviewer Participants", "Approved", D2, eca: "ECA-2", ecnState: "Open"));
            tasks.Add(T("Quality Check Approval", "In progress", null, eca: "ECA-2", ecnState: "Open"));
            var r = _calc.Calculate(tasks);
            Assert.Equal(D3, r.FirstApprovalDate);
            Assert.Null(r.QualCheckApprovalDate);
            Assert.Null(r.CaiiiApprovalDate);
        }

        [Fact]
        public void H7_EcaStateComparedOnCompleteChangeNoticeEntryOnly_CaseSensitive()
        {
            var tasks = Nominal("Open", "resolved");
            Assert.Null(_calc.Calculate(tasks).QualCheckApprovalDate);
        }

        [Fact]
        public void H8_QualityCheckWrongVote_ResolvedEca_QualNull_OpenEcn()
        {
            var tasks = Nominal("Open");
            tasks[3] = tasks[3] with { Vote = "Rework" };
            Assert.Null(_calc.Calculate(tasks).QualCheckApprovalDate);
        }

        // ---------- F. CAIII ----------

        [Fact]
        public void H9_EcnNotResolved_CaiiiNull()
        {
            var r = _calc.Calculate(Nominal("Open"));
            Assert.Equal(D4, r.QualCheckApprovalDate);
            Assert.Null(r.CaiiiApprovalDate);
        }

        [Fact]
        public void H10_EcnCanceled_CaiiiIsMaxAudit()
        {
            var tasks = Nominal("Canceled");
            tasks.Add(T("Audit Change Notice", "Approved", D6, ecnState: "Canceled"));
            Assert.Equal(D6, _calc.Calculate(tasks).CaiiiApprovalDate);
        }

        [Fact]
        public void H11_NoApprovedAudit_CaiiiEqualsQual()
        {
            // Historical example B-002302.
            var tasks = Nominal();
            tasks[4] = tasks[4] with { Vote = "In progress" };
            Assert.Equal(D4, _calc.Calculate(tasks).CaiiiApprovalDate);
        }

        [Fact]
        public void H9_EcnStateReadOnFirstCompleteChangeNoticeEntry()
        {
            var tasks = Nominal("Open");
            // Second ECA entry claims Resolved ECN, but the historical rule reads AllEca.FirstOrDefault() (Open).
            tasks.Add(T("Complete Change Notice Task", "Approved", D1, eca: "ECA-1", ecnState: "Resolved"));
            Assert.Null(_calc.Calculate(tasks).CaiiiApprovalDate);
        }

        [Fact]
        public void F0_NotTriggered_WhenAllFlagsDone()
        {
            var r = _calc.Calculate(Nominal());
            Assert.Equal(D5, r.CaiiiApprovalDate);
            Assert.Equal(D2, r.DesignerStartAppDate);
        }

        [Fact]
        public void F0_EcnStateReadOnFirstTask_OpenFirstTaskSkipsFallback()
        {
            // Historical example B-002222 pattern but tasks[0] not resolved -> no fallback.
            var r = _calc.Calculate(new[]
            {
                T("Other", ecnState: "Open"),
                T("Complete Change Notice Task", "Approved", D1, ecnState: "Resolved"),
            });
            AssertAllNull(r);
        }

        [Fact]
        public void F1_F4_F6_F8_ResolvedWithoutSetUpReviewer_UsesCompleteChangeNotice()
        {
            // Historical example B-002222: ECN resolved, tasks missing.
            var r = _calc.Calculate(new[]
            {
                T("Complete Change Notice Task", "Approved", D2),
                T("Complete Change Notice Task", "Approved", D3, eca: "ECA-2"),
            });
            Assert.Equal(D2, r.DesignerStartAppDate);   // F1 Min
            Assert.Equal(D3, r.FirstApprovalDate);      // F4 Max
            Assert.Equal(D3, r.QualCheckApprovalDate);  // F6
            Assert.Equal(D3, r.CaiiiApprovalDate);      // F8
        }

        [Fact]
        public void F2_F5_F7_NoCompleteApproved_DesignerFromAudit()
        {
            var r = _calc.Calculate(new[]
            {
                T("Audit Change Notice", "Approved", D5, ecnState: "Canceled"),
                T("Audit Change Notice", "Approved", D6, ecnState: "Canceled"),
            });
            Assert.Equal(D6, r.DesignerStartAppDate);   // F2 Max audit
            Assert.Equal(D6, r.FirstApprovalDate);      // F5 = designer
            Assert.Equal(D6, r.QualCheckApprovalDate);  // F6
            Assert.Equal(D6, r.CaiiiApprovalDate);      // F7 Max audit
        }

        [Fact]
        public void F3_F5_F8_NothingApproved_AllNull()
        {
            var r = _calc.Calculate(new[] { T("Other", ecnState: "Resolved") });
            Assert.True(r.IsEcnFoundInPdm);
            AssertAllNull(r);
        }

        [Fact]
        public void F4_KeepsDesignerFromH3_FirstFromCompleteMax()
        {
            // H3 done, H5 fails (no QC) -> fallback: designer kept, first = Max(Complete approved).
            var r = _calc.Calculate(new[]
            {
                T("Complete Change Notice Task", "Approved", D1),
                T("Complete Change Notice Task", "Approved", D6, eca: "ECA-2"),
                T("Set Up Reviewer Participants", "Approved", D2),
                T("Set Up Reviewer Participants", "Approved", D3, eca: "ECA-2"),
                T("Audit Change Notice", "Approved", D5),
            });
            Assert.Equal(D2, r.DesignerStartAppDate);
            Assert.Equal(D6, r.FirstApprovalDate);
            Assert.Equal(D6, r.QualCheckApprovalDate);
            Assert.Equal(D5, r.CaiiiApprovalDate); // F7
        }

        [Fact]
        public void F6_FirstDoneQualMissing_QualEqualsFirst()
        {
            var tasks = Nominal();
            tasks[3] = tasks[3] with { Vote = "In progress" };
            var r = _calc.Calculate(tasks);
            Assert.Equal(D3, r.FirstApprovalDate);
            Assert.Equal(D3, r.QualCheckApprovalDate);
            // H11 gave caiii=null(qual) with isCaiiiAppDone=true -> F8 overrides with qual (= first).
            Assert.Equal(D3, r.CaiiiApprovalDate);
        }

        [Fact]
        public void F8_Anomaly_CaiiiAlreadyDone_IsOverriddenByQual()
        {
            // Historical anomaly: CAIII computed from Audit in H10, but First missing triggers F0,
            // and the "else" branch replaces CAIII by Qual.
            var tasks = Nominal();
            tasks.RemoveAt(2); // no Review Change Notice Task
            var r = _calc.Calculate(tasks);
            Assert.Equal(D1, r.FirstApprovalDate);        // F4 Max(Complete)
            Assert.Equal(D4, r.QualCheckApprovalDate);    // H8 kept
            Assert.Equal(D4, r.CaiiiApprovalDate);        // F8 override (not D5)
        }

        [Fact]
        public void F7_CaiiiNotDone_EcaNotResolved_UsesAudit()
        {
            var tasks = Nominal(ecaState: "Open");
            var r = _calc.Calculate(tasks);
            Assert.Equal(D3, r.FirstApprovalDate);
            Assert.Equal(D3, r.QualCheckApprovalDate);  // F6
            Assert.Equal(D5, r.CaiiiApprovalDate);      // F7
        }

        // ---------- H. Nulls / duplicates / precision / determinism ----------

        [Fact]
        public void NullFields_DoNotThrow()
        {
            var r = _calc.Calculate(new[] { new EcnWorkflowTaskItem(), T(null, null, null, null, null, null) });
            AssertAllNull(r);
        }

        [Fact]
        public void DuplicateCompleteChangeNoticeEntries_AreNotDeduplicated_ButNeutral()
        {
            var tasks = Nominal();
            tasks.Add(tasks[0] with { });
            var r = _calc.Calculate(tasks);
            Assert.Equal(D5, r.CaiiiApprovalDate);
        }

        [Fact]
        public void TimePrecision_IsKept_ForMinMax()
        {
            var tasks = Nominal();
            tasks.Add(T("Review Change Notice Task", "Approved", D3.AddHours(5)));
            tasks.Add(T("Set Up Reviewer Participants", "Approved", D2.AddHours(-1)));
            var r = _calc.Calculate(tasks);
            Assert.Equal(D3.AddHours(5), r.FirstApprovalDate);
            Assert.Equal(D2.AddHours(-1), r.DesignerStartAppDate);
        }

        [Fact]
        public void Deterministic_NoMutation_NoState()
        {
            var tasks = Nominal();
            var snapshot = tasks.ToList();
            var r1 = _calc.Calculate(tasks);
            var r2 = _calc.Calculate(tasks);
            Assert.Equal(r1, r2);
            Assert.Equal(snapshot, tasks);
        }

        [Fact]
        public void DependencyInjection_ResolvesSingleton()
        {
            var sc = new ServiceCollection();
            sc.AddSingleton<IEcnWorkflowDateCalculator, EcnWorkflowDateCalculator>();
            using var sp = sc.BuildServiceProvider(true);
            Assert.Same(sp.GetRequiredService<IEcnWorkflowDateCalculator>(), sp.GetRequiredService<IEcnWorkflowDateCalculator>());
        }

        [Fact]
        public void SourceCode_HasNoInfrastructureDependency()
        {
            var d = new DirectoryInfo(AppContext.BaseDirectory);
            while (d != null && !File.Exists(Path.Combine(d.FullName, "MCG.Tools.EcnEcoFollowUp", "Services", "Workflow", "EcnWorkflowDateCalculator.cs")))
                d = d.Parent;
            Assert.NotNull(d);
            var text = File.ReadAllText(Path.Combine(d!.FullName, "MCG.Tools.EcnEcoFollowUp", "Services", "Workflow", "EcnWorkflowDateCalculator.cs"));
            foreach (var forbidden in new[] { "DbContext", "NetworkCredential", "IWindchill", "ISap", "TraceLog", "ILogger", "IConfiguration",
                         "Environment.", "DateTime.Now", "DateTime.Today", "TimeProvider", "MessageBox", "ShowDialog", "Dispatcher", "ViewModel", "ICommand", "Task.Run" })
                Assert.DoesNotContain(forbidden, text);
        }

        private static void AssertAllNull(EcnWorkflowDateResult r)
        {
            Assert.Null(r.DesignerStartAppDate);
            Assert.Null(r.FirstApprovalDate);
            Assert.Null(r.QualCheckApprovalDate);
            Assert.Null(r.CaiiiApprovalDate);
        }
    }
}
