using MCG.CommonLib.DataBaseAccess.Models.SapHupDbResult;
using MCG.CREO_Tools.MiscTools.Interfaces;
using MCG.CREO_Tools.MiscTools.View.CraneSearch;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace MCG.CREO_Tools.MiscTools.Services
{
    internal class MiscToolsWindchillService : IMiscToolsWindchillService
    {
        private readonly IServiceProvider _serviceProvider;
        private Window _CraneSearchMainView;

        public MiscToolsWindchillService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowCraneSearchMainView(List<string> listObject, bool isAlreadyCreated)
        {
            if (isAlreadyCreated)
            {
                if (_CraneSearchMainView != null)
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
                if (_CraneSearchMainView != null)
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

        public void closeCraneSearchMainView()
        {
            if (_CraneSearchMainView != null)
            {
                _CraneSearchMainView.Close();
                _CraneSearchMainView = null;
            }
        }
    }
}
