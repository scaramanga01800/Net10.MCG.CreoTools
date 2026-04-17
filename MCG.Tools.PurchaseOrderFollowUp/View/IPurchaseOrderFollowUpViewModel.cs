using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System.Windows.Input;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    public interface IPurchaseOrderFollowUpViewModel
    {
        PurchaseOrderFollowUpDataContext CurrentDataContext { get; set; }

        ICommand CommandOpenHelp { get; }
        ICommand CommandCreatePurchaseOrder { get; }
        ICommand CommandUpdatePurchaseOrder { get; }
        ICommand CommandCreateItem { get; }
        ICommand CommandRemoveItem { get; }
        ICommand CommandSaveRequest { get; }
        ICommand CommandDrop { get; }
        ICommand CommandDropVendor { get; }
        ICommand CommandRemoveAttachement { get; }
        ICommand CommandRemoveAttachementVendor { get; }
        ICommand CommandOpenAttachment { get; }
        ICommand CommandOpenAttachmentVendor { get; }
        ICommand CommandSendRequest { get; }
        ICommand CommandSetSearchOrder { get; }
        ICommand CommandSetSearchVendor { get; }
        ICommand CommandSearchItem { get; }
        ICommand CommandSelectItem { get; }
        ICommand CommandSelectVendor { get; }
        ICommand CommandSearchRquestType { get; }
        ICommand CommandAskNewVendor { get; }
        ICommand CommandAskUpdateVendor { get; }
        ICommand CommandAskNewInternalOrder { get; }
        ICommand CommandAskExtendPart { get; }
        ICommand CommandSendVendorRequest { get; }
        ICommand CommandShowVendor { get; }
        ICommand CommandAddRequestFromSapPr { get; }
        ICommand CommandAddRequestFromSapPo { get; }
        ICommand CommandAddRequestFromSapPoDate { get; }
        ICommand CommandAddInternalOrder { get; }
        ICommand CommandUpdateInternalOrder { get; }
        ICommand CommandDeleteInternalOrder { get; }
        ICommand CommandAddRequestFromSapPoDates { get; }
        ICommand CommandDeleteRequest { get; }
        ICommand CommandUpdatePrFromSapHub { get; }
        ICommand CommandUpdatePoFromSapHub { get; }
        ICommand CommandCopyPurchaseRequest { get; }
        ICommand CommandCopyPurchaseOrder { get; }
        ICommand CommandOpenPurchaseRequest { get; }
        ICommand CommandOpenPurchaseOrder { get; }
        ICommand CommandOpenResa { get; }
        ICommand CommandStartUpdateAllRequestFromSap { get; }
        ICommand CommandCreateSapRequest { get; }
        ICommand CommandShowSapStock { get; }
        ICommand CommandConvertSapRequest { get; }
        ICommand CommandDownloadAttachment { get; }
        ICommand CommandSearchColumnKeyWord { get; }
        ICommand CommandOpenPurchaseOrderPdf { get; }
        ICommand CommandCopyVendorNumber { get; }
        ICommand CommandReceiptPurchaseOrder { get; }
        ICommand CommandSearchDuplicateRequest { get; }
        ICommand CommandAdminUpdute { get; }
        ICommand CommandCopyIONumber { get; }
        ICommand CommandCheckExtendPart { get; }
    }
}
