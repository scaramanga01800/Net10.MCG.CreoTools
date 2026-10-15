using MCG.Tools.EcnEcoFollowUp.Models.Workflow;

namespace MCG.Tools.EcnEcoFollowUp.Interfaces
{
    /// <summary>
    /// Pure, deterministic reconstruction of the historical ExtractPdmWfTaskDate calculation.
    /// No Windchill, database, credential, UI or logging dependency.
    /// </summary>
    public interface IEcnWorkflowDateCalculator
    {
        /// <param name="tasks">
        /// Workflow tasks in the historical order (ECN tasks first, then ECA tasks).
        /// The order matters: the first task and the first "Complete Change Notice Task" provide the ECN state.
        /// </param>
        /// <exception cref="ArgumentNullException">tasks is null.</exception>
        /// <exception cref="ArgumentException">tasks contains a null element.</exception>
        EcnWorkflowDateResult Calculate(IReadOnlyList<EcnWorkflowTaskItem> tasks);
    }
}
