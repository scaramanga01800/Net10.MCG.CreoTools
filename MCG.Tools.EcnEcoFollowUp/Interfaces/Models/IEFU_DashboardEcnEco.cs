using MCG.Tools.EcnEcoFollowUp.Models;

namespace MCG.Tools.EcnEcoFollowUp.Interfaces.Models
{
    interface IEFU_DashboardEcnEco
    {
        EFU_EcnEcoToShowEndUser EcnEcoToShowEndUser { get; set; }
        string Department { get; set; }
        string Comment { get; set; }
        string ApprovalEcnStep { get; set; }
        string Information { get; set; }
        string SapOrder { get; set; }
        int? EcoTimeResolution { get; set; }
        bool IsSelected { get; set; }
        string Priority { get; set; }

        event EventHandler IsSelectedEvent;
        event EventHandler IsUpdateEvent;
    }
}
