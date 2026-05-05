using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.Services;
using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Models;
using MCG.Tools.EcnEcoFollowUp.View;
using System.Windows;
using System.Windows.Input;

namespace MCG.Tools.EcnEcoFollowUp.ViewModel
{
    public class EcnEcoFollowUpDashboardViewModel : ObservableObject, IEcnEcoFollowUpDashboardViewModel
    {

        #region [REGION] Properties not from interface
        private EFU_DashboardItem _DashboardItem;
        public EFU_DashboardItem DashboardItem
        {
            get { return this._DashboardItem; }
            set
            {
                if (this._DashboardItem != value)
                {
                    this._DashboardItem = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsTabCreated { get; set; } = false;
        #endregion

        #region [REGION] Properties not from interface
        public EcnEcoFollowUpViewModel ParentApp { get; set; }

        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        #endregion

        #region [REGION] Commands
        public ICommand CommandAddOneEcn { get => new RelayCommand(() => ExecuteAddOneEcn()); }
        public ICommand CommandAddEcnFromSearch { get => new RelayCommand(() => ExecuteAddEcnFromSearch()); }
        public ICommand CommandDeleteSelectedEcn { get => new RelayCommand(() => ExecuteDeleteSelectedEcn()); }
        public ICommand CommandExportXls { get => new RelayCommand(() => ExecuteExportXls()); }
        public ICommand CommandHideDashboard { get => new RelayCommand(() => ExecuteHideDashboard()); }
        public ICommand CommandRefreshDashboard { get => new RelayCommand(() => ExecuteRefreshDashboard()); }
        public ICommand CommandMenuItemOpenEcn { get => new RelayCommand(() => ExecuteMenuItemOpenEcn()); }
        public ICommand CommandMenuItemOpenEcnDocs { get => new RelayCommand(() => ExecuteMenuItemOpenEcnDocs()); }
        public ICommand CommandMenutItemSearchEcnWfTask { get => new RelayCommand(() => ExecuteMenutItemSearchEcnWfTask()); }
        public ICommand CommandMenutItemSearchEcoWfTask { get => new RelayCommand(() => ExecuteMenutItemSearchEcoWfTask()); }
        public ICommand CommandMenutItemRemoveEcnEco { get => new RelayCommand(() => ExecuteMenutItemRemoveEcnEco()); }
        public ICommand CommandCheckAllDashboard { get => new RelayCommand(() => ExecuteCheckUncheckAll(true)); }
        public ICommand CommandUncheckAllDashboard { get => new RelayCommand(() => ExecuteCheckUncheckAll(false)); }
        public ICommand CommandMenutItemAddEcnEcoToDashboard { get => new RelayCommand<EcnEcoFollowUpDashboardViewModel>((item) => ExecuteMenutItemAddEcnEcoToDashboard(item)); }
        #endregion

        #region [REGION] Events
        public event EventHandler DashboardHideEvent;
        public event EventHandler DashboardShowEvent;
        public void RaiseDashboardHideEvent()
        {
            try
            {
                DashboardHideEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        public void RaiseDashboardShowEvent()
        {
            try
            {
                DashboardShowEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Init
        public EcnEcoFollowUpDashboardViewModel(IMcgCommonLibWindowService mcgCommonLibWindowService)
        {
            _mcgCommonLibWindowService = mcgCommonLibWindowService;
        }

        public void SetEcnEcoFollowUpDashboardViewModelProperties(EFU_DashboardItem currentEFU_DashboardItem)
        {
            try
            {
                DashboardItem = currentEFU_DashboardItem;
                if (currentEFU_DashboardItem != null)
                    ParentApp = currentEFU_DashboardItem.ParentApp;
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void SetEcnEcoFollowUpDashboardViewModelProperties(EFU_DashboardItem currentEFU_DashboardItem, EcnEcoFollowUpViewModel parentApp)
        {
            try
            {
                DashboardItem = currentEFU_DashboardItem;
                ParentApp = parentApp;
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteAddOneEcn()
        {
            try
            {
                // McgWindowOkCancelListValue aMCGWindowOkCancel = new McgWindowOkCancelListValue();

                var windowReturn =  _mcgCommonLibWindowService.ShowDialogMcgWindowCancelListClass<EFU_EcnEcoCopyPaste>(McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco"), 150, 300);

                //McgWindowOkCancelListClassViewModel<EFU_EcnEcoCopyPaste> aMCGWindowOkCancel = new McgWindowOkCancelListClassViewModel<EFU_EcnEcoCopyPaste>() { WindowWidth=300, WindowTitle = McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco") };
                //aMCGWindowOkCancel.ShowDialog();

                if (windowReturn.DialogValue == MessageBoxResult.OK)
                {
                    var CurrentList = windowReturn.Values.GroupBy(p=>p.EcnEcoNumber).Select(g=>g.First()).ToList();
                       if (CurrentList != null && CurrentList.Count > 0)
                       {
                            foreach (var ecn in CurrentList)
                                if (ecn != null && ecn.EcnEcoNumber != null && ecn.EcnEcoNumber.Trim() != "")
                                    ParentApp.AddOneEcnEcoToDashboard(this, ecn);
                       }
                       else
                            MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgAddOneEcnEcoToDashboard"), McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddEcnFromSearch()
        {
            try
            {
                if (MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgAddEcnEcoToDashboardFromSearchAsk"), McgWpfTools.GetStringResource("EFU_TitleAddOneEcnEco"), MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    ParentApp.AddEcnEcoToDashboardFromSearch(this);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDeleteSelectedEcn()
        {
            try
            {
                if (DashboardItem.ListEcnEco != null)
                {
                    var ListToRemove = DashboardItem.ListEcnEco.Where((item) => item.IsSelected).ToList();

                    if (ListToRemove.Count > 0)
                        if (MessageBox.Show(McgWpfTools.GetStringResource("EFU_MsgRemoveSelectedEcnEco"), McgWpfTools.GetStringResource("EFU_WTitleRemoveEcnEco"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            foreach (var item in ListToRemove)
                                ParentApp.RemoveOneEcnEcoFromDashboard(this, item);
                }
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteExportXls()
        {
            try
            {
                ParentApp.ExecuteDashBoardExport(this);

            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteHideDashboard()
        {
            try
            {
                ParentApp.ExecuteDashBoardHide(this);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRefreshDashboard()
        {
            try
            {
                ParentApp.UpdateDashboardInformation(DashboardItem);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenuItemOpenEcn()
        {
            try
            {
                if (DashboardItem.SelectedEcnEco != null)
                    ParentApp.ExecuteMenuItemOpenEcn(DashboardItem.SelectedEcnEco.EcnEcoToShowEndUser);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteMenuItemOpenEcnDocs(EFU_EcnEcoToShowEndUser CurrentEcn = null)
        {
            try
            {
                if (DashboardItem.SelectedEcnEco != null)
                    ParentApp.ExecuteMenuItemOpenEcnDocs(DashboardItem.SelectedEcnEco.EcnEcoToShowEndUser);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenutItemSearchEcnWfTask()
        {
            try
            {
                if (DashboardItem.SelectedEcnEco != null)
                    ParentApp.ExecuteMenutItemSearchEcnWfTask(DashboardItem.SelectedEcnEco.EcnEcoToShowEndUser);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenutItemSearchEcoWfTask()
        {
            try
            {
                if (DashboardItem.SelectedEcnEco != null)
                    ParentApp.ExecuteMenutItemSearchEcoWfTask(DashboardItem.SelectedEcnEco.EcnEcoToShowEndUser);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenutItemRemoveEcnEco()
        {
            try
            {
                if (DashboardItem.SelectedEcnEco != null)
                    ParentApp.RemoveOneEcnEcoFromDashboard(this, DashboardItem.SelectedEcnEco);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckUncheckAll(bool IsChecked)
        {
            try
            {

                foreach (var item in DashboardItem.ListEcnEco)
                    item.IsSelected = IsChecked;
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenutItemAddEcnEcoToDashboard(EcnEcoFollowUpDashboardViewModel Dashboard = null)
        {
            try
            {
                if (DashboardItem.SelectedEcnEco != null && Dashboard != null)
                    ParentApp.AddOneEcnEcoToDashboard(Dashboard, DashboardItem.SelectedEcnEco.EcnEcoToShowEndUser.EcnEcoFollowUp.Ecn_Number);
            }
            catch (Exception ex)
            {
                EcnEcoFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        public override string ToString()
        {
            try
            {
                return DashboardItem.Name;
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }
    }
}
