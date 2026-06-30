using CommunityToolkit.Mvvm.ComponentModel;
using LiveCharts;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.View;
using System.Collections.ObjectModel;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderFollowUpDataContext : ObservableObject, IPurchaseOrderFollowUpDataContext
    {
        #region [REGION] Properties from Interface
        public ObservableCollection<PurchaseOrderRequest> ListRequest { get; set; } = new ObservableCollection<PurchaseOrderRequest>();
        public ObservableCollection<PurchaseOrderRequest> ListMyRequest { get; set; } = new ObservableCollection<PurchaseOrderRequest>();
        public ObservableCollection<PurchaseOrderRequest> ListShownMyRequest { get; set; } = new ObservableCollection<PurchaseOrderRequest>();
        public ObservableCollection<PurchaseOrderRequest> ListSearchedRequest { get; set; } = new ObservableCollection<PurchaseOrderRequest>();
        public ObservableCollection<PurchaseOrderRequest> ListShownRequest { get; set; } = new ObservableCollection<PurchaseOrderRequest>();

        private PurchaseOrderRequest _SelectedRequest;
        public PurchaseOrderRequest SelectedRequest
        {
            get { return _SelectedRequest; }
            set
            {
                if (this._SelectedRequest != value)
                {
                    this._SelectedRequest = value;
                    OnPropertyChanged();
                }

            }
        }

        public PurchaseOrderRequest CurrentRequest { get; set; }

        public ObservableCollection<PurchaseOrderType> ListPurchaseType { get; set; } = new ObservableCollection<PurchaseOrderType>();

        //private PurchaseOrderType _SelectedPurchaseType;
        //public PurchaseOrderType SelectedPurchaseType
        //{
        //    get { return _SelectedPurchaseType; }
        //    set
        //    {
        //        if (this._SelectedPurchaseType != value)
        //        {
        //            this._SelectedPurchaseType = value;
        //            OnPropertyChanged();
        //            UpdateDienNlag();
        //        }

        //    }
        //}

        public ObservableCollection<PurchaseOrderCostCenter> ListCostCenter { get; set; } = new ObservableCollection<PurchaseOrderCostCenter>();

        //private PurchaseOrderCostCenter _SelectedCostCenter;
        //public PurchaseOrderCostCenter SelectedCostCenter
        //{
        //    get { return _SelectedCostCenter; }
        //    set
        //    {
        //        if (this._SelectedCostCenter != value)
        //        {
        //            this._SelectedCostCenter = value;
        //            OnPropertyChanged();
        //        }

        //    }
        //}

        public ObservableCollection<PurchaseOrderInternalOrder> ListAllInternalOrder { get; set; } = new ObservableCollection<PurchaseOrderInternalOrder>();

        private PurchaseOrderInternalOrder _SelectedInternalOrder;
        public PurchaseOrderInternalOrder SelectedInternalOrder
        {
            get { return _SelectedInternalOrder; }
            set
            {
                if (this._SelectedInternalOrder != value)
                {
                    this._SelectedInternalOrder = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<PurchaseOrderInternalOrder> ListInternalOrder { get; set; } = new ObservableCollection<PurchaseOrderInternalOrder>();

        //private PurchaseOrderInternalOrder _SelectedInternalOrder;
        //public PurchaseOrderInternalOrder SelectedInternalOrder
        //{
        //    get { return _SelectedInternalOrder; }
        //    set
        //    {
        //        if (this._SelectedInternalOrder != value)
        //        {
        //            this._SelectedInternalOrder = value;
        //            OnPropertyChanged();
        //        }

        //    }
        //}

        public ObservableCollection<PurchaseOrderMaterial> ListDienNlagMaterial { get; set; } = new ObservableCollection<PurchaseOrderMaterial>();

        public ObservableCollection<PurchaseOrderVendor> ListVendor { get; set; } = new ObservableCollection<PurchaseOrderVendor>();

        private PurchaseOrderVendor _CurrentVendor;
        public PurchaseOrderVendor CurrentVendor
        {
            get { return _CurrentVendor; }
            set
            {
                if (this._CurrentVendor != value)
                {
                    this._CurrentVendor = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<PurchaseOrderLocation> ListDeliveryLocation { get; set; } = new ObservableCollection<PurchaseOrderLocation>();

        private string _GeneralDescription = string.Empty;
        public string GeneralDescription
        {
            get { return _GeneralDescription; }
            set
            {
                if (this._GeneralDescription != value)
                {
                    this._GeneralDescription = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsDienNlagMaterial = false;
        public bool IsDienNlagMaterial
        {
            get { return _IsDienNlagMaterial; }
            set
            {
                if (this._IsDienNlagMaterial != value)
                {
                    this._IsDienNlagMaterial = value;
                    OnPropertyChanged();
                }

            }
        }

        private object _ListItem;
        public object ListItem
        {
            get { return _ListItem; }
            set
            {
                if (this._ListItem != value)
                {
                    this._ListItem = value;
                    OnPropertyChanged();
                }

            }
        }

        private object _SelectedItem;
        public object SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (this._SelectedItem != value)
                {
                    this._SelectedItem = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsCloseBtShown = false;
        public bool IsCloseBtShown
        {
            get { return _IsCloseBtShown; }
            set
            {
                if (this._IsCloseBtShown != value)
                {
                    this._IsCloseBtShown = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _NumberSearchField = string.Empty;
        public string NumberSearchField
        {
            get { return _NumberSearchField; }
            set
            {
                if (this._NumberSearchField != value)
                {
                    this._NumberSearchField = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _DescriptionSearchField = string.Empty;
        public string DescriptionSearchField
        {
            get { return _DescriptionSearchField; }
            set
            {
                if (this._DescriptionSearchField != value)
                {
                    this._DescriptionSearchField = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _HasLocationProperty;
        public bool HasLocationProperty
        {
            get { return _HasLocationProperty; }
            set
            {
                if (this._HasLocationProperty != value)
                {
                    this._HasLocationProperty = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsAllBtRequestTypeShown = false;
        public bool IsAllBtRequestTypeShown
        {
            get { return _IsAllBtRequestTypeShown; }
            set
            {
                if (this._IsAllBtRequestTypeShown != value)
                {
                    this._IsAllBtRequestTypeShown = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _BtRequestTypeQuestion = string.Empty;
        public string BtRequestTypeQuestion
        {
            get { return _BtRequestTypeQuestion; }
            set
            {
                if (this._BtRequestTypeQuestion != value)
                {
                    this._BtRequestTypeQuestion = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _BtRequestType1 = string.Empty;
        public string BtRequestType1
        {
            get { return _BtRequestType1; }
            set
            {
                if (this._BtRequestType1 != value)
                {
                    this._BtRequestType1 = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _BtRequestType2 = string.Empty;
        public string BtRequestType2
        {
            get { return _BtRequestType2; }
            set
            {
                if (this._BtRequestType2 != value)
                {
                    this._BtRequestType2 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _BtRequestType3 = string.Empty;
        public string BtRequestType3
        {
            get { return _BtRequestType3; }
            set
            {
                if (this._BtRequestType3 != value)
                {
                    this._BtRequestType3 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _BtRequestType4 = string.Empty;
        public string BtRequestType4
        {
            get { return _BtRequestType4; }
            set
            {
                if (this._BtRequestType4 != value)
                {
                    this._BtRequestType4 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _BtRequestType5 = string.Empty;
        public string BtRequestType5
        {
            get { return _BtRequestType5; }
            set
            {
                if (this._BtRequestType5 != value)
                {
                    this._BtRequestType5 = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _BtRequestType6 = string.Empty;
        public string BtRequestType6
        {
            get { return _BtRequestType6; }
            set
            {
                if (this._BtRequestType6 != value)
                {
                    this._BtRequestType6 = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<PurchaseOrderMaterialGroup> ListMaterialGroup { get; set; } = new ObservableCollection<PurchaseOrderMaterialGroup>();

        private string _InternalOrderDescription = string.Empty;
        public string InternalOrderDescription
        {
            get { return _InternalOrderDescription; }
            set
            {
                if (this._InternalOrderDescription != value)
                {
                    this._InternalOrderDescription = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsRoleAdmin = false;
        public bool IsRoleAdmin
        {
            get { return _IsRoleAdmin; }
            set
            {
                if (this._IsRoleAdmin != value)
                {
                    this._IsRoleAdmin = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsRoleSuperviser = false;
        public bool IsRoleSuperviser
        {
            get { return _IsRoleSuperviser; }
            set
            {
                if (this._IsRoleSuperviser != value)
                {
                    this._IsRoleSuperviser = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsRoleSapCreator = false;
        public bool IsRoleSapCreator
        {
            get { return _IsRoleSapCreator; }
            set
            {
                if (this._IsRoleSapCreator != value)
                {
                    this._IsRoleSapCreator = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsAllCostCenterSelected = true;
        public bool IsAllCostCenterSelected
        {
            get { return _IsAllCostCenterSelected; }
            set
            {
                if (this._IsAllCostCenterSelected != value)
                {
                    this._IsAllCostCenterSelected = value;
                    OnPropertyChanged();
                    RaiseIsAllCostCenterSelectedEvent();
                }
            }
        }

        private DateOnly? _PoCreatedAfter = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));
        public DateOnly? PoCreatedAfter
        {
            get { return _PoCreatedAfter; }
            set
            {
                if (this._PoCreatedAfter != value)
                {
                    this._PoCreatedAfter = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PoCreatedAfterDateTime));
                }
            }
        }

        public DateTime? PoCreatedAfterDateTime
        {
            get => PoCreatedAfter.HasValue
                ? PoCreatedAfter.Value.ToDateTime(TimeOnly.MinValue)
                : (DateTime?)null;

            set
            {
                var newVal = value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null;
                if (PoCreatedAfter == newVal) return;
                PoCreatedAfter = newVal;
            }
        }

        private DateOnly? _PoCreatedBefore = DateOnly.FromDateTime(DateTime.Today);
        public DateOnly? PoCreatedBefore
        {
            get { return _PoCreatedBefore; }
            set
            {
                if (this._PoCreatedBefore != value)
                {
                    this._PoCreatedBefore = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PoCreatedBeforeDateTime));
                }

            }
        }

        public DateTime? PoCreatedBeforeDateTime
        {
            get => PoCreatedBefore.HasValue
                ? PoCreatedBefore.Value.ToDateTime(TimeOnly.MinValue)
                : (DateTime?)null;

            set
            {
                var newVal = value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null;
                if (PoCreatedBefore == newVal) return;
                PoCreatedBefore = newVal;
            }
        }


        private bool _StatusNewSelected = true;
        public bool StatusNewSelected
        {
            get { return _StatusNewSelected; }
            set
            {
                if (this._StatusNewSelected != value)
                {
                    this._StatusNewSelected = value;
                    OnPropertyChanged();
                    RaiseUpdateFilterEvent();
                }

            }
        }

        private bool _StatusSentSelected = true;
        public bool StatusSentSelected
        {
            get { return _StatusSentSelected; }
            set
            {
                if (this._StatusSentSelected != value)
                {
                    this._StatusSentSelected = value;
                    OnPropertyChanged();
                    RaiseUpdateFilterEvent();
                }

            }
        }

        private bool _StatusCreatedSelected = true;
        public bool StatusCreatedSelected
        {
            get { return _StatusCreatedSelected; }
            set
            {
                if (this._StatusCreatedSelected != value)
                {
                    this._StatusCreatedSelected = value;
                    OnPropertyChanged();
                    RaiseUpdateFilterEvent();
                }

            }
        }

        private bool _StatusGoodsReceiptSelected = true;
        public bool StatusGoodsReceiptSelected
        {
            get { return _StatusGoodsReceiptSelected; }
            set
            {
                if (this._StatusGoodsReceiptSelected != value)
                {
                    this._StatusGoodsReceiptSelected = value;
                    OnPropertyChanged();
                    RaiseUpdateFilterEvent();
                }

            }
        }

        private bool _StatusInvoiceReceiptSelected = true;
        public bool StatusInvoiceReceiptSelected
        {
            get { return _StatusInvoiceReceiptSelected; }
            set
            {
                if (this._StatusInvoiceReceiptSelected != value)
                {
                    this._StatusInvoiceReceiptSelected = value;
                    OnPropertyChanged();
                    RaiseUpdateFilterEvent();
                }

            }
        }

        private bool _StatusClosedSelected = false;
        public bool StatusClosedSelected
        {
            get { return _StatusClosedSelected; }
            set
            {
                if (this._StatusClosedSelected != value)
                {
                    this._StatusClosedSelected = value;
                    OnPropertyChanged();
                    RaiseUpdateFilterEvent();
                }

            }
        }

        private bool _IsNoActionInProgress = true;
        public bool IsNoActionInProgress
        {
            get { return _IsNoActionInProgress; }
            set
            {
                if (this._IsNoActionInProgress != value)
                {
                    this._IsNoActionInProgress = value;
                    OnPropertyChanged();
                }

            }
        }

        private int _CurrentStep = 0;
        public int CurrentStep
        {
            get { return _CurrentStep; }
            set
            {
                if (this._CurrentStep != value)
                {
                    this._CurrentStep = value;
                    OnPropertyChanged();
                }

            }
        }

        private int _TotalStep = 1;
        public int TotalStep
        {
            get { return _TotalStep; }
            set
            {
                if (this._TotalStep != value)
                {
                    this._TotalStep = value;
                    OnPropertyChanged();
                }

            }
        }

        // For Charts
        private bool _ChartStatusNewSelected = true;
        public bool ChartStatusNewSelected
        {
            get { return _ChartStatusNewSelected; }
            set
            {
                if (this._ChartStatusNewSelected != value)
                {
                    this._ChartStatusNewSelected = value;
                    OnPropertyChanged();
                    RaiseUpdateChartFilterEvent();
                }

            }
        }

        private bool _ChartStatusSentSelected = true;
        public bool ChartStatusSentSelected
        {
            get { return _ChartStatusSentSelected; }
            set
            {
                if (this._ChartStatusSentSelected != value)
                {
                    this._ChartStatusSentSelected = value;
                    OnPropertyChanged();
                    RaiseUpdateChartFilterEvent();
                }

            }
        }

        private bool _ChartStatusCreatedSelected = true;
        public bool ChartStatusCreatedSelected
        {
            get { return _ChartStatusCreatedSelected; }
            set
            {
                if (this._ChartStatusCreatedSelected != value)
                {
                    this._ChartStatusCreatedSelected = value;
                    OnPropertyChanged();
                    RaiseUpdateChartFilterEvent();
                }

            }
        }

        private bool _ChartStatusGoodsReceiptSelected = true;
        public bool ChartStatusGoodsReceiptSelected
        {
            get { return _ChartStatusGoodsReceiptSelected; }
            set
            {
                if (this._ChartStatusGoodsReceiptSelected != value)
                {
                    this._ChartStatusGoodsReceiptSelected = value;
                    OnPropertyChanged();
                    RaiseUpdateChartFilterEvent();
                }

            }
        }

        private bool _ChartStatusInvoiceReceiptSelected = true;
        public bool ChartStatusInvoiceReceiptSelected
        {
            get { return _ChartStatusInvoiceReceiptSelected; }
            set
            {
                if (this._ChartStatusInvoiceReceiptSelected != value)
                {
                    this._ChartStatusInvoiceReceiptSelected = value;
                    OnPropertyChanged();
                    RaiseUpdateChartFilterEvent();
                }

            }
        }

        private bool _ChartStatusClosedSelected = true;
        public bool ChartStatusClosedSelected
        {
            get { return _ChartStatusClosedSelected; }
            set
            {
                if (this._ChartStatusClosedSelected != value)
                {
                    this._ChartStatusClosedSelected = value;
                    OnPropertyChanged();
                    RaiseUpdateChartFilterEvent();
                }

            }
        }

        private DateOnly? _ChartPoCreatedAfter = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2));
        public DateOnly? ChartPoCreatedAfter
        {
            get { return _ChartPoCreatedAfter; }
            set
            {
                if (this._ChartPoCreatedAfter != value)
                {
                    this._ChartPoCreatedAfter = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ChartPoCreatedAfterDateTime));
                    RaiseUpdateChartFilterEvent();
                    RaiseUpdateDateFilterEvent();
                }
            }
        }

        public DateTime? ChartPoCreatedAfterDateTime
        {
            get => ChartPoCreatedAfter.HasValue
                ? ChartPoCreatedAfter.Value.ToDateTime(TimeOnly.MinValue)
                : (DateTime?)null;

            set
            {
                var newVal = value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null;
                if (ChartPoCreatedAfter == newVal) return;
                ChartPoCreatedAfter = newVal;
            }
        }

        private DateOnly? _ChartPoCreatedBefore = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly? ChartPoCreatedBefore
        {
            get { return _ChartPoCreatedBefore; }
            set
            {
                if (this._ChartPoCreatedBefore != value)
                {
                    this._ChartPoCreatedBefore = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ChartPoCreatedBeforeDateTime));
                    RaiseUpdateChartFilterEvent();
                    RaiseUpdateDateFilterEvent();
                }

            }
        }

        public DateTime? ChartPoCreatedBeforeDateTime
        {
            get => ChartPoCreatedBefore.HasValue
                ? ChartPoCreatedBefore.Value.ToDateTime(TimeOnly.MinValue)
                : (DateTime?)null;

            set
            {
                var newVal = value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null;
                if (ChartPoCreatedBefore == newVal) return;
                ChartPoCreatedBefore = newVal;
            }
        }

        private SeriesCollection _AllPurchasePieSeriesCost;
        public SeriesCollection AllPurchasePieSeriesCost
        {
            get { return _AllPurchasePieSeriesCost; }
            set
            {
                if (this._AllPurchasePieSeriesCost != value)
                {
                    this._AllPurchasePieSeriesCost = value;
                    OnPropertyChanged();
                }

            }
        }

        private SeriesCollection _AllPurchasePieSeriesNumber;
        public SeriesCollection AllPurchasePieSeriesNumber
        {
            get { return _AllPurchasePieSeriesNumber; }
            set
            {
                if (this._AllPurchasePieSeriesNumber != value)
                {
                    this._AllPurchasePieSeriesNumber = value;
                    OnPropertyChanged();
                }

            }
        }

        private SeriesCollection _AllPurchaseStackColSeriesNumber;
        public SeriesCollection AllPurchaseStackColSeriesNumber
        {
            get { return _AllPurchaseStackColSeriesNumber; }
            set
            {
                if (this._AllPurchaseStackColSeriesNumber != value)
                {
                    this._AllPurchaseStackColSeriesNumber = value;
                    OnPropertyChanged();
                }

            }
        }

        private SeriesCollection _AllPurchasePieSeriesVendorCost;
        public SeriesCollection AllPurchasePieSeriesVendorCost
        {
            get { return _AllPurchasePieSeriesVendorCost; }
            set
            {
                if (this._AllPurchasePieSeriesVendorCost != value)
                {
                    this._AllPurchasePieSeriesVendorCost = value;
                    OnPropertyChanged();
                }

            }
        }

        private Func<double, string> _PieSeriesCostFormatter;
        public Func<double, string> PieSeriesCostFormatter
        {
            get { return _PieSeriesCostFormatter; }
            set
            {
                if (this._PieSeriesCostFormatter != value)
                {
                    this._PieSeriesCostFormatter = value;
                    OnPropertyChanged();
                }

            }
        }

        private double _MinVendorRatio =2.5;
        public double MinVendorRatio
        {
            get { return _MinVendorRatio; }
            set
            {
                if (this._MinVendorRatio != value)
                {
                    this._MinVendorRatio = value;
                    OnPropertyChanged();
                    RaiseUpdateChartFilterEvent();
                }

            }
        }

        private bool _IsPleaseWaitShown = true;
        public bool IsPleaseWaitShown
        {
            get { return _IsPleaseWaitShown; }
            set
            {
                if (this._IsPleaseWaitShown != value)
                {
                    this._IsPleaseWaitShown = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<PurchaseOrderDuplicate> ListDuplicateRequest { get; set; } = new ObservableCollection<PurchaseOrderDuplicate>();

        #endregion

        #region [REGION] Internal variables
        public List<PurchaseOrderVendor> ListPlantVendor { get; set; }
        public List<PurchaseOrderRequest> ListAllRequest { get; set; }
        #endregion

        #region [REGION] Events Action
        public event EventHandler UpdateFilterEvent;
        public void RaiseUpdateFilterEvent()
        {
            try
            {
                UpdateFilterEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UpdateChartFilterEvent;
        public void RaiseUpdateChartFilterEvent()
        {
            try
            {
                UpdateChartFilterEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler IsAllCostCenterSelectedEvent;
        public void RaiseIsAllCostCenterSelectedEvent()
        {
            try
            {
                IsAllCostCenterSelectedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UpdateDateFilterEvent;

        public void RaiseUpdateDateFilterEvent()
        {
            try
            {
                UpdateDateFilterEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Misc
        public void UpdateDienNlag(PurchaseOrderType CurrentType)
        {
            try
            {
                IsDienNlagMaterial = (CurrentType == PurchaseOrderType.ZRMI);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
