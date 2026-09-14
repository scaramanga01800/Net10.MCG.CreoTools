using System.Collections.Generic;

namespace MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing
{
    /// <summary>
    /// Entree pure (sans dependance Creo) representant une ligne candidate a la mise a jour :
    /// construite par le ViewModel a partir d'un <see cref="ManufacturingComponentItem"/>, elle
    /// permet de tester le regroupement par modele et la detection de conflit sans session Creo.
    /// </summary>
    public sealed class ManufacturingUpdateCandidate
    {
        /// <summary>Identite du modele Creo (voir <c>CreoSimpRepComponentInfo.ModelKey</c>). Vide si non resolu.</summary>
        public required string ModelKey { get; init; }

        /// <summary>Nom du composant tel qu'affiche dans la grille (pour les messages de conflit).</summary>
        public required string ComponentName { get; init; }

        public required bool HasReferenceChanged { get; init; }
        public required string ReferenceValue { get; init; }

        public required bool HasDescriptionMthChanged { get; init; }
        public required string DescriptionMthValue { get; init; }

        /// <summary>Faux si le modele n'a pas pu etre resolu lors de la lecture (ModelKey vide).</summary>
        public bool HasResolvedModel => !string.IsNullOrWhiteSpace(ModelKey);

        public bool HasAnyChange => HasReferenceChanged || HasDescriptionMthChanged;
    }

    /// <summary>Une entree valide du plan : un modele Creo unique a mettre a jour.</summary>
    public sealed class ManufacturingUpdatePlanEntry
    {
        public required string ModelKey { get; init; }
        public required IReadOnlyList<string> ComponentNames { get; init; }

        public required bool UpdateReference { get; init; }
        public required string ReferenceValue { get; init; }

        public required bool UpdateDescriptionMth { get; init; }
        public required string DescriptionMthValue { get; init; }
    }

    /// <summary>
    /// Conflit detecte : plusieurs occurrences du meme modele Creo portent des valeurs
    /// contradictoires pour REFERENCE et/ou DESCRIPTION_MTH. Bloque la mise a jour de ce modele.
    /// </summary>
    public sealed class ManufacturingUpdateConflict
    {
        public required string ModelKey { get; init; }
        public required IReadOnlyList<string> ComponentNames { get; init; }
        public required IReadOnlyList<string> ConflictingReferenceValues { get; init; }
        public required IReadOnlyList<string> ConflictingDescriptionMthValues { get; init; }
    }

    /// <summary>
    /// Modele Creo dont l'identite n'a pas pu etre resolue (ModelKey vide) mais qui porte
    /// neanmoins une modification en attente : il ne peut pas etre mis a jour de facon fiable.
    /// </summary>
    public sealed class ManufacturingUpdateUnresolved
    {
        public required string ComponentName { get; init; }
    }

    /// <summary>
    /// Resultat pur de la planification de mise a jour : compose exclusivement a partir de
    /// <see cref="ManufacturingUpdateCandidate"/>, sans aucun appel Creo. Regroupe les occurrences
    /// du meme modele (un seul modele n'est jamais mis a jour/sauvegarde plus d'une fois), detecte
    /// les conflits de valeurs et isole les composants dont le modele n'a pas pu etre resolu.
    /// </summary>
    public sealed class ManufacturingUpdatePlan
    {
        public required IReadOnlyList<ManufacturingUpdatePlanEntry> Entries { get; init; }
        public required IReadOnlyList<ManufacturingUpdateConflict> Conflicts { get; init; }
        public required IReadOnlyList<ManufacturingUpdateUnresolved> Unresolved { get; init; }

        /// <summary>Vrai si au moins un conflit bloque la mise a jour.</summary>
        public bool HasConflicts => Conflicts.Count > 0;

        /// <summary>Vrai si aucune ligne modifiee n'a ete detectee.</summary>
        public bool IsEmpty => Entries.Count == 0 && Conflicts.Count == 0 && Unresolved.Count == 0;
    }
}
