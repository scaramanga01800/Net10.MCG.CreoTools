using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.Converters;
using MCG.Tools.EcnEcoFollowUp.Configuration;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using System.Windows.Data;
using System.Windows.Input;

namespace MCG.Tools.EcnEcoFollowUp.View
{
    public partial class EcnEcoFollowUpFluentTabView : RibbonTabItem
    {

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

        public event EventHandler DataContextUpdatedEvent;
        public void RaiseDataContextUpdatedEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                DataContextUpdatedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        private bool IsAppAlreadyInit { get; set; } = false;
        private EcnEcoFollowUpViewModel CurrentEcnEcoFollowUpViewModel { get; set; }
        private string ImageResourcePath { get; set; }

        #region [REGION] Init
        public EcnEcoFollowUpFluentTabView()
        {
                TraceLog.AddTraceLog("Create EcnEcoFollowUpFluentTabView");
            ImageResourcePath = EcnEcoFollowUpConstants.ImageResourcesPath;

            string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);

            if (MainAppFolder == null || MainAppFolder == "")
                MainAppFolder = CommonLibConstants.MainAppFolder;
            McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{EcnEcoFollowUpConstants.MainDictionary}", UriKind.Absolute);

            InitializeComponent();
            DataContextChanged += EcnEcoFollowUpFluentTabView_DataContextChanged;

        }

        private void EcnEcoFollowUpFluentTabView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAppAlreadyInit && DataContext != null && DataContext.GetType() == typeof(EcnEcoFollowUpViewModel))
                {
                    CurrentEcnEcoFollowUpViewModel = DataContext as EcnEcoFollowUpViewModel;
                    IsAppAlreadyInit = true;

                    CurrentEcnEcoFollowUpViewModel.ActionDoneEvent += RaiseActionDoneEvent;
                    CurrentEcnEcoFollowUpViewModel.ActionInProgressEvent += RaiseActionInProgressEvent;

                    CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.RecentSearchesListEvent += UpdateMenuRecentSearches;
                    CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.SavedSearchesListEvent += UpdateMenuSavedSearches;
                    CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.DashboardListEvent += UpdateMenuDashboard;

                    UpdateMenuRecentSearches();
                    UpdateMenuSavedSearches();
                    UpdateMenuDashboard();

                    CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.EcnShownList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler((sender, e) => SubscribeToIsSelectedEvent(sender, e));
                    StartSubscribeToAllEcnEcoIsSelected();
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateMenuRecentSearches(object sender = null, EventArgs e = null)
        {
            try
            {
                if (CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.RecentSearchesList != null)
                {
                    MenuItem newMenuItem;

                    MiRecentSearches.Items.Clear();

                    foreach (var search in CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.RecentSearchesList)
                    {
                        newMenuItem = new MenuItem() { DataContext = CurrentEcnEcoFollowUpViewModel, Header = search.Name };
                        //newMenuItem.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage(Properties.Resources.search_16x16) };
                        newMenuItem.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/search_16x16.png") };
                        newMenuItem.SetBinding(MenuItem.CommandProperty, new Binding("CommandSavedOrRecentSearch"));
                        newMenuItem.CommandParameter = search;
                        MiRecentSearches.Items.Add(newMenuItem);
                    }
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateMenuSavedSearches(object sender = null, EventArgs e = null)
        {
            try
            {
                if (CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.SavedSearchesList != null)
                {
                    MenuItem newMenuItem;
                    MenuItem newMenuDelete;
                    MenuItem newMenuRename;
                    MenuItem newMenuUpdate;
                    MenuItem newMenuSearch;
                    MenuItem newMenuExport;
                    MiSavedSearches.Items.Clear();

                    foreach (var search in CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.SavedSearchesList)
                    {
                        newMenuItem = new MenuItem() { DataContext = CurrentEcnEcoFollowUpViewModel, Header = search.Name };
                        //newMenuItem.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage(Properties.Resources.search_16x16) };
                        newMenuItem.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/search_16x16.png") };

                        newMenuExport = new MenuItem() { DataContext = CurrentEcnEcoFollowUpViewModel, Header = McgWpfTools.GetStringResource("EFU_BtExportExcel") };
                        newMenuSearch = new MenuItem() { DataContext = CurrentEcnEcoFollowUpViewModel, Header = McgWpfTools.GetStringResource("EFU_MiSearch") };
                        newMenuRename = new MenuItem() { DataContext = CurrentEcnEcoFollowUpViewModel, Header = McgWpfTools.GetStringResource("EFU_MiRename") };
                        newMenuUpdate = new MenuItem() { DataContext = CurrentEcnEcoFollowUpViewModel, Header = McgWpfTools.GetStringResource("EFU_MiUpdate") };
                        newMenuDelete = new MenuItem() { DataContext = CurrentEcnEcoFollowUpViewModel, Header = McgWpfTools.GetStringResource("EFU_MiDelete") };

                        newMenuExport.SetBinding(MenuItem.CommandProperty, new Binding("CommandExportSearch"));
                        newMenuExport.CommandParameter = search;

                        newMenuSearch.SetBinding(MenuItem.CommandProperty, new Binding("CommandSavedOrRecentSearch"));
                        newMenuSearch.CommandParameter = search;

                        newMenuRename.SetBinding(MenuItem.CommandProperty, new Binding("CommandRenameSearch"));
                        newMenuRename.CommandParameter = search;

                        newMenuUpdate.SetBinding(MenuItem.CommandProperty, new Binding("CommandUpdateSearch"));
                        newMenuUpdate.CommandParameter = search;

                        newMenuDelete.SetBinding(MenuItem.CommandProperty, new Binding("CommandDeleteSearch"));
                        newMenuDelete.CommandParameter = search;

                        newMenuSearch.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/execute_16x16.gif") };
                        newMenuExport.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/icon_excel.gif") };
                        newMenuRename.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/rename_16x16.png") };
                        newMenuUpdate.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/update_16x16.png") };
                        newMenuDelete.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/Delete.ico") };

                        newMenuItem.Items.Add(newMenuSearch);
                        newMenuItem.Items.Add(newMenuExport);
                        newMenuItem.Items.Add(newMenuRename);
                        newMenuItem.Items.Add(newMenuUpdate);
                        newMenuItem.Items.Add(newMenuDelete);

                        MiSavedSearches.Items.Add(newMenuItem);
                    }

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
                Binding TempBinding;
                if (CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.DashboardList != null)
                {
                    MenuItem newMenuItem;

                    // All sub-menu of main menu
                    MenuItem newMenuDelete;
                    MenuItem newMenuRename = null;
                    MenuItem newMenuShow;
                    MenuItem newMenuHide;
                    MenuItem newMenuExport;
                    MiSavedDashboard.Items.Clear();

                    foreach (var Dashboard in CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.DashboardList)
                    {
                        newMenuItem = new MenuItem() { DataContext = Dashboard };
                        newMenuItem.SetBinding(MenuItem.HeaderProperty, new Binding("DashboardItem.Name"));

                        newMenuShow = new MenuItem() { DataContext = Dashboard, Header = McgWpfTools.GetStringResource("EFU_MiSavedDashboardShow") };
                        newMenuHide = new MenuItem() { DataContext = Dashboard, Header = McgWpfTools.GetStringResource("EFU_MiSavedDashboardHide") };
                        newMenuExport = new MenuItem() { DataContext = Dashboard, Header = McgWpfTools.GetStringResource("EFU_MiSavedDashboardExport") };
                        if (Dashboard.DashboardItem.IsCreator)
                            newMenuRename = new MenuItem() { DataContext = Dashboard, Header = McgWpfTools.GetStringResource("EFU_MiSavedDashboardRename") };

                        if (Dashboard.DashboardItem.IsCreator)
                        {
                            newMenuItem.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/Dashboard_16x16.png") };
                            newMenuDelete = new MenuItem() { DataContext = Dashboard, Header = McgWpfTools.GetStringResource("EFU_MiSavedDashboardDelete") };
                        }
                        else
                        {
                            newMenuItem.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/Dashboard_shared_16x16.png") };
                            newMenuDelete = new MenuItem() { DataContext = Dashboard, Header = McgWpfTools.GetStringResource("EFU_MiSavedDashboardRemove") };
                        }

                        TempBinding = new Binding("DashboardItem.IsShown");
                        TempBinding.Converter = new InverseBoolToVisibilityConverter();
                        newMenuShow.SetBinding(MenuItem.VisibilityProperty, TempBinding);
                        newMenuShow.SetBinding(System.Windows.Controls.MenuItem.CommandProperty, new Binding("ParentApp.CommandDashBoardShow"));
                        newMenuShow.CommandParameter = Dashboard;

                        TempBinding = new Binding("DashboardItem.IsShown");
                        TempBinding.Converter = new BoolToVisibilityConverter();
                        newMenuHide.SetBinding(MenuItem.VisibilityProperty, TempBinding);
                        newMenuHide.SetBinding(MenuItem.CommandProperty, new Binding("ParentApp.CommandDashBoardHide"));
                        newMenuHide.CommandParameter = Dashboard;

                        newMenuExport.SetBinding(MenuItem.CommandProperty, new Binding("ParentApp.CommandDashBoardExport"));
                        newMenuExport.CommandParameter = Dashboard;

                        if (Dashboard.DashboardItem.IsCreator)
                        {
                            newMenuRename.SetBinding(MenuItem.CommandProperty, new Binding("ParentApp.CommandDashBoardRename"));
                            newMenuRename.CommandParameter = Dashboard;
                        }
                        if (Dashboard.DashboardItem.IsCreator)
                            newMenuDelete.SetBinding(MenuItem.CommandProperty, new Binding("ParentApp.CommandDashBoardDelete"));
                        else
                            newMenuDelete.SetBinding(MenuItem.CommandProperty, new Binding("ParentApp.CommandDashBoardRemove"));
                        newMenuDelete.CommandParameter = Dashboard;

                        newMenuShow.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/show_16x16.png") };
                        newMenuHide.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/hide_16x16.png") };
                        newMenuExport.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/icon_excel.gif") };
                        if (Dashboard.DashboardItem.IsCreator)
                            newMenuRename.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/rename_16x16.png") };
                        if (Dashboard.DashboardItem.IsCreator)
                            newMenuDelete.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/Delete.ico") };
                        else
                            newMenuDelete.Icon = new System.Windows.Controls.Image() { Source = McgWpfTools.GetBitmapImage($"{ImageResourcePath}/Remove.gif") };

                        newMenuItem.Items.Add(newMenuShow);
                        newMenuItem.Items.Add(newMenuHide);
                        newMenuItem.Items.Add(newMenuExport);
                        if (Dashboard.DashboardItem.IsCreator)
                            newMenuItem.Items.Add(newMenuRename);
                        newMenuItem.Items.Add(newMenuDelete);

                        MiSavedDashboard.Items.Add(newMenuItem);
                    }
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateMenuDashboardold(object sender = null, EventArgs e = null)
        {
            try
            {
                if (CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.DashboardList != null)
                {
                    MiSavedDashboard.Items.Clear();

                    foreach (var Dashboard in CurrentEcnEcoFollowUpViewModel.CurrentEcnEcoFollowUpDataContext.DashboardList)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void CheckIfMultiselection(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Events
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

        private void StartSubscribeToAllEcnEcoIsSelected()
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

        private void DgECN_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

    }
}
