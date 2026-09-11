using MCG.CREO_Tools.MiscTools.ViewModel.Manufacturing;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.Manufacturing
{
    /// <summary>
    /// Contrat d'etat UI de la fenetre Manufacturing View.
    /// </summary>
    internal interface IManufacturingDataContext
    {
        bool IsCreoConnected { get; set; }
        bool IsPleaseWaitShown { get; set; }
        bool IsAssemblyLoaded { get; set; }
        bool IsActiveModelModifiable { get; set; }
        bool HasPendingChanges { get; set; }

        string ActiveModelName { get; set; }

        int NbModels { get; set; }
        int NbModelsInProgress { get; set; }

        ObservableCollection<ManufacturingComponentItem> ListItem { get; set; }
    }
}
