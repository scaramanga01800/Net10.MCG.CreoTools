using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.DataBaseAccess.Models.SapHupDbResult;
using MCG.CommonLib.Models.Email;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.Models.SAP;
using MCG.CommonLib.SapTools.Services;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Services.Interfaces;
using MCG.CommonLib.WpfComponent.View;
using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.CommonLib.WpfComponent.ViewModel.Mail;
using MCG.Tools.PurchaseOrderFollowUp.Configuration;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.Interfaces;
using MCG.Tools.PurchaseOrderFollowUp.View;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderFollowUpViewModel : ObservableObject, IPurchaseOrderFollowUpViewModel //, IMcgToolApp
    {
        #region [REGION] Properties from Interface
        private PurchaseOrderFollowUpDataContext _CurrentDataContext;
        public PurchaseOrderFollowUpDataContext CurrentDataContext
        {
            get { return _CurrentDataContext; }
            set
            {
                if (this._CurrentDataContext != value)
                {
                    this._CurrentDataContext = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        private ISapPurchasingService _sapPurchasingService;
        private ISapHupService _sapHupService;
        private IUserAuthorizationService _userAuthorizationService;
        private IPurchaseOrderService _purchaseOrderService;
        private IOracleMiscTools _oracleMiscTools;
        private IXmlSerializeTools _xmlSerializeTools;
        private IPurchaseOrderFollowWindowService _purchaseOrderFollowWindowService;
        private IMcgCommonLibWindowService _mcgCommonLibWindowService;

        public bool IsBatchMode { get; set; } = false;

        private string MainAppFolder { get; set; }
        private string CryptedPassWordRO { get; set; } = null;
        private string CryptedLoginRO { get; set; } = null;
        private string CryptedPassWordUpdate { get; set; } = null;
        private string CryptedLoginUpdate { get; set; } = null;
        private PurchaseOrderFollowUpConfiguration CurrentPurchaseOrderFollowUpConfiguration { get; set; }
        private UserPrincipal LoggedUser { get; set; } = UserPrincipal.Current;
        private string SapHupConnectionString { get; set; }
        private string SearchType { get; set; }
        private string PreviousSearchType { get; set; } = "";
        private bool IsSearchDbDone { get; set; } = false;
        private bool IsSearchDbMaterialGroupDone { get; set; } = false;
        private string CurrentBtRequestType { get; set; }
        private int QuesionLevel { get; set; }
        public List<McgColumnData> ListFilters { get; set; } = new List<McgColumnData>();
        public List<McgColumnData> ListMyFilters { get; set; } = new List<McgColumnData>();
        private List<Color> usedColors { get; set; } = new List<Color>();
        #endregion

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

        public event EventHandler MaxRowSearchedEvent;
        public void RaiseMaxRowSearchedEvent()
        {
            try
            {
                MaxRowSearchedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler StartRequestTypeEvent;
        public void RaiseStartRequestTypeEvent()
        {
            try
            {
                StartRequestTypeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler ChangeRequestTypeQuestionEvent;
        public void RaiseChangeRequestTypeQuestionEvent()
        {
            try
            {
                ChangeRequestTypeQuestionEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler EndRequestTypeEvent;
        public void RaiseEndRequestTypeEvent()
        {
            try
            {
                EndRequestTypeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UpdateRequestTypeEvent;
        public void RaiseUpdateRequestTypeEvent()
        {
            try
            {
                UpdateRequestTypeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UpdateNotAllowedEvent;

        public void RaiseUpdateNotAllowedEvent()
        {
            try
            {
                UpdateNotAllowedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public void PurgeStartRequestTypeEvent()
        {
            try
            {
                if (StartRequestTypeEvent != null)
                {
                    foreach (Delegate d in StartRequestTypeEvent.GetInvocationList())
                    {
                        StartRequestTypeEvent -= (EventHandler)d;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public void PurgeChangeRequestTypeQuestionEvent()
        {
            try
            {
                if (ChangeRequestTypeQuestionEvent != null)
                {
                    foreach (Delegate d in ChangeRequestTypeQuestionEvent.GetInvocationList())
                    {
                        ChangeRequestTypeQuestionEvent -= (EventHandler)d;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public void PurgeEndRequestTypeEvent()
        {
            try
            {
                if (EndRequestTypeEvent != null)
                {
                    foreach (Delegate d in EndRequestTypeEvent.GetInvocationList())
                    {
                        EndRequestTypeEvent -= (EventHandler)d;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public void PurgeUpdateRequestTypeEvent()
        {
            try
            {
                if (UpdateRequestTypeEvent != null)
                {
                    foreach (Delegate d in UpdateRequestTypeEvent.GetInvocationList())
                    {
                        UpdateRequestTypeEvent -= (EventHandler)d;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public void PurgeUpdateNotAllowedEvent()
        {
            try
            {
                if (UpdateNotAllowedEvent != null)
                {
                    foreach (Delegate d in UpdateNotAllowedEvent.GetInvocationList())
                    {
                        UpdateNotAllowedEvent -= (EventHandler)d;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Commands
        public ICommand CommandOpenHelp { get => new RelayCommand<string>((res) => ExecuteOpenHelp(res)); }
        public ICommand CommandCreatePurchaseOrder { get => new RelayCommand(() => ExecuteCreatePurchaseOrder()); }
        public ICommand CommandUpdatePurchaseOrder { get => new RelayCommand(() => ExecuteUpdatePurchaseOrder()); }
        public ICommand CommandCreateItem { get => new RelayCommand(() => ExecuteCreateItem()); }
        public ICommand CommandRemoveItem { get => new RelayCommand(() => ExecuteRemoveItem()); }
        public ICommand CommandSaveRequest { get => new RelayCommand<bool>((obj) => ExecuteSaveRequest(obj)); }
        public ICommand CommandDrop { get => new RelayCommand<DragEventArgs>((obj) => ExecuteDrop(obj)); }
        public ICommand CommandDropVendor { get => new RelayCommand<DragEventArgs>((obj) => ExecuteDropVendor(obj)); }
        public ICommand CommandRemoveAttachement { get => new RelayCommand(() => ExecuteRemoveAttachement()); }
        public ICommand CommandRemoveAttachementVendor { get => new RelayCommand(() => ExecuteRemoveAttachementVendor()); }
        public ICommand CommandOpenAttachment { get => new RelayCommand(() => ExecuteOpenAttachment()); }
        public ICommand CommandOpenAttachmentVendor { get => new RelayCommand(() => ExecuteOpenAttachmentVendor()); }
        public ICommand CommandSendRequest { get => new RelayCommand<bool>((obj) => ExecuteSendRequest(obj)); }
        public ICommand CommandSetSearchOrder { get => new RelayCommand(() => ExecuteSetSearchOrder()); }
        public ICommand CommandSetSearchVendor { get => new RelayCommand(() => ExecuteSetSearchVendor()); }
        public ICommand CommandSearchItem { get => new RelayCommand(() => ExecuteSetSearchItem()); }
        public ICommand CommandSelectItem { get => new RelayCommand(() => ExecuteSelectItem()); }
        public ICommand CommandSelectVendor { get => new RelayCommand(() => ExecuteSelectVendor()); }
        public ICommand CommandSearchRquestType { get => new RelayCommand<string>((obj) => ExecuteSearchRquestType(obj)); }
        public ICommand CommandAskNewVendor { get => new RelayCommand(() => ExecuteAskNewVendor()); }
        public ICommand CommandAskUpdateVendor { get => new RelayCommand<bool>((obj) => ExecuteAskUpdateVendor(obj)); }
        public ICommand CommandAskNewInternalOrder { get => new RelayCommand(() => ExecuteAskNewInternalOrder()); }
        public ICommand CommandAskExtendPart { get => new RelayCommand(() => ExecuteAskExtendPart()); }
        public ICommand CommandSendVendorRequest { get => new RelayCommand(() => ExecuteSendVendorRequest()); }
        public ICommand CommandShowVendor { get => new RelayCommand<bool>((obj) => ExecuteShowVendor(obj)); }
        public ICommand CommandAddRequestFromSapPr { get => new RelayCommand(() => ExecuteAddRequestFromSapPr()); }
        public ICommand CommandAddRequestFromSapPo { get => new RelayCommand(() => ExecuteAddRequestFromSapPo()); }
        public ICommand CommandAddRequestFromSapPoDate { get => new RelayCommand<PurchaseOrderRequest>((obj) => ExecuteAddRequestFromSapPo(obj)); }
        public ICommand CommandAddInternalOrder { get => new RelayCommand(() => ExecuteAddInternalOrder()); }
        public ICommand CommandUpdateInternalOrder { get => new RelayCommand(() => ExecuteUpdateInternalOrder()); }
        public ICommand CommandDeleteInternalOrder { get => new RelayCommand(() => ExecuteDeleteInternalOrder()); }
        public ICommand CommandAddRequestFromSapPoDates { get => new RelayCommand(() => ExecuteAddRequestFromSapPoDates()); }
        public ICommand CommandDeleteRequest { get => new RelayCommand(() => ExecuteDeleteRequest()); }
        public ICommand CommandUpdatePrFromSapHub { get => new RelayCommand(() => ExecuteUpdatePrFromSapHub()); }
        public ICommand CommandUpdatePoFromSapHub { get => new RelayCommand(() => ExecuteUpdatePoFromSapHub()); }
        public ICommand CommandCopyPurchaseRequest { get => new RelayCommand(() => { McgWpfTools.CopyTextClipboard(CurrentDataContext.SelectedRequest?.SapPurchaseRequest); }); }
        public ICommand CommandCopyPurchaseOrder { get => new RelayCommand(() => { McgWpfTools.CopyTextClipboard(CurrentDataContext.SelectedRequest?.SapPurchaseOrder); }); }
        public ICommand CommandOpenPurchaseRequest { get => new RelayCommand(() => ExecuteOpenPurchaseRequest()); }
        public ICommand CommandOpenPurchaseOrder { get => new RelayCommand(() => ExecuteOpenPurchaseOrder()); }
        public ICommand CommandOpenResa { get => new RelayCommand(() => ExecuteOpenResa()); }
        public ICommand CommandStartUpdateAllRequestFromSap { get => new RelayCommand<bool>((obj) => ExecuteStartUpdateAllRequestFromSap(obj)); }
        public ICommand CommandCreateSapRequest { get => new RelayCommand(() => ExecuteCreateSapRequest()); }
        public ICommand CommandShowSapStock { get => new RelayCommand<bool>((obj) => ExecuteShowSapStock(obj)); }
        public ICommand CommandConvertSapRequest { get => new RelayCommand(() => ExecuteConvertSapRequest()); }
        public ICommand CommandDownloadAttachment { get => new RelayCommand(() => ExecuteDownloadAttachment()); }
        public ICommand CommandSearchColumnKeyWord { get => new RelayCommand<McgColumnData>((data) => ExecuteSearchColumnKeyWord(data)); }
        public ICommand CommandOpenPurchaseOrderPdf { get => new RelayCommand(() => ExecuteOpenPurchaseOrderPdf()); }
        public ICommand CommandCopyVendorNumber { get => new RelayCommand(() => { McgWpfTools.CopyTextClipboard(CurrentDataContext.SelectedRequest?.Vendor?.Number); }); }
        public ICommand CommandReceiptPurchaseOrder { get => new RelayCommand(() => ExecuteReceiptPurchaseOrder()); }
        public ICommand CommandSearchDuplicateRequest { get => new RelayCommand(() => ExecuteSearchDuplicateRequest()); }
        public ICommand CommandAdminUpdute { get => new RelayCommand(() => ExecuteAdminUpdate()); }
        public ICommand CommandCopyIONumber { get => new RelayCommand(() => { McgWpfTools.CopyTextClipboard(CurrentDataContext.SelectedInternalOrder?.Number); }); }
        public ICommand CommandCheckExtendPart { get => new RelayCommand(() => ExecuteCheckExtendPart()); }
        #endregion

        #region [REGION] Init
        //public PurchaseOrderFollowUpViewModel(bool IsBatchMode = false)
        public PurchaseOrderFollowUpViewModel(ISapPurchasingService sapPurchasingService,
                                              IXmlSerializeTools xmlSerializeTools,
                                              ISapHupService sapHupService,
                                              IUserAuthorizationService userAuthorizationService,
                                              IPurchaseOrderService purchaseOrderService,
                                              IOracleMiscTools oracleMiscTools,
                                              IPurchaseOrderFollowWindowService purchaseOrderFollowWindowService,
                                              IMcgCommonLibWindowService mcgCommonLibWindowService)
        {
            try
            {
                _sapPurchasingService = sapPurchasingService;
                _xmlSerializeTools = xmlSerializeTools;
                _userAuthorizationService = userAuthorizationService;
                _purchaseOrderService = purchaseOrderService;
                _oracleMiscTools = oracleMiscTools;
                _sapHupService = sapHupService;
                _purchaseOrderFollowWindowService = purchaseOrderFollowWindowService;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;

                CurrentDataContext = new PurchaseOrderFollowUpDataContext();

                MainAppFolder = Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (string.IsNullOrEmpty(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                CurrentPurchaseOrderFollowUpConfiguration = _xmlSerializeTools.GetDeserializedXml<PurchaseOrderFollowUpConfiguration>(Path.Combine(MainAppFolder, CommonLibConstants.ResourcesFolder, PurchaseOrderFollowUpConstants.ConfigurationFile));

                if (CurrentPurchaseOrderFollowUpConfiguration != null)
                {
                    foreach (var item in CurrentPurchaseOrderFollowUpConfiguration.ListCostCenter)
                        CurrentDataContext.ListCostCenter.Add(item);
                    foreach (var item in CurrentPurchaseOrderFollowUpConfiguration.ListOrderType)
                        CurrentDataContext.ListPurchaseType.Add(item);
                    foreach (var item in CurrentPurchaseOrderFollowUpConfiguration.ListDeliveryLocation)
                        CurrentDataContext.ListDeliveryLocation.Add(item);
                    CurrentDataContext.ListPlantVendor = CurrentPurchaseOrderFollowUpConfiguration.ListPlantVendor;
                }

                SearchRequestWithoutItemAttachment(CurrentDataContext.ChartPoCreatedAfter.Value.ToDateTime(new TimeOnly(0, 0)), CurrentDataContext.ChartPoCreatedBefore.Value.ToDateTime(new TimeOnly(0, 0)));

                CurrentDataContext.UpdateDateFilterEvent += (e, i) => SearchRequestWithoutItemAttachment(CurrentDataContext.ChartPoCreatedAfter.Value.ToDateTime(new TimeOnly(0, 0)), CurrentDataContext.ChartPoCreatedBefore.Value.ToDateTime(new TimeOnly(0, 0)));

                SearchAllDefaultListFromAllRequest();

                UpdateRequestTypeEvent += SearchRquestType_UpdateRequestTypeEvent;
                CurrentDataContext.UpdateFilterEvent += UpdateRequestList;
                CurrentDataContext.UpdateChartFilterEvent += UpdateCharts;
                CurrentDataContext.UpdateDateFilterEvent += UpdateRequestListFromDate;
                CurrentDataContext.IsAllCostCenterSelectedEvent += CurrentDataContext_IsAllCostCenterSelectedEvent;

                foreach (var item in CurrentDataContext.ListCostCenter)
                    item.IsSelectedEvent += UpdateCharts;

                CheckUserAuthorization();

                UpdateRequestList();

                if (!IsBatchMode)
                    UpdateCharts();
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void CurrentDataContext_IsAllCostCenterSelectedEvent(object sender, EventArgs e)
        {
            try
            {
                foreach (var item in CurrentDataContext.ListCostCenter)
                {
                    item.IsSelectedEvent -= UpdateCharts;
                    item.IsSelected = CurrentDataContext.IsAllCostCenterSelected;
                    item.IsSelectedEvent += UpdateCharts;
                }
                UpdateCharts();
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void InitApp()
        {

        }

        public void CheckUserAuthorization()
        {
            try
            {
                string CurrentUser = LoggedUser.SamAccountName.ToUpper();

                CurrentDataContext.IsRoleAdmin = _userAuthorizationService.GetIsRoleAdmin(CurrentUser);
                CurrentDataContext.IsRoleSuperviser = _userAuthorizationService.GetIsRoleSuperviser(CurrentUser);
                CurrentDataContext.IsRoleSapCreator = _userAuthorizationService.GetIsRoleSapCreator(CurrentUser);
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateRequestList(object sender = null, EventArgs e = null)
        {
            try
            {
                string AccountID = LoggedUser.SamAccountName.ToUpper();
                //AccountID = "CP67327";

                CurrentDataContext.ListMyRequest.Clear();
                CurrentDataContext.ListShownMyRequest.Clear();
                CurrentDataContext.ListRequest.Clear();
                CurrentDataContext.ListShownRequest.Clear();
                List<PurchaseOrderRequest> TempList = CurrentDataContext.ListAllRequest.Where(item => item.RequestedBy?.ToUpper() == AccountID || item.SapCreatedBy?.ToUpper() == AccountID || item.CreatedBy.ToUpper() == AccountID).ToList();
                List<PurchaseOrderRequest> TempListAll = new List<PurchaseOrderRequest>();
                TempListAll.AddRange(CurrentDataContext.ListAllRequest);

                if (!CurrentDataContext.StatusClosedSelected)
                {
                    TempList.RemoveAll(item => item.Status == PurchaseOrderStatus.CLOSED);
                    TempListAll.RemoveAll(item => item.Status == PurchaseOrderStatus.CLOSED);
                }
                if (!CurrentDataContext.StatusInvoiceReceiptSelected)
                {
                    TempList.RemoveAll(item => item.Status == PurchaseOrderStatus.INVOICE_RECEIPT || item.Status == PurchaseOrderStatus.PARTIAL_INVOICE_RECEIPT);
                    TempListAll.RemoveAll(item => item.Status == PurchaseOrderStatus.INVOICE_RECEIPT || item.Status == PurchaseOrderStatus.PARTIAL_INVOICE_RECEIPT);
                }
                if (!CurrentDataContext.StatusGoodsReceiptSelected)
                {
                    TempList.RemoveAll(item => item.Status == PurchaseOrderStatus.GOODS_RECEIPT || item.Status == PurchaseOrderStatus.PARTIAL_GOODS_RECEIPT);
                    TempListAll.RemoveAll(item => item.Status == PurchaseOrderStatus.GOODS_RECEIPT || item.Status == PurchaseOrderStatus.PARTIAL_GOODS_RECEIPT);
                }
                if (!CurrentDataContext.StatusCreatedSelected)
                {
                    TempList.RemoveAll(item => item.Status == PurchaseOrderStatus.CREATED
                                    || item.Status == PurchaseOrderStatus.UNDER_REVIEW
                                    || item.Status == PurchaseOrderStatus.PR_CREATED
                                    || item.Status == PurchaseOrderStatus.PO_CREATED
                                    || item.Status == PurchaseOrderStatus.PR_APPROVED
                                    || item.Status == PurchaseOrderStatus.PO_APPROVED);
                    TempListAll.RemoveAll(item => item.Status == PurchaseOrderStatus.CREATED
                                    || item.Status == PurchaseOrderStatus.UNDER_REVIEW
                                    || item.Status == PurchaseOrderStatus.PR_CREATED
                                    || item.Status == PurchaseOrderStatus.PO_CREATED
                                    || item.Status == PurchaseOrderStatus.PR_APPROVED
                                    || item.Status == PurchaseOrderStatus.PO_APPROVED);
                }
                if (!CurrentDataContext.StatusSentSelected)
                {
                    TempList.RemoveAll(item => item.Status == PurchaseOrderStatus.SENT);
                    TempListAll.RemoveAll(item => item.Status == PurchaseOrderStatus.SENT);
                }
                if (!CurrentDataContext.StatusNewSelected)
                {
                    TempList.RemoveAll(item => item.Status == PurchaseOrderStatus.NEW);
                    TempListAll.RemoveAll(item => item.Status == PurchaseOrderStatus.NEW);
                }

                foreach (var req in TempList)
                {
                    CurrentDataContext.ListMyRequest.Add(req);
                    CurrentDataContext.ListShownMyRequest.Add(req);
                }
                foreach (var req in TempListAll)
                {
                    CurrentDataContext.ListRequest.Add(req);
                    CurrentDataContext.ListShownRequest.Add(req);
                }

                ExecuteSearchColumnKeyWord();
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateRequestListFromDate(object sender = null, EventArgs e = null)
        {
            try
            {
                SearchRequestWithoutItemAttachment(CurrentDataContext.ChartPoCreatedAfter.Value.ToDateTime(new TimeOnly(0, 0)), CurrentDataContext.ChartPoCreatedBefore.Value.ToDateTime(new TimeOnly(0, 0)));
                UpdateRequestList();
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        void UpdateCharts(object sender = null, EventArgs e = null)
        {
            try
            {
                // update piecharts
                List<PurchaseOrderRequest> TempListAll = new List<PurchaseOrderRequest>();
                TempListAll.AddRange(CurrentDataContext.ListAllRequest.Where(item => item.CreatedOn >= CurrentDataContext.ChartPoCreatedAfter.Value
                                                                                && item.CreatedOn <= CurrentDataContext.ChartPoCreatedBefore.Value));

                // update Purchase Request with status
                if (!CurrentDataContext.ChartStatusClosedSelected)
                {
                    TempListAll.RemoveAll(item => item.Status == PurchaseOrderStatus.CLOSED);
                }
                if (!CurrentDataContext.ChartStatusInvoiceReceiptSelected)
                {
                    TempListAll.RemoveAll(item => item.Status == PurchaseOrderStatus.INVOICE_RECEIPT || item.Status == PurchaseOrderStatus.PARTIAL_INVOICE_RECEIPT);
                }
                if (!CurrentDataContext.ChartStatusGoodsReceiptSelected)
                {
                    TempListAll.RemoveAll(item => item.Status == PurchaseOrderStatus.GOODS_RECEIPT || item.Status == PurchaseOrderStatus.PARTIAL_GOODS_RECEIPT);
                }
                if (!CurrentDataContext.ChartStatusCreatedSelected)
                {
                    TempListAll.RemoveAll(item => item.Status == PurchaseOrderStatus.CREATED
                                    || item.Status == PurchaseOrderStatus.UNDER_REVIEW
                                    || item.Status == PurchaseOrderStatus.PR_CREATED
                                    || item.Status == PurchaseOrderStatus.PO_CREATED
                                    || item.Status == PurchaseOrderStatus.PR_APPROVED
                                    || item.Status == PurchaseOrderStatus.PO_APPROVED);
                }
                if (!CurrentDataContext.ChartStatusSentSelected)
                {
                    TempListAll.RemoveAll(item => item.Status == PurchaseOrderStatus.SENT);
                }
                if (!CurrentDataContext.ChartStatusNewSelected)
                {
                    TempListAll.RemoveAll(item => item.Status == PurchaseOrderStatus.NEW);
                }

                // update Purchase Request with selected cost center
                foreach (var costCenter in CurrentDataContext.ListCostCenter.Where(costCenter => !costCenter.IsSelected))
                {
                    TempListAll.RemoveAll(item => item.CostCenter?.Number == costCenter.Number);
                }

                TempListAll.RemoveAll(item => item.CostCenter == null);

                TempListAll.RemoveAll(item => !CurrentDataContext.ListCostCenter.Select(cost => cost.Number).Contains(item.CostCenter.Number));


                //string hexColor = "#FF3456";
                //SolidColorBrush solidColorBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor);

                // Chart for all Request (count)
                CurrentDataContext.AllPurchasePieSeriesNumber = new SeriesCollection()
                {
                       new PieSeries
                        {
                            Title = McgWpfTools.GetStringResource("POF_CheckBoxStatusNew"),
                            Values = new ChartValues<double> { TempListAll.Where(item => item.Status == PurchaseOrderStatus.NEW).Count() },
                            DataLabels = true,
                            Fill = Brushes.LightGray
                        },
                       new PieSeries
                        {
                            Title = McgWpfTools.GetStringResource("POF_CheckBoxStatusSent"),
                            Values = new ChartValues<double> { TempListAll.Where(item => item.Status == PurchaseOrderStatus.SENT).Count() },
                            DataLabels = true,
                            Fill = Brushes.Gray
                        },
                       new PieSeries
                        {
                            Title = McgWpfTools.GetStringResource("POF_CheckBoxStatusCreated"),
                            Values = new ChartValues<double> { TempListAll.Where(item => item.Status == PurchaseOrderStatus.CREATED
                                                                                        || item.Status == PurchaseOrderStatus.UNDER_REVIEW
                                                                                        || item.Status == PurchaseOrderStatus.PR_CREATED
                                                                                        || item.Status == PurchaseOrderStatus.PO_CREATED
                                                                                        || item.Status == PurchaseOrderStatus.PR_APPROVED
                                                                                        || item.Status == PurchaseOrderStatus.PO_APPROVED).Count() },
                            DataLabels = true,
                            Fill = Brushes.LightGreen
                        },
                       new PieSeries
                        {
                            Title = McgWpfTools.GetStringResource("POF_CheckBoxStatusGoodsReceipt"),
                            Values = new ChartValues<double> { TempListAll.Where(item => item.Status == PurchaseOrderStatus.GOODS_RECEIPT || item.Status == PurchaseOrderStatus.PARTIAL_GOODS_RECEIPT).Count() },
                            DataLabels = true,
                            Fill = Brushes.SteelBlue
                        },
                       new PieSeries
                        {
                            Title = McgWpfTools.GetStringResource("POF_CheckBoxStatusInvoiceReceipt"),
                            Values = new ChartValues<double> { TempListAll.Where(item => item.Status == PurchaseOrderStatus.INVOICE_RECEIPT || item.Status == PurchaseOrderStatus.PARTIAL_INVOICE_RECEIPT).Count() },
                            DataLabels = true,
                            Fill = Brushes.PaleVioletRed
                        },
                       new PieSeries
                        {
                            Title = McgWpfTools.GetStringResource("POF_CheckBoxStatusClosed"),
                            Values = new ChartValues<double> { TempListAll.Where(item => item.Status == PurchaseOrderStatus.CLOSED).Count() },
                            DataLabels = true,
                            Fill = Brushes.Violet
                        },

                };


                // Chart for all Cost Request 

                double TotalInvoice = TempListAll.Select(item => item.Total_Invoice).Sum();
                double TotalGoodR = TempListAll.Select(item => item.Total_Goods).Sum();
                double TotalOrdered = TempListAll.Select(item => item.Total_Ordered).Sum();

                CurrentDataContext.AllPurchasePieSeriesCost = new SeriesCollection()
                {
                new PieSeries
                        {
                            Title = McgWpfTools.GetStringResource("POF_ChartLabelOrdered"),
                            Values = new ChartValues<double> { TotalOrdered-TotalGoodR },
                            DataLabels = true
                        },
                new PieSeries
                        {
                            Title = McgWpfTools.GetStringResource("POF_ChartLabelGoodR"),
                            Values = new ChartValues<double> { TotalGoodR-TotalInvoice },
                            DataLabels = true
                        },
                new PieSeries
                        {
                            Title = McgWpfTools.GetStringResource("POF_ChartLabelInvoiced"),
                            Values = new ChartValues<double> { TotalInvoice },
                            DataLabels = true
                        },
                };

                CurrentDataContext.AllPurchaseStackColSeriesNumber = new SeriesCollection()
                {
                new StackedColumnSeries
                        {
                            Values = new ChartValues<double> { TotalOrdered-TotalGoodR },
                            StackMode = StackMode.Values,
                            DataLabels = true,
                            Title= McgWpfTools.GetStringResource("POF_ChartLabelOrdered"),
                            Fill = Brushes.Orange
                        },
                                new StackedColumnSeries
                        {
                            Values = new ChartValues<double> { TotalGoodR-TotalInvoice  },
                            StackMode = StackMode.Values,
                            DataLabels = true,
                            Title= McgWpfTools.GetStringResource("POF_ChartLabelGoodR"),
                            Fill = Brushes.Blue
                        },
                                new StackedColumnSeries
                        {
                            Values = new ChartValues<double> { TotalInvoice  },
                            StackMode = StackMode.Values,
                            DataLabels = true,
                            Title= McgWpfTools.GetStringResource("POF_ChartLabelInvoiced"),
                            Fill = Brushes.Green
                        }
                };

                CurrentDataContext.PieSeriesCostFormatter = value => (value / 1000).ToString("0.00") + " k€";


                // Chart for all provider by cost %

                List<PurchaseOrderVendor> ListVendor = GetListForChartVendor(TempListAll, CurrentDataContext.MinVendorRatio);
                usedColors = new List<Color>();

                double threshold = 441 / Math.Sqrt(ListVendor.Count + 10);

                CurrentDataContext.AllPurchasePieSeriesVendorCost = new SeriesCollection();
                foreach (PurchaseOrderVendor vendor in ListVendor)
                {
                    Color randomColor = GenerateRandomColor(threshold);
                    SolidColorBrush solidColorBrush = new SolidColorBrush(randomColor);

                    CurrentDataContext.AllPurchasePieSeriesVendorCost.Add(new PieSeries
                    {
                        Title = vendor.Description,
                        Values = new ChartValues<double> { vendor.TotalCostOrdered },
                        DataLabels = true,
                        Fill = solidColorBrush,
                        ToolTip = vendor.TotalCostOrdered,
                        LabelPoint = p => ""
                    });
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private List<PurchaseOrderVendor> GetListForChartVendor(List<PurchaseOrderRequest> ListRequest, double MinPourcentage)
        {
            try
            {
                List<PurchaseOrderVendor> ListVendor = new List<PurchaseOrderVendor>();
                List<PurchaseOrderVendor> TempListVendor = new List<PurchaseOrderVendor>();

                PurchaseOrderVendor CurrentVendor;

                foreach (PurchaseOrderRequest request in ListRequest.Where(item => item.Vendor != null && item.Vendor.Number != null))
                {
                    CurrentVendor = TempListVendor.FirstOrDefault(item => item.Number == request.Vendor.Number);
                    if (CurrentVendor == null)
                    {
                        CurrentVendor = new PurchaseOrderVendor()
                        {
                            Number = request.Vendor.Number,
                            Description = request.Vendor.Description,
                            DescriptionShort = request.Vendor.DescriptionShort,
                            TotalCostOrdered = request.Total_Ordered
                        };
                        TempListVendor.Add(CurrentVendor);
                    }
                    else
                    {
                        CurrentVendor.TotalCostOrdered += request.Total_Ordered;
                    }
                }

                double TotalCost = ListRequest.Where(item => item.Vendor != null && item.Vendor.Number != null).Sum(item => item.Total_Ordered);

                PurchaseOrderVendor OtherVendor = new PurchaseOrderVendor()
                {
                    Description = "Other",
                    DescriptionShort = "Other",
                    Number = "0000000000",
                    TotalCostOrderedPourcentage = 0,
                    TotalCostOrdered = 0
                };


                foreach (PurchaseOrderVendor vendor in TempListVendor)
                {
                    if (vendor.TotalCostOrdered > 0)
                        vendor.TotalCostOrderedPourcentage = vendor.TotalCostOrdered * 100 / TotalCost;
                    else
                        vendor.TotalCostOrderedPourcentage = 0;

                    if (vendor.TotalCostOrderedPourcentage >= MinPourcentage)
                        ListVendor.Add(vendor);
                    else
                    {
                        OtherVendor.TotalCostOrderedPourcentage += vendor.TotalCostOrderedPourcentage;
                        OtherVendor.TotalCostOrdered += vendor.TotalCostOrdered;
                    }
                }

                if (OtherVendor.TotalCostOrderedPourcentage > 0)
                    ListVendor.Add(OtherVendor);

                return ListVendor.OrderByDescending(item => item.TotalCostOrdered).ToList();
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public Color GenerateRandomColor(double Threshold)
        {
            try
            {
                Random random = new Random();

                while (true)
                {
                    // Générer une couleur aléatoire
                    Color randomColor = Color.FromArgb(255, (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));

                    // Vérifier la distance avec les couleurs déjà utilisées
                    bool tooClose = false;
                    foreach (Color usedColor in usedColors)
                    {
                        if (ColorDistance(randomColor, usedColor) < Threshold) // Ajustez ce seuil selon vos besoins
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    // Si la couleur est suffisamment éloignée, l'utiliser
                    if (!tooClose)
                    {
                        usedColors.Add(randomColor);
                        return randomColor;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private double ColorDistance(Color color1, Color color2)
        {
            try
            {
                int dr = color1.R - color2.R;
                int dg = color1.G - color2.G;
                int db = color1.B - color2.B;

                return Math.Sqrt(dr * dr + dg * dg + db * db);
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteOpenHelp(string ResourceHelp)
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource(ResourceHelp));
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreatePurchaseOrder(PurchaseOrderRequest CurrentRequest = null)
        {
            try
            {

                PurgeStartRequestTypeEvent();
                PurgeChangeRequestTypeQuestionEvent();
                PurgeEndRequestTypeEvent();
                //PurgeUpdateRequestTypeEvent();

                if (!IsSearchDbDone)
                    SearchAllDefaultListFromAllRequest();

                if (CurrentRequest == null)
                {
                    CurrentDataContext.CurrentRequest = new PurchaseOrderRequest()
                    {
                        CreatedBy = LoggedUser.SamAccountName,
                        CreatedOn = DateOnly.FromDateTime(DateTime.Now),
                        Status = PurchaseOrderStatus.NEW,
                        InternalOrder = null,
                        CostCenter = CurrentDataContext.ListCostCenter.FirstOrDefault(),
                        Vendor = null,
                        RequestedBy = LoggedUser.SamAccountName,
                        CanBeClosedWithoutSaving = false
                    };
                    CurrentDataContext.CurrentRequest.RequestType = CurrentDataContext.ListPurchaseType.FirstOrDefault();
                    CurrentDataContext.CurrentRequest.WindowTitle = $"{McgWpfTools.GetStringResource("POF_WindowTitleCreateRequest")}";
                }
                else
                {
                    CurrentRequest.WindowTitle = $"{McgWpfTools.GetStringResource("POF_WindowTitleCreateRequest")} : {CurrentRequest.ID}";
                    CurrentDataContext.CurrentRequest = CurrentRequest;
                }

                CurrentDataContext.CurrentRequest.CheckUpdateRight(LoggedUser.SamAccountName, CurrentDataContext.IsRoleAdmin);

                CurrentDataContext.CurrentRequest.RequestTypeChangeEvent += (o, e) => { CurrentDataContext.UpdateDienNlag(CurrentDataContext.CurrentRequest.RequestType); };

                _purchaseOrderFollowWindowService.ShowDialogPurchaseOrderFollowCreateUpdateView(this);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdatePurchaseOrder(bool isAdminUpdate = false)
        {
            try
            {
                if (CurrentDataContext.SelectedRequest != null)
                {
                    PurgeUpdateNotAllowedEvent();
                    PurgeStartRequestTypeEvent();
                    PurgeChangeRequestTypeQuestionEvent();
                    PurgeEndRequestTypeEvent();
                    //PurgeUpdateRequestTypeEvent();

                    if (!IsSearchDbDone) // && (CurrentDataContext.SelectedRequest.Status == PurchaseOrderStatus.NEW || CurrentDataContext.SelectedRequest.Status == PurchaseOrderStatus.SENT))
                        SearchAllDefaultListFromAllRequest();

                    UpdateRequestItemAttachment(CurrentDataContext.SelectedRequest);

                    CurrentDataContext.CurrentRequest = CurrentDataContext.SelectedRequest.Clone();
                    CurrentDataContext.CurrentRequest.IsAdminUpdate = isAdminUpdate;

                    CurrentDataContext.CurrentRequest.RequestTypeChangeEvent += (o, e) => { CurrentDataContext.UpdateDienNlag(CurrentDataContext.CurrentRequest.RequestType); };

                    CurrentDataContext.CurrentRequest.Vendor = CurrentDataContext.ListVendor.FirstOrDefault(item => item.IdVendor == CurrentDataContext.CurrentRequest.Vendor?.IdVendor);
                    CurrentDataContext.CurrentRequest.InternalOrder = CurrentDataContext.ListInternalOrder.FirstOrDefault(item => item?.IdIo == CurrentDataContext.CurrentRequest.InternalOrder?.IdIo);

                    CurrentDataContext.CurrentRequest.UpdatedListItem.Clear();
                    CurrentDataContext.CurrentRequest.UpdatedListAttachment.Clear();
                    foreach (var item in CurrentDataContext.CurrentRequest.ListItem)
                    {
                        CurrentDataContext.CurrentRequest.UpdatedListItem.Add(item);
                    }
                    foreach (var item in CurrentDataContext.CurrentRequest.ListAttachment)
                    {
                        CurrentDataContext.CurrentRequest.UpdatedListAttachment.Add(item);
                    }

                    CurrentDataContext.CurrentRequest.CheckUpdateRight(LoggedUser.SamAccountName, CurrentDataContext.IsRoleAdmin);

                    CurrentDataContext.CurrentRequest.WindowTitle = $"{McgWpfTools.GetStringResource("POF_WindowTitleCreateRequest")} : {CurrentDataContext.CurrentRequest.ID}";

                    if (!CurrentDataContext.CurrentRequest.IsUpdateAllowed)
                        RaiseUpdateNotAllowedEvent();

                    CurrentDataContext.UpdateDienNlag(CurrentDataContext.CurrentRequest.RequestType);

                    CurrentDataContext.CurrentRequest.CanBeClosedWithoutSaving = true;
                    _purchaseOrderFollowWindowService.ShowDialogPurchaseOrderFollowCreateUpdateView(this);
                    CurrentDataContext.CurrentRequest.IsAdminUpdate = false;

                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreateItem()
        {
            try
            {
                if (CurrentDataContext.CurrentRequest != null)
                {
                    if (CurrentDataContext.CurrentRequest.ListItem.Count < 41)
                    {
                        PurchaseOrderItem LastItem = CurrentDataContext.CurrentRequest.UpdatedListItem.LastOrDefault();
                        PurchaseOrderItem NewItem = new PurchaseOrderItem()
                        {
                            AccountAssignementCategory = "K",
                            RequestedBy = CurrentDataContext.CurrentRequest.RequestedBy,
                            CostCenter = CurrentDataContext.CurrentRequest.CostCenter,
                            InternalOrder = CurrentDataContext.CurrentRequest.InternalOrder,
                            Vendor = CurrentDataContext.CurrentRequest.Vendor,

                        };
                        if (LastItem != null)
                        {
                            NewItem.Number = LastItem.Number + 10;
                            NewItem.DeliveryDate = LastItem.DeliveryDate;
                            NewItem.DeliveryAdress = LastItem.DeliveryAdress;
                            NewItem.DeliveryPlant = LastItem.DeliveryPlant;
                            if (CurrentDataContext.CurrentRequest.RequestType == PurchaseOrderType.ZRMI)
                                NewItem.SelectedMaterial = LastItem.SelectedMaterial;
                        }
                        else
                        {
                            NewItem.Number = 10;
                        }

                        CurrentDataContext.CurrentRequest.UpdatedListItem.Add(NewItem);
                    }
                    else
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("POF_MsgExceededItem"), McgWpfTools.GetStringResource("POF_MsgTitleExceededItem"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemoveItem()
        {
            try
            {
                if (CurrentDataContext.CurrentRequest.SelectedItem != null)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("POF_MsgRemoveItem"), McgWpfTools.GetStringResource("POF_MsgTitleRemoveItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        CurrentDataContext.CurrentRequest.UpdatedListItem.Remove(CurrentDataContext.CurrentRequest.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSaveRequest(bool CloseWindow)
        {
            try
            {
                CreateUpdateRequestOnDataBase(CurrentDataContext.CurrentRequest);

                PurchaseOrderRequest CurrentRequest = CurrentDataContext.ListAllRequest.FirstOrDefault(item => item.ID == CurrentDataContext.CurrentRequest.ID);

                if (CurrentRequest != null)
                {
                    CurrentDataContext.ListRequest.Remove(CurrentRequest);
                    CurrentDataContext.ListAllRequest.Remove(CurrentRequest);
                    CurrentDataContext.ListMyRequest.Remove(CurrentRequest);
                }

                CurrentDataContext.CurrentRequest.CanBeClosedWithoutSaving = true;
                CurrentDataContext.ListRequest.Add(CurrentDataContext.CurrentRequest);
                CurrentDataContext.ListAllRequest.Add(CurrentDataContext.CurrentRequest);
                if (CurrentDataContext.CurrentRequest.CreatedBy.ToUpper() == LoggedUser.SamAccountName.ToUpper() || CurrentDataContext.CurrentRequest.RequestedBy.ToUpper() == LoggedUser.SamAccountName.ToUpper())
                    CurrentDataContext.ListMyRequest.Add(CurrentDataContext.CurrentRequest);

                if (CloseWindow)
                {
                    _purchaseOrderFollowWindowService.ClosePurchaseOrderFollowCreateUpdateView();
                }

                UpdateRequestList();
                UpdateCharts();
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDrop(DragEventArgs obj)
        {
            try
            {
                if (CurrentDataContext.CurrentRequest.IsUpdateAllowed)
                {
                    //if (obj != null && CurrentDataContext.WtDocumentSelected)
                    PurchaseOrderAttachment CurrentAttachment;
                    if (obj != null && obj.Data != null && obj.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        string[] files = (string[])obj.Data.GetData(DataFormats.FileDrop);

                        foreach (var file in files)
                        {
                            if (CurrentDataContext.CurrentRequest.UpdatedListAttachment.FirstOrDefault((item) => item.CompleteFilename != null && item.CompleteFilename == file) == null)
                            {
                                CurrentAttachment = new PurchaseOrderAttachment() { CompleteFilename = file };
                                CurrentAttachment.Update();
                                CurrentDataContext.CurrentRequest.UpdatedListAttachment.Add(CurrentAttachment);
                            }
                        }
                    }
                    else if (obj != null && obj.Data != null && obj.Data.GetDataPresent("FileContents"))
                    {
                        if (obj.Data.GetDataPresent("FileGroupDescriptor"))
                        {
                            Stream fileGroupDescriptorStream = (Stream)obj.Data.GetData("FileGroupDescriptor");
                            byte[] fileGroupDescriptorBytes = new byte[fileGroupDescriptorStream.Length];
                            fileGroupDescriptorStream.Read(fileGroupDescriptorBytes, 0, fileGroupDescriptorBytes.Length);

                            int FILEDESCRIPTOR_SIZE = 592;
                            int MAX_FILENAME_LENGTH = 260;

                            string fileName = null;

                            using (BinaryReader reader = new BinaryReader(new MemoryStream(fileGroupDescriptorBytes)))
                            {
                                reader.BaseStream.Position = 0 * FILEDESCRIPTOR_SIZE + 76; //i*
                                char[] fileNameChars = reader.ReadChars(MAX_FILENAME_LENGTH);

                                fileName = new string(fileNameChars);
                                fileName = fileName.Substring(0, fileName.IndexOf('\0'));
                            }

                            if (fileName != null)
                            {
                                MemoryStream fileContents = (MemoryStream)obj.Data.GetData("FileContents");

                                CurrentAttachment = new PurchaseOrderAttachment() { FileName = fileName };
                                CurrentAttachment.ReadFileFromMemorystream(fileContents);
                                CurrentDataContext.CurrentRequest.UpdatedListAttachment.Add(CurrentAttachment);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDropVendor(DragEventArgs obj)
        {
            try
            {
                if (obj != null && obj.Data != null && obj.Data.GetDataPresent(DataFormats.FileDrop) && CurrentDataContext.CurrentVendor != null)
                {
                    string[] files = (string[])obj.Data.GetData(DataFormats.FileDrop);
                    PurchaseOrderAttachment CurrentAttachment;

                    foreach (var file in files)
                    {
                        if (CurrentDataContext.CurrentVendor.ListAttachment.FirstOrDefault((item) => item.CompleteFilename != null && item.CompleteFilename == file) == null)
                        {
                            CurrentAttachment = new PurchaseOrderAttachment() { CompleteFilename = file };
                            CurrentAttachment.Update();
                            CurrentDataContext.CurrentVendor.ListAttachment.Add(CurrentAttachment);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemoveAttachement()
        {
            try
            {
                if (CurrentDataContext.CurrentRequest.SelectedAttachment != null)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("POF_MsgRemoveAttachement"), McgWpfTools.GetStringResource("POF_MsgTitleRemoveItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        CurrentDataContext.CurrentRequest.UpdatedListAttachment.Remove(CurrentDataContext.CurrentRequest.SelectedAttachment);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemoveAttachementVendor()
        {
            try
            {
                if (CurrentDataContext.CurrentVendor.SelectedAttachment != null)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("POF_MsgRemoveAttachement"), McgWpfTools.GetStringResource("POF_MsgTitleRemoveItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        CurrentDataContext.CurrentVendor.ListAttachment.Remove(CurrentDataContext.CurrentVendor.SelectedAttachment);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenAttachment()
        {
            try
            {
                if (CurrentDataContext.CurrentRequest.SelectedAttachment != null)
                {
                    CurrentDataContext.CurrentRequest.SelectedAttachment.WriteFile();
                    McgFileAndSystemTools.OpenFile(CurrentDataContext.CurrentRequest.SelectedAttachment.TempCompleteFileName);

                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenAttachmentVendor()
        {
            try
            {
                if (CurrentDataContext.CurrentVendor.SelectedAttachment != null)
                {
                    CurrentDataContext.CurrentVendor.SelectedAttachment.WriteFile();
                    McgFileAndSystemTools.OpenFile(CurrentDataContext.CurrentVendor.SelectedAttachment.TempCompleteFileName);

                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSendRequest(bool CloseWindow = true)
        {
            try
            {
                if (CheckRequestData(CurrentDataContext.CurrentRequest))
                {
                    ExecuteSaveRequest(false);

                    Regex RegexProc = new Regex("Excel", RegexOptions.IgnoreCase);
                    List<Process> OldExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));

                    string TempFolder = System.Environment.GetEnvironmentVariable("TEMP");
                    string XlsFileName = $"{TempFolder}\\Purchase_Request_{CurrentDataContext.CurrentRequest.ID}.xlsx";

                    ExcelToolsClosedXml CurrentExcel = new ExcelToolsClosedXml() { CompleteFileName = XlsFileName, CompleteTemplateFileName = Path.Combine(MainAppFolder, CommonLibConstants.ResourcesFolder, PurchaseOrderFollowUpConstants.PurchaseRequestXlsTemplate) };
                    //if (CurrentExcel.OpenFile(Path.Combine(MainAppFolder,CommonLibConstants.ResourcesFolder,PurchaseOrderFollowUpConstants.PurchaseRequestXlsTemplate)) != ExcelStatus.OK)
                    if (CurrentExcel.OpenFile(CurrentExcel.CompleteTemplateFileName) != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("POF_CreateXlsIssue"), XlsFileName), McgWpfTools.GetStringResource("POF_TitleCreateXlsIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    List<Process> NewExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    Process newExcelProcess = NewExcelCurrentProcess.FirstOrDefault((proc) => !OldExcelCurrentProcess.ToList().Exists((oldprc) => proc.Id == oldprc.Id));

                    CurrentExcel.CurrentSheet = "Purchase Request";
                    UpdatePurchaseRequestXls(CurrentExcel);


                    if (CurrentExcel.SaveClose() != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("POF_CreateXlsIssue"), XlsFileName), McgWpfTools.GetStringResource("POF_TitleCreateXlsIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    if (newExcelProcess != null)
                        newExcelProcess.Kill();

                    // Add Xls as attachment
                    PurchaseOrderAttachment CurrentAttachment = new PurchaseOrderAttachment() { CompleteFilename = XlsFileName };
                    CurrentAttachment.IsRequestFile = true;
                    CurrentAttachment.Update();

                    PurchaseOrderAttachment OldXlsRequest = CurrentDataContext.CurrentRequest.ListAttachment.FirstOrDefault((x) => x.IsRequestFile);
                    if (OldXlsRequest != null)
                        CurrentDataContext.CurrentRequest.UpdatedListAttachment.Remove(OldXlsRequest);

                    CurrentDataContext.CurrentRequest.UpdatedListAttachment.Add(CurrentAttachment);

                    if (CurrentDataContext.CurrentRequest.Status == PurchaseOrderStatus.NEW)
                        CurrentDataContext.CurrentRequest.Status = PurchaseOrderStatus.SENT;

                    ExecuteSaveRequest(true);

                    //SendPurchaseRequestEmail();
                    SendPurchaseRequestEmailWithoutOulook();
                }
                else
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowCreateRequestMissingData"), McgWpfTools.GetStringResource("POF_TitleCreateRequestIssue"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSetSearchOrder()
        {
            try
            {
                SearchType = "ORDER";
                if (PreviousSearchType != SearchType)
                {
                    CurrentDataContext.ListItem = null;
                    CurrentDataContext.NumberSearchField = "";
                    CurrentDataContext.DescriptionSearchField = "";
                }
                PreviousSearchType = SearchType;
                CurrentDataContext.HasLocationProperty = false;
                // CurrentDataContext.ListItem = CurrentDataContext.ListInternalOrder;
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSetSearchVendor()
        {
            try
            {
                SearchType = "VENDOR";
                if (PreviousSearchType != SearchType)
                {
                    CurrentDataContext.ListItem = null;
                    CurrentDataContext.NumberSearchField = "";
                    CurrentDataContext.DescriptionSearchField = "";
                }
                PreviousSearchType = SearchType;
                CurrentDataContext.HasLocationProperty = true;
                // CurrentDataContext.ListItem = CurrentDataContext.ListVendor;

            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSetSearchItem()
        {
            try
            {
                if (SearchType == "VENDOR")
                {
                    SearchSapHubVendor();
                }
                else if (SearchType == "ORDER")
                {
                    SearchInternalOrder();
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSelectItem()
        {
            try
            {
                if (CurrentDataContext.SelectedItem != null)
                {
                    if (CurrentDataContext.SelectedItem.GetType() == typeof(PurchaseOrderInternalOrder))
                    {
                        PurchaseOrderInternalOrder SelectedOrder = (PurchaseOrderInternalOrder)CurrentDataContext.SelectedItem;
                        if (CurrentDataContext.ListInternalOrder.FirstOrDefault(item => item.Number == SelectedOrder.Number) == null)
                        {
                            CurrentDataContext.ListInternalOrder.Add(SelectedOrder);
                            CurrentDataContext.CurrentRequest.InternalOrder = SelectedOrder;
                        }
                        else
                            CurrentDataContext.CurrentRequest.InternalOrder = CurrentDataContext.ListInternalOrder.FirstOrDefault(item => item.Number == SelectedOrder.Number);
                    }
                    else if (CurrentDataContext.SelectedItem.GetType() == typeof(PurchaseOrderVendor))
                    {
                        PurchaseOrderVendor SelectedVendor = (PurchaseOrderVendor)CurrentDataContext.SelectedItem;
                        if (CurrentDataContext.ListVendor.FirstOrDefault(item => item.Number == SelectedVendor.Number) == null)
                        {
                            CurrentDataContext.ListVendor.Add(SelectedVendor);
                            CurrentDataContext.CurrentRequest.Vendor = SelectedVendor;
                        }
                        else
                            CurrentDataContext.CurrentRequest.Vendor = CurrentDataContext.ListVendor.FirstOrDefault(item => item.Number == SelectedVendor.Number);
                    }
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSelectVendor()
        {
            try
            {
                if (CurrentDataContext.SelectedItem != null)
                {
                    if (CurrentDataContext.SelectedItem.GetType() == typeof(PurchaseOrderVendor))
                    {
                        CurrentDataContext.CurrentVendor = (PurchaseOrderVendor)CurrentDataContext.SelectedItem;
                    }
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSearchRquestType(string BtNumber)
        {
            try
            {
                CurrentBtRequestType = BtNumber;
                switch (BtNumber)
                {
                    case "RESET":
                        RaiseUpdateRequestTypeEvent();
                        break;
                    case "BT1":
                        if (QuesionLevel == 1)
                        {
                            RaiseChangeRequestTypeQuestionEvent();
                        }
                        else if (QuesionLevel == 2)
                        {
                            RaiseUpdateRequestTypeEvent();
                        }
                        break;
                    case "BT2":
                        if (QuesionLevel == 1)
                        {
                            RaiseChangeRequestTypeQuestionEvent();
                        }
                        else if (QuesionLevel == 2)
                        {
                            RaiseUpdateRequestTypeEvent();
                        }
                        break;
                    case "BT3":
                        RaiseUpdateRequestTypeEvent();
                        break;
                    case "BT4":
                        RaiseUpdateRequestTypeEvent();
                        break;
                    case "BT5":
                        RaiseUpdateRequestTypeEvent();
                        break;
                    case "BT6":
                        RaiseUpdateRequestTypeEvent();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAskNewVendor()
        {
            try
            {
                if (!IsSearchDbMaterialGroupDone)
                {
                    SearchDbMaterialGroup();
                    IsSearchDbMaterialGroupDone = true;
                }

                CurrentDataContext.CurrentVendor = new PurchaseOrderVendor() { ToBeUpdated = false };
                _purchaseOrderFollowWindowService.ShowDialogPurchaseOrderFollowUpCreateUpdateVendorView(this);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAskUpdateVendor(bool IsFromRequest = false)
        {
            try
            {
                if (!IsSearchDbMaterialGroupDone)
                {
                    SearchDbMaterialGroup();
                    IsSearchDbMaterialGroupDone = true;
                }

                CurrentDataContext.CurrentVendor = new PurchaseOrderVendor() { ToBeUpdated = true };
                bool ResultWindow = true;

                if (IsFromRequest && CurrentDataContext.SelectedRequest != null)
                {
                    CurrentDataContext.CurrentVendor.Number = CurrentDataContext.SelectedRequest.Vendor.Number;
                    CurrentDataContext.CurrentVendor.Description = CurrentDataContext.SelectedRequest.Vendor.Description;
                    CurrentDataContext.CurrentVendor.Location = CurrentDataContext.SelectedRequest.Vendor.Location;
                }
                else
                {
                    ResultWindow = false;
                    ExecuteSetSearchVendor();
                    var dialogResult = _purchaseOrderFollowWindowService.ShowDialogPurchaseOrderFollowUpSelectVendorView(this);
                    if (dialogResult != null && CurrentDataContext.CurrentVendor != null && !string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.Number))
                    {
                        CurrentDataContext.CurrentVendor.ToBeUpdated = true;
                        ResultWindow = dialogResult.Value;
                    }
                }

                if (ResultWindow)
                    _purchaseOrderFollowWindowService.ShowDialogPurchaseOrderFollowUpCreateUpdateVendorView(this);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAskNewInternalOrder()
        {
            try
            {
                var dialogResult = _purchaseOrderFollowWindowService.ShowDialogPurchaseOrderFollowUpInternalOrderRequestView(this);

                if (dialogResult != true)
                    return;

                if (!string.IsNullOrWhiteSpace(CurrentDataContext.InternalOrderDescription))
                {
                    SendInternalOrderRequestEmail();
                    return;
                }

                MessageBox.Show(
                    McgWpfTools.GetStringResource("POF_MsgErrorInternalOrderDesc"),
                    McgWpfTools.GetStringResource("POF_MailInternalOrderObject"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                ExecuteAskNewInternalOrder();
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAskExtendPart()
        {
            try
            {
                if (!(CurrentDataContext.SelectedRequest.RequestType == PurchaseOrderType.ZRMI))
                {
                    if (CurrentDataContext.SelectedRequest.IsCheckExtendedRun)
                        SendExtendPartRequestEmail();
                    else
                        MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowExtendPartNotRun"), McgWpfTools.GetStringResource("POF_TitleExtendPartIssue"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowExtendPartIssue"), McgWpfTools.GetStringResource("POF_TitleExtendPartIssue"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSendVendorRequest()
        {
            try
            {
                if (CheckVendorRequestData() || CurrentDataContext.CurrentVendor.ToBeUpdated)
                {
                    Regex RegexProc = new Regex("Excel", RegexOptions.IgnoreCase);
                    List<Process> OldExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));

                    string TempFolder = System.Environment.GetEnvironmentVariable("TEMP");
                    string XlsFileName = $"{TempFolder}\\Vendor_Request_{(new Random(100000)).Next()}.xlsx";

                    ExcelToolsClosedXml CurrentExcel = new ExcelToolsClosedXml() { CompleteFileName = XlsFileName, CompleteTemplateFileName = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{PurchaseOrderFollowUpConstants.VendorRequestXlsTemplate}" };
                    if (CurrentExcel.OpenFile($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{PurchaseOrderFollowUpConstants.VendorRequestXlsTemplate}") != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("POF_CreateXlsIssue"), XlsFileName), McgWpfTools.GetStringResource("POF_TitleCreateXlsIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    List<Process> NewExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    Process newExcelProcess = NewExcelCurrentProcess.FirstOrDefault((proc) => !OldExcelCurrentProcess.ToList().Exists((oldprc) => proc.Id == oldprc.Id));

                    CurrentExcel.CurrentSheet = "Vendor Request";
                    UpdateVendorRequestXls(CurrentExcel);

                    if (CurrentExcel.SaveClose() != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("POF_CreateXlsIssue"), XlsFileName), McgWpfTools.GetStringResource("POF_TitleCreateXlsIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    if (newExcelProcess != null)
                        newExcelProcess.Kill();

                    // Add Xls as attachment
                    PurchaseOrderAttachment CurrentAttachment = new PurchaseOrderAttachment() { CompleteFilename = XlsFileName };
                    CurrentAttachment.IsRequestFile = true;
                    CurrentAttachment.Update();

                    PurchaseOrderAttachment OldXlsRequest = CurrentDataContext.CurrentVendor.ListAttachment.FirstOrDefault((x) => x.IsRequestFile);
                    if (OldXlsRequest != null)
                        CurrentDataContext.CurrentVendor.ListAttachment.Remove(OldXlsRequest);


                    CurrentDataContext.CurrentVendor.ListAttachment.Add(CurrentAttachment);

                    SendVendorRequestEmail();
                }
                else
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("POF_MsgVendorMissingData"), McgWpfTools.GetStringResource("POF_TitleMsgVendorMissingData"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteShowVendor(bool IsFromRequest = false)
        {
            try
            {
                CurrentDataContext.CurrentVendor = new PurchaseOrderVendor();
                bool ResultWindow = true;

                if (IsFromRequest && CurrentDataContext.SelectedRequest != null)
                {
                    CurrentDataContext.CurrentVendor.Number = CurrentDataContext.SelectedRequest.Vendor.Number;
                    CurrentDataContext.CurrentVendor.Description = CurrentDataContext.SelectedRequest.Vendor.Description;
                    CurrentDataContext.CurrentVendor.Location = CurrentDataContext.SelectedRequest.Vendor.Location;
                }
                else
                {
                    ResultWindow = false;
                    ExecuteSetSearchVendor();
                    var dialogResult = _purchaseOrderFollowWindowService.ShowDialogPurchaseOrderFollowUpSelectVendorView(this);
                    if (dialogResult != null)
                        ResultWindow = dialogResult.Value;
                }

                if (ResultWindow)
                {
                    if (_sapPurchasingService.ShowVendorWindow(CurrentDataContext.CurrentVendor.Number) != SAPBomMsg.OK)
                        MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowSapNotStarted"), McgWpfTools.GetStringResource("POF_TitleSapIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddRequestFromSapPr()
        {
            try
            {
                var (dialogResult, valueResult) = _mcgCommonLibWindowService.ShowDialogMcgWindowOkCancel();

                //McgWindowOkCancel mcgWindowOkCancel = new McgWindowOkCancel();
                //mcgWindowOkCancel.ShowDialog();

                //if (mcgWindowOkCancel.DialogResult.Value)
                if (dialogResult.Value)
                {
                    //if (!string.IsNullOrEmpty(mcgWindowOkCancel.CurrendDataContext.Value))
                    if (!string.IsNullOrEmpty(valueResult))
                    {
                        //PurchaseOrderRequest SearchedRequest = SearchSapHubRequestFromPurchaseRequest(mcgWindowOkCancel.CurrendDataContext.Value);
                        PurchaseOrderRequest SearchedRequest = SearchSapHubRequestFromPurchaseRequest(valueResult);
                        if (SearchedRequest != null)
                        {
                            UpdateListFromRequest(SearchedRequest);
                            ExecuteCreatePurchaseOrder(SearchedRequest);
                        }
                        else
                        {
                            //MessageBox.Show(string.Format(McgWpfTools.GetStringResource("POF_MsgAddRequestNtoFound"), mcgWindowOkCancel.CurrendDataContext.Value), McgWpfTools.GetStringResource("POF_TitleAddRequestIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("POF_MsgAddRequestNtoFound"), valueResult), McgWpfTools.GetStringResource("POF_TitleAddRequestIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        }
                    }
                    else
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("POF_MsgAddRequestIssue"), McgWpfTools.GetStringResource("POF_TitleAddRequestIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddRequestFromSapPo()
        {
            try
            {
                var (dialogResult, valueResult) = _mcgCommonLibWindowService.ShowDialogMcgWindowOkCancel();

                //McgWindowOkCancel mcgWindowOkCancel = new McgWindowOkCancel();
                //mcgWindowOkCancel.ShowDialog();

                //if (mcgWindowOkCancel.DialogResult.Value)
                if (dialogResult.Value)
                {
                    // if (!string.IsNullOrEmpty(mcgWindowOkCancel.CurrendDataContext.Value))
                    if (!string.IsNullOrEmpty(valueResult))
                    {
                        //PurchaseOrderRequest SearchedRequest = SearchSapHubRequestFromPurchaseOrder(mcgWindowOkCancel.CurrendDataContext.Value);
                        PurchaseOrderRequest SearchedRequest = SearchSapHubRequestFromPurchaseOrder(valueResult);
                        if (SearchedRequest == null)
                            //SearchedRequest = SearchSapHubRequestFromPurchaseOrderWithoutPurchaseResquest(mcgWindowOkCancel.CurrendDataContext.Value);
                            SearchedRequest = SearchSapHubRequestFromPurchaseOrderWithoutPurchaseResquest(valueResult);
                        if (SearchedRequest != null)
                        {
                            //if (!CheckIfRequestAlreadyExist(mcgWindowOkCancel.CurrendDataContext.Value, "PO"))
                            if (!CheckIfRequestAlreadyExist(valueResult, "PO"))
                            {
                                UpdateListFromRequest(SearchedRequest);
                                ExecuteCreatePurchaseOrder(SearchedRequest);
                            }
                            else
                            {
                                //MessageBox.Show(string.Format(McgWpfTools.GetStringResource("POF_MsgAddRequestAlreadyExist"), mcgWindowOkCancel.CurrendDataContext.Value), McgWpfTools.GetStringResource("POF_TitleAddRequestIssue"), MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("POF_MsgAddRequestAlreadyExist"), valueResult), McgWpfTools.GetStringResource("POF_TitleAddRequestIssue"), MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                            }
                        }
                        else
                        {
                            //MessageBox.Show(string.Format(McgWpfTools.GetStringResource("POF_MsgAddRequestNotFound"), mcgWindowOkCancel.CurrendDataContext.Value), McgWpfTools.GetStringResource("POF_TitleAddRequestIssue"), MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("POF_MsgAddRequestNotFound"), valueResult), McgWpfTools.GetStringResource("POF_TitleAddRequestIssue"), MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                        }
                    }
                    else
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("POF_MsgAddRequestIssue"), McgWpfTools.GetStringResource("POF_TitleAddRequestIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddRequestFromSapPo(PurchaseOrderRequest SearchedRequest)
        {
            try
            {
                if (SearchedRequest != null)
                {
                    if (SearchedRequest.IsAlreadyExist)
                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("POF_MsgAddRequestAlreadyExist"), SearchedRequest.SapPurchaseOrder), McgWpfTools.GetStringResource("POF_TitleAddRequestIssue"), MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    else
                    {
                        UpdateListFromRequest(SearchedRequest);
                        ExecuteCreatePurchaseOrder(SearchedRequest);
                        SearchedRequest.IsAlreadyExist = true;
                    }
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddInternalOrder()
        {
            try
            {
                //if (true)
                //{

                //}
                //else
                //{
                //    MessageBox.Show(McgMiscTools.GetStringResource("POF_MsgVendorMissingData"), McgMiscTools.GetStringResource("POF_TitleMsgVendorMissingData"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                //}
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateInternalOrder()
        {
            try
            {
                //    if (true)
                //    {

                //    }
                //    else
                //    {
                //        MessageBox.Show(McgMiscTools.GetStringResource("POF_MsgVendorMissingData"), McgMiscTools.GetStringResource("POF_TitleMsgVendorMissingData"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                //    }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDeleteInternalOrder()
        {
            try
            {
                //if (true)
                //{

                //}
                //else
                //{
                //    MessageBox.Show(McgMiscTools.GetStringResource("POF_MsgVendorMissingData"), McgMiscTools.GetStringResource("POF_TitleMsgVendorMissingData"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                //}
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddRequestFromSapPoDates()
        {
            try
            {
                CurrentDataContext.ListSearchedRequest.Clear();
                var ListReq = SearchSapHubRequestFromPurchaseRequestFromDates(CurrentDataContext.PoCreatedAfter.Value.ToDateTime(new TimeOnly(0, 0)), CurrentDataContext.PoCreatedBefore.Value.ToDateTime(new TimeOnly(0, 0)));
                if (ListReq != null)
                {
                    foreach (var req in ListReq)
                        CurrentDataContext.ListSearchedRequest.Add(req);

                    _purchaseOrderFollowWindowService.ShowDialogPurchaseOrderFollowListRequestView(this);
                    //PurchaseOrderFollowListRequestView CurrentWindow = new PurchaseOrderFollowListRequestView();
                    //CurrentWindow.DataContext = this;
                    //CurrentWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDeleteRequest()
        {
            try
            {
                if (CurrentDataContext.SelectedRequest != null)
                {
                    if (((CurrentDataContext.SelectedRequest.Status == PurchaseOrderStatus.NEW || CurrentDataContext.SelectedRequest.Status == PurchaseOrderStatus.SENT)
                        && (LoggedUser.SamAccountName == CurrentDataContext.SelectedRequest.CreatedBy || LoggedUser.SamAccountName == CurrentDataContext.SelectedRequest.RequestedBy)) || CurrentDataContext.IsRoleAdmin)
                    {
                        if (MessageBox.Show(string.Format(McgWpfTools.GetStringResource("POF_WindowDeleteRequest"), CurrentDataContext.SelectedRequest.ID), McgWpfTools.GetStringResource("POF_TitleDeleteRequest"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            DeleteRequestFromDataBase(CurrentDataContext.SelectedRequest);
                        }
                    }
                    else
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowDeleteRequestNot"), McgWpfTools.GetStringResource("POF_TitleDeleteRequest"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdatePrFromSapHub()
        {
            try
            {
                if (CurrentDataContext.CurrentRequest != null)
                {
                    UpdateRequestFromSapHubPurchaseRequest(CurrentDataContext.CurrentRequest);
                    if (!string.IsNullOrEmpty(CurrentDataContext.CurrentRequest.SapPurchaseOrder))
                    {
                        UpdateRequestFromSapHubPurchaseOrder(CurrentDataContext.CurrentRequest);
                        UpdateRequestFromSapHubPurchaseOrderGoodsInvoiceReceipt(CurrentDataContext.CurrentRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdatePoFromSapHub()
        {
            try
            {
                if (CurrentDataContext.CurrentRequest != null && !string.IsNullOrEmpty(CurrentDataContext.CurrentRequest.SapPurchaseOrder))
                {
                    UpdateRequestFromSapHubPurchaseOrder(CurrentDataContext.CurrentRequest);
                    UpdateRequestFromSapHubPurchaseOrderGoodsInvoiceReceipt(CurrentDataContext.CurrentRequest);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenPurchaseRequest()
        {
            try
            {
                if (CurrentDataContext.SelectedRequest != null && CurrentDataContext.SelectedRequest.SapPurchaseRequest != null)
                {
                    if (_sapPurchasingService.ShowPurchaseRequestWindow(CurrentDataContext.SelectedRequest.SapPurchaseRequest) != SAPBomMsg.OK)
                        MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowSapNotStarted"), McgWpfTools.GetStringResource("POF_TitleSapIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenPurchaseOrder()
        {
            try
            {
                if (CurrentDataContext.SelectedRequest != null && CurrentDataContext.SelectedRequest.SapPurchaseOrder != null)
                {
                    if (_sapPurchasingService.ShowPurchaseOrderWindow(CurrentDataContext.SelectedRequest.SapPurchaseOrder) != SAPBomMsg.OK)
                        MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowSapNotStarted"), McgWpfTools.GetStringResource("POF_TitleSapIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenResa()
        {
            try
            {
                if (CurrentDataContext.SelectedRequest != null && CurrentDataContext.SelectedRequest.SapPurchaseRequest != null)
                {
                    if (_sapPurchasingService.ShowResaWindow(CurrentDataContext.SelectedRequest.SapPurchaseRequest) != SAPBomMsg.OK)
                        MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowSapNotStarted"), McgWpfTools.GetStringResource("POF_TitleSapIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartUpdateAllRequestFromSap(bool IsAsynch = true)
        {
            try
            {
                CurrentDataContext.IsNoActionInProgress = false;

                if (IsAsynch)
                {
                    Thread aThread = new Thread(new ThreadStart(StartUpdateAllRequestFromSapAcynch));
                    aThread.IsBackground = true;
                    aThread.Start();
                }
                else
                    StartUpdateAllRequestFromSapAcynch();
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreateSapRequest()
        {
            try
            {
                CurrentDataContext.IsNoActionInProgress = false;

                Thread aThread = new Thread(new ThreadStart(StartCreateSapRequestAsynch));
                aThread.IsBackground = true;
                aThread.Start();
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteShowSapStock(bool FromShownRequest)
        {
            try
            {
                PurchaseOrderRequest CurrentRequest = null;
                if (FromShownRequest)
                    CurrentRequest = CurrentDataContext.CurrentRequest;
                else
                {
                    CurrentRequest = CurrentDataContext.SelectedRequest;
                    if (CurrentRequest != null && CurrentRequest.ListItem.Count == 0)
                        UpdateRequestItemAttachment(CurrentRequest);
                }
                List<string> ListParts = CurrentRequest.UpdatedListItem.Where(item => item != null && item.MaterialNumber != null && !string.IsNullOrEmpty(item.MaterialNumber.Number)).Select(item => item.MaterialNumber.Number).ToList();

                if (_sapPurchasingService.ShowPartsStock(ListParts) != SAPBomMsg.OK)
                    MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowSapNotStarted"), McgWpfTools.GetStringResource("POF_TitleSapIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteConvertSapRequest()
        {
            try
            {
                CurrentDataContext.IsNoActionInProgress = false;

                Thread aThread = new Thread(new ThreadStart(StartConvertSapRequestAsynch));
                aThread.IsBackground = true;
                aThread.Start();

            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDownloadAttachment()
        {
            try
            {
                if (CurrentDataContext.CurrentRequest.SelectedAttachment != null)
                {
                    CurrentDataContext.CurrentRequest.SelectedAttachment.DownloadFile();
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSearchColumnKeyWord(McgColumnData data = null)
        {
            try
            {
                if (data != null)
                {
                    List<McgColumnData> tempListFilters;
                    //update filters list

                    if (data.ListName == "ListShownRequest")
                        tempListFilters = ListFilters;
                    else
                        tempListFilters = ListMyFilters;

                    McgColumnData currentFilter = tempListFilters.FirstOrDefault(item => item.ColumnReference == data.ColumnReference);
                    if (currentFilter != null)
                    {
                        if (string.IsNullOrEmpty(data.FilterValue))
                            tempListFilters.Remove(currentFilter);
                        else
                            currentFilter.FilterValue = data.FilterValue;
                    }
                    else
                        if (!string.IsNullOrEmpty(data.FilterValue))
                            tempListFilters.Add(data);
                }

                //Apply filters
                CurrentDataContext.ListShownRequest.Clear();
                CurrentDataContext.ListShownMyRequest.Clear();

                List<PurchaseOrderRequest> listTempBefore = CurrentDataContext.ListRequest.ToList();
                List<PurchaseOrderRequest> listTempAfter = new List<PurchaseOrderRequest>();
                List<PurchaseOrderRequest> listTempMyBefore = CurrentDataContext.ListMyRequest.ToList();
                List<PurchaseOrderRequest> listTempMyAfter = new List<PurchaseOrderRequest>();

                object currentPropValue;
                foreach (McgColumnData filter in ListFilters)
                {
                    listTempAfter.Clear();
                    foreach (PurchaseOrderRequest item in listTempBefore)
                    {
                        currentPropValue = McgReflectionTools.GetNestedPropertyValue(item, filter.ColumnReference);
                        if (currentPropValue != null && currentPropValue.ToString().ToUpper().Contains(filter.FilterValue.ToUpper()))
                            listTempAfter.Add(item);
                    }

                    listTempBefore.Clear();
                    foreach (PurchaseOrderRequest item in listTempAfter)
                        listTempBefore.Add(item);
                }

                foreach (McgColumnData filter in ListMyFilters)
                {
                    listTempMyAfter.Clear();
                    foreach (PurchaseOrderRequest item in listTempMyBefore)
                    {
                        currentPropValue = McgReflectionTools.GetNestedPropertyValue(item, filter.ColumnReference);
                        if (currentPropValue != null && currentPropValue.ToString().ToUpper().Contains(filter.FilterValue.ToUpper()))
                            listTempMyAfter.Add(item);
                    }

                    listTempMyBefore.Clear();
                    foreach (PurchaseOrderRequest item in listTempMyAfter)
                        listTempMyBefore.Add(item);
                }

                foreach (PurchaseOrderRequest item in listTempBefore)
                    CurrentDataContext.ListShownRequest.Add(item);

                foreach (PurchaseOrderRequest item in listTempMyBefore)
                    CurrentDataContext.ListShownMyRequest.Add(item);

            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void SearchRquestType_UpdateRequestTypeEvent(object sender, EventArgs e)
        {
            try
            {
                string Plant = "";
                switch (CurrentBtRequestType)
                {
                    case "RESET":
                        CurrentDataContext.BtRequestTypeQuestion = McgWpfTools.GetStringResource("POF_RequestTypeQ01");
                        CurrentDataContext.BtRequestType1 = McgWpfTools.GetStringResource("POF_RequestTypeR01_1");
                        CurrentDataContext.BtRequestType2 = McgWpfTools.GetStringResource("POF_RequestTypeR01_2");
                        CurrentDataContext.IsAllBtRequestTypeShown = false;
                        QuesionLevel = 1;
                        break;
                    case "BT1":
                        if (QuesionLevel == 1)
                        {
                            CurrentDataContext.BtRequestTypeQuestion = McgWpfTools.GetStringResource("POF_RequestTypeQ03");
                            CurrentDataContext.BtRequestType1 = McgWpfTools.GetStringResource("POF_RequestTypeR03_1");
                            CurrentDataContext.BtRequestType2 = McgWpfTools.GetStringResource("POF_RequestTypeR03_2");
                            CurrentDataContext.BtRequestType3 = McgWpfTools.GetStringResource("POF_RequestTypeR03_3");
                            CurrentDataContext.BtRequestType4 = McgWpfTools.GetStringResource("POF_RequestTypeR03_4");
                            CurrentDataContext.BtRequestType5 = McgWpfTools.GetStringResource("POF_RequestTypeR03_5");
                            CurrentDataContext.BtRequestType6 = McgWpfTools.GetStringResource("POF_RequestTypeR03_6");
                            CurrentDataContext.IsAllBtRequestTypeShown = true;
                            QuesionLevel++;
                        }
                        else if (QuesionLevel == 2)
                        {
                            if (CurrentDataContext.BtRequestTypeQuestion == McgWpfTools.GetStringResource("POF_RequestTypeQ03"))
                            {
                                CurrentDataContext.CurrentRequest.RequestType = PurchaseOrderType.RESA;
                                Plant = McgWpfTools.GetStringResource("POF_RequestTypeR03_1");
                            }
                            else if (CurrentDataContext.BtRequestTypeQuestion == McgWpfTools.GetStringResource("POF_RequestTypeQ02"))
                            {
                                CurrentDataContext.CurrentRequest.RequestType = PurchaseOrderType.ZNB;
                            }
                            RaiseEndRequestTypeEvent();
                        }
                        break;
                    case "BT2":
                        if (QuesionLevel == 1)
                        {
                            CurrentDataContext.BtRequestTypeQuestion = McgWpfTools.GetStringResource("POF_RequestTypeQ02");
                            CurrentDataContext.BtRequestType1 = McgWpfTools.GetStringResource("POF_RequestTypeR02_1");
                            CurrentDataContext.BtRequestType2 = McgWpfTools.GetStringResource("POF_RequestTypeR02_2");
                            CurrentDataContext.IsAllBtRequestTypeShown = false;
                            QuesionLevel++;
                        }
                        else if (QuesionLevel == 2)
                        {
                            if (CurrentDataContext.BtRequestTypeQuestion == McgWpfTools.GetStringResource("POF_RequestTypeQ03"))
                            {
                                CurrentDataContext.CurrentRequest.RequestType = PurchaseOrderType.RESA;
                                Plant = McgWpfTools.GetStringResource("POF_RequestTypeR03_2");
                            }
                            else if (CurrentDataContext.BtRequestTypeQuestion == McgWpfTools.GetStringResource("POF_RequestTypeQ02"))
                            {
                                CurrentDataContext.CurrentRequest.RequestType = PurchaseOrderType.ZRMI;
                            }
                            RaiseEndRequestTypeEvent();
                        }

                        break;
                    case "BT3":
                        CurrentDataContext.CurrentRequest.RequestType = PurchaseOrderType.ZICP;
                        Plant = McgWpfTools.GetStringResource("POF_RequestTypeR03_3");
                        RaiseEndRequestTypeEvent();
                        break;
                    case "BT4":
                        CurrentDataContext.CurrentRequest.RequestType = PurchaseOrderType.ZICP;
                        Plant = McgWpfTools.GetStringResource("POF_RequestTypeR03_4");
                        RaiseEndRequestTypeEvent();
                        break;
                    case "BT5":
                        CurrentDataContext.CurrentRequest.RequestType = PurchaseOrderType.ZICP;
                        Plant = McgWpfTools.GetStringResource("POF_RequestTypeR03_5");
                        RaiseEndRequestTypeEvent();
                        break;
                    case "BT6":
                        CurrentDataContext.CurrentRequest.RequestType = PurchaseOrderType.RESA;
                        Plant = McgWpfTools.GetStringResource("POF_RequestTypeR03_6");
                        RaiseEndRequestTypeEvent();
                        break;
                    default:
                        break;
                }


                if (QuesionLevel == 2)
                    GetVendorFromRequestType(CurrentDataContext.CurrentRequest.RequestType, Plant);

            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenPurchaseOrderPdf()
        {
            try
            {
                if (CurrentDataContext.SelectedRequest != null && CurrentDataContext.SelectedRequest.SapPurchaseOrder != null)
                {
                    if (_sapPurchasingService.ShowPurchaseOrderPdfWindow(CurrentDataContext.SelectedRequest.SapPurchaseOrder) != SAPBomMsg.OK)
                        MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowSapNotStarted"), McgWpfTools.GetStringResource("POF_TitleSapIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteReceiptPurchaseOrder()
        {
            try
            {
                if (CurrentDataContext.SelectedRequest != null && CurrentDataContext.SelectedRequest.SapPurchaseOrder != null)
                {
                    if (_sapPurchasingService.ShowReceiptPurchaseOrderWindow(CurrentDataContext.SelectedRequest.SapPurchaseOrder) != SAPBomMsg.OK)
                        MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowSapNotStarted"), McgWpfTools.GetStringResource("POF_TitleSapIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSearchDuplicateRequest()
        {
            try
            {
                CurrentDataContext.ListDuplicateRequest.Clear();
                SearchSimilarRequest();

                _purchaseOrderFollowWindowService.ShowPurchaseOrderFollowUpDuplicate(this);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAdminUpdate()
        {
            try
            {
                if (CurrentDataContext.SelectedRequest != null)
                {
                    ExecuteUpdatePurchaseOrder(true);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckExtendPart()
        {
            try
            {
                if (!(CurrentDataContext.SelectedRequest.RequestType == PurchaseOrderType.ZRMI))
                {
                    SendExtendCheckExtendPart();
                }
                else
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowExtendPartIssue"), McgWpfTools.GetStringResource("POF_TitleExtendPartIssue"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] SQL Database Methods
        private void SearchRequestWithoutItemAttachment()
        {
            try
            {
                string AccountID = LoggedUser.SamAccountName.ToUpper();
                //AccountID = "CP67327";
                CurrentDataContext.ListAllRequest = new List<PurchaseOrderRequest>();
                var allRequest = _purchaseOrderService.GetAllPoRequests();

                PurchaseOrderRequest CurrentRequest = null;
                foreach (var request in allRequest)
                {
                    CurrentRequest = PurchaseOrderRequest.GetRequestFromDb(request);
                    CurrentRequest.CostCenter = CurrentDataContext.ListCostCenter.FirstOrDefault(cc => cc.Number == request.Costcenter);
                    CurrentRequest.Vendor = PurchaseOrderVendor.GetVendorFromDbItem(_purchaseOrderService.GetPoVendorById(request.Idvendor.Value).FirstOrDefault());
                    CurrentRequest.InternalOrder = PurchaseOrderInternalOrder.GetInternalOrderFromDbItem(_purchaseOrderService.GetPoInternalOrderById(request.Idio.Value).FirstOrDefault());
                    CurrentDataContext.ListRequest.Add(CurrentRequest);
                    CurrentDataContext.ListAllRequest.Add(CurrentRequest);
                    CurrentRequest.CheckUpdateRight(AccountID, CurrentDataContext.IsRoleAdmin);
                    if (request.Createdby.ToUpper() == AccountID || request.Requestedby.ToUpper() == AccountID || request.Sapcreatedby?.ToUpper() == AccountID)
                        CurrentDataContext.ListMyRequest.Add(CurrentRequest);
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void SearchRequestWithoutItemAttachment(DateTime startDate, DateTime endDate)
        {
            try
            {
                string accountId = LoggedUser.SamAccountName.ToUpper();

                CurrentDataContext.ListAllRequest = new List<PurchaseOrderRequest>();

                var allRequest = _purchaseOrderService.GetAllPoRequestWithVendorAndInternalOrder(startDate, endDate);

                foreach (var request in allRequest)
                {
                    var currentRequest = PurchaseOrderRequest.GetRequestFromDb(request);

                    currentRequest.CostCenter =
                        CurrentDataContext.ListCostCenter.FirstOrDefault(cc => cc.Number == request.Costcenter);

                    // ✅ Vendor déjà disponible via Include
                    currentRequest.Vendor = request.IdvendorNavigation != null
                        ? PurchaseOrderVendor.GetVendorFromDbItem(request.IdvendorNavigation)
                        : null;

                    // ✅ InternalOrder déjà disponible via Include
                    currentRequest.InternalOrder = request.IdioNavigation != null
                        ? PurchaseOrderInternalOrder.GetInternalOrderFromDbItem(request.IdioNavigation)
                        : null;

                    CurrentDataContext.ListRequest.Add(currentRequest);
                    CurrentDataContext.ListAllRequest.Add(currentRequest);

                    currentRequest.CheckUpdateRight(accountId, CurrentDataContext.IsRoleAdmin);

                    if ((request.Createdby ?? "").ToUpper() == accountId
                        || (request.Requestedby ?? "").ToUpper() == accountId
                        || (request.Sapcreatedby ?? "").ToUpper() == accountId)
                    {
                        CurrentDataContext.ListMyRequest.Add(currentRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateRequestItemAttachment(PurchaseOrderRequest CurrentRequest)
        {
            try
            {
                if (CurrentRequest != null)
                {
                    CurrentRequest.ListItem.Clear();
                    CurrentRequest.UpdatedListItem.Clear();
                    CurrentRequest.ListAttachment.Clear();
                    CurrentRequest.UpdatedListAttachment.Clear();

                    var ListItem = _purchaseOrderService.GetAllPoItems(CurrentRequest.ID);

                    PurchaseOrderItem CurrentItem = null;
                    foreach (var item in ListItem)
                    {
                        CurrentItem = PurchaseOrderItem.GetFromDbItem(item);
                        if (CurrentRequest.RequestType == PurchaseOrderType.ZRMI && !string.IsNullOrEmpty(CurrentItem.Material))
                            CurrentItem.SelectedMaterial = CurrentDataContext.ListDienNlagMaterial.FirstOrDefault(mat => mat.Number == CurrentItem.Material);

                        CurrentItem.Vendor = PurchaseOrderVendor.GetVendorFromDbItem(_purchaseOrderService.GetPoVendorById(item.Idvendor.Value).FirstOrDefault());
                        CurrentItem.InternalOrder = PurchaseOrderInternalOrder.GetInternalOrderFromDbItem(_purchaseOrderService.GetPoInternalOrderById(item.Idio.Value).FirstOrDefault());
                        CurrentRequest.ListItem.Add(CurrentItem);
                        CurrentRequest.UpdatedListItem.Add(CurrentItem);
                        CurrentItem.DeliveryAdress = CurrentDataContext.ListDeliveryLocation.FirstOrDefault(add => add.Name == CurrentItem.DeliveryAdress.Name);
                    }

                    var ListAttachement = _purchaseOrderService.GetAllPoAttachments(CurrentRequest.ID);
                    foreach (var attachment in ListAttachement)
                    {
                        CurrentRequest.ListAttachment.Add(PurchaseOrderAttachment.GetFromDbAttachement(attachment));
                        CurrentRequest.UpdatedListAttachment.Add(PurchaseOrderAttachment.GetFromDbAttachement(attachment));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void SearchAllDefaultListFromAllRequest()
        {
            try
            {
                CurrentDataContext.ListAllInternalOrder.Clear();
                foreach (var io in _purchaseOrderService.GetAllPoInternalOrders())
                    CurrentDataContext.ListAllInternalOrder.Add(PurchaseOrderInternalOrder.GetInternalOrderFromDbItem(io));

                //Internal Order
                var ListIO = CurrentDataContext.ListRequest.Select(item => item.InternalOrder?.IdIo).Distinct().ToList();
                List<PurchaseOrderInternalOrder> ListInternalOrder = new List<PurchaseOrderInternalOrder>();

                foreach (var InternalOrder in _purchaseOrderService.GetPoInternalOrdersByIds(ListIO))
                {
                    if (InternalOrder != null)
                        ListInternalOrder.Add(PurchaseOrderInternalOrder.GetInternalOrderFromDbItem(InternalOrder));
                }

                foreach (var io in ListInternalOrder.OrderBy(item => item.Description))
                    CurrentDataContext.ListInternalOrder.Add(io);

                //Internal Vendor
                var ListIdVendor = CurrentDataContext.ListRequest.Select(item => item.Vendor?.IdVendor).Distinct().ToList();
                List<PurchaseOrderVendor> ListVendor = new List<PurchaseOrderVendor>();

                foreach (var Vendor in _purchaseOrderService.GetPoVendorsByIds(ListIdVendor))
                {
                    if (Vendor != null)
                        ListVendor.Add(PurchaseOrderVendor.GetVendorFromDbItem(Vendor));
                }
                foreach (var vendor in ListVendor.OrderBy(item => item.Description))
                    CurrentDataContext.ListVendor.Add(vendor);

                // Material
                foreach (var item in _purchaseOrderService.GetAllPoMatDienNlags())
                    CurrentDataContext.ListDienNlagMaterial.Add(PurchaseOrderMaterial.GetMaterialFromDbItem(item));
                IsSearchDbDone = true;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void CreateUpdateRequestOnDataBase(PurchaseOrderRequest CurrentRequestToSave)
        {
            try
            {
                TraceLog.AddTraceLog($"start CreateUpdateRequestOnDataBase: Request:{CurrentRequestToSave.ID}");
                CurrentRequestToSave.ListAttachment.Clear();
                CurrentRequestToSave.ListItem.Clear();
                foreach (var item in CurrentRequestToSave.UpdatedListAttachment)
                {
                    CurrentRequestToSave.ListAttachment.Add(item);
                }
                foreach (var item in CurrentRequestToSave.UpdatedListItem)
                {
                    CurrentRequestToSave.ListItem.Add(item);
                }

                // Update Vendor DB
                PoVendor dbVendor = null;
                if (CurrentRequestToSave.Vendor != null && CurrentRequestToSave.Vendor.Number != null)
                    dbVendor = _purchaseOrderService.UpsertVendor(CurrentRequestToSave.Vendor.Number, CurrentRequestToSave.Vendor.Description, CurrentRequestToSave.Vendor.Location);

                if (dbVendor != null)
                {
                    CurrentRequestToSave.Vendor.IdVendor = dbVendor.Idvendor;
                    foreach (var item in CurrentRequestToSave.ListItem)
                        if (item.Vendor == null)
                            item.Vendor = new PurchaseOrderVendor() { IdVendor = dbVendor.Idvendor };
                        else
                            item.Vendor.IdVendor = dbVendor.Idvendor;
                }

                // Update Internal Order
                if (CurrentRequestToSave.InternalOrder != null)
                {
                    foreach (var item in CurrentRequestToSave.ListItem)
                        if (item.InternalOrder == null)
                            item.InternalOrder = new PurchaseOrderInternalOrder() { IdIo = CurrentRequestToSave.InternalOrder.IdIo };
                        else
                            item.InternalOrder.IdIo = CurrentRequestToSave.InternalOrder.IdIo;
                }

                // Create/update Request
                PoRequest dbRequestToSave = CurrentRequestToSave.GetDbrequest();
                if (dbRequestToSave.Idio == 0)
                    dbRequestToSave.Idio = _purchaseOrderService.GetPoInternalOrderByNumber(CurrentRequestToSave.InternalOrder.Number).FirstOrDefault()?.Idio ?? 0;

                var dbResult = _purchaseOrderService.UpsertRequest(dbRequestToSave);
                PoRequest DbRequest = dbResult.DbRequest;
                bool isNew = dbResult.IsNew;

                if (CurrentRequestToSave.Status == PurchaseOrderStatus.NEW || CurrentRequestToSave.Status == PurchaseOrderStatus.SENT)
                {
                    double TotalOrdered = 0;
                    foreach (var item in CurrentRequestToSave.ListItem)
                    {
                        TotalOrdered += item.Quantity * item.Price;
                        item.Total_Ordered = item.Quantity * item.Price;
                    }
                    CurrentRequestToSave.Total_Ordered = TotalOrdered;
                }

                if (isNew)
                    CurrentRequestToSave.ID = DbRequest.Idrequest;

                // Update List Attachment
                List<PurchaseOrderAttachment> CurrentListAttachement = CurrentRequestToSave.ListAttachment.ToList();
                foreach (var item in CurrentListAttachement)
                    item.IsDbSaved = false;

                List<PoAttachment> ListDbAttachment = _purchaseOrderService.GetAllPoAttachmentsForUpdate(CurrentRequestToSave.ID);

                foreach (var DbAttachement in ListDbAttachment)
                {
                    PurchaseOrderAttachment attachment = CurrentListAttachement.FirstOrDefault(item => DbAttachement.Idattachment == item.IdAttachment);
                    if (attachment != null)
                    {
                        attachment.UpdateDbAttachment(DbAttachement);
                        attachment.IsDbSaved = true;
                    }
                    else
                    {
                        _purchaseOrderService.DeletePoAttachment(DbAttachement.Idattachment);
                    }
                }
                foreach (var attachment in CurrentListAttachement.Where(item => !item.IsDbSaved))
                {
                    _purchaseOrderService.AddPoAttachment(attachment.GetDbAttachment(DbRequest.Idrequest));
                }

                // Update List Item 
                List<PoItem> listDbItemForUpdate = CurrentRequestToSave.ListItem.Select(item => item.GetDbItem(CurrentRequestToSave.ID)).ToList();
                _purchaseOrderService.ReplaceItemsForRequest(CurrentRequestToSave.ID, listDbItemForUpdate);

                TraceLog.AddTraceLog("End CreateUpdateRequestOnDataBase");
            }
            catch (Exception ex)
            {
                TraceLog.AddTraceLog("issue CreateUpdateRequestOnDataBase");
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void GetVendorFromRequestType(PurchaseOrderType RequestType, string Plant)
        {
            try
            {
                PurchaseOrderVendor vendorFromType = CurrentDataContext.ListPlantVendor.FirstOrDefault(item => item.RequestType == RequestType && item.Plant == Plant);
                if (vendorFromType != null)
                {
                    var dbVendor = _purchaseOrderService.GetPoVendorByNumber(vendorFromType.Number).FirstOrDefault();
                    if (dbVendor != null)
                    {
                        PurchaseOrderVendor VendorFromDb = PurchaseOrderVendor.GetVendorFromDbItem(dbVendor);
                        PurchaseOrderVendor SelectedVendor = CurrentDataContext.ListVendor.FirstOrDefault(item => item.Number == VendorFromDb.Number);

                        if (SelectedVendor == null)
                        {
                            CurrentDataContext.ListVendor.Add(VendorFromDb);
                        }
                        CurrentDataContext.CurrentRequest.Vendor = CurrentDataContext.ListVendor.FirstOrDefault(item => item.Number == VendorFromDb.Number);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void DeleteRequestFromDataBase(PurchaseOrderRequest deletedRequest)
        {
            try
            {
                _purchaseOrderService.DeleteRequestCompletely(deletedRequest.ID);

                CurrentDataContext.ListAllRequest.Remove(deletedRequest);
                UpdateRequestList();
                UpdateCharts();
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private bool CheckIfRequestAlreadyExist(string Number, string From)
        {
            try
            {
                if (From == "PO")
                {
                    PoRequest temp = _purchaseOrderService.GetPoRequestFromSapPo(Number);
                    return temp != null;
                }
                else if (From == "PR")
                {
                    PoRequest temp = _purchaseOrderService.GetPoRequestFromSapPr(Number);
                    return temp != null;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] SAP Hup Methods
        private void SearchSapHubVendor()
        {
            try
            {
                var tempList = new List<PurchaseOrderVendor>();

                string number = CurrentDataContext.NumberSearchField?.Trim();
                string desc = CurrentDataContext.DescriptionSearchField?.Trim();

                if (string.IsNullOrWhiteSpace(number) && string.IsNullOrWhiteSpace(desc))
                {
                    CurrentDataContext.ListItem = tempList;
                    return;
                }

                List<VendorSearchResult> vendors;

                if (!string.IsNullOrWhiteSpace(number))
                {
                    vendors = _sapHupService.SearchVendors(number, searchByName: false);
                }
                else
                {
                    var terms = desc
                        .Replace("*", " ")
                        .Split(new[] { ' ', '|', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToArray();

                    if (terms.Length == 0)
                    {
                        CurrentDataContext.ListItem = tempList;
                        return;
                    }

                    vendors = _sapHupService.SearchVendors(terms[0], searchByName: true);

                    for (int i = 1; i < terms.Length && vendors.Count > 0; i++)
                    {
                        var next = _sapHupService.SearchVendors(terms[i], searchByName: true);
                        var nextSet = new HashSet<string>(next.Select(v => v.LIFNR));
                        vendors = vendors.Where(v => nextSet.Contains(v.LIFNR)).ToList();
                    }
                }

                bool maxReached = vendors.Count > 100;

                foreach (var v in vendors.Take(100))
                {
                    tempList.Add(new PurchaseOrderVendor
                    {
                        Description = v.NAME1,
                        Number = v.LIFNR,
                        Location = v.LAND1,
                        Society = v.BUKRS,
                        City = v.ORT01
                    });
                }

                CurrentDataContext.ListItem = tempList;

                if (maxReached)
                    RaiseMaxRowSearchedEvent();
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private PurchaseOrderRequest SearchSapHubRequestFromPurchaseRequest(string SapPrNumber)
        {
            try
            {
                PurchaseOrderRequest CurrentRequest = new PurchaseOrderRequest();

                PurchaseOrderItem CurrentItem = null;
                CurrentRequest.SapPurchaseRequest = SapPrNumber;
                bool FirstItem = true;
                double Total_Ordered = 0;

                var dbResult = _sapHupService.GetAllItemsFromPR(SapPrNumber);
                foreach (var dbItem in dbResult)
                {
                    CurrentItem = new PurchaseOrderItem()
                    {
                        AccountAssignementCategory = dbItem.KNTTP,
                        CostCenter = GetPurchaseOrderCostCenterFromNumber(dbItem.KOSTL),
                        InternalOrder = GetPurchaseOrderInternalOrderFromNumber(dbItem.AUFNR),
                        Number = Convert.ToInt32(dbItem.BNFPO),
                        Material = dbItem.MATNR,
                        MaterialNumber = GetPurchaseOrderMaterialFromNumber(dbItem.MATNR),
                        Description = dbItem.TXZ01,
                        DeliveryDate = dbItem.LFDAT.HasValue ? DateOnly.FromDateTime(dbItem.LFDAT.Value) : (DateOnly?)null,
                        DeliveryPlant = new SapPlant() { Number = dbItem.BERID },
                        DeliveryAdress = GetPurchaseOrderLocationFromNumber(dbItem.BERID),
                        Price = dbItem.PREIS.HasValue ? (double)dbItem.PREIS.Value : 0,
                        Quantity = dbItem.MENGE.HasValue ? (double)dbItem.MENGE.Value : 0,
                        RequestedBy = dbItem.AFNAM,
                        Vendor = GetPurchaseOrderVendorFromNumber(dbItem.FLIEF),
                        Total_Ordered = dbItem.RLWRT.HasValue ? (double)dbItem.RLWRT.Value : 0
                    };


                    Total_Ordered += CurrentItem.Total_Ordered;
                    CurrentRequest.ListItem.Add(CurrentItem);
                    CurrentRequest.UpdatedListItem.Add(CurrentItem);

                    if (FirstItem)
                    {
                        CurrentRequest.SapCreatedBy = dbItem.ERNAM;
                        CurrentRequest.Status = PurchaseOrderStatus.PO_CREATED;
                        CurrentRequest.SapPurchaseRequest = dbItem.BANFN;
                        CurrentRequest.CostCenter = CurrentItem.CostCenter;
                        CurrentRequest.Vendor = CurrentItem.Vendor;
                        CurrentRequest.InternalOrder = CurrentItem.InternalOrder;
                        CurrentRequest.RequestedBy = CurrentItem.RequestedBy;
                        CurrentRequest.CreatedBy = CurrentItem.RequestedBy;
                        CurrentRequest.RequestType = GetPurchaseOrderTypeFromPrPoType(dbItem.EBAN_BSART, "", dbItem.FLIEF);
                        CurrentRequest.SapCreatedOn = dbItem.ERDAT.HasValue ? DateOnly.FromDateTime(dbItem.ERDAT.Value) : (DateOnly?)null;
                        CurrentRequest.CreatedOn = dbItem.ERDAT.HasValue ? DateOnly.FromDateTime(dbItem.ERDAT.Value) : (DateOnly?)null;
                        FirstItem = false;
                    }
                }

                CurrentRequest.Total_Ordered = Total_Ordered;

                return CurrentRequest;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private PurchaseOrderRequest SearchSapHubRequestFromPurchaseOrder(string SapPoNumber)
        {
            try
            {
                PurchaseOrderRequest CurrentRequest = new PurchaseOrderRequest();
                PurchaseOrderItem CurrentItem = null;
                CurrentRequest.SapPurchaseOrder = SapPoNumber;
                bool FirstItem = true;
                double Total_Ordered = 0;

                var dbResult = _sapHupService.GetAllItemsFromPO(SapPoNumber);

                foreach (var dbItem in dbResult)
                {
                    CurrentItem = new PurchaseOrderItem()
                    {
                        AccountAssignementCategory = dbItem.KNTTP,
                        CostCenter = GetPurchaseOrderCostCenterFromNumber(dbItem.KOSTL),
                        InternalOrder = GetPurchaseOrderInternalOrderFromNumber(dbItem.AUFNR),
                        Number = Convert.ToInt32(dbItem.BNFPO),
                        Material = dbItem.MATNR,
                        MaterialNumber = GetPurchaseOrderMaterialFromNumber(dbItem.MATNR),
                        Description = dbItem.TXZ01,
                        DeliveryDate = dbItem.LFDAT.HasValue ? DateOnly.FromDateTime(dbItem.LFDAT.Value) : (DateOnly?)null,
                        DeliveryPlant = new SapPlant() { Number = dbItem.BERID },
                        DeliveryAdress = GetPurchaseOrderLocationFromNumber(dbItem.BERID),
                        Price = dbItem.PREIS.HasValue ? (double)dbItem.PREIS.Value : 0,
                        Quantity = dbItem.MENGE.HasValue ? (double)dbItem.MENGE.Value : 0,
                        RequestedBy = dbItem.AFNAM,
                        Vendor = GetPurchaseOrderVendorFromNumber(dbItem.FLIEF),
                        Total_Ordered = dbItem.NETWR.HasValue ? (double)dbItem.NETWR.Value : 0
                    };

                    Total_Ordered += CurrentItem.Total_Ordered;
                    CurrentRequest.ListItem.Add(CurrentItem);
                    CurrentRequest.UpdatedListItem.Add(CurrentItem);

                    if (FirstItem)
                    {
                        CurrentRequest.SapCreatedBy = dbItem.ERNAM;
                        CurrentRequest.Status = PurchaseOrderStatus.PO_CREATED;
                        CurrentRequest.SapPurchaseRequest = dbItem.BANFN;
                        CurrentRequest.CostCenter = CurrentItem.CostCenter;
                        CurrentRequest.Vendor = CurrentItem.Vendor;
                        CurrentRequest.InternalOrder = CurrentItem.InternalOrder;
                        CurrentRequest.RequestedBy = CurrentItem.RequestedBy;
                        CurrentRequest.CreatedBy = CurrentItem.RequestedBy;
                        CurrentRequest.RequestType = GetPurchaseOrderTypeFromPrPoType(dbItem.EBAN_BSART, dbItem.EKKO_BSART, dbItem.FLIEF);
                        CurrentRequest.SapCreatedOn = dbItem.AEDAT.HasValue ? DateOnly.FromDateTime(dbItem.AEDAT.Value) : (DateOnly?)null;
                        CurrentRequest.CreatedOn = dbItem.AEDAT.HasValue ? DateOnly.FromDateTime(dbItem.AEDAT.Value) : (DateOnly?)null;
                        FirstItem = false;
                    }
                }

                CurrentRequest.Total_Ordered = Total_Ordered;
                return CurrentRequest;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private PurchaseOrderRequest SearchSapHubRequestFromPurchaseOrderWithoutPurchaseResquest(string SapPoNumber)
        {
            try
            {
                PurchaseOrderRequest CurrentRequest = new PurchaseOrderRequest();
                PurchaseOrderItem CurrentItem = null;
                CurrentRequest.SapPurchaseOrder = SapPoNumber;
                bool FirstItem = true;
                double Total_Ordered = 0;

                var dbResult = _sapHupService.GetAllItemsFromPOWithoutPR(SapPoNumber);

                foreach (var dbItem in dbResult)
                {
                    CurrentItem = new PurchaseOrderItem()
                    {
                        AccountAssignementCategory = dbItem.KNTTP,
                        CostCenter = GetPurchaseOrderCostCenterFromNumber(dbItem.KOSTL),
                        InternalOrder = GetPurchaseOrderInternalOrderFromNumber(dbItem.AUFNR),
                        Number = Convert.ToInt32(dbItem.EBELP),
                        Material = dbItem.MATNR,
                        MaterialNumber = GetPurchaseOrderMaterialFromNumber(dbItem.MATNR),
                        Description = dbItem.TXZ01,
                        DeliveryDate = dbItem.EILDT.HasValue ? DateOnly.FromDateTime(dbItem.EILDT.Value) : (DateOnly?)null,
                        DeliveryPlant = new SapPlant() { Number = dbItem.WERKS },
                        DeliveryAdress = GetPurchaseOrderLocationFromNumber(dbItem.WERKS),
                        Price = dbItem.NETPR.HasValue ? (double)dbItem.NETPR.Value : 0,
                        Quantity = dbItem.MENGE.HasValue ? (double)dbItem.MENGE.Value : 0,
                        RequestedBy = dbItem.ERNAM,
                        Vendor = GetPurchaseOrderVendorFromNumber(dbItem.LIFNR),
                        Total_Ordered = dbItem.NETWR.HasValue ? (double)dbItem.NETWR.Value : 0,
                    };

                    Total_Ordered += CurrentItem.Total_Ordered;
                    CurrentRequest.ListItem.Add(CurrentItem);
                    CurrentRequest.UpdatedListItem.Add(CurrentItem);
                    if (CurrentItem.InternalOrder == null)
                        CurrentItem.InternalOrder = new PurchaseOrderInternalOrder()
                        {
                            Number = dbItem.AUFNR,
                            Description = $"{dbItem.AUFNR}NO DESCRIPTION"
                        };

                    if (FirstItem)
                    {
                        CurrentRequest.SapCreatedBy = dbItem.ERNAM;
                        CurrentRequest.Status = PurchaseOrderStatus.NEW;
                        CurrentRequest.SapPurchaseRequest = "";
                        CurrentRequest.CostCenter = CurrentItem.CostCenter;
                        CurrentRequest.Vendor = CurrentItem.Vendor;
                        CurrentRequest.InternalOrder = CurrentItem.InternalOrder;
                        CurrentRequest.RequestedBy = CurrentItem.RequestedBy;
                        CurrentRequest.CreatedBy = CurrentItem.RequestedBy;
                        CurrentRequest.RequestType = GetPurchaseOrderTypeFromPrPoType("", dbItem.BSART, "");
                        CurrentRequest.SapCreatedOn = dbItem.AEDAT.HasValue ? DateOnly.FromDateTime(dbItem.AEDAT.Value) : (DateOnly?)null;
                        CurrentRequest.CreatedOn = dbItem.AEDAT.HasValue ? DateOnly.FromDateTime(dbItem.AEDAT.Value) : (DateOnly?)null;
                        FirstItem = false;
                    }
                }

                CurrentRequest.Total_Ordered = Total_Ordered;
                return CurrentRequest;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private List<PurchaseOrderRequest> SearchSapHubRequestFromPurchaseRequestFromDates(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                TraceLog.AddTraceLog("start SearchSapHubRequestFromPurchaseRequestFromDates");
                List<PurchaseOrderRequest> ListRequest = new List<PurchaseOrderRequest>();

                if (CurrentDataContext.PoCreatedAfter != null && CurrentDataContext.PoCreatedBefore != null && CurrentDataContext.PoCreatedAfter <= CurrentDataContext.PoCreatedBefore)
                {
                    TraceLog.AddTraceLog($"start request:  GetPurchaseOrderBetweenDates(DateTime startDate, DateTime endDate)");

                    var tempListPo = _sapHupService.GetPurchaseOrderBetweenDates(FromDate, ToDate);
                    if (tempListPo == null || tempListPo.Count == 0)
                    {
                        TraceLog.AddTraceLog($"no purchase order found between dates: {FromDate.ToString("dd-MM-yyyy")} and {ToDate.ToString("dd-MM-yyyy")}");
                        return ListRequest;
                    }
                    foreach (var item in tempListPo)
                    {
                        string SapPurchaseOrder = item.EBELN;

                        PurchaseOrderRequest CurrentRequest = ListRequest.FirstOrDefault(x => x.SapPurchaseOrder == SapPurchaseOrder);
                        string vendorNumber = (!string.IsNullOrWhiteSpace(item.FLIEF)) ? item.FLIEF : item.LIFNR;

                        PurchaseOrderItem CurrentItem = new PurchaseOrderItem()
                        {
                            AccountAssignementCategory = item.KNTTP,
                            CostCenter = GetPurchaseOrderCostCenterFromNumber(item.KOSTL),
                            InternalOrder = GetPurchaseOrderInternalOrderFromNumber(item.AUFNR),
                            Number = Convert.ToInt32(item.BNFPO),
                            Material = item.MATNR,
                            MaterialNumber = GetPurchaseOrderMaterialFromNumber(item.MATNR),
                            Description = item.TXZ01,
                            DeliveryDate = item.LFDAT.HasValue ? DateOnly.FromDateTime(item.LFDAT.Value) : (DateOnly?)null,
                            DeliveryPlant = new SapPlant() { Number = item.BERID },
                            DeliveryAdress = GetPurchaseOrderLocationFromNumber(item.BERID),
                            Price = item.PREIS.HasValue ? (double)item.PREIS.Value : 0,
                            Quantity = item.MENGE.HasValue ? (double)item.MENGE.Value : 0,
                            RequestedBy = item.AFNAM,
                            Vendor = GetPurchaseOrderVendorFromNumber(vendorNumber),
                        };


                        if (CurrentRequest == null)
                        {
                            CurrentRequest = new PurchaseOrderRequest() { SapPurchaseOrder = SapPurchaseOrder };
                            ListRequest.Add(CurrentRequest);

                            CurrentRequest.SapCreatedBy = item.ERNAM;
                            CurrentRequest.Status = PurchaseOrderStatus.PO_CREATED;
                            CurrentRequest.SapPurchaseRequest = item.BANFN;
                            CurrentRequest.CostCenter = CurrentItem.CostCenter;
                            CurrentRequest.Vendor = CurrentItem.Vendor;
                            CurrentRequest.InternalOrder = CurrentItem.InternalOrder;
                            CurrentRequest.RequestedBy = CurrentItem.RequestedBy;
                            CurrentRequest.CreatedBy = CurrentItem.RequestedBy;
                            CurrentRequest.RequestType = GetPurchaseOrderTypeFromPrPoType(item.EBAN_BSART, item.EKKO_BSART, item.FLIEF);
                            if (!CurrentRequest.IsAlreadyExist)
                                TraceLog.AddTraceLog($"request type found: {item.EBAN_BSART}. return:{CurrentRequest.RequestType}");
                            CurrentRequest.SapCreatedOn = item.AEDAT.HasValue ? DateOnly.FromDateTime(item.AEDAT.Value) : (DateOnly?)null;
                            CurrentRequest.CreatedOn = item.AEDAT.HasValue ? DateOnly.FromDateTime(item.AEDAT.Value) : (DateOnly?)null;
                            CurrentRequest.IsAlreadyExist = CurrentDataContext.ListAllRequest.Any(item => item.SapPurchaseOrder == CurrentRequest.SapPurchaseOrder || (item.SapPurchaseRequest != null && item.SapPurchaseRequest == CurrentRequest.SapPurchaseRequest));
                        }

                        if (CurrentRequest.RequestType == PurchaseOrderType.ZRMI)
                        {
                            PurchaseOrderMaterial CurrentMaterial = CurrentDataContext.ListDienNlagMaterial.FirstOrDefault(mat => mat.Number == CurrentItem.Material);
                            if (CurrentMaterial == null)
                                CurrentItem.SelectedMaterial = CurrentItem.MaterialNumber;
                            else
                                CurrentItem.SelectedMaterial = CurrentMaterial;
                        }

                        CurrentRequest.ListItem.Add(CurrentItem);
                        CurrentRequest.UpdatedListItem.Add(CurrentItem);
                    }
                }
                TraceLog.AddTraceLog("end SearchSapHubRequestFromPurchaseRequestFromDates");

                return ListRequest;
            }
            catch (Exception ex)
            {
                TraceLog.AddTraceLog("issue SearchSapHubRequestFromPurchaseRequestFromDates");
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private List<PurchaseOrderRequest> SearchSapHubRequestFromPurchaseOrderFromDates(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                List<PurchaseOrderRequest> ListRequest = new List<PurchaseOrderRequest>();

                if (!(CurrentDataContext.PoCreatedAfter == null) && !(CurrentDataContext.PoCreatedBefore == null) && (CurrentDataContext.PoCreatedAfter <= CurrentDataContext.PoCreatedBefore))
                {
                    PurchaseOrderRequest CurrentRequest;

                    PurchaseOrderItem CurrentItem = null;

                    string SapPurchaseOrder;

                    var tempListPo = _sapHupService.GetSearchPurchaseOrderBetweenDates(FromDate, ToDate);

                    foreach (var dbItem in tempListPo)
                    {
                        SapPurchaseOrder = dbItem.EBELN;
                        CurrentRequest = ListRequest.FirstOrDefault(x => x.SapPurchaseOrder == SapPurchaseOrder);

                        CurrentItem = new PurchaseOrderItem()
                        {
                            AccountAssignementCategory = dbItem.KNTTP,
                            CostCenter = GetPurchaseOrderCostCenterFromNumber(dbItem.KOSTL),
                            InternalOrder = GetPurchaseOrderInternalOrderFromNumber(dbItem.AUFNR),
                            Number = Convert.ToInt32(dbItem.EBELP),
                            Material = dbItem.MATNR,
                            MaterialNumber = GetPurchaseOrderMaterialFromNumber(dbItem.MATNR),
                            Description = dbItem.TXZ01,
                            DeliveryDate = dbItem.AEDAT.HasValue ? DateOnly.FromDateTime(dbItem.AEDAT.Value) : (DateOnly?)null,
                            DeliveryPlant = new SapPlant() { Number = dbItem.BERID },
                            DeliveryAdress = GetPurchaseOrderLocationFromNumber(dbItem.BERID),
                            Price = dbItem.NETPR.HasValue ? (double)dbItem.NETPR.Value : 0,
                            Quantity = dbItem.MENGE.HasValue ? (double)dbItem.MENGE.Value : 0,
                            RequestedBy = dbItem.AFNAM,
                            Vendor = GetPurchaseOrderVendorFromNumber(dbItem.LIFNR),
                        };

                        if (CurrentRequest == null)
                        {
                            CurrentRequest = new PurchaseOrderRequest() { SapPurchaseOrder = SapPurchaseOrder };
                            CurrentRequest.IsAlreadyExist = CurrentDataContext.ListAllRequest.Any(item => item.SapPurchaseOrder == CurrentRequest.SapPurchaseOrder);
                            ListRequest.Add(CurrentRequest);

                            CurrentRequest.SapCreatedBy = dbItem.ERNAM;
                            CurrentRequest.Status = PurchaseOrderStatus.PO_CREATED;
                            CurrentRequest.SapPurchaseRequest = dbItem.BANFN;
                            CurrentRequest.CostCenter = CurrentItem.CostCenter;
                            CurrentRequest.Vendor = CurrentItem.Vendor;
                            CurrentRequest.InternalOrder = CurrentItem.InternalOrder;
                            CurrentRequest.RequestedBy = CurrentItem.RequestedBy;
                            CurrentRequest.CreatedBy = CurrentItem.RequestedBy;
                            CurrentRequest.RequestType = GetPurchaseOrderTypeFromPrPoType("", dbItem.EKKO_BSART, dbItem.FLIEF);
                            CurrentRequest.SapCreatedOn = dbItem.AEDAT.HasValue ? DateOnly.FromDateTime(dbItem.AEDAT.Value) : (DateOnly?)null;
                            CurrentRequest.CreatedOn = dbItem.AEDAT.HasValue ? DateOnly.FromDateTime(dbItem.AEDAT.Value) : (DateOnly?)null;
                        }
                        CurrentRequest.ListItem.Add(CurrentItem);
                        CurrentRequest.UpdatedListItem.Add(CurrentItem);
                    }
                }
                return ListRequest;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateRequestFromSapHubPurchaseRequest(PurchaseOrderRequest currentRequest)
        {
            if (currentRequest == null || string.IsNullOrEmpty(currentRequest.SapPurchaseRequest))
                return;
            try
            {
                TraceLog.AddTraceLog($"SAP Request PR: SqlRequestAllItemsFromPR");

                int itemNumber = 0;
                double totalOrdered = 0;
                bool firstItem = true;
                string site;

                var dbResult = _sapHupService.GetAllItemsFromPR(currentRequest.SapPurchaseRequest);

                foreach (var dbItem in dbResult)
                {
                    itemNumber = Convert.ToInt32(dbItem.BNFPO);
                    PurchaseOrderItem currentItem = currentRequest.ListItem.FirstOrDefault(item => item.Number == itemNumber);
                    if (currentItem != null)
                    {
                        if (currentItem.DeliveryAdress == null)
                        {
                            site = dbItem.BERID;
                            currentItem.DeliveryAdress = GetSiteLocation(site);
                        }
                        currentItem.Total_Ordered = dbItem.RLWRT.HasValue ? (double)dbItem.RLWRT.Value : 0;
                        currentItem.Price = dbItem.PREIS.HasValue ? (double)dbItem.PREIS.Value : 0;
                        currentItem.Quantity = dbItem.MENGE.HasValue ? (double)dbItem.MENGE.Value : 0;

                        currentItem.CostCenter = GetPurchaseOrderCostCenterFromNumber(dbItem.KOSTL);
                        currentItem.InternalOrder = GetPurchaseOrderInternalOrderFromNumber(dbItem.AUFNR);
                        string vendorNumber = (!string.IsNullOrWhiteSpace(dbItem.FLIEF)) ? dbItem.FLIEF : dbItem.LIFNR;
                        currentItem.Vendor = GetPurchaseOrderVendorFromNumber(vendorNumber);
                        totalOrdered += currentItem.Total_Ordered;
                    }

                    if (firstItem)
                    {
                        currentRequest.SapCreatedBy = dbItem.ERNAM;
                        currentRequest.SapCreatedOn = dbItem.ERDAT.HasValue ? DateOnly.FromDateTime(dbItem.ERDAT.Value) : (DateOnly?)null;
                        currentRequest.CostCenter = GetPurchaseOrderCostCenterFromNumber(dbItem.KOSTL);
                        currentRequest.InternalOrder = GetPurchaseOrderInternalOrderFromNumber(dbItem.AUFNR);
                        string vendorNumber = (!string.IsNullOrWhiteSpace(dbItem.FLIEF)) ? dbItem.FLIEF : dbItem.LIFNR;
                        currentRequest.Vendor = GetPurchaseOrderVendorFromNumber(vendorNumber);
                        if (string.IsNullOrEmpty(currentRequest.SapPurchaseOrder))
                            currentRequest.Status = PurchaseOrderStatus.PR_CREATED;
                        else
                            currentRequest.Status = PurchaseOrderStatus.PO_CREATED;

                        firstItem = false;
                    }

                    if (string.IsNullOrEmpty(currentRequest.SapPurchaseOrder?.Trim()) && !string.IsNullOrEmpty(dbItem.EBELN?.Trim()))
                    {
                        currentRequest.SapPurchaseOrder = dbItem.EBELN;
                    }
                }
                currentRequest.Total_Ordered = totalOrdered;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private PurchaseOrderLocation GetSiteLocation(string siteLocationNumber)
        {
            try
            {
                switch (siteLocationNumber)
                {
                    case "1011":
                        return new PurchaseOrderLocation() { Name = "MLS", Number = siteLocationNumber };
                    case "1012":
                        return new PurchaseOrderLocation() { Name = "CHL", Number = siteLocationNumber };
                    case "1015":
                        return new PurchaseOrderLocation() { Name = "LSY-PVC", Number = siteLocationNumber };
                    case "1050":
                        return new PurchaseOrderLocation() { Name = "NIE", Number = siteLocationNumber };
                    case "1000":
                        return new PurchaseOrderLocation() { Name = "PTO", Number = siteLocationNumber };
                    case "2010":
                        return new PurchaseOrderLocation() { Name = "SPC", Number = siteLocationNumber };
                    case "1070":
                        return new PurchaseOrderLocation() { Name = "CHINA", Number = siteLocationNumber };
                    case "8010":
                        return new PurchaseOrderLocation() { Name = "DDY", Number = siteLocationNumber };
                    default:
                        return new PurchaseOrderLocation() { Name = "UNKNOWN", Number = siteLocationNumber };
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateRequestFromSapHubPurchaseOrder(PurchaseOrderRequest currentRequest)
        {
            if (currentRequest == null || string.IsNullOrEmpty(currentRequest.SapPurchaseOrder))
                return;

            try
            {
                TraceLog.AddTraceLog($"SAP Request PO: SqlRequestAllItemsFromPO");

                int itemNumber = 0;
                double totalOrdered = 0;
                bool firstItem = true;
                string site;

                var dbResult = _sapHupService.GetAllItemsFromPO(currentRequest.SapPurchaseOrder);

                foreach (var dbItem in dbResult)
                {
                    itemNumber = Convert.ToInt32(dbItem.EBELP);
                    PurchaseOrderItem currentItem = currentRequest.ListItem.FirstOrDefault(item => item.Number == itemNumber);
                    if (currentItem != null)
                    {
                        if (currentItem.DeliveryAdress == null)
                        {
                            site = dbItem.BERID;
                            currentItem.DeliveryAdress = GetSiteLocation(site);
                        }
                        currentItem.Total_Ordered = dbItem.NETWR.HasValue ? (double)dbItem.NETWR.Value : 0;
                        currentItem.Price = dbItem.PREIS.HasValue ? (double)dbItem.PREIS.Value : 0;
                        currentItem.Quantity = dbItem.MENGE.HasValue ? (double)dbItem.MENGE.Value : 0;
                        totalOrdered += currentItem.Total_Ordered;
                    }

                    if (firstItem)
                    {
                        currentRequest.Status = PurchaseOrderStatus.PO_CREATED;
                        firstItem = false;
                    }
                }

                if (totalOrdered > 0)
                    currentRequest.Total_Ordered = totalOrdered;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateRequestFromSapHubPurchaseOrderGoodsInvoiceReceipt(PurchaseOrderRequest currentRequest)
        {
            if (currentRequest == null || string.IsNullOrEmpty(currentRequest.SapPurchaseOrder))
                return;

            try
            {
                TraceLog.AddTraceLog($"SAP Request GR: SqlRequestUpdateRequestFromPO");

                var sapItemList = new List<PurchaseOrderItemSapHubInformation>();

                var dbResult = _sapHupService.GetUpdateRequestFromPO(currentRequest.SapPurchaseOrder);
                foreach (var dbItem in dbResult)
                {
                    var currentSapItem = new PurchaseOrderItemSapHubInformation()
                    {
                        OperationStatus = GetOperationStatusFromNumber(dbItem.VGABE),
                        Number = Convert.ToInt32(dbItem.EBELP),
                        SapPurchaseOrder = dbItem.EBELN,
                        Quantity_GR = dbItem.MENG_GR.HasValue ? (double)dbItem.MENG_GR.Value : 0,
                        Quantity_Ordered = dbItem.MENGE_ORDER.HasValue ? (double)dbItem.MENGE_ORDER.Value : 0,
                        Price_GR = dbItem.DMBTR.HasValue ? (double)dbItem.DMBTR.Value : 0,
                        Price_Ordered = dbItem.NETWR.HasValue ? (double)dbItem.NETWR.Value : 0,
                        Price_Paid = dbItem.DMBTR.HasValue ? (double)dbItem.DMBTR.Value : 0,
                        CreditDebit_Info = dbItem.SHKZG,
                        Canceled_Value = dbItem.AREWR.HasValue ? (double)dbItem.AREWR.Value : 0,
                        Mouvement_Type = dbItem.BEWTP,
                        Closed_Check = GetCheckValue(dbItem.ELIKZ),
                    };
                    if (currentSapItem.CreditDebit_Info == "H")
                        currentSapItem.Price_Paid = currentSapItem.Price_Paid * -1;
                    sapItemList.Add(currentSapItem);
                }

                currentRequest.Total_Invoice = 0;
                currentRequest.Total_Goods = 0;
                currentRequest.Total_Real_Goods = 0;

                foreach (var item in currentRequest.ListItem)
                {
                    item.GoodReceiptStatus = PurchaseOrderStatus.CREATED;

                    var tempGoodsReceiptList = sapItemList.Where(info => info.Number == item.Number && info.OperationStatus == PurchaseOrderStatus.GOODS_RECEIPT).ToList();
                    if (tempGoodsReceiptList.Count > 0)
                    {
                        item.Total_Ordered = tempGoodsReceiptList.FirstOrDefault().Price_Ordered;
                        item.Total_Goods = tempGoodsReceiptList.Where(info => info.CreditDebit_Info == "S").Sum(info => info.Price_GR) - tempGoodsReceiptList.Where(info => info.CreditDebit_Info == "H").Sum(info => info.Price_GR);

                        if (tempGoodsReceiptList.FirstOrDefault(obj => obj.Closed_Check) != null)
                            item.Closed_Check = true;

                        if (item.Total_Goods >= tempGoodsReceiptList.FirstOrDefault().Price_Ordered || item.Closed_Check)
                            item.GoodReceiptStatus = PurchaseOrderStatus.GOODS_RECEIPT;
                        else
                            item.GoodReceiptStatus = PurchaseOrderStatus.PARTIAL_GOODS_RECEIPT;
                    }

                    var tempInvoiceReceiptList = sapItemList.Where(info => info.Number == item.Number && info.OperationStatus == PurchaseOrderStatus.INVOICE_RECEIPT).ToList();
                    if (tempInvoiceReceiptList.Count > 0)
                    {
                        item.Total_Ordered = tempInvoiceReceiptList.FirstOrDefault().Price_Ordered;
                        item.Total_Invoice = tempInvoiceReceiptList.Sum(info => info.Price_Paid);
                        item.Total_Real_Goods = item.Total_Goods - tempInvoiceReceiptList.Where(obj => obj.Mouvement_Type == "K").Sum(info => info.Canceled_Value);

                        if (item.Total_Invoice >= item.Total_Real_Goods && item.Closed_Check)
                            //if (item.Total_Invoice >= item.Total_Real_Goods)
                            item.GoodReceiptStatus = PurchaseOrderStatus.INVOICE_RECEIPT;
                        else
                            item.GoodReceiptStatus = PurchaseOrderStatus.PARTIAL_INVOICE_RECEIPT;
                    }
                }

                currentRequest.Total_Goods = currentRequest.ListItem.Sum(obj => obj.Total_Goods);
                currentRequest.Total_Real_Goods = currentRequest.ListItem.Sum(obj => obj.Total_Real_Goods);
                currentRequest.Total_Invoice = currentRequest.ListItem.Sum(obj => obj.Total_Invoice);

                UdpateRequestStatus(currentRequest);
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UdpateRequestStatus(PurchaseOrderRequest CurrentRequest)
        {
            try
            {
                PurchaseOrderStatus CurrentStatus = CurrentRequest.Status;
                if (CurrentRequest.ListItem.Count == CurrentRequest.ListItem.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.INVOICE_RECEIPT))
                    CurrentRequest.Status = PurchaseOrderStatus.CLOSED;
                else if (CurrentRequest.ListItem.Count == CurrentRequest.ListItem.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.GOODS_RECEIPT))
                    CurrentRequest.Status = PurchaseOrderStatus.GOODS_RECEIPT;
                else if (CurrentRequest.ListItem.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.INVOICE_RECEIPT) > 0)
                    CurrentRequest.Status = PurchaseOrderStatus.PARTIAL_INVOICE_RECEIPT;
                else if (CurrentRequest.ListItem.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.PARTIAL_INVOICE_RECEIPT) > 0)
                    CurrentRequest.Status = PurchaseOrderStatus.PARTIAL_INVOICE_RECEIPT;
                else if (CurrentRequest.ListItem.Count == CurrentRequest.ListItem.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.GOODS_RECEIPT))
                    CurrentRequest.Status = PurchaseOrderStatus.GOODS_RECEIPT;
                else if (CurrentRequest.ListItem.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.GOODS_RECEIPT) > 0)
                    CurrentRequest.Status = PurchaseOrderStatus.PARTIAL_GOODS_RECEIPT;
                else if (CurrentRequest.ListItem.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.PARTIAL_GOODS_RECEIPT) > 0)
                    CurrentRequest.Status = PurchaseOrderStatus.PARTIAL_GOODS_RECEIPT;

            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private PurchaseOrderStatus GetOperationStatusFromNumber(string v)
        {
            try
            {
                switch (v)
                {
                    case "1":
                        return PurchaseOrderStatus.GOODS_RECEIPT;

                    case "2":
                        return PurchaseOrderStatus.INVOICE_RECEIPT;

                    default:
                        return PurchaseOrderStatus.UNKNOWN;
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private bool GetCheckValue(string v)
        {
            try
            {
                switch (v)
                {
                    case "X":
                        return true;

                    case "":
                        return false;

                    case " ":
                        return false;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public void CreateSapRequestFromDate(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                DateTime fromDate = from ?? DateTime.Today.AddDays(-15);
                DateTime toDate = to ?? DateTime.Today;


                //fromDate = DateTime.Today.AddDays(-7);
                var listReq = SearchSapHubRequestFromPurchaseRequestFromDates(fromDate, toDate)
                    .Where(item => !item.IsAlreadyExist)
                    .ToList();
                foreach (var item in listReq)
                {
                    UpdateListFromRequest(item);
                    CreateUpdateRequestOnDataBase(item);
                }

            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Other Methods
        private void SendExtendCheckExtendPart()
        {
            try
            {
                UpdateRequestItemAttachment(CurrentDataContext.SelectedRequest);

                var listFromDb = _sapHupService.GetListMaterialMasterViewsByPlant(CurrentDataContext.SelectedRequest.ListItem.Select(item => item.MaterialNumber?.Number).ToList());
                List<SAPMaterialMaster> list = listFromDb.Select(r => new SAPMaterialMaster(r)).ToList();

                foreach (var purchaseItem in CurrentDataContext.SelectedRequest.ListItem)
                {
                    // check Purchasing View Status
                    // Remplacez cette ligne incorrecte :
                    if (list.Any(item => item.PartNumber == purchaseItem.MaterialNumber?.Number && item.PlantNumber == purchaseItem.DeliveryAdress?.Number && item.PlantViews.Contains("E")))
                    {
                        purchaseItem.PurchasingViewStatus = PurchaseOrderStatus.CREATED;
                    }
                    else
                    {
                        purchaseItem.PurchasingViewStatus = PurchaseOrderStatus.TO_BE_CREATED;
                    }

                    // check Mrp View Status
                    //if (list.Any(item => item.PartNumber == purchaseItem.MaterialNumber.Number && item.PlantNumber == purchaseItem.DeliveryAdress?.Number && item.PlantViews.Contains("V")))
                    if (list.Any(item => item.PartNumber == purchaseItem.MaterialNumber.Number && item.PlantNumber == purchaseItem.DeliveryAdress?.Number && item.PlantViews.Contains("D")))
                    {
                        purchaseItem.MrpViewStatus = PurchaseOrderStatus.CREATED;
                    }
                    else
                    {
                        purchaseItem.MrpViewStatus = PurchaseOrderStatus.TO_BE_CREATED;
                    }

                    // check Storage View Status
                    if (list.Any(item => item.PartNumber == purchaseItem.MaterialNumber.Number && item.PlantNumber == purchaseItem.DeliveryAdress?.Number && item.PlantViews.Contains("L")))
                    {
                        purchaseItem.StorageViewStatus = PurchaseOrderStatus.CREATED;
                    }
                    else
                    {
                        purchaseItem.StorageViewStatus = PurchaseOrderStatus.TO_BE_CREATED;
                    }

                    // check Quality View Status
                    if (list.Any(item => item.PartNumber == purchaseItem.MaterialNumber.Number && item.PlantNumber == purchaseItem.DeliveryAdress?.Number && item.PlantViews.Contains("Q")))
                    {
                        purchaseItem.QualityViewStatus = PurchaseOrderStatus.CREATED;
                    }
                    else
                    {
                        if (list.Any(item => item.PartNumber == purchaseItem.MaterialNumber.Number && item.IsQualityView))
                        {
                            purchaseItem.QualityViewStatus = PurchaseOrderStatus.TO_BE_CREATED;
                        }
                        else
                        {
                            purchaseItem.QualityViewStatus = PurchaseOrderStatus.NOT_APPLICABLE;
                        }
                    }

                }

                CurrentDataContext.SelectedRequest.IsCheckExtendedRun = true;

                _purchaseOrderFollowWindowService.ShowPurchaseOrderFollowUpExtendedPartView(this);
                //PurchaseOrderFollowUpExtendedPartView purchaseOrderFollowUpExtendedPartView = new PurchaseOrderFollowUpExtendedPartView();
                //purchaseOrderFollowUpExtendedPartView.DataContext = CurrentDataContext;
                //purchaseOrderFollowUpExtendedPartView.Owner = Application.Current.MainWindow;
                //purchaseOrderFollowUpExtendedPartView.ShowDialog();

            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdatePurchaseRequestXls(ExcelToolsClosedXml CurrentExcel)
        {
            try
            {
                CurrentExcel.CurrentSheet = "Purchase Request";
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentRequest.RequestType.ToString(), 1, 2);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentRequest.CostCenter.Number, 2, 2);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentRequest.CostCenter.Description, 2, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentRequest.InternalOrder?.Number, 3, 2);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentRequest.InternalOrder?.Description, 3, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentRequest.Description, 1, 6);

                int Index = 6;

                foreach (var item in CurrentDataContext.CurrentRequest.UpdatedListItem)
                {
                    CurrentExcel.SetCellValue(item.Number, Index, 1);
                    CurrentExcel.SetCellValue(item.AccountAssignementCategory, Index, 2);
                    CurrentExcel.SetCellValue(item.MaterialNumber?.Number, Index, 3);
                    CurrentExcel.SetCellValue(item.Description, Index, 4);
                    CurrentExcel.SetCellValue(item.Quantity, Index, 5);
                    if (item.DeliveryDate != null)
                        CurrentExcel.SetCellValue(item.DeliveryDate.Value.ToString("dd.MM.yyyy"), Index, 7);
                    CurrentExcel.SetCellValue(item.Vendor?.Number, Index, 9);
                    CurrentExcel.SetCellValue(item.Price, Index, 10);
                    CurrentExcel.SetCellValue(item.DeliveryAdress?.Name, Index, 11);
                    CurrentExcel.SetCellValue(item.Detail, Index, 12);

                    Index++;
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private bool CheckVendorRequestData()
        {
            try
            {
                if (CurrentDataContext.CurrentVendor != null)
                {
                    if (string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.Description)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.DescriptionShort)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.StreetNumber)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.StreetName)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.City)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.PostalCode)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.Country)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.Langue)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.CompanyTel)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.CompanyMail)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.BusinessType)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.ContactFirstName)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.ContactLastName)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.ContactDepartment)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.ContactTel)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.ContactEmail)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.Siret)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.Siren)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.Tva)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.BankCountry)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.Iban)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.MaterialGroup)
                        || string.IsNullOrEmpty(CurrentDataContext.CurrentVendor.PurchaserCode))
                    {
                        return false;
                    }

                    string allowedChars = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.@-_";
                    string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                    Regex regex = new Regex(emailPattern);

                    if (!regex.IsMatch(CurrentDataContext.CurrentVendor.CompanyMail))
                    {
                        return false;
                    }
                    foreach (char c in CurrentDataContext.CurrentVendor.CompanyMail)
                    {
                        if (!allowedChars.Contains(c))
                        {
                            return false;
                        }
                    }

                    if (!regex.IsMatch(CurrentDataContext.CurrentVendor.ContactEmail))
                    {
                        return false;
                    }
                    foreach (char c in CurrentDataContext.CurrentVendor.ContactEmail)
                    {
                        if (!allowedChars.Contains(c))
                        {
                            return false;
                        }
                    }

                    if (CurrentDataContext.CurrentVendor.ListAttachment.Count == 0) { return false; }

                    return true;
                }
                else
                    return false;

            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private bool CheckRequestData(PurchaseOrderRequest CurrentRequest)
        {
            try
            {
                if (CurrentRequest == null) return false;
                if (CurrentRequest.CostCenter == null || CurrentRequest.Vendor == null || string.IsNullOrEmpty(CurrentRequest.RequestedBy) || CurrentRequest.InternalOrder == null) return false; //CurrentRequest.InternalOrder == null ||
                if (CurrentRequest.UpdatedListItem.Count == 0) return false;
                if (string.IsNullOrEmpty(CurrentRequest.Description)) return false;

                foreach (var item in CurrentRequest.UpdatedListItem)
                {
                    if (item == null || item.MaterialNumber == null || string.IsNullOrEmpty(item.MaterialNumber?.Number) || item.DeliveryDate == null || item.Quantity <= 0 || item.DeliveryAdress == null) return false; //string.IsNullOrEmpty(item.Material) || 
                    if (CurrentRequest.RequestType == PurchaseOrderType.ZRMI && (item.Price <= 0 || string.IsNullOrEmpty(item.Description))) return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateVendorRequestXls(ExcelToolsClosedXml CurrentExcel)
        {
            try
            {
                CurrentExcel.CurrentSheet = "Vendor Request";

                CurrentExcel.SetCellValue(LoggedUser.DisplayName, 3, 3);
                CurrentExcel.SetCellValue(LoggedUser.SamAccountName, 4, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.Description, 5, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.DescriptionShort, 6, 3);

                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.StreetNumber, 9, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.StreetName, 10, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.City, 11, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.PostalCode, 12, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.Country, 13, 3);

                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.Langue, 16, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.CompanyTel, 17, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.CompanyMail, 18, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.BusinessType, 19, 3);

                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.ContactLastName, 22, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.ContactFirstName, 23, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.ContactDepartment, 24, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.ContactTel, 25, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.ContactEmail, 26, 3);

                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.Siret, 29, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.Siren, 30, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.Tva, 31, 3);

                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.BankCountry, 34, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.Iban, 35, 3);

                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.MaterialGroup, 37, 3);
                CurrentExcel.SetCellValue(CurrentDataContext.CurrentVendor.PurchaserCode, 39, 3);
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void SearchInternalOrder()
        {
            try
            {
                List<PurchaseOrderInternalOrder> TempList = new List<PurchaseOrderInternalOrder>();
                string RegExDescriptionStr = "";
                Regex RegExDescription = null;
                if (CurrentDataContext.DescriptionSearchField != null)
                {
                    int index = 1;
                    foreach (var item in CurrentDataContext.DescriptionSearchField.Replace("*", " ").Replace(" ", "|").Split('|'))
                    {
                        if (index == 1)
                            RegExDescriptionStr = $@"^(?=.*{item})";
                        else
                            RegExDescriptionStr = $@"{RegExDescriptionStr}(?=.*{item})";

                        index++;
                    }
                    RegExDescriptionStr = $"{RegExDescriptionStr}.*$";

                    RegExDescription = new Regex(RegExDescriptionStr, RegexOptions.IgnoreCase);
                }

                string RegExNumberStr = "";
                Regex RegExNumber = null;
                if (CurrentDataContext.NumberSearchField != null)
                {
                    int index = 1;
                    foreach (var item in CurrentDataContext.NumberSearchField.Replace("*", " ").Replace(" ", "|").Split('|'))
                    {
                        if (index == 1)
                            RegExNumberStr = $@"^(?=.*{item})";
                        else
                            RegExNumberStr = $@"{RegExNumberStr}(?=.*{item})";

                        index++;
                    }
                    RegExNumberStr = $"{RegExNumberStr}.*$";

                    RegExNumber = new Regex(RegExNumberStr, RegexOptions.IgnoreCase);
                }

                foreach (var item in _purchaseOrderService.GetAllPoInternalOrders())
                {
                    //if ((item.NumberIo !=null && RegExNumber.IsMatch(item.NumberIo))
                    //    || (RegExDescription != null && RegExDescription.IsMatch(item.DescriptionIo)))
                    if (item.NumberIo == CurrentDataContext.NumberSearchField
                    || RegExDescription != null && RegExDescription.IsMatch(item.DescriptionIo))
                    {
                        TempList.Add(PurchaseOrderInternalOrder.GetInternalOrderFromDbItem(item));
                    }
                }

                CurrentDataContext.ListItem = TempList;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void SearchDbMaterialGroup()
        {
            try
            {
                List<PurchaseOrderMaterialGroup> TempList = new List<PurchaseOrderMaterialGroup>();

                foreach (var item in _purchaseOrderService.GetAllPoMaterialGroups())
                {
                    CurrentDataContext.ListMaterialGroup.Add(PurchaseOrderMaterialGroup.GetMaterialGroupFromDbItem(item));
                }

                CurrentDataContext.ListItem = TempList;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private PurchaseOrderLocation GetPurchaseOrderLocationFromNumber(string Number)
        {
            try
            {
                return CurrentDataContext.ListDeliveryLocation.FirstOrDefault(item => item.Number == Number);
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private PurchaseOrderInternalOrder GetPurchaseOrderInternalOrderFromNumber(string Number)
        {
            try
            {
                PurchaseOrderInternalOrder CurrentInternalOrder = null;

                PoInternalOrder CurrentDbItem = _purchaseOrderService.GetPoInternalOrderByNumber(Number).FirstOrDefault();
                CurrentInternalOrder = PurchaseOrderInternalOrder.GetInternalOrderFromDbItem(CurrentDbItem);

                return CurrentInternalOrder;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private PurchaseOrderMaterial GetPurchaseOrderMaterialFromNumber(string Number)
        {
            try
            {
                PurchaseOrderMaterial CurrentMaterial = null;
                PoMatDienNlag CurrentDbItem = _purchaseOrderService.GetPoMatDienNlagByNumber(Number);
                CurrentMaterial = PurchaseOrderMaterial.GetMaterialFromDbItem(CurrentDbItem);

                return CurrentMaterial;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private PurchaseOrderCostCenter GetPurchaseOrderCostCenterFromNumber(string Number)
        {
            try
            {
                char[] zeros = { '0' };
                Number = Number.TrimStart(zeros);

                PurchaseOrderCostCenter CurrentCostCenter = CurrentDataContext.ListCostCenter.FirstOrDefault(item => item.Number == Number);

                if (CurrentCostCenter == null)
                    CurrentCostCenter = new PurchaseOrderCostCenter()
                    {
                        Number = Number,
                        Description = Number
                    };

                return CurrentCostCenter;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private PurchaseOrderType GetPurchaseOrderTypeFromPrPoType(string PrType, string PoType = null, string VendorNumber = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(PrType))
                {
                    if (PrType == "NB")
                    {
                        if (string.IsNullOrEmpty(PoType))
                        {
                            if (string.IsNullOrEmpty(VendorNumber))
                                return PurchaseOrderType.ZNB;
                            else if (Regex.IsMatch(VendorNumber, "^V"))
                                return PurchaseOrderType.ZICP;
                        }
                        else if (PoType == "ZICP")
                            return PurchaseOrderType.ZICP;
                    }
                    else if (PrType == "ZRMI")
                    {
                        return PurchaseOrderType.ZRMI;
                    }
                }

                return PurchaseOrderType.ZRMI;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private PurchaseOrderVendor GetPurchaseOrderVendorFromNumber(string Number)
        {
            try
            {
                PurchaseOrderVendor CurrentVendor = null;

                var dbVendor = _sapHupService.GetSearchVendorFromNumber(Number);

                CurrentVendor = new PurchaseOrderVendor()
                {
                    Description = dbVendor.NAME1,
                    Number = dbVendor.LIFNR,
                    Location = dbVendor.LAND1
                };

                return CurrentVendor;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateListFromRequest(PurchaseOrderRequest Request)
        {
            try
            {
                TraceLog.AddTraceLog($"UpdateListFromRequest: request type before: {Request.RequestType}");
                Request.RequestType = CurrentDataContext.ListPurchaseType.FirstOrDefault(item => item == Request.RequestType);
                TraceLog.AddTraceLog($"UpdateListFromRequest: request type before: {Request.RequestType}");
                Request.CostCenter = CurrentDataContext.ListCostCenter.FirstOrDefault(item => item.Number == Request.CostCenter?.Number);

                PurchaseOrderInternalOrder TempInternalOrder = CurrentDataContext.ListInternalOrder.FirstOrDefault(item => item != null && item.Number != null && item.Number == Request?.InternalOrder?.Number);
                if (TempInternalOrder != null)
                {
                    Request.InternalOrder = TempInternalOrder;
                }
                else
                    CurrentDataContext.ListInternalOrder.Add(Request.InternalOrder);

                PurchaseOrderVendor TempVendor = CurrentDataContext.ListVendor.FirstOrDefault(item => item.Number == Request.Vendor?.Number);
                if (TempVendor != null)
                {
                    Request.Vendor = TempVendor;
                }
                else
                    CurrentDataContext.ListVendor.Add(Request.Vendor);

            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void StartCreateSapRequestAsynch()
        {
            try
            {
                SAPPurchaseOrderRequest CurrentRequest = CurrentDataContext.CurrentRequest.GetSapRequest();
                if (_sapPurchasingService.CreatePurchaseRequest(CurrentRequest) != SAPBomMsg.OK)
                    MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowSapNotStarted"), McgWpfTools.GetStringResource("POF_TitleSapIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsNoActionInProgress = true;
            }
        }

        private void StartConvertSapRequestAsynch()
        {
            try
            {
                if (_sapPurchasingService.ConvertPurchaseRequest(CurrentDataContext.CurrentRequest.SapPurchaseRequest, CurrentDataContext.CurrentRequest.RequestedBy) != SAPBomMsg.OK)
                    MessageBox.Show(McgWpfTools.GetStringResource("POF_WindowSapNotStarted"), McgWpfTools.GetStringResource("POF_TitleSapIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsNoActionInProgress = true;
            }
        }

        private void StartUpdateAllRequestFromSapAcynch()
        {
            try
            {
                TraceLog.AddTraceLog("Start StartUpdateAllRequestFromSapAcynch");
                List<PurchaseOrderRequest> ListFromPrToUpdate = new List<PurchaseOrderRequest>();
                List<PurchaseOrderRequest> ListFromPoToUpdate = new List<PurchaseOrderRequest>();
                CurrentDataContext.CurrentStep = 0;

                foreach (var dbReq in _purchaseOrderService.GetOpenSapPrRequests())
                    ListFromPrToUpdate.Add(PurchaseOrderRequest.GetRequestFromDb(dbReq));
                foreach (var dbReq in _purchaseOrderService.GetOpenRequestsWithSapPoWithoutSapPr())
                    ListFromPoToUpdate.Add(PurchaseOrderRequest.GetRequestFromDb(dbReq));

                CurrentDataContext.TotalStep = ListFromPrToUpdate.Count + ListFromPoToUpdate.Count;
                foreach (var req in ListFromPrToUpdate)
                {
                    TraceLog.AddTraceLog($"Current Step: {CurrentDataContext.CurrentStep}/{CurrentDataContext.TotalStep} Request {req.ID}");
                    UpdateRequestItemAttachment(req);
                    UpdateRequestFromSapHubPurchaseRequest(req);
                    if (!string.IsNullOrEmpty(req.SapPurchaseOrder))
                    {
                        UpdateRequestFromSapHubPurchaseOrder(req);
                        UpdateRequestFromSapHubPurchaseOrderGoodsInvoiceReceipt(req);
                    }
                    CreateUpdateRequestOnDataBase(req);
                    CurrentDataContext.CurrentStep++;
                }
                TraceLog.AddTraceLog("End StartUpdateAllRequestFromSapAcynch");
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsNoActionInProgress = true;
            }
        }

        private void SearchSimilarRequest()
        {
            try
            {
                List<PurchaseOrderDuplicate> duplicates = CurrentDataContext.ListAllRequest
                    .GroupBy(obj => new { obj.Total_Ordered, obj.Vendor?.Number, ItemCount = obj.ListItem?.Count }) //, NumberOrderKey = GetNumberOrderKey(obj.SapPurchaseRequest) })
                    .Where(g => g.Count() > 1)
                    .Select(g => new PurchaseOrderDuplicate(g.ToList(), 0))
                    .ToList();

                int index = 1;

                foreach (PurchaseOrderDuplicate dup in duplicates)
                {
                    if (dup.ListeDuplicateOrder.Count == 2 && (string.IsNullOrWhiteSpace(dup.ListeDuplicateOrder.First().SapPurchaseRequest)
                                                                || string.IsNullOrWhiteSpace(dup.ListeDuplicateOrder.Last().SapPurchaseRequest)
                                                                || dup.ListeDuplicateOrder.First().SapPurchaseRequest == dup.ListeDuplicateOrder.Last().SapPurchaseRequest))
                    {
                        dup.NumberItem = index++;
                        CurrentDataContext.ListDuplicateRequest.Add(dup);
                    }
                    else if (dup.ListeDuplicateOrder.Count > 2)
                    {
                        if (dup.ListeDuplicateOrder.FirstOrDefault(item => string.IsNullOrWhiteSpace(item.SapPurchaseRequest)) != null)
                        {
                            dup.NumberItem = index++;
                            CurrentDataContext.ListDuplicateRequest.Add(dup);
                        }
                        else if (dup.ListeDuplicateOrder.Count != dup.ListeDuplicateOrder.GroupBy(obj => new { obj.SapPurchaseRequest }).Count())
                        {
                            dup.NumberItem = index++;
                            CurrentDataContext.ListDuplicateRequest.Add(dup);
                        }
                    }

                    foreach (var item in dup.ListeDuplicateOrder)
                    {
                        if (item.SapPurchaseRequest == null) item.SapPurchaseRequest = "";
                        if (item.SapPurchaseOrder == null) item.SapPurchaseOrder = "";
                        if (item.Vendor == null) item.Vendor = new PurchaseOrderVendor() { Description = "", Number = "" };
                        if (item.Vendor?.Description == null) item.Vendor.Description = "";
                    }
                }

            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private string GetNumberOrderKey(string numberOrder)
        {
            if (string.IsNullOrWhiteSpace(numberOrder))
                return "NO_NUMBER";
            return numberOrder;
        }
        #endregion

        #region [REGION] Send Mail
        private void SendPurchaseRequestEmail()
        {
            try
            {
                PurchaseOrderRequest CurrentRequest = CurrentDataContext.CurrentRequest;

                List<PurchaseOrderMail> ListMail = GetListMailFromRequest(CurrentRequest);

                string SendEmail = CommonLibConstants.CadAdminEmail;

                //if (ListMail.Count == 0)
                //    ListMail.Add(new PurchaseOrderMail() { Email = SendEmail });

                string MailFrom = $"{System.Environment.GetEnvironmentVariable("USERNAME")}@manitowoc.com";


                List<string> ListFileName = new List<string>();
                foreach (var attachment in CurrentRequest.ListAttachment)
                {
                    attachment.WriteFile();
                    ListFileName.Add(attachment.TempCompleteFileName);
                }

                if (CurrentRequest.InternalOrder == null)
                {
                    CurrentRequest.InternalOrder = new PurchaseOrderInternalOrder()
                    {
                        Description = "WITHOUT",
                        Number = ""
                    };
                }

                string MailBody = $"<html><body><p>{McgWpfTools.GetStringResource("POF_MailBody01")}";
                MailBody = $"{MailBody}</p><p><br>{McgWpfTools.GetStringResource("POF_MailBody02")}"; MailBody = $"{MailBody}<br><br><br>{string.Format(McgWpfTools.GetStringResource("POF_MailBody03"), CurrentRequest.RequestType)}";
                MailBody = $"{MailBody}<br>{string.Format(McgWpfTools.GetStringResource("POF_MailBody04"), $"{CurrentRequest.CostCenter.Number} - {CurrentRequest.CostCenter.Description}")}";
                MailBody = $"{MailBody}<br>{string.Format(McgWpfTools.GetStringResource("POF_MailBody05"), $"{CurrentRequest.InternalOrder.Number} - {CurrentRequest.InternalOrder.Description}")}";
                MailBody = $"{MailBody}<br>{string.Format(McgWpfTools.GetStringResource("POF_MailBody06"), $"{CurrentRequest.Vendor.Number} - {CurrentRequest.Vendor.Description}")}";
                MailBody = $"{MailBody}<br><br><p>{McgWpfTools.GetStringResource("POF_MailBody07")}";

                MailBody = $"{MailBody}<br><br>{McgWpfTools.GetStringResource("POF_MailBodyEnd01")}";
                MailBody = $"{MailBody}<br>{McgWpfTools.GetStringResource("POF_MailBodyEnd02")}";
                MailBody = $"{MailBody}<br><br>{LoggedUser.GivenName} {LoggedUser.Surname}";


                string MailObject = string.Format(McgWpfTools.GetStringResource("POF_MailObject"), CurrentRequest.ID);

                McgEMail NewEmail = new McgEMail()
                {
                    MailBody = MailBody,
                    MailFrom = MailFrom,
                    Mailsubject = MailObject,
                    MailRestritedListAddress = new List<McgEMailItem>(),
                    MailRestritedListAddressCC = new List<McgEMailItem>()
                };

                foreach (var mail in ListMail)
                {
                    if (!string.IsNullOrEmpty(mail.Email))
                        NewEmail.MailRestritedListAddress.Add(new McgEMailItem() { Location = "ALL", MailAddress = mail.Email, Name = mail.Email });
                    if (!string.IsNullOrEmpty(mail.EmailCC))
                        NewEmail.MailRestritedListAddressCC.Add(new McgEMailItem() { Location = "ALL", MailAddress = mail.EmailCC, Name = mail.EmailCC });
                }


                NewEmail.SendMailOutlook(ListFileName);

                if (CurrentDataContext.CurrentRequest.Status == PurchaseOrderStatus.NEW)
                    CurrentDataContext.CurrentRequest.Status = PurchaseOrderStatus.SENT;
                // Environment.Exit(0);
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void SendPurchaseRequestEmailWithoutOulook()
        {
            try
            {
                PurchaseOrderRequest currentRequest = CurrentDataContext.CurrentRequest;

                // Construction des listes d'adresses mail
                List<McgEMailItem> mailListAddress = new List<McgEMailItem>();
                List<McgEMailItem> mailListAddressCC = new List<McgEMailItem>();
                List<MailAttachment> listAttachments = new List<MailAttachment>();

                // Ajout des pièces jointes
                foreach (var attachment in currentRequest.ListAttachment)
                {
                    attachment.WriteFile();
                    listAttachments.Add(new MailAttachment { CompleteFilename = attachment.TempCompleteFileName });
                }

                // Ajout des destinataires principaux
                var listMail = GetListMailFromRequest(currentRequest);
                foreach (var mail in listMail)
                {
                    if (!string.IsNullOrEmpty(mail.Email))
                        mailListAddress.Add(new McgEMailItem { MailAddress = mail.Email });
                }

                // Ajout des destinataires en copie
                foreach (var mail in listMail)
                {
                    if (!string.IsNullOrEmpty(mail.EmailCC))
                        mailListAddressCC.Add(new McgEMailItem { MailAddress = mail.EmailCC });
                }

                string email = UserPrincipal.Current?.EmailAddress;
                if (!string.IsNullOrEmpty(email))
                    mailListAddressCC.Add(new McgEMailItem { MailAddress = email });

                // Construction du corps et de l'objet du mail
                string mailBody = $"<html><body><p>{McgWpfTools.GetStringResource("POF_MailBody01")}";
                mailBody = $"{mailBody}</p><p><br>{McgWpfTools.GetStringResource("POF_MailBody02")}";
                mailBody = $"{mailBody}<br><p><p>{string.Format(McgWpfTools.GetStringResource("POF_MailBody03"), currentRequest.RequestType)}";
                mailBody = $"{mailBody}<p>{string.Format(McgWpfTools.GetStringResource("POF_MailBody04"), $"{currentRequest.CostCenter.Number} - {currentRequest.CostCenter.Description}")}";
                mailBody = $"{mailBody}<p>{string.Format(McgWpfTools.GetStringResource("POF_MailBody05"), $"{currentRequest.InternalOrder?.Number} - {currentRequest.InternalOrder?.Description}")}";
                mailBody = $"{mailBody}<p>{string.Format(McgWpfTools.GetStringResource("POF_MailBody06"), $"{currentRequest.Vendor.Number} - {currentRequest.Vendor.Description}")}";
                mailBody = $"{mailBody}<br><p><p>{McgWpfTools.GetStringResource("POF_MailBody07")}";
                mailBody = $"{mailBody}<br><p>{McgWpfTools.GetStringResource("POF_MailBodyEnd01")}";
                mailBody = $"{mailBody}<p>{McgWpfTools.GetStringResource("POF_MailBodyEnd02")}";
                mailBody = $"{mailBody}<br><p>{LoggedUser.GivenName} {LoggedUser.Surname}";

                string mailObject = string.Format(McgWpfTools.GetStringResource("POF_MailObject"), currentRequest.ID);

                // Affichage de la fenêtre de mail personnalisée
                McgMailView mcgMail = new McgMailView(
                    mailBody,
                    mailObject,
                    mailListAddress,
                    mailListAddressCC,
                    listAttachments
                );
                mcgMail.Show();

                if (CurrentDataContext.CurrentRequest.Status == PurchaseOrderStatus.NEW)
                    CurrentDataContext.CurrentRequest.Status = PurchaseOrderStatus.SENT;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private List<PurchaseOrderMail> GetListMailFromRequest(PurchaseOrderRequest CurrentRequest)
        {
            try
            {
                List<PurchaseOrderMail> ListMail = new List<PurchaseOrderMail>();

                if (CurrentRequest != null
                    && CurrentRequest.ListItem != null
                    && CurrentRequest.ListItem.Count > 0
                    && CurrentRequest.ListItem.ElementAt(0).DeliveryAdress != null
                    && CurrentRequest.ListItem.ElementAt(0).DeliveryAdress.Name != null)
                {
                    PurchaseOrderLocation CurrentLocation = CurrentDataContext.ListDeliveryLocation.FirstOrDefault(item => item.Name == CurrentRequest.ListItem.ElementAt(0).DeliveryAdress.Name);
                    if (CurrentLocation != null)
                    {
                        ListMail.AddRange(CurrentLocation.ListMail.Where(item => item.TypeRequest == CurrentRequest.RequestType));
                    }
                }

                return ListMail;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void SendVendorRequestEmail()
        {
            try
            {
                PurchaseOrderVendor CurrentRequest = CurrentDataContext.CurrentVendor;

                string SendEmail = PurchaseOrderFollowUpConstants.MailCreateVendor;
                string SendEmailCC = PurchaseOrderFollowUpConstants.CadAdminEmail;

                string MailFrom = $"{System.Environment.GetEnvironmentVariable("USERNAME")}@manitowoc.com";


                List<string> ListFileName = new List<string>();
                foreach (var attachment in CurrentRequest.ListAttachment)
                {
                    attachment.WriteFile();
                    ListFileName.Add(attachment.TempCompleteFileName);
                }
                string MailBody = "";
                string MailObject = "";

                if (CurrentRequest.ToBeUpdated)
                {
                    MailBody = $"<html><body><p>{McgWpfTools.GetStringResource("POF_MailVendorBody01")}";
                    MailBody = $"{MailBody}</p><p><br>{McgWpfTools.GetStringResource("POF_MailVendorBody04")}";
                    MailBody = $"{MailBody}<br><br><br><p>{string.Format(McgWpfTools.GetStringResource("POF_MailVendorBody05"), CurrentRequest.Description)}";
                    MailBody = $"{MailBody}<br>{string.Format(McgWpfTools.GetStringResource("POF_MailVendorBody06"), CurrentRequest.Number)}";
                    //MailBody = $"{MailBody}<br><br><br><p>{string.Format(McgMiscTools.GetStringResource("POF_MailVendorBody06"), CurrentRequest.Number)}";
                    MailObject = string.Format(McgWpfTools.GetStringResource("POF_MailVendorUpdateObject"), CurrentRequest.Description);
                }
                else
                {
                    MailBody = $"<html><body><p>{McgWpfTools.GetStringResource("POF_MailVendorBody01")}";
                    MailBody = $"{MailBody}</p><p><br>{McgWpfTools.GetStringResource("POF_MailVendorBody02")}";
                    MailBody = $"{MailBody}<br><br><br><p>{string.Format(McgWpfTools.GetStringResource("POF_MailVendorBody03"), CurrentRequest.Description)}";
                    MailObject = string.Format(McgWpfTools.GetStringResource("POF_MailVendorObject"), CurrentRequest.Description);
                }

                MailBody = $"{MailBody}<br><br>{McgWpfTools.GetStringResource("POF_MailBodyEnd01")}";
                MailBody = $"{MailBody}<br>{McgWpfTools.GetStringResource("POF_MailBodyEnd02")}";
                MailBody = $"{MailBody}<br><br>{LoggedUser.GivenName} {LoggedUser.Surname}";

                McgEMail NewEmail = new McgEMail()
                {
                    MailBody = MailBody,
                    MailFrom = MailFrom,
                    Mailsubject = MailObject,
                    MailRestritedListAddress = new List<McgEMailItem>(),
                    MailRestritedListAddressCC = new List<McgEMailItem>()
                };
                NewEmail.MailRestritedListAddress.Add(new McgEMailItem() { Location = "ALL", MailAddress = SendEmail, Name = SendEmail });
                NewEmail.MailRestritedListAddressCC.Add(new McgEMailItem() { Location = "ALL", MailAddress = SendEmailCC, Name = SendEmailCC });

                NewEmail.SendMailOutlook(ListFileName);

                //CurrentDataContext.CurrentRequest.Status = PurchaseOrderStatus.SENT;
                // Environment.Exit(0);
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void SendExtendPartRequestEmail()
        {
            try
            {
                string SendEmail = PurchaseOrderFollowUpConstants.MailExtendPart;
                string SendEmailCC = PurchaseOrderFollowUpConstants.CadAdminEmail;


                string MailFrom = $"{System.Environment.GetEnvironmentVariable("USERNAME")}@manitowoc.com";

                string MailBody = "";
                string MailObject = "";

                MailBody = $"<html> <style>table, th, td {{border: 1px solid black;}} </style>  <body><p>{McgWpfTools.GetStringResource("POF_MailExtendPartBody01")}";
                MailBody = $"{MailBody}<br>{McgWpfTools.GetStringResource("POF_MailExtendPartBody02")}";
                MailBody = $"{MailBody}</p><br><table><tr><td><b>{McgWpfTools.GetStringResource("POF_MailExtendPartBody03")}</b></td><td><b>{McgWpfTools.GetStringResource("POF_MailExtendPartBody03b")}</b></td></tr>";
                foreach (var item in CurrentDataContext.SelectedRequest.ListItem.Where(req => !req.IsExtended))
                    MailBody = $"{MailBody}<tr><td>{item.Material}</td><td>{item.DeliveryAdress?.Number} - {item.DeliveryAdress?.Name}</td></tr>";
                MailBody = $"{MailBody}</table><p><br><br>{McgWpfTools.GetStringResource("POF_MailExtendPartBody04")}";
                MailBody = $"{MailBody}<br>{McgWpfTools.GetStringResource("POF_MailExtendPartBody05")}";
                MailBody = $"{MailBody}<br>{McgWpfTools.GetStringResource("POF_MailExtendPartBody06")}";
                MailBody = $"{MailBody}<br>{McgWpfTools.GetStringResource("POF_MailExtendPartBody07")}";
                MailBody = $"{MailBody}<br>{McgWpfTools.GetStringResource("POF_MailExtendPartBody08")}";
                MailBody = $"{MailBody}<br>{McgWpfTools.GetStringResource("POF_MailExtendPartBody09")}";
                MailBody = $"{MailBody}<br><br>{McgWpfTools.GetStringResource("POF_MailBodyEnd01")}";
                MailBody = $"{MailBody}<br>{McgWpfTools.GetStringResource("POF_MailBodyEnd02")}";
                MailBody = $"{MailBody}<br><br>{LoggedUser.GivenName} {LoggedUser.Surname}";

                MailObject = McgWpfTools.GetStringResource("POF_MailExtendPartObject");

                McgEMail NewEmail = new McgEMail()
                {
                    MailBody = MailBody,
                    MailFrom = MailFrom,
                    Mailsubject = MailObject,
                    MailRestritedListAddress = new List<McgEMailItem>(),
                    MailRestritedListAddressCC = new List<McgEMailItem>()
                };
                NewEmail.MailRestritedListAddress.Add(new McgEMailItem() { Location = "ALL", MailAddress = SendEmail, Name = SendEmail });
                NewEmail.MailRestritedListAddressCC.Add(new McgEMailItem() { Location = "ALL", MailAddress = SendEmailCC, Name = SendEmailCC });

                NewEmail.SendMailOutlook();
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void SendInternalOrderRequestEmail()
        {
            try
            {
                string SendEmail = PurchaseOrderFollowUpConstants.MailCreateInternalOrder;
                string SendEmailCC = PurchaseOrderFollowUpConstants.MailInternalOrderCC;

                string MailFrom = $"{System.Environment.GetEnvironmentVariable("USERNAME")}@manitowoc.com";

                string MailBody = "";
                string MailObject = "";

                MailBody = $"<html><body><p>{McgWpfTools.GetStringResource("POF_MailInternalOrderBody01")}";
                MailBody = $"{MailBody}</p><p><br>{McgWpfTools.GetStringResource("POF_MailInternalOrderBody02")}";
                MailBody = $"{MailBody}<br>{string.Format(McgWpfTools.GetStringResource("POF_MailInternalOrderBody03"), CurrentDataContext.InternalOrderDescription)}";
                MailBody = $"{MailBody}<br>{string.Format(McgWpfTools.GetStringResource("POF_MailInternalOrderBody04"), LoggedUser.SamAccountName)}";

                MailBody = $"{MailBody}<br><br>{McgWpfTools.GetStringResource("POF_MailBodyEnd01")}";
                MailBody = $"{MailBody}<br>{McgWpfTools.GetStringResource("POF_MailBodyEnd02")}";
                MailBody = $"{MailBody}<br><br>{LoggedUser.GivenName} {LoggedUser.Surname}";

                MailObject = McgWpfTools.GetStringResource("POF_MailInternalOrderObject");

                McgEMail NewEmail = new McgEMail()
                {
                    MailBody = MailBody,
                    MailFrom = MailFrom,
                    Mailsubject = MailObject,
                    MailRestritedListAddress = new List<McgEMailItem>(),
                    MailRestritedListAddressCC = new List<McgEMailItem>()
                };

                NewEmail.MailRestritedListAddress.Add(new McgEMailItem() { Location = "ALL", MailAddress = SendEmail, Name = SendEmail });

                if (SendEmailCC != null)
                    foreach (var mail in SendEmailCC.Split(';'))
                        NewEmail.MailRestritedListAddressCC.Add(new McgEMailItem() { Location = "ALL", MailAddress = mail, Name = mail });

                NewEmail.SendMailOutlook();
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion
    }

}
