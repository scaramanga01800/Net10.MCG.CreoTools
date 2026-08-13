using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.VisualizationLib.Configuration;
using MCG.Tools.VisualizationLib.Exceptions;
using MCG.Tools.VisualizationLib.ViewModel;
using System.IO;
using System.Windows;

namespace MCG.Tools.VisualizationLib.View
{
    public partial class DownloadVisualizationFileMainView : RibbonTabItem
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

        public DownloadVisualizationFileMainView()
        {
            try
            {
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                TraceLog.AddTraceLog($"DownloadVisualizationFileMainView: Local App Directory {MainAppFolder}");

                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{VisualizationLibConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();
                DataContextChanged += DownloadVisualizationFileMainView_DataContextChanged;
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void DownloadVisualizationFileMainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(DownloadVisualizationFileViewModel))
                {
                    DownloadVisualizationFileViewModel CurrentDataContext = (DownloadVisualizationFileViewModel)DataContext;
                    CurrentDataContext.ActionDoneEvent += RaiseActionDoneEvent;
                    CurrentDataContext.ActionInProgressEvent += RaiseActionInProgressEvent;
                    IsAlreadyInit = true;
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
