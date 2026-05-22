namespace MCG.CREO_Tools.MiscTools.Interfaces
{
    public interface IMiscToolsWindchillService
    {
        void closeBomEnvirConfigMainView();
        void closeCraneSearchMainView();

        void ShowBomEnvirConfigMainView( bool isAlreadyCreated = false);
        void ShowDialogBomEnvirConfigMainView(bool isAlreadyCreated = false);

        void ShowAndExecuteCraneSearchMainView(List<string> listObject, bool isAlreadyCreated);
        void ShowCraneSearchMainView(List<string> listObject, bool isAlreadyCreated);
        void ShowDialogCraneSearchMainView(List<string> listObject);
    }
}