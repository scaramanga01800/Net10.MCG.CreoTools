using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.Tools.NumberingTool.Configuration;
using MCG.Tools.NumberingTool.Exceptions;
using MCG.Tools.NumberingTool.ViewModel;
using System.Windows;

namespace MCG.Tools.NumberingTool.View
{
    public partial class NumberingToolFluentMainView : RibbonWindow
    {
        private NumberingToolViewModel CurrentDataContext;

        public event EventHandler CreateNumberEvent;
        public void RaiseCreateNumberEvent()
        {
            try
            {
                CreateNumberEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UseNumberEvent;
        public void RaiseUseNumberEvent(string CurrentNumber)
        {
            try
            {
                UseNumberEvent?.Invoke(CurrentNumber, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public NumberingToolFluentMainView(NumberingToolViewModel currentVm, ISharedAppContext sharedAppContext)
        {
            TraceLog.AddTraceLog($"Enter NumberingToolFluentMainView App");
            string MainAppFolder = Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
            if (MainAppFolder == null || MainAppFolder == "")
                MainAppFolder = CommonLibConstants.MainAppFolder;

            McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{NumberingToolConstants.MainDictionary}", UriKind.Absolute);
            CurrentDataContext = currentVm;
            InitializeComponent();
            McgWpfTools.UpdateMergeDictionaries(sharedAppContext.CurrentLanguage?.Language?.CultureInfo?.Substring(0, 2));
        }

        public void SetNumberingToolFluentMainViewProperties(bool pNoRangeAuthorized = false)
        {
            CurrentDataContext.SetNumberingToolViewModelProperties(pNoRangeAuthorized);
            CurrentDataContext.CreateNumberEvent += CreateNumber_Done;
            CurrentDataContext.UseNumberEvent += UseNumber_Done;
            DataContext = CurrentDataContext;
        }

        private void UseNumber_Done(object sender, EventArgs e)
        {
            try
            {
                if (sender != null && sender is string)
                    RaiseUseNumberEvent((string)sender);
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void CreateNumber_Done(object sender, EventArgs e)
        {
            try
            {
                RaiseCreateNumberEvent();
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void Expander_ExpandedCollapsed(object sender, RoutedEventArgs e)
        {
            this.SizeToContent = SizeToContent.Height;
        }
    }
}
