using MCG.CommonLib.WebtermLib.Models;
using MCG.CommonLib.WebtermLib.Services.Interfaces;
using System;
using System.Collections.Concurrent;

namespace MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing
{
    /// <summary>
    /// Donnees source necessaires au calcul de DESCRIPTION_MTH pour une ligne Manufacturing.
    /// </summary>
    public sealed class DescriptionMthCalculationInput
    {
        public string? PtcCommonName { get; init; }
        public string? Description2 { get; init; }
        public string? Description2_1 { get; init; }
        public string? Description2_2 { get; init; }
    }

    /// <summary>
    /// Service pur (independant de Creo et de toute interface graphique) qui calcule la valeur
    /// proposee de DESCRIPTION_MTH a partir des donnees d'une ligne Manufacturing, en appliquant
    /// les regles suivantes, dans cet ordre :
    ///
    /// Regle 1 : DESCRIPTION2_1 et DESCRIPTION2_2 toutes deux renseignees (ni vides, ni "-") =>
    ///           DESCRIPTION2_1 + "|" + DESCRIPTION2_2.
    /// Regle 2 : PTC_COMMON_NAME renseigne et DESCRIPTION_2 vide ou "-" => PTC_COMMON_NAME.
    /// Regle 3 : PTC_COMMON_NAME renseigne et une traduction francaise exploitable est obtenue via
    ///           <see cref="IWebtermTools.GetTermFromEnglish(string, WebtermLanguage, System.Collections.Generic.List{MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb.Webterm})"/> =>
    ///           traduction + "|" + DESCRIPTION_2 (sans "|" si DESCRIPTION_2 est vide).
    /// Regle 4 (repli) : DESCRIPTION2_1/DESCRIPTION2_2 vides ou "-", PTC_COMMON_NAME renseigne mais
    ///           sans traduction Webterm FR exploitable, et DESCRIPTION_2 renseignee (ni vide ni
    ///           "-") => PTC_COMMON_NAME + "|" + DESCRIPTION_2.
    /// Secours : aucune regle n'a produit de valeur exploitable => <see cref="DescriptionMthRule.None"/>,
    ///           la valeur existante dans Creo n'est jamais ecrasee automatiquement par ce service.
    ///
    /// L'appel Webterm (regle 3) n'est effectue que si les regles 1 et 2 n'ont pas deja produit de
    /// resultat, et est mis en cache par (PTC_COMMON_NAME, langue) le temps de vie de l'instance du
    /// service, pour eviter plusieurs appels identiques pendant une meme lecture.
    /// </summary>
    public sealed class DescriptionMthCalculationService
    {
        private readonly IWebtermTools _webtermTools;
        private readonly WebtermLanguage _translationLanguage;
        private readonly ConcurrentDictionary<string, DescriptionMthCalculationResult> _webtermCache
            = new ConcurrentDictionary<string, DescriptionMthCalculationResult>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Construit le service de calcul, avec injection de <see cref="IWebtermTools"/> (regle 3).
        /// La langue de traduction cible est le francais, conformement a la regle 3.
        /// </summary>
        public DescriptionMthCalculationService(IWebtermTools webtermTools)
        {
            _webtermTools = webtermTools ?? throw new ArgumentNullException(nameof(webtermTools));
            _translationLanguage = WebtermLanguage.FRENCH;
        }

        /// <summary>
        /// Calcule la proposition de DESCRIPTION_MTH pour <paramref name="input"/> en appliquant les
        /// regles 1, 2 puis 3 dans cet ordre. Ne leve jamais d'exception pour une absence de
        /// traduction Webterm : seule une erreur reelle du service Webterm est remontee via
        /// <see cref="DescriptionMthCalculationResult.IsWebtermError"/>.
        /// </summary>
        public DescriptionMthCalculationResult Calculate(DescriptionMthCalculationInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var ptcCommonName = Normalize(input.PtcCommonName);
            var description2 = Normalize(input.Description2);
            var description2_1 = Normalize(input.Description2_1);
            var description2_2 = Normalize(input.Description2_2);

            // Regle 1 : DESCRIPTION2_1 et DESCRIPTION2_2 toutes deux renseignees et differentes de "-".
            if (IsMeaningful(description2_1) && IsMeaningful(description2_2))
            {
                return DescriptionMthCalculationResult.Success(
                    DescriptionMthRule.CombinedDescription2,
                    Join(description2_1, description2_2));
            }

            // Regle 2 : PTC_COMMON_NAME renseigne et DESCRIPTION_2 vide ou "-".
            if (IsMeaningful(ptcCommonName) && !IsMeaningful(description2))
            {
                return DescriptionMthCalculationResult.Success(
                    DescriptionMthRule.PtcCommonNameOnly,
                    ptcCommonName!);
            }

            // Regle 3 : traduction Webterm de PTC_COMMON_NAME (uniquement si 1 et 2 n'ont rien produit).
            // A ce stade, DESCRIPTION_2 est necessairement renseignee (sinon la regle 2 aurait deja
            // produit un resultat), ce qui est la condition requise par la regle 4 (repli).
            if (IsMeaningful(ptcCommonName))
            {
                var webtermResult = GetOrTranslate(ptcCommonName!);

                if (webtermResult.IsWebtermError)
                    return webtermResult;

                if (webtermResult.Value != null)
                {
                    var value = IsMeaningful(description2)
                        ? Join(webtermResult.Value, description2!)
                        : webtermResult.Value;

                    return DescriptionMthCalculationResult.Success(DescriptionMthRule.WebtermTranslation, value);
                }

                // Regle 4 (repli) : DESCRIPTION2_1/DESCRIPTION2_2 vides ou "-" (sinon la regle 1
                // aurait deja produit un resultat), PTC_COMMON_NAME renseigne mais sans traduction
                // Webterm FR exploitable, et DESCRIPTION_2 renseignee (ni vide ni "-").
                if (IsMeaningful(description2))
                {
                    return DescriptionMthCalculationResult.Success(
                        DescriptionMthRule.PtcCommonNameAndDescription2,
                        Join(ptcCommonName!, description2!));
                }
            }

            // Regle de secours : aucune regle n'a produit de resultat exploitable.
            return DescriptionMthCalculationResult.NoResult();
        }

        /// <summary>
        /// Traduit <paramref name="ptcCommonName"/> en francais via Webterm, avec mise en cache pour
        /// eviter les appels identiques repetes pendant une meme lecture. Distingue explicitement une
        /// absence de traduction (retour null, pas une erreur) d'une erreur reelle du service Webterm.
        /// </summary>
        private DescriptionMthCalculationResult GetOrTranslate(string ptcCommonName)
        {
            var cacheKey = ptcCommonName + "|" + _translationLanguage;

            return _webtermCache.GetOrAdd(cacheKey, _ =>
            {
                try
                {
                    var translation = _webtermTools.GetTermFromEnglish(ptcCommonName, _translationLanguage);
                    var normalizedTranslation = Normalize(translation);

                    return IsMeaningful(normalizedTranslation)
                        ? DescriptionMthCalculationResult.Success(DescriptionMthRule.WebtermTranslation, normalizedTranslation!)
                        : DescriptionMthCalculationResult.NoResult();
                }
                catch (Exception ex)
                {
                    // Erreur reelle du service Webterm (reseau, base indisponible, ...), a ne pas
                    // confondre avec une simple absence de traduction pour ce terme.
                    return DescriptionMthCalculationResult.WebtermFailure(ex.Message);
                }
            });
        }

        /// <summary>
        /// Considere null, chaine vide, chaine faite uniquement d'espaces, et "-" (apres trim) comme
        /// non renseignes.
        /// </summary>
        private static bool IsMeaningful(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return !string.Equals(value.Trim(), "-", StringComparison.Ordinal);
        }

        /// <summary>Normalise une valeur source : null/blanc reste tel quel apres trim, jamais de casse modifiee.</summary>
        private static string? Normalize(string? value)
        {
            return value?.Trim();
        }

        /// <summary>
        /// Joint deux valeurs deja jugees significatives avec le separateur "|", sans jamais produire
        /// de double separateur.
        /// </summary>
        private static string Join(string left, string right)
        {
            return left + "|" + right;
        }
    }
}
