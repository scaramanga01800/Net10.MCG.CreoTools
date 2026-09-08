using MCG.CREO_Tools.MiscTools.ViewModel.SimplifiedRep;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.SimplifiedRep
{
    internal interface ISimplifiedRepDataContext
    {
        bool IsCreoConnected { get; set; }
        bool IsPleaseWaitShown { get; set; }
        bool IsAssemblyLoaded { get; set; }
        bool IsSimpRepSelected { get; set; }

        string ActiveModelName { get; set; }
        string NewSimpRepName { get; set; }
        string SelectedSimpRepName { get; set; }
        string SelectedDefaultRule { get; set; }

        ObservableCollection<string> ListSimpRepName { get; set; }
        ObservableCollection<string> ListDefaultRule { get; set; }
        ObservableCollection<SimplifiedRepComponentItem> ListItem { get; set; }

        int NbModels { get; set; }
        int NbModelsInProgress { get; set; }
    }
}
