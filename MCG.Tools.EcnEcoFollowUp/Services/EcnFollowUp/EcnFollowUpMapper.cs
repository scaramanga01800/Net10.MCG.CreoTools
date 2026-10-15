using System.Globalization;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.Models.EcnFollowUp;
using MCG.Tools.EcnEcoFollowUp.Models.Workflow;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;

namespace MCG.Tools.EcnEcoFollowUp.Services.EcnFollowUp
{
    /// <summary>
    /// Pure mapping rules shared by the historical AddUpdatreOneEcnEcoFollowUp (new ECN) and
    /// AddUpdateOneEcnEcoFollowUp (workflow update). No network, no database, no workflow-date rule.
    /// </summary>
    public static class EcnFollowUpMapper
    {
        public const string NoUpdateRequired = "NO UPDATE REQUIRED";
        public const string NotFound = "Not Found";
        public const string EcoStatusNotCreated = "Not Created";
        public const string StateResolved = "Resolved";
        public const string StateCanceled = "Canceled";
        public const string LastPdmEcnSearchFormat = "yyyy-MM-dd";
        public const string PdmUpdateStatusFormat = "dd/MM/yyyy";

        /// <summary>Historical SAP sentinel "01/01/9999", compared as a typed date.</summary>
        public static readonly DateOnly SapSentinelDate = new(9999, 1, 1);

        /// <summary>Historical counters: DRW documents are a subset of EPM documents (overlap preserved).</summary>
        public static ResultingObjectCounts CountResultingObjects(RestOdataChangeNotice changeNotice)
        {
            ArgumentNullException.ThrowIfNull(changeNotice);
            return new ResultingObjectCounts(
                NbPart: changeNotice.ListWtPart.Count,
                NbEpmDoc: changeNotice.ListEpmDocument.Count,
                NbWtDoc: changeNotice.ListWtDocument.Count,
                NbDrw: changeNotice.ListEpmDocument.Count(item => item.ObjectWindchillSubType == WindchillObjectType.DRW));
        }

        public static EcnChangeNoticeSnapshot ToSnapshot(string ecnNumber, WindchillChangeNotice changeNotice, ResultingObjectCounts counts)
        {
            ArgumentNullException.ThrowIfNull(changeNotice);
            ArgumentNullException.ThrowIfNull(counts);
            return new EcnChangeNoticeSnapshot
            {
                EcnNumber = ecnNumber,
                Name = changeNotice.Name,
                State = changeNotice.State,
                CreatedBy = changeNotice.CreatedBy,
                Description = changeNotice.Description,
                PdmEcnId = changeNotice.Id,
                PdmContext = changeNotice.Context.Name,
                CreatedOn = DateOnly.FromDateTime(changeNotice.CreatedOn),
                Counts = counts
            };
        }

        public static string? ResolvePdmProduct(PdmContextLookup? pdm) => pdm is null ? NotFound : pdm.Product;

        public static string? ResolveCaiiiName(PdmContextLookup? pdm) => pdm is null ? NotFound : pdm.ParticipantId;

        /// <summary>Explicit typed conversion of a calculator date to the DateOnly? column.</summary>
        public static DateOnly? ToDateOnly(DateTime? value) => value.HasValue ? DateOnly.FromDateTime(value.Value) : null;

        public static bool IsSapSentinel(DateOnly? value) => value.HasValue && value.Value == SapSentinelDate;

        /// <summary>
        /// New row of the historical AddUpdatreOneEcnEcoFollowUp: common metadata, Eco_Status "Not Created",
        /// workflow and ECO dates null, PDM_UPDATE_STATUS null (the historical copy was added before the status was set).
        /// </summary>
        public static Ecnecofollowup CreateNewRow(int id, EcnChangeNoticeSnapshot snapshot, PdmContextLookup? pdm)
        {
            ArgumentNullException.ThrowIfNull(snapshot);
            return new Ecnecofollowup
            {
                Id = id,
                EcnNumber = snapshot.EcnNumber,
                EcnName = snapshot.Name,
                EcnState = snapshot.State,
                EcnCreatedBy = snapshot.CreatedBy,
                EcnDescription = snapshot.Description,
                PdmEcnId = snapshot.PdmEcnId,
                PdmContext = snapshot.PdmContext,
                PdmProduct = ResolvePdmProduct(pdm),
                CaiiiName = ResolveCaiiiName(pdm),
                EcnCreatedOn = snapshot.CreatedOn,
                EcoStatus = EcoStatusNotCreated,
                NbPart = snapshot.Counts.NbPart,
                NbEpmDoc = snapshot.Counts.NbEpmDoc,
                NbWtDoc = snapshot.Counts.NbWtDoc,
                NbDrw = snapshot.Counts.NbDrw,
                DesignerStartAppDate = null,
                FirstApprovalDate = null,
                QualCheckApprovalDate = null,
                CaiiiApprovalDate = null,
                EcoCreatedOn = null,
                EcoWfStartedOn = null,
                EcoEffectivityDate = null,
                PdmUpdateStatus = null
            };
        }

        /// <summary>
        /// Existing-row update of the historical AddUpdateOneEcnEcoFollowUp. The four workflow dates are always
        /// overwritten (null included). PdmEcnId and EcoStatus are intentionally not refreshed (historical).
        /// SAP sentinel dates are nulled on their own column (intentional correction of the ECN_CREATED_ON target).
        /// Returns true when PDM_UPDATE_STATUS became "NO UPDATE REQUIRED".
        /// </summary>
        public static bool ApplyWorkflowUpdate(
            Ecnecofollowup row,
            EcnChangeNoticeSnapshot snapshot,
            PdmContextLookup? pdm,
            EcnWorkflowDateResult workflow,
            DateOnly processingDate)
        {
            ArgumentNullException.ThrowIfNull(row);
            ArgumentNullException.ThrowIfNull(snapshot);
            ArgumentNullException.ThrowIfNull(workflow);

            row.DesignerStartAppDate = ToDateOnly(workflow.DesignerStartAppDate);
            row.CaiiiApprovalDate = ToDateOnly(workflow.CaiiiApprovalDate);
            row.FirstApprovalDate = ToDateOnly(workflow.FirstApprovalDate);
            row.QualCheckApprovalDate = ToDateOnly(workflow.QualCheckApprovalDate);
            row.PdmContext = snapshot.PdmContext;
            row.PdmProduct = ResolvePdmProduct(pdm);
            row.CaiiiName = ResolveCaiiiName(pdm);
            row.EcnName = snapshot.Name;
            row.EcnDescription = snapshot.Description;
            row.NbPart = snapshot.Counts.NbPart;
            row.NbEpmDoc = snapshot.Counts.NbEpmDoc;
            row.NbDrw = snapshot.Counts.NbDrw;
            row.NbWtDoc = snapshot.Counts.NbWtDoc;
            row.EcnState = snapshot.State;
            row.EcnCreatedOn = snapshot.CreatedOn;
            row.EcnCreatedBy = snapshot.CreatedBy;

            NormalizeSapSentinelDates(row);

            bool noUpdateRequired = IsNoUpdateRequired(snapshot.State, workflow);
            row.PdmUpdateStatus = noUpdateRequired
                ? NoUpdateRequired
                : FormatPdmUpdateStatus(processingDate);
            return noUpdateRequired;
        }

        /// <summary>Historical rule: state Resolved or Canceled (ordinal) and the four workflow dates populated.</summary>
        public static bool IsNoUpdateRequired(string? ecnState, EcnWorkflowDateResult workflow)
        {
            ArgumentNullException.ThrowIfNull(workflow);
            bool closed = string.Equals(ecnState, StateResolved, StringComparison.Ordinal)
                       || string.Equals(ecnState, StateCanceled, StringComparison.Ordinal);
            return closed
                && workflow.DesignerStartAppDate.HasValue
                && workflow.CaiiiApprovalDate.HasValue
                && workflow.FirstApprovalDate.HasValue
                && workflow.QualCheckApprovalDate.HasValue;
        }

        /// <summary>Typed replacement of the historical ToString().IndexOf("01/01/9999") == 0 checks.</summary>
        public static void NormalizeSapSentinelDates(Ecnecofollowup row)
        {
            ArgumentNullException.ThrowIfNull(row);
            if (IsSapSentinel(row.EcoCreatedOn))
                row.EcoCreatedOn = null;
            if (IsSapSentinel(row.EcoWfStartedOn))
                row.EcoWfStartedOn = null;
            if (IsSapSentinel(row.EcoClosedOn))
                row.EcoClosedOn = null;
            if (IsSapSentinel(row.EcoEffectivityDate))
                row.EcoEffectivityDate = null;
        }

        public static string FormatPdmUpdateStatus(DateOnly processingDate) =>
            processingDate.ToString(PdmUpdateStatusFormat, CultureInfo.InvariantCulture);

        public static string FormatLastPdmEcnSearch(DateOnly date) =>
            date.ToString(LastPdmEcnSearchFormat, CultureInfo.InvariantCulture);

        public static bool TryParseLastPdmEcnSearch(string? status, out DateOnly date) =>
            DateOnly.TryParseExact(status?.Trim(), LastPdmEcnSearchFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

        /// <summary>
        /// Historical SearchWfTaskOneEcn order: AllEcnWfTask.Union(AllEcaWfTask) — ECN tasks first, then ECA tasks,
        /// reference-equality Union, no sorting. Each task is converted with the existing EFU_EcnEcoWorkflowItem rules.
        /// </summary>
        public static IReadOnlyList<EcnWorkflowTaskItem> MapWorkflowTasks(
            IEnumerable<WindchillObjectWorkflowTask> ecnTasks,
            IEnumerable<WindchillObjectWorkflowTask> ecaTasks)
        {
            ArgumentNullException.ThrowIfNull(ecnTasks);
            ArgumentNullException.ThrowIfNull(ecaTasks);

            var list = new List<EcnWorkflowTaskItem>();
            foreach (var task in ecnTasks.Union(ecaTasks))
            {
                var item = EFU_EcnEcoWorkflowItem.GetEFU_EcnEcoWorkflowItem(task)
                    ?? throw new InvalidOperationException("Conversion d'une tache workflow Windchill impossible.");
                list.Add(new EcnWorkflowTaskItem
                {
                    WfTaskName = item.WfTaskName,
                    Vote = item.Vote,
                    WfTaskCompletedOn = item.WfTaskCompletedOn,
                    EcaNumber = item.EcaNumber,
                    EcaState = item.EcaSate,
                    EcnState = item.EcnState
                });
            }
            return list;
        }

        /// <summary>Ordinal deduplication preserving first-occurrence order. Null/blank numbers are reported separately.</summary>
        public static IReadOnlyList<string> Deduplicate(IEnumerable<string?> numbers, out int duplicates, out int invalid)
        {
            ArgumentNullException.ThrowIfNull(numbers);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var unique = new List<string>();
            duplicates = 0;
            invalid = 0;
            foreach (var number in numbers)
            {
                if (string.IsNullOrWhiteSpace(number))
                {
                    invalid++;
                    continue;
                }
                if (seen.Add(number))
                    unique.Add(number);
                else
                    duplicates++;
            }
            return unique;
        }
    }
}
