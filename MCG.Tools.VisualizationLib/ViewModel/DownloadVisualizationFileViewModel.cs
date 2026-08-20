using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Models.Pdf;
using MCG.CommonLib.SapTools.Interfaces;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.WindchillCredential;
using MCG.Tools.VisualizationLib.Configuration;
using MCG.Tools.VisualizationLib.Exceptions;
using MCG.Tools.VisualizationLib.Interfaces;
using MCG.Tools.VisualizationLib.View;
using MCG.WindchillRequestTool;
using MCG.WindchillRequestTool.Model.BomComparison;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.Services.Interfaces;
using MCG.WindchillRequestTool.ViewModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.Tools.VisualizationLib.ViewModel
{
    public class DownloadVisualizationFileViewModel : ObservableObject, IDownloadVisualizationFileViewModel
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
        public DownloadVisualizationFileDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private NetworkCredential WindchillNetworkCredential { get; set; } = null;
        private List<VisualizationItem> ListItemInProgress { get; set; }
        public DownloadVisualizationFileConfiguration CurrentDownloadVisuConfiguration { get; set; }
        public DownloadVisualizationFileUserConfiguration CurrentDownloadVisuUserConfiguration { get; set; }
        Dispatcher MainDispatcher { get; set; } = null;
        private List<string> OfficialDrawingStates { get; set; }

        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly IWtDownloadViewableTools _wtDownloadViewableTools;
        private readonly IPdfTools _pdfTools;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly IWindchillNavigationService _windchillNavigationService;
        private readonly IWindchillRequestMiscService _windchillRequestMiscService;
        private readonly IWindchillPartManagementService _windchillPartManagementService;
        private readonly IWindchillChangeManagementService _windchillChangeManagementService;
        private readonly IWindchillDocumentManagementService _windchillDocumentManagementService;
        private readonly IWindchillBomManagementService _windchillBomManagementService;
        private readonly IWindchillVisualizationManagementService _windchillVisualizationManagementService;
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        private readonly ISapBomService _sapBomService;
        #endregion


        #region [REGION] Commands
        public ICommand CommandSearchEcn { get => new RelayCommand(() => ExecuteSearchEcn()); }
        public ICommand CommandSearchPart { get => new RelayCommand(() => ExecuteSearchPart()); }
        public ICommand CommandSearchFromPressEnterKey { get => new RelayCommand(() => ExecuteSearchFromPressEnterKey()); }
        public ICommand CommandDeleteSelectedParts { get => new RelayCommand(() => ExecuteDeleteSelectedParts()); }
        public ICommand CommandSearchVisuFile { get => new RelayCommand(() => ExecuteSearchVisuFile()); }
        public ICommand CommandExportZip { get => new RelayCommand(() => ExecuteExportZip()); }
        public ICommand CommandMenuItemPastePart { get => new RelayCommand(() => ExecuteMenuItemPastePart()); }
        public ICommand CommandMenuItemOpenPart { get => new RelayCommand(() => ExecuteMenuItemOpenPart()); }
        public ICommand CommandMenuItemOpenEcn { get => new RelayCommand(() => ExecuteMenuItemOpenEcn()); }
        public ICommand CommandMenuItemDeletePart { get => new RelayCommand(() => ExecuteMenuItemDeletePart()); }
        public ICommand CommandCheckUncheckAll { get => new RelayCommand<bool>((ischecked) => ExecuteCheckUncheckAll(ischecked)); }
        public ICommand CommandUncheckAll { get => new RelayCommand(() => ExecuteUncheckAll()); }
        public ICommand CommandUpdateCheckAllPart { get => new RelayCommand(() => ExecuteUpdateCheckAllPart()); }
        public ICommand CommandPaste { get => new RelayCommand<KeyEventArgs>((obj) => ExecuteCommandPaste(obj)); }
        public ICommand CommandApplyFilters { get => new RelayCommand(() => ApplyFiltersVisuFile()); }
        public ICommand CommandDownloadVisuFiles { get => new RelayCommand(() => ExecuteDownloadVisuFiles()); }
        public ICommand CommandUpdateColumn { get => new RelayCommand(() => ExecuteUpdateColumn()); }
        public ICommand CommandMenuItemSearchBom { get => new RelayCommand<string>((level) => ExecuteUMenuItemSearchBom(level)); }
        public ICommand CommandMenuItemSearchSapBom { get => new RelayCommand<string>((level) => ExecuteUMenuItemSearchSapBom(level)); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        public ICommand CommandChangeExportFolder { get => new RelayCommand(() => ExecuteChangeExportFolder()); }
        public ICommand CommandOpenFolder { get => new RelayCommand(() => McgFileAndSystemTools.OpenFolder(CurrentDataContext.ExportFolder)); }
        public ICommand CommandDownloadEcnFlash { get => new RelayCommand(() => ExecuteDownloadEcnFlash()); }
        public ICommand CommandDownloadPartFlash { get => new RelayCommand(() => ExecuteDownloadPartFlash()); }
        public ICommand CommandExportExcel { get => new RelayCommand(() => ExecuteExportExcel()); }
        #endregion

        #region [REGION] Init

        public DownloadVisualizationFileViewModel(DownloadVisualizationFileDataContext currentDC,
                                                  IWtDownloadViewableTools wtDownloadViewableTools,
                                                  IXmlSerializeTools xmlSerializeTools,
                                                  IPdfTools pdfTools,
                                                  IWindchillRequestMiscService windchillRequestMiscService,
                                                  IWindchillPartManagementService windchillPartManagementService,
                                                  IWindchillChangeManagementService windchillChangeManagementService,
                                                  IWindchillDocumentManagementService windchillDocumentManagementService,
                                                  IWindchillNavigationService windchillNavigationService,
                                                  IWindchillCredentialService windchillCredentialService,
                                                  IWindchillBomManagementService windchillBomManagementService,
                                                  IWindchillVisualizationManagementService windchillVisualizationManagementService,
                                                  IMcgCommonLibWindowService mcgCommonLibWindowService,
                                                  ISapBomService sapBomService)
        {
            try
            {
                CurrentDataContext = currentDC;
                _wtDownloadViewableTools = wtDownloadViewableTools;
                _windchillCredentialService = windchillCredentialService;
                _windchillRequestMiscService = windchillRequestMiscService;
                _windchillPartManagementService = windchillPartManagementService;
                _windchillChangeManagementService = windchillChangeManagementService;
                _windchillDocumentManagementService = windchillDocumentManagementService;
                _windchillNavigationService = windchillNavigationService;
                _windchillBomManagementService = windchillBomManagementService;
                _windchillVisualizationManagementService = windchillVisualizationManagementService;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;
                _xmlSerializeTools = xmlSerializeTools;
                _pdfTools = pdfTools;
                _sapBomService = sapBomService;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                MainDispatcher = Dispatcher.CurrentDispatcher;
                CurrentDownloadVisuConfiguration = _xmlSerializeTools.GetDeserializedXml<DownloadVisualizationFileConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{VisualizationLibConstants.ConfigurationFile}");
                CurrentDownloadVisuUserConfiguration = _xmlSerializeTools.GetDeserializedXmlFromAppData<DownloadVisualizationFileUserConfiguration>(VisualizationLibConstants.UserConfigurationFile);
                CurrentDataContext.ExportFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (CurrentDownloadVisuUserConfiguration != null)
                {
                    CurrentDataContext.IsColAddedFromShown = CurrentDownloadVisuUserConfiguration.IsColAddedFromShown;
                    CurrentDataContext.IsColDescriptionEngShown = CurrentDownloadVisuUserConfiguration.IsColDescriptionEngShown;
                    CurrentDataContext.IsColDescriptionLocalShown = CurrentDownloadVisuUserConfiguration.IsColDescriptionLocalShown;
                    CurrentDataContext.IsColPdmContextShown = CurrentDownloadVisuUserConfiguration.IsColPdmContextShown;

                    CurrentDataContext.IsPdfTiffSelected = CurrentDownloadVisuUserConfiguration.IsPdfTiffSelected;
                    CurrentDataContext.IsOfficeDocSelected = CurrentDownloadVisuUserConfiguration.IsOfficeDocSelected;
                    CurrentDataContext.IsPvzSelected = CurrentDownloadVisuUserConfiguration.IsPvzSelected;
                    CurrentDataContext.IsDxfSelected = CurrentDownloadVisuUserConfiguration.IsDxfSelected;
                    CurrentDataContext.IsStepSelected = CurrentDownloadVisuUserConfiguration.IsStepSelected;
                    CurrentDataContext.IsIgesSelected = CurrentDownloadVisuUserConfiguration.IsIgesSelected;
                    CurrentDataContext.IsOtherSelected = CurrentDownloadVisuUserConfiguration.IsOtherSelected;
                    CurrentDataContext.IsCreateZip = CurrentDownloadVisuUserConfiguration.IsCreateZip;
                    CurrentDataContext.IsAdminActivated = CurrentDownloadVisuUserConfiguration.IsAdmin;

                    if (CurrentDownloadVisuUserConfiguration.ExportFolder != null && CurrentDownloadVisuUserConfiguration.ExportFolder.Trim() != "" && Directory.Exists(CurrentDownloadVisuUserConfiguration.ExportFolder))
                        CurrentDataContext.ExportFolder = CurrentDownloadVisuUserConfiguration.ExportFolder;

                    CurrentDataContext.UserConfigurationUpdateEvent += UpdateUserConfigXmlFile;
                }
                else
                {
                    CurrentDownloadVisuUserConfiguration = new DownloadVisualizationFileUserConfiguration()
                    {
                        IsColAddedFromShown = CurrentDataContext.IsColAddedFromShown,
                        IsColDescriptionEngShown = CurrentDataContext.IsColDescriptionEngShown,
                        IsColDescriptionLocalShown = CurrentDataContext.IsColDescriptionLocalShown,
                        IsColPdmContextShown = CurrentDataContext.IsColPdmContextShown,
                        IsDxfSelected = CurrentDataContext.IsDxfSelected,
                        IsIgesSelected = CurrentDataContext.IsIgesSelected,
                        IsOfficeDocSelected = CurrentDataContext.IsOfficeDocSelected,
                        IsOtherSelected = CurrentDataContext.IsOtherSelected,
                        IsPdfTiffSelected = CurrentDataContext.IsPdfTiffSelected,
                        IsPvzSelected = CurrentDataContext.IsPvzSelected,
                        IsStepSelected = CurrentDataContext.IsStepSelected,
                        IsCreateZip = CurrentDataContext.IsCreateZip,
                        ExportFolder = CurrentDataContext.ExportFolder
                    };
                    UpdateUserConfigXmlFile();
                }

                // Update Optional Watermark Values
                if (CurrentDownloadVisuConfiguration.OptionalWatermarkValues != null && CurrentDataContext.OptionalWatermarkValues != null)
                {
                    foreach (var value in CurrentDownloadVisuConfiguration.OptionalWatermarkValues)
                        CurrentDataContext.OptionalWatermarkValues.Add(value);
                    CurrentDataContext.OptionalWatermark = CurrentDataContext.OptionalWatermarkValues.FirstOrDefault();

                    foreach (var item in CurrentDownloadVisuConfiguration.ListSapPlant)
                        CurrentDataContext.AllSapPlants.Add(item);
                    CurrentDataContext.Plant = CurrentDataContext.AllSapPlants.FirstOrDefault();
                }

                var listBomUsage = McgBusinessTools.GetLIstSapBomUsage();
                if (listBomUsage != null && listBomUsage.Count > 0)
                    foreach (var usage in listBomUsage)
                        CurrentDataContext.AllBomUsage.Add(usage);
                CurrentDataContext.BomUsage = CurrentDataContext.AllBomUsage.FirstOrDefault(item => item.Usage == "3");

                ActionInProgressEvent += (sender, e) => CurrentDataContext.ActionInProgress = true;
                ActionDoneEvent += (sender, e) => CurrentDataContext.ActionInProgress = false;


                OfficialDrawingStates = VisualizationLibConstants.OfficialDrawingStates.Split('|').ToList();

            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex); ;
            }
        }

        public void InitApp()
        {
        }

        private void UpdateUserConfigXmlFile(object sender = null, EventArgs e = null)
        {
            try
            {
                if (CurrentDownloadVisuUserConfiguration != null)
                {
                    CurrentDownloadVisuUserConfiguration.IsColAddedFromShown = CurrentDataContext.IsColAddedFromShown;
                    CurrentDownloadVisuUserConfiguration.IsColDescriptionEngShown = CurrentDataContext.IsColDescriptionEngShown;
                    CurrentDownloadVisuUserConfiguration.IsColDescriptionLocalShown = CurrentDataContext.IsColDescriptionLocalShown;
                    CurrentDownloadVisuUserConfiguration.IsColPdmContextShown = CurrentDataContext.IsColPdmContextShown;
                    CurrentDownloadVisuUserConfiguration.IsDxfSelected = CurrentDataContext.IsDxfSelected;
                    CurrentDownloadVisuUserConfiguration.IsIgesSelected = CurrentDataContext.IsIgesSelected;
                    CurrentDownloadVisuUserConfiguration.IsOfficeDocSelected = CurrentDataContext.IsOfficeDocSelected;
                    CurrentDownloadVisuUserConfiguration.IsOtherSelected = CurrentDataContext.IsOtherSelected;
                    CurrentDownloadVisuUserConfiguration.IsPdfTiffSelected = CurrentDataContext.IsPdfTiffSelected;
                    CurrentDownloadVisuUserConfiguration.IsPvzSelected = CurrentDataContext.IsPvzSelected;
                    CurrentDownloadVisuUserConfiguration.IsStepSelected = CurrentDataContext.IsStepSelected;
                    CurrentDownloadVisuUserConfiguration.ExportFolder = CurrentDataContext.ExportFolder;
                    CurrentDownloadVisuUserConfiguration.IsCreateZip = CurrentDataContext.IsCreateZip;
                    CurrentDownloadVisuUserConfiguration.IsAdmin = CurrentDataContext.IsAdminActivated;
                    _xmlSerializeTools.SerializedXmlInAppData<DownloadVisualizationFileUserConfiguration>(CurrentDownloadVisuUserConfiguration, VisualizationLibConstants.UserConfigurationFile);
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteSearchEcn()
        {
            try
            {
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                Thread ThreadSearchPart = new Thread(() => SearchEcnAsynch());
                ThreadSearchPart.Start();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSearchPart()
        {
            try
            {
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                Thread ThreadSearchPart = new Thread(() => SearchPartAsynch());
                ThreadSearchPart.Start();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
                RaiseActionDoneEvent();
            }
        }

        private void ExecuteSearchFromPressEnterKey()
        {
            try
            {
                if (CurrentDataContext.FilterNumber != null && CurrentDataContext.FilterNumber.Trim() != "")
                {
                    Regex CheckEcn = new Regex(VisualizationLibConstants.EcnCopyPasteRegEx, RegexOptions.IgnoreCase);
                    if (CheckEcn.IsMatch(CurrentDataContext.FilterNumber))

                        ExecuteSearchEcn();
                    else
                        ExecuteSearchPart();
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
                RaiseActionDoneEvent();
            }
        }

        private void ExecuteDeleteSelectedParts()
        {
            try
            {
                if (CurrentDataContext.SearchedPartList != null)
                {
                    var tempList = CurrentDataContext.SearchedPartList.Where((part) => part.IsSelected).ToList();
                    foreach (var item in tempList)
                        CurrentDataContext.SearchedPartList.Remove(item);
                    CurrentDataContext.IsAllPartSelected = false;
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteSearchVisuFile()
        {
            try
            {
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                Thread ThreadSearchPart = new Thread(() => SearchVisualizationFileAsynch());
                ThreadSearchPart.Start();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteExportZip()
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenuItemPastePart()
        {
            try
            {
                GetVisuItemFromClipboard();
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                Thread ThreadSearchPart = new Thread(() => SearchListPartEcnAsynch());
                ThreadSearchPart.Start();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenuItemOpenPart()
        {
            try
            {
                if (CurrentDataContext.SelectedPart != null && CurrentDataContext.SelectedPart.WindchillPart != null)
                    _windchillNavigationService.OpenWtPartDetailPage(CurrentDataContext.SelectedPart.WindchillPart.Id, null, CommonLibConstants.WindchillUrl);
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenuItemOpenEcn()
        {
            try
            {
                if (CurrentDataContext.SelectedPart != null && CurrentDataContext.SelectedPart.WindchillEcn != null)
                    _windchillNavigationService.OpenEcnDetailPage(CurrentDataContext.SelectedPart.WindchillEcn.GetWindchillId(), null, CommonLibConstants.WindchillUrl);

            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMenuItemDeletePart()
        {
            try
            {
                if (CurrentDataContext.SelectedPart != null)
                {
                    CurrentDataContext.SearchedPartList.Remove(CurrentDataContext.SelectedPart);
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
                if (CurrentDataContext.SearchedPartList != null)
                    foreach (var item in CurrentDataContext.SearchedPartList)
                    {
                        item.IsSelected = IsChecked;
                        item.IsAllSelected = IsChecked;
                    }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateCheckAllPart()
        {
            try
            {
                bool TempCheck = true;
                if (CurrentDataContext.SearchedPartList != null)
                {
                    foreach (var item in CurrentDataContext.SearchedPartList)
                    {
                        if (!item.IsSelected)
                            TempCheck = false;
                    }
                    CurrentDataContext.IsAllPartSelected = TempCheck;
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUncheckAll()
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCommandPaste(KeyEventArgs e = null)
        {
            try
            {
                if (e == null || (Keyboard.Modifiers == ModifierKeys.Control && e != null && e.Key == Key.V))
                {
                    ExecuteMenuItemPastePart();
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDownloadVisuFiles()
        {
            try
            {
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                Thread ThreadSearchPart = new Thread(() => DownloadSelectedVisuFileAsynch(CurrentDataContext.IsDefaultWatermark));
                ThreadSearchPart.Start();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateColumn()
        {
            try
            {

            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUMenuItemSearchBom(string Level)
        {
            try
            {
                int BomLevel = 1;
                if (Level != null)
                    BomLevel = Int32.Parse(Level);

                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                Thread ThreadSearchPart = new Thread(() => SearchBomComponentAsynch(BomLevel));
                ThreadSearchPart.Start();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUMenuItemSearchSapBom(string Level)
        {
            try
            {
                int BomLevel = 1;
                if (Level != null)
                    BomLevel = Int32.Parse(Level);

                RaiseActionInProgressEvent();
                Thread ThreadSearchPart = new Thread(() => SearchSapBomComponentAsynch(BomLevel));
                ThreadSearchPart.Start();
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
                //McgMiscTools.OpenFile($"{MainAppFolder}\\{McgMiscTools.GetAppSetting(this, "DocumentationFolder")}\\{McgWpfTools.GetStringResource("VIS_UserGuide")}");
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("VIS_UserGuide"));
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
                    UpdateUserConfigXmlFile();
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteSearchVisuFileNotAsynch()
        {
            try
            {
                CheckWindchillCredential();
                SearchVisualizationFileAsynch();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteDownloadVisuFilesNotAsynch()
        {
            try
            {
                CheckWindchillCredential();
                DownloadSelectedVisuFileAsynch(CurrentDataContext.IsDefaultWatermark);
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteDownloadEcnFlash()
        {
            try
            {
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                Thread ThreadSearchPart = new Thread(() => DownloadEcnFlashAsynch());
                ThreadSearchPart.Start();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteDownloadPartFlash()
        {
            try
            {
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                Thread ThreadSearchPart = new Thread(() => DownloadPartFlashAsynch());
                ThreadSearchPart.Start();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void ExecuteExportExcel()
        {
            try
            {
                Regex RegexProc = new Regex("Excel", RegexOptions.IgnoreCase);
                List<Process> OldExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                string XlsFileName = $"{CurrentDataContext.ExportFolder}\\Vizu_Tool_Export.xlsx";

                ExcelToolsClosedXml CurrentExcel = new ExcelToolsClosedXml() { CompleteFileName = XlsFileName };
                if (CurrentExcel.CreateNewFile("PARTS") != ExcelStatus.OK)
                {
                    MessageBox.Show(String.Format(McgWpfTools.GetStringResource("VIS_MsgIssueExporExcel"), XlsFileName), McgWpfTools.GetStringResource("VIS_IssueExporExcel"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }

                List<Process> NewExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                Process newExcelProcess = NewExcelCurrentProcess.FirstOrDefault((proc) => !OldExcelCurrentProcess.ToList().Exists((oldprc) => proc.Id == oldprc.Id));

                CurrentExcel.CurrentSheet = "PARTS";
                CurrentExcel.SetCellValue("Number", 1, 1);
                CurrentExcel.SetCellValue("Revision", 1, 2);
                CurrentExcel.SetCellValue("Main Drawing", 1, 3);
                CurrentExcel.SetCellValue("TDFC/GEI", 1, 4);
                CurrentExcel.SetCellValue("Comment", 1, 5);
                CurrentExcel.SetCellValue("Added From", 1, 6);

                CurrentExcel.CreateSheet("ALL DOCUMENTS");
                CurrentExcel.CurrentSheet = "ALL DOCUMENTS";
                CurrentExcel.SetCellValue("Part Number", 1, 1);
                CurrentExcel.SetCellValue("Part Revision", 1, 2);
                CurrentExcel.SetCellValue("Document Number", 1, 3);
                CurrentExcel.SetCellValue("Document Revision", 1, 4);
                CurrentExcel.SetCellValue("Comment", 1, 5);

                int index = 2;
                int index2 = 2;
                foreach (var item in CurrentDataContext.SearchedPartList)
                {
                    CurrentExcel.CurrentSheet = "PARTS";
                    CurrentExcel.SetCellValue(item.PartNumber, index, 1);
                    CurrentExcel.SetCellValue(item.PartRevision, index, 2);
                    var doc = item.SearchedCompleteDocumentList.FirstOrDefault(docTemp => docTemp.IsMainDrawing && docTemp.Comment == McgWpfTools.GetStringResource("VIS_DocCommentMain"));
                    if (doc != null)
                        CurrentExcel.SetCellValue(doc.DocumentNumber, index, 3);
                    doc = item.SearchedCompleteDocumentList.FirstOrDefault(docTemp => docTemp.IsMainDrawing && docTemp.Comment == McgWpfTools.GetStringResource("VIS_DocCommentRef"));
                    if (doc != null)
                        CurrentExcel.SetCellValue(doc.DocumentNumber, index, 4);
                    CurrentExcel.SetCellValue(item.Comment, index, 5);
                    CurrentExcel.SetCellValue(item.AddedFrom, index, 6);


                    if (item.SearchedDocumentList != null)
                    {
                        CurrentExcel.CurrentSheet = "ALL DOCUMENTS";
                        foreach (var doc2 in item.SearchedCompleteDocumentList)
                        {
                            CurrentExcel.SetCellValue(item.PartNumber, index2, 1);
                            CurrentExcel.SetCellValue(item.PartRevision, index2, 2);
                            CurrentExcel.SetCellValue(doc2.DocumentNumber, index2, 3);
                            CurrentExcel.SetCellValue(doc2.DocumentRevision, index2, 4);
                            CurrentExcel.SetCellValue(doc2.Comment, index2, 5);
                            index2++;
                        }
                    }
                    index++;
                }

                if (CurrentExcel.SaveClose() != ExcelStatus.OK)
                {
                    MessageBox.Show(String.Format(String.Format(McgWpfTools.GetStringResource("VIS_MsgIssueExporExcel"), XlsFileName), McgWpfTools.GetStringResource("VIS_IssueExporExcel"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
                    return;
                }

                if (newExcelProcess != null)
                    newExcelProcess.Kill();

                _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("VIS_ExporExcelDone"), String.Format(McgWpfTools.GetStringResource("VIS_ExporExcelDone2"), XlsFileName), McgWpfTools.GetStringResource("VIS_ExporExcelOpen"), McgWpfTools.GetStringResource("VIS_ExporExcelOpenFolder"), McgWpfTools.GetStringResource("VIS_ExporExcelClose"), XlsFileName);
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Windchill Methods
        private bool CheckWindchillCredential()
        {
            try
            {
                if (WindchillNetworkCredential == null)
                {
                    WindchillCredentialItem WindchillCrendential = _windchillCredentialService.GetWindchillCredential($"{CommonLibConstants.WindchillUrl}/", $"{CommonLibConstants.WindchillUrl}/");
                    if (!WindchillCrendential.IsCredentialOk) return false;
                    WindchillNetworkCredential = WindchillCrendential.WindchillCredential;
                    //WindchillNetworkCredential.UserName = WindchillCrendential.Login;
                    //WindchillNetworkCredential.Password = WindchillCrendential.PassWord;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }

        private void SearchEcnAsynch()
        {
            try
            {
                CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("VIS_StatusBarSearchPartInProgress");
                ListItemInProgress = new List<VisualizationItem>();
                VisualizationItem CurrentItem;
                WindchillObjectWtPart CurrentWindchillPart;
                if (CurrentDataContext.FilterNumber != null && CurrentDataContext.FilterNumber != "" && CurrentDataContext.FilterNumber != "*")
                {
                    TraceLog.AddTraceLog($"Start Search ECN {CurrentDataContext.FilterNumber} from Search Action");
                    //RestOdataChangeNoticeUpper CurrentEcnUpper = WindchillRestOdataTool.GetChangeNoticeWithWtPart(WindchillNetworkCredential, CurrentDataContext.FilterNumber, false, McgMiscTools.GetAppSetting(this, "WindchillUrl"));
                    RestOdataChangeNoticeUpper CurrentEcnUpper = _windchillChangeManagementService.GetChangeNoticeFullAllPartsRevision(WindchillNetworkCredential, CurrentDataContext.FilterNumber, true, false, false, CommonLibConstants.WindchillUrl);
                    if (CurrentEcnUpper != null && CurrentEcnUpper.value != null && CurrentEcnUpper.value.Count > 0)
                    {
                        RestOdataChangeNotice CurrentEcn = CurrentEcnUpper.value.FirstOrDefault();
                        CurrentItem = new VisualizationItem()
                        {
                            PartNumber = CurrentEcn.Number,
                            PartRevision = "",
                            State = CurrentEcn.State.Display,
                            ItemType = DocumentTypeEnum.ECN,
                            IsEcnInformationSearched = true,
                            EcnNumber = CurrentEcn.Number,
                            WindchillEcn = CurrentEcn,
                            DescriptionEng = CurrentEcn.Name,
                            DescriptionLocal = CurrentEcn.Name,
                            AddedFrom = McgWpfTools.GetStringResource("VIS_FromSearch")
                        };
                        if (CurrentEcn.Location != null && CurrentEcn.Location.Split('/').Count() > 1)
                            CurrentItem.PdmContext = CurrentEcn.Location.Split('/').ElementAt(1);

                        ListItemInProgress.Add(CurrentItem);
                        if (CurrentEcn.ListWtPart != null)
                        {
                            foreach (var item in CurrentEcn.ListWtPart)
                            {
                                CurrentWindchillPart = _windchillRequestMiscService.GetWindchillPart(item);
                                CurrentItem = new VisualizationItem()
                                {
                                    PartNumber = item.Number,
                                    PartRevision = item.Revision,
                                    State = item.State.Display,
                                    ItemType = DocumentTypeEnum.PART,
                                    ItemFrom = DocumentTypeEnum.FROMECN,
                                    IsEcnInformationSearched = true,
                                    EcnNumber = CurrentEcn.Number,
                                    WindchillPart = CurrentWindchillPart,
                                    DescriptionEng = $"{CurrentWindchillPart.Name}|{CurrentWindchillPart.DescriptionEn2}",
                                    DescriptionLocal = $"{CurrentWindchillPart.DescriptionLocal1}|{CurrentWindchillPart.DescriptionLocal2}",
                                    PdmContext = CurrentWindchillPart.Context.Name,
                                    AddedFrom = string.Format(McgWpfTools.GetStringResource("VIS_FromEcn"), new string[1] { $"{CurrentEcn.Number} ({item.Eca.Number})" })
                                };
                                if (item.Revisions != null)
                                {
                                    CurrentItem.AllOdataWtPartRevision = item.Revisions;
                                    foreach (var partRev in item.Revisions.OrderBy((part) => part.Revision))
                                    {
                                        CurrentItem.AllPartRevision.Add(partRev.Revision);
                                        CurrentItem.AllPartRevisionState.Add(new VisualizationItemRevisionState()
                                        {
                                            Revision = partRev.Revision,
                                            State = partRev.State?.Display
                                        });
                                    }
                                }

                                ListItemInProgress.Add(CurrentItem);
                            }
                        }
                        MainDispatcher.Invoke(new Action(UpdateListVisualizationItem));
                        TraceLog.AddTraceLog($"End Search ECN {CurrentDataContext.FilterNumber}");
                    }
                    else
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("VIS_ErrorMsgEcnNotFound"), McgWpfTools.GetStringResource("VIS_TitleWindowSearch"), MessageBoxButton.OK, MessageBoxImage.Error);
                        TraceLog.AddTraceLog($"End Search ECN {CurrentDataContext.FilterNumber}: Not found");
                    }

                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.StatusBarTextRight = "";
            }
        }

        private void SearchPartAsynch()
        {
            try
            {
                CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("VIS_StatusBarSearchPartInProgress");
                ListItemInProgress = new List<VisualizationItem>();
                VisualizationItem CurrentItem;
                WindchillObjectWtPart CurrentWindchillPart;
                if (CurrentDataContext.FilterNumber != null && CurrentDataContext.FilterNumber != "" && CurrentDataContext.FilterNumber != "*")
                {
                    Dictionary<string, string> ListPart = new Dictionary<string, string>();
                    ListPart.Add(CurrentDataContext.FilterNumber, "Latest");
                    List<RestOdataWtPart> ListPartResult = _windchillPartManagementService.GetListPartAllRevisions(WindchillNetworkCredential, ListPart, CommonLibConstants.WindchillUrl);

                    if (ListPartResult != null && ListPartResult.Count > 0)
                    {
                        foreach (var item in ListPartResult)
                        {
                            TraceLog.AddTraceLog($"Start Search PART {item.Number} ");

                            CurrentWindchillPart = _windchillRequestMiscService.GetWindchillPart(item);
                            CurrentItem = new VisualizationItem()
                            {
                                PartNumber = item.Number,
                                PartRevision = item.Revision,
                                State = item.State.Display,
                                ItemType = DocumentTypeEnum.PART,
                                WindchillPart = CurrentWindchillPart,
                                DescriptionEng = $"{CurrentWindchillPart.Name}|{CurrentWindchillPart.DescriptionEn2}",
                                DescriptionLocal = $"{CurrentWindchillPart.DescriptionLocal1}|{CurrentWindchillPart.DescriptionLocal2}",
                                PdmContext = CurrentWindchillPart.Context.Name,
                                AddedFrom = McgWpfTools.GetStringResource("VIS_FromSearch")

                            };

                            if (item.Revisions != null)
                            {
                                CurrentItem.AllOdataWtPartRevision = item.Revisions;
                                foreach (var partRev in item.Revisions.OrderBy((part) => part.Revision))
                                {
                                    CurrentItem.AllPartRevision.Add(partRev.Revision);
                                    CurrentItem.AllPartRevisionState.Add(new VisualizationItemRevisionState()
                                    {
                                        Revision = partRev.Revision,
                                        State = partRev.State?.Display
                                    });
                                }
                            }

                            ListItemInProgress.Add(CurrentItem);
                            TraceLog.AddTraceLog($"End Search PART {item.Number}");
                        }
                        MainDispatcher.Invoke(new Action(UpdateListVisualizationItem));
                    }
                    else
                        MessageBox.Show(McgWpfTools.GetStringResource("VIS_ErrorMsgPartNotFound"), McgWpfTools.GetStringResource("VIS_TitleWindowSearch"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("VIS_ErrorMsgPartNotFound"), McgWpfTools.GetStringResource("VIS_TitleWindowSearch"), MessageBoxButton.OK, MessageBoxImage.Error);

            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.StatusBarTextRight = "";
            }
        }

        private void SearchListPartEcnAsynch(bool UpdateAddFrom = true)
        {
            try
            {
                CurrentDataContext.IsSearchInProgress = true;
                CurrentDataContext.CurrentStep = 0;
                CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("VIS_StatusBarSearchPartInProgress");
                WindchillObjectWtPart CurrentWindchillPart;
                if (ListItemInProgress != null && ListItemInProgress.Count > 0)
                {
                    var ListPartItem = ListItemInProgress.Where((item) => item.ItemType == DocumentTypeEnum.PART).ToList();
                    var ListEcnItem = ListItemInProgress.Where((item) => item.ItemType == DocumentTypeEnum.ECN).ToList();
                    CurrentDataContext.TotalStep = ListPartItem.Count + ListEcnItem.Count;

                    foreach (var itemPart in ListPartItem)
                    {
                        Dictionary<string, string> ListPart = new Dictionary<string, string>();
                        if (itemPart.PartRevision != null && itemPart.PartRevision.Trim() != "")
                            ListPart.Add(itemPart.PartNumber, itemPart.PartRevision);
                        else
                            ListPart.Add(itemPart.PartNumber, "Latest");

                        TraceLog.AddTraceLog($"Start Search PART {itemPart.PartNumber}.{itemPart.PartRevision} from Search Copy/paste");
                        //if (CurrentDataContext.SearchedPartList.FirstOrDefault((item) => item.PartNumber == itemPart.PartNumber && item.PartRevision == item.PartRevision) != null)
                        //    CurrentDataContext.SearchedPartList.Remove(itemPart);
                        //else
                        //{

                        List<RestOdataWtPart> ListPartResult = _windchillPartManagementService.GetListPartAllRevisions(WindchillNetworkCredential, ListPart, CommonLibConstants.WindchillUrl);

                        var sorted = ListPartResult.OrderBy(d => d.Number).ThenBy(d => d.Revision, new McgRevisionComparer()).ToList();
                        ListPartResult = sorted;

                        //List<RestOdataWtPart> ListPartResult = WindchillRestOdataTool.GetListPart(WindchillNetworkCredential, ListPart, McgMiscTools.GetAppSetting(this, "WindchillUrl"));
                        if (ListPartResult != null)
                        {
                            RestOdataWtPart TempPart = ListPartResult.FirstOrDefault();
                            if (TempPart != null)
                            {
                                CurrentWindchillPart = _windchillRequestMiscService.GetWindchillPart(TempPart);
                                itemPart.PartRevision = TempPart.Revision;
                                itemPart.State = TempPart.State.Display;
                                itemPart.WindchillPart = CurrentWindchillPart;
                                itemPart.DescriptionEng = $"{CurrentWindchillPart.Name}|{CurrentWindchillPart.DescriptionEn2}";
                                itemPart.DescriptionLocal = $"{CurrentWindchillPart.DescriptionLocal1}|{CurrentWindchillPart.DescriptionLocal2}";
                                itemPart.PdmContext = CurrentWindchillPart.Context.Name;
                                if (UpdateAddFrom)
                                    itemPart.AddedFrom = McgWpfTools.GetStringResource("VIS_FromCopyPaste");
                                if (TempPart.Revisions != null)
                                {
                                    itemPart.AllOdataWtPartRevision = TempPart.Revisions;
                                    foreach (var partRev in TempPart.Revisions.OrderBy((part) => part.Revision))
                                    {
                                        itemPart.AllPartRevision.Add(partRev.Revision);
                                        itemPart.AllPartRevisionState.Add(new VisualizationItemRevisionState()
                                        {
                                            Revision = partRev.Revision,
                                            State = partRev.State?.Display
                                        });
                                    }
                                }
                            }
                            else
                            {
                                itemPart.Comment = McgWpfTools.GetStringResource("VIS_Status04");
                                itemPart.DetailComment = McgWpfTools.GetStringResource("VIS_Status04");
                                itemPart.ItemType = DocumentTypeEnum.UNKNOWN;
                            }
                        }
                        else
                        {
                            itemPart.Comment = McgWpfTools.GetStringResource("VIS_Status04");
                            itemPart.DetailComment = McgWpfTools.GetStringResource("VIS_Status04");
                            itemPart.ItemType = DocumentTypeEnum.UNKNOWN;
                        }
                        //}
                        CurrentDataContext.CurrentStep++;
                        TraceLog.AddTraceLog($"End Search PART {itemPart.PartNumber}");

                    }

                    foreach (var itemEcn in ListEcnItem)
                    {
                        TraceLog.AddTraceLog($"Start Search ECN {itemEcn.PartNumber} from Search Copy/paste");

                        VisualizationItem CurrentItem;
                        //RestOdataChangeNoticeUpper CurrentEcnUpper = WindchillRestOdataTool.GetChangeNoticeWithWtPart(WindchillNetworkCredential, itemEcn.PartNumber, false, McgMiscTools.GetAppSetting(this, "WindchillUrl"));
                        RestOdataChangeNoticeUpper CurrentEcnUpper = _windchillChangeManagementService.GetChangeNoticeFull(WindchillNetworkCredential, itemEcn.PartNumber, true, false, false, CommonLibConstants.WindchillUrl);
                        if (CurrentEcnUpper != null && CurrentEcnUpper.value != null && CurrentEcnUpper.value.Count > 0)
                        {
                            RestOdataChangeNotice CurrentEcn = CurrentEcnUpper.value.FirstOrDefault();
                            itemEcn.WindchillEcn = CurrentEcn;
                            itemEcn.State = CurrentEcn.State.Display;
                            itemEcn.IsEcnInformationSearched = true;
                            itemEcn.EcnNumber = CurrentEcn.Number;
                            itemEcn.DescriptionEng = CurrentEcn.Name;
                            itemEcn.DescriptionLocal = CurrentEcn.Name;
                            if (UpdateAddFrom)
                                itemEcn.AddedFrom = McgWpfTools.GetStringResource("VIS_FromCopyPaste");
                            if (CurrentEcn.Location != null && CurrentEcn.Location.Split('/').Count() > 1)
                                itemEcn.PdmContext = CurrentEcn.Location.Split('/').ElementAt(1);
                            if (CurrentEcn.ListWtPart != null)
                            {
                                foreach (var item in CurrentEcn.ListWtPart)
                                {
                                    CurrentWindchillPart = _windchillRequestMiscService.GetWindchillPart(item);
                                    CurrentItem = new VisualizationItem()
                                    {
                                        PartNumber = item.Number,
                                        PartRevision = item.Revision,
                                        State = item.State.Display,
                                        ItemType = DocumentTypeEnum.PART,
                                        ItemFrom = DocumentTypeEnum.FROMECN,
                                        WindchillPart = CurrentWindchillPart,
                                        IsEcnInformationSearched = true,
                                        EcnNumber = CurrentEcn.Number,
                                        DescriptionEng = $"{CurrentWindchillPart.Name}|{CurrentWindchillPart.DescriptionEn2}",
                                        DescriptionLocal = $"{CurrentWindchillPart.DescriptionLocal1}|{CurrentWindchillPart.DescriptionLocal2}",
                                        PdmContext = CurrentWindchillPart.Context.Name
                                    };
                                    if (UpdateAddFrom)
                                        CurrentItem.AddedFrom = string.Format(McgWpfTools.GetStringResource("VIS_FromEcn"), new string[1] { CurrentEcn.Number });
                                    ListItemInProgress.Add(CurrentItem);
                                }
                            }
                        }
                        else
                        {
                            itemEcn.Comment = McgWpfTools.GetStringResource("VIS_Status09");
                            itemEcn.DetailComment = McgWpfTools.GetStringResource("VIS_Status09");
                            itemEcn.ItemType = DocumentTypeEnum.UNKNOWN;
                        }

                        CurrentDataContext.CurrentStep++;
                        TraceLog.AddTraceLog($"End Search ECN {itemEcn.PartNumber}");

                    }

                    MainDispatcher.Invoke(new Action(UpdateListVisualizationItem));
                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.StatusBarTextRight = "";
                CurrentDataContext.IsSearchInProgress = false;
            }
        }

        private void SearchBomComponentAsynch(int BomLevel)
        {
            try
            {
                TraceLog.AddTraceLog($"Start Search BOM for {CurrentDataContext.SelectedPart.PartNumber}.{CurrentDataContext.SelectedPart.PartRevision}, level {BomLevel}");

                CurrentDataContext.IsSearchInProgress = true;
                CurrentDataContext.CurrentStep = 0;
                CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("VIS_StatusBarBomInProgress");
                WindchillNamingConvention NamingConvention = null;
                NamingConvention = _xmlSerializeTools.GetDeserializedXml<WindchillNamingConvention>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{CommonLibConstants.NamingConventionFile}");

                WindchillObjStructureComponent FinalStructure = _windchillBomManagementService.SearchBomLevelByLevel(
                                                                    CurrentDataContext.SelectedPart.PartNumber,
                                                                    CurrentDataContext.SelectedPart.PartRevision,
                                                                    WindchillObjectType.PART,
                                                                    BomLevel,
                                                                    WindchillNetworkCredential,
                                                                    NamingConvention,
                                                                    false,
                                                                    CommonLibConstants.WindchillUrl);

                if (FinalStructure != null && FinalStructure.Structure != null && FinalStructure.Structure.Count > 0)
                {
                    GetVisuItemFromBom(FinalStructure.Structure.ToList(), CurrentDataContext.SelectedPart.PartNumber, 1);
                    SearchListPartEcnAsynch(false);
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("VIS_ErrorMsgBomNotFound"), McgWpfTools.GetStringResource("VIS_TitleWindowSearch"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.StatusBarTextRight = "";
                CurrentDataContext.IsSearchInProgress = false;
            }
        }

        private void SearchSapBomComponentAsynch(int BomLevel)
        {
            try
            {
                TraceLog.AddTraceLog($"Start Search SAP BOM for {CurrentDataContext.SelectedPart.PartNumber}.{CurrentDataContext.SelectedPart.PartRevision}, level {BomLevel}");

                CurrentDataContext.IsSearchInProgress = true;
                CurrentDataContext.CurrentStep = 0;
                CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("VIS_StatusBarBomInProgress");

                string tempNumber = CurrentDataContext.Plant.Number == "0000" ? "" : CurrentDataContext.Plant.Number;

                List<BomComponent> extractedBom = _sapBomService.ExtractOneMaterialMasterSapBom(CurrentDataContext.SelectedPart.PartNumber?.Trim(), CurrentDataContext.DateValidity.ToString("yyyyMMdd"), tempNumber, CurrentDataContext.BomUsage.Usage);
                extractedBom.RemoveAll(c => c.Level == 0);
                extractedBom.RemoveAll(c => c.Level > BomLevel);
                extractedBom.RemoveAll(c => string.IsNullOrEmpty(c.Number));

                if (extractedBom != null && extractedBom.Count > 0)
                {
                    GetVisuItemFromSapBom(extractedBom, CurrentDataContext.SelectedPart.PartNumber, 1);
                    SearchListPartEcnAsynch(false);
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("VIS_ErrorMsgBomNotFound"), McgWpfTools.GetStringResource("VIS_TitleWindowSearch"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.StatusBarTextRight = "";
                CurrentDataContext.IsSearchInProgress = false;
            }
        }


        private void SearchVisualizationFileAsynch()
        {
            try
            {
                CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("VIS_StatusBarSearchDocInProgress");
                CurrentDataContext.IsSearchInProgress = true;
                CurrentDataContext.CurrentStep = 0;
                CurrentDataContext.TotalStep = CurrentDataContext.SearchedPartList.Where((visu) => (visu.ItemType == DocumentTypeEnum.ECN ||
                                                                                                    visu.ItemType == DocumentTypeEnum.PART) &&
                                                                                                    !visu.IsDocumentSearched).Count();
                ListItemInProgress = new List<VisualizationItem>();
                VisualizationItem CurrentItem;

                // Search ECN document
                foreach (var item in CurrentDataContext.SearchedPartList.Where((visu) => visu.ItemType == DocumentTypeEnum.ECN && !visu.IsDocumentSearched))
                {
                    CurrentItem = new VisualizationItem()
                    {
                        PartNumber = item.PartNumber,
                        PartRevision = item.PartRevision,
                        ItemType = DocumentTypeEnum.ECN,
                    };
                    ListItemInProgress.Add(CurrentItem);
                    List<RestOdataAttachment> CurrentListDoc = _windchillChangeManagementService.GetChangeNoticeattachments(WindchillNetworkCredential, item.PartNumber, CommonLibConstants.WindchillUrl);
                    if (CurrentListDoc != null && CurrentListDoc.Count > 0)
                        foreach (var doc in CurrentListDoc)
                            CurrentItem.SearchedCompleteDocumentList.Add(new VisualizationDocument()
                            {
                                DocumentNumber = doc.FileName,
                                FileName = doc.FileName,
                                DocumentRevision = "",
                                WindchillEcn = doc,
                                IsDefaultWatermark = true,
                            });
                    item.IsDocumentSearched = true;
                    CurrentDataContext.CurrentStep++;
                }

                // Search Part Document
                foreach (var item in CurrentDataContext.SearchedPartList.Where((visu) => visu.ItemType == DocumentTypeEnum.PART && !visu.IsDocumentSearched))
                {
                    CurrentItem = new VisualizationItem()
                    {
                        PartNumber = item.PartNumber,
                        PartRevision = item.PartRevision,
                        ItemType = DocumentTypeEnum.ECN
                    };
                    ListItemInProgress.Add(CurrentItem);
                    //WindchillObjectViewable CurrentPartViewable = WindchillRestOdataTool.GetPartViewable(WindchillNetworkCredential, item.PartNumber, item.PartRevision, McgMiscTools.GetAppSetting(this, "WindchillUrl"));
                    WindchillObjectViewable CurrentPartViewable = _windchillVisualizationManagementService.GetPartViewableChangeMgtInfo(WindchillNetworkCredential, item.PartNumber, item.PartRevision, CommonLibConstants.WindchillUrl);
                    item.Comment = McgWpfTools.GetStringResource("VIS_Status10");
                    item.DetailComment = McgWpfTools.GetStringResource("VIS_Status10");
                    if (CurrentPartViewable != null)
                    {
                        if (CurrentPartViewable.IsDescribedDocAvailable && CurrentPartViewable.DescribedDoc != null)
                        {
                            CurrentItem.SearchedCompleteDocumentList.Add(new VisualizationDocument()
                            {
                                DocumentNumber = CurrentPartViewable.DescribedDoc.FileName,
                                FileName = CurrentPartViewable.DescribedDoc.FileName,
                                DocumentRevision = CurrentPartViewable.DescribedDoc.Revision,
                                WindchillDocument = CurrentPartViewable.DescribedDoc,
                                WindchillPartViewable = CurrentPartViewable,
                                IsDefaultWatermark = true,
                                Comment = McgWpfTools.GetStringResource("VIS_DocCommentMain"),
                                IsMainDrawing = true
                            });
                            CurrentPartViewable.DescribedDoc.KeepSameName = false;
                            item.Comment = McgWpfTools.GetStringResource("VIS_Status02");
                            if (!CurrentPartViewable.IsDrwAvailable)
                                item.DetailComment = McgWpfTools.GetStringResource("VIS_Status14");
                            else if (!CurrentPartViewable.IsDrwViewableAvailable)
                                item.DetailComment = McgWpfTools.GetStringResource("VIS_Status13");
                            else
                                item.DetailComment = McgWpfTools.GetStringResource("VIS_Status16");
                        }
                        if (CurrentPartViewable.IsDrwViewableAvailable && CurrentPartViewable.DrwViewable != null)
                        {
                            CurrentItem.SearchedCompleteDocumentList.Add(new VisualizationDocument()
                            {
                                DocumentNumber = CurrentPartViewable.DrwViewable.FileName,
                                FileName = CurrentPartViewable.DrwViewable.FileName,
                                DocumentRevision = CurrentPartViewable.DrwViewable.Revision,
                                WindchillDocument = CurrentPartViewable.DrwViewable,
                                WindchillPartViewable = CurrentPartViewable,
                                IsDefaultWatermark = true,
                                Comment = McgWpfTools.GetStringResource("VIS_DocCommentMain"),
                                IsMainDrawing = true
                            });
                            CurrentPartViewable.DrwViewable.KeepSameName = false;
                            item.Comment = McgWpfTools.GetStringResource("VIS_Status01");
                            item.DetailComment = McgWpfTools.GetStringResource("VIS_Status01");
                        }
                        if (CurrentPartViewable.IsReferenceDocAvailable && CurrentPartViewable.ReferenceDoc != null)
                        {
                            CurrentItem.SearchedCompleteDocumentList.Add(new VisualizationDocument()
                            {
                                DocumentNumber = CurrentPartViewable.ReferenceDoc.FileName,
                                FileName = CurrentPartViewable.ReferenceDoc.FileName,
                                DocumentRevision = CurrentPartViewable.ReferenceDoc.Revision,
                                WindchillDocument = CurrentPartViewable.ReferenceDoc,
                                WindchillPartViewable = CurrentPartViewable,
                                IsDefaultWatermark = true,
                                Comment = McgWpfTools.GetStringResource("VIS_DocCommentRef"),
                                IsMainDrawing = true
                            });
                            CurrentPartViewable.ReferenceDoc.KeepSameName = false;
                            if (!CurrentPartViewable.IsDrwAvailable)
                                item.DetailComment = McgWpfTools.GetStringResource("VIS_Status14");
                            else if (!CurrentPartViewable.IsDrwViewableAvailable)
                                item.DetailComment = $"{McgWpfTools.GetStringResource("VIS_Status13")} - {McgWpfTools.GetStringResource("VIS_Status15")}";

                            if (item.Comment != McgWpfTools.GetStringResource("VIS_Status10"))
                                item.Comment = $"{item.Comment} {McgWpfTools.GetStringResource("VIS_Status12")}";
                            else
                                item.Comment = McgWpfTools.GetStringResource("VIS_Status11");
                        }
                        if (CurrentPartViewable.IsOtherDocAvailable && CurrentPartViewable.OtherDoc != null)
                            foreach (var doc in CurrentPartViewable.OtherDoc)
                            {
                                CurrentItem.SearchedCompleteDocumentList.Add(new VisualizationDocument()
                                {
                                    DocumentNumber = doc.FileName,
                                    FileName = doc.FileName,
                                    DocumentRevision = doc.Revision,
                                    WindchillDocument = doc,
                                    WindchillPartViewable = CurrentPartViewable,
                                    IsDefaultWatermark = true,
                                    IsMainDrawing = false
                                });
                                doc.KeepSameName = true;
                            }
                        if (CurrentPartViewable.Is3DViewableAvailable && CurrentPartViewable.PvzViewables != null)
                            foreach (var doc in CurrentPartViewable.PvzViewables)
                            {
                                CurrentItem.SearchedCompleteDocumentList.Add(new VisualizationDocument()
                                {
                                    DocumentNumber = doc.FileName,
                                    FileName = doc.FileName,
                                    DocumentRevision = doc.Revision,
                                    WindchillDocument = doc,
                                    WindchillPartViewable = CurrentPartViewable,
                                    IsDefaultWatermark = false,
                                    IsMainDrawing = false
                                });
                                doc.KeepSameName = true;
                            }

                        if (item.Comment == McgWpfTools.GetStringResource("VIS_Status10"))
                        {
                            if (!CurrentPartViewable.IsDrwAvailable)
                                item.DetailComment = McgWpfTools.GetStringResource("VIS_Status14");
                            else if (!CurrentPartViewable.IsDrwViewableAvailable)
                                item.DetailComment = $"{McgWpfTools.GetStringResource("VIS_Status13")} - {McgWpfTools.GetStringResource("VIS_Status15")}";
                        }
                    }
                    item.IsDocumentSearched = true;
                    CurrentDataContext.CurrentStep++;
                }

                MainDispatcher.Invoke(new Action(UpdateListVisualizationItemDocument));
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.StatusBarTextRight = "";
                CurrentDataContext.IsSearchInProgress = false;
            }
        }

        private void UpdateListVisualizationItem()
        {
            try
            {
                if (ListItemInProgress != null)
                    foreach (var item in ListItemInProgress)
                        if (CurrentDataContext.SearchedPartList.FirstOrDefault((part) => part.PartNumber == item.PartNumber && part.PartRevision == item.PartRevision) == null)
                        {
                            CurrentDataContext.SearchedPartList.Add(item);
                        }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateListVisualizationItemDocument()
        {
            try
            {
                VisualizationItem CurrentVisuItem;
                if (ListItemInProgress != null)
                    foreach (var item in ListItemInProgress)
                    {
                        CurrentVisuItem = CurrentDataContext.SearchedPartList.FirstOrDefault((part) => part.PartNumber == item.PartNumber && part.PartRevision == item.PartRevision);
                        if (CurrentVisuItem != null && item.SearchedCompleteDocumentList.Count > 0)
                        {
                            foreach (var doc in item.SearchedCompleteDocumentList)
                                CurrentVisuItem.SearchedCompleteDocumentList.Add(doc);
                        }
                    }
                ApplyFiltersVisuFile();
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ApplyFiltersVisuFile()
        {
            try
            {
                foreach (var CurrentVisuItem in CurrentDataContext.SearchedPartList)
                {
                    CurrentVisuItem.SearchedDocumentList.Clear();
                    foreach (var doc in CurrentVisuItem.SearchedCompleteDocumentList)
                    {
                        if (doc.DocumentType == DocumentTypeEnum.DXF && CurrentDataContext.IsDxfSelected)
                            CurrentVisuItem.SearchedDocumentList.Add(doc);
                        else if (doc.DocumentType == DocumentTypeEnum.IGES && CurrentDataContext.IsIgesSelected)
                            CurrentVisuItem.SearchedDocumentList.Add(doc);
                        else if (doc.DocumentType == DocumentTypeEnum.STEP && CurrentDataContext.IsStepSelected)
                            CurrentVisuItem.SearchedDocumentList.Add(doc);
                        else if (doc.DocumentType == DocumentTypeEnum.PVZ && CurrentDataContext.IsPvzSelected)
                            CurrentVisuItem.SearchedDocumentList.Add(doc);
                        else if (doc.DocumentType == DocumentTypeEnum.OTHER && CurrentDataContext.IsOtherSelected)
                            CurrentVisuItem.SearchedDocumentList.Add(doc);
                        else if ((doc.DocumentType == DocumentTypeEnum.PDF || doc.DocumentType == DocumentTypeEnum.TIFF) && CurrentDataContext.IsPdfTiffMainSelected && doc.IsMainDrawing)
                            CurrentVisuItem.SearchedDocumentList.Add(doc);
                        else if ((doc.DocumentType == DocumentTypeEnum.PDF || doc.DocumentType == DocumentTypeEnum.TIFF) && CurrentDataContext.IsPdfTiffSelected && !doc.IsMainDrawing)
                            CurrentVisuItem.SearchedDocumentList.Add(doc);
                        else if ((doc.DocumentType == DocumentTypeEnum.WORD || doc.DocumentType == DocumentTypeEnum.EXCEL || doc.DocumentType == DocumentTypeEnum.POWERPOINT) && CurrentDataContext.IsOfficeDocSelected)
                            CurrentVisuItem.SearchedDocumentList.Add(doc);
                        else
                            doc.IsSelected = false;
                    }
                    CurrentVisuItem.IsDocumentFound = (CurrentVisuItem.SearchedDocumentList.Count > 0);
                }

            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void GetVisuItemFromBom(List<WindchillObjStructureComponent> Structure, string UpperLevel, int BomLevel)
        {
            try
            {
                VisualizationItem NewValue = null;
                if (BomLevel == 1)
                    ListItemInProgress = new List<VisualizationItem>();

                foreach (var component in Structure)
                {
                    NewValue = new VisualizationItem()
                    {
                        PartNumber = component.Number,
                        ItemType = DocumentTypeEnum.PART,
                        ItemFrom = DocumentTypeEnum.FROMBOM,
                        AddedFrom = $"{McgWpfTools.GetStringResource("VIS_LabelComponentOf")} {UpperLevel} - {McgWpfTools.GetStringResource("VIS_MiSearchBomLevel")} {BomLevel}"
                    };
                    if (component.MainWindchillObject != null)
                        NewValue.PartRevision = component.MainWindchillObject.Revision;


                    ListItemInProgress.Add(NewValue);
                    if (component.Structure != null && component.Structure.Count > 0)
                        GetVisuItemFromBom(component.Structure.ToList(), UpperLevel, BomLevel + 1);
                }
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }

        private void GetVisuItemFromSapBom (List<BomComponent> Structure, string UpperLevel, int BomLevel)
        {
            try
            {
                VisualizationItem NewValue = null;
                if (BomLevel == 1)
                    ListItemInProgress = new List<VisualizationItem>();
                foreach (var component in Structure)
                {
                    NewValue = new VisualizationItem()
                    {
                        PartNumber = component.Number,
                        ItemType = DocumentTypeEnum.PART,
                        ItemFrom = DocumentTypeEnum.FROMBOM,
                        AddedFrom = $"{McgWpfTools.GetStringResource("VIS_LabelComponentOf")} {UpperLevel} - {McgWpfTools.GetStringResource("VIS_MiSearchBomLevel")} {component.Level}"
                    };
                    if (!string.IsNullOrEmpty(component.Revision))
                        NewValue.PartRevision = component.Revision;

                    ListItemInProgress.Add(NewValue);
                    if (component.Structure != null && component.Structure.Count > 0)
                        GetVisuItemFromSapBom(component.Structure.ToList(), UpperLevel, BomLevel + 1);
                }
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }

        private void GetVisuItemFromClipboard()
        {
            try
            {
                ListItemInProgress = new List<VisualizationItem>();
                string CompleteString = null;
                if (Clipboard.GetData(DataFormats.Text) != null)
                    CompleteString = Clipboard.GetData(DataFormats.Text).ToString();

                if (CompleteString != null)
                {
                    var AllLines = CompleteString.Split('\n');

                    VisualizationItem NewValue = null;
                    string linePurged = null;
                    string TempNumber;
                    string TempRevision;
                    Regex CheckEcn = new Regex(VisualizationLibConstants.EcnCopyPasteRegEx, RegexOptions.IgnoreCase);
                    foreach (var line in AllLines)
                    {
                        linePurged = line.Split('\r').FirstOrDefault();
                        var AllValues = linePurged.Split('\t');
                        if (AllValues != null && AllValues.Count() > 0)
                        {
                            TempNumber = AllValues.FirstOrDefault().Trim().ToUpper();
                            if (TempNumber != null && TempNumber.Trim() != "" && TempNumber.Trim() != "*")
                            {
                                NewValue = new VisualizationItem();
                                NewValue.PartNumber = TempNumber;
                                if (AllValues.Count() > 1)
                                {
                                    TempRevision = AllValues[1].Trim().ToUpper();
                                    NewValue.PartRevision = TempRevision;
                                    if (TempRevision == "ECN" || TempRevision == "ECO" || CheckEcn.IsMatch(TempNumber))
                                        NewValue.ItemType = DocumentTypeEnum.ECN;
                                    else
                                        NewValue.ItemType = DocumentTypeEnum.PART;
                                }
                                else
                                {
                                    NewValue.PartRevision = "";
                                    if (CheckEcn.IsMatch(TempNumber))
                                        NewValue.ItemType = DocumentTypeEnum.ECN;
                                    else
                                        NewValue.ItemType = DocumentTypeEnum.PART;
                                }
                                if (ListItemInProgress.FirstOrDefault((item) => item.PartRevision == NewValue.PartRevision && item.PartNumber == NewValue.PartNumber) == null)
                                    ListItemInProgress.Add(NewValue);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }

        private void DownloadSelectedVisuFileAsynch(bool IsDefaultWatermark = true)
        {
            try
            {
                bool isDocSelected = false;
                bool isOfficialDrawing = false;
                foreach (var visuItem in CurrentDataContext.SearchedPartList.Where((itemFile) => itemFile.IsDocumentSearched))
                    foreach (var visuDoc in visuItem.SearchedDocumentList.Where((docfile) => docfile.IsSelected))
                        isDocSelected = true;

                if (isDocSelected)
                {
                    CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("VIS_StatusBarExportDocInProgress");
                    CurrentDataContext.IsSearchInProgress = true;
                    List<ViewableResult> AllViewableResult = new List<ViewableResult>();

                    // Create Temp Folder to download documents
                    Random rnd = new Random();
                    string TempFolder = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Visu_{rnd.Next(10000000)}";
                    while (Directory.Exists(TempFolder))
                        TempFolder = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Visu_{rnd.Next(10000000)}";

                    if (!Directory.Exists(TempFolder))

                        Directory.CreateDirectory(TempFolder);

                    // Update nb steps
                    CurrentDataContext.CurrentStep = 0;
                    CurrentDataContext.TotalStep = 0;
                    foreach (var item in CurrentDataContext.SearchedPartList.Where((itemFile) => itemFile.IsDocumentSearched))
                        CurrentDataContext.TotalStep += item.SearchedDocumentList.Where((docfile) => docfile.IsSelected).Count();

                    // Download Documents
                    WindchillObjectViewableItemDownload VisuDocDownload;
                    foreach (var visuItem in CurrentDataContext.SearchedPartList.Where((itemFile) => itemFile.IsDocumentSearched))
                    {
                        isOfficialDrawing = false;
                        if (visuItem.ItemType == DocumentTypeEnum.PART)
                        {
                            foreach (var visuDoc in visuItem.SearchedDocumentList.Where((docfile) => docfile.IsSelected))
                            {
                                isOfficialDrawing = OfficialDrawingStates.Contains(visuItem.State?.ToUpper());

                                visuDoc.ListWatermark = GetListWatermark(visuDoc, !isOfficialDrawing);
                                if (!visuDoc.IsAlreadyDownloaded)
                                {
                                    VisuDocDownload = _windchillRequestMiscService.GetWindchillObjectViewableItemDownload(visuDoc.WindchillDocument);
                                    _windchillRequestMiscService.WindchillObjectViewableItemDownloadDownload(VisuDocDownload, WindchillNetworkCredential, TempFolder, false, CurrentDataContext.AddRevisionInFileName, CurrentDataContext.AddStateInFileName);
                                    if (!VisuDocDownload.IsDownloadedOk)
                                    {
                                        visuDoc.IsAlreadyDownloaded = false;
                                        if (MessageBox.Show(string.Format(McgWpfTools.GetStringResource("VIS_DownloadTimeoutMsg"), visuDoc.DocumentNumber), McgWpfTools.GetStringResource("VIS_DownloadTimeoutTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                                            throw new VisualizationToolUserStopException();
                                    }
                                    if (VisuDocDownload.IsDownloadedOk)
                                    {
                                        if (visuDoc.Viewable == null)
                                            visuDoc.Viewable = new ViewableResult() { AllViewableDownload = new List<WindchillObjectViewableItemDownload>() };
                                        VisuDocDownload.ListWatermark = visuDoc.ListWatermark;
                                        visuDoc.Viewable.AllViewableDownload.Add(VisuDocDownload);
                                    }
                                }
                                else if (visuDoc.Viewable.AllViewableDownload != null)
                                {
                                    foreach (var viewableDownload in visuDoc.Viewable.AllViewableDownload)
                                    {
                                        viewableDownload.ListWatermark = visuDoc.ListWatermark;
                                        if (viewableDownload.IsWatermark)
                                            viewableDownload.IsWatermark = IsDefaultWatermark;
                                    }
                                }
                                AllViewableResult.Add(visuDoc.Viewable);
                                visuDoc.IsAlreadyDownloaded = true;
                                visuDoc.ListWatermark = GetListWatermark(visuDoc, !isOfficialDrawing);
                                CurrentDataContext.CurrentStep++;
                            }
                        }

                        if (visuItem.ItemType == DocumentTypeEnum.ECN)
                        {
                            foreach (var visuDoc in visuItem.SearchedDocumentList.Where((docfile) => docfile.IsSelected))
                            {
                                if (visuDoc.WindchillEcn != null)
                                {
                                    _windchillRequestMiscService.Download(visuDoc.WindchillEcn, WindchillNetworkCredential, TempFolder);

                                    if (!visuDoc.WindchillEcn.IsDownloadedOk)
                                    {
                                        visuDoc.IsAlreadyDownloaded = false;
                                        if (MessageBox.Show(string.Format(McgWpfTools.GetStringResource("VIS_DownloadTimeoutMsg"), visuDoc.DocumentNumber), McgWpfTools.GetStringResource("VIS_DownloadTimeoutTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                                            throw new VisualizationToolUserStopException();
                                    }
                                    else
                                    {
                                        if (visuDoc.Viewable == null)
                                            visuDoc.Viewable = new ViewableResult() { AllViewableDownload = new List<WindchillObjectViewableItemDownload>() };
                                        visuDoc.ListWatermark = GetListWatermark(visuDoc);

                                        AllViewableResult.Add(new ViewableResult()
                                        {
                                            AllViewableDownload = new List<WindchillObjectViewableItemDownload>()
                                                { new WindchillObjectViewableItemDownload()
                                                    {
                                                         CompleteFileName=visuDoc.WindchillEcn.CompleteFileName,
                                                         ConvertToPdf=true,
                                                         IsWatermark=IsDefaultWatermark,
                                                         ListWatermark=visuDoc.ListWatermark
                                                    }
                                                }
                                        });
                                    }
                                    CurrentDataContext.CurrentStep++;
                                }
                            }
                        }
                    }



                    // Create ZIP
                    TempFolder = $"{TempFolder}\\zip";
                    if (!Directory.Exists(TempFolder))
                        Directory.CreateDirectory(TempFolder);

                    System.Threading.Thread.Sleep(2000);

                    string ZipFileName = $"{TempFolder}\\export_viewable.zip";
                    _wtDownloadViewableTools.CreateZipFromListViewable(AllViewableResult,
                                ZipFileName,
                                true,
                                TempFolder,
                                CurrentDataContext.ActivatePdfSecurity,
                                new PdfToolsSecuritySetting()
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
                                },
                                (CurrentDataContext.ActivatePdfConvert && CurrentDataContext.ActivateTiffConvert),
                                (CurrentDataContext.ActivatePdfConvert && CurrentDataContext.ActivateWordConvert),
                                (CurrentDataContext.ActivatePdfConvert && CurrentDataContext.ActivateExcelConvert),
                                (CurrentDataContext.ActivatePdfConvert && CurrentDataContext.ActivatePowerPointConvert),
                                CurrentDataContext.IsCreateZip);

                    string TempZipFileName = $"{CurrentDataContext.ExportFolder}\\export_viewable_{rnd.Next(10000000)}.zip";
                    while (File.Exists(TempZipFileName))
                        TempZipFileName = $"{CurrentDataContext.ExportFolder}\\export_viewable_{rnd.Next(10000000)}.zip";


                    if (CurrentDataContext.IsCreateZip)
                    {
                        File.Move(ZipFileName, TempZipFileName);
                        McgFileAndSystemTools.OpenFile(TempZipFileName);
                    }
                    else
                    {
                        foreach (var item in AllViewableResult)
                        {
                            foreach (var file in item.AllViewableDownload)
                            {
                                if (file != null && file.FileName != null && file.CompleteFileName != null && File.Exists(file.CompleteFileName))
                                {
                                    TempZipFileName = $"{CurrentDataContext.ExportFolder}\\{file.CompleteFileName.Split('\\').LastOrDefault()}";
                                    if (File.Exists(TempZipFileName))
                                        File.Delete(TempZipFileName);
                                    File.Move(file.CompleteFileName, TempZipFileName);
                                }
                            }
                        }
                        McgFileAndSystemTools.OpenFile(CurrentDataContext.ExportFolder);
                    }
                }
            }
            catch (VisualizationToolUserStopException)
            {
                TraceLog.AddTraceLog($"VisualizationToolUserStopException in DownloadVisualizationFileViewModel.DownloadSelectedVisuFileAsynch method: Export cancel by user due to Timeout exception");
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.StatusBarTextRight = "";
                CurrentDataContext.IsSearchInProgress = false;
            }
        }

        private List<PdfToolsWatermarkItem> GetListWatermark(VisualizationDocument visuDoc, bool forceWatermark = false)
        {
            try
            {
                PdfToolsWatermarkItem CurrentWatermak;
                List<PdfToolsWatermarkItem> ListWatermark = new List<PdfToolsWatermarkItem>();
                // Optional Watermark
                if (CurrentDataContext.IsOptionalWatermark)
                {
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
                    ListWatermark.Add(CurrentWatermak);
                }

                // Optional Watermark for drawing in state other than "RELEASED" or "PROTOTYPE": add "For Consultation Only" NoneOfficialDrawingWatermark
                if (!CurrentDataContext.IsOptionalWatermark && forceWatermark)
                {
                    CurrentWatermak = new PdfToolsWatermarkItem()
                    {
                        IsWatermark = true,
                        MaxFontSize = VisualizationLibConstants.WatermarkMaxFontSize,
                        MinFontSize = VisualizationLibConstants.WatermarkMinFontSize,
                        TextAxialOffset = VisualizationLibConstants.WatermakTextAxialOffset,
                        TextRadialOffset = VisualizationLibConstants.WatermakTextRadialOffset,
                        TextFont = VisualizationLibConstants.WatermarkFontName,
                        WatermarkPosition = WatermarkPositionEnum.CENTER,
                        WatermarkText = VisualizationLibConstants.NoneOfficialDrawingWatermark
                    };
                    CurrentWatermak.SetFontType(CommonLib.Models.Enums.FontStyle.Regular);
                    ListWatermark.Add(CurrentWatermak);
                }

                // Bottom Watermark
                CurrentWatermak = new PdfToolsWatermarkItem()
                {
                    IsWatermark = CurrentDataContext.IsDefaultWatermark,
                    MaxFontSize = VisualizationLibConstants.WatermarkMaxFontSize,
                    MinFontSize = VisualizationLibConstants.WatermarkMinFontSize,
                    TextAxialOffset = VisualizationLibConstants.WatermakTextAxialOffset,
                    TextRadialOffset = VisualizationLibConstants.WatermakTextRadialOffset,
                    TextFont = VisualizationLibConstants.WatermarkFontName,
                    WatermarkPosition = WatermarkPositionEnum.BOTTOM_TEXT_RIGHT,
                    WatermarkText = _wtDownloadViewableTools.GetWatermarkStateDrw(visuDoc.WindchillPartViewable),
                };
                if (visuDoc.FileName != null && visuDoc.FileName.ToUpper().Contains("CR-"))
                    CurrentWatermak.WatermarkPosition = WatermarkPositionEnum.BOTTOM_TEXT_LEFT;

                CurrentWatermak.SetFontType(CommonLib.Models.Enums.FontStyle.Regular);
                ListWatermark.Add(CurrentWatermak);

                // Right Watermark
                CurrentWatermak = new PdfToolsWatermarkItem()
                {
                    IsWatermark = CurrentDataContext.IsDefaultWatermark,
                    MaxFontSize = VisualizationLibConstants.WatermarkMaxFontSize,
                    MinFontSize = VisualizationLibConstants.WatermarkMinFontSize,
                    TextAxialOffset = VisualizationLibConstants.WatermakTextAxialOffset,
                    TextRadialOffset = VisualizationLibConstants.WatermakTextRadialOffset,
                    TextFont = VisualizationLibConstants.WatermarkFontName,
                    WatermarkPosition = WatermarkPositionEnum.RIGHT_TEXT_BOTTOM,
                    WatermarkText = _wtDownloadViewableTools.GetWatermarkPublishedBy(),
                };
                if (visuDoc.FileName != null && visuDoc.FileName.ToUpper().Contains("CR-"))
                    CurrentWatermak.WatermarkPosition = WatermarkPositionEnum.LEFT_TEXT_BOTTOM;

                CurrentWatermak.SetFontType(CommonLib.Models.Enums.FontStyle.Italic);
                ListWatermark.Add(CurrentWatermak);

                return ListWatermark;
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);
            }
        }

        private void DownloadEcnFlashAsynch()
        {
            try
            {
                RaiseActionInProgressEvent();
                SearchEcnAsynch();
                VisualizationItem CurrentVizuItem = CurrentDataContext.SearchedPartList.FirstOrDefault(item => item.EcnNumber == CurrentDataContext.FilterNumber);
                if (CurrentVizuItem != null)
                {
                    RaiseActionInProgressEvent();
                    SearchVisualizationFileAsynch();

                    // Search all Item from the ECN
                    List<VisualizationItem> EcnDocList = CurrentDataContext.SearchedPartList.Where(item => item.EcnNumber == CurrentVizuItem.EcnNumber && item.ItemType == DocumentTypeEnum.PART).ToList();
                    List<VisualizationDocument> AllDoc = new List<VisualizationDocument>();
                    AllDoc.AddRange(CurrentVizuItem.SearchedCompleteDocumentList);

                    foreach (VisualizationItem EcnItem in EcnDocList)
                        AllDoc.AddRange(EcnItem.SearchedCompleteDocumentList.Where(item => item.IsMainDrawing));


                    if (AllDoc.Count > 0)
                    {
                        ExecuteCheckUncheckAll(false);
                        foreach (var item in AllDoc)
                            item.IsSelected = true;
                        RaiseActionInProgressEvent();
                        DownloadSelectedVisuFileAsynch(CurrentDataContext.IsDefaultWatermark);
                        ExecuteCheckUncheckAll(false);
                    }
                    else
                        MessageBox.Show(McgWpfTools.GetStringResource("VIS_MsgDownloadEcn"), McgWpfTools.GetStringResource("VIS_MsgTitleDownloadEcn"), MessageBoxButton.OK, MessageBoxImage.Warning);

                }
            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.StatusBarTextRight = "";
            }
        }

        private void DownloadPartFlashAsynch()
        {
            try
            {
                SearchPartAsynch();

                VisualizationItem CurrentVizuItem = CurrentDataContext.SearchedPartList.FirstOrDefault(item => item.PartNumber == CurrentDataContext.FilterNumber);
                if (CurrentVizuItem != null)
                {
                    RaiseActionInProgressEvent();
                    SearchVisualizationFileAsynch();

                    if (CurrentVizuItem.SearchedCompleteDocumentList.Count > 0)
                    {
                        var ListDrw = CurrentVizuItem.SearchedCompleteDocumentList.Where(item => item.IsMainDrawing).ToList();
                        if (ListDrw.Count > 0)
                        {
                            ExecuteCheckUncheckAll(false);
                            foreach (var item in ListDrw)
                                item.IsSelected = true;

                            RaiseActionInProgressEvent();
                            DownloadSelectedVisuFileAsynch(CurrentDataContext.IsDefaultWatermark);
                            ExecuteCheckUncheckAll(false);
                        }
                        else
                            MessageBox.Show(McgWpfTools.GetStringResource("VIS_MsgDownloadPart"), McgWpfTools.GetStringResource("VIS_MsgTitleDownloadPart"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                        MessageBox.Show(McgWpfTools.GetStringResource("VIS_MsgDownloadPart"), McgWpfTools.GetStringResource("VIS_MsgTitleDownloadPart"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }


            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.StatusBarTextRight = "";
            }
        }

        [Obsolete]
        private void DownloadSelectedVisuFileAsynchOld()
        {
            try
            {
                CurrentDataContext.IsSearchInProgress = true;
                List<ViewableResult> AllViewableResult = new List<ViewableResult>();

                // Create Temp Folder to download documents
                Random rnd = new Random();
                string TempFolder = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Visu_{rnd.Next(10000000)}";
                while (Directory.Exists(TempFolder))
                    TempFolder = $"{System.Environment.GetEnvironmentVariable("TEMP")}\\Visu_{rnd.Next(10000000)}";

                if (!Directory.Exists(TempFolder))
                    Directory.CreateDirectory(TempFolder);

                // Update nb steps
                CurrentDataContext.CurrentStep = 0;
                foreach (var item in CurrentDataContext.SearchedPartList.Where((itemFile) => itemFile.IsDocumentSearched))
                    CurrentDataContext.TotalStep += item.SearchedDocumentList.Where((docfile) => docfile.IsSelected).Count();

                // Download Documents
                foreach (var item in CurrentDataContext.SearchedPartList.Where((itemFile) => itemFile.IsDocumentSearched))
                {
                    if (item.ItemType == DocumentTypeEnum.PART)
                    {
                        foreach (var doc in item.SearchedDocumentList.Where((docfile) => docfile.IsSelected))
                        {
                            if (!doc.IsAlreadyDownloaded)
                                doc.Viewable = _wtDownloadViewableTools.GetOneWtPartViewables(WindchillNetworkCredential, doc.WindchillPartViewable, doc.WindchillDocument, doc.IsDefaultWatermark, doc.IsOptionaltWatermark, doc.OptionalWatermark, true, TempFolder, CommonLibConstants.WindchillUrl);
                            AllViewableResult.Add(doc.Viewable);
                            doc.IsAlreadyDownloaded = true;
                            CurrentDataContext.CurrentStep++;
                        }
                    }

                    if (item.ItemType == DocumentTypeEnum.ECN)
                    {
                        foreach (var doc in item.SearchedDocumentList.Where((docfile) => docfile.IsSelected))
                        {
                            if (doc.WindchillEcn != null)
                            {
                                doc.WindchillEcn.Download(WindchillNetworkCredential, TempFolder);
                                AllViewableResult.Add(new ViewableResult()
                                {
                                    AllViewableDownload = new List<WindchillObjectViewableItemDownload>()
                                    { new WindchillObjectViewableItemDownload()
                                        {
                                             CompleteFileName=doc.WindchillEcn.CompleteFileName,
                                             ConvertToPdf=false,
                                             IsWatermark=false
                                        }
                                    }
                                });
                                CurrentDataContext.CurrentStep++;
                            }
                        }
                    }
                }

                foreach (var viewRes in AllViewableResult)
                {
                    foreach (var viewDoc in viewRes.AllViewableDownload)
                    {
                        viewDoc.IsOptionalWatermark = CurrentDataContext.IsOptionalWatermark;
                        viewDoc.OptionalWatermark = CurrentDataContext.OptionalWatermark;
                    }
                }

                // Create ZIP
                TempFolder = $"{TempFolder}\\zip";
                if (!Directory.Exists(TempFolder))
                    Directory.CreateDirectory(TempFolder);

                string ZipFileName = $"{TempFolder}\\export_viewable.zip";
                _wtDownloadViewableTools.CreateZipFromListViewable(AllViewableResult,
                            ZipFileName,
                            true,
                            TempFolder,
                            CurrentDataContext.ActivatePdfSecurity,
                            new PdfToolsSecuritySetting()
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
                            },
                            (CurrentDataContext.ActivatePdfConvert && CurrentDataContext.ActivateTiffConvert),
                            (CurrentDataContext.ActivatePdfConvert && CurrentDataContext.ActivateWordConvert),
                            (CurrentDataContext.ActivatePdfConvert && CurrentDataContext.ActivateExcelConvert),
                            (CurrentDataContext.ActivatePdfConvert && CurrentDataContext.ActivatePowerPointConvert));

                string TempZipFileName = $"{CurrentDataContext.ExportFolder}\\export_viewable_{rnd.Next(10000000)}.zip";
                while (File.Exists(TempZipFileName))
                    TempZipFileName = $"{CurrentDataContext.ExportFolder}\\export_viewable_{rnd.Next(10000000)}.zip";

                File.Move(ZipFileName, TempZipFileName);

                McgFileAndSystemTools.OpenFile(TempZipFileName);

            }
            catch (Exception ex)
            {
                VisualizationException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                CurrentDataContext.IsSearchInProgress = false;
            }
        }
        #endregion


    }
}
