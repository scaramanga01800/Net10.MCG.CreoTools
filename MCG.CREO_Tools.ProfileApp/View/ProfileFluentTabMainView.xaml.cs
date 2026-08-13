using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.ProfileApp.Configuration;
using MCG.CREO_Tools.ProfileApp.Exceptions;
using MCG.CREO_Tools.ProfileApp.ViewModel;
using System.Diagnostics;

namespace MCG.CREO_Tools.ProfileApp.View
{
    public partial class ProfileFluentTabMainView : RibbonTabItem
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

        public ProfileFluentTabMainView()
        {
            try
            {
                var sw = Stopwatch.StartNew();

                TraceLog.AddTraceLog("Create ProfileFluentTabMainView");
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                
                TraceLog.AddTraceLog($"Before ProfileFluentTabMainView MergeDictionary : {sw.ElapsedMilliseconds} ms");
                
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{ProfileAppConstants.MainDictionary}", UriKind.Absolute);
                
                TraceLog.AddTraceLog($"After ProfileFluentTabMainView MergeDictionary : {sw.ElapsedMilliseconds} ms");
                
                InitializeComponent();

                TraceLog.AddTraceLog($"After ProfileFluentTabMainView InitializeComponent : {sw.ElapsedMilliseconds} ms");

                DataContextChanged += ProfileFluentTabMainView_DataContextChanged;
            }
            catch (Exception ex)
            {
                ProfileException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ProfileFluentTabMainView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(ProfileViewModel))
                {
                    ProfileViewModel CurrentDataContext = DataContext as ProfileViewModel;
                    CurrentDataContext.ActionDoneEvent += RaiseActionDoneEvent;
                    CurrentDataContext.ActionInProgressEvent += RaiseActionInProgressEvent;
                    IsAlreadyInit = true;
                }
            }
            catch (Exception ex)
            {
                ProfileException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
