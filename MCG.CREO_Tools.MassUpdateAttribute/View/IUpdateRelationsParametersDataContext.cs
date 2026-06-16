using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MassUpdateAttribute.View
{
    public interface IUpdateRelationsParametersDataContext
    {
        string ActiveModelFileName { get; set; }
        string CadDocType { get; set; }
        bool IsCreoEnable { get; set; }

        bool IsPleaseWaitShown { get; set; }
        int NbModels { get; set; }
        int NbModelsInProgress { get; set; }

        bool IsUpperLevelSelected { get; set; }
        bool IsOneLevelSelected { get; set; }
        bool IsAllLevelsSelected { get; set; }

        ObservableCollection<UpdateRelationsParametersItem> ListItem { get; set; }
    }
}
