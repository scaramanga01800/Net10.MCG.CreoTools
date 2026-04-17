using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.View;


namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderInternalOrder: ObservableObject, IPurchaseOrderInternalOrder
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
        #endregion

        #region [REGION] Internal variables
        public int IdIo { get; set; }
        #endregion

        #region [REGION] Misc
        public override string ToString()
        {
            return Description; 
        }

        internal static PurchaseOrderInternalOrder GetInternalOrderFromDbItem(PoInternalOrder item)
        {
            try
            { 
                if (item != null)
                {
                    return new PurchaseOrderInternalOrder()
                    {
                        Number = item.NumberIo,
                        Description = item.DescriptionIo,
                        IdIo = item.Idio
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException("PurchaseOrderInternalOrder", ex);
            }
        }
        #endregion
    }
}
