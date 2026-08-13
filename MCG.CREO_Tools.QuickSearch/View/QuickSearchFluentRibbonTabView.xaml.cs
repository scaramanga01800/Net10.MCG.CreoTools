using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.QuickSearch.Configuration;
using MCG.CREO_Tools.QuickSearch.Exceptions;
using MCG.CREO_Tools.QuickSearch.ViewModel;
using System.Diagnostics;
using System.Windows;

namespace MCG.CREO_Tools.QuickSearch.View
{
    public partial class QuickSearchFluentRibbonTabView : RibbonTabItem
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

        public event EventHandler DataContextUpdatedEvent;
        public void RaiseDataContextUpdatedEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                DataContextUpdatedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Internal properties
        private bool IsAlreadyInit { get; set; } = false;
        private QuickSearchViewModel CurrentQuickSearchViewModel { get; set; }
        #endregion

        public QuickSearchFluentRibbonTabView()
        {
            try
            {
                var sw = Stopwatch.StartNew();

                TraceLog.AddTraceLog("Create QuickSearchFluentRibbonTabView");
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                
                TraceLog.AddTraceLog($"Before QuickSearchFluentRibbonTabView MergeDictionary : {sw.ElapsedMilliseconds} ms");

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{QuickSearchConstants.MainDictionary}", UriKind.Absolute);
                
                TraceLog.AddTraceLog($"After QuickSearchFluentRibbonTabView MergeDictionary : {sw.ElapsedMilliseconds} ms");
                DataContextChanged += QuickSearchFluentRibbonTabView_DataContextChanged;

                InitializeComponent();
                TraceLog.AddTraceLog($"After QuickSearchFluentRibbonTabView InitializeComponent : {sw.ElapsedMilliseconds} ms");    
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void QuickSearchFluentRibbonTabView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(QuickSearchViewModel))
                {
                    CurrentQuickSearchViewModel = DataContext as QuickSearchViewModel;
                    IsAlreadyInit = true;
                    CurrentQuickSearchViewModel.ActionDoneEvent += RaiseActionDoneEvent;
                    CurrentQuickSearchViewModel.ActionInProgressEvent += RaiseActionInProgressEvent;
                    CurrentQuickSearchViewModel.CurrentQuickSearchDataContext.ShortCutChangedEvent += UpdateShortCut;
                    UpdateShortCut();
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateShortCut(object sender = null, EventArgs e = null)
        {
            try
            {
                SbShortcuts.Children.Clear();
                QuickSearchShortCut CurrentQuickSearchShortCut;
                foreach (var shortcut in CurrentQuickSearchViewModel.CurrentQuickSearchDataContext.ListShortCut)
                {
                    CurrentQuickSearchShortCut = new QuickSearchShortCut()
                    {
                        CurrentQuickSearchShortCutViewModel = shortcut
                    };
                    CurrentQuickSearchShortCut.DataContext = CurrentQuickSearchShortCut.CurrentQuickSearchShortCutViewModel;
                    SbShortcuts.Children.Add(CurrentQuickSearchShortCut);
                }

            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
