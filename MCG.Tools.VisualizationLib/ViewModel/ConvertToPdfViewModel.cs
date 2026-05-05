using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Pdf;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.Tools.VisualizationLib.Configuration;
using MCG.Tools.VisualizationLib.Exceptions;
using MCG.Tools.VisualizationLib.Interfaces;
using MCG.Tools.VisualizationLib.Services;
using MCG.Tools.VisualizationLib.View;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.Tools.VisualizationLib.ViewModel
{
    public class ConvertToPdfViewModel : ObservableObject, IConvertToPdfViewModel
    {
        #region [REGION] Events Action
        public event EventHandler ActionInProgressEvent;
        public void RaiseActionInProgressEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                ActionInProgressEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler ActionDoneEvent;
        public void RaiseActionDoneEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                ActionDoneEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Properties from Interface
        public ConvertToPdfDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        Dispatcher MainDispatcher { get; set; } = null;
        public DownloadVisualizationFileConfiguration CurrentDownloadVisuConfiguration { get; set; }

        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly IPdfTools _pdfTools;
        private readonly ITiffTools _tiffTools;
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        private readonly IMcgToolsVisualizationLibWindowService _mcgToolsVisualizationLibWindowService;
        #endregion

        #region [REGION] Commands
        public ICommand CommandDrop { get => new RelayCommand<DragEventArgs>((obj) => ExecuteDrop(obj)); }
        public ICommand CommandStartConvert { get => new RelayCommand(() => ExecuteStartConvert()); }
        public ICommand CommandRemoveAll { get => new RelayCommand(() => ExecuteRemoveAll()); }
        public ICommand CommandChangeExportFolder { get => new RelayCommand(() => ExecuteChangeExportFolder()); }
        public ICommand CommandOpenFolder { get => new RelayCommand(() => McgFileAndSystemTools.OpenFolder(CurrentDataContext.ExportFolder)); }
        public ICommand CommandMergeTiff { get => new RelayCommand(() => ExecuteMergeTiff()); }
        public ICommand CommandMergePdf { get => new RelayCommand(() => ExecuteMergePdf()); }
        public ICommand CommandCheckUncheckAll { get => new RelayCommand<bool>((ischecked) => ExecuteCheckUncheckAll(ischecked)); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        #endregion

        #region [REGION] Init

        public ConvertToPdfViewModel(ConvertToPdfDataContext currentDC,
                                     IXmlSerializeTools xmlSerializeTools,
                                     IPdfTools pdfTools,
                                     ITiffTools tiffTools,
                                     IMcgCommonLibWindowService mcgCommonLibWindowService,
                                     IMcgToolsVisualizationLibWindowService mcgToolsVisualizationLibWindowService)
        {
            try
            {
                CurrentDataContext = currentDC;
                MainDispatcher = Dispatcher.CurrentDispatcher;
                _xmlSerializeTools = xmlSerializeTools;
                _pdfTools = pdfTools;
                _tiffTools = tiffTools;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;
                _mcgToolsVisualizationLibWindowService = mcgToolsVisualizationLibWindowService;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                CurrentDataContext.ExportFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                CurrentDownloadVisuConfiguration = _xmlSerializeTools.GetDeserializedXml<DownloadVisualizationFileConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{VisualizationLibConstants.ConfigurationFile}");

                // Update Optional Watermark Values
                if (CurrentDownloadVisuConfiguration.OptionalWatermarkValues != null && CurrentDataContext.OptionalWatermarkValues != null)
                {
                    foreach (var value in CurrentDownloadVisuConfiguration.OptionalWatermarkValues)
                        CurrentDataContext.OptionalWatermarkValues.Add(value);
                    CurrentDataContext.OptionalWatermark = CurrentDataContext.OptionalWatermarkValues.FirstOrDefault();
                }

                CurrentDataContext.OptionalWatermark = CurrentDataContext.OptionalWatermarkValues.FirstOrDefault();

                ActionInProgressEvent += (sender, e) => CurrentDataContext.ActionInProgress = true;
                ActionDoneEvent += (sender, e) => CurrentDataContext.ActionInProgress = false;
                CurrentDataContext.SecurityWatermarkChangeEvent += CurrentDataContext_SecurityChangeEvent;
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex); ;
            }
        }

        private void CurrentDataContext_SecurityChangeEvent(object sender, EventArgs e)
        {
            try
            {
                foreach (var item in CurrentDataContext.ListConvertItem)
                    item.IsConvertToPdfSuccesfull = false;
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void InitApp()
        {
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteDrop(DragEventArgs obj)
        {
            try
            {
                if (obj != null)
                    if (obj != null && obj.Data != null && obj.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        List<string> Newfiles = ((string[])obj.Data.GetData(DataFormats.FileDrop)).OrderBy(item => item).ToList();
                        foreach (var file in Newfiles)
                        {
                            if (CurrentDataContext.ListConvertItem.FirstOrDefault((item) => item.OrigFileName != null && item.OrigFileName == file) == null)
                                CurrentDataContext.ListConvertItem.Add(new ConvertToPdfItem()
                                {
                                    OrigFileName = file,
                                    Status = McgWpfTools.GetStringResource("VIS_ColStatusNotConverted")
                                });
                        }
                    }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartConvert()
        {
            try
            {
                RaiseActionInProgressEvent();
                Thread ThreadSearchPart = new Thread(() => StartConvertPdf());
                ThreadSearchPart.Start();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemoveAll()
        {
            try
            {
                var tempList = CurrentDataContext.ListConvertItem.Where(item => item.IsSelected).ToList();
                foreach (var temp in tempList)
                    CurrentDataContext.ListConvertItem.Remove(temp);
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteChangeExportFolder()
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
                openFolderDialog.ShowDialog();
                if (openFolderDialog.SelectedPath != "")
                {
                    CurrentDataContext.ExportFolder = openFolderDialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMergeTiff()
        {
            try
            {
                List<ConvertToPdfItem> ListPdfItem = new List<ConvertToPdfItem>();
                foreach (var item in CurrentDataContext.ListConvertItem.Where(pdf => pdf.IsSelected))
                    ListPdfItem.Add(item);
                if (ListPdfItem.Count > 0)
                {
                    string TiffFileName = $"{ListPdfItem.First().OrigFileName.Split('.').FirstOrDefault().Split('\\').LastOrDefault()}_merge";

                    var returnWindow = _mcgToolsVisualizationLibWindowService.ShowDialogConvertToPdfMergeWindowView(ListPdfItem, TiffFileName);
                    //ConvertToPdfMergeWindowView CurrentWindow = new ConvertToPdfMergeWindowView(ListPdfItem, TiffFileName);
                    //CurrentWindow.ShowDialog();
                    if (returnWindow.ResultDialog == MessageBoxResult.OK)
                    {
                        RaiseActionInProgressEvent();
                        //TiffFileName = returnWindow.FileName; // CurrentWindow.CurrentDataContext.FileName.Split('.').FirstOrDefault();
                        TiffFileName = $"{CurrentDataContext.ExportFolder}\\{returnWindow.FileName}.tif";
                        Thread ThreadSearchPart = new Thread(() => StartMergeTif(TiffFileName, ListPdfItem));
                        ThreadSearchPart.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMergePdf()
        {
            try
            {
                List<ConvertToPdfItem> ListPdfItem = new List<ConvertToPdfItem>();
                foreach (var item in CurrentDataContext.ListConvertItem.Where(pdf => pdf.IsSelected))
                    ListPdfItem.Add(item);

                if (ListPdfItem.Count > 0)
                {
                    //string PdfFileName = $"{CurrentDataContext.ListConvertItem.First().OrigFileName.Split('.').FirstOrDefault().Split('\\').LastOrDefault()}_merge";
                    string PdfFileName = $"{ListPdfItem.First().OrigFileName.Split('\\').LastOrDefault().Split('.').FirstOrDefault()}_merge";

                    var returnWindow = _mcgToolsVisualizationLibWindowService.ShowDialogConvertToPdfMergeWindowView(ListPdfItem, PdfFileName);
                    //ConvertToPdfMergeWindowView CurrentWindow = new ConvertToPdfMergeWindowView(ListPdfItem, PdfFileName);
                    //CurrentWindow.ShowDialog();
                    if (returnWindow.ResultDialog == MessageBoxResult.OK)
                    {
                        RaiseActionInProgressEvent();
                        //PdfFileName = CurrentWindow.CurrentDataContext.FileName.Split('.').FirstOrDefault();
                        PdfFileName = $"{CurrentDataContext.ExportFolder}\\{returnWindow.FileName}.pdf";
                        Thread ThreadSearchPart = new Thread(() => StartMergePdf(PdfFileName, ListPdfItem));
                        ThreadSearchPart.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckUncheckAll(bool IsChecked)
        {
            try
            {
                if (CurrentDataContext.ListConvertItem != null)
                    foreach (var item in CurrentDataContext.ListConvertItem)
                    {
                        item.IsSelected = IsChecked;
                    }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenHelp()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("VIS_UserGuideConvPdf"));
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        private void StartConvertPdf(bool IsTempFolder = false, bool ApplySecurity = true)
        {
            try
            {
                foreach (var item in CurrentDataContext.ListConvertItem.Where((file) => file.IsConvertToPdfSuccesfull && file.IsSelected))
                    item.Status = McgWpfTools.GetStringResource("VIS_ColStatusConverted");

                foreach (var item in CurrentDataContext.ListConvertItem.Where((file) => !file.IsConvertToPdfSuccesfull && file.IsSelected))
                {
                    StartOneConvertPdf(item, IsTempFolder, ApplySecurity);
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void StartOneConvertPdf(ConvertToPdfItem CurrentConvertItem, bool IsTempFolder = false, bool ApplySecurity = true)
        {
            try
            {
                string PdfFileName = null;
                if (CurrentConvertItem.OrigFileName != null)
                {
                    Regex TiffRegex = new Regex(@".+\.tiff$|.+\.tif$", RegexOptions.IgnoreCase);
                    Regex WordRegex = new Regex(@".+\.docx$|.+\.docm$|.+\.doc$", RegexOptions.IgnoreCase);
                    Regex ExcelRegex = new Regex(@".+\.xlsx$|.+\.xlsm$|.+\.xls$", RegexOptions.IgnoreCase);
                    Regex PowerPointRegex = new Regex(@".+\.pptx$|.+\.pptm$|.+\.ppt$", RegexOptions.IgnoreCase);
                    Regex PdfRegex = new Regex(@".+\.pdf$", RegexOptions.IgnoreCase);
                    Regex JpgRegex = new Regex(@".+\.jpg$|.+\.jpeg$|.+\.png$|.+\.gif$|.+\.bmp$", RegexOptions.IgnoreCase);
                    Regex TxtRegex = new Regex(@".+\.txt$|.+\.csv$|.+\.log$|.+\.xml$|.+\.xaml$", RegexOptions.IgnoreCase);

                    if (IsTempFolder)
                    {
                        Random rnd = new Random();
                        PdfFileName = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Merge_{rnd.Next(10000000)}.pdf";
                        while (File.Exists(PdfFileName))
                            PdfFileName = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Merge_{rnd.Next(10000000)}.pdf";
                    }
                    else
                    {
                        PdfFileName = CurrentConvertItem.OrigFileName.Split('.').FirstOrDefault().Split('\\').LastOrDefault();
                        PdfFileName = $"{CurrentDataContext.ExportFolder}\\{PdfFileName}.pdf";
                    }
                    CurrentConvertItem.ConvertedFileName = PdfFileName;

                    // Check if tiff doccument
                    if (TiffRegex.IsMatch(CurrentConvertItem.OrigFileName))
                        CurrentConvertItem.IsConvertToPdfSuccesfull = _pdfTools.ConvertTifToPdf(CurrentConvertItem.OrigFileName, PdfFileName);

                    // Check if Word doccument
                    else if (WordRegex.IsMatch(CurrentConvertItem.OrigFileName))
                        CurrentConvertItem.IsConvertToPdfSuccesfull = _pdfTools.ConvertWordToPdf(CurrentConvertItem.OrigFileName, PdfFileName);

                    // Check if Excel doccument
                    else if (ExcelRegex.IsMatch(CurrentConvertItem.OrigFileName))
                        CurrentConvertItem.IsConvertToPdfSuccesfull = _pdfTools.ConvertExcelToPdf(CurrentConvertItem.OrigFileName, PdfFileName);

                    // Check if PowerPoint doccument
                    else if (PowerPointRegex.IsMatch(CurrentConvertItem.OrigFileName))
                        CurrentConvertItem.IsConvertToPdfSuccesfull = _pdfTools.ConvertPowerPointToPdf(CurrentConvertItem.OrigFileName, PdfFileName);

                    // Check if image doccument
                    else if (JpgRegex.IsMatch(CurrentConvertItem.OrigFileName))
                        CurrentConvertItem.IsConvertToPdfSuccesfull = _pdfTools.ConvertImageToPdf(CurrentConvertItem.OrigFileName, PdfFileName);

                    // Check if text doccument
                    else if (TxtRegex.IsMatch(CurrentConvertItem.OrigFileName))
                        CurrentConvertItem.IsConvertToPdfSuccesfull = _pdfTools.ConvertTextToPdf(CurrentConvertItem.OrigFileName, PdfFileName);

                    if (CurrentConvertItem.IsConvertToPdfSuccesfull)
                        CurrentConvertItem.Status = McgWpfTools.GetStringResource("VIS_ColStatusConverted");
                    else if (PdfRegex.IsMatch(CurrentConvertItem.OrigFileName))
                    {
                        CurrentConvertItem.Status = McgWpfTools.GetStringResource("VIS_ColStatusAlreadyPdf");
                        CurrentConvertItem.IsConvertToPdfSuccesfull = true;
                        CurrentConvertItem.ConvertedFileName = CurrentConvertItem.OrigFileName;
                        File.Copy(CurrentConvertItem.ConvertedFileName, PdfFileName, true);
                    }
                    else
                        CurrentConvertItem.Status = McgWpfTools.GetStringResource("VIS_ColStatusIssueConverted");

                    // Watermark and Security 
                    if (ApplySecurity && CurrentConvertItem.IsConvertToPdfSuccesfull)
                    {
                        // Watermark
                        if (CurrentDataContext.IsOptionalWatermark)
                            AddWatermark(PdfFileName);

                        // Security
                        if (CurrentDataContext.ActivatePdfSecurity)
                        {
                            PdfToolsSecuritySetting CurrentPdfsecurity = new PdfToolsSecuritySetting()
                            {
                                UserPassword = CurrentDataContext.PdfUserPassword,
                                OwnerPassword = CurrentDataContext.PdfOwnerPassword,
                                PermitAccessibilityExtractContent = CurrentDataContext.PdfPermitExtractContent,
                                PermitAnnotations = CurrentDataContext.PdfPermitAnnotation,
                                PermitAssembleDocument = false,
                                PermitExtractContent = CurrentDataContext.PdfPermitExtractContent,
                                PermitFormsFill = false,
                                PermitFullQualityPrint = CurrentDataContext.PdfPermitPrint,
                                PermitModifyDocument = CurrentDataContext.PdfPermitModify,
                                PermitPrint = CurrentDataContext.PdfPermitPrint
                            };
                            _pdfTools.SetPdfSecurity(PdfFileName, CurrentPdfsecurity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void StartMergeTif(string TiffFileName, List<ConvertToPdfItem> ListTifItem)
        {
            try
            {
                List<string> AllFiles = new List<string>();
                foreach (var item in ListTifItem.OrderBy(item => item.Order))
                    if (item.DocumentType == DocumentTypeEnum.TIFF)
                    {
                        AllFiles.Add(item.OrigFileName);
                        item.Status = McgWpfTools.GetStringResource("VIS_ColStatusTifFile");
                    }
                    else
                        item.Status = McgWpfTools.GetStringResource("VIS_ColStatusExcluded");

                if (AllFiles.Count > 0)
                {
                    _tiffTools.MergeTiff(TiffFileName, AllFiles);
                    if (!File.Exists(TiffFileName))
                        MessageBox.Show(McgWpfTools.GetStringResource("VIS_MsgTiffNotCreated"), McgWpfTools.GetStringResource("VIS_MsgTitleTiffNotCreated"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    else if (AllFiles.Count != ListTifItem.Count)
                        MessageBox.Show(McgWpfTools.GetStringResource("VIS_MsgTiffNotAllMerged"), McgWpfTools.GetStringResource("VIS_MsgTitleTiffNotAllMerged"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    if (File.Exists(TiffFileName))
                        MainDispatcher.Invoke(() => OpenCreatedFile(TiffFileName));
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("VIS_MsgNoTiffSelected"), McgWpfTools.GetStringResource("VIS_MsgTitleNoTiffSelected"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void StartMergePdf(string PdfFileName, List<ConvertToPdfItem> ListPdfItem)
        {
            try
            {
                if (CurrentDataContext.ActivatePdfSecurity)
                    foreach (var item in ListPdfItem)
                        item.IsConvertToPdfSuccesfull = false;

                StartConvertPdf(true, false);
                List<string> ListPdf = new List<string>();



                foreach (var item in ListPdfItem.OrderBy(item => item.Order).Where(pdf => pdf.IsConvertToPdfSuccesfull))
                {
                    item.Status = McgWpfTools.GetStringResource("VIS_ColStatusConverted");
                    ListPdf.Add(item.ConvertedFileName);
                }

                if (ListPdf.Count > 0)
                {
                    _pdfTools.MergePDFs(PdfFileName, ListPdf);
                    if (!File.Exists(PdfFileName))
                        MessageBox.Show(McgWpfTools.GetStringResource("VIS_MsgPdfNotCreated"), McgWpfTools.GetStringResource("VIS_MsgTitlePdfNotCreated"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    else if (ListPdf.Count != ListPdfItem.Count)
                        MessageBox.Show(McgWpfTools.GetStringResource("VIS_MsgTiffNotAllMerged"), McgWpfTools.GetStringResource("VIS_MsgTitleTiffNotAllMerged"), MessageBoxButton.OK, MessageBoxImage.Warning);

                    if (File.Exists(PdfFileName))
                    {
                        // Watermark
                        if (CurrentDataContext.IsOptionalWatermark)
                            AddWatermark(PdfFileName);
                        // Security
                        if (CurrentDataContext.ActivatePdfSecurity)
                        {
                            PdfToolsSecuritySetting CurrentPdfsecurity = new PdfToolsSecuritySetting()
                            {
                                UserPassword = CurrentDataContext.PdfUserPassword,
                                OwnerPassword = CurrentDataContext.PdfOwnerPassword,
                                PermitAccessibilityExtractContent = CurrentDataContext.PdfPermitExtractContent,
                                PermitAnnotations = CurrentDataContext.PdfPermitAnnotation,
                                PermitAssembleDocument = false,
                                PermitExtractContent = CurrentDataContext.PdfPermitExtractContent,
                                PermitFormsFill = false,
                                PermitFullQualityPrint = CurrentDataContext.PdfPermitPrint,
                                PermitModifyDocument = CurrentDataContext.PdfPermitModify,
                                PermitPrint = CurrentDataContext.PdfPermitPrint
                            };
                            _pdfTools.SetPdfSecurity(PdfFileName, CurrentPdfsecurity);
                        }
                        CurrentDataContext_SecurityChangeEvent(null, null);
                        MainDispatcher.Invoke(() => OpenCreatedFile(PdfFileName));
                    }
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("VIS_MsgNoPdfConverted"), McgWpfTools.GetStringResource("VIS_MsgTitleNoPdfConverted"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void OpenCreatedFile(string FileName)
        {
            try
            {
                _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("VIS_MergedFile"), String.Format(McgWpfTools.GetStringResource("VIS_MsgMergedFile"), FileName), McgWpfTools.GetStringResource("VIS_ToolTipOpen"), McgWpfTools.GetStringResource("VIS_ToolTipOpenFolder"), McgWpfTools.GetStringResource("VIS_ToolTipClose"), FileName);
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void AddWatermark(string CompleteFileName)
        {
            try
            {
                PdfToolsWatermarkItem CurrentWatermak;

                CurrentWatermak = new PdfToolsWatermarkItem()
                {
                    IsWatermark = true,
                    MaxFontSize = VisualizationLibConstants.WatermarkMaxFontSize,
                    MinFontSize = VisualizationLibConstants.WatermarkMinFontSize,
                    TextAxialOffset = VisualizationLibConstants.WatermakTextAxialOffset,
                    TextRadialOffset = VisualizationLibConstants.WatermakTextRadialOffset,
                    TextFont = VisualizationLibConstants.WatermarkFontName,
                    WatermarkPosition = WatermarkPositionEnum.CENTER,
                    WatermarkText = CurrentDataContext.OptionalWatermark
                };
                CurrentWatermak.SetFontType(CommonLib.Models.Enums.FontStyle.Regular);

                _pdfTools.AddWatermarkToPdfDrawing(CompleteFileName, CurrentWatermak);
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
