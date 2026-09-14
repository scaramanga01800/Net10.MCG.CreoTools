using System.Collections.Generic;

namespace MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing
{
    /// <summary>Issue de la mise a jour d'un modele Creo donne, pour la synthese finale.</summary>
    public enum ManufacturingUpdateOutcomeStatus
    {
        /// <summary>Parametre(s) ecrits et modele sauvegarde avec succes.</summary>
        Updated,

        /// <summary>Aucune modification necessaire (ne devrait pas apparaitre : filtre en amont).</summary>
        Unchanged,

        /// <summary>Modele ecarte volontairement (conflit de valeurs entre occurrences).</summary>
        Ignored,

        /// <summary>Modele Creo introuvable lors de la nouvelle lecture locale.</summary>
        NotFound,

        /// <summary>Erreur lors de l'ecriture du parametre ou de la sauvegarde.</summary>
        Error
    }

    /// <summary>Resultat de traitement d'un modele Creo pour le bilan final de la mise a jour.</summary>
    public sealed class ManufacturingUpdateOutcome
    {
        public required string ModelKey { get; init; }
        public required IReadOnlyList<string> ComponentNames { get; init; }
        public required ManufacturingUpdateOutcomeStatus Status { get; init; }

        /// <summary>Detail complementaire (message d'erreur, raison de conflit, etc.), pour le log et le bilan.</summary>
        public string Detail { get; init; } = string.Empty;
    }
}
