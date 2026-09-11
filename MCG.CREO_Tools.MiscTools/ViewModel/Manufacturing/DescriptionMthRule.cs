namespace MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing
{
    /// <summary>
    /// Regle ayant produit (ou non) la valeur calculee de DESCRIPTION_MTH, dans l'ordre de
    /// priorite applique par <see cref="DescriptionMthCalculationService"/>.
    /// </summary>
    public enum DescriptionMthRule
    {
        /// <summary>Regle 1 : DESCRIPTION2_1 et DESCRIPTION2_2 sont toutes deux renseignees.</summary>
        CombinedDescription2,

        /// <summary>Regle 2 : PTC_COMMON_NAME renseigne et DESCRIPTION_2 vide ou "-".</summary>
        PtcCommonNameOnly,

        /// <summary>Regle 3 : traduction Webterm de PTC_COMMON_NAME combinee a DESCRIPTION_2.</summary>
        WebtermTranslation,

        /// <summary>
        /// Regle 4 (repli) : DESCRIPTION2_1/DESCRIPTION2_2 vides ou "-", PTC_COMMON_NAME renseigne
        /// mais sans traduction Webterm FR exploitable, et DESCRIPTION_2 renseignee (ni vide ni "-")
        /// => PTC_COMMON_NAME + "|" + DESCRIPTION_2.
        /// </summary>
        PtcCommonNameAndDescription2,

        /// <summary>Aucune regle n'a permis de produire une valeur exploitable.</summary>
        None
    }

    /// <summary>
    /// Statut explicite d'une ligne Manufacturing vis-a-vis du calcul de DESCRIPTION_MTH.
    /// </summary>
    public enum DescriptionMthStatus
    {
        /// <summary>La valeur affichee est identique a la valeur initiale lue dans Creo.</summary>
        Unchanged,

        /// <summary>La valeur affichee est le resultat d'un calcul automatique (regle 1, 2 ou 3).</summary>
        Calculated,

        /// <summary>La valeur affichee a ete saisie ou modifiee manuellement par l'utilisateur.</summary>
        ManuallyModified,

        /// <summary>Une donnee source a change depuis le dernier calcul : une confirmation est necessaire.</summary>
        UpdateRequired,

        /// <summary>Aucune regle n'a permis de produire une valeur, et aucune valeur initiale n'existait.</summary>
        CalculationImpossible,

        /// <summary>Une erreur reelle (reseau, service) est survenue lors de l'appel Webterm.</summary>
        WebtermError
    }
}
