using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;

namespace MCG.CREO_Tools.MassUpdateAttribute.Interfaces
{
    public interface IMassUpdateAttributeWindowService
    {
        void ShowCreateNewCadDocumentFluentWindow(bool isAlreadyCreated = false);
        void ShowDialogCreateNewCadDocumentFluentWindow();
        void CloseCreateNewCadDocumentFluentWindow();

        void ShowMassUpdateAttributeChangeName(MassUpdateAttributeViewModel dataContext, bool isAlreadyCreated = false);
        void ShowDialogMassUpdateAttributeChangeName(MassUpdateAttributeViewModel dataContext);
        void CloseMassUpdateAttributeChangeName();

        void ShowUpdateRelationsParametersMainView(bool isAlreadyCreated = false);
        void ShowDialogUpdateRelationsParametersMainView();
        void CloseUpdateRelationsParametersMainView();

    }
}