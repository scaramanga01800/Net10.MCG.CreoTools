using MCG.CREO_Tools.MiscTools.ViewModel.BomEnvirConfig;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.BomEnvirConfig
{
    public interface IBomEnvirConfigDataContext
    {
        string ActiveModelFileName { get; set; }
        string CadDocType { get; set; }
        bool IsCreoEnable { get; set; }

        bool IsPleaseWaitShown { get; set; }
        int NbModels { get; set; }
        int NbModelsInProgress { get; set; }

        string AsmNameValue { get; set; }

        ObservableCollection<BomEnvirConfigItem> ListItem { get; set; }

        event EventHandler AsmNameChangedEvent;
    }
}
