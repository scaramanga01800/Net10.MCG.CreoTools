using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.WindchillTools.ManageWTObject.Configuration;
using MCG.WindchillTools.ManageWTObject.Exceptions;
using MCG.WindchillTools.ManageWTObject.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MCG.WindchillTools.ManageWTObject.View
{
    /// <summary>
    /// Logique d'interaction pour CreateUpdateWtDocumentWtPartMainView.xaml
    /// </summary>
    public partial class CreateUpdateWtDocumentWtPartMainView : RibbonWindow
    {
        public CreateUpdateWtDocumentWtPartMainView(CreateUpdateWtDocumentWtPartViewModel currentVM)
        {
            string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
            TraceLog.AddTraceLog($"MechanismAnalysisMainView: Local App Directory {MainAppFolder}");

            if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                MainAppFolder = CommonLibConstants.MainAppFolder;

            McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{ManageWTObjectConstants.MainDictionary}", UriKind.Absolute);

            DataContext = currentVM;
            InitializeComponent();
        }

        #region [REGION] Methods for Drag and Drop
        private void MainSP_Drop(object sender, DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Collapsed;
        }

        private void MainSP_DragEnter(object sender, DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Visible;
        }

        private void MainSP_DragLeave(object sender, DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Collapsed;
        }
        #endregion

        private bool JustChecked;
        private void RB_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton s = (System.Windows.Controls.RadioButton)sender;
            // Action on Check...
            JustChecked = true;
        }
        private void RB_Clicked(object sender, RoutedEventArgs e)
        {
            if (JustChecked)
            {
                JustChecked = false;
                e.Handled = true;
                return;
            }
            System.Windows.Controls.RadioButton s = (System.Windows.Controls.RadioButton)sender;
            if (s.IsChecked.Value)
                s.IsChecked = false;
        }

        private void ComboBoxBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ((CreateUpdateWtDocumentWtPartViewModel)DataContext).CurrentDataContext.RaiseUpdateBrandEvent();
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ComboBoxGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ((CreateUpdateWtDocumentWtPartViewModel)DataContext).CurrentDataContext.RaiseUpdateGroupEvent();
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
