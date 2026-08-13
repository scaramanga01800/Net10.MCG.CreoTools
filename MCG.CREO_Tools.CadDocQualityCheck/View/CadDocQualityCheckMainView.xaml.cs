using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.CadDocQualityCheck.Configuration;
using MCG.CREO_Tools.CadDocQualityCheck.Exceptions;
using MCG.CREO_Tools.CadDocQualityCheck.ViewModel;

namespace MCG.CREO_Tools.CadDocQualityCheck.View
{
    public partial class CadDocQualityCheckMainView : RibbonTabItem
    {
        private bool IsAlreadyInit { get; set; } = false;

        #region [REGION] Events Action
        public event EventHandler ActionInProgressEvent;
        public void RaiseActionInProgressEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                ActionInProgressEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler ActionDoneEvent;
        public void RaiseActionDoneEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                ActionDoneEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        public CadDocQualityCheckMainView()
        {
            try
            {
                TraceLog.AddTraceLog("Create CadDocQualityCheckTabContentView");

                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{CadDocQualityCheckConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();

                DataContextChanged += CadDocQualityCheckMainView_DataContextChanged;
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void CadDocQualityCheckMainView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(CadDocQualityCheckViewModel))
                {
                    CadDocQualityCheckViewModel CurrentDataContext = DataContext as CadDocQualityCheckViewModel;
                    CurrentDataContext.ActionDoneEvent += RaiseActionDoneEvent;
                    CurrentDataContext.ActionInProgressEvent += RaiseActionInProgressEvent;
                    IsAlreadyInit = true;
                }
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
