using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.EcnEcoFollowUp.Interfaces.Models;

namespace MCG.Tools.EcnEcoFollowUp.Models
{

    public class EFU_DashboardEcnEco : ObservableObject, IEFU_DashboardEcnEco
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
                    UpdateEcoTimeResolution();
                    UpdateApprovalEcnStep();
                }
            }
        }

        private string _Department = string.Empty;
        public string Department
        {
            get { return this._Department; }
            set
            {
                if (this._Department != value)
                {
                    this._Department = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEvent();
                }
            }
        }

        private string _Comment = string.Empty;
        public string Comment
        {
            get { return this._Comment; }
            set
            {
                if (this._Comment != value)
                {
                    this._Comment = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEvent();
                }
            }
        }

        private string _ApprovalEcnStep = string.Empty;
        public string ApprovalEcnStep
        {
            get { return this._ApprovalEcnStep; }
            set
            {
                if (this._ApprovalEcnStep != value)
                {
                    this._ApprovalEcnStep = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Information = string.Empty;
        public string Information
        {
            get { return this._Information; }
            set
            {
                if (this._Information != value)
                {
                    this._Information = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEvent();
                }
            }
        }

        private string _SapOrder = string.Empty;
        public string SapOrder
        {
            get { return this._SapOrder; }
            set
            {
                if (this._SapOrder != value)
                {
                    this._SapOrder = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEvent();
                }
            }
        }

        private int? _EcoTimeResolution;
        public int? EcoTimeResolution
        {
            get { return this._EcoTimeResolution; }
            set
            {
                if (this._EcoTimeResolution != value)
                {
                    this._EcoTimeResolution = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsSelected = false;
        public bool IsSelected
        {
            get { return this._IsSelected; }
            set
            {
                if (this._IsSelected != value)
                {
                    this._IsSelected = value;
                    OnPropertyChanged();
                    RaiseIsSelectedEvent();
                }
            }
        }

        private string _Priority = string.Empty;
        public string Priority
        {
            get { return this._Priority; }
            set
            {
                if (this._Priority != value)
                {
                    this._Priority = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEvent();
                }
            }
        }
        #endregion

        #region [REGION] Events
        public event EventHandler IsSelectedEvent;
        public event EventHandler IsUpdateEvent;

        public void RaiseIsSelectedEvent()
        {
            try
            {
                IsSelectedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        public void RaiseIsUpdateEvent()
        {
            try
            {
                IsUpdateEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Properties not from Interface
        public EcnecodashboardDetail EcnEcoDasboardDetail { get; set; }
        #endregion

        public void UpdateEcoTimeResolution()
        {
            try
            {
                if (EcnEcoToShowEndUser != null && EcnEcoToShowEndUser.EcnEcoFollowUp != null && EcnEcoToShowEndUser.EcnEcoFollowUp.Eco_Created_On != null && EcnEcoToShowEndUser.EcnEcoFollowUp.Eco_Closed_On != null)
                    EcoTimeResolution = (EcnEcoToShowEndUser.EcnEcoFollowUp.Eco_Closed_On - EcnEcoToShowEndUser.EcnEcoFollowUp.Eco_Created_On).Value.Days;
            }
            catch (Exception)
            {
                EcoTimeResolution = null;
            }
        }

        public void UpdateApprovalEcnStep()
        {
            try
            {
                if (EcnEcoToShowEndUser != null && EcnEcoToShowEndUser.EcnEcoFollowUp != null)
                    if (EcnEcoToShowEndUser.EcnEcoFollowUp.Designer_Start_App_Date == null) ApprovalEcnStep = "Not started";
                    else if (EcnEcoToShowEndUser.EcnEcoFollowUp.First_Approval_Date == null) ApprovalEcnStep = "Awaiting First Approval";
                    else if (EcnEcoToShowEndUser.EcnEcoFollowUp.Qual_Check_Approval_Date == null) ApprovalEcnStep = "Awaiting Quality Check Approval";
                    else if (EcnEcoToShowEndUser.EcnEcoFollowUp.CAIII_Approval_Date == null) ApprovalEcnStep = "Awaiting Last Approval";
                    else if (EcnEcoToShowEndUser.EcnEcoFollowUp.CAIII_Approval_Date != null) ApprovalEcnStep = "Approved";
                    else ApprovalEcnStep = "Unknown";
            }
            catch (Exception)
            {
                EcoTimeResolution = null;
            }
        }

        public override string ToString()
        {
            if (EcnEcoToShowEndUser != null && EcnEcoToShowEndUser.EcnEcoFollowUp != null)
                return EcnEcoToShowEndUser.EcnEcoFollowUp.Ecn_Number;
            else
                return base.ToString();
        }
    }
}
