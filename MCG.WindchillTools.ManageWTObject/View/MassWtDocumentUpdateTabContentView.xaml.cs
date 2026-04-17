using DocumentFormat.OpenXml.Wordprocessing;
using MCG.WindchillTools.ManageWTObject.Exceptions;
using MCG.WindchillTools.ManageWTObject.ViewModel;
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

namespace MCG.WindchillTools.ManageWTObject.View
{
    /// <summary>
    /// Logique d'interaction pour MassWtDocumentUpdateTabContentView.xaml
    /// </summary>
    public partial class MassWtDocumentUpdateTabContentView : UserControl
    {
        private bool IsAppAlreadyInit { get; set; } = false;
        public MassWtDocumentUpdateViewModel CurrentDataContext { get; set; }

        public MassWtDocumentUpdateTabContentView()
        {
            try
            {
                InitializeComponent();
                DataContextChanged += MassWtDocumentUpdateTabContentView_DataContextChanged;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void MassWtDocumentUpdateTabContentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAppAlreadyInit && DataContext != null && DataContext.GetType() == typeof(MassWtDocumentUpdateViewModel))
                {
                    CurrentDataContext = ((MassWtDocumentUpdateViewModel)DataContext);
                    IsAppAlreadyInit = true;
                    CurrentDataContext.CurrentDataContext.WtDocumentList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler((newsender, newe) => SubscribeToIsSelectedEvent(newsender, newe));
                    SubscribeToIsSelectedEvent(null, null);
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
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
                foreach (var item in CurrentDataContext.CurrentDataContext.WtDocumentList)
                {
                    item.IsSelectedEvent -= CheckIfMultiselection;
                    item.IsSelectedEvent += CheckIfMultiselection;
                }
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
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
                        MultiSelectionAction(((MgtWtDocumentItem)sender).IsSelected);
                    }
                    else
                        PreviousSelectedIndex = GetSelectedIndex(sender);
                }
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        public void MultiSelectionAction(bool SelectedValue)
        {
            try
            {
                IsMultiSelectionInProgress = true;
                for (int index = Math.Min(PreviousSelectedIndex, SelectedIndex); index <= Math.Max(PreviousSelectedIndex, SelectedIndex); index++)
                    ((MgtWtDocumentItem)DgWtDocument.Items[index]).IsSelected = SelectedValue;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
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
                if (DgWtDocument.Items != null)
                {
                    foreach (var item in DgWtDocument.Items)
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
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }
        #endregion


        #region [REGION] Methods for Drag and Drop
        private void DockPanel_Drop(object sender, DragEventArgs e)
        {
            if (!CurrentDataContext.IsSingleWtDocumentDragDropInProgress)
                ImageDragDrop.Visibility = Visibility.Collapsed;
            //MainDockPanel.AllowDrop = false;
        }

        private void MainDockPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (!CurrentDataContext.IsSingleWtDocumentDragDropInProgress)
                ImageDragDrop.Visibility = Visibility.Visible;
        }

        private void MainDockPanel_DragLeave(object sender, DragEventArgs e)
        {
            if (!CurrentDataContext.IsSingleWtDocumentDragDropInProgress)
                ImageDragDrop.Visibility = Visibility.Collapsed;
        }

        private void Image_Drop(object sender, DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Collapsed;
            if (sender.GetType() == typeof(Image))
            {
                object[] obj = new object[2];
                obj[0] = ((Image)sender).DataContext;
                obj[1] = e;
                ((Image)sender).Opacity = 0.1;

                if (CurrentDataContext != null && CurrentDataContext.CommandDragAndDropWtDocument.CanExecute(obj))
                {
                    CurrentDataContext.CommandDragAndDropWtDocument.Execute(obj);
                }
            }
        }

        private void Image_DragEnter(object sender, DragEventArgs e)
        {
            CurrentDataContext.IsSingleWtDocumentDragDropInProgress = true;
            ImageDragDrop.Visibility = Visibility.Collapsed;
            if (sender.GetType() == typeof(Image))
            {
                ((Image)sender).Opacity = 0.8;
            }
        }

        private void Image_DragLeave(object sender, DragEventArgs e)
        {
            CurrentDataContext.IsSingleWtDocumentDragDropInProgress = false;
            ImageDragDrop.Visibility = Visibility.Collapsed;
            if (sender.GetType() == typeof(Image))
            {
                ((Image)sender).Opacity = 0.1;
            }
        }
        #endregion

        #region [REGION] Methods to ba able to uncheck all RadioButton
        private bool JustChecked;
        private void RB_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton s = (RadioButton)sender;
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
            RadioButton s = (RadioButton)sender;
            if (s.IsChecked.Value)
                s.IsChecked = false;
        }
        #endregion

    }
}
