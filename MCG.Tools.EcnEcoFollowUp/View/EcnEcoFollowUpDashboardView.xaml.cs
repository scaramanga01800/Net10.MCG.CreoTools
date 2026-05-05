using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MCG.Tools.EcnEcoFollowUp.View
{
    public partial class EcnEcoFollowUpDashboardView : UserControl
    {
        public EcnEcoFollowUpDashboardViewModel CurrentEcnEcoFollowUpDashboardViewModel { get; set; }
        private int PreviousSelectedIndex = -1;
        private int SelectedIndex = -1;
        private bool IsMultiSelectionInProgress = false;
        private bool IsAppAlreadyInit { get; set; } = false;

        public EcnEcoFollowUpDashboardView(EcnEcoFollowUpDashboardViewModel currentVm)
        {
            InitializeComponent();
            CurrentEcnEcoFollowUpDashboardViewModel = currentVm;
        }


        public void SetEcnEcoFollowUpDashboardViewProperties(EFU_DashboardItem currentEFU_DashboardItem)
        {
            try
            {
                CurrentEcnEcoFollowUpDashboardViewModel.SetEcnEcoFollowUpDashboardViewModelProperties(currentEFU_DashboardItem);
                this.DataContext = CurrentEcnEcoFollowUpDashboardViewModel;

                if (!IsAppAlreadyInit && CurrentEcnEcoFollowUpDashboardViewModel != null && CurrentEcnEcoFollowUpDashboardViewModel.ParentApp != null)
                {
                    CurrentEcnEcoFollowUpDashboardViewModel.DashboardItem.ListEcnEco.CollectionChanged += new NotifyCollectionChangedEventHandler((sender, e) => SubscribeToIsSelectedEvent(sender, e));
                    StartSubscribeToAllEcnEcoIsSelected();
                    CurrentEcnEcoFollowUpDashboardViewModel.ParentApp.CurrentEcnEcoFollowUpDataContext.DashboardListEvent += UpdateMenuDashboard;
                    UpdateMenuDashboard();
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }

        }

        private void UpdateMenuDashboard(object sender = null, EventArgs e = null)
        {
            try
            {
                if (CurrentEcnEcoFollowUpDashboardViewModel.ParentApp.CurrentEcnEcoFollowUpDataContext.DashboardList != null)
                {
                    MenuItem newMenuDashboard;
                    MiSearchAddEcnEcoToDashboard.Items.Clear();

                    foreach (var Dashboard in CurrentEcnEcoFollowUpDashboardViewModel.ParentApp.CurrentEcnEcoFollowUpDataContext.DashboardList)
                    {

                        //Update ContextMenu in the dashboard
                        if (!Dashboard.DashboardItem.IsReadOnly || Dashboard.DashboardItem.IsCreator)
                        {
                            newMenuDashboard = new MenuItem() { DataContext = CurrentEcnEcoFollowUpDashboardViewModel };
                            newMenuDashboard.SetBinding(MenuItem.HeaderProperty, new Binding("DashboardItem.Name"));
                            newMenuDashboard.SetBinding(MenuItem.CommandProperty, new Binding("CommandMenutItemAddEcnEcoToDashboard"));
                            newMenuDashboard.CommandParameter = Dashboard;
                            newMenuDashboard.Header = Dashboard;
                            MiSearchAddEcnEcoToDashboard.Items.Add(newMenuDashboard);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void StartSubscribeToAllEcnEcoIsSelected()
        {
            try
            {
                foreach (var item in CurrentEcnEcoFollowUpDashboardViewModel.DashboardItem.ListEcnEco)
                {
                    item.IsSelectedEvent -= CheckIfMultiselection;
                    item.IsSelectedEvent += CheckIfMultiselection;
                }

            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
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
                        SelectedIndex = GetSelectedIndex((EFU_DashboardEcnEco)sender);
                        MultiSelectionAction(((EFU_DashboardEcnEco)sender).IsSelected);
                    }
                    else
                        PreviousSelectedIndex = GetSelectedIndex((EFU_DashboardEcnEco)sender);
                }
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private int GetSelectedIndex(EFU_DashboardEcnEco SelectedItem)
        {
            try
            {
                int CurrentIndex = 0;
                if (DgEcnEco.Items != null)
                {
                    foreach (var item in DgEcnEco.Items)
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
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void SubscribeToIsSelectedEvent(object sender, EventArgs e)
        {
            try
            {
                StartSubscribeToAllEcnEcoIsSelected();
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void DataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (DgEcnEco.SelectedCells.Count > 0)
                    CurrentEcnEcoFollowUpDashboardViewModel.DashboardItem.SelectedEcnEco = (EFU_DashboardEcnEco)DgEcnEco.SelectedCells.First().Item;


                if (DgEcnEco.SelectedCells.Count == 1)
                {
                    DataGridRow row = (DataGridRow)DgEcnEco.ItemContainerGenerator.ContainerFromItem(CurrentEcnEcoFollowUpDashboardViewModel.DashboardItem.SelectedEcnEco);

                    if (DgEcnEco.SelectedCells.First().Column.DisplayIndex > 1)
                        if (row.DetailsVisibility == Visibility.Visible)
                            row.DetailsVisibility = Visibility.Collapsed;
                        else
                            row.DetailsVisibility = Visibility.Visible;
                }
                else
                    DgEcnEco.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void MultiSelectionAction(bool SelectedValue)
        {
            try
            {
                IsMultiSelectionInProgress = true;
                for (int index = Math.Min(PreviousSelectedIndex, SelectedIndex); index <= Math.Max(PreviousSelectedIndex, SelectedIndex); index++)
                    ((EFU_DashboardEcnEco)DgEcnEco.Items[index]).IsSelected = SelectedValue;
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
            finally
            {
                IsMultiSelectionInProgress = false;
            }
        }

        string[] displayedColumnOrder;

        private void DgEcnEco_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null
                    && sender.GetType() == typeof(DataGrid)
                    && CurrentEcnEcoFollowUpDashboardViewModel.DashboardItem.CurrentDashboardConfiguration.ColumnsOrder != null
                    && CurrentEcnEcoFollowUpDashboardViewModel.DashboardItem.CurrentDashboardConfiguration.ColumnsOrder.Count() > 0)
                {
                    DataGrid currentDatagrid = (DataGrid)sender;

                    // Search current Columns
                    ObservableCollection<DataGridColumn> columnCollection = currentDatagrid.Columns;
                    List<DataGridColumn> tempColumnCollection = columnCollection.ToList();

                    string[] currentDisplayedColumnOrder = new string[columnCollection.Count()];
                    int columnIndexWorking;
                    foreach (var item_Column in columnCollection)
                    {
                        if (item_Column.GetType() == typeof(MCG.CommonLib.WpfComponent.View.DataGridComboBoxColumnMcg) ||
                            item_Column.GetType() == typeof(MCG.CommonLib.WpfComponent.View.DataGridTemplateColumnMcg) ||
                            item_Column.GetType() == typeof(MCG.CommonLib.WpfComponent.View.DataGridTextColumnMcg))
                        {
                            columnIndexWorking = item_Column.DisplayIndex;
                            if (columnIndexWorking != -1)
                                currentDisplayedColumnOrder[columnIndexWorking] = item_Column.GetType().GetProperty("ColumnId").GetValue(item_Column).ToString();
                        }
                        else
                            tempColumnCollection.Remove(item_Column);
                    }

                    // Update Columns with new order
                    ObservableCollection<DataGridColumn> newColumnCollection = new ObservableCollection<DataGridColumn>();

                    DataGridColumn tempDataGridCol = null;
                    foreach (var strCol in CurrentEcnEcoFollowUpDashboardViewModel.DashboardItem.CurrentDashboardConfiguration.ColumnsOrder)
                    {
                        tempDataGridCol = tempColumnCollection.FirstOrDefault((item) => item.GetType().GetProperty("ColumnId").GetValue(item).ToString() == strCol);
                        if (tempDataGridCol != null)
                        {
                            newColumnCollection.Add(tempDataGridCol);
                            tempColumnCollection.Remove(tempDataGridCol);
                        }
                    }

                    if (tempColumnCollection.Count > 0)
                    {
                        foreach (var col in tempColumnCollection)
                            newColumnCollection.Add(col);
                    }

                    //currentDatagrid.Columns = newColumnCollection;
                    currentDatagrid.Columns.Clear();
                    foreach (var col in newColumnCollection)
                        currentDatagrid.Columns.Add(col);
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        void _getColumnOrder(IEnumerable<DataGridColumn> columnCollection)
        {
            try
            {


                if (!CurrentEcnEcoFollowUpDashboardViewModel.DashboardItem.IsPersonalDashBoard)
                {
                    DataGridColumn[] columnArray;
                    int columnIndexWorking;
                    displayedColumnOrder = new string[columnCollection.Count()];
                    columnArray = columnCollection.ToArray();

                    foreach (var item_Column in columnCollection)
                    {
                        if (item_Column.GetType() == typeof(MCG.CommonLib.WpfComponent.View.DataGridComboBoxColumnMcg) ||
                            item_Column.GetType() == typeof(MCG.CommonLib.WpfComponent.View.DataGridTemplateColumnMcg) ||
                            item_Column.GetType() == typeof(MCG.CommonLib.WpfComponent.View.DataGridTextColumnMcg))
                        {
                            columnIndexWorking = item_Column.DisplayIndex;
                            if (columnIndexWorking != -1)
                                displayedColumnOrder[columnIndexWorking] = item_Column.GetType().GetProperty("ColumnId").GetValue(item_Column).ToString();
                        }
                    }

                    CurrentEcnEcoFollowUpDashboardViewModel.DashboardItem.CurrentDashboardConfiguration.ColumnsOrder = displayedColumnOrder;
                    CurrentEcnEcoFollowUpDashboardViewModel.DashboardItem.CurrentDashboardConfiguration.RaiseIsUpdateColumsOrderUserEvent();
                }
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        private void DgEcnEco_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {
            try
            {
                DataGrid _dataGrid = (DataGrid)sender;
                _getColumnOrder(_dataGrid.Columns);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
