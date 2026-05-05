namespace MCG.Tools.EcnEcoFollowUp.Interfaces.Models
{
    interface IEFU_DashboardConfiguration
    {
        bool IsStatusNotCreated { get; set; }
        bool IsStatus99 { get; set; }
        bool IsStatus01 { get; set; }
        bool IsStatus02 { get; set; }
        bool IsStatus03 { get; set; }

        bool IsInProgress { get; set; }
        bool IsUnderReview { get; set; }
        bool IsResolved { get; set; }
        bool IsCanceled { get; set; }

        string[] ColumnsOrder { get; set; }

        event EventHandler IsUpdateColumsOrderUserEvent;
    }
}
