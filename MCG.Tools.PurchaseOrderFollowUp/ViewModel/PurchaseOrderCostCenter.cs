using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.PurchaseOrderFollowUp.View;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderCostCenter : ObservableObject, IPurchaseOrderCostCenter
    {
        #region [REGION] Properties from Interface
        private string _Number = string.Empty;
        public string Number
        {
            get { return _Number; }
            set
            {
                if (this._Number != value)
                {
                    this._Number = value;
                    OnPropertyChanged();
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
                }

            }
        }

        private bool _IsSelected = true;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (this._IsSelected != value)
                {
                    this._IsSelected = value;
                    OnPropertyChanged();
                    RaiseIsSelectedEvent();
                }

            }
        }

        #endregion

        #region [REGION] Events

        public event EventHandler IsSelectedEvent;

        public void RaiseIsSelectedEvent()
        {
            try
            {
                IsSelectedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region [REGION] Internal variables
        #endregion

        #region [REGION] Misc
        public override string ToString()
        {
            return Number;
        }
        #endregion
    }
}
