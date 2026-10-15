using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.Models.PdmContext;
using MCG.WindchillRequestTool.Model.Windchill;

namespace MCG.Tools.EcnEcoFollowUp.Services.PdmContext
{
    /// <summary>
    /// Pure, stateless reconstruction of the historical BatchModeUpdatePdmContext filtering and matching rules.
    /// Source objects returned by Windchill are never mutated.
    /// </summary>
    public static class PdmContextSyncPlanner
    {
        public const string DesignerRole = "DESIGNER";
        public const string ChangeAdministratorRole = "CHANGE ADMINISTRATOR III";

        /// <summary>Historical exact exclusions (ordinal, case-sensitive, equality on ParticipantId).</summary>
        public static readonly IReadOnlyList<string> ExcludedCoordinatorParticipantIds = new[]
        {
            "FR_ECN_Coordinators",
            "CHI_TWR_ECN_Coordinators"
        };

        public static bool IsExcludedCoordinator(WindchillContextTeamMember member) =>
            member.ParticipantId is not null
            && ExcludedCoordinatorParticipantIds.Any(id => string.Equals(id, member.ParticipantId, StringComparison.Ordinal));

        /// <summary>
        /// Builds the candidates for PDMCONTEXT and PDMALLCONTEXT.
        /// Order rules (historical): DESIGNER = Product then Library; CHANGE ADMINISTRATOR III = Library then Product,
        /// first match by exact ContextName wins.
        /// </summary>
        public static PdmContextSyncPlan Build(
            IReadOnlyList<WindchillContextTeamMember> designerProducts,
            IReadOnlyList<WindchillContextTeamMember> designerLibraries,
            IReadOnlyList<WindchillContextTeamMember> changeAdministratorProducts,
            IReadOnlyList<WindchillContextTeamMember> changeAdministratorLibraries,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(designerProducts);
            ArgumentNullException.ThrowIfNull(designerLibraries);
            ArgumentNullException.ThrowIfNull(changeAdministratorProducts);
            ArgumentNullException.ThrowIfNull(changeAdministratorLibraries);

            var errors = new List<PdmContextSyncError>();

            // CHANGE ADMINISTRATOR III : Library.Union(Product) filtered, first by ContextName wins.
            int excluded = 0;
            int duplicateCaiii = 0;
            var caiiiByContext = new Dictionary<string, WindchillContextTeamMember>(StringComparer.Ordinal);
            foreach (var member in changeAdministratorLibraries.Concat(changeAdministratorProducts))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (member is null)
                    continue;
                if (IsExcludedCoordinator(member))
                {
                    excluded++;
                    continue;
                }
                if (member.ContextName is null)
                    continue;
                if (!caiiiByContext.TryAdd(member.ContextName, member))
                    duplicateCaiii++;
            }

            // DESIGNER : Product (typed PRODUCT) then Library (typed LIBRARY); one final context per exact ContextName.
            int duplicateDesigner = 0;
            int withoutCaiii = 0;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var all = new List<PdmContextMemberSnapshot>();

            var designers = designerProducts.Select(m => (Member: m, Type: WindchillContextType.PRODUCT))
                .Concat(designerLibraries.Select(m => (Member: m, Type: WindchillContextType.LIBRARY)));

            foreach (var (member, type) in designers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (member is null)
                    continue;

                if (string.IsNullOrWhiteSpace(member.ContextName))
                {
                    errors.Add(new PdmContextSyncError(
                        member.ContextName,
                        PdmContextSyncTable.None,
                        PdmContextSyncPhase.Normalization,
                        $"DESIGNER {type} assignment without ContextName ignored.",
                        new InvalidOperationException("Windchill DESIGNER assignment has no ContextName.")));
                    continue;
                }

                if (!seen.Add(member.ContextName))
                {
                    duplicateDesigner++;
                    continue;
                }

                string product;
                try
                {
                    product = EFU_PdmContext.ExtractProduct(member.ContextName);
                }
                catch (Exception ex)
                {
                    errors.Add(new PdmContextSyncError(member.ContextName, PdmContextSyncTable.None,
                        PdmContextSyncPhase.ProductExtraction, "Product extraction failed.", ex));
                    continue;
                }

                PdmContextMemberSnapshot snapshot;
                if (caiiiByContext.TryGetValue(member.ContextName, out var caiii))
                {
                    snapshot = new PdmContextMemberSnapshot
                    {
                        ContextName = member.ContextName,
                        ContextType = type.ToString(),
                        TeamRole = caiii.TeamRole,
                        ParticipantId = caiii.ParticipantId,
                        ParticipantName = caiii.ParticipantName,
                        ParticipantType = caiii.ParticipantType,
                        Product = product
                    };
                }
                else
                {
                    withoutCaiii++;
                    snapshot = new PdmContextMemberSnapshot
                    {
                        ContextName = member.ContextName,
                        ContextType = type.ToString(),
                        TeamRole = ChangeAdministratorRole,
                        ParticipantId = string.Empty,
                        ParticipantName = string.Empty,
                        ParticipantType = string.Empty,
                        Product = product
                    };
                }
                all.Add(snapshot);
            }

            var withParticipant = all.Where(s => s.HasParticipant).ToList();

            return new PdmContextSyncPlan
            {
                AllContexts = all,
                PdmContexts = withParticipant,
                Errors = errors,
                ExcludedCoordinatorAssignments = excluded,
                DuplicateDesignerAssignments = duplicateDesigner,
                DuplicateChangeAdministratorAssignments = duplicateCaiii,
                ContextsWithoutChangeAdministrator = withoutCaiii,
                SkippedMissingParticipantId = all.Count - withParticipant.Count
            };
        }
    }
}
