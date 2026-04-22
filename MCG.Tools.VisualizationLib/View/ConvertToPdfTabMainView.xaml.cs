using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.VisualizationLib.Configuration;
using MCG.Tools.VisualizationLib.Exceptions;
using MCG.Tools.VisualizationLib.ViewModel;
using System.Windows;


namespace MCG.Tools.VisualizationLib.View
{
    public partial class ConvertToPdfTabMainView : RibbonTabItem
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

        public ConvertToPdfTabMainView()
        {

            string MainAppFolder = Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);

            if (MainAppFolder == null || MainAppFolder == "")
                MainAppFolder = CommonLibConstants.MainAppFolder;
            McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{VisualizationLibConstants.MainDictionary}", UriKind.Absolute);



            InitializeComponent();
            DataContextChanged += ConvertToPdfTabMainView_DataContextChanged;   
        }

        private void ConvertToPdfTabMainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext!= null && DataContext.GetType() == typeof(ConvertToPdfViewModel))
                {
                    ConvertToPdfViewModel CurrentDataContext = (ConvertToPdfViewModel) DataContext;
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
