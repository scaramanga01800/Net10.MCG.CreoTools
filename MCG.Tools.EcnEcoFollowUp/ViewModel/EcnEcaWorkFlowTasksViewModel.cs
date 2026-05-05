using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.View;
using System.Collections.ObjectModel;

namespace MCG.Tools.EcnEcoFollowUp.ViewModel
{
    public class EcnEcaWorkFlowTasksViewModel : ObservableObject, IEcnEcaWorkFlowTasksViewModel
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

        public ObservableCollection<EFU_EcnEcoWorkflowItem> ListEcnWfTask { get; set; } = new ObservableCollection<EFU_EcnEcoWorkflowItem>();
        public ObservableCollection<EFU_EcnEcoWorkflowItem> ListEcaWfTask { get; set; } = new ObservableCollection<EFU_EcnEcoWorkflowItem>();
        #endregion

        #region [REGION] Properties not from interface
        private List<EFU_EcnEcoWorkflowItem> ListAllTask;
        #endregion

        #region [REGION] Init
        public void SetEcnEcaWorkFlowTasksViewModelProperties(EFU_EcnEcoToShowEndUser EcnEco, List<EFU_EcnEcoWorkflowItem> ListTaks)
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
                var TempListEcnWf = ListAllTask.Where((item) => item.EcaNumber == null || item.EcaNumber.Trim() == "").OrderBy((item) => item.WfTaskCompletedOn).ToList();
                var TempListEcaWf = ListAllTask.Where((item) => item.EcaNumber != null && item.EcaNumber.Trim() != "").OrderBy((item) => item.WfTaskCompletedOn).ToList();

                foreach (var item in TempListEcnWf)
                    ListEcnWfTask.Add(item);
                foreach (var item in TempListEcaWf)
                    ListEcaWfTask.Add(item);
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
