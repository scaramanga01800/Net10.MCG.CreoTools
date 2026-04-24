using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Models.Pdf;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.VisualizationLib.Exceptions;
using MCG.Tools.VisualizationLib.Interfaces;
using MCG.Tools.VisualizationLib.ViewModel;
using MCG.WindchillRequestTool;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.Services;
using MCG.WindchillRequestTool.Services.Interfaces;
using MCG.WindchillRequestTool.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using static System.Windows.Forms.AxHost;

namespace MCG.Tools.VisualizationLib.Services
{
    public class WtDownloadViewableTools : IWtDownloadViewableTools
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWindchillVisualizationManagementService _windchillVisualizationManagementService;
        private readonly IPdfTools _pdfTools;
        private readonly IWindchillRequestMiscService _windchillRequestMiscService;
        private readonly IWindchillChangeManagementService _windchillChangeManagementService;

        public WtDownloadViewableTools(IPdfTools pdfTools, 
                                       IWindchillRequestMiscService windchillRequestMiscService,
                                       IWindchillVisualizationManagementService windchillVisualizationManagementService,
                                       IWindchillChangeManagementService windchillChangeManagementService,
                                       IServiceProvider serviceProvider)
        {
            _pdfTools = pdfTools;
            _windchillRequestMiscService = windchillRequestMiscService;
            _windchillVisualizationManagementService = windchillVisualizationManagementService;
            _windchillChangeManagementService = windchillChangeManagementService;
            _serviceProvider = serviceProvider;
        }

        #region [REGION] Methods to search and download viewables

        public ViewableResult GetOneWtPartViewables(NetworkCredential WindchillCredential, WindchillObjectViewable CurrentPartViewable, WindchillObjectViewableItem CurrentPartViewableItem, bool IsWartermark = false, bool IsOptionalWatermark = false, string OptionalWatermark = "", bool ConvertToPdf = false, string TempFolder = null, string WindchillUrl = "Default")
        {
            try
            {
                ViewableResult CurrentViewableResult = new ViewableResult();
                CurrentViewableResult.ViewablePart = CurrentPartViewable;

                if (CurrentPartViewable != null && CurrentPartViewableItem != null)
                {
                    WindchillObjectViewableItemDownload CurrentViewable = null;
                    CurrentViewableResult.AllViewableDownload = new List<WindchillObjectViewableItemDownload>();
                    CurrentViewable = _windchillRequestMiscService.GetWindchillObjectViewableItemDownload(CurrentPartViewableItem);
                    CurrentViewable.IsWatermark = IsWartermark;
                    CurrentViewable.ConvertToPdf = ConvertToPdf;
                    CurrentViewable.WatermarkStateDrw = GetWatermarkStateDrw(CurrentPartViewable);
                    CurrentViewable.WatermarkPublishedBy = GetWatermarkPublishedBy();
                    CurrentViewable.OptionalWatermark = OptionalWatermark;
                    CurrentViewable.IsOptionalWatermark = IsOptionalWatermark;
                    CurrentViewableResult.AllViewableDownload.Add(CurrentViewable);
                }
                if (TempFolder == null)
                {
                    Random rnd = new Random();
                    TempFolder = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Visu_{rnd.Next(100000)}";
                }

                foreach (var doc in CurrentViewableResult.AllViewableDownload)
                    _windchillRequestMiscService.WindchillObjectViewableItemDownloadDownload(doc,WindchillCredential, TempFolder);

                CurrentViewableResult.IsViewableSearchSuccesfull = true;

                return CurrentViewableResult;
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }

        public ViewableResult GetOneWtPartViewables(NetworkCredential WindchillCredential, string Number, string Revision, string TempFolder = null, bool SearchEcnInformation = true, string EcnNumber = null, string WindchillUrl = "Default")
        {
            try
            {
                WindchillObjectViewable CurrentPartViewable;
                ViewableResult CurrenttViewableResult = new ViewableResult();

                if (SearchEcnInformation)
                    CurrentPartViewable = _windchillVisualizationManagementService.GetPartViewableChangeMgtInfo(WindchillCredential, Number, Revision, WindchillUrl);
                else
                {
                    CurrentPartViewable = _windchillVisualizationManagementService.GetPartViewable(WindchillCredential, Number, Revision, WindchillUrl);
                    if (CurrentPartViewable != null && CurrentPartViewable.CurrentPart != null)
                        CurrentPartViewable.CurrentPart.EcnNumber = EcnNumber;
                }

                CurrenttViewableResult.ViewablePart = CurrentPartViewable;

                if (CurrentPartViewable != null)
                {
                    WindchillObjectViewableItemDownload CurrentViewable = null;
                    CurrenttViewableResult.AllViewableDownload = new List<WindchillObjectViewableItemDownload>();
                    if (CurrentPartViewable.IsDrwViewableAvailable)
                    {
                        CurrentViewable = _windchillRequestMiscService.GetWindchillObjectViewableItemDownload(CurrentPartViewable.DrwViewable);
                        CurrentViewable.IsWatermark = true;
                        CurrentViewable.WatermarkStateDrw = GetWatermarkStateDrw(CurrentPartViewable);
                        CurrentViewable.WatermarkPublishedBy = GetWatermarkPublishedBy();
                        CurrenttViewableResult.AllViewableDownload.Add(CurrentViewable);
                    }
                    else if (CurrentPartViewable.IsDescribedDocAvailable)
                    {
                        CurrentViewable = _windchillRequestMiscService.GetWindchillObjectViewableItemDownload(CurrentPartViewable.DescribedDoc);
                        CurrentViewable.IsWatermark = true;
                        CurrentViewable.WatermarkStateDrw = GetWatermarkStateDrw(CurrentPartViewable);
                        CurrentViewable.WatermarkPublishedBy = GetWatermarkPublishedBy();
                        CurrenttViewableResult.AllViewableDownload.Add(CurrentViewable);
                    }
                    if (CurrentPartViewable.IsReferenceDocAvailable)
                    {
                        CurrentViewable = _windchillRequestMiscService.GetWindchillObjectViewableItemDownload(CurrentPartViewable.ReferenceDoc);
                        CurrentViewable.IsWatermark = true;
                        CurrentViewable.WatermarkStateDrw = GetWatermarkStateDrw(CurrentPartViewable);
                        CurrentViewable.WatermarkPublishedBy = GetWatermarkPublishedBy();
                        CurrenttViewableResult.AllViewableDownload.Add(_windchillRequestMiscService.GetWindchillObjectViewableItemDownload(CurrentPartViewable.ReferenceDoc));
                    }
                }
                if (TempFolder == null)
                {
                    Random rnd = new Random();
                    TempFolder = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Visu_{rnd.Next(100000)}";
                }

                foreach (var doc in CurrenttViewableResult.AllViewableDownload)
                    _windchillRequestMiscService.WindchillObjectViewableItemDownloadDownload(doc, WindchillCredential, TempFolder);

                CurrenttViewableResult.IsViewableSearchSuccesfull = true;
                return CurrenttViewableResult;
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }

        public List<ViewableResult> GetWtPartViewablesFromEcn(NetworkCredential WindchillCredential, string EcnNumber, string TempFolder = null, string WindchillUrl = "Default")
        {
            try
            {
                List<ViewableResult> CurrentViewableList = new List<ViewableResult>();
                RestOdataChangeNoticeUpper CurrentEcn = _windchillChangeManagementService.GetChangeNoticeWithWtPart(WindchillCredential, EcnNumber, true, WindchillUrl);
                ViewableResult TempViewableResult;
                if (CurrentEcn != null && CurrentEcn.value != null && CurrentEcn.value.Count > 0)
                {

                    var Ecn = CurrentEcn.value.FirstOrDefault();
                    if (Ecn.ListWtPart != null)
                    {
                        if (TempFolder == null)
                        {
                            Random rnd = new Random();
                            TempFolder = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Visu_{rnd.Next(100000)}";
                        }
                        foreach (var part in Ecn.ListWtPart)
                        {
                            TempViewableResult = GetOneWtPartViewables(WindchillCredential, part.Number, part.Revision, TempFolder, false, EcnNumber, WindchillUrl);
                            CurrentViewableList.Add(TempViewableResult);
                        }
                    }
                }
                return CurrentViewableList;
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }

        public List<ViewableResult> GetWtPartViewablesFromList(NetworkCredential WindchillCredential, List<McgObjectNumber> PartList, string TempFolder = null, string WindchillUrl = "Default")
        {
            try
            {
                List<ViewableResult> CurrentViewableList = new List<ViewableResult>();
                ViewableResult TempViewableResult;

                if (PartList != null && PartList.Count > 0)
                {
                    if (TempFolder == null)
                    {
                        Random rnd = new Random();
                        TempFolder = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Visu_{rnd.Next(100000)}";
                    }
                    foreach (var currentPart in PartList)
                    {
                        TempViewableResult = GetOneWtPartViewables(WindchillCredential, currentPart.Number, currentPart.Revision, TempFolder, true, null, WindchillUrl);
                        CurrentViewableList.Add(TempViewableResult);
                    }
                }
                return CurrentViewableList;
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }

        public string GetWatermarkStateDrw(WindchillObjectViewable CurrentPartViewable)
        {
            try
            {
                string Watermak = "";
                if (CurrentPartViewable != null && CurrentPartViewable.CurrentPart != null)
                {
                    if (CurrentPartViewable.CurrentPart.EcnNumber != null)
                        Watermak = $"{CurrentPartViewable.CurrentPart.State.ToUpper()} throw {CurrentPartViewable.CurrentPart.EcnNumber} on {CurrentPartViewable.CurrentPart.EcnModifiedOn.ToShortDateString()}";
                    else
                        Watermak = $"{CurrentPartViewable.CurrentPart.State.ToUpper()}";
                }
                return Watermak;
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }

        public string GetWatermarkPublishedBy()
        {
            try
            {
                string Watermak = "";
                Watermak = $"Published by {McgActiveDirectoryTools.GetWindowsSessionUserFullName()} on {DateTime.Today.ToShortDateString()}";
                return Watermak;
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }

        public bool DownloadPartMainDrwing( string number, string revision = "Latest", DocumentTypeEnum itemType = DocumentTypeEnum.PART, bool isCreateZip = false)
        {
            try
            {
                var visuViewModel = _serviceProvider.GetRequiredService<DownloadVisualizationFileViewModel>();
                visuViewModel.CurrentDataContext.SearchedPartList.Add(new VisualizationItem
                {
                    PartNumber = number,
                    PartRevision = revision,
                    ItemType = itemType
                });

                visuViewModel.ExecuteSearchVisuFileNotAsynch();
                VisualizationItem CurrentVisuItem = visuViewModel.CurrentDataContext.SearchedPartList.FirstOrDefault();
                visuViewModel.CurrentDataContext.IsCreateZip = isCreateZip;
                var listDoc = CurrentVisuItem.SearchedDocumentList.Where((item) => item.IsMainDrawing).ToList();
                if (listDoc != null && listDoc.Count > 0)
                {
                    foreach (var doc in listDoc)
                        doc.IsSelected = true;

                    visuViewModel.ExecuteDownloadVisuFilesNotAsynch();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }
        #endregion

        #region [REGION] Methods to download and create ZIP
        public List<ViewableResult> DownloadOnePart(NetworkCredential WindchillCredential, string Number, string Revision, bool IsWatermarkConsultationOnly = false, bool ZipFiles = true, string TempFolder = null)
        {
            try
            {
                if (TempFolder == null || TempFolder.Trim() == "")
                {
                    Random rnd = new Random();
                    TempFolder = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Visu_{rnd.Next(100000)}";
                }

                List<ViewableResult> ListView = new List<ViewableResult>();
                ListView.Add(GetOneWtPartViewables(WindchillCredential, Number.Trim().ToUpper(), Revision.Trim().ToUpper(), TempFolder));

                if (ListView != null && ListView.Count > 0)
                {
                    string ZipFileName = $"{TempFolder}\\{Number}_{Revision}.zip";
                    return UpdatePdfTif(ListView, IsWatermarkConsultationOnly, ZipFiles, ZipFileName);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }

        public List<ViewableResult> DownloadListPart(NetworkCredential WindchillCredential, List<McgObjectNumber> ListPart, bool IsWatermarkConsultationOnly = false, bool ZipFiles = true, string TempFolder = null)
        {
            try
            {
                Random rnd = new Random();
                if (TempFolder == null || TempFolder.Trim() == "")
                    TempFolder = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Visu_{rnd.Next(100000)}";

                List<ViewableResult> ListView = GetWtPartViewablesFromList(WindchillCredential, ListPart, TempFolder);

                if (ListView != null && ListView.Count > 0)
                {
                    string ZipFileName = $"{TempFolder}\\Part_List_{rnd.Next(100000)}.zip";
                    return UpdatePdfTif(ListView, IsWatermarkConsultationOnly, ZipFiles, ZipFileName);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }

        public List<ViewableResult> DownloadFromFile(NetworkCredential WindchillCredential, string CompleteFileName, bool IsWatermarkConsultationOnly = false, bool ZipFiles = true, string TempFolder = null)
        {
            try
            {
                Regex FileCsvXls = new Regex(@".csv$|.xls$|.xlsx$|.xlsm$", RegexOptions.IgnoreCase);
                if (CompleteFileName != null && File.Exists(CompleteFileName) && FileCsvXls.IsMatch(CompleteFileName))
                {
                    List<McgObjectNumber> ListPart = new List<McgObjectNumber>();

                    ExcelTools CurrentXls = new ExcelTools(CompleteFileName);
                    if (CurrentXls.OpenRead() == ExcelStatus.OK)
                    {
                        //CurrentXls.SetCurrentWorksheet(null);
                        int LinIndex = 1;
                        object value;
                        string Number = null;
                        string Revision = null;
                        value = CurrentXls.GetCellValue(LinIndex, 1);
                        if (value != null) Number = value.ToString();
                        value = CurrentXls.GetCellValue(LinIndex, 2);
                        if (value != null) Revision = value.ToString();

                        while (Number != null && Number.Trim() != "")
                        {
                            ListPart.Add(new McgObjectNumber() { Number = Number.ToUpper(), Revision = Revision.ToUpper() });
                            LinIndex++;
                            Number = null;
                            Revision = null;
                            value = CurrentXls.GetCellValue(LinIndex, 1);
                            if (value != null) Number = value.ToString();
                            value = CurrentXls.GetCellValue(LinIndex, 2);
                            if (value != null) Revision = value.ToString();
                        }

                    }
                    return DownloadListPart(WindchillCredential, ListPart, IsWatermarkConsultationOnly, ZipFiles, TempFolder);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }

        public List<ViewableResult> DownloadEcn(NetworkCredential WindchillCredential, string EcnNumber, bool IsWatermarkConsultationOnly = false, bool ZipFiles = true)
        {
            try
            {
                Random rnd = new Random();
                string TempFolder = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Visu_{rnd.Next(100000)}";

                List<ViewableResult> ListView = GetWtPartViewablesFromEcn(WindchillCredential, EcnNumber.Trim().ToUpper(), TempFolder);

                if (ListView != null && ListView.Count > 0)
                {
                    string ZipFileName = $"{TempFolder}\\{EcnNumber}.zip";
                    return UpdatePdfTif(ListView, IsWatermarkConsultationOnly, ZipFiles, ZipFileName);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }

        private List<ViewableResult> UpdatePdfTif(List<ViewableResult> ListView, bool IsWatermarkConsultationOnly = false, bool ZipFiles = true, string CompleteZipFileName = null)
        {
            try
            {
                List<string> FileList = new List<string>();
                foreach (ViewableResult CurrentViewableResult in ListView)
                {
                    if (CurrentViewableResult.AllViewableDownload != null)
                    {
                        string PdfFileName = null;
                        // Search Tif files and convert to PDF and add watermark
                        foreach (var doc in CurrentViewableResult.AllViewableDownload.Where((item) => item.Format.ToUpper().Contains("TIF")))
                        {
                            PdfFileName = doc.CompleteFileName.ToUpper().Replace("TIF", "PDF").Replace("TIF", "PDF");
                            doc.IsConvertToPdfSuccesfull = _pdfTools.ConvertTifToPdf(doc.CompleteFileName, PdfFileName);
                            doc.FileName = doc.FileName.ToUpper().Replace("TIF", "PDF").Replace("TIF", "PDF");
                            doc.CompleteFileName = PdfFileName;
                        }

                        // Search PDF to add watermark
                        foreach (var doc in CurrentViewableResult.AllViewableDownload.Where((item) => item.IsWatermark && item.Format.ToUpper().Contains("PDF")))
                        {
                            doc.IsConvertToPdfSuccesfull = true;
                            doc.IsWatermerkSuccesfull = _pdfTools.AddWatermarkToPdfDrawing(doc.CompleteFileName, doc.WatermarkStateDrw, IsWatermarkConsultationOnly);
                        }

                        FileList.AddRange(CurrentViewableResult.AllViewableDownload.Select((item) => item.CompleteFileName).ToList());
                    }
                }

                // Create Zip File
                if (ZipFiles && CompleteZipFileName != null)
                {
                    if (File.Exists(CompleteZipFileName))
                        File.Delete(CompleteZipFileName);
                    string ZipFileName = CompleteZipFileName.Split('\\').LastOrDefault();
                    string ZipFolder = CompleteZipFileName.Replace(ZipFileName, "");
                    McgFileAndSystemTools.CreateZipFile(ZipFileName, ZipFolder, FileList);
                }

                return ListView;
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }

        public void CreateZipFromListViewable(List<ViewableResult> ListView, string ZipFileNameComplete, bool IsBackupFile = false, string BackupFolder = null, bool ActivatePdfSecurity = false, PdfToolsSecuritySetting CurrentPdfsecurity = null, bool IsTiffToPdf = false, bool IsWordToPdf = false, bool IsExcelToPdf = false, bool IsPowerPointToPdf = false, bool IsCreateZip = true)
        {
            try
            {
                List<string> FileList = new List<string>();
                List<string> TempFileList = new List<string>();
                List<WindchillObjectViewableItemDownload> AllViewableResult = new List<WindchillObjectViewableItemDownload>();

                string CompleteBackupFile = "";

                if (IsBackupFile && BackupFolder != null && Directory.Exists(BackupFolder))
                    IsBackupFile = true;
                else
                    IsBackupFile = false;

                foreach (var CurrentViewableResult in ListView.Where((item) => item != null && item.AllViewableDownload != null))
                {
                    foreach (var currentViewable in CurrentViewableResult.AllViewableDownload)
                    {
                        if (AllViewableResult.FirstOrDefault((tempViewable) => tempViewable.CompleteFileName == currentViewable.CompleteFileName) == null)
                        {
                            if (IsBackupFile)
                            {
                                if (string.IsNullOrWhiteSpace(currentViewable.CompleteOrigFileName))
                                    currentViewable.CompleteOrigFileName = currentViewable.CompleteFileName;
                                if (string.IsNullOrWhiteSpace(currentViewable.FileName))
                                    currentViewable.FileName = currentViewable.CompleteFileName.Split('\\').LastOrDefault();
                                //if (currentViewable.CompleteOrigFileName == null || currentViewable.CompleteOrigFileName.Trim() == "")
                                //    currentViewable.CompleteOrigFileName = currentViewable.CompleteFileName;
                                //if (currentViewable.FileName == null || currentViewable.FileName.Trim() == "")
                                //    currentViewable.FileName = currentViewable.CompleteFileName.Split('\\').LastOrDefault();


                                CompleteBackupFile = $"{BackupFolder}\\{currentViewable.FileName}";
                                if (!File.Exists(CompleteBackupFile))
                                    File.Copy(currentViewable.CompleteOrigFileName, CompleteBackupFile);

                                currentViewable.CompleteFileName = CompleteBackupFile;
                            }

                            if (AllViewableResult.FirstOrDefault((item) => item.CompleteFileName == currentViewable.CompleteFileName) == null)
                                AllViewableResult.Add(currentViewable);
                        }
                    }
                }

                string PdfFileName = null;
                Regex TiffRegex = new Regex(@".+\.tiff$|.+\.tif$", RegexOptions.IgnoreCase);
                Regex WordRegex = new Regex(@".+\.docx$|.+\.docm$|.+\.doc$", RegexOptions.IgnoreCase);
                Regex ExcelRegex = new Regex(@".+\.xlsx$|.+\.xlsm$|.+\.xls$", RegexOptions.IgnoreCase);
                Regex PowerPointRegex = new Regex(@".+\.pptx$|.+\.pptm$|.+\.ppt$", RegexOptions.IgnoreCase);
                string ExtensionDoc = null;
                foreach (var currentViewable in AllViewableResult)
                {
                    if (currentViewable.CompleteFileName != null)
                    {
                        ExtensionDoc = currentViewable.CompleteFileName.Split('.').LastOrDefault();
                        PdfFileName = currentViewable.CompleteFileName.Replace($".{ExtensionDoc}", ".PDF");

                        if (IsTiffToPdf && currentViewable.Format != null && currentViewable.Format.ToUpper().Contains("TIF"))
                        {
                            currentViewable.IsConvertToPdfSuccesfull = _pdfTools.ConvertTifToPdf(currentViewable.CompleteOrigFileName, PdfFileName);
                            if (currentViewable.IsConvertToPdfSuccesfull)
                            {
                                currentViewable.FileName = currentViewable.FileName.ToUpper().Replace($".{ExtensionDoc}", ".PDF");
                                currentViewable.CompleteFileName = PdfFileName;
                            }
                        }

                        if (IsWordToPdf && WordRegex.IsMatch(currentViewable.FileName))
                        {
                            currentViewable.IsConvertToPdfSuccesfull = _pdfTools.ConvertWordToPdf(currentViewable.CompleteOrigFileName, PdfFileName);
                            if (currentViewable.IsConvertToPdfSuccesfull)
                            {
                                currentViewable.FileName = currentViewable.FileName.ToUpper().Replace($".{ExtensionDoc}", ".PDF");
                                currentViewable.CompleteFileName = PdfFileName;
                            }
                        }

                        if (IsExcelToPdf && ExcelRegex.IsMatch(currentViewable.FileName))
                        {
                            currentViewable.IsConvertToPdfSuccesfull = _pdfTools.ConvertExcelToPdf(currentViewable.CompleteOrigFileName, PdfFileName);
                            if (currentViewable.IsConvertToPdfSuccesfull)
                            {
                                currentViewable.FileName = currentViewable.FileName.ToUpper().Replace($".{ExtensionDoc}", ".PDF");
                                currentViewable.CompleteFileName = PdfFileName;
                            }
                        }

                        if (IsPowerPointToPdf && PowerPointRegex.IsMatch(currentViewable.FileName))
                        {
                            currentViewable.IsConvertToPdfSuccesfull = _pdfTools.ConvertPowerPointToPdf(currentViewable.CompleteFileName, PdfFileName);
                            if (currentViewable.IsConvertToPdfSuccesfull)
                            {
                                currentViewable.FileName = currentViewable.FileName.ToUpper().Replace($".{ExtensionDoc}", ".PDF");
                                currentViewable.CompleteFileName = PdfFileName;
                            }
                        }

                        //if (currentViewable.Format != null && currentViewable.IsWatermark && currentViewable.Format.ToUpper().Contains("PDF"))
                        if (currentViewable.IsWatermark && currentViewable.CompleteFileName.ToUpper().Contains(".PDF"))
                        {
                            currentViewable.IsConvertToPdfSuccesfull = true;
                            //currentViewable.IsWatermerkSuccesfull = PdfTools.AddWatermarkToPdfDrawing(currentViewable.CompleteFileName, currentViewable.WatermarkStateDrw, currentViewable.IsOptionalWatermark, currentViewable.OptionalWatermark);
                            //currentViewable.IsWatermerkSuccesfull = PdfTools.AddWatermarkToPdfDrawing(currentViewable.CompleteFileName, false, true, false, true, currentViewable.IsOptionalWatermark, "", currentViewable.WatermarkStateDrw, "", currentViewable.WatermarkPublishedBy, currentViewable.OptionalWatermark);
                            if (currentViewable.ListWatermark != null)
                                foreach (var watermark in currentViewable.ListWatermark)
                                    _pdfTools.AddWatermarkToPdfDrawing(currentViewable.CompleteFileName, watermark);
                        }
                    }
                }
                FileList.AddRange(AllViewableResult.Select((item) => item.CompleteFileName).ToList());

                // Set PDF Security
                if (ActivatePdfSecurity)
                    foreach (var pdf in FileList.Where((item) => item.ToUpper().Contains(".PDF")))
                        _pdfTools.SetPdfSecurity(pdf, CurrentPdfsecurity);

                // Create Zip File
                if (IsCreateZip && ZipFileNameComplete != null)
                {
                    if (File.Exists(ZipFileNameComplete))
                        File.Delete(ZipFileNameComplete);
                    string ZipFileName = ZipFileNameComplete.Split('\\').LastOrDefault();
                    string ZipFolder = ZipFileNameComplete.Replace(ZipFileName, "");
                    McgFileAndSystemTools.CreateZipFile(ZipFileName, ZipFolder, FileList);
                }
            }
            catch (Exception ex)
            {
                throw new VisualizationException("WtDownloadViewableTools", ex);
            }
        }
        #endregion

        #region [REGION] Methods to download and create ZIP from WebServices
        public void DownloadWtPartViewableWebService(NetworkCredential Windchillcredential, string PartNumber, string PartRevision, string WindchillUrl = "default")
        {
            try
            {
                Random rnd = new Random();
                string TempFolder = null; //$"temp\\Visu_{rnd.Next(100000)}";
                List<ViewableResult> ZipList = DownloadOnePart(Windchillcredential, PartNumber, PartRevision, true, true, TempFolder);

            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region [REGION] Methods from VisualizationItem
        public void UpdateSelectedRevisionInformation(VisualizationItem currentItem)
        {
            try
            {
                currentItem.IsDocumentFound = false;
                currentItem.IsDocumentSearched = false;
                currentItem.IsAllSelected = false;
                currentItem.SearchedDocumentList.Clear();
                currentItem.SearchedCompleteDocumentList.Clear();
                if (currentItem.AllOdataWtPartRevision != null)
                {
                    RestOdataWtObject CurrentOdataPart = currentItem.AllOdataWtPartRevision.FirstOrDefault((item) => item.Revision == currentItem.PartRevision);
                    if (CurrentOdataPart != null)
                    {
                        WindchillObjectWtPart CurrentWindchillPart = _windchillRequestMiscService.GetWindchillPart(CurrentOdataPart.GetWtPart());
                        currentItem.State = CurrentOdataPart.State.Display;
                        currentItem.DescriptionEng = $"{CurrentWindchillPart.Name}|{CurrentWindchillPart.DescriptionEn2}";
                        currentItem.DescriptionLocal = $"{CurrentWindchillPart.DescriptionLocal1}|{CurrentWindchillPart.DescriptionLocal2}";
                        currentItem.PdmContext = CurrentWindchillPart.Context.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
