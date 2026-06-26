using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.Models.Main;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCG.WindchillTools.ManageWTObject.Interfaces
{
    public interface IMcgWindchillToolsManageWTObjectWindowService
    {
        void ShowSearchWtDocumentPartView(CreateUpdateWtDocumentWtPartViewModel currentDataContext);
        void ShowCreateWtDocumentMainView(List<string> pListWindchillDocumentType,
                                                 List<string> pListWindchillPartType,
                                                 List<Webterm> pAllWebterm,
                                                 MCGLanguage LocalLanguage,
                                                 List<WindchillContext> pWindchillContextList,
                                                 List<string> ListGroup,
                                                 List<string> ListBrand);
        void ShowCreateUpdateWtDocumentWtPartViewModel();

        void ShowDialogSearchWtDocumentPartView(CreateUpdateWtDocumentWtPartViewModel currentDataContext);
        public (bool? DialogResult, MgtWtDocumentItem WtDocItem) ShowDialogCreateWtDocumentMainView(List<string> pListWindchillDocumentType,
                                                 List<string> pListWindchillPartType,
                                                 List<Webterm> pAllWebterm,
                                                 MCGLanguage LocalLanguage,
                                                 List<WindchillContext> pWindchillContextList,
                                                 List<string> ListGroup,
                                                 List<string> ListBrand);
        void ShowDialogCreateUpdateWtDocumentWtPartViewModel(bool isAlreadyCreated = false);

        void CloseSearchWtDocumentPartView();
        void CloseCreateWtDocumentMainView();
        void CloseCreateUpdateWtDocumentWtPartViewModel();
    }
}
