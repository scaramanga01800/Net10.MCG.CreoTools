using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.CadDocQualityCheck.Exceptions;
using MCG.CREO_Tools.CadDocQualityCheck.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MCG.CREO_Tools.CadDocQualityCheck.View
{
    public partial class CadDocQualityCheckTabContentView : UserControl
    {

        private bool IsAppAlreadyInit { get; set; } = false;
        private bool IsMouseInRowDetail { get; set; } = false;
        public CadDocQualityCheckViewModel CurrentDataContext { get; set; }

        public CadDocQualityCheckTabContentView()
        {
            try
            {
                InitializeComponent();
                DataContextChanged += CadDocQualityCheckTabContentView_DataContextChanged;
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void CadDocQualityCheckTabContentView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAppAlreadyInit && DataContext != null && DataContext.GetType() == typeof(CadDocQualityCheckViewModel))
                {
                    CurrentDataContext = ((CadDocQualityCheckViewModel)DataContext);
                    IsAppAlreadyInit = true;
                    CurrentDataContext.CurrentDataContext.ShownCadModels.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler((newsender, newe) => SubscribeToIsSelectedEvent(newsender, newe));
                    SubscribeToIsSelectedEvent(null, null);
                }
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
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
                foreach (var item in CurrentDataContext.CurrentDataContext.ShownCadModels)
                {
                    item.IsSelectedEvent -= CheckIfMultiselection;
                    item.IsSelectedEvent += CheckIfMultiselection;
                }
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
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
                        MultiSelectionAction(((CadDocQualityCheckItem)sender).IsSelected);
                    }
                    else
                        PreviousSelectedIndex = GetSelectedIndex(sender);
                }
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        public void MultiSelectionAction(bool SelectedValue)
        {
            try
            {
                IsMultiSelectionInProgress = true;
                for (int index = Math.Min(PreviousSelectedIndex, SelectedIndex); index <= Math.Max(PreviousSelectedIndex, SelectedIndex); index++)
                    ((CadDocQualityCheckItem)DataGridCadItem.Items[index]).IsSelected = SelectedValue;
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
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
                if (DataGridCadItem.Items != null)
                {
                    foreach (var item in DataGridCadItem.Items)
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
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }
        #endregion

        private void DataGridCadItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!IsMouseInRowDetail && DataGridCadItem.SelectedCells.Count > 0)
                {
                    CurrentDataContext.CurrentDataContext.SelectedItem = (CadDocQualityCheckItem)DataGridCadItem.SelectedCells.First().Item;

                    DataGridRow row = (DataGridRow)DataGridCadItem.ItemContainerGenerator.ContainerFromItem(CurrentDataContext.CurrentDataContext.SelectedItem);

                    if (row != null)
                        if (row.DetailsVisibility == Visibility.Visible)
                            row.DetailsVisibility = Visibility.Collapsed;
                        else
                            row.DetailsVisibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            IsMouseInRowDetail = true;
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseInRowDetail = false;
        }
    }
}
