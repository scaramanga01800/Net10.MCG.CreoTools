using MCG.Tools.VisualizationLib.Exceptions;
using MCG.Tools.VisualizationLib.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MCG.Tools.VisualizationLib.View
{
    public partial class DownloadVisualizationFileTabContentView : UserControl
    {
        #region [REGION] Internal variables
        private bool IsAppAlreadyInit { get; set; } = false;
        public DownloadVisualizationFileViewModel CurrentDataContext { get; set; }
        #endregion

        public DownloadVisualizationFileTabContentView()
        {
            try
            {
                InitializeComponent();
                DataContextChanged += DownloadVisualizationFileTabContentView_DataContextChanged;
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void DownloadVisualizationFileTabContentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAppAlreadyInit && DataContext != null && DataContext.GetType() == typeof(DownloadVisualizationFileViewModel))
                {
                    CurrentDataContext = ((DownloadVisualizationFileViewModel)DataContext);
                    IsAppAlreadyInit = true;
                    CurrentDataContext.CurrentDataContext.SearchedPartList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler((newsender, newe) => SubscribeToIsSelectedEvent(newsender, newe));
                    SubscribeToIsSelectedEvent(null, null);
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        #region [REGION] Methods for Multiselection with Shift
        public int PreviousSelectedIndex { get; set; } = -1;
        public int SelectedIndex { get; set; } = -1;
        public bool IsMultiSelectionInProgress { get; set; } = false;

        private void SubscribeToIsSelectedEvent(object sender, EventArgs e)
        {
            try
            {
                foreach (var item in CurrentDataContext.CurrentDataContext.SearchedPartList)
                {
                    item.IsSelectedEvent -= CheckIfMultiselection;
                    item.IsSelectedEvent += CheckIfMultiselection;
                }
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }

        private void CheckIfMultiselection(object sender, EventArgs e)
        {
            try
            {
                if (!IsMultiSelectionInProgress)
                {
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                    {
                        SelectedIndex = GetSelectedIndex(sender);
                        MultiSelectionAction(((VisualizationItem)sender).IsSelected);
                    }
                    else
                        PreviousSelectedIndex = GetSelectedIndex(sender);
                }
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }

        public void MultiSelectionAction(bool SelectedValue)
        {
            try
            {
                IsMultiSelectionInProgress = true;
                for (int index = Math.Min(PreviousSelectedIndex, SelectedIndex); index <= Math.Max(PreviousSelectedIndex, SelectedIndex); index++)
                    ((VisualizationItem)DgParts.Items[index]).IsSelected = SelectedValue;
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
            finally
            {
                IsMultiSelectionInProgress = false;
            }
        }

        private int GetSelectedIndex(object SelectedItem)
        {
            try
            {
                int CurrentIndex = 0;
                if (DgParts.Items != null)
                {
                    foreach (var item in DgParts.Items)
                    {
                        if (item.GetHashCode() == SelectedItem.GetHashCode())
                            return CurrentIndex;
                        CurrentIndex++;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }
        #endregion


        #region [REGION] Methods for Drag and Drop
        private void DockPanel_Drop(object sender, DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Collapsed;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var file = files[0];
            }
        }

        private void MainDockPanel_DragEnter(object sender, DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Visible;
        }

        private void MainDockPanel_DragLeave(object sender, DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}
