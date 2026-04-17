using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.View;
using System.Collections.ObjectModel;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderVendor : ObservableObject, IPurchaseOrderVendor
    {
        #region [REGION] Properties from Interface
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

        private string _Location = string.Empty;
        public string Location
        {
            get { return _Location; }
            set
            {
                if (this._Location != value)
                {
                    this._Location = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public int IdVendor { get; set; }

        public PurchaseOrderType RequestType { get; set; } = PurchaseOrderType.ZRMI;

        public string Plant { get; set; } = string.Empty;

        private string _DescriptionShort = string.Empty;
        public string DescriptionShort
        {
            get { return _DescriptionShort; }
            set
            {
                if (this._DescriptionShort != value)
                {
                    this._DescriptionShort = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _StreetNumber = string.Empty;
        public string StreetNumber
        {
            get { return _StreetNumber; }
            set
            {
                if (this._StreetNumber != value)
                {
                    this._StreetNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _StreetName = string.Empty;
        public string StreetName
        {
            get { return _StreetName; }
            set
            {
                if (this._StreetName != value)
                {
                    this._StreetName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _City = string.Empty;
        public string City
        {
            get { return _City; }
            set
            {
                if (this._City != value)
                {
                    this._City = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _PostalCode = string.Empty;
        public string PostalCode
        {
            get { return _PostalCode; }
            set
            {
                if (this._PostalCode != value)
                {
                    this._PostalCode = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Country = string.Empty;    
        public string Country
        {
            get { return _Country; }
            set
            {
                if (this._Country != value)
                {
                    this._Country = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Langue = string.Empty;
        public string Langue
        {
            get { return _Langue; }
            set
            {
                if (this._Langue != value)
                {
                    this._Langue = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _CompanyTel = string.Empty;
        public string CompanyTel
        {
            get { return _CompanyTel; }
            set
            {
                if (this._CompanyTel != value)
                {
                    this._CompanyTel = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _CompanyMail = string.Empty;
        public string CompanyMail
        {
            get { return _CompanyMail; }
            set
            {
                if (this._CompanyMail != value)
                {
                    this._CompanyMail = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _BusinessType = string.Empty;
        public string BusinessType
        {
            get { return _BusinessType; }
            set
            {
                if (this._BusinessType != value)
                {
                    this._BusinessType = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ContactFirstName = string.Empty;
        public string ContactFirstName
        {
            get { return _ContactFirstName; }
            set
            {
                if (this._ContactFirstName != value)
                {
                    this._ContactFirstName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ContactLastName = string.Empty;
        public string ContactLastName
        {
            get { return _ContactLastName; }
            set
            {
                if (this._ContactLastName != value)
                {
                    this._ContactLastName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ContactDepartment = string.Empty;
        public string ContactDepartment
        {
            get { return _ContactDepartment; }
            set
            {
                if (this._ContactDepartment != value)
                {
                    this._ContactDepartment = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ContactTel = string.Empty;
        public string ContactTel
        {
            get { return _ContactTel; }
            set
            {
                if (this._ContactTel != value)
                {
                    this._ContactTel = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ContactEmail = string.Empty;
        public string ContactEmail
        {
            get { return _ContactEmail; }
            set
            {
                if (this._ContactEmail != value)
                {
                    this._ContactEmail = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Siret = string.Empty;
        public string Siret
        {
            get { return _Siret; }
            set
            {
                if (this._Siret != value)
                {
                    this._Siret = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Siren = string.Empty;
        public string Siren
        {
            get { return _Siren; }
            set
            {
                if (this._Siren != value)
                {
                    this._Siren = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Tva = string.Empty;
        public string Tva
        {
            get { return _Tva; }
            set
            {
                if (this._Tva != value)
                {
                    this._Tva = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _BankCountry = string.Empty;
        public string BankCountry
        {
            get { return _BankCountry; }
            set
            {
                if (this._BankCountry != value)
                {
                    this._BankCountry = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _IbanP1 = string.Empty;
        public string IbanP1
        {
            get { return _IbanP1; }
            set
            {
                if (this._IbanP1 != value)
                {
                    this._IbanP1 = value;
                    OnPropertyChanged();
                    UpdateIban();
                }
            }
        }

        private string _IbanP2 = string.Empty;
        public string IbanP2
        {
            get { return _IbanP2; }
            set
            {
                if (this._IbanP2 != value)
                {
                    this._IbanP2 = value;
                    OnPropertyChanged();
                    UpdateIban();
                }
            }
        }

        private string _IbanP3 = string.Empty;
        public string IbanP3
        {
            get { return _IbanP3; }
            set
            {
                if (this._IbanP3 != value)
                {
                    this._IbanP3 = value;
                    OnPropertyChanged();
                    UpdateIban();
                }
            }
        }

        private string _IbanP4 = string.Empty;
        public string IbanP4
        {
            get { return _IbanP4; }
            set
            {
                if (this._IbanP4 != value)
                {
                    this._IbanP4 = value;
                    OnPropertyChanged();
                    UpdateIban();
                }
            }
        }

        private string _IbanP5 = string.Empty;
        public string IbanP5
        {
            get { return _IbanP5; }
            set
            {
                if (this._IbanP5 != value)
                {
                    this._IbanP5 = value;
                    OnPropertyChanged();
                    UpdateIban();
                }
            }
        }

        private string _IbanP6 = string.Empty;
        public string IbanP6
        {
            get { return _IbanP6; }
            set
            {
                if (this._IbanP6 != value)
                {
                    this._IbanP6 = value;
                    OnPropertyChanged();
                    UpdateIban();
                }
            }
        }

        private string _IbanP7 = string.Empty;
        public string IbanP7
        {
            get { return _IbanP7; }
            set
            {
                if (this._IbanP7 != value)
                {
                    this._IbanP7 = value;
                    OnPropertyChanged();
                    UpdateIban();
                }
            }
        }

        private string _IbanP8 = string.Empty;
        public string IbanP8
        {
            get { return _IbanP8; }
            set
            {
                if (this._IbanP8 != value)
                {
                    this._IbanP8 = value;
                    OnPropertyChanged();
                    UpdateIban();
                }
            }
        }

        private string _IbanP9 = string.Empty;
        public string IbanP9
        {
            get { return _IbanP9; }
            set
            {
                if (this._IbanP9 != value)
                {
                    this._IbanP9 = value;
                    OnPropertyChanged();
                    UpdateIban();
                }
            }
        }

        private string _MaterialGroup = string.Empty;
        public string MaterialGroup
        {
            get { return _MaterialGroup; }
            set
            {
                if (this._MaterialGroup != value)
                {
                    this._MaterialGroup = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _PurchaserCode = "150";
        public string PurchaserCode
        {
            get { return _PurchaserCode; }
            set
            {
                if (this._PurchaserCode != value)
                {
                    this._PurchaserCode = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Iban = string.Empty;
        public string Iban
        {
            get { return _Iban; }
            set
            {
                if (this._Iban != value)
                {
                    this._Iban = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _Society = string.Empty;
        public string Society
        {
            get { return _Society; }
            set
            {
                if (this._Society != value)
                {
                    this._Society = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<PurchaseOrderAttachment> ListAttachment { get; set; } = new ObservableCollection<PurchaseOrderAttachment>();

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

        private bool _ToBeUpdated = false;
        public bool ToBeUpdated
        {
            get { return _ToBeUpdated; }
            set
            {
                if (this._ToBeUpdated != value)
                {
                    this._ToBeUpdated = value;
                    OnPropertyChanged();
                }

            }
        }

        #endregion

        #region [REGION] Properties not from Interface
        public double TotalCostOrdered { get; set; } = 0;
        public double TotalCostOrderedPourcentage { get; set; } = 0;
        #endregion

        #region [REGION] Misc
        public override string ToString()
        {
            return $"{Description}";
        }

        public string GetFullString()
        {
            return $"{Number} - {Description}";
        }

        internal static PurchaseOrderVendor GetVendorFromDbItem(PoVendor vendor)
        {
            try
            {
                if (vendor != null)
                {
                    return new PurchaseOrderVendor()
                    {
                        Description = vendor.NameVendor,
                        Number = vendor.NumberVendor,
                        IdVendor = vendor.Idvendor,
                        Location = vendor.Location
                    };
                }
                return null ;
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException("GetVendorFromDbItem", ex);
            }
        }

        private void UpdateIban()
        {
            try
            {
                Iban = $"{IbanP1}{IbanP2}{IbanP3}{IbanP4}{IbanP5}{IbanP6}{IbanP7}{IbanP8}{IbanP9}";
            }
            catch (Exception ex)
            {
                throw new PurchaseOrderFollowUpException("GetVendorFromDbItem", ex);
            }
        }
        #endregion

    }
}
