using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Models.Pdf;
using MCG.Tools.VisualizationLib.ViewModel;
using MCG.WindchillRequestTool;
using System.Net;

namespace MCG.Tools.VisualizationLib.Interfaces
{
    public interface IWtDownloadViewableTools
    {
        void CreateZipFromListViewable(List<ViewableResult> ListView, string ZipFileNameComplete, bool IsBackupFile = false, string BackupFolder = null, bool ActivatePdfSecurity = false, PdfToolsSecuritySetting CurrentPdfsecurity = null, bool IsTiffToPdf = false, bool IsWordToPdf = false, bool IsExcelToPdf = false, bool IsPowerPointToPdf = false, bool IsCreateZip = true);
        List<ViewableResult> DownloadEcn(NetworkCredential WindchillCredential, string EcnNumber, bool IsWatermarkConsultationOnly = false, bool ZipFiles = true);
        List<ViewableResult> DownloadFromFile(NetworkCredential WindchillCredential, string CompleteFileName, bool IsWatermarkConsultationOnly = false, bool ZipFiles = true, string TempFolder = null);
        List<ViewableResult> DownloadListPart(NetworkCredential WindchillCredential, List<McgObjectNumber> ListPart, bool IsWatermarkConsultationOnly = false, bool ZipFiles = true, string TempFolder = null);
        List<ViewableResult> DownloadOnePart(NetworkCredential WindchillCredential, string Number, string Revision, bool IsWatermarkConsultationOnly = false, bool ZipFiles = true, string TempFolder = null);
        void DownloadWtPartViewableWebService(NetworkCredential Windchillcredential, string PartNumber, string PartRevision, string WindchillUrl = "default");
        ViewableResult GetOneWtPartViewables(NetworkCredential WindchillCredential, string Number, string Revision, string TempFolder = null, bool SearchEcnInformation = true, string EcnNumber = null, string WindchillUrl = "Default");
        ViewableResult GetOneWtPartViewables(NetworkCredential WindchillCredential, WindchillObjectViewable CurrentPartViewable, WindchillObjectViewableItem CurrentPartViewableItem, bool IsWartermark = false, bool IsOptionalWatermark = false, string OptionalWatermark = "", bool ConvertToPdf = false, string TempFolder = null, string WindchillUrl = "Default");
        string GetWatermarkPublishedBy();
        string GetWatermarkStateDrw(WindchillObjectViewable CurrentPartViewable);
        List<ViewableResult> GetWtPartViewablesFromEcn(NetworkCredential WindchillCredential, string EcnNumber, string TempFolder = null, string WindchillUrl = "Default");
        List<ViewableResult> GetWtPartViewablesFromList(NetworkCredential WindchillCredential, List<McgObjectNumber> PartList, string TempFolder = null, string WindchillUrl = "Default");
        void UpdateSelectedRevisionInformation(VisualizationItem currentItem);
    }
}