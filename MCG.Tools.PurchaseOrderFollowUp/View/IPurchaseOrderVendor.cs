using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    public interface IPurchaseOrderVendor
    {
        string Description { get; set; }  
        string Number { get; set; }
        string Location { get; set; }
        string DescriptionShort { get; set; }
        string StreetNumber { get; set; }
        string StreetName { get; set; }
        string City { get; set; }
        string PostalCode { get; set; }
        string Country { get; set; }
        string Langue { get; set; }
        string CompanyTel { get; set; }
        string CompanyMail { get; set; }
        string BusinessType { get; set; }
        string ContactFirstName { get; set; }
        string ContactLastName { get; set; }
        string ContactDepartment { get; set; }
        string ContactTel { get; set; }
        string ContactEmail { get; set; }
        string Siret { get; set; }
        string Siren { get; set; }
        string Tva { get; set; }
        string BankCountry { get; set; }
        string IbanP1 { get; set; }
        string IbanP2 { get; set; }
        string IbanP3 { get; set; }
        string IbanP4 { get; set; }
        string IbanP5 { get; set; }
        string IbanP6 { get; set; }
        string IbanP7 { get; set; }
        string IbanP8 { get; set; }
        string IbanP9 { get; set; }
        string MaterialGroup { get; set; }
        string PurchaserCode { get; set; }
        string Iban { get; set; }
        string Society { get; set; }
        ObservableCollection<PurchaseOrderAttachment> ListAttachment { get; set; }
        PurchaseOrderAttachment SelectedAttachment { get; set; }
        bool ToBeUpdated { get; set; }
    }
}
