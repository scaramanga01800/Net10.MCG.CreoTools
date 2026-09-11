using System;

namespace MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing
{
    /// <summary>
    /// Resultat explicite du calcul de DESCRIPTION_MTH pour une ligne Manufacturing : indique la
    /// regle appliquee (ou l'absence de resultat), la valeur produite, et si un appel Webterm a
    /// echoue reellement (par opposition a une simple absence de traduction).
    /// </summary>
    public sealed class DescriptionMthCalculationResult
    {
        /// <summary>Regle qui a produit <see cref="Value"/>, ou <see cref="DescriptionMthRule.None"/>.</summary>
        public required DescriptionMthRule Rule { get; init; }

        /// <summary>
        /// Valeur calculee. Null si aucune regle n'a pu produire de valeur exploitable
        /// (voir <see cref="Rule"/> == <see cref="DescriptionMthRule.None"/>).
        /// </summary>
        public string? Value { get; init; }

        /// <summary>
        /// Vrai si la regle 3 a ete tentee et que l'appel Webterm a leve une erreur reelle
        /// (reseau, service indisponible, ...) plutot qu'une simple absence de traduction.
        /// </summary>
        public bool IsWebtermError { get; init; }

        /// <summary>Message d'erreur Webterm le cas echeant, pour tracabilite/log.</summary>
        public string? WebtermErrorMessage { get; init; }

        public static DescriptionMthCalculationResult Success(DescriptionMthRule rule, string value)
        {
            return new DescriptionMthCalculationResult { Rule = rule, Value = value };
        }

        public static DescriptionMthCalculationResult NoResult()
        {
            return new DescriptionMthCalculationResult { Rule = DescriptionMthRule.None, Value = null };
        }

        public static DescriptionMthCalculationResult WebtermFailure(string errorMessage)
        {
            return new DescriptionMthCalculationResult
            {
                Rule = DescriptionMthRule.None,
                Value = null,
                IsWebtermError = true,
                WebtermErrorMessage = errorMessage
            };
        }
    }
}
