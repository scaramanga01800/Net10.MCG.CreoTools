namespace MCG.CREO_Tools.MiscTools.Interfaces
{
    public interface IMiscToolsWindchillService
    {
        void closeCraneSearchMainView();
        void ShowAndExecuteCraneSearchMainView(List<string> listObject, bool isAlreadyCreated);
        void ShowCraneSearchMainView(List<string> listObject, bool isAlreadyCreated);
        void ShowDialogCraneSearchMainView(List<string> listObject);
    }
}