using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.Tools.EcnEcoFollowUp.Interfaces.Models
{
    public interface IEFU_DashboardItem
    {
        string Name { get; set; }
        string Id { get; set; }
        string CreatedBy { get; set; }
        string GeneralComment { get; set; }
        DateTime? CreatedOn { get; set; }

        bool IsShown { get; set; }
        bool IsActive { get; set; }
        bool IsCreator { get; set; }
        bool IsSelected { get; set; }
        bool IsShared { get; set; }
        bool IsReadOnly { get; set; }
        bool UpdateAllowed { get; set; }
        ObservableCollection<EFU_DashboardEcnEco> ListEcnEco { get; set; }
        EFU_DashboardEcnEco SelectedEcnEco { get; set; }
        EcnEcoFollowUpViewModel ParentApp { get; set; }
        ObservableCollection<string> ListPriority { get; set; }

        bool IsAddDeletEcnEcoAllowed { get; set; }
        bool IsHideShowDashboardAllowed { get; set; }
        bool IsPersonalInfoShown { get; set; }

        EFU_DashboardConfiguration CurrentDashboardConfiguration { get; set; }

        bool IsPersonalDashBoard { get; set; }

    }
}
