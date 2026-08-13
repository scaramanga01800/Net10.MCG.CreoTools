using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.CutLengthApp.Configuration;
using MCG.CREO_Tools.CutLengthApp.Exceptions;
using MCG.CREO_Tools.CutLengthApp.ViewModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.CutLengthApp.View
{
    public partial class CutLengthMainView : RibbonTabItem
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

        public CutLengthMainView()
        {
            try
            {
                var sw = Stopwatch.StartNew();

                TraceLog.AddTraceLog("Create CutLengthMainView");
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                TraceLog.AddTraceLog($"Before CutLengthMainView MergeDictionary : {sw.ElapsedMilliseconds} ms");

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{CutLengthAppConstants.MainDictionary}", UriKind.Absolute);

                TraceLog.AddTraceLog($"After CutLengthMainView MergeDictionary : {sw.ElapsedMilliseconds} ms");

                InitializeComponent();

                TraceLog.AddTraceLog($"After CutLengthMainView InitializeComponent : {sw.ElapsedMilliseconds} ms");

                DataContextChanged += CutLengthMainView_DataContextChanged;

                TraceLog.AddTraceLog($"CutLength ctor DataContext : {DataContext?.GetType().Name ?? "NULL"}");
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void CutLengthMainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                TraceLog.AddTraceLog($"CutLength DataContextChanged : {DataContext?.GetType().Name ?? "NULL"}");
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(CutLengthViewModel))
                {
                    CutLengthViewModel CurrentDataContext = DataContext as CutLengthViewModel;
                    CurrentDataContext.ActionDoneEvent += RaiseActionDoneEvent;
                    CurrentDataContext.ActionInProgressEvent += RaiseActionInProgressEvent;
                    IsAlreadyInit = true;
                }
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void TbDoubleInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

    }
}
