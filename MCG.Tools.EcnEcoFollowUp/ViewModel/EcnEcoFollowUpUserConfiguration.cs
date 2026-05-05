
namespace MCG.Tools.EcnEcoFollowUp.ViewModel
{
    public class EcnEcoFollowUpUserConfiguration
    {
        private string _LatestSelectedDashboardId = string.Empty;
        public string LatestSelectedDashboardId
        {
            get { return _LatestSelectedDashboardId; }
            set
            {
                if (this._LatestSelectedDashboardId != value)
                {
                    this._LatestSelectedDashboardId = value;
                }
                RaiseUserConfigurationUpdateEvent();
            }
        }

        public event EventHandler UserConfigurationUpdateEvent;
        public void RaiseUserConfigurationUpdateEvent()
        {
            try
            {
                UserConfigurationUpdateEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
    }
}
