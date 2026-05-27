namespace MCG.CREO_Tools.MiscTools.Interfaces
{
    public interface IMiscToolsWindchillService
    {
        void CloseBomEnvirConfigMainView();
        void CloseCraneSearchMainView();
        void CloseCadAutoColorMainView();
        void CloseCadDocRenameMainView();
        void CloseMechanismAnalysisMainView();
        void CloseNumberCumulationMainView();
        void CloseQuickChangeMainView();
        void CloseSapBomExportMainView();
        void CloseSapBomExportAllLevelMainView();

        void ShowBomEnvirConfigMainView( bool isAlreadyCreated = false);
        void ShowDialogBomEnvirConfigMainView();

        void ShowCadAutoColorMainView(bool isAlreadyCreated = false);
        void ShowDialogCadAutoColorMainView();

        void ShowCadDocRenameMainView(bool isAlreadyCreated = false);
        void ShowDialogCadDocRenameMainView();

        void ShowAndExecuteCraneSearchMainView(List<string> listObject, bool isAlreadyCreated);
        void ShowCraneSearchMainView(List<string> listObject, bool isAlreadyCreated);
        void ShowDialogCraneSearchMainView(List<string> listObject);

        void ShowMechanismAnalysisMainView(bool isAlreadyCreated = false);
        void ShowDialogMechanismAnalysisMainView();

        void ShowNumberCumulationMainView(bool isAlreadyCreated = false);
        void ShowDialogNumberCumulationMainView();

        void ShowQuickChangeMainView(bool isAlreadyCreated = false);
        void ShowDialogQuickChangeMainView();

        void ShowSapBomExportMainView(bool isAlreadyCreated = false);
        void ShowDialogSapBomExportMainView();

        void ShowSapBomExportAllLevelMainView(bool isAlreadyCreated = false);
        void ShowDialogSapBomExportAllLevelMainView();
    }
}