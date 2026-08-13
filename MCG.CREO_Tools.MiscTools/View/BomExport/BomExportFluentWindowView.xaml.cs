using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.ViewModel.BomExport;
using MCG.WindchillRequestTool.Exceptions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MCG.CREO_Tools.MiscTools.View.BomExport
{
    public partial class BomExportFluentWindowView : RibbonWindow
    {
        private BomExportWindowViewModel CurrentBomExportWindowViewModel;

        public BomExportFluentWindowView(BomExportWindowViewModel currentViewModel,
                                         ISharedAppContext sharedAppContext)
        {
            try
            {
                TraceLog.AddTraceLog("Create BomExportFluentWindowView");

                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();
                CurrentBomExportWindowViewModel = currentViewModel;
                DataContext = currentViewModel;
                CurrentBomExportWindowViewModel.ParentWindow = this;
                CurrentBomExportWindowViewModel.SubcribeCloseEvent();
                CurrentBomExportWindowViewModel.ClosingEvent += (obj, e) => { this.Close(); };
             
                McgWpfTools.UpdateMergeDictionaries(sharedAppContext.CurrentLanguage?.Language?.CultureInfo?.Substring(0,2));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void Separator_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex SingleCharRegex = new Regex(@"^.$");
            e.Handled = SingleCharRegex.IsMatch(((System.Windows.Controls.TextBox)sender).Text);
        }

        private void TreeView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ScrollHori.ScrollToLeftEnd();
        }

        #region [REGION] Drag and Drop Methods
        private Point startPoint { get; set; } = new Point();
        private int startIndex { get; set; } = -1;

        private void lstview_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Get current mouse position
                startPoint = e.GetPosition(null);
            }
            catch (Exception ex)
            {
                WindchillRequestException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void lstview_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (!e.Data.GetDataPresent("BomExportParameter") || sender != e.Source)
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                WindchillRequestException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void lstview_Drop(object sender, DragEventArgs e)
        {
            try
            {
                int index = -1;

                if (e.Data.GetDataPresent("BomExportParameter") && sender == e.Source)
                {
                    // Get the drop ListViewItem destination
                    ListView listView = sender as ListView;
                    ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                    if (listViewItem == null)
                    {
                        // Abort
                        e.Effects = DragDropEffects.None;
                        return;
                    }

                    // Find the data behind the ListViewItem
                    BomExportParameter item = (BomExportParameter)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

                    // Move item into observable collection 
                    // (this will be automatically reflected to lstView.ItemsSource)
                    e.Effects = DragDropEffects.Move;
                    index = CurrentBomExportWindowViewModel.CurrentBomExportWindowDataContext.ListAllParameters.IndexOf(item);
                    if (startIndex >= 0 && index >= 0)
                    {
                        CurrentBomExportWindowViewModel.CurrentBomExportWindowDataContext.ListAllParameters.Move(startIndex, index);
                    }
                    startIndex = -1;
                }
            }
            catch (Exception ex)
            {
                WindchillRequestException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void lstview_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;

                if (e.LeftButton == MouseButtonState.Pressed &&
                    (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                           Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    // Get the dragged ListViewItem
                    ListView listView = sender as ListView;
                    ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                    if (listViewItem == null) return;           // Abort
                                                                // Find the data behind the ListViewItem
                    BomExportParameter item = (BomExportParameter)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    if (item == null) return;                   // Abort
                                                                // Initialize the drag & drop operation
                    startIndex = lstView.SelectedIndex;
                    DataObject dragData = new DataObject("BomExportParameter", item);
                    DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {
                WindchillRequestException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private static T FindAnchestor<T>(DependencyObject current)
        where T : DependencyObject
        {
            try
            {
                do
                {
                    if (current is T)
                    {
                        return (T)current;
                    }
                    current = VisualTreeHelper.GetParent(current);
                }
                while (current != null);
                return null;
            }
            catch (Exception ex)
            {
                throw new WindchillRequestException("BomExportFluentWindowView", ex);
            }

        }
        #endregion


    }
}
