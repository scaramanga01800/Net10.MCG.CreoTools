using MCG.CREO_Tools.MiscTools.Interfaces;
using MCG.CREO_Tools.MiscTools.View.BomEnvirConfig;
using MCG.CREO_Tools.MiscTools.View.CadAutoColor;
using MCG.CREO_Tools.MiscTools.View.CadDocRename;
using MCG.CREO_Tools.MiscTools.View.CraneSearch;
using MCG.CREO_Tools.MiscTools.View.MechanismTool;
using Microsoft.Extensions.DependencyInjection;

using System.Windows;

namespace MCG.CREO_Tools.MiscTools.Services
{
    internal class MiscToolsWindchillService : IMiscToolsWindchillService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _CraneSearchMainView;
        private Window _BomEnvirConfigMainView;
        private Window _CadAutoColorMainView;
        private Window _CadDocRenameMainView;
        private Window _MechanismAnalysisMainView;

        public MiscToolsWindchillService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowBomEnvirConfigMainView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_BomEnvirConfigMainView != null && _BomEnvirConfigMainView.IsVisible)
                {
                    _BomEnvirConfigMainView.Activate();
                    return;
                }
            }
            _BomEnvirConfigMainView = _serviceProvider.GetRequiredService<BomEnvirConfigMainView>();
            _BomEnvirConfigMainView.Show();
        }
        public void ShowDialogBomEnvirConfigMainView()
        {
            _BomEnvirConfigMainView = _serviceProvider.GetRequiredService<BomEnvirConfigMainView>();
            _BomEnvirConfigMainView.ShowDialog();
        }

        public void ShowCadAutoColorMainView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_CadAutoColorMainView != null && _CadAutoColorMainView.IsVisible)
                {
                    _CadAutoColorMainView.Activate();
                    return;
                }
            }
            _CadAutoColorMainView = _serviceProvider.GetRequiredService<CadAutoColorMainView>();
            _CadAutoColorMainView.Show();
        }
        public void ShowDialogCadAutoColorMainView()
        {
            _CadAutoColorMainView = _serviceProvider.GetRequiredService<CadAutoColorMainView>();
            _CadAutoColorMainView.ShowDialog();
        }

        public void ShowCadDocRenameMainView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_CadDocRenameMainView != null && _CadDocRenameMainView.IsVisible)
                {
                    _CadDocRenameMainView.Activate();
                    return;
                }
            }
            _CadDocRenameMainView = _serviceProvider.GetRequiredService<CadDocRenameMainView>();
            _CadDocRenameMainView.Show();
        }
        public void ShowDialogCadDocRenameMainView()
        {
            _CadDocRenameMainView = _serviceProvider.GetRequiredService<CadDocRenameMainView>();
            _CadDocRenameMainView.ShowDialog();
        }

        public void ShowCraneSearchMainView(List<string> listObject, bool isAlreadyCreated)
        {
            if (isAlreadyCreated)
            {
                if (_CraneSearchMainView != null && _CraneSearchMainView.IsVisible)
                {
                    _CraneSearchMainView.Activate();
                    return;
                }
            }
            _CraneSearchMainView = _serviceProvider.GetRequiredService<CraneSearchMainView>();
            ((CraneSearchMainView)_CraneSearchMainView).SetCraneSearchViewModelProperties(listObject);
            _CraneSearchMainView.Show();
        }
        public void ShowAndExecuteCraneSearchMainView(List<string> listObject, bool isAlreadyCreated)
        {
            if (isAlreadyCreated)
            {
                if (_CraneSearchMainView != null && _CraneSearchMainView.IsVisible)
                {
                    _CraneSearchMainView.Activate();
                    return;
                }
            }

            _CraneSearchMainView = _serviceProvider.GetRequiredService<CraneSearchMainView>();
            ((CraneSearchMainView)_CraneSearchMainView).SetCraneSearchViewModelProperties(listObject);
            _CraneSearchMainView.Show();
            ((CraneSearchMainView)_CraneSearchMainView).CurrentDataContext.CommandSearchSapCrane.Execute(null);
        }
        public void ShowDialogCraneSearchMainView(List<string> listObject)
        {
            var view = _serviceProvider.GetRequiredService<CraneSearchMainView>();
            ((CraneSearchMainView)view).SetCraneSearchViewModelProperties(listObject);
            view.ShowDialog();
        }

        public void ShowMechanismAnalysisMainView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_MechanismAnalysisMainView != null && _MechanismAnalysisMainView.IsVisible)
                {
                    _MechanismAnalysisMainView.Activate();
                    return;
                }
            }
            _MechanismAnalysisMainView = _serviceProvider.GetRequiredService<MechanismAnalysisMainView>();
            _MechanismAnalysisMainView.Show();
        }
        public void ShowDialogMechanismAnalysisMainView()
        {
            _MechanismAnalysisMainView = _serviceProvider.GetRequiredService<MechanismAnalysisMainView>();
            _MechanismAnalysisMainView.ShowDialog();
        }

        public void closeBomEnvirConfigMainView()
        {
            if (_BomEnvirConfigMainView != null)
            {
                _BomEnvirConfigMainView.Close();
                _BomEnvirConfigMainView = null;
            }
        }
        public void closeCadAutoColorMainView()
        {
            if (_CadAutoColorMainView != null)
            {
                _CadAutoColorMainView.Close();
                _CadAutoColorMainView = null;
            }
        }
        public void closeCadDocRenameMainView()
        {
            if (_CadDocRenameMainView != null)
            {
                _CadDocRenameMainView.Close();
                _CadDocRenameMainView = null;
            }
        }
        public void closeCraneSearchMainView()
        {
            if (_CraneSearchMainView != null)
            {
                _CraneSearchMainView.Close();
                _CraneSearchMainView = null;
            }
        }
        public void closeMechanismAnalysisMainView()
        {
            if (_MechanismAnalysisMainView != null)
            {
                _MechanismAnalysisMainView.Close();
                _MechanismAnalysisMainView = null;
            }
        }
    }
}
