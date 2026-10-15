using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.EcnEcoFollowUp.Models.PdmContext;

namespace MCG.Tools.EcnEcoFollowUp.Services.PdmContext
{
    /// <summary>
    /// Allocates application-assigned identifiers for one table during one persistence transaction.
    /// PDMCONTEXT.ID / PDMALLCONTEXT.ID are configured ValueGeneratedNever() in CreoToolsDbContext:
    /// the base value is read once (inside the caller's serializable transaction), never per row.
    /// Residual risk: a writer outside this transaction model may still collide; the primary key then
    /// rejects the insert and the whole transaction is rolled back (no silent corruption).
    /// </summary>
    public sealed class PdmContextIdAllocator
    {
        private int _next;

        public PdmContextIdAllocator(int? currentMaxId)
        {
            _next = (currentMaxId ?? 0) + 1;
        }

        public int Next() => _next++;
    }

    /// <summary>
    /// Pure row synchronizer for PDMCONTEXT / PDMALLCONTEXT (no DbContext, fully testable).
    /// Existing rows are matched by exact ordinal PdmContext == ContextName (historical key).
    /// Synchronized fields: TeamRole, ParticipantType, ParticipantId, ParticipantName, Product, Type.
    /// ContextLink is only set ("-") on insert, as historically. Obsolete rows are never deleted.
    /// </summary>
    public static class PdmContextRowSynchronizer
    {
        public const string DefaultContextLink = "-";

        public static IReadOnlyList<Pdmcontext> SyncPdmContexts(
            IReadOnlyList<Pdmcontext> existing,
            IReadOnlyList<PdmContextMemberSnapshot> candidates,
            Func<PdmContextIdAllocator> allocatorFactory,
            PdmContextTableSyncResult result,
            CancellationToken cancellationToken)
            => Sync(existing, candidates, allocatorFactory, result, cancellationToken,
                e => e.PdmContext, e => e.Id, Apply,
                (s, id) => new Pdmcontext { Id = id, PdmContext = s.ContextName, ContextLink = DefaultContextLink, TeamRole = s.TeamRole, ParticipantType = s.ParticipantType, ParticipantId = s.ParticipantId, ParticipantName = s.ParticipantName, Product = s.Product, Type = s.ContextType });

        public static IReadOnlyList<Pdmallcontext> SyncPdmAllContexts(
            IReadOnlyList<Pdmallcontext> existing,
            IReadOnlyList<PdmContextMemberSnapshot> candidates,
            Func<PdmContextIdAllocator> allocatorFactory,
            PdmContextTableSyncResult result,
            CancellationToken cancellationToken)
            => Sync(existing, candidates, allocatorFactory, result, cancellationToken,
                e => e.PdmContext, e => e.Id, Apply,
                (s, id) => new Pdmallcontext { Id = id, PdmContext = s.ContextName, ContextLink = DefaultContextLink, TeamRole = s.TeamRole, ParticipantType = s.ParticipantType, ParticipantId = s.ParticipantId, ParticipantName = s.ParticipantName, Product = s.Product, Type = s.ContextType });

        private static IReadOnlyList<T> Sync<T>(
            IReadOnlyList<T> existing,
            IReadOnlyList<PdmContextMemberSnapshot> candidates,
            Func<PdmContextIdAllocator> allocatorFactory,
            PdmContextTableSyncResult result,
            CancellationToken cancellationToken,
            Func<T, string?> key,
            Func<T, int> id,
            Func<T, PdmContextMemberSnapshot, bool> apply,
            Func<PdmContextMemberSnapshot, int, T> create)
        {
            result.Candidates = candidates.Count;

            // Deterministic: lowest Id wins when legacy duplicates exist for one context.
            var byKey = new Dictionary<string, T>(StringComparer.Ordinal);
            foreach (var row in existing.OrderBy(id))
            {
                var k = key(row);
                if (k is null)
                    continue;
                if (!byKey.TryAdd(k, row))
                    result.DuplicateExistingRows++;
            }

            var candidateKeys = new HashSet<string>(candidates.Select(c => c.ContextName), StringComparer.Ordinal);
            result.ObsoleteRowsDetected = byKey.Keys.Count(k => !candidateKeys.Contains(k));

            PdmContextIdAllocator? allocator = null;
            var inserted = new List<T>();
            foreach (var candidate in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (byKey.TryGetValue(candidate.ContextName, out var row))
                {
                    if (apply(row, candidate))
                        result.Updated++;
                    else
                        result.Unchanged++;
                }
                else
                {
                    allocator ??= allocatorFactory();
                    var entity = create(candidate, allocator.Next());
                    inserted.Add(entity);
                    byKey[candidate.ContextName] = entity;
                    result.Inserted++;
                }
            }
            return inserted;
        }

        public static bool Apply(Pdmcontext row, PdmContextMemberSnapshot s)
        {
            bool changed = false;
            if (!string.Equals(row.TeamRole, s.TeamRole, StringComparison.Ordinal)) { row.TeamRole = s.TeamRole; changed = true; }
            if (!string.Equals(row.ParticipantType, s.ParticipantType, StringComparison.Ordinal)) { row.ParticipantType = s.ParticipantType; changed = true; }
            if (!string.Equals(row.ParticipantId, s.ParticipantId, StringComparison.Ordinal)) { row.ParticipantId = s.ParticipantId; changed = true; }
            if (!string.Equals(row.ParticipantName, s.ParticipantName, StringComparison.Ordinal)) { row.ParticipantName = s.ParticipantName; changed = true; }
            if (!string.Equals(row.Product, s.Product, StringComparison.Ordinal)) { row.Product = s.Product; changed = true; }
            if (!string.Equals(row.Type, s.ContextType, StringComparison.Ordinal)) { row.Type = s.ContextType; changed = true; }
            return changed;
        }

        public static bool Apply(Pdmallcontext row, PdmContextMemberSnapshot s)
        {
            bool changed = false;
            if (!string.Equals(row.TeamRole, s.TeamRole, StringComparison.Ordinal)) { row.TeamRole = s.TeamRole; changed = true; }
            if (!string.Equals(row.ParticipantType, s.ParticipantType, StringComparison.Ordinal)) { row.ParticipantType = s.ParticipantType; changed = true; }
            if (!string.Equals(row.ParticipantId, s.ParticipantId, StringComparison.Ordinal)) { row.ParticipantId = s.ParticipantId; changed = true; }
            if (!string.Equals(row.ParticipantName, s.ParticipantName, StringComparison.Ordinal)) { row.ParticipantName = s.ParticipantName; changed = true; }
            if (!string.Equals(row.Product, s.Product, StringComparison.Ordinal)) { row.Product = s.Product; changed = true; }
            if (!string.Equals(row.Type, s.ContextType, StringComparison.Ordinal)) { row.Type = s.ContextType; changed = true; }
            return changed;
        }
    }
}
