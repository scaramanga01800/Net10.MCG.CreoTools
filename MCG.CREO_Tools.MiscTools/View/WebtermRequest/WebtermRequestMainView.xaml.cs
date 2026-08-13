using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.ViewModel.WebtermRequest;
using System.IO;
using System.Windows;

namespace MCG.CREO_Tools.MiscTools.View.WebtermRequest
{
    public partial class WebtermRequestMainView : RibbonWindow
    {
        public WebtermRequestMainView(WebtermRequestViewModel currentViewModel, ISharedAppContext sharedAppContext)
        {
            try
            {
                TraceLog.AddTraceLog("Create WebtermRequestMainView");
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                TraceLog.AddTraceLog($"CadDocRenameMainView: Local App Directory {MainAppFolder}");

                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);
                DataContext = currentViewModel;

                ((WebtermRequestViewModel)DataContext).CallCloseEvent += (sender, e) => { Close(); };

                InitializeComponent();
                McgWpfTools.UpdateMergeDictionaries(sharedAppContext.CurrentLanguage?.Language?.CultureInfo?.Substring(0, 2));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
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

    }
}
