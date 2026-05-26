namespace MCG.CREO_Tools.MiscTools.Interfaces
{
    public interface IMiscToolsWindchillService
    {
        void closeBomEnvirConfigMainView();
        void closeCraneSearchMainView();
        void closeCadAutoColorMainView();

        void ShowBomEnvirConfigMainView( bool isAlreadyCreated = false);
        void ShowDialogBomEnvirConfigMainView();

        void ShowCadAutoColorMainView(bool isAlreadyCreated = false);
        void ShowDialogCadAutoColorMainView();

        void ShowCadDocRenameMainView(bool isAlreadyCreated = false);
        void ShowDialogCadDocRenameMainView();
        void closeCadDocRenameMainView();

        void ShowAndExecuteCraneSearchMainView(List<string> listObject, bool isAlreadyCreated);
        void ShowCraneSearchMainView(List<string> listObject, bool isAlreadyCreated);
        void ShowDialogCraneSearchMainView(List<string> listObject);
    }
}