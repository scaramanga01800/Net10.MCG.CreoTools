using MCG.CREO_Tools.MiscTools.Interfaces;
using MCG.CREO_Tools.MiscTools.View.BomComparison;
using MCG.CREO_Tools.MiscTools.View.BomEnvirConfig;
using MCG.CREO_Tools.MiscTools.View.BomExport;
using MCG.CREO_Tools.MiscTools.View.CadAutoColor;
using MCG.CREO_Tools.MiscTools.View.CadDocRename;
using MCG.CREO_Tools.MiscTools.View.CraneSearch;
using MCG.CREO_Tools.MiscTools.View.MechanismTool;
using MCG.CREO_Tools.MiscTools.View.NumberCumulation;
using MCG.CREO_Tools.MiscTools.View.QuickChange;
using MCG.CREO_Tools.MiscTools.View.SapBomExport;
using MCG.CREO_Tools.MiscTools.View.SapBomExportAllLevel;
using MCG.CREO_Tools.MiscTools.View.SapFertBom;
using MCG.CREO_Tools.MiscTools.View.WebtermRequest;
using MCG.CREO_Tools.MiscTools.ViewModel.BomExport;
using MCG.WindchillRequestTool.Model.BomComparison;
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
        private Window _NumberCumulationMainView;
        private Window _QuickChangeMainView;
        private Window _SapBomExportMainView;
        private Window _SapBomExportAllLevelMainView;
        private Window _SapFertMissingPart;
        private Window _SapFertBomMainView;
        private Window _WebtermRequestMainView;
        private Window _BomComparisonView;
        private Window _BomExportCumulativeView;
        private Window _BomExportFluentWindowView;

        public MiscToolsWindchillService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowBomComparisonView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_BomComparisonView != null && _BomComparisonView.IsVisible)
                {
                    _BomComparisonView.Activate();
                    return;
                }
            }
            _BomComparisonView = _serviceProvider.GetRequiredService<BomComparisonView>();
            _BomComparisonView.Show();
        }
        public void ShowDialogBomComparisonView()
        {
            _BomComparisonView = _serviceProvider.GetRequiredService<BomComparisonView>();
            _BomComparisonView.ShowDialog();
        }

        public void ShowBomExportCumulativeView(BomExportWindowViewModel dataContext, bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_BomExportCumulativeView != null && _BomExportCumulativeView.IsVisible)
                {
                    _BomExportCumulativeView.Activate();
                    return;
                }
            }
            _BomExportCumulativeView = _serviceProvider.GetRequiredService<BomExportCumulativeView>();
            ((BomExportCumulativeView)_BomExportCumulativeView).SetDataContext(dataContext);
            _BomExportCumulativeView.Show();
        }
        public void ShowDialogBomExportCumulativeView(BomExportWindowViewModel dataContext)
        {
            _BomExportCumulativeView = _serviceProvider.GetRequiredService<BomExportCumulativeView>();
            ((BomExportCumulativeView)_BomExportCumulativeView).SetDataContext(dataContext);
            _BomExportCumulativeView.ShowDialog();
        }

        public void ShowBomExportFluentWindowView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_BomExportFluentWindowView != null && _BomExportFluentWindowView.IsVisible)
                {
                    _BomExportFluentWindowView.Activate();
                    return;
                }
            }
            _BomExportFluentWindowView = _serviceProvider.GetRequiredService<BomExportFluentWindowView>();
            _BomExportFluentWindowView.Show();  
        }
        public void ShowDialogBomExportFluentWindowView()
        {
            _BomExportFluentWindowView = _serviceProvider.GetRequiredService<BomExportFluentWindowView>();
            _BomExportFluentWindowView.ShowDialog();
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

        public void ShowNumberCumulationMainView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_NumberCumulationMainView != null && _NumberCumulationMainView.IsVisible)
                {
                    _NumberCumulationMainView.Activate();
                    return;
                }
            }
            _NumberCumulationMainView = _serviceProvider.GetRequiredService<NumberCumulationMainView>();
            _NumberCumulationMainView.Show();
        }
        public void ShowDialogNumberCumulationMainView()
        {
            _NumberCumulationMainView = _serviceProvider.GetRequiredService<NumberCumulationMainView>();
            _NumberCumulationMainView.ShowDialog();
        }

        public void ShowQuickChangeMainView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_QuickChangeMainView != null && _QuickChangeMainView.IsVisible)
                {
                    _QuickChangeMainView.Activate();
                    return;
                }
            }
            _QuickChangeMainView = _serviceProvider.GetRequiredService<QuickChangeMainView>();
            _QuickChangeMainView.Show();
        }
        public void ShowDialogQuickChangeMainView()
        {
            _QuickChangeMainView = _serviceProvider.GetRequiredService<QuickChangeMainView>();
            _QuickChangeMainView.ShowDialog();
        }

        public void ShowSapBomExportMainView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_SapBomExportMainView != null && _SapBomExportMainView.IsVisible)
                {
                    _SapBomExportMainView.Activate();
                    return;
                }
            }
            _SapBomExportMainView = _serviceProvider.GetRequiredService<SapBomExportMainView>();
            _SapBomExportMainView.Show();
        }
        public void ShowDialogSapBomExportMainView()
        {
            _SapBomExportMainView = _serviceProvider.GetRequiredService<SapBomExportMainView>();
            _SapBomExportMainView.ShowDialog();
        }

        public void ShowSapBomExportAllLevelMainView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_SapBomExportAllLevelMainView != null && _SapBomExportAllLevelMainView.IsVisible)
                {
                    _SapBomExportAllLevelMainView.Activate();
                    return;
                }
            }
            _SapBomExportAllLevelMainView = _serviceProvider.GetRequiredService<SapBomExportAllLevelMainView>();
            _SapBomExportAllLevelMainView.Show();
        }
        public void ShowDialogSapBomExportAllLevelMainView()
        {
            _SapBomExportAllLevelMainView = _serviceProvider.GetRequiredService<SapBomExportAllLevelMainView>();
            _SapBomExportAllLevelMainView.ShowDialog();
        }

        public void ShowSapFertMissingPart(List<BomMissingComponentItem> listMissingComp, bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_SapFertMissingPart != null && _SapFertMissingPart.IsVisible)
                {
                    _SapFertMissingPart.Activate();
                    return;
                }
            }
            _SapFertMissingPart = _serviceProvider.GetRequiredService<SapFertMissingPart>();
            ((SapFertMissingPart)_SapFertMissingPart).ListPart = listMissingComp;
            _SapFertMissingPart.Show();
        }
        public void ShowDialogSapFertMissingPart(List<BomMissingComponentItem> listMissingComp)
        {
            _SapFertMissingPart = _serviceProvider.GetRequiredService<SapFertMissingPart>();
            ((SapFertMissingPart)_SapFertMissingPart).ListPart = listMissingComp;
            _SapFertMissingPart.ShowDialog();
        }

        public void ShowSapFertBomMainView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_SapFertBomMainView != null && _SapFertBomMainView.IsVisible)
                {
                    _SapFertBomMainView.Activate();
                    return;
                }
            }
            _SapFertBomMainView = _serviceProvider.GetRequiredService<SapFertBomMainView>();
            _SapFertBomMainView.Show();
        }
        public void ShowDialogSapFertBomMainView()
        {
            _SapFertBomMainView = _serviceProvider.GetRequiredService<SapFertBomMainView>();
            _SapFertBomMainView.ShowDialog();
        }

        public void ShowWebtermRequestMainView(bool isAlreadyCreated = false)
        {
            if (isAlreadyCreated)
            {
                if (_WebtermRequestMainView != null && _WebtermRequestMainView.IsVisible)
                {
                    _WebtermRequestMainView.Activate();
                    return;
                }
            }
            _WebtermRequestMainView = _serviceProvider.GetRequiredService<WebtermRequestMainView>();
            _WebtermRequestMainView.Show();
        }
        public void ShowDialogWebtermRequestMainView()
        {
            _WebtermRequestMainView = _serviceProvider.GetRequiredService<WebtermRequestMainView>();
            _WebtermRequestMainView.ShowDialog();
        }

        public void CloseBomComparisonView()
        {
            if (_BomComparisonView != null)
            {
                _BomComparisonView.Close();
                _BomComparisonView = null;
            }
        }
        public void CloseBomExportCumulativeView()
        {
            if (_BomExportCumulativeView != null)
            {
                _BomExportCumulativeView.Close();
                _BomExportCumulativeView = null;
            }
        }
        public void CloseBomExportFluentWindowView()
        {
            if (_BomExportFluentWindowView != null)
            {
                _BomExportFluentWindowView.Close();
                _BomExportFluentWindowView = null;
            }
        }
        public void CloseBomEnvirConfigMainView()
        {
            if (_BomEnvirConfigMainView != null)
            {
                _BomEnvirConfigMainView.Close();
                _BomEnvirConfigMainView = null;
            }
        }
        public void CloseCadAutoColorMainView()
        {
            if (_CadAutoColorMainView != null)
            {
                _CadAutoColorMainView.Close();
                _CadAutoColorMainView = null;
            }
        }
        public void CloseCadDocRenameMainView()
        {
            if (_CadDocRenameMainView != null)
            {
                _CadDocRenameMainView.Close();
                _CadDocRenameMainView = null;
            }
        }
        public void CloseCraneSearchMainView()
        {
            if (_CraneSearchMainView != null)
            {
                _CraneSearchMainView.Close();
                _CraneSearchMainView = null;
            }
        }
        public void CloseMechanismAnalysisMainView()
        {
            if (_MechanismAnalysisMainView != null)
            {
                _MechanismAnalysisMainView.Close();
                _MechanismAnalysisMainView = null;
            }
        }
        public void CloseNumberCumulationMainView()
        {
            if (_NumberCumulationMainView != null)
            {
                _NumberCumulationMainView.Close();
                _NumberCumulationMainView = null;
            }
        }
        public void CloseQuickChangeMainView()
        {
            if (_QuickChangeMainView != null)
            {
                _QuickChangeMainView.Close();
                _QuickChangeMainView = null;
            }
        }
        public void CloseSapBomExportMainView()
        {
            if (_SapBomExportMainView != null)
            {
                _SapBomExportMainView.Close();
                _SapBomExportMainView = null;
            }
        }
        public void CloseSapBomExportAllLevelMainView()
        {
            if (_SapBomExportAllLevelMainView != null)
            {
                _SapBomExportAllLevelMainView.Close();
                _SapBomExportAllLevelMainView = null;
            }
        }
        public void CloseSapFertMissingPart()
        {
            if (_SapFertMissingPart != null)
            {
                _SapFertMissingPart.Close();
                _SapFertMissingPart = null;
            }
        }
        public void CloseSapFertBomMainView()
        {
            if (_SapFertBomMainView != null)
            {
                _SapFertBomMainView.Close();
                _SapFertBomMainView = null;
            }
        }
        public void CloseWebtermRequestMainView()
        {
            if (_WebtermRequestMainView != null)
            {
                _WebtermRequestMainView.Close();
                _WebtermRequestMainView = null;
            }
        }
    }
}
