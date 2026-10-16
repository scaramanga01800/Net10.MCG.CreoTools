using MCG.CommonLib.DataBaseAccess.Models.SapHupDb;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.EcoSapHup;

namespace MCG.Tools.EcnEcoFollowUp.Services.EcoSapHup
{
    /// <summary>
    /// Reconstruction of the historical workflow-step rules (EcnEcoFollowUpManagement.SearchSapHupDmEcoViewStatusFromList).
    /// Historical reader returned "" for DB NULL strings: null values are normalized to "" before comparison.
    /// </summary>
    public sealed class EcoSapHupStepCalculator : IEcoSapHupStepCalculator
    {
        public const string CompletedStatus = "COMPLETED";

        public EcoSapHupStepResult Calculate(IReadOnlyList<DmEcoViewWfEcoTask> tasks)
        {
            ArgumentNullException.ThrowIfNull(tasks);

            if (tasks.Count == 0)
                return EcoSapHupStepResult.NoTask;

            var openTasks = tasks.Where(t => !string.Equals(N(t.WiStatus), CompletedStatus, StringComparison.Ordinal)).ToList();

            var mainTasks = openTasks.Where(t => string.Equals(N(t.CalculatedPlant), N(t.EcoCoord), StringComparison.Ordinal)).ToList();
            var secondaryTasks = openTasks.Where(t => !string.Equals(N(t.CalculatedPlant), N(t.EcoCoord), StringComparison.Ordinal)).ToList();

            string mainStep = string.Empty;
            DateTime? mainDate = null;
            if (mainTasks.Count > 0)
            {
                var selected = SelectMaxTypeItem(mainTasks);
                mainStep = N(selected.TypeItem);
                mainDate = selected.WiCreationDate;
            }

            string secondaryStep = string.Empty;
            DateTime? secondaryDate = null;
            string secondaryPlants = string.Empty;
            if (secondaryTasks.Count > 0)
            {
                var selected = SelectMaxTypeItem(secondaryTasks);
                secondaryStep = N(selected.TypeItem);
                secondaryDate = selected.WiCreationDate;
                secondaryPlants = FormatSecondaryPlants(secondaryTasks.Select(t => N(t.CalculatedPlant)));
            }

            return new EcoSapHupStepResult(true, mainStep, mainDate, secondaryStep, secondaryDate, secondaryPlants);
        }

        /// <summary>Historical: OrderBy(TYPE_ITEM).LastOrDefault() (stable sort: last of ties in input order wins).</summary>
        private static DmEcoViewWfEcoTask SelectMaxTypeItem(List<DmEcoViewWfEcoTask> tasks)
            => tasks.OrderBy(t => N(t.TypeItem), StringComparer.Ordinal).Last();

        /// <summary>
        /// Distinct plants (ordinal), sorted ordinally so the persisted value does not depend on the order returned by Oracle
        /// (the SAP HUP query has no stable ORDER BY). Separator "," unchanged; no trimming, casing preserved.
        /// Plant order carries no business meaning: the column is only displayed as text.
        /// </summary>
        public static string FormatSecondaryPlants(IEnumerable<string> plants)
            => string.Join(",", plants.Distinct(StringComparer.Ordinal).OrderBy(p => p, StringComparer.Ordinal));

        private static string N(string? value) => value ?? string.Empty;
    }
}
