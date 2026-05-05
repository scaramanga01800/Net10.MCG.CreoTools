using MCG.Tools.EcnEcoFollowUp.Models;

namespace MCG.Tools.EcnEcoFollowUp.Interfaces.Models
{
    public interface IEFU_EcnEcoToShowEndUser
    {
        EFU_Status Status { get; set; }

        EFU_EcnEcoFollowUp EcnEcoFollowUp { get; set; }

        bool IsSelected { get; set; }

        event EventHandler IsSelectedEvent;
    }
}
