using MCG.CREO_Tools.MiscTools.ViewModel.QuickChange;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.QuickChange
{
    public interface IQuickChangeDataContext
    {
        bool IsCreoEnable { get; set; }
        bool IsPleaseWaitShown { get; set; }
        int NbModels { get; set; }
        int NbModelsInProgress { get; set; }
        bool AllLevel { get; set; }

        ObservableCollection<QuickChangeItem> ListItem { get; set; }
    }
}
