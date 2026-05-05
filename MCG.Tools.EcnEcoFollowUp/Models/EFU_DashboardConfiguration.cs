using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.EcnEcoFollowUp.Exceptions;
using MCG.Tools.EcnEcoFollowUp.Interfaces.Models;

namespace MCG.Tools.EcnEcoFollowUp.Models
{
    public class EFU_DashboardConfiguration : ObservableObject, IEFU_DashboardConfiguration
    {
        #region [REGION] Properties from Interface
        private bool _IsStatusNotCreated = true;
        public bool IsStatusNotCreated
        {
            get { return _IsStatusNotCreated; }
            set
            {
                if (this._IsStatusNotCreated != value)
                {
                    this._IsStatusNotCreated = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEventEvent();
                    RaiseIsUpdateFilterEvent();
                }
            }
        }
        private bool _IsStatus99 = true;
        public bool IsStatus99
        {
            get { return this._IsStatus99; }
            set
            {
                if (this._IsStatus99 != value)
                {
                    this._IsStatus99 = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEventEvent();
                    RaiseIsUpdateFilterEvent();
                }
            }
        }

        private bool _IsStatus01 = true;
        public bool IsStatus01
        {
            get { return this._IsStatus01; }
            set
            {
                if (this._IsStatus01 != value)
                {
                    this._IsStatus01 = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEventEvent();
                    RaiseIsUpdateFilterEvent();
                }
            }
        }

        private bool _IsStatus02 = false;
        public bool IsStatus02
        {
            get { return this._IsStatus02; }
            set
            {
                if (this._IsStatus02 != value)
                {
                    this._IsStatus02 = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEventEvent();
                    RaiseIsUpdateFilterEvent();
                }
            }
        }

        private bool _IsStatus03 = false;
        public bool IsStatus03
        {
            get { return this._IsStatus03; }
            set
            {
                if (this._IsStatus03 != value)
                {
                    this._IsStatus03 = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEventEvent();
                    RaiseIsUpdateFilterEvent();
                }
            }
        }

        private bool _IsInProgress = true;
        public bool IsInProgress
        {
            get { return this._IsInProgress; }
            set
            {
                if (this._IsInProgress != value)
                {
                    this._IsInProgress = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEventEvent();
                    RaiseIsUpdateFilterEvent();
                }
            }
        }

        private bool _IsUnderReview = true;
        public bool IsUnderReview
        {
            get { return this._IsUnderReview; }
            set
            {
                if (this._IsUnderReview != value)
                {
                    this._IsUnderReview = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEventEvent();
                    RaiseIsUpdateFilterEvent();
                }
            }
        }

        private bool _IsResolved = false;
        public bool IsResolved
        {
            get { return this._IsResolved; }
            set
            {
                if (this._IsResolved != value)
                {
                    this._IsResolved = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEventEvent();
                    RaiseIsUpdateFilterEvent();
                }
            }
        }

        private bool _IsCancelled = false;
        public bool IsCanceled
        {
            get { return this._IsCancelled; }
            set
            {
                if (this._IsCancelled != value)
                {
                    this._IsCancelled = value;
                    OnPropertyChanged();
                    RaiseIsUpdateEventEvent();
                    RaiseIsUpdateFilterEvent();
                }
            }
        }

        private string[] _ColumnsOrder;
        public string[] ColumnsOrder
        {
            get { return _ColumnsOrder; }
            set
            {
                if (this._ColumnsOrder != value)
                {
                    this._ColumnsOrder = value;
                    UpdateColumnsOrderStr();
                    OnPropertyChanged();
                    //RaiseIsUpdateColumsOrderEvent();
                }

            }
        }
        #endregion

        #region [REGION] Events
        public event EventHandler IsUpdateEvent;
        public event EventHandler IsUpdateFilterEvent;

        public void RaiseIsUpdateEventEvent()
        {
            try
            {
                IsUpdateEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        public void RaiseIsUpdateFilterEvent()
        {
            try
            {
                IsUpdateFilterEvent?.Invoke(Parent, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler IsUpdateColumsOrderUserEvent;
        public void RaiseIsUpdateColumsOrderUserEvent()
        {
            try
            {
                IsUpdateColumsOrderUserEvent?.Invoke(Parent, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Properties not from Interface
        public EFU_DashboardItem Parent { get; internal set; }

        public string ColumnsOrderStr { get; set; }
        #endregion

        #region [REGION] Misc Methods
        private void UpdateColumnsOrderStr()
        {
            try
            {
                ColumnsOrderStr = "";
                if (ColumnsOrder!=null && ColumnsOrder.Count() >0)
                {
                    foreach (var col in ColumnsOrder)
                        if (col != null)
                            ColumnsOrderStr = $"{ColumnsOrderStr}{col}|";
                }
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }

        public void UpdateColumnsOrder(string newColumnsOrder)
        {
            try
            {
                if (newColumnsOrder !=null)
                    ColumnsOrder = newColumnsOrder.Split('|');
            }
            catch (Exception ex)
            {
                throw new EcnEcoFollowUpException(this.GetType().Name, ex);
            }
        }
        #endregion


    }
}
