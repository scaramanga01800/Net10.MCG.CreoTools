using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.View;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderRequest : ObservableObject, IPurchaseOrderRequest
    {
        #region [REGION] Properties from Interface
        private int _ID = -1;
        public int ID
        {
            get { return _ID; }
            set
            {
                if (this._ID != value)
                {
                    this._ID = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _CreatedBy = string.Empty;
        public string CreatedBy
        {
            get { return _CreatedBy; }
            set
            {
                if (this._CreatedBy != value)
                {
                    this._CreatedBy = value;
                    OnPropertyChanged();
                }

            }
        }

        private DateOnly? _CreatedOn = null;
        public DateOnly? CreatedOn
        {
            get { return _CreatedOn; }
            set
            {
                if (this._CreatedOn != value)
                {
                    this._CreatedOn = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _RequestedBy = string.Empty;
        public string RequestedBy
        {
            get { return _RequestedBy; }
            set
            {
                if (this._RequestedBy != value)
                {
                    this._RequestedBy = value;
                    OnPropertyChanged();
                    CanBeClosedWithoutSaving = false;
                }

            }
        }

        private string _SapCreatedBy = string.Empty;
        public string SapCreatedBy
        {
            get { return _SapCreatedBy; }
            set
            {
                if (this._SapCreatedBy != value)
                {
                    this._SapCreatedBy = value;
                    OnPropertyChanged();
                }

            }
        }

        private DateOnly? _SapCreatedOn = null;
        public DateOnly? SapCreatedOn
        {
            get { return _SapCreatedOn; }
            set
            {
                if (this._SapCreatedOn != value)
                {
                    this._SapCreatedOn = value;
                    OnPropertyChanged();
                }

            }
        }

        private PurchaseOrderType _RequestType = PurchaseOrderType.ZRMI;
        public PurchaseOrderType RequestType
        {
            get { return _RequestType; }
            set
            {
                if (this._RequestType != value)
                {
                    this._RequestType = value;
                    OnPropertyChanged();
                    RaiseRequestTypeChangeEvent();
                    CanBeClosedWithoutSaving = false;
                }
            }
        }

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (this._Description != value)
                {
                    this._Description = value;
                    OnPropertyChanged();
                    CanBeClosedWithoutSaving = false;
                }

            }
        }

        private string _SapPurchaseOrder;
        public string SapPurchaseOrder
        {
            get { return _SapPurchaseOrder; }
            set
            {
                if (this._SapPurchaseOrder != value)
                {
                    this._SapPurchaseOrder = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _SapPurchaseRequest;
        public string SapPurchaseRequest
        {
            get { return _SapPurchaseRequest; }
            set
            {
                if (this._SapPurchaseRequest != value)
                {
                    this._SapPurchaseRequest = value;
                    OnPropertyChanged();
                    CanBeClosedWithoutSaving = false;
                }

            }
        }

        private PurchaseOrderStatus _Status = PurchaseOrderStatus.UNKNOWN;
        public PurchaseOrderStatus Status
        {
            get { return _Status; }
            set
            {
                if (this._Status != value)
                {
                    this._Status = value;
                    OnPropertyChanged();
                }

            }
        }

        private PurchaseOrderInternalOrder _InternalOrder;
        public PurchaseOrderInternalOrder InternalOrder
        {
            get { return _InternalOrder; }
            set
            {
                if (this._InternalOrder != value)
                {
                    this._InternalOrder = value;
                    OnPropertyChanged();
                    CanBeClosedWithoutSaving = false;
                }

            }
        }

        private PurchaseOrderCostCenter _CostCenter;
        public PurchaseOrderCostCenter CostCenter
        {
            get { return _CostCenter; }
            set
            {
                if (this._CostCenter != value)
                {
                    this._CostCenter = value;
                    OnPropertyChanged();
                    CanBeClosedWithoutSaving = false;
                }

            }
        }

        private PurchaseOrderVendor _Vendor;
        public PurchaseOrderVendor Vendor
        {
            get { return _Vendor; }
            set
            {
                if (this._Vendor != value)
                {
                    this._Vendor = value;
                    OnPropertyChanged();
                    CanBeClosedWithoutSaving = false;
                }

            }
        }

        public ObservableCollection<PurchaseOrderItem> ListItem { get; set; } = new ObservableCollection<PurchaseOrderItem>();
        public ObservableCollection<PurchaseOrderItem> UpdatedListItem { get; set; } = new ObservableCollection<PurchaseOrderItem>();

        private PurchaseOrderItem _SelectedItem;
        public PurchaseOrderItem SelectedItem
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

        public ObservableCollection<PurchaseOrderAttachment> ListAttachment { get; set; } = new ObservableCollection<PurchaseOrderAttachment>();

        public ObservableCollection<PurchaseOrderAttachment> UpdatedListAttachment { get; set; } = new ObservableCollection<PurchaseOrderAttachment>();

        private PurchaseOrderAttachment _SelectedAttachment;
        public PurchaseOrderAttachment SelectedAttachment
        {
            get { return _SelectedAttachment; }
            set
            {
                if (this._SelectedAttachment != value)
                {
                    this._SelectedAttachment = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsUpdateAllowed = false;
        public bool IsUpdateAllowed
        {
            get { return _IsUpdateAllowed; }
            set
            {
                if (this._IsUpdateAllowed != value)
                {
                    this._IsUpdateAllowed = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsAlreadyExist = false;
        public bool IsAlreadyExist
        {
            get { return _IsAlreadyExist; }
            set
            {
                if (this._IsAlreadyExist != value)
                {
                    this._IsAlreadyExist = value;
                    OnPropertyChanged();
                }

            }
        }

        private double _Total_Ordered;
        public double Total_Ordered
        {
            get { return _Total_Ordered; }
            set
            {
                if (this._Total_Ordered != value)
                {
                    this._Total_Ordered = value;
                    OnPropertyChanged();
                    CanBeClosedWithoutSaving = false;
                }

            }
        }

        private double _Total_Goods;
        public double Total_Goods
        {
            get { return _Total_Goods; }
            set
            {
                if (this._Total_Goods != value)
                {
                    this._Total_Goods = value;
                    OnPropertyChanged();
                    CanBeClosedWithoutSaving = false;
                }

            }
        }

        private double _Total_Real_Goods;
        public double Total_Real_Goods
        {
            get { return _Total_Real_Goods; }
            set
            {
                if (this._Total_Real_Goods != value)
                {
                    this._Total_Real_Goods = value;
                    OnPropertyChanged();
                    CanBeClosedWithoutSaving = false;
                }

            }
        }

        private double _Total_Invoice;
        public double Total_Invoice
        {
            get { return _Total_Invoice; }
            set
            {
                if (this._Total_Invoice != value)
                {
                    this._Total_Invoice = value;
                    OnPropertyChanged();
                    CanBeClosedWithoutSaving = false;
                }

            }
        }

        private string _CurrencySymbol = "€";
        public string CurrencySymbol
        {
            get { return _CurrencySymbol; }
            set
            {
                if (this._CurrencySymbol != value)
                {
                    this._CurrencySymbol = value;
                    OnPropertyChanged();
                    CanBeClosedWithoutSaving = false;
                }

            }
        }

        private bool _CanBeClosedWithoutSaving = true;
        public bool CanBeClosedWithoutSaving
        {
            get { return _CanBeClosedWithoutSaving; }
            set
            {
                if (this._CanBeClosedWithoutSaving != value)
                {
                    this._CanBeClosedWithoutSaving = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _WindowTitle;
        public string WindowTitle
        {
            get { return _WindowTitle; }
            set
            {
                if (this._WindowTitle != value)
                {
                    this._WindowTitle = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsAdminUpdate = false;
        public bool IsAdminUpdate
        {
            get { return _IsAdminUpdate; }
            set
            {
                if (this._IsAdminUpdate != value)
                {
                    this._IsAdminUpdate = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public bool IsCheckExtendedRun { get; set; } = false;
        #endregion

        #region [REGION] Events
        public event EventHandler RequestTypeChangeEvent;
        public void RaiseRequestTypeChangeEvent()
        {
            try
            {
                RequestTypeChangeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        public PurchaseOrderRequest()
        {
            try
            {
                UpdatedListItem.CollectionChanged += (sender, e) =>
                {
                    CanBeClosedWithoutSaving = false;
                    foreach (var item in UpdatedListItem)
                    {
                        item.PurgeIsUpdatedEvent();
                        item.IsUpdatedEvent += (sender2, e2) => { CanBeClosedWithoutSaving = false; };
                    }
                };

                UpdatedListAttachment.CollectionChanged += (sender, e) =>
                {
                    CanBeClosedWithoutSaving = false;
                    foreach (var item in UpdatedListAttachment)
                    {
                        item.PurgeIsUpdatedEvent();
                        item.IsUpdatedEvent += (sender2, e2) => { CanBeClosedWithoutSaving = false; };
                    }
                };
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        #region [REGION] Misc
        public static PurchaseOrderRequest GetRequestFromDb(PoRequest DbRequest)
        {
            try
            {
                PurchaseOrderRequest newrequest = null;
                if (DbRequest != null)
                {
                    newrequest = new PurchaseOrderRequest()
                    {
                        CreatedBy = DbRequest.Createdby,
                        CreatedOn = DbRequest.Createdon,
                        Description = DbRequest.Description,
                        ID = DbRequest.Idrequest,
                        RequestedBy = DbRequest.Requestedby,
                        RequestType = (PurchaseOrderType)Enum.Parse(typeof(PurchaseOrderType), DbRequest.Requestype),
                        SapCreatedBy = DbRequest.Sapcreatedby,
                        SapPurchaseOrder = DbRequest.Sappoid,
                        SapPurchaseRequest = DbRequest.Sapprid,
                        SapCreatedOn = DbRequest.Sapcreatedon,
                        Status = (PurchaseOrderStatus)Enum.Parse(typeof(PurchaseOrderStatus), DbRequest.Status)
                    };
                    if (DbRequest.TotalOrdered != null)
                        newrequest.Total_Ordered = DbRequest.TotalOrdered.Value;
                    else
                        newrequest.Total_Ordered = 0;

                    if (DbRequest.TotalInvoice != null)
                        newrequest.Total_Invoice = DbRequest.TotalInvoice.Value;
                    else
                        newrequest.Total_Invoice = 0;

                    if (DbRequest.TotalGr != null)
                        newrequest.Total_Goods = DbRequest.TotalGr.Value;
                    else
                        newrequest.Total_Goods = 0;
                }
                return newrequest;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException("GetRequestFromDb", ex);
            }
        }

        public void UpdateDbrequest(PoRequest DbRequest)
        {
            try
            {
                DbRequest.Description = Description;
                DbRequest.Requestedby = RequestedBy;
                DbRequest.Costcenter = CostCenter?.Number;
                DbRequest.Idio = InternalOrder?.IdIo;
                DbRequest.Idvendor = Vendor?.IdVendor;
                DbRequest.Sappoid = SapPurchaseOrder;
                DbRequest.Sapprid = SapPurchaseRequest;
                if (DbRequest.Sapprid != null && DbRequest.Sapprid.Trim() == "")
                {
                    DbRequest.Requestype = RequestType.ToString();
                    DbRequest.Status = Status.ToString();
                }
                DbRequest.Sapcreatedby = SapCreatedBy;
                DbRequest.Sapcreatedon = SapCreatedOn;
                DbRequest.Status = Status.ToString();
                DbRequest.TotalInvoice = Total_Invoice;
                DbRequest.Requestype = RequestType.ToString();
                DbRequest.TotalGr = Total_Goods;
                DbRequest.TotalOrdered = Total_Ordered;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public PoRequest GetDbrequest()
        {
            try
            {
                PoRequest newDbRequest = new PoRequest()
                {
                    Requestedby = RequestedBy,
                    Requestype = RequestType.ToString(),
                    Status = Status.ToString(),
                    Createdon = CreatedOn,
                    Createdby = CreatedBy,
                    Description = Description,
                    Costcenter = CostCenter?.Number,
                    Idio = InternalOrder?.IdIo,
                    Idvendor = Vendor?.IdVendor,
                    Sappoid = SapPurchaseOrder,
                    Sapprid = SapPurchaseRequest,
                    Sapcreatedby = SapCreatedBy,
                    Sapcreatedon = SapCreatedOn,
                    TotalGr = Total_Goods,
                    TotalInvoice = Total_Invoice,
                    TotalOrdered = Total_Ordered,
                    Idrequest = ID
                };

                return newDbRequest;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException("GetRequestFromDb", ex);
            }
        }

        public PurchaseOrderRequest GetNewInstance()
        {
            try
            {
                PurchaseOrderRequest NewRequest = new PurchaseOrderRequest();

                return NewRequest;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public PurchaseOrderRequest Clone()
        {
            try
            {
                PurchaseOrderRequest clone = new PurchaseOrderRequest();

                clone.ID = this.ID;
                clone.CreatedBy = this.CreatedBy;
                clone.CreatedOn = this.CreatedOn;
                clone.RequestedBy = this.RequestedBy;
                clone.SapCreatedBy = this.SapCreatedBy;
                clone.SapCreatedOn = this.SapCreatedOn;
                clone.RequestType = this.RequestType;
                clone.Description = this.Description;
                clone.SapPurchaseOrder = this.SapPurchaseOrder;
                clone.SapPurchaseRequest = this.SapPurchaseRequest;
                clone.Status = this.Status;
                clone.InternalOrder = this.InternalOrder;
                clone.CostCenter = this.CostCenter;
                clone.Vendor = this.Vendor;
                clone.Total_Goods = this.Total_Goods;
                clone.Total_Invoice = this.Total_Invoice;
                clone.Total_Ordered = this.Total_Ordered;

                // Copie de l'ObservableCollection des items
                foreach (var item in this.ListItem)
                {
                    clone.ListItem.Add(item.Clone()); ;
                }

                // Copie de l'ObservableCollection des pièces jointes
                foreach (var attachment in this.ListAttachment)
                {
                    clone.ListAttachment.Add(attachment.Clone());
                }

                return clone;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }

        }

        internal SAPPurchaseOrderRequest GetSapRequest()
        {
            try
            {
                SAPPurchaseOrderRequest CurrenRequest = new SAPPurchaseOrderRequest()
                {
                    ListItem = new List<SAPPurchaseOrderItem>(),
                    CostCenter = CostCenter?.Number,
                    Description = $"Engineering PO {ID}\n {Description}",
                    InternalOrder = InternalOrder.Number,
                    RequestedBy = RequestedBy,
                    Vendor = Vendor.Number,
                };

                switch (RequestType)
                {
                    case PurchaseOrderType.ZICP:
                        CurrenRequest.RequestType = "NB";
                        break;
                    case PurchaseOrderType.ZRMI:
                        CurrenRequest.RequestType = "ZRMI";
                        break;
                    case PurchaseOrderType.ZNB:
                        CurrenRequest.RequestType = "NB";
                        break;
                    case PurchaseOrderType.RESA:
                        CurrenRequest.RequestType = "NB";
                        break;
                    default:
                        CurrenRequest.RequestType = "NB";
                        break;
                }


                foreach (var item in this.ListItem.OrderBy(obj => obj.Number))
                {
                    CurrenRequest.ListItem.Add(item.GetSapItem(Vendor?.Description));
                }

                return CurrenRequest;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public void CheckUpdateRight(string user, bool IsRoleAdmin)
        {
            try
            {
                IsUpdateAllowed = ((user.ToUpper() == CreatedBy?.ToUpper() || user.ToUpper() == RequestedBy?.ToUpper())
                            && (Status == PurchaseOrderStatus.NEW || Status == PurchaseOrderStatus.SENT || Status == PurchaseOrderStatus.REWORK)) || IsRoleAdmin;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
