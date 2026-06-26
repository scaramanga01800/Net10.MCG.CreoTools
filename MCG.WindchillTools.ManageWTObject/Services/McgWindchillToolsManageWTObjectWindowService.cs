using DocumentFormat.OpenXml.Office.CoverPageProps;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.Models.Main;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.Interfaces;
using MCG.WindchillTools.ManageWTObject.View;
using MCG.WindchillTools.ManageWTObject.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MCG.WindchillTools.ManageWTObject.Services
{
    public class McgWindchillToolsManageWTObjectWindowService : IMcgWindchillToolsManageWTObjectWindowService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _SearchWtDocumentPartView;
        private Window _CreateWtDocumentMainView;
        private Window _CreateUpdateWtDocumentWtPartViewModel;

        public McgWindchillToolsManageWTObjectWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowSearchWtDocumentPartView(CreateUpdateWtDocumentWtPartViewModel currentDataContext)
        {
            _SearchWtDocumentPartView = _serviceProvider.GetRequiredService<SearchWtDocumentPartView>();
            _SearchWtDocumentPartView.DataContext = currentDataContext;
            _SearchWtDocumentPartView.Show();
        }

        public void ShowCreateWtDocumentMainView(List<string> pListWindchillDocumentType,
                                                 List<string> pListWindchillPartType,
                                                 List<Webterm> pAllWebterm,
                                                 MCGLanguage LocalLanguage,
                                                 List<WindchillContext> pWindchillContextList,
                                                 List<string> ListGroup,
                                                 List<string> ListBrand)
        {
            _CreateWtDocumentMainView = _serviceProvider.GetRequiredService<CreateWtDocumentMainView>();
            ((CreateWtDocumentMainView)_CreateWtDocumentMainView).SetCreateWtDocumentMainViewProperties(pListWindchillDocumentType, pListWindchillPartType, pAllWebterm, LocalLanguage, pWindchillContextList, ListGroup, ListBrand);
            _CreateWtDocumentMainView.Show();
        }

        public void ShowCreateUpdateWtDocumentWtPartViewModel()
        {
            _CreateUpdateWtDocumentWtPartViewModel = _serviceProvider.GetRequiredService<CreateUpdateWtDocumentWtPartMainView>();
            _CreateUpdateWtDocumentWtPartViewModel.Show();
        }



        public void ShowDialogSearchWtDocumentPartView(CreateUpdateWtDocumentWtPartViewModel currentDataContext)
        {
            _SearchWtDocumentPartView = _serviceProvider.GetRequiredService<SearchWtDocumentPartView>();
            _SearchWtDocumentPartView.DataContext = currentDataContext;
            _SearchWtDocumentPartView.ShowDialog();
        }
        public (bool? DialogResult, MgtWtDocumentItem WtDocItem) ShowDialogCreateWtDocumentMainView(List<string> pListWindchillDocumentType,
                                                       List<string> pListWindchillPartType,
                                                       List<Webterm> pAllWebterm,
                                                       MCGLanguage LocalLanguage,
                                                       List<WindchillContext> pWindchillContextList,
                                                       List<string> ListGroup,
                                                       List<string> ListBrand)
        {
            _CreateWtDocumentMainView = _serviceProvider.GetRequiredService<CreateWtDocumentMainView>();
            ((CreateWtDocumentMainView)_CreateWtDocumentMainView).SetCreateWtDocumentMainViewProperties(pListWindchillDocumentType, pListWindchillPartType, pAllWebterm, LocalLanguage, pWindchillContextList, ListGroup, ListBrand);
            _CreateWtDocumentMainView.ShowDialog();
            return (_CreateWtDocumentMainView.DialogResult, ((CreateWtDocumentMainView)_CreateWtDocumentMainView).WtDocumentItem);
        }

        public void ShowDialogCreateUpdateWtDocumentWtPartViewModel(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_CreateUpdateWtDocumentWtPartViewModel != null && _CreateUpdateWtDocumentWtPartViewModel.IsVisible)
                {
                    _CreateUpdateWtDocumentWtPartViewModel.Activate();
                    return;
                }
            }
            _CreateUpdateWtDocumentWtPartViewModel = _serviceProvider.GetRequiredService<CreateUpdateWtDocumentWtPartMainView>();
            _CreateUpdateWtDocumentWtPartViewModel.ShowDialog();
        }

        public void CloseSearchWtDocumentPartView()
        {
            if (_SearchWtDocumentPartView != null)
            {
                _SearchWtDocumentPartView.Close();
                _SearchWtDocumentPartView = null;
            }
        }
        public void CloseCreateWtDocumentMainView()
        {
            if (_CreateWtDocumentMainView != null)
            {
                _CreateWtDocumentMainView.Close();
                _CreateWtDocumentMainView = null;
            }
        }
        public void CloseCreateUpdateWtDocumentWtPartViewModel()
        {
            if (_CreateUpdateWtDocumentWtPartViewModel != null)
            {
                _CreateUpdateWtDocumentWtPartViewModel.Close();
                _CreateUpdateWtDocumentWtPartViewModel = null;
            }
        }
    }
}
