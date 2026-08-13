using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.EcnDataCheck.Configuration;
using MCG.Tools.EcnDataCheck.Exceptions;
using MCG.Tools.EcnDataCheck.ViewModel;
using System.Windows;

namespace MCG.Tools.EcnDataCheck.View
{
    public partial class EcnDataCheckRibbonTabView : RibbonTabItem
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

        public EcnDataCheckRibbonTabView()
        {
            try
            {
                TraceLog.AddTraceLog("Create EcnDataCheckRibbonTabView");
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{EcnDataCheckConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();
                DataContextChanged += EcnDataCheckRibbonTabView_DataContextChanged;
            }
            catch (Exception ex)
            {
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void EcnDataCheckRibbonTabView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(EcnDataCheckViewModel))
            {
                EcnDataCheckViewModel CurrentDataContext = DataContext as EcnDataCheckViewModel;
                CurrentDataContext.ActionDoneEvent += RaiseActionDoneEvent;
                CurrentDataContext.ActionInProgressEvent += RaiseActionInProgressEvent;
                IsAlreadyInit = true;
            }
        }
    }
}
