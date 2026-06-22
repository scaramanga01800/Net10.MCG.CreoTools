using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.ShearedTube.Configuration;
using MCG.CREO_Tools.ShearedTube.Exceptions;
using MCG.CREO_Tools.ShearedTube.ViewModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.ShearedTube.View
{
    public partial class ShearedTubeFluentTabMainView : RibbonTabItem
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

        public ShearedTubeFluentTabMainView()
        {
            try
            {
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{ShearedTubeConstants.MainDictionary}", UriKind.Absolute);

                DataContextChanged += ShearedTubeFluentTabMainView_DataContextChanged;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                ShearedTubeException.SendMessageBox(this.GetType().Name, ex);

            }
        }

        private void ShearedTubeFluentTabMainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(ShearedTubeViewModel))
                {
                    ShearedTubeViewModel CurrentDataContext = DataContext as ShearedTubeViewModel;
                    CurrentDataContext.ActionDoneEvent += RaiseActionDoneEvent;
                    CurrentDataContext.ActionInProgressEvent += RaiseActionInProgressEvent;
                    IsAlreadyInit = true;
                }
            }
            catch (Exception ex)
            {
                ShearedTubeException.SendMessageBox(this.GetType().Name, ex);

            }
        }

        #region [REGION] Misc
        private void Double_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex StartPointRegex = new Regex(@"^\.");
            if (StartPointRegex.IsMatch(((TextBox)sender).Text))
                ((TextBox)sender).Text = $"0{((TextBox)sender).Text}";

            Regex WholeTextRegex = new Regex(@"^$|^[0-9]+\.?$|^[0-9]+\.?[0-9]+$");
            Regex SingleCharRegex = new Regex("[0-9.]");
            e.Handled = !(WholeTextRegex.IsMatch(((TextBox)sender).Text) && SingleCharRegex.IsMatch(e.Text) && WholeTextRegex.IsMatch($"{((TextBox)sender).Text}{e.Text}"));
        }

        private void Double_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text == null || ((TextBox)sender).Text.Trim() == "")
                ((TextBox)sender).Text = "0";
        }
        #endregion
    }
}
