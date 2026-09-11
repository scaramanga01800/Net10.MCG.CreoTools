namespace MCG.CREO_Tools.MiscTools.View.Manufacturing
{
    /// <summary>
    /// Contrat d'une ligne de la grille Manufacturing View : un composant de la nomenclature,
    /// a un niveau de profondeur donne dans l'assemblage.
    /// </summary>
    internal interface IManufacturingComponentItem
    {
        int TreeIndex { get; set; }
        int Level { get; set; }
        string HierarchicalNumber { get; set; }
        int ComponentId { get; set; }
        string Name { get; set; }

        string Reference { get; set; }
        string PtcCommonName { get; set; }
        string Description2 { get; set; }
        string Description2_1 { get; set; }
        string Description2_2 { get; set; }
        string DescriptionMth { get; set; }

        bool IsModified { get; }

        /// <summary>
        /// Vrai si le modele reference par ce composant a deja ete rencontre ailleurs dans la
        /// nomenclature (a un autre endroit de l'arbre, hors ascendance directe).
        /// </summary>
        bool IsDuplicateModel { get; set; }

        /// <summary>
        /// Vrai si ce composant a ete ecarte de l'expansion car son modele figure deja parmi ses
        /// propres ancetres (cycle reel dans la structure de l'assemblage).
        /// </summary>
        bool IsCycleDetected { get; set; }
    }
}
