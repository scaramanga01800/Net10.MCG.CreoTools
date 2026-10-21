using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.SapHupDb;
using MCG.CommonLib.DataBaseAccess.Shadow;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.EcoSapHup;

namespace MCG.Tools.EcnEcoFollowUp.Services.EcoSapHup
{
    /// <summary>
    /// Stateless headless SAP HUP ECO synchronization (historical treatments 1 and 5).
    /// </summary>
    public sealed class EcoSapHupSyncService : IEcoSapHupSyncService
    {
        private readonly IEcoSapHupSyncRepository _repository;
        private readonly ISapHupService _sapHupService;
        private readonly IEcoSapHupStepCalculator _stepCalculator;
        private readonly TimeProvider _timeProvider;
        private readonly IOracleShadowRecorder? _shadowRecorder;

        public EcoSapHupSyncService(
            IEcoSapHupSyncRepository repository,
            ISapHupService sapHupService,
            IEcoSapHupStepCalculator stepCalculator,
            TimeProvider timeProvider,
            IOracleShadowRecorder? shadowRecorder = null)
        {
            _repository = repository;
            _sapHupService = sapHupService;
            _stepCalculator = stepCalculator;
            _timeProvider = timeProvider;
            _shadowRecorder = shadowRecorder;
        }

        public EcoSapHupSyncResult Synchronize(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Single time capture for the whole operation.
            DateTime localNow = _timeProvider.GetLocalNow().DateTime;
            DateOnly processingDate = DateOnly.FromDateTime(localNow);
            DateOnly closedAfter = DateOnly.FromDateTime(localNow.AddYears(-1));

            TraceLog.AddTraceLog("Start EcoSapHupSync");

            IReadOnlyList<EcoSapHupCandidate> candidates;
            try
            {
                candidates = _repository.GetCandidates(closedAfter);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new EcoSapHupSyncException("Echec de la selection des ECO a synchroniser avec SAP HUP.", ex);
            }

            var result = new EcoSapHupSyncResult
            {
                CandidatesSelected = candidates.Count,
                RollingYearCandidates = candidates.Count(c => c.IsRollingYearCandidate),
                PendingSapUpdateCandidates = candidates.Count(c => c.IsPendingSapUpdateCandidate)
            };

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var candidate in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ProcessCandidate(candidate, processingDate, result, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }

            TraceLog.AddTraceLog($"End EcoSapHupSync : {result}");
            return result;
        }

        private void ProcessCandidate(EcoSapHupCandidate candidate, DateOnly processingDate, EcoSapHupSyncResult result, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(candidate.EcnNumber))
            {
                result.Skipped++;
                result.AddWarning(new EcoSapHupSyncWarning(candidate.Id, candidate.EcnNumber, EcoSapHupSyncWarningKind.MissingEcoNumber,
                    "Numero ECO absent : enregistrement ignore."));
                return;
            }

            string eco = candidate.EcnNumber;

            // 1. SAP HUP main status row.
            cancellationToken.ThrowIfCancellationRequested();
            List<DmEcoViewStatusDate> mainRows;
            try
            {
                TraceLog.AddTraceLog($"SAP HUP request [EcoViewStatus] start: ECO={eco}, id={candidate.Id}");
                var hupWatch = System.Diagnostics.Stopwatch.StartNew();
                mainRows = _sapHupService.GetEcoViewStatusData(eco);
                TraceLog.AddTraceLog($"SAP HUP request [EcoViewStatus] end: ECO={eco}, rows={mainRows?.Count ?? 0}, duration={hupWatch.ElapsedMilliseconds} ms");
                NotifyShadow(() => _shadowRecorder?.RecordEcoStatus(eco, mainRows, hupWatch.ElapsedMilliseconds));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                AddError(result, candidate, EcoSapHupSyncPhase.MainStatusRetrieval, "Echec de lecture DM_ECO_VIEW_STATUS_DATE.", ex);
                return;
            }

            // Historical: first row read, no ordering.
            var main = mainRows.FirstOrDefault();
            if (main is null)
            {
                result.NotFoundInSapHup++;
                TraceLog.AddTraceLog($"EcoSapHupSync warning phase=MainStatusRetrieval classification=NotFoundInSapHup eco={eco} id={candidate.Id}");
                result.AddWarning(new EcoSapHupSyncWarning(candidate.Id, eco, EcoSapHupSyncWarningKind.NotFoundInSapHup,
                    "ECO absent de SAP HUP : donnees conservees."));
                return;
            }

            result.FoundInSapHup++;

            // Historical: ECO_DESC == "" => "Oracle Search Not Found" => no update.
            if (string.IsNullOrEmpty(main.EcoDesc))
            {
                result.Skipped++;
                result.AddWarning(new EcoSapHupSyncWarning(candidate.Id, eco, EcoSapHupSyncWarningKind.EmptySapHupDescription,
                    "Description SAP HUP vide : donnees conservees."));
                return;
            }

            // 2. Last-change comparison (historical SearchAllEcoFromLastYear), transported explicitly.
            bool lastChangeDiffers = candidate.IsRollingYearCandidate
                && EcoSapHupMapper.LastChangeDiffers(candidate.StoredEcoClosedOn, main.EcoLastChange);
            if (lastChangeDiffers)
                result.LastChangeDifferences++;

            if (!candidate.IsPendingSapUpdateCandidate && !lastChangeDiffers)
            {
                result.RollingYearCandidatesWithoutDifference++;
                return;
            }

            // 3. Workflow tasks.
            cancellationToken.ThrowIfCancellationRequested();
            List<DmEcoViewWfEcoTask> tasks;
            try
            {
                TraceLog.AddTraceLog($"SAP HUP request [EcoWfTask] start: ECO={eco}, id={candidate.Id}");
                var hupTaskWatch = System.Diagnostics.Stopwatch.StartNew();
                tasks = _sapHupService.GetEcoWfTaskData(eco);
                TraceLog.AddTraceLog($"SAP HUP request [EcoWfTask] end: ECO={eco}, rows={tasks?.Count ?? 0}, duration={hupTaskWatch.ElapsedMilliseconds} ms");
                NotifyShadow(() => _shadowRecorder?.RecordEcoWorkflow(eco, tasks, hupTaskWatch.ElapsedMilliseconds));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                AddError(result, candidate, EcoSapHupSyncPhase.WorkflowTaskRetrieval, "Echec de lecture DM_ECO_VIEW_WF_ECO_TASKS.", ex);
                return;
            }

            var steps = _stepCalculator.Calculate(tasks);
            var snapshot = EcoSapHupMapper.CreateSnapshot(main, steps, lastChangeDiffers);

            // 4. Persistence (isolated context per ECO).
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                bool changed = _repository.UpdateEco(candidate.Id, entity => EcoSapHupMapper.Apply(entity, snapshot, processingDate));
                if (changed)
                    result.Updated++;
                else
                    result.Unchanged++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                AddError(result, candidate, EcoSapHupSyncPhase.Persistence, "Echec de mise a jour ECNECOFOLLOWUP.", ex);
            }
        }

        /// <summary>Observation shadow : ne modifie jamais le traitement historique, toute exception est absorbée.</summary>
        private static void NotifyShadow(Action notify)
        {
            try
            {
                notify();
            }
            catch (Exception)
            {
                // Le shadow mode est purement observationnel.
            }
        }

        private static void AddError(EcoSapHupSyncResult result, EcoSapHupCandidate candidate, EcoSapHupSyncPhase phase, string message, Exception ex)
        {
            result.AddError(new EcoSapHupSyncError(candidate.Id, candidate.EcnNumber, phase, message, ex));
            TraceLog.Error(ex, $"EcoSapHupSync item failure: ECO={candidate.EcnNumber}, id={candidate.Id}, phase={phase}");
        }
    }
}
