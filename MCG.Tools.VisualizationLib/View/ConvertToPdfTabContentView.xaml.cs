using MCG.Tools.VisualizationLib.Exceptions;
using MCG.Tools.VisualizationLib.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MCG.Tools.VisualizationLib.View
{
    public partial class ConvertToPdfTabContentView : UserControl
    {
        private bool IsAppAlreadyInit { get; set; } = false;
        public ConvertToPdfViewModel CurrentDataContext { get; set; }

        public ConvertToPdfTabContentView()
        {
            try
            {
                InitializeComponent();
                DataContextChanged += ConvertToPdfTabMainView_DataContextChanged;
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ConvertToPdfTabMainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAppAlreadyInit && DataContext != null && DataContext.GetType() == typeof(ConvertToPdfViewModel))
                {
                    CurrentDataContext = (ConvertToPdfViewModel)DataContext;
                    IsAppAlreadyInit = true;
                    CurrentDataContext.CurrentDataContext.ListConvertItem.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler((newsender, newe) => SubscribeToIsSelectedEvent(newsender, newe));
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
                foreach (var item in CurrentDataContext.CurrentDataContext.ListConvertItem)
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
                        MultiSelectionAction(((ConvertToPdfItem)sender).IsSelected);
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
                    ((ConvertToPdfItem)DgFiles.Items[index]).IsSelected = SelectedValue;
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
                if (DgFiles.Items != null)
                {
                    foreach (var item in DgFiles.Items)
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

        private void MainSP_Drop(object sender, DragEventArgs e)
        {
            try
            {
                ImageDragDrop.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
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
