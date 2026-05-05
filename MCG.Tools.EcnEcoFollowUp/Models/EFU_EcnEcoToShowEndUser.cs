using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Interfaces.Models;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MCG.Tools.EcnEcoFollowUp.Models
{
    public class EFU_EcnEcoToShowEndUser : ObservableObject, IEFU_EcnEcoToShowEndUser
    {
        #region [REGION] Properties from Interface
        private EFU_Status _Status;
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

        private EFU_EcnEcoFollowUp _EcnEcoFollowUp;
        public EFU_EcnEcoFollowUp EcnEcoFollowUp
        {
            get { return this._EcnEcoFollowUp; }
            set
            {
                if (this._EcnEcoFollowUp != value)
                {
                    this._EcnEcoFollowUp = value;
                    OnPropertyChanged();
                    if (value != null)
                        UpdateEcnStatus();
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

        public ObservableCollection<MenuItem> MenuAttachments { get; set; } = new ObservableCollection<MenuItem>();
        #endregion

        #region [REGION] Events
        public event EventHandler IsSelectedEvent;

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
        #endregion

        #region [REGION] Properties not from interface
        public bool AlreadySearchAttachments { get; set; } = false;
        #endregion

        private void UpdateEcnStatus()
        {
            try
            {
                // Ecn In progress
                if (EcnEcoFollowUp.Ecn_State != "Resolved" && EcnEcoFollowUp.Ecn_State != "Canceled")
                {
                    if (EcnEcoFollowUp.Ecn_Created_On != null)
                    {
                        TimeSpan TreatmentTime = (DateTime.Now - EcnEcoFollowUp.Ecn_Created_On.Value);
                        if ((TreatmentTime.Days > 90))
                            Status = EFU_Status.ECNINPROGRESS90;
                        else
                            Status = EFU_Status.ECNINPROGRESS;
                    }
                    else
                        Status = EFU_Status.ECNINPROGRESS;
                }
                // Eco to be created or in status 99,01,02,03
                else if (EcnEcoFollowUp.Ecn_State == "Resolved")
                {
                    TimeSpan TreatmentTime = default(TimeSpan);
                    if (EcnEcoFollowUp.Eco_Wf_Started_On != null)
                        TreatmentTime = (DateTime.Now - EcnEcoFollowUp.Eco_Wf_Started_On.Value);
                    if ((EcnEcoFollowUp.Eco_Status == "Not Created"))
                        Status = EFU_Status.ECOTOBECREATED;
                    else if ((EcnEcoFollowUp.Eco_Status == "99"))
                        Status = EFU_Status.ECOSTATUS99;
                    else if ((EcnEcoFollowUp.Eco_Status == "01"))
                        if (TreatmentTime != default(TimeSpan) && TreatmentTime.Days > 180)
                            Status = EFU_Status.ECOSTATUS01_6MONTHS;
                        else
                            Status = EFU_Status.ECOSTATUS01;
                    else if ((EcnEcoFollowUp.Eco_Status == "02"))
                        Status = EFU_Status.ECOSTATUS02;
                    else if ((EcnEcoFollowUp.Eco_Status == "03"))
                        Status = EFU_Status.ECOSTATUS03;
                }
                else
                    Status = EFU_Status.ECNCANCELED;

                //  ECN Under Review
                if (EcnEcoFollowUp.Ecn_State == "Implementation" && EcnEcoFollowUp.Designer_Start_App_Date != null && EcnEcoFollowUp.Designer_Start_App_Date.ToString().Trim() != "")
                    Status = EFU_Status.ECNUNDERREVIEW;
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }
    }
}
