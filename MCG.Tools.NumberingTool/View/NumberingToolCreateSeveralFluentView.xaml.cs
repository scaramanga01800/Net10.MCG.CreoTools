using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.NumberingTool.Configuration;
using MCG.Tools.NumberingTool.Exceptions;
using MCG.Tools.NumberingTool.ViewModel;

namespace MCG.Tools.NumberingTool.View
{
    public partial class NumberingToolCreateSeveralFluentView : RibbonWindow
    {
        #region [REGION] Events
        public event EventHandler UseNumberEvent;
        public void RaiseUseNumberEvent(object sender)
        {
            try
            {
                UseNumberEvent?.Invoke(sender, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        public NumberingToolCreateSeveralFluentView()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter NumberingToolCreateSeveralFluentView App");
                string MainAppFolder = Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{NumberingToolConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void SetNumberingToolCreateSeveralFluentViewProperties(NumberingToolViewModel CurrentDataContext)
        {
            DataContext = CurrentDataContext;
            CurrentDataContext.UseNumberEvent += UseNumber;
        }

        private void UseNumber(object sender, EventArgs e)
        {
            try
            {
                RaiseUseNumberEvent(sender);
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
