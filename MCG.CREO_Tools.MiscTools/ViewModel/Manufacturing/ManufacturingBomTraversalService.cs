using System;
using System.Collections.Generic;

namespace MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing
{
    /// <summary>
    /// Resultat de la visite d'un noeud lors du parcours de nomenclature : porte le niveau de
    /// profondeur, la numerotation hierarchique et un indicateur de doublon de modele, sans
    /// dependre d'aucune API Creo. Le noeud d'origine (<typeparamref name="TNode"/>) est fourni
    /// tel quel afin que l'appelant puisse en extraire ce dont il a besoin (parametres, etc.).
    /// </summary>
    /// <typeparam name="TNode">Type du noeud visite (composant Creo ou tout autre modele de test).</typeparam>
    public sealed class ManufacturingBomVisitResult<TNode>
    {
        public required TNode Node { get; init; }

        /// <summary>Profondeur du composant dans la nomenclature (0 = premier niveau).</summary>
        public required int Level { get; init; }

        /// <summary>Numerotation hierarchique du composant (ex : 1, 1.1, 1.1.2).</summary>
        public required string HierarchicalNumber { get; init; }

        /// <summary>
        /// Vrai si le modele identifie par la cle de ce noeud a deja ete rencontre ailleurs dans
        /// l'arbre (a un autre endroit de la nomenclature, hors chemin d'ascendance).
        /// </summary>
        public required bool IsDuplicateModel { get; init; }

        /// <summary>
        /// Vrai si ce noeud a ete ecarte de l'expansion car son modele figure deja parmi ses
        /// propres ancetres (cycle reel dans la structure). Le noeud est neanmoins visite/liste,
        /// mais ses enfants ne sont pas parcourus.
        /// </summary>
        public required bool IsCycleDetected { get; init; }
    }

    /// <summary>
    /// Parcours generique et pur (sans aucune dependance Creo/COM) d'une nomenclature en arbre.
    /// Compose la numerotation hierarchique, detecte les cycles (modele deja present dans la
    /// pile d'ascendance du noeud courant) et signale les doublons (modele deja rencontre
    /// ailleurs dans l'arbre, hors ascendance). Entierement testable via des noeuds falsifies.
    /// </summary>
    public static class ManufacturingBomTraversalService
    {
        /// <summary>
        /// Parcourt l'arbre en profondeur a partir des enfants directs de la racine.
        /// </summary>
        /// <typeparam name="TNode">Type de noeud opaque manipule par l'appelant.</typeparam>
        /// <param name="rootChildren">Enfants de premier niveau de la racine.</param>
        /// <param name="getChildren">
        /// Fournit les enfants directs d'un noeud donne (encapsule l'appel Creo verifie
        /// cote appelant, ou toute logique de test).
        /// </param>
        /// <param name="getModelKey">
        /// Fournit l'identite de modele d'un noeud (utilisee pour la detection de cycle et de
        /// doublon). Une cle vide ou nulle desactive la detection pour ce noeud.
        /// </param>
        /// <param name="isExpandable">
        /// Indique si un noeud doit voir ses enfants parcourus (ex : uniquement les
        /// sous-assemblages). Si null, tous les noeuds sont consideres comme expansibles.
        /// </param>
        /// <param name="maxLevel">
        /// Profondeur maximale (garde-fou supplementaire independant de la detection de cycle).
        /// </param>
        /// <returns>La liste ordonnee (parcours en profondeur, prefixe) des noeuds visites.</returns>
        public static List<ManufacturingBomVisitResult<TNode>> Traverse<TNode>(
            IEnumerable<TNode> rootChildren,
            Func<TNode, IEnumerable<TNode>> getChildren,
            Func<TNode, string?> getModelKey,
            Func<TNode, bool>? isExpandable,
            int maxLevel)
        {
            if (rootChildren == null) throw new ArgumentNullException(nameof(rootChildren));
            if (getChildren == null) throw new ArgumentNullException(nameof(getChildren));
            if (getModelKey == null) throw new ArgumentNullException(nameof(getModelKey));

            var results = new List<ManufacturingBomVisitResult<TNode>>();

            // Modeles deja rencontres n'importe ou dans l'arbre (hors ascendance courante) :
            // sert uniquement a marquer les doublons, n'empeche jamais l'expansion.
            var seenAnywhere = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int localIndex = 0;
            foreach (var child in rootChildren)
            {
                localIndex++;
                TraverseNode(
                    child,
                    level: 0,
                    hierarchicalNumber: localIndex.ToString(),
                    ancestorModelKeys: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                    seenAnywhere: seenAnywhere,
                    getChildren: getChildren,
                    getModelKey: getModelKey,
                    isExpandable: isExpandable,
                    maxLevel: maxLevel,
                    results: results);
            }

            return results;
        }

        private static void TraverseNode<TNode>(
            TNode node,
            int level,
            string hierarchicalNumber,
            HashSet<string> ancestorModelKeys,
            HashSet<string> seenAnywhere,
            Func<TNode, IEnumerable<TNode>> getChildren,
            Func<TNode, string?> getModelKey,
            Func<TNode, bool>? isExpandable,
            int maxLevel,
            List<ManufacturingBomVisitResult<TNode>> results)
        {
            string modelKey = getModelKey(node) ?? string.Empty;
            bool hasModelKey = !string.IsNullOrWhiteSpace(modelKey);

            bool isCycle = hasModelKey && ancestorModelKeys.Contains(modelKey);

            // Un doublon est un modele deja vu ailleurs dans l'arbre (n'importe quel chemin
            // precedent), qu'il s'agisse ou non d'un cycle. On l'enregistre avant de tester
            // "seenAnywhere.Contains" afin de determiner l'etat AVANT l'ajout courant.
            bool isDuplicate = hasModelKey && seenAnywhere.Contains(modelKey);

            if (hasModelKey)
                seenAnywhere.Add(modelKey);

            results.Add(new ManufacturingBomVisitResult<TNode>
            {
                Node = node,
                Level = level,
                HierarchicalNumber = hierarchicalNumber,
                IsDuplicateModel = isDuplicate,
                IsCycleDetected = isCycle
            });

            // Garde-fous : cycle reel detecte, profondeur maximale atteinte, ou noeud non
            // expansible (ex : piece, par opposition a un sous-assemblage).
            if (isCycle) return;
            if (level + 1 >= maxLevel) return;
            if (isExpandable != null && !isExpandable(node)) return;

            IEnumerable<TNode> children;
            try
            {
                children = getChildren(node) ?? Array.Empty<TNode>();
            }
            catch
            {
                // Un composant illisible ne doit pas interrompre le reste du parcours : ce
                // noeud est deja liste ci-dessus, ses enfants sont simplement ignores.
                return;
            }

            var childAncestorModelKeys = hasModelKey
                ? new HashSet<string>(ancestorModelKeys, StringComparer.OrdinalIgnoreCase) { modelKey }
                : ancestorModelKeys;

            int localIndex = 0;
            foreach (var child in children)
            {
                localIndex++;
                var childHierarchicalNumber = $"{hierarchicalNumber}.{localIndex}";

                TraverseNode(
                    child,
                    level: level + 1,
                    hierarchicalNumber: childHierarchicalNumber,
                    ancestorModelKeys: childAncestorModelKeys,
                    seenAnywhere: seenAnywhere,
                    getChildren: getChildren,
                    getModelKey: getModelKey,
                    isExpandable: isExpandable,
                    maxLevel: maxLevel,
                    results: results);
            }
        }
    }
}
