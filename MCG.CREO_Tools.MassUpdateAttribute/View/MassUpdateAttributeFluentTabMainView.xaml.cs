using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MassUpdateAttribute.Configuration;
using MCG.CREO_Tools.MassUpdateAttribute.Exceptions;
using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;
using System.Windows;

namespace MCG.CREO_Tools.MassUpdateAttribute.View
{
    public partial class MassUpdateAttributeFluentTabMainView : RibbonTabItem
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

        public MassUpdateAttributeFluentTabMainView()
        {
            try
            {
                TraceLog.AddTraceLog("Create MassUpdateAttributeFluentTabMainView");
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MassUpdateAttributeConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();
                DataContextChanged += MassUpdateAttributeFluentTabMainView_DataContextChanged;  
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void MassUpdateAttributeFluentTabMainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(MassUpdateAttributeViewModel))
                {
                    MassUpdateAttributeViewModel CurrentDataContext = DataContext as MassUpdateAttributeViewModel;
                    CurrentDataContext.ActionDoneEvent += RaiseActionDoneEvent;
                    CurrentDataContext.ActionInProgressEvent += RaiseActionInProgressEvent;
                    IsAlreadyInit = true;
                }
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
