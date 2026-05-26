using MCG.CREO_Tools.MiscTools.ViewModel.CadDocRename;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.CadDocRename
{
    internal interface ICadDocRenameDataContext
    {
        string CadNumber { get; set; }
        bool IsCreoConnected { get; set; }
        bool IsRenamedPossible { get; set; }
        int SelectedLeadingZero { get; set; }
        ObservableCollection<int> ListLeadingZero { get; set; }

        ObservableCollection<CadDocRenameItem> ListItem { get; set; }

        int NbModels { get; set; }
        int NbModelsInProgress { get; set; }
        bool IsPleaseWaitShown { get; set; }

    }
}
