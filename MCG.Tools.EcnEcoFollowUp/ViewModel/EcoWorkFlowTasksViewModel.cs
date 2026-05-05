using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.View;
using System.Collections.ObjectModel;

namespace MCG.Tools.EcnEcoFollowUp.ViewModel
{
    public class EcoWorkFlowTasksViewModel: ObservableObject, IEcoWorkFlowTasksViewModel
    {
        #region [REGION] Properties from Interface
        private EFU_EcnEcoToShowEndUser _EcnEcoToShowEndUser;
        public EFU_EcnEcoToShowEndUser EcnEcoToShowEndUser
        {
            get { return this._EcnEcoToShowEndUser; }
            set
            {
                if (this._EcnEcoToShowEndUser != value)
                {
                    this._EcnEcoToShowEndUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<EFU_SapHupOracle_DmEcoTasks> EcoWfTaskListMainPlant { get; set; } = new ObservableCollection<EFU_SapHupOracle_DmEcoTasks>();
        public ObservableCollection<EFU_SapHupOracle_DmEcoTasks> EcoWfTaskListOtherPlants { get; set; } = new ObservableCollection<EFU_SapHupOracle_DmEcoTasks>();

        #endregion

        #region [REGION] Properties not from interface
        private List<EFU_SapHupOracle_DmEcoTasks> ListAllTask;
        #endregion

        #region [REGION] Init
        public void SetEcoWorkFlowTasksViewModelProperties(EFU_EcnEcoToShowEndUser EcnEco, List<EFU_SapHupOracle_DmEcoTasks> ListTaks)
        {
            try
            {
                EcnEcoToShowEndUser = EcnEco;
                ListAllTask = ListTaks;
                UpdateListWfTask();
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateListWfTask()
        {
            try
            {
                if (ListAllTask != null)
                    foreach (var item in ListAllTask)
                        if (item.ECO_COORD == item.CALCULATED_PLANT)
                            EcoWfTaskListMainPlant.Add(item);
                        else
                            EcoWfTaskListOtherPlants.Add(item);
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
