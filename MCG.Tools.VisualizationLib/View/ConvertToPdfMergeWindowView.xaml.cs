using Fluent;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.VisualizationLib.Exceptions;
using MCG.Tools.VisualizationLib.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MCG.Tools.VisualizationLib.View
{
    /// <summary>
    /// Logique d'interaction pour ConvertToPdfMergeWindowView.xaml
    /// </summary>
    public partial class ConvertToPdfMergeWindowView : RibbonWindow
    {

        public ConvertToPdfMergeWindowViewModel CurrentDataContext { get; set; }
        public MessageBoxResult Return { get; set; } = MessageBoxResult.Cancel;

        public ConvertToPdfMergeWindowView(ConvertToPdfMergeWindowViewModel currentVM)
        {
            try
            {
                TraceLog.AddTraceLog("Create ConvertToPdfMergeWindowView");
                InitializeComponent();
                CurrentDataContext = currentVM;
                DataContext = CurrentDataContext;
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }

        public void SetConvertToPdfMergeWindowViewProperties(List<ConvertToPdfItem> pListFiles, string DefaultFileName)
        {
            try

            {
                CurrentDataContext.SetConvertToPdfMergeWindowViewModelProperties(pListFiles, DefaultFileName);
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }


        private void ConvertToPdfMergeWindowView_Closed(object sender, EventArgs e)
        {
            Return = CurrentDataContext.Return;
        }

        #region [REGION] Drag and Drop Methods
        private Point startPoint = new Point();
        private int startIndex = -1;

        private void ListView_CurrentMousePosition(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Get current mouse position
                startPoint = e.GetPosition(null);
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }


        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                if (!e.Data.GetDataPresent("ConvertToPdfItem") || sender != e.Source)
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
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
                throw new VisualizationException("ConvertToPdfMergeWindowView", ex);
            }

        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            try
            {
                int index = -1;

                if (e.Data.GetDataPresent("ConvertToPdfItem") && sender == e.Source)
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
                    ConvertToPdfItem item = (ConvertToPdfItem)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

                    // Move item into observable collection 
                    // (this will be automatically reflected to lstView.ItemsSource)
                    e.Effects = DragDropEffects.Move;
                    index = CurrentDataContext.ListFiles.IndexOf(item);
                    if (startIndex >= 0 && index >= 0)
                    {
                        CurrentDataContext.ListFiles.Move(startIndex, index);
                    }
                    startIndex = -1;
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ListView_MouseMove(object sender, MouseEventArgs e)
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
                    ConvertToPdfItem item = (ConvertToPdfItem)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    if (item == null) return;                   // Abort
                                                                // Initialize the drag & drop operation
                    startIndex = lstView.SelectedIndex;
                    DataObject dragData = new DataObject("ConvertToPdfItem", item);
                    DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        #endregion

    }
}
