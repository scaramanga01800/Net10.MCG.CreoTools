using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Models.SAP;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.View;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderItem : ObservableObject, IPurchaseOrderItem
    {
        #region [REGION] Properties from Interface
        private int _Number;
        public int Number
        {
            get { return _Number; }
            set
            {
                if (this._Number != value)
                {
                    this._Number = value;
                    OnPropertyChanged();
                    RaiseIsUpdatedEvent();
                }

            }
        }

        private string _Description = string.Empty;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (this._Description != value)
                {
                    this._Description = value;
                    OnPropertyChanged();
                    RaiseIsUpdatedEvent();
                }

            }
        }

        private string _AccountAssignementCategory = string.Empty;
        public string AccountAssignementCategory
        {
            get { return _AccountAssignementCategory; }
            set
            {
                if (this._AccountAssignementCategory != value)
                {
                    this._AccountAssignementCategory = value;
                    OnPropertyChanged();
                    RaiseIsUpdatedEvent();
                }

            }
        }

        private string _Material = string.Empty;
        public string Material
        {
            get { return _Material; }
            set
            {
                if (this._Material != value)
                {
                    this._Material = value;
                    OnPropertyChanged();
                    UpdateMaterial(true);
                    RaiseIsUpdatedEvent();
                }

            }
        }

        private PurchaseOrderMaterial _SelectedMaterial;
        public PurchaseOrderMaterial SelectedMaterial
        {
            get { return _SelectedMaterial; }
            set
            {
                if (this._SelectedMaterial != value)
                {
                    this._SelectedMaterial = value;
                    OnPropertyChanged();
                    UpdateMaterial(false);
                    RaiseIsUpdatedEvent();
                }

            }
        }

        private double _Quantity;
        public double Quantity
        {
            get { return _Quantity; }
            set
            {
                if (this._Quantity != value)
                {
                    this._Quantity = value;
                    OnPropertyChanged();
                    RaiseIsUpdatedEvent();
                }

            }
        }

        private DateOnly? _DeliveryDate;
        public DateOnly? DeliveryDate
        {
            get { return _DeliveryDate; }
            set
            {
                if (this._DeliveryDate != value)
                {
                    this._DeliveryDate = value;
                    OnPropertyChanged();
                    RaiseIsUpdatedEvent();
                }

            }
        }
        public DateTime? DeliveryDateDateTime
        {
            get => DeliveryDate.HasValue
                ? DeliveryDate.Value.ToDateTime(TimeOnly.MinValue)
                : (DateTime?)null;

            set
            {
                var newVal = value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null;
                if (DeliveryDate == newVal) return;
                DeliveryDate = newVal;
            }
        }


        private double _Price;
        public double Price
        {
            get { return _Price; }
            set
            {
                if (this._Price != value)
                {
                    this._Price = value;
                    OnPropertyChanged();
                    RaiseIsUpdatedEvent();
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
                    RaiseIsUpdatedEvent();
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
                    RaiseIsUpdatedEvent();
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
                    RaiseIsUpdatedEvent();
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
                    RaiseIsUpdatedEvent();
                }

            }
        }

        private string _Detail = string.Empty;
        public string Detail
        {
            get { return _Detail; }
            set
            {
                if (this._Detail != value)
                {
                    this._Detail = value;
                    OnPropertyChanged();
                    RaiseIsUpdatedEvent();
                }

            }
        }

        private PurchaseOrderLocation _DeliveryAdress;
        public PurchaseOrderLocation DeliveryAdress
        {
            get { return _DeliveryAdress; }
            set
            {
                if (this._DeliveryAdress != value)
                {
                    this._DeliveryAdress = value;
                    OnPropertyChanged();
                    RaiseIsUpdatedEvent();
                }

            }
        }

        private SapPlant _DeliveryPlant;
        public SapPlant DeliveryPlant
        {
            get { return _DeliveryPlant; }
            set
            {
                if (this._DeliveryPlant != value)
                {
                    this._DeliveryPlant = value;
                    OnPropertyChanged();
                    RaiseIsUpdatedEvent();
                }

            }
        }

        private PurchaseOrderStatus _GoodReceiptStatus = PurchaseOrderStatus.NEW;
        public PurchaseOrderStatus GoodReceiptStatus
        {
            get { return _GoodReceiptStatus; }
            set
            {
                if (this._GoodReceiptStatus != value)
                {
                    this._GoodReceiptStatus = value;
                    OnPropertyChanged();
                }
                RaiseIsUpdatedEvent();

            }
        }

        private bool _IsExtended = false;
        public bool IsExtended
        {
            get { return _IsExtended; }
            set
            {
                if (this._IsExtended != value)
                {
                    this._IsExtended = value;
                    OnPropertyChanged();
                }

            }
        }

        private PurchaseOrderStatus _PurchasingViewStatus = PurchaseOrderStatus.NOT_APPLICABLE;
        public PurchaseOrderStatus PurchasingViewStatus
        {
            get { return _PurchasingViewStatus; }
            set
            {
                if (_PurchasingViewStatus != value)
                {
                    _PurchasingViewStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private PurchaseOrderStatus _MrpViewStatus = PurchaseOrderStatus.UNKNOWN;
        public PurchaseOrderStatus MrpViewStatus
        {
            get { return _MrpViewStatus; }
            set
            {
                if (_MrpViewStatus != value)
                {
                    _MrpViewStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private PurchaseOrderStatus _StorageViewStatus = PurchaseOrderStatus.UNKNOWN;
        public PurchaseOrderStatus StorageViewStatus
        {
            get { return _StorageViewStatus; }
            set
            {
                if (_StorageViewStatus != value)
                {
                    _StorageViewStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        private PurchaseOrderStatus _QualityViewStatus = PurchaseOrderStatus.UNKNOWN;
        public PurchaseOrderStatus QualityViewStatus
        {
            get { return _QualityViewStatus; }
            set
            {
                if (_QualityViewStatus != value)
                {
                    _QualityViewStatus = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region [REGION] Internal variables
        public PurchaseOrderMaterial MaterialNumber { get; set; }
        public int IdItem { get; set; }
        public bool IsDbSaved { get; set; } = false;

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
                }

            }
        }

        private bool _Closed_Check = false;
        public bool Closed_Check
        {
            get { return _Closed_Check; }
            set
            {
                if (this._Closed_Check != value)
                {
                    this._Closed_Check = value;
                    OnPropertyChanged();
                }

            }
        }

        #endregion

        public event EventHandler IsUpdatedEvent;

        public void RaiseIsUpdatedEvent()
        {
            try
            {
                IsUpdatedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        #region [REGION] Misc
        public void PurgeIsUpdatedEvent()
        {
            try
            {
                IsUpdatedEvent = null;
                //if (IsUpdatedEvent != null)
                //{
                //    foreach (Delegate d in IsUpdatedEvent.GetInvocationList())
                //    {
                //        IsUpdatedEvent -= (EventHandler)d;
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        private void UpdateMaterial(bool FromMaterial)
        {
            try
            {
                if (FromMaterial)
                {
                    MaterialNumber = new PurchaseOrderMaterial()
                    {
                        Number = Material,
                        Type = SapMaterialType.HALB
                    };
                }
                else
                {
                    MaterialNumber = new PurchaseOrderMaterial()
                    {
                        Number = SelectedMaterial.Number,
                        Type = SelectedMaterial.Type,
                        Description = SelectedMaterial.Description
                    };
                }
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        internal void UpdateDbItem(PoItem dbItem)
        {
            try
            {
                dbItem.Accountassignement = AccountAssignementCategory;
                dbItem.Costcenter = CostCenter?.Number;
                dbItem.Deliverydate = DeliveryDate;
                dbItem.Deliveryadress = DeliveryAdress?.Name;
                dbItem.Description = Description;
                dbItem.Grstatus = GoodReceiptStatus.ToString();
                dbItem.Idio = InternalOrder?.IdIo;
                dbItem.Idvendor = Vendor?.IdVendor;
                dbItem.Material = MaterialNumber?.Number;
                dbItem.Number = Number;
                dbItem.Price = Price;
                dbItem.Quantity = Quantity;
                dbItem.Requestedby = RequestedBy;
                dbItem.Textdetail = Detail;
                dbItem.TotalGr = Total_Goods;
                dbItem.TotalInvoice = Total_Invoice;
                dbItem.TotalOrdered = Total_Ordered;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        internal PoItem GetDbItem(int iDREQUEST)
        {
            try
            {
                PoItem dbItem = new PoItem()
                {
                    Accountassignement = AccountAssignementCategory,
                    Costcenter = CostCenter?.Number,
                    Deliverydate = DeliveryDate,
                    Deliveryadress = DeliveryAdress?.Name,
                    Description = Description,
                    Grstatus = GoodReceiptStatus.ToString(),
                    Idio = InternalOrder?.IdIo,
                    Idvendor = Vendor?.IdVendor,
                    Material = MaterialNumber?.Number,
                    Number = Number,
                    Price = Price,
                    Quantity = Quantity,
                    Requestedby = RequestedBy,
                    Textdetail = Detail,
                    Idrequest = iDREQUEST,
                    TotalInvoice = Total_Invoice,
                    TotalGr = Total_Goods,
                    TotalOrdered = Total_Ordered,
                };

                return dbItem;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        internal static PurchaseOrderItem GetFromDbItem(PoItem item)
        {
            try
            {
                PurchaseOrderItem newItem = new PurchaseOrderItem()
                {
                    AccountAssignementCategory = item.Accountassignement,
                    Description = item.Description,
                    DeliveryDate = item.Deliverydate,
                    Detail = item.Textdetail,
                    IdItem = item.Iditem,
                    Material = item.Material,
                    Number = item.Number.Value,
                    Price = item.Price.Value,
                    Quantity = item.Quantity.Value,
                    RequestedBy = item.Requestedby,
                };
                newItem.CostCenter = new PurchaseOrderCostCenter()
                {
                    Number = item.Costcenter
                };
                newItem.DeliveryAdress = new PurchaseOrderLocation()
                {
                    Name = item.Deliveryadress
                };
                newItem.GoodReceiptStatus = (PurchaseOrderStatus)Enum.Parse(typeof(PurchaseOrderStatus), item.Grstatus);
                newItem.InternalOrder = new PurchaseOrderInternalOrder()
                {
                    Number = item.IdioNavigation?.NumberIo
                };

                if (item.TotalGr != null)
                    newItem.Total_Goods = item.TotalGr.Value;
                else
                    newItem.Total_Goods = 0;

                if (item.TotalInvoice != null)
                    newItem.Total_Invoice = item.TotalInvoice.Value;
                else
                    newItem.Total_Invoice = 0;

                if (item.TotalOrdered != null)
                    newItem.Total_Ordered = item.TotalOrdered.Value;
                else
                    newItem.Total_Ordered = 0;

                return newItem;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException("PurchaseOrderItem", ex);
            }
        }

        internal SAPPurchaseOrderItem GetSapItem(string vendor = null)
        {
            try
            {

                if (string.IsNullOrEmpty(AccountAssignementCategory))
                    AccountAssignementCategory = "K";

                var fullRequestBy = McgActiveDirectoryTools.GetUserInfoFromAd(RequestedBy.ToUpper());

                if (fullRequestBy == null) fullRequestBy = new ADUserInfo() { FirstName = RequestedBy.ToUpper(), LastName = "" };

                SAPPurchaseOrderItem CurrentItem = new SAPPurchaseOrderItem()
                {
                    AccountAssignmentCategory = AccountAssignementCategory,
                    DeliveryPlant = DeliveryAdress?.Number,
                    Material = Material,
                    Number = Number,
                    Price = Price,
                    Quantity = Quantity,
                    RequestedBy = RequestedBy.ToUpper(),
                    ItemDescription = $"{Detail}\nVendor: {vendor}\nRequested by: {fullRequestBy}",
                    MaterialDescription = Description
                };

                if (DeliveryDate != null)
                    CurrentItem.DeliveryDate = DeliveryDate.Value;
                else
                    CurrentItem.DeliveryDate = DateOnly.FromDateTime(DateTime.Today);

                if (string.IsNullOrEmpty(CurrentItem.ItemDescription))
                    CurrentItem.ItemDescription = $"{Description} - Delivery adress: {DeliveryAdress?.Description}";
                return CurrentItem;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException(this.GetType().Name, ex);
            }
        }

        public PurchaseOrderItem Clone()
        {
            try
            {
                PurchaseOrderItem clone = new PurchaseOrderItem();

                clone.Number = this.Number;
                clone.Description = this.Description;
                clone.AccountAssignementCategory = this.AccountAssignementCategory;
                clone.Material = this.Material;
                clone.SelectedMaterial = this.SelectedMaterial;
                clone.Quantity = this.Quantity;
                clone.DeliveryDate = this.DeliveryDate;
                clone.Price = this.Price;
                clone.InternalOrder = this.InternalOrder;
                clone.CostCenter = this.CostCenter;
                clone.RequestedBy = this.RequestedBy;
                clone.Vendor = this.Vendor;
                clone.Detail = this.Detail;
                clone.DeliveryAdress = this.DeliveryAdress;
                clone.DeliveryPlant = this.DeliveryPlant;
                clone.GoodReceiptStatus = this.GoodReceiptStatus;
                clone.Total_Invoice = this.Total_Invoice;
                clone.Total_Goods = this.Total_Goods;
                clone.Total_Ordered = this.Total_Ordered;

                return clone;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException("PurchaseOrderItem", ex);
            }

        }
        #endregion
    }
}
