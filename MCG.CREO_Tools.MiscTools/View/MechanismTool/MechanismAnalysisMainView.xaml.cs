using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool;
using System.IO;
using System.Windows;

namespace MCG.CREO_Tools.MiscTools.View.MechanismTool
{
    public partial class MechanismAnalysisMainView : RibbonWindow
    {
        public MechanismAnalysisMainView(MechanismAnalysisViewModel currentVm, ISharedAppContext sharedAppContext)
        {
            try
            {
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                TraceLog.AddTraceLog($"MechanismAnalysisMainView: Local App Directory {MainAppFolder}");

                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);

                DataContext = currentVm;

                InitializeComponent();
                McgWpfTools.UpdateMergeDictionaries(sharedAppContext.CurrentLanguage?.Language?.CultureInfo?.Substring(0, 2));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void MainSP_Drop(object sender, DragEventArgs e)
        {
            try
            {
                ImageDragDrop.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void MainSP_DragEnter(object sender, DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Visible;
        }

        private void MainSP_DragLeave(object sender, DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Collapsed;
        }
    }
}
