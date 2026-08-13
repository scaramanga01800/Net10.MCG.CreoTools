using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.ViewModel.BomComparison;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.WindchillRequestTool.Exceptions;
using System.Windows;

namespace MCG.CREO_Tools.MiscTools.View.BomComparison
{
    public partial class BomComparisonTabMainView : RibbonTabItem
    {
        private BomComparisonViewModel CurrenBomComparisonViewModel { get; set; }
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

        public BomComparisonTabMainView()
        {
            try
            {
                TraceLog.AddTraceLog("Create BomComparisonTabMainView");

                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);

                DataContextChanged += BomComparisonTabMainView_DataContextChanged;
                InitializeComponent();
            }
            catch (MiscToolsException ex)
            {
                WindchillRequestException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void BomComparisonTabMainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(BomComparisonViewModel))
                {
                    TraceLog.AddTraceLog($"BomComparisonTabMainView: Init DataContext");
                    CurrenBomComparisonViewModel = DataContext as BomComparisonViewModel;
                    CurrenBomComparisonViewModel.ActionDoneEvent += RaiseActionDoneEvent;
                    CurrenBomComparisonViewModel.ActionInProgressEvent += RaiseActionInProgressEvent;
                    IsAlreadyInit = true;
                }
            }
            catch (Exception ex)
            {
                WindchillRequestException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
