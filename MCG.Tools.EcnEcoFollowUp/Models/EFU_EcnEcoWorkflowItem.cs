using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.EcnEcoFollowUp.Interfaces.Models;
using MCG.WindchillRequestTool.Model.Windchill;

namespace MCG.Tools.EcnEcoFollowUp.Models
{
    public class EFU_EcnEcoWorkflowItem: ObservableObject, IEFU_EcnEcoWorkflowItem
    {
        #region [REGION] Properties from Interface
        private EFU_Status _Status = EFU_Status.UNKNOWN;
        public EFU_Status Status
        {
            get { return this._Status; }
            set
            {
                if (this._Status != value)
                {
                    this._Status = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _WfTaskName = string.Empty;
        public string WfTaskName
        {
            get { return this._WfTaskName; }
            set
            {
                if (this._WfTaskName != value)
                {
                    this._WfTaskName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _WfTaskOwner = string.Empty;
        public string WfTaskOwner
        {
            get { return this._WfTaskOwner; }
            set
            {
                if (this._WfTaskOwner != value)
                {
                    this._WfTaskOwner = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Vote = "Unknown";
        public string Vote
        {
            get { return this._Vote; }
            set
            {
                if (this._Vote != value)
                {
                    this._Vote = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _WfTaskCreatedOn;
        public DateTime? WfTaskCreatedOn
        {
            get { return this._WfTaskCreatedOn; }
            set
            {
                if (this._WfTaskCreatedOn != value)
                {
                    this._WfTaskCreatedOn = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime? _WfTaskCompletedOn;
        public DateTime? WfTaskCompletedOn
        {
            get { return this._WfTaskCompletedOn; }
            set
            {
                if (this._WfTaskCompletedOn != value)
                {
                    this._WfTaskCompletedOn = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _EcaNumber = string.Empty;
        public string EcaNumber
        {
            get { return this._EcaNumber; }
            set
            {
                if (this._EcaNumber != value)
                {
                    this._EcaNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _EcaSate = string.Empty;
        public string EcaSate
        {
            get { return this._EcaSate; }
            set
            {
                if (this._EcaSate != value)
                {
                    this._EcaSate = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region [REGION] Properties not from interface
        public string EcnName { get;  set; } = string.Empty;
        public string EcnNumber { get;  set; } = string.Empty;
        public string EcnState { get;  set; } = string.Empty;
        public string WfTaskCompletedBy { get;  set; } = string.Empty;
        public string WfTaskEvents { get;  set; } = string.Empty;
        public DateTime? WfTaskLastModified { get;  set; }
        public string WfTaskRole { get;  set; } = string.Empty;
        public string WfTaskStatus { get;  set; } = string.Empty;
        #endregion

        public static EFU_EcnEcoWorkflowItem GetEFU_EcnEcoWorkflowItem(WindchillObjectWorkflowTask item)
        {
            try
            {
                EFU_EcnEcoWorkflowItem currentWfTask = null/* TODO Change to default(_) if this is not a reference type */;
                if ((item != null))
                {
                    currentWfTask = new EFU_EcnEcoWorkflowItem() { EcaNumber = item.EcaNumber,
                        EcaSate = item.EcaState,
                        EcnName = item.EcnName,
                        EcnNumber = item.EcnNumber,
                        EcnState = item.EcnState,
                        WfTaskCompletedBy = item.CompletedBy,
                        WfTaskCreatedOn = item.CreatedOn,
                        WfTaskEvents = item.Events,
                        WfTaskLastModified = item.LastModified,
                        WfTaskName = item.Name,
                        WfTaskOwner = item.Owner,
                        WfTaskRole = item.Role,
                        WfTaskStatus = item.Status };

                    if (currentWfTask.WfTaskStatus == "Potential")
                    {
                        currentWfTask.Status = EFU_Status.WFTASKINPROGRESS;
                        currentWfTask.WfTaskCompletedOn = null;
                        currentWfTask.Vote = "In progress";
                    }
                    else if (currentWfTask.WfTaskStatus == "Completed")
                    {
                        if (currentWfTask.WfTaskEvents.IndexOf("Rework") >= 0)
                        {
                            currentWfTask.Status = EFU_Status.WFTASKREWORKED;
                            currentWfTask.WfTaskCompletedOn = currentWfTask.WfTaskLastModified;
                            currentWfTask.Vote = "Rework";
                        }
                        else
                        {
                            currentWfTask.Status = EFU_Status.WFTASKCOMPLETED;
                            currentWfTask.WfTaskCompletedOn = currentWfTask.WfTaskLastModified;
                            currentWfTask.Vote = "Approved";
                        }
                    }
                }

                return currentWfTask;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
