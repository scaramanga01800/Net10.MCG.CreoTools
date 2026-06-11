using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.JpgExport.Configuration;
using MCG.CREO_Tools.JpgExport.Exceptions;
using MCG.CREO_Tools.JpgExport.ViewModel;

namespace MCG.CREO_Tools.JpgExport.View
{
    public partial class JpgExportFluentTabMainView : RibbonTabItem
    {
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

        private bool IsAlreadyInit { get; set; } = false;

        public JpgExportFluentTabMainView()
        {
            try
            {
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{JpgExportConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();

                DataContextChanged += JpgExportFluentTabContentView_DataContextChanged;
            }
            catch (Exception ex)
            {
                JpgExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void JpgExportFluentTabContentView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(JpgExportViewModel))
            {
                JpgExportViewModel CurrentDataContext = DataContext as JpgExportViewModel;
                CurrentDataContext.Update();
                CurrentDataContext.ActionDoneEvent += RaiseActionDoneEvent;
                CurrentDataContext.ActionInProgressEvent += RaiseActionInProgressEvent;
                IsAlreadyInit = true;
            }
        }
    }
}
