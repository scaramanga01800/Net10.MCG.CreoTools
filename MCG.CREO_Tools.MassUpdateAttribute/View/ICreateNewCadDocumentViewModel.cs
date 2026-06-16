using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.MassUpdateAttribute.View
{
    interface ICreateNewCadDocumentViewModel
    {
        CreateNewCadDocumentDataContext CurrentCreateNewCadDocumentDataContext { get; set; }

        ICommand CommandCreateCadDoc { get; }
        ICommand CommandCancel { get; }
        ICommand CommandPartNumberGenerator { get; }

    }
}
