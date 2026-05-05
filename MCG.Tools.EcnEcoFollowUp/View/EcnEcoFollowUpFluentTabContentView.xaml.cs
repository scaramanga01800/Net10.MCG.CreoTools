using MCG.CommonLib.Services.Statics;
using MCG.Converters;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MCG.Tools.EcnEcoFollowUp.View
{
    public partial class EcnEcoFollowUpFluentTabContentView : UserControl
    {
        public EcnEcoFollowUpViewModel CurrentEcnEcoFollowUpViewModel { get; set; }
        private bool IsAppAlreadyInit = false;
        private int PreviousSelectedIndex = -1;
        private int SelectedIndex = -1;
        private bool IsMultiSelectionInProgress = false;


        public EcnEcoFollowUpFluentTabContentView()
        {
            InitializeComponent();
            DataContextChanged += EcnEcoFollowUpFluentTabContentView_DataContextChanged;

        }

        private void EcnEcoFollowUpFluentTabContentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAppAlreadyInit && DataContext != null && DataContext.GetType() == typeof(EcnEcoFollowUpViewModel))
                {
                    CurrentEcnEcoFollowUpViewModel = ((EcnEcoFollowUpViewModel)DataContext);
                    CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.DashboardListEvent += UpdateMenuDashboard;
                    UpdateMenuDashboard();
                    IsAppAlreadyInit = true;
                    CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.EcnShownList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler((sender, e) => SubscribeToIsSelectedEvent(sender, e));
                    SubscribeToIsSelectedEvent(null, null);
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
                System.Windows.Controls.TabItem CurrentTabItem;
                Binding TempBinding;

                if (CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.DashboardList != null)
                {
                    MenuItem newMenuDashboard;
                    MiSearchAddEcnEcoToDashboard.Items.Clear();

                    foreach (var Dashboard in CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.DashboardList)
                    {
                        if (!Dashboard.IsTabCreated)
                        {
                            CurrentTabItem = new System.Windows.Controls.TabItem();
                            EcnEcoFollowUpDashboardView CurrentEcnEcoFollowUpDashboardView = new EcnEcoFollowUpDashboardView(Dashboard);
                            CurrentTabItem.Content = CurrentEcnEcoFollowUpDashboardView;
                            CurrentTabItem.DataContext = CurrentEcnEcoFollowUpDashboardView.CurrentEcnEcoFollowUpDashboardViewModel;
                            TempBinding = new Binding("DashboardItem.IsShown");
                            TempBinding.Converter = new BoolToVisibilityConverter();
                            CurrentTabItem.SetBinding(System.Windows.Controls.TabItem.VisibilityProperty, TempBinding);
                            CurrentTabItem.SetBinding(System.Windows.Controls.TabItem.HeaderProperty, new Binding("DashboardItem.Name"));
                            Dashboard.DashboardHideEvent += SelectMainTab;
                            Dashboard.DashboardShowEvent += SelectShownTab;

                            TabControlDashboard.Items.Add(CurrentTabItem);
                            Dashboard.IsTabCreated = true;
                        }
                        if (!Dashboard.DashboardItem.IsReadOnly || Dashboard.DashboardItem.IsCreator)
                        {
                            newMenuDashboard = new MenuItem() { DataContext = Dashboard };
                            newMenuDashboard.SetBinding(MenuItem.HeaderProperty, new Binding("DashboardItem.Name"));
                            newMenuDashboard.SetBinding(MenuItem.CommandProperty, new Binding("ParentApp.CommandMenutItemAddEcnEcoToDashboard"));
                            newMenuDashboard.CommandParameter = Dashboard;
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

        private void SelectMainTab(object sender, EventArgs e)
        {
            try
            {
                if (TabControlDashboard != null
                    && TabControlDashboard.Items.Count > 0
                    && ((System.Windows.Controls.TabItem)TabControlDashboard.SelectedItem).Content.GetType() == typeof(EcnEcoFollowUpDashboardView)
                    && ((EcnEcoFollowUpDashboardView)((System.Windows.Controls.TabItem)TabControlDashboard.SelectedItem).Content).CurrentEcnEcoFollowUpDashboardViewModel.GetHashCode() == sender.GetHashCode())
                    TabControlDashboard.SelectedItem = TabControlDashboard.Items[0];
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void SelectShownTab(object sender, EventArgs e)
        {
            try
            {
                if (sender != null && TabControlDashboard != null && TabControlDashboard.Items.Count > 0)
                {
                    System.Windows.Controls.TabItem CurrentTabItem = null;
                    for (int index = 0; index < TabControlDashboard.Items.Count; index++)
                    {
                        if (((TabItem)TabControlDashboard.Items[index]).Content.GetType() == typeof(EcnEcoFollowUpDashboardView)
                            && ((EcnEcoFollowUpDashboardView)((System.Windows.Controls.TabItem)TabControlDashboard.Items[index]).Content).CurrentEcnEcoFollowUpDashboardViewModel.GetHashCode() == sender.GetHashCode())
                            CurrentTabItem = (System.Windows.Controls.TabItem)TabControlDashboard.Items[index];
                    }
                    if (CurrentTabItem != null)
                        TabControlDashboard.SelectedItem = CurrentTabItem;
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void DgECN_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (DgEcnEco.SelectedCells.Count > 0)
                    CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.SelectedEcn = (EFU_EcnEcoToShowEndUser)DgEcnEco.SelectedCells.First().Item;

                if (DgEcnEco.SelectedCells.Count > 1)
                    DgEcnEco.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;

                else if (DgEcnEco.SelectedCells.Count == 1)
                {
                    DataGridRow row = (DataGridRow)DgEcnEco.ItemContainerGenerator.ContainerFromItem(CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.SelectedEcn);
                    if (row.DetailsVisibility == Visibility.Visible)
                        row.DetailsVisibility = Visibility.Collapsed;
                    else
                        row.DetailsVisibility = Visibility.Visible;
                }
                else
                    DgEcnEco.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }


        #region [REGION] Events
        private void SubscribeToIsSelectedEvent(object sender, EventArgs e)
        {
            try
            {
                foreach (var item in CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.EcnShownList)
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
        #endregion


        #region [REGION] Methods for Multiselection
        private void CheckIfMultiselection(object sender, EventArgs e)
        {
            try
            {
                if (!IsMultiSelectionInProgress)
                {
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                    {
                        SelectedIndex = GetSelectedIndex(sender);
                        MultiSelectionAction(((EFU_EcnEcoToShowEndUser)sender).IsSelected);
                    }
                    else
                        PreviousSelectedIndex = GetSelectedIndex(sender);
                }
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        public void MultiSelectionAction(bool SelectedValue)
        {
            try
            {
                IsMultiSelectionInProgress = true;
                for (int index = Math.Min(PreviousSelectedIndex, SelectedIndex); index <= Math.Max(PreviousSelectedIndex, SelectedIndex); index++)
                    ((EFU_EcnEcoToShowEndUser)DgEcnEco.Items[index]).IsSelected = SelectedValue;
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

        private int GetSelectedIndex(object SelectedItem)
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
        #endregion

        private void TabControlDashboard_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    object CurrentApp = e.AddedItems[0];
                    if (CurrentApp != null && CurrentApp.GetType() == typeof(TabItem))
                    {
                        var currentDataContext = ((TabItem)CurrentApp).Content;
                        if (currentDataContext.GetType() == typeof(EcnEcoFollowUpDashboardView))
                        {
                            MethodInfo CurrentMethodInit = currentDataContext.GetType().GetMethod("InitApp");
                            if (CurrentMethodInit != null)
                            {
                                TraceLog.AddTraceLog($"Enter {CurrentApp.GetType().Name} App");
                                CurrentMethodInit.Invoke(currentDataContext, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
