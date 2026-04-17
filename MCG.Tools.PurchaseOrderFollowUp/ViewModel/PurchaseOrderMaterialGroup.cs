using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.View;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderMaterialGroup : ObservableObject, IPurchaseOrderMaterialGroup
    {
        #region [REGION] Properties from Interface
        private string _Category = string.Empty;
        public string Category
        {
            get { return _Category; }
            set
            {
                if (this._Category != value)
                {
                    this._Category = value;
                    OnPropertyChanged();
                }

            }
        }

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
        public int IdMG { get; set; }
        #endregion

        #region [REGION] Misc
        internal static PurchaseOrderMaterialGroup GetMaterialGroupFromDbItem(PoMaterialgroup item)
        {
            try
            {
                if (item == null) return null;
                return new PurchaseOrderMaterialGroup()
                {
                    Category = item.Category,
                    Number = item.Number,
                    Description = item.Description,
                    IdMG = item.Idmg
                };
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException("PurchaseOrderMaterialGroup", ex);
            }
        }

        public override string ToString()
        {
            return $"{Category}:{Description} - {Number}";
        }
        #endregion

    }
}
