namespace MCG.CREO_Tools.MiscTools.View.Manufacturing
{
    using System;
    using MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing;

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

        /// <summary>
        /// Identite du modele Creo reference par ce composant (voir
        /// <c>CreoSimpRepComponentInfo.ModelKey</c>). Deux lignes partageant cette valeur
        /// referencent le meme modele Creo, independamment de leur position/niveau dans la
        /// nomenclature ; elles doivent donc partager la meme valeur DESCRIPTION_MTH lorsqu'elle
        /// est modifiee manuellement.
        /// </summary>
        string ModelKey { get; set; }

        string Reference { get; set; }
        string PtcCommonName { get; set; }
        string Description2 { get; set; }
        string Description2_1 { get; set; }
        string Description2_2 { get; set; }
        string DescriptionMth { get; set; }

        /// <summary>Valeur de DESCRIPTION_MTH telle que trouvee dans Creo lors de la derniere lecture.</summary>
        string InitialDescriptionMth { get; set; }

        /// <summary>Derniere valeur produite par le calcul automatique (null si non calculable).</summary>
        string? CalculatedDescriptionMth { get; set; }

        /// <summary>Regle ayant produit <see cref="CalculatedDescriptionMth"/>.</summary>
        DescriptionMthRule CalculatedDescriptionMthRule { get; set; }

        /// <summary>Vrai si la valeur affichee de DESCRIPTION_MTH provient d'une saisie manuelle.</summary>
        bool IsManualDescriptionMth { get; set; }

        /// <summary>Statut explicite de la ligne vis-a-vis du calcul/de la saisie de DESCRIPTION_MTH.</summary>
        DescriptionMthStatus Status { get; set; }

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

        /// <summary>
        /// Declenche lorsque l'utilisateur modifie manuellement DESCRIPTION_MTH sur cette ligne,
        /// avec la nouvelle valeur saisie. Permet a l'appelant (ViewModel) de propager la meme
        /// valeur a tous les autres composants partageant le meme <see cref="ModelKey"/>, quel que
        /// soit leur niveau dans la nomenclature.
        /// </summary>
        event EventHandler<string>? ManualDescriptionMthChangedEvent;
    }
}

