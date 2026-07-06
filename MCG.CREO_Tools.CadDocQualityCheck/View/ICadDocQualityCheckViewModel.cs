using MCG.CREO_Tools.CadDocQualityCheck.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.CadDocQualityCheck.View
{
    interface ICadDocQualityCheckViewModel
    {
        ICommand CommandListCadDoc { get; }
        ICommand CommandCheckCadDoc { get; }
        ICommand CommandUpdateRelations { get; }
        ICommand CommandUpdateAttributes { get; }
        ICommand CommandUpdateLayers { get; }
        ICommand CommandUpdateUnits { get; }
        ICommand CommandSelectUnselectAll { get; }
        ICommand CommandCopyPreRelations { get; }
        ICommand CommandInitPreRelations { get; }
        ICommand CommandCopyPostRelations { get; }
        ICommand CommandInitPostRelations { get; }
        ICommand CommandImportFromExcel { get; }
        ICommand CommandOpenModelInCreo { get; }
        ICommand CommandCheckIn { get; }
        ICommand CommandCheckOut { get; }
        ICommand CommandDeleteSelectedCadDoc { get; }
        ICommand CommandUpdateComponentAssembly { get; }

        CadDocQualityCheckDataContext CurrentDataContext { get; set; }
    }
}
