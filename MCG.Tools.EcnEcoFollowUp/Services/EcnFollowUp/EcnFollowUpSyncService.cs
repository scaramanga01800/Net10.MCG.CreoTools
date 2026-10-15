using System.Net;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.EcnFollowUp;
using MCG.Tools.EcnEcoFollowUp.Models.Workflow;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.Services.Interfaces;

namespace MCG.Tools.EcnEcoFollowUp.Services.EcnFollowUp
{
    /// <summary>
    /// Headless reconstruction of BatchModeAddAllNewEcn (phase A) then BatchModeUpdateEcnWfDate (phase B).
    /// Stateless: every candidate list, credential and result is local to one Synchronize call.
    /// Workflow dates are calculated exclusively by IEcnWorkflowDateCalculator.
    /// </summary>
    public sealed class EcnFollowUpSyncService : IEcnFollowUpSyncService
    {
        /// <summary>Historical number filter of GetQueryBuilderEcnCreatedFromDate.</summary>
        public const string EcnNumberFilter = "B-*";

        private readonly IWindchillBatchCredentialProvider _credentialProvider;
        private readonly IWindchillReportingManagementService _reporting;
        private readonly IWindchillChangeManagementService _changeManagement;
        private readonly IWindchillRequestMiscService _misc;
        private readonly IEcnWorkflowDateCalculator _calculator;
        private readonly IEcnFollowUpSyncRepository _repository;
        private readonly TimeProvider _timeProvider;

        public EcnFollowUpSyncService(
            IWindchillBatchCredentialProvider credentialProvider,
            IWindchillReportingManagementService reporting,
            IWindchillChangeManagementService changeManagement,
            IWindchillRequestMiscService misc,
            IEcnWorkflowDateCalculator calculator,
            IEcnFollowUpSyncRepository repository,
            TimeProvider timeProvider)
        {
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _reporting = reporting ?? throw new ArgumentNullException(nameof(reporting));
            _changeManagement = changeManagement ?? throw new ArgumentNullException(nameof(changeManagement));
            _misc = misc ?? throw new ArgumentNullException(nameof(misc));
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public EcnFollowUpSyncResult Synchronize(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TraceLog.AddTraceLog("Start EcnFollowUpSync");
            var result = new EcnFollowUpSyncResult();
            DateOnly processingDate = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);

            try
            {
                NetworkCredential credential = _credentialProvider.GetCredential();
                cancellationToken.ThrowIfCancellationRequested();

                RunNewEcnDiscovery(credential, processingDate, result, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                RunWorkflowUpdate(credential, processingDate, result, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                TraceLog.AddTraceLog($"End EcnFollowUpSync : {result}");
                return result;
            }
            catch (OperationCanceledException)
            {
                TraceLog.AddTraceLog("EcnFollowUpSync cancelled");
                throw;
            }
            catch (Exception ex)
            {
                TraceLog.AddTraceLog($"EcnFollowUpSync global failure : {ex.GetType().Name} - {ex.Message}");
                throw;
            }
        }

        #region Phase A - BatchModeAddAllNewEcn

        private void RunNewEcnDiscovery(NetworkCredential credential, DateOnly processingDate, EcnFollowUpSyncResult result, CancellationToken cancellationToken)
        {
            TraceLog.AddTraceLog("Start EcnFollowUpSync new ECN discovery");

            cancellationToken.ThrowIfCancellationRequested();
            LastPdmEcnSearchMarker marker;
            try
            {
                marker = _repository.ReadLastPdmEcnSearch(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new EcnFollowUpSyncException("Echec de lecture du marqueur LASTPDMECNSEARCH.", ex);
            }

            if (!marker.RowExists)
                throw new EcnFollowUpSyncException("Ligne ECNECOFOLLOWUP_UPDATE 'LASTPDMECNSEARCH' introuvable : decouverte des nouveaux ECN impossible.");
            if (string.IsNullOrWhiteSpace(marker.Status))
                throw new EcnFollowUpSyncException("Marqueur LASTPDMECNSEARCH vide : decouverte des nouveaux ECN impossible.");
            if (!EcnFollowUpMapper.TryParseLastPdmEcnSearch(marker.Status, out DateOnly fromDate))
                throw new EcnFollowUpSyncException($"Marqueur LASTPDMECNSEARCH invalide (format attendu {EcnFollowUpMapper.LastPdmEcnSearchFormat}).");

            result.DiscoveryStartDate = fromDate;
            TraceLog.AddTraceLog($"EcnFollowUpSync discovery from {EcnFollowUpMapper.FormatLastPdmEcnSearch(fromDate)}");

            cancellationToken.ThrowIfCancellationRequested();
            List<WindchillChangeNotice>? discovered;
            try
            {
                discovered = _reporting.GetQueryBuilderEcnCreatedFromDate<WindchillChangeNotice>(
                    credential, EcnFollowUpMapper.FormatLastPdmEcnSearch(fromDate), EcnNumberFilter);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new EcnFollowUpSyncException("Echec de la recherche Windchill des ECN crees depuis LASTPDMECNSEARCH.", ex);
            }
            cancellationToken.ThrowIfCancellationRequested();

            discovered ??= new List<WindchillChangeNotice>();
            result.EcnsReturnedByWindchill = discovered.Count;
            var candidates = EcnFollowUpMapper.Deduplicate(discovered.Select(e => e?.Number), out int duplicates, out int invalid);
            result.DuplicateCandidates = duplicates;
            result.InvalidCandidates = invalid;
            if (invalid > 0)
                result.AddWarning($"{invalid} ECN retourne(s) par Windchill sans numero ignore(s).");
            TraceLog.AddTraceLog($"EcnFollowUpSync new ECN candidates={candidates.Count} (returned={discovered.Count}, duplicates={duplicates})");

            int? nextId = null;
            foreach (var ecnNumber in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ProcessNewEcn(ecnNumber, credential, result, ref nextId, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (result.NewEcnFailures > 0)
            {
                result.AddWarning("LASTPDMECNSEARCH non avance : au moins un nouvel ECN a echoue.");
                TraceLog.AddTraceLog($"EcnFollowUpSync LASTPDMECNSEARCH not updated ({result.NewEcnFailures} failure(s))");
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            string newValue = EcnFollowUpMapper.FormatLastPdmEcnSearch(processingDate);
            try
            {
                _repository.WriteLastPdmEcnSearch(newValue, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new EcnFollowUpSyncException("Echec de mise a jour du marqueur LASTPDMECNSEARCH.", ex);
            }
            result.LastPdmEcnSearchUpdated = true;
            result.LastPdmEcnSearchNewValue = newValue;
            TraceLog.AddTraceLog($"EcnFollowUpSync LASTPDMECNSEARCH updated to {newValue}; inserted={result.EcnsInserted}, existing={result.EcnsAlreadyExisting}");
        }

        private void ProcessNewEcn(string ecnNumber, NetworkCredential credential, EcnFollowUpSyncResult result, ref int? nextId, CancellationToken cancellationToken)
        {
            bool exists;
            try
            {
                exists = _repository.EcnExists(ecnNumber, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                RecordNewEcnFailure(result, ecnNumber, EcnFollowUpSyncPhase.NewEcnPersistence, "Lecture de l'ECN en base impossible.", ex);
                return;
            }

            // Historical AddUpdatreOneEcnEcoFollowUp only modified a detached copy of an existing row:
            // existing ECNs are left unchanged by phase A (phase B refreshes them).
            if (exists)
            {
                result.EcnsAlreadyExisting++;
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            EcnChangeNoticeSnapshot? snapshot;
            try
            {
                snapshot = RetrieveChangeNotice(ecnNumber, credential);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                RecordNewEcnFailure(result, ecnNumber, EcnFollowUpSyncPhase.NewEcnChangeNoticeRetrieval, "Recuperation du Change Notice impossible.", ex);
                return;
            }

            if (snapshot is null)
            {
                result.NewEcnsNotFoundInWindchill++;
                result.AddWarning($"ECN {ecnNumber} retourne par la recherche mais introuvable via GetChangeNoticeWithAllResultingObject (historique 'Not Found').");
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var pdm = _repository.FindPdmContext(snapshot.PdmContext, cancellationToken);
                nextId ??= _repository.GetMaxEcnFollowUpId(cancellationToken) + 1;
                var row = EcnFollowUpMapper.CreateNewRow(nextId.Value, snapshot, pdm);
                cancellationToken.ThrowIfCancellationRequested();
                _repository.InsertEcn(row, cancellationToken);
                nextId++;
                result.EcnsInserted++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                nextId = null; // re-read MAX(ID) after a failed insert
                RecordNewEcnFailure(result, ecnNumber, EcnFollowUpSyncPhase.NewEcnPersistence, "Insertion de l'ECN impossible.", ex);
            }
        }

        private static void RecordNewEcnFailure(EcnFollowUpSyncResult result, string ecnNumber, EcnFollowUpSyncPhase phase, string message, Exception ex)
        {
            result.NewEcnFailures++;
            result.AddError(new EcnFollowUpSyncError(ecnNumber, phase, message, ex));
            TraceLog.AddTraceLog($"EcnFollowUpSync {ecnNumber} ({phase}) : {ex}");
        }

        #endregion

        #region Phase B - BatchModeUpdateEcnWfDate

        private void RunWorkflowUpdate(NetworkCredential credential, DateOnly processingDate, EcnFollowUpSyncResult result, CancellationToken cancellationToken)
        {
            TraceLog.AddTraceLog("Start EcnFollowUpSync workflow update");
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<string?> rawCandidates;
            try
            {
                rawCandidates = _repository.GetWorkflowCandidateEcnNumbers(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new EcnFollowUpSyncException("Echec de selection des ECN a mettre a jour (PDM_UPDATE_STATUS).", ex);
            }

            result.WorkflowDatabaseCandidates = rawCandidates.Count;
            var candidates = EcnFollowUpMapper.Deduplicate(rawCandidates, out int duplicates, out int invalid);
            result.WorkflowDuplicateCandidates = duplicates;
            result.WorkflowInvalidCandidates = invalid;
            result.WorkflowDeduplicatedEcns = candidates.Count;
            if (invalid > 0)
                result.AddWarning($"{invalid} ligne(s) ECNECOFOLLOWUP sans ECN_NUMBER ignoree(s).");
            TraceLog.AddTraceLog($"EcnFollowUpSync workflow candidates={candidates.Count} (rows={rawCandidates.Count}, duplicates={duplicates})");

            foreach (var ecnNumber in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ProcessWorkflowEcn(ecnNumber, credential, processingDate, result, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }

            TraceLog.AddTraceLog($"EcnFollowUpSync workflow updated={result.WorkflowEcnsUpdated}, notFound={result.WorkflowEcnsNotFoundInWindchill}, failures={result.WorkflowFailures}");
        }

        private void ProcessWorkflowEcn(string ecnNumber, NetworkCredential credential, DateOnly processingDate, EcnFollowUpSyncResult result, CancellationToken cancellationToken)
        {
            // Historical SearchWfTaskOneEcn: ECA tasks requested first, ECN tasks second, list built as ECN.Union(ECA).
            IReadOnlyList<EcnWorkflowTaskItem> tasks;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var ecaTasks = _reporting.GetQueryBuilderEcaWorkflowTask<WindchillObjectWorkflowTask>(credential, ecnNumber) ?? new List<WindchillObjectWorkflowTask>();
                cancellationToken.ThrowIfCancellationRequested();
                var ecnTasks = _reporting.GetQueryBuilderEcnWorkflowTask<WindchillObjectWorkflowTask>(credential, ecnNumber) ?? new List<WindchillObjectWorkflowTask>();
                tasks = EcnFollowUpMapper.MapWorkflowTasks(ecnTasks, ecaTasks);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                RecordWorkflowFailure(result, ecnNumber, EcnFollowUpSyncPhase.WorkflowTaskRetrieval, "Recuperation des taches workflow impossible.", ex);
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            EcnWorkflowDateResult workflow;
            try
            {
                workflow = _calculator.Calculate(tasks);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                RecordWorkflowFailure(result, ecnNumber, EcnFollowUpSyncPhase.WorkflowCalculation, "Calcul des dates workflow impossible.", ex);
                return;
            }
            result.WorkflowCalculationsPerformed++;
            if (!workflow.IsEcnFoundInPdm)
                result.WorkflowEcnsWithoutWorkflowTasks++;

            cancellationToken.ThrowIfCancellationRequested();
            EcnChangeNoticeSnapshot? snapshot;
            try
            {
                snapshot = RetrieveChangeNotice(ecnNumber, credential);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                RecordWorkflowFailure(result, ecnNumber, EcnFollowUpSyncPhase.WorkflowChangeNoticeRetrieval, "Recuperation du Change Notice impossible.", ex);
                return;
            }

            // Historical ECN_NOT_IN_PDM: the row is left untouched (no field written).
            if (snapshot is null)
            {
                result.WorkflowEcnsNotFoundInWindchill++;
                return;
            }
            result.WorkflowEcnsFoundInWindchill++;

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var pdm = _repository.FindPdmContext(snapshot.PdmContext, cancellationToken);
                bool noUpdateRequired = false;
                cancellationToken.ThrowIfCancellationRequested();
                bool updated = _repository.UpdateExistingEcn(
                    ecnNumber,
                    row => noUpdateRequired = EcnFollowUpMapper.ApplyWorkflowUpdate(row, snapshot, pdm, workflow, processingDate),
                    cancellationToken);

                if (!updated)
                {
                    result.AddWarning($"ECN {ecnNumber} n'existe plus dans ECNECOFOLLOWUP au moment de la mise a jour.");
                    return;
                }
                result.WorkflowEcnsUpdated++;
                if (noUpdateRequired)
                    result.WorkflowEcnsSetNoUpdateRequired++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                RecordWorkflowFailure(result, ecnNumber, EcnFollowUpSyncPhase.WorkflowPersistence, "Mise a jour de l'ECN impossible.", ex);
            }
        }

        private static void RecordWorkflowFailure(EcnFollowUpSyncResult result, string ecnNumber, EcnFollowUpSyncPhase phase, string message, Exception ex)
        {
            result.WorkflowFailures++;
            result.AddError(new EcnFollowUpSyncError(ecnNumber, phase, message, ex));
            TraceLog.AddTraceLog($"EcnFollowUpSync {ecnNumber} ({phase}) : {ex}");
        }

        #endregion

        /// <summary>Historical GetChangeNoticeWithAllResultingObject(credential, number, false); null when value is empty.</summary>
        private EcnChangeNoticeSnapshot? RetrieveChangeNotice(string ecnNumber, NetworkCredential credential)
        {
            RestOdataChangeNoticeUpper? upper = _changeManagement.GetChangeNoticeWithAllResultingObject(credential, ecnNumber, false);
            RestOdataChangeNotice? restEcn = upper?.value?.FirstOrDefault();
            if (restEcn is null)
                return null;

            WindchillChangeNotice changeNotice = _misc.GetWindchillChangeNotice(restEcn);
            return EcnFollowUpMapper.ToSnapshot(ecnNumber, changeNotice, EcnFollowUpMapper.CountResultingObjects(restEcn));
        }
    }
}
