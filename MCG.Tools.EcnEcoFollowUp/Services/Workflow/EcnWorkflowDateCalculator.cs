using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.Workflow;

namespace MCG.Tools.EcnEcoFollowUp.Services.Workflow
{
    /// <summary>
    /// Branch-by-branch reconstruction of EcnEcoFollowUpManagement.ExtractPdmWfTaskDate (.NET Framework).
    /// All literals are compared with ordinal, case-sensitive equality as in the historical "==".
    /// Stateless: safe as a singleton.
    /// </summary>
    public sealed class EcnWorkflowDateCalculator : IEcnWorkflowDateCalculator
    {
        internal const string SubmitChangeNotice = "Submit Change Notice";
        internal const string CompleteChangeNoticeTask = "Complete Change Notice Task";
        internal const string SetUpReviewerParticipants = "Set Up Reviewer Participants";
        internal const string QualityCheckApproval = "Quality Check Approval";
        internal const string ReviewChangeNoticeTask = "Review Change Notice Task";
        internal const string AuditChangeNotice = "Audit Change Notice";
        internal const string VoteInProgress = "In progress";
        internal const string VoteApproved = "Approved";
        internal const string StateResolved = "Resolved";
        internal const string StateCanceled = "Canceled";

        public EcnWorkflowDateResult Calculate(IReadOnlyList<EcnWorkflowTaskItem> tasks)
        {
            ArgumentNullException.ThrowIfNull(tasks);
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i] is null)
                    throw new ArgumentException($"Workflow task at index {i} is null.", nameof(tasks));
            }

            // H0 : empty list -> ECN_NOT_IN_PDM, all dates null.
            if (tasks.Count == 0)
                return new EcnWorkflowDateResult { IsEcnFoundInPdm = false };

            // H1 : any "Submit Change Notice" / "In progress" -> all dates null.
            if (tasks.Any(t => Eq(t.WfTaskName, SubmitChangeNotice) && Eq(t.Vote, VoteInProgress)))
                return new EcnWorkflowDateResult { IsEcnFoundInPdm = true, IsSubmitChangeNoticeInProgress = true };

            DateTime? designer = null, first = null, qual = null, caiii = null;
            bool isStartAppDone = false, isFirstAppDone = false, isQualCheckAppDone = false, isCaiiiAppDone = false;

            // H2 : "ECA" entries = every "Complete Change Notice Task" (historical Distinct is reference-based: no deduplication).
            var allEca = tasks.Where(t => Eq(t.WfTaskName, CompleteChangeNoticeTask)).ToList();

            if (allEca.Count > 0)
            {
                // H3 : Designer = Min(approved Set Up Reviewer Participants), flag set even when every date is null.
                var setUpDates = ApprovedDates(tasks, SetUpReviewerParticipants);
                if (setUpDates.Count > 0)
                {
                    designer = setUpDates.Min();
                    isStartAppDone = true;

                    // H4 : every ECA entry must have an approved Set Up Reviewer Participants with the same EcaNumber.
                    bool allSetUpReviewerCompleted = allEca.All(eca => tasks.Any(t =>
                        Eq(t.EcaNumber, eca.EcaNumber) && Eq(t.WfTaskName, SetUpReviewerParticipants) && Eq(t.Vote, VoteApproved)));

                    if (allSetUpReviewerCompleted)
                    {
                        // H5 : every ECA entry must have a Quality Check Approval task (any vote).
                        bool allQualityCheckStarted = allEca.All(eca => tasks.Any(t =>
                            Eq(t.EcaNumber, eca.EcaNumber) && Eq(t.WfTaskName, QualityCheckApproval)));

                        if (allQualityCheckStarted)
                        {
                            // H6 : First = Max(approved Review Change Notice Task).
                            var reviewDates = ApprovedDates(tasks, ReviewChangeNoticeTask);
                            if (reviewDates.Count > 0)
                            {
                                first = reviewDates.Max();
                                isFirstAppDone = true;
                            }

                            // H7 : every ECA entry in state "Resolved".
                            if (allEca.All(eca => Eq(eca.EcaState, StateResolved)))
                            {
                                // H8 : Qual = Max(approved Quality Check Approval).
                                var qcDates = ApprovedDates(tasks, QualityCheckApproval);
                                if (qcDates.Count > 0)
                                {
                                    qual = qcDates.Max();
                                    isQualCheckAppDone = true;
                                }

                                // H9 : ECN state read on the FIRST "Complete Change Notice Task" entry.
                                if (IsResolvedOrCanceled(allEca[0].EcnState))
                                {
                                    // H10 / H11 : Max(approved Audit Change Notice), otherwise Qual.
                                    var auditDates = ApprovedDates(tasks, AuditChangeNotice);
                                    caiii = auditDates.Count > 0 ? auditDates.Max() : qual;
                                    isCaiiiAppDone = true;
                                }
                            }
                        }
                    }
                }
            }

            // F0 : fallback when the FIRST task's ECN state is Resolved/Canceled and at least one date flag is missing.
            if (IsResolvedOrCanceled(tasks[0].EcnState)
                && (!isCaiiiAppDone || !isFirstAppDone || !isQualCheckAppDone || !isStartAppDone))
            {
                var auditDates = ApprovedDates(tasks, AuditChangeNotice);
                var completeDates = ApprovedDates(tasks, CompleteChangeNoticeTask);

                // F1 / F2 / F3
                if (!isStartAppDone)
                {
                    if (completeDates.Count > 0)
                        designer = completeDates.Min();
                    else if (auditDates.Count > 0)
                        designer = auditDates.Max();
                }

                // F4 / F5
                if (!isFirstAppDone)
                    first = completeDates.Count > 0 ? completeDates.Max() : designer;

                // F6
                if (!isQualCheckAppDone)
                    qual = first;

                // F7 / F8 : historical "else" also overrides a CAIII date already computed in H10/H11.
                caiii = (!isCaiiiAppDone && auditDates.Count > 0) ? auditDates.Max() : qual;
            }

            return new EcnWorkflowDateResult
            {
                IsEcnFoundInPdm = true,
                DesignerStartAppDate = designer,
                FirstApprovalDate = first,
                QualCheckApprovalDate = qual,
                CaiiiApprovalDate = caiii
            };
        }

        private static List<DateTime?> ApprovedDates(IReadOnlyList<EcnWorkflowTaskItem> tasks, string taskName) =>
            tasks.Where(t => Eq(t.WfTaskName, taskName) && Eq(t.Vote, VoteApproved)).Select(t => t.WfTaskCompletedOn).ToList();

        private static bool IsResolvedOrCanceled(string? state) => Eq(state, StateResolved) || Eq(state, StateCanceled);

        private static bool Eq(string? a, string? b) => string.Equals(a, b, StringComparison.Ordinal);
    }
}
