using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.ViewModel.BomExport;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MCG.CREO_Tools.MiscTools.View.BomExport
{
    public partial class BomExportTabMainView : RibbonTabItem
    {
        private BomExportWindowViewModel CurrentBomExportWindowViewModel;
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


        public BomExportTabMainView()
        {
            try
            {
                TraceLog.AddTraceLog($"BomExportWindowViewModel: Init app");

                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);

                DataContextChanged += BomExportTabMainView_DataContextChanged;
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void BomExportTabMainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext != null && DataContext.GetType() == typeof(BomExportWindowViewModel))
                {
                    TraceLog.AddTraceLog($"BomExportWindowViewModel: Init DataContext");
                    CurrentBomExportWindowViewModel = DataContext as BomExportWindowViewModel;
                    CurrentBomExportWindowViewModel.ActionDoneEvent += RaiseActionDoneEvent;
                    CurrentBomExportWindowViewModel.ActionInProgressEvent += RaiseActionInProgressEvent;
                    CurrentBomExportWindowViewModel.CurrentBomExportWindowDataContext.ShowSapCostVolumeInfo = false;
                    CurrentBomExportWindowViewModel.CurrentBomExportWindowDataContext.RaiseIsParameterUpdateEvent();
                    IsAlreadyInit = true;
                }
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

        #region [REGION] Drag and Drop Methods
        private Point startPoint { get; set; } = new Point();
        private int startIndex { get; set; } = -1;

        private void lstView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Get current mouse position
                startPoint = e.GetPosition(null);
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void lstView_DragEnter(object sender, DragEventArgs e)
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
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void lstView_Drop(object sender, DragEventArgs e)
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
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void lstView_MouseMove(object sender, MouseEventArgs e)
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
                                                                //startIndex = lstView.SelectedIndex;
                    DataObject dragData = new DataObject("BomExportParameter", item);
                    DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
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
                throw new MiscToolsException("BomExportFluentWindowView", ex);
            }

        }
        #endregion

    }
}
