using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.ViewModel.CraneSearch;
using System.IO;
using System.Windows;

namespace MCG.CREO_Tools.MiscTools.View.CraneSearch
{
    public partial class CraneSearchMainView : RibbonWindow
    {
        public CraneSearchViewModel CurrentDataContext { get; set; }

        public CraneSearchMainView(CraneSearchViewModel currentVm)
        {
            try
            {
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                TraceLog.AddTraceLog($"CraneSearchMainView: Local App Directory {MainAppFolder}");

                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);

                CurrentDataContext = currentVm;
                DataContext = CurrentDataContext;
                CurrentDataContext.CurrentDataContext.IsStandAlone = true;

                CurrentDataContext.CallCloseEvent += (o, e) => { this.Close(); };

                InitializeComponent();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void SetCraneSearchViewModelProperties(List<string> listObject)
        {
            CurrentDataContext.PartList = listObject;
            CurrentDataContext.CurrentDataContext.IsStandAlone = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (TabItemPart.Visibility == Visibility.Collapsed)
            {
                TabControlCrane.SelectedIndex = 1;
                TabControlCrane.Focus();
            }
        }
    }
}
