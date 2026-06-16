
using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.MassUpdateAttribute.View
{
    public interface IMassUpdateAttributeViewModel
    {
        MassUpdateAttributeDataContext CurrentMassUpdAttribDataContext { get; set; }

        ICommand CommandBtHelpMouseLeftButtonUpEvent { get; }
        ICommand CommandListCadDoc { get; }
        ICommand CommandUpdateCadDoc { get; }
        ICommand CommandDisplayedModelsOnly { get; }
        ICommand CommandActiveModelOnly { get; }
        ICommand CommandShowCheckedOut { get; }
        ICommand CommandShowLocallyModified { get; }
        ICommand CommandShowReadOnly { get; }

        ICommand CommandApplyWebtermSelectedItem { get; }
        ICommand CommandApplyToAllSamePartNumber { get; }
        ICommand CommandCheckOutOneCadDoc { get; }
        ICommand CommandCheckInOneCadDoc { get; }
        ICommand CommandResetUpdateSelectedItem { get; }

        ICommand CommandSelectUnselectAll { get; }
        ICommand CommandSelectUnselectAllRename { get; }

        ICommand CommandApplyWebtermToSelected { get; }
        ICommand CommandCheckInAllCadDoc { get; }
        ICommand CommandCheckOutAllCadDoc { get; }
        ICommand CommandResetUpdateAllSelectedItem { get; }

        ICommand CommandApplyAllHeaderValue { get; }

        ICommand CommandImportFromExcel { get; }
        ICommand CommandUpdateParamRelation { get; }
        ICommand CommandUpdateLayers { get; }
        ICommand CommandOpenModelInCreo { get; }
        ICommand CommandRenameObject { get; }

        ICommand CommandRemoveColor { get; }
        ICommand CommandUpdateColorPalette { get; }
        ICommand CommandUpdateColor { get; }
        ICommand CommandDeleteSelectedCadDoc { get; }

        ICommand CommandStartRename { get; }
       // ICommand CommandClose { get; }
        ICommand CommandRemove { get; }
    }
}
