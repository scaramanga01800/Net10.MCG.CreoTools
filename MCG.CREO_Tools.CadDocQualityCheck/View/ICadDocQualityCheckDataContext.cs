using MCG.CREO_Tools.CadDocQualityCheck.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.CadDocQualityCheck.View
{
    interface ICadDocQualityCheckDataContext
    {
        bool ShowActionButton { get; set; }
        bool IsSearchCadModelInProgress { get; set; }
        bool IsOnlyDisplayedModels { get; set; }
        bool IsOnlyActiveModel { get; set; }
        bool IsLoadedFromCreo { get; set; }
        bool IsCheckedOutShown { get; set; }
        bool IsLocallyModifiedShown { get; set; }
        bool IsReadOnlyShown { get; set; }
        bool IsNoActionInProgress { get; set; }

        bool IsCheckDone { get; set; }

        ObservableCollection<CadDocQualityCheckItem> ShownCadModels { get; set; }
        CadDocQualityCheckItem SelectedItem { get; set; }
        int SelectedIndex { get; set; }
        bool IsAllSelected { get; set; }

        string TextStatusBar { get; set; }

        long NbModelsInSession { get; set; }
        long NbModelsInSessionInProgress { get; set; }

        bool CheckUncheckedOutItem { get; set; }
        bool ForceTypeProeUpdate { get; set; }

    }
}
