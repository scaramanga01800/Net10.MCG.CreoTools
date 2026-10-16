using MCG.CommonLib.DataBaseAccess.Exceptions;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.PdmContext;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.Services.Interfaces;
using System.Net;

namespace MCG.Tools.EcnEcoFollowUp.Services.PdmContext
{
    /// <summary>
    /// Headless reconstruction of BatchModeUpdatePdmContext. All four Windchill retrievals complete
    /// before any database access. No UI, no interactive credentials, credentials are never logged.
    /// </summary>
    public sealed class PdmContextSyncService : IPdmContextSyncService
    {
        private readonly IWindchillBatchCredentialProvider _credentialProvider;
        private readonly IWindchillReportingManagementService _windchill;
        private readonly IPdmContextSyncRepository _repository;

        public PdmContextSyncService(
            IWindchillBatchCredentialProvider credentialProvider,
            IWindchillReportingManagementService windchill,
            IPdmContextSyncRepository repository)
        {
            _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            _windchill = windchill ?? throw new ArgumentNullException(nameof(windchill));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public PdmContextSyncResult Synchronize(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TraceLog.AddTraceLog("Start PdmContextSync");

            try
            {
                NetworkCredential credential;
                try
                {
                    credential = _credentialProvider.GetCredential();
                }
                catch (Exception ex) when (ex is not OperationCanceledException and not WindchillBatchCredentialConfigurationException)
                {
                    throw new PdmContextSyncException("Echec de la resolution des identifiants Windchill batch.", ex);
                }
                cancellationToken.ThrowIfCancellationRequested();

                var designerProducts = Retrieve(() => _windchill.GetQueryBuilderUserProductFromTeamRole<WindchillContextTeamMember>(credential, PdmContextSyncPlanner.DesignerRole), "Product DESIGNER", cancellationToken);
                var designerLibraries = Retrieve(() => _windchill.GetQueryBuilderUserLibraryFromTeamRole<WindchillContextTeamMember>(credential, PdmContextSyncPlanner.DesignerRole), "Library DESIGNER", cancellationToken);
                var caiiiProducts = Retrieve(() => _windchill.GetQueryBuilderUserProductFromTeamRole<WindchillContextTeamMember>(credential, PdmContextSyncPlanner.ChangeAdministratorRole), "Product CHANGE ADMINISTRATOR III", cancellationToken);
                var caiiiLibraries = Retrieve(() => _windchill.GetQueryBuilderUserLibraryFromTeamRole<WindchillContextTeamMember>(credential, PdmContextSyncPlanner.ChangeAdministratorRole), "Library CHANGE ADMINISTRATOR III", cancellationToken);

                var result = new PdmContextSyncResult
                {
                    DesignerProductAssignments = designerProducts.Count,
                    DesignerLibraryAssignments = designerLibraries.Count,
                    ChangeAdministratorProductAssignments = caiiiProducts.Count,
                    ChangeAdministratorLibraryAssignments = caiiiLibraries.Count
                };

                PdmContextSyncPlan plan;
                try
                {
                    plan = PdmContextSyncPlanner.Build(designerProducts, designerLibraries, caiiiProducts, caiiiLibraries, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    throw new PdmContextSyncException("Echec de la construction des contextes PDM.", ex);
                }

                result.ExcludedCoordinatorAssignments = plan.ExcludedCoordinatorAssignments;
                result.DuplicateDesignerAssignments = plan.DuplicateDesignerAssignments;
                result.DuplicateChangeAdministratorAssignments = plan.DuplicateChangeAdministratorAssignments;
                result.NormalizedContexts = plan.AllContexts.Count;
                result.ContextsWithoutChangeAdministrator = plan.ContextsWithoutChangeAdministrator;
                result.SkippedMissingParticipantId = plan.SkippedMissingParticipantId;
                EnsureSourceIsConsistent(designerProducts, designerLibraries, caiiiProducts, caiiiLibraries, plan);

                result.AddErrors(plan.Errors);
                foreach (var error in plan.Errors.Take(MaxLoggedErrors))
                    TraceLog.Error(error.Exception, $"PdmContextSync item failure: context={error.ContextName}, phase={error.Phase}");
                if (plan.Errors.Count > MaxLoggedErrors)
                    TraceLog.Warn($"PdmContextSync {plan.Errors.Count - MaxLoggedErrors} additional errors not logged individually (total={plan.Errors.Count}).");

                TraceLog.AddTraceLog($"PdmContextSync normalized={plan.AllContexts.Count}, pdmContext={plan.PdmContexts.Count}, excludedCoordinators={plan.ExcludedCoordinatorAssignments}");

                cancellationToken.ThrowIfCancellationRequested();
                PdmContextPersistenceResult persistence;
                try
                {
                    persistence = _repository.Synchronize(plan.PdmContexts, plan.AllContexts, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    throw new PdmContextSyncException("Echec de la persistance PDMCONTEXT / PDMALLCONTEXT (transaction annulee).", ex);
                }

                result.PdmContext = persistence.PdmContext;
                result.PdmAllContext = persistence.PdmAllContext;
                result.AddErrors(persistence.Errors);

                cancellationToken.ThrowIfCancellationRequested();
                TraceLog.AddTraceLog($"End PdmContextSync : {result}");
                return result;
            }
            catch (OperationCanceledException)
            {
                TraceLog.AddTraceLog("PdmContextSync cancelled");
                throw;
            }
            catch (Exception ex)
            {
                TraceLog.AddTraceLog($"PdmContextSync global failure : {ex.GetType().Name} - {ex.Message}");
                throw;
            }
        }

        private const int MaxLoggedErrors = 5;

        /// <summary>
        /// Objective invariants checked before any persistence: invalid or empty source => no transaction, no write.
        /// </summary>
        private static void EnsureSourceIsConsistent(
            IReadOnlyList<WindchillContextTeamMember> designerProducts,
            IReadOnlyList<WindchillContextTeamMember> designerLibraries,
            IReadOnlyList<WindchillContextTeamMember> caiiiProducts,
            IReadOnlyList<WindchillContextTeamMember> caiiiLibraries,
            PdmContextSyncPlan plan)
        {
            var populations = new (string Name, IReadOnlyList<WindchillContextTeamMember> Items)[]
            {
                ("Product DESIGNER", designerProducts),
                ("Library DESIGNER", designerLibraries),
                ("Product CHANGE ADMINISTRATOR III", caiiiProducts),
                ("Library CHANGE ADMINISTRATOR III", caiiiLibraries)
            };

            var summary = string.Join("; ", populations.Select(p =>
            {
                int missing = p.Items.Count(m => string.IsNullOrWhiteSpace(m.ContextName));
                return $"{p.Name}: received={p.Items.Count}, withoutContextName={missing}, withContextName={p.Items.Count - missing}";
            }));

            int totalReceived = populations.Sum(p => p.Items.Count);
            if (totalReceived == 0)
                throw new PdmContextSyncException($"Source PDM invalide : aucune affectation Windchill recue. {summary}");

            var allMissing = populations.Where(p => p.Items.Count > 0 && p.Items.All(m => string.IsNullOrWhiteSpace(m.ContextName))).Select(p => p.Name).ToList();
            if (allMissing.Count > 0)
                throw new PdmContextSyncException($"Source PDM invalide : toutes les affectations sans ContextName pour [{string.Join(", ", allMissing)}]. {summary}");

            if (plan.AllContexts.Count == 0)
                throw new PdmContextSyncException($"Source PDM invalide : affectations recues mais aucun contexte normalisable. {summary}");
        }

        private static IReadOnlyList<WindchillContextTeamMember> Retrieve(
            Func<List<WindchillContextTeamMember>> call, string population, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<WindchillContextTeamMember>? list;
            try
            {
                list = call();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new PdmContextSyncException($"Echec de la recuperation Windchill ({population}).", ex);
            }
            if (list is null)
                throw new PdmContextSyncException($"Echec de la recuperation Windchill ({population}).",
                    new InvalidOperationException("Windchill returned a null list."));

            TraceLog.AddTraceLog($"PdmContextSync {population} : {list.Count}");
            cancellationToken.ThrowIfCancellationRequested();
            return list;
        }
    }
}
