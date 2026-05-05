namespace MCG.Tools.EcnEcoFollowUp.Interfaces.Models
{
    public interface IEFU_SapHupOracle_DmEcoTasks
    {
        string TYPE_ITEM { get; set; }
        string WI_STATUS { get; set; }
        string CALCULATED_PLANT_DESC { get; set; }
        string WI_ACTUAL_AGENT { get; set; }

        DateTime? WI_CREATION_DATE { get; set; }
        DateTime? WI_END_DATE { get; set; }
    }
}
