using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.Models.Enums;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.View;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderMaterial : ObservableObject, IPurchaseOrderMaterial
    {
        #region [REGION] Properties from Interface
        private string _Number;
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
                }

            }
        }

        private SapMaterialType _Type = SapMaterialType.DIEN;
        public SapMaterialType Type
        {
            get { return _Type; }
            set
            {
                if (this._Type != value)
                {
                    this._Type = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        #endregion

        #region [REGION] Misc
        public override string ToString()
        {
            return $"{Type} - {Number}";
        }

        internal static PurchaseOrderMaterial GetMaterialFromDbItem(PoMatDienNlag item)
        {
            try
            {
                if (item == null) return null ;
                return new PurchaseOrderMaterial()
                {
                    Number = item.NumberMat,
                    Description = item.DescriptionMat,
                    Type = (SapMaterialType)Enum.Parse(typeof(SapMaterialType), item.TypeMat)
                };
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException("GetMaterialFromDbItem", ex);
            }

        }
        #endregion
    }
}
