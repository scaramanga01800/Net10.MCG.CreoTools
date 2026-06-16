using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.MassUpdateAttribute.View
{
    public interface IUpdateRelationsParametersViewModel
    {
        UpdateRelationsParametersDataContext CurrentDataContext { get; set; }

        ICommand CommandUpdateActiveCadModel { get; }
        ICommand CommandReadAndUpdateCadDocument { get; }

    }
}
