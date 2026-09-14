using System;
using System.Collections.Generic;
using System.Linq;

namespace MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing
{
    /// <summary>
    /// Service pur (sans aucune dependance Creo) de planification de la mise a jour Manufacturing :
    /// regroupe les lignes modifiees par modele Creo (<see cref="ManufacturingUpdateCandidate.ModelKey"/>),
    /// detecte les conflits de valeurs entre occurrences du meme modele et isole les lignes dont le
    /// modele n'a pas pu etre resolu. Entierement testable via des candidats falsifies.
    /// </summary>
    public static class ManufacturingUpdatePlanningService
    {
        /// <summary>
        /// Construit le plan de mise a jour a partir de toutes les lignes de la grille (modifiees ou
        /// non : seules celles portant <see cref="ManufacturingUpdateCandidate.HasAnyChange"/> sont
        /// retenues). Un meme modele Creo (ModelKey) n'apparait qu'une seule fois dans
        /// <see cref="ManufacturingUpdatePlan.Entries"/>, meme s'il est reference par plusieurs
        /// occurrences dans la nomenclature.
        /// </summary>
        public static ManufacturingUpdatePlan BuildPlan(IEnumerable<ManufacturingUpdateCandidate> candidates)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));

            var modifiedCandidates = candidates.Where(c => c.HasAnyChange).ToList();

            var entries = new List<ManufacturingUpdatePlanEntry>();
            var conflicts = new List<ManufacturingUpdateConflict>();
            var unresolved = new List<ManufacturingUpdateUnresolved>();

            // Lignes modifiees dont le modele n'a pas pu etre resolu (ModelKey vide) : aucun
            // regroupement possible, elles ne peuvent jamais etre mises a jour de facon fiable.
            foreach (var candidate in modifiedCandidates.Where(c => !c.HasResolvedModel))
            {
                unresolved.Add(new ManufacturingUpdateUnresolved
                {
                    ComponentName = candidate.ComponentName
                });
            }

            var groupedByModel = modifiedCandidates
                .Where(c => c.HasResolvedModel)
                .GroupBy(c => c.ModelKey, StringComparer.OrdinalIgnoreCase);

            foreach (var group in groupedByModel)
            {
                var occurrences = group.ToList();
                var componentNames = occurrences.Select(o => o.ComponentName).ToList();

                var distinctReferenceValues = occurrences
                    .Where(o => o.HasReferenceChanged)
                    .Select(o => o.ReferenceValue)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                var distinctDescriptionMthValues = occurrences
                    .Where(o => o.HasDescriptionMthChanged)
                    .Select(o => o.DescriptionMthValue)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                bool hasReferenceConflict = distinctReferenceValues.Count > 1;
                bool hasDescriptionMthConflict = distinctDescriptionMthValues.Count > 1;

                if (hasReferenceConflict || hasDescriptionMthConflict)
                {
                    conflicts.Add(new ManufacturingUpdateConflict
                    {
                        ModelKey = group.Key,
                        ComponentNames = componentNames,
                        ConflictingReferenceValues = distinctReferenceValues,
                        ConflictingDescriptionMthValues = distinctDescriptionMthValues
                    });

                    // Aucune valeur n'est choisie arbitrairement : ce modele est exclu des entrees
                    // valides du plan, la mise a jour de ce modele est bloquee.
                    continue;
                }

                bool updateReference = distinctReferenceValues.Count == 1;
                bool updateDescriptionMth = distinctDescriptionMthValues.Count == 1;

                entries.Add(new ManufacturingUpdatePlanEntry
                {
                    ModelKey = group.Key,
                    ComponentNames = componentNames,
                    UpdateReference = updateReference,
                    ReferenceValue = updateReference ? distinctReferenceValues[0] : string.Empty,
                    UpdateDescriptionMth = updateDescriptionMth,
                    DescriptionMthValue = updateDescriptionMth ? distinctDescriptionMthValues[0] : string.Empty
                });
            }

            return new ManufacturingUpdatePlan
            {
                Entries = entries,
                Conflicts = conflicts,
                Unresolved = unresolved
            };
        }
    }
}
