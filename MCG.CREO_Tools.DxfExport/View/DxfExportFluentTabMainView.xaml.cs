using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.DxfExport.Configuration;
using MCG.CREO_Tools.DxfExport.Exceptions;
using MCG.CREO_Tools.DxfExport.ViewModel;

namespace MCG.CREO_Tools.DxfExport.View
{
    public partial class DxfExportFluentTabMainView : RibbonTabItem
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

        public DxfExportFluentTabMainView()
        {
            try
            {
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{DxfExportConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();
                DataContextChanged += DxfExportFluentTabMainView_DataContextChanged; ;
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void DxfExportFluentTabMainView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(DxfExportViewModel))
            {
                DxfExportViewModel CurrentDataContext = DataContext as DxfExportViewModel;
                CurrentDataContext.ActionDoneEvent += RaiseActionDoneEvent;
                CurrentDataContext.ActionInProgressEvent += RaiseActionInProgressEvent;
                IsAlreadyInit = true;
            }
        }
    }
}
