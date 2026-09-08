using MCG.CommonLib.CreoInteractionTools.Models;

namespace MCG.CREO_Tools.MiscTools.ViewModel.SimplifiedRep
{
    /// <summary>
    /// Photo de l'etat d'une ligne de la grille au moment ou l'utilisateur demande la mise a jour.
    /// Permet de lire les collections liees sur le thread UI, puis d'executer les appels Creo
    /// en tache de fond sans toucher aux objets de l'interface.
    /// </summary>
    internal class SimplifiedRepPendingChange
    {
        /// <summary>Chemin du composant depuis l'assemblage racine.</summary>
        public List<int> ComponentPath { get; set; } = new();

        /// <summary>Composant d'origine, necessaire pour resoudre une substitution par nom.</summary>
        public CreoSimpRepComponentInfo? ComponentInfo { get; set; }

        /// <summary>Etat de la case "Inclus" de la grille.</summary>
        public bool IsIncluded { get; set; }

        /// <summary>Representation simplifiee choisie dans la colonne "definie par l'utilisateur".</summary>
        public string SubstitutedSimpRepName { get; set; } = string.Empty;
    }
}
