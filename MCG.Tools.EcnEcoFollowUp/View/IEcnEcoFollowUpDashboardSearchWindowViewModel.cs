using MCG.Tools.EcnEcoFollowUp.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MCG.Tools.EcnEcoFollowUp.View
{
    interface IEcnEcoFollowUpDashboardSearchWindowViewModel
    {
        EcnEcoFollowUpDashboardSearchWindow ParentWindow { set; get; }

        string CreatedByFullName { set; get; }
        string CreatedById { set; get; }
        string DashboardName { set; get; }
        string DashboardID { set; get; }

        ObservableCollection<EFU_DashboardItem> ListSearchedDashboard { set; get; }

        ICommand CommandSearchDashboard { get; }
        ICommand CommandAddDashboard { get; }
        ICommand CommandClose { get; }

    }
}
