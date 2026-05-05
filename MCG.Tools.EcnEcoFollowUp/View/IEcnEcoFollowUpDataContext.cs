using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MCG.Tools.EcnEcoFollowUp.View
{
    public interface IEcnEcoFollowUpDataContext
    {
        string EcnNumber { get; set; }
        bool IsOtherFieldEnable { get; set; }
        DateTime? CreatedAfter { get; set; }
        DateTime? CreatedBefore { get; set; }
        DateTime? ResolvedAfter { get; set; }
        DateTime? ResolvedBefore { get; set; }

        ObservableCollection<string> PdmProductList { get; set; }
        string PdmProduct { get; set; }

        ObservableCollection<string> EcnStateList { get; set; }
        string EcnState { get; set; }

        string KeyWords { get; set; }
        string EcnCreator { get; set; }

        DateTime? CreatedAfterSap { get; set; }
        DateTime? CreatedBeforeSap { get; set; }

        string KeyWordsProject { get; set; }
        string KeyWordsCategory { get; set; }

        bool IsStatusNotCreated { get; set; }
        bool IsStatus99 { get; set; }
        bool IsStatus01 { get; set; }
        bool IsStatus02 { get; set; }
        bool IsStatus03 { get; set; }

        int NbParts { get; set; }
        int NbPartsPdmApproved { get; set; }
        int NbPartsSapApproved { get; set; }

        int NbDrawings { get; set; }
        int NbDrawingsPdmApproved { get; set; }
        int NbDrawingsSapApproved { get; set; }

        int NbEpmDoc { get; set; }
        int NbEpmDocPdmApproved { get; set; }
        int NbEpmDocSapApproved { get; set; }

        int NbWtDoc { get; set; }
        int NbWtDocPdmApproved { get; set; }
        int NbWtDocSapApproved { get; set; }

        int NbEcn { get; set; }
        int NbEcnPdmApproved { get; set; }
        int NbEcnSapApproved { get; set; }

        string StatusBarText { get; set; }

        ObservableCollection<EFU_EcnEcoToShowEndUser> EcnShownList { get; set; }
        EFU_EcnEcoToShowEndUser SelectedEcn { get; set; }

        List<EFU_SearchTemplate> SavedSearchesList { get; set; }
        List<EFU_SearchTemplate> RecentSearchesList { get; set; }

        EcnEcoFollowUpDashboardView PersonalDashboard { get; set; }

        List<EcnEcoFollowUpDashboardViewModel> DashboardList { get; set; }

        bool IsAdminToolsEnabled { get; set; }

        TabItem SelectedTab { get; set; }

        event EventHandler SavedSearchesListEvent;
        event EventHandler RecentSearchesListEvent;
        event EventHandler DashboardListEvent;


    }
}
