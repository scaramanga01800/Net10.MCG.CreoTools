using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.SapTools.Interfaces;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WebtermLib.Services.Interfaces;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.CraneSearch;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.ViewModel.CraneSearch
{
    public class CraneSearchViewModel : ObservableObject, ICraneSearchViewModel
    {
        #region [REGION] Properties from Interface
        public CraneSearchDataContext CurrentDataContext { get; set; }
        public List<string> PartList { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private SapConfiguration CurrentConfiguration { get; set; }
        private List<string> ListItemInProgress { get; set; }
       // private McgCraneMapping CraneMapping { get; set; } = new McgCraneMapping();
        #endregion

        #region [REGION] Commands
        public ICommand CommandCtrlPaste { get => new RelayCommand(() => ExecuteCtrlVPaste()); }
        public ICommand CommandPaste { get => new RelayCommand(() => ExecutePaste()); }
        public ICommand CommandSearchSapCrane { get => new RelayCommand(() => ExecuteSearchSapCrane()); }
        public ICommand CommandExportExcel { get => new RelayCommand(() => ExecuteExportExcel()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        public ICommand CommandClose { get => new RelayCommand(() => ExecuteClose()); }
        public ICommand CommandRemoveAll { get => new RelayCommand(() => ExecuteRemoveAll()); }
        #endregion

        #region [REGION] Events Action

        public event EventHandler CallCloseEvent;

        public void RaiseCallCloseEvent()
        {
            try
            {
                CallCloseEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Init
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        private readonly ISapMaterialService _sapMaterialService;
        private readonly IMcgCraneMapping _craneMapping;

        public CraneSearchViewModel(IXmlSerializeTools xmlSerializeTools,
                                    IMcgCommonLibWindowService mcgCommonLibWindowService,
                                    ISapMaterialService sapMaterialService,
                                    IMcgCraneMapping mcgCraneMapping)
        {
            _xmlSerializeTools = xmlSerializeTools;
            _mcgCommonLibWindowService = mcgCommonLibWindowService;
            _sapMaterialService = sapMaterialService;
            _craneMapping = mcgCraneMapping;

            try
            {
                string envVarName = CommonLibConstants.MainAppFolderEnvirName;
                MainAppFolder = Environment.GetEnvironmentVariable(envVarName) ?? CommonLibConstants.MainAppFolder;

                string resourcesFolder = CommonLibConstants.ResourcesFolder;
                string configFileName = MiscToolsConstants.ConfigurationSapBomExport;
                string configFilePath = Path.Combine(MainAppFolder, resourcesFolder, configFileName);

                CurrentConfiguration = _xmlSerializeTools.GetDeserializedXml<SapConfiguration>(configFilePath);
                CurrentDataContext = new CraneSearchDataContext();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteCtrlVPaste(KeyEventArgs e = null)
        {
            try
            {
                if (CurrentDataContext.IsStandAlone)
                    if (e == null || (Keyboard.Modifiers == ModifierKeys.Control && e != null && e.Key == Key.V))
                    {
                        ExecutePaste();
                    }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecutePaste()
        {
            try
            {
                if (PartList == null)
                    PartList = new List<string>();

                ListItemInProgress = new List<string>();

                string clipboardText = Clipboard.GetText(TextDataFormat.Text);
                if (string.IsNullOrWhiteSpace(clipboardText))
                    return;

                var lines = clipboardText.Split('\n');

                foreach (var rawLine in lines)
                {
                    var line = rawLine.Split('\r').FirstOrDefault()?.Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var values = line.Split('\t');
                    if (values.Length == 0)
                        continue;

                    var tempNumber = values[0].Trim().ToUpper();
                    if (!string.IsNullOrWhiteSpace(tempNumber) && tempNumber != "*"
                        && !ListItemInProgress.Contains(tempNumber))
                    {
                        ListItemInProgress.Add(tempNumber);
                    }
                }

                PartList = PartList.Union(ListItemInProgress).ToList();

                foreach (var part in PartList)
                {
                    if (!CurrentDataContext.PartList.Contains(part))
                        CurrentDataContext.PartList.Add(part);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecutePaste2()
        {
            try
            {

                if (PartList == null)
                    PartList = new List<string>();

                ListItemInProgress = new List<string>();
                string CompleteString = null;
                if (Clipboard.GetData(DataFormats.Text) != null)
                    CompleteString = Clipboard.GetData(DataFormats.Text).ToString();

                if (CompleteString != null)
                {
                    var AllLines = CompleteString.Split('\n');

                    string linePurged = null;
                    string TempNumber;
                    foreach (var line in AllLines)
                    {
                        linePurged = line.Split('\r').FirstOrDefault();
                        var AllValues = linePurged.Split('\t');
                        if (AllValues != null && AllValues.Count() > 0)
                        {
                            TempNumber = AllValues.FirstOrDefault().Trim().ToUpper();
                            if (TempNumber != null && TempNumber.Trim() != "" && TempNumber.Trim() != "*")
                            {
                                if (ListItemInProgress.FirstOrDefault((item) => item == TempNumber) == null)
                                    ListItemInProgress.Add(TempNumber);
                            }
                        }
                    }
                }
                PartList = PartList.Union(ListItemInProgress).ToList();

                if (PartList != null && PartList.Count > 0)
                {
                    foreach (var part in PartList)
                    {
                        if (CurrentDataContext.PartList.FirstOrDefault(item => item == part) == null)
                            CurrentDataContext.PartList.Add(part);
                    }
                }

            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSearchSapCrane()

        {
            try
            {
                SearchSapCraneAsynch();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteExportExcel()
        {
            try
            {
                if (PartList == null || PartList.Count == 0)
                    return;

                var regexProc = new Regex("Excel", RegexOptions.IgnoreCase);
                var oldExcelProcesses = Process.GetProcesses().Where(p => regexProc.IsMatch(p.ProcessName)).ToList();

                string userDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string xlsFilePath = Path.Combine(userDocuments, "CRANE.xlsx");

                var excel = new ExcelToolsClosedXml { CompleteFileName = xlsFilePath };
                string tabParts = McgWpfTools.GetStringResource("CRS_TabParts");

                if (excel.CreateNewFile(tabParts) != ExcelStatus.OK)
                {
                    ShowExportError(xlsFilePath);
                    return;
                }

                // Onglet Pièces
                excel.CurrentSheet = tabParts;
                excel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColHeader01"), 1, 1);
                int row = 2;
                foreach (var part in PartList)
                {
                    excel.SetCellValue(part, row++, 1);
                }

                // Onglet Grues
                string tabCrane = McgWpfTools.GetStringResource("CRS_TabCrane");
                excel.CreateSheet(tabCrane);
                excel.CurrentSheet = tabCrane;
                excel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColCraneName"), 1, 1);
                excel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColPlant"), 1, 2);
                excel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColPlantCrane"), 1, 3);

                row = 2;
                foreach (var crane in CurrentDataContext.CraneList)
                {
                    excel.SetCellValue(crane.CraneName, row, 1);
                    excel.SetCellValue(crane.Plant, row, 2);
                    excel.SetCellValue(crane.PlantCrane, row, 3);
                    row++;

                    foreach (var part in crane.PartList)
                    {
                        excel.SetCellValue(part.Number, row, 2);
                        excel.SetCellValue(part.Description, row, 3);
                        row++;
                    }
                }

                // Onglet Usines
                string tabPlant = McgWpfTools.GetStringResource("CRS_TabPlant");
                excel.CreateSheet(tabPlant);
                excel.CurrentSheet = tabPlant;
                excel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColPlantNumber"), 1, 1);
                excel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColPlantName"), 1, 2);
                excel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColPlantCity"), 1, 3);

                row = 2;
                foreach (var plant in CurrentDataContext.PlantList)
                {
                    excel.SetCellValue(plant.Number, row, 1);
                    excel.SetCellValue(plant.Name, row, 2);
                    excel.SetCellValue(plant.City, row, 3);
                    row++;
                }

                if (excel.SaveClose() != ExcelStatus.OK)
                {
                    ShowExportError(xlsFilePath);
                    return;
                }

                var newExcelProcesses = Process.GetProcesses().Where(p => regexProc.IsMatch(p.ProcessName)).ToList();
                var newExcelProcess = newExcelProcesses.FirstOrDefault(p => !oldExcelProcesses.Any(old => old.Id == p.Id));
                if (newExcelProcess != null)
                    newExcelProcess.Kill();


                _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("CRS_BtCraneExport"),
                                                                       string.Format(McgWpfTools.GetStringResource("CRS_ExportXls"), xlsFilePath),
                                                                       McgWpfTools.GetStringResource("CRS_ToolTipOpen"),
                                                                       McgWpfTools.GetStringResource("CRS_ToolTipOpenFolder"),
                                                                       McgWpfTools.GetStringResource("CRS_ToolTipClose"),
                                                                       xlsFilePath);
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void ShowExportError(string filePath)
        {
            try
            {
                MessageBox.Show(
                    string.Format(McgWpfTools.GetStringResource("CRS_ExportXlsIssue"), filePath),
                    McgWpfTools.GetStringResource("CRS_TitleExportXlsIssue"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void ExecuteExportExcel2()
        {

            try
            {
                if (PartList != null) //&& CurrentDataContext.PartList.Count > 0)
                {
                    Regex RegexProc = new Regex("Excel", RegexOptions.IgnoreCase);
                    List<Process> OldExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    string UserDocumentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string XlsFileName = $"{UserDocumentFolder}\\CRANE.xlsx";

                    ExcelToolsClosedXml CurrentExcel = new ExcelToolsClosedXml() { CompleteFileName = XlsFileName };
                    if (CurrentExcel.CreateNewFile(McgWpfTools.GetStringResource("CRS_TabParts")) != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("CRS_ExportXlsIssue"), XlsFileName), McgWpfTools.GetStringResource("CRS_TitleExportXlsIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    List<Process> NewExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    Process newExcelProcess = NewExcelCurrentProcess.FirstOrDefault((proc) => !OldExcelCurrentProcess.ToList().Exists((oldprc) => proc.Id == oldprc.Id));

                    // Update Part tab
                    CurrentExcel.CurrentSheet = McgWpfTools.GetStringResource("CRS_TabParts");

                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColHeader01"), 1, 1);

                    int index = 2;
                    foreach (var part in PartList)
                    {
                        CurrentExcel.SetCellValue(part, index, 1);
                        index++;
                    }

                    // Update Crane tab

                    CurrentExcel.CreateSheet(McgWpfTools.GetStringResource("CRS_TabCrane"));
                    CurrentExcel.CurrentSheet = McgWpfTools.GetStringResource("CRS_TabCrane");
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColCraneName"), 1, 1);
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColPlant"), 1, 2);
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColPlantCrane"), 1, 3);
                    index = 2;
                    foreach (var crane in CurrentDataContext.CraneList)
                    {
                        CurrentExcel.SetCellValue(crane.CraneName, index, 1);
                        CurrentExcel.SetCellValue(crane.Plant, index, 2);
                        CurrentExcel.SetCellValue(crane.PlantCrane, index, 3);
                        index++;

                        foreach (var part in crane.PartList)
                        {
                            CurrentExcel.SetCellValue(part.Number, index, 2);
                            CurrentExcel.SetCellValue(part.Description, index, 3);
                            index++;
                        }
                    }

                    // Update plant tab
                    CurrentExcel.CreateSheet(McgWpfTools.GetStringResource("CRS_TabPlant"));

                    CurrentExcel.CurrentSheet = McgWpfTools.GetStringResource("CRS_TabPlant");
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColPlantNumber"), 1, 1);
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColPlantName"), 1, 2);
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("CRS_ColPlantCity"), 1, 3);
                    index = 2;
                    foreach (var plant in CurrentDataContext.PlantList)
                    {
                        CurrentExcel.SetCellValue(plant.Number, index, 1);
                        CurrentExcel.SetCellValue(plant.Name, index, 2);
                        CurrentExcel.SetCellValue(plant.City, index, 3);

                        index++;
                    }

                    if (CurrentExcel.SaveClose() != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("CRS_ExportXlsIssue"), XlsFileName), McgWpfTools.GetStringResource("CRS_TitleExportXlsIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    if (newExcelProcess != null)
                        newExcelProcess.Kill();

                    _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("CRS_BtCraneExport"), 
                                                                           string.Format(McgWpfTools.GetStringResource("CRS_ExportXls"), XlsFileName), 
                                                                           McgWpfTools.GetStringResource("CRS_ToolTipOpen"), 
                                                                           McgWpfTools.GetStringResource("CRS_ToolTipOpenFolder"), 
                                                                           McgWpfTools.GetStringResource("CRS_ToolTipClose"), XlsFileName);
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenHelp()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("CRS_UserGuide"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteClose()
        {
            try
            {
                RaiseCallCloseEvent();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemoveAll()
        {
            try
            {
                if (MessageBox.Show(McgWpfTools.GetStringResource("CRS_MsgRemoveAll"), McgWpfTools.GetStringResource("CRS_TitleRemoveAll"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    CurrentDataContext.CraneList.Clear();
                    CurrentDataContext.PlantList.Clear();
                    CurrentDataContext.PartList.Clear();
                    PartList.Clear();
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        #endregion

        #region [REGION] Read/update information in SQL Server DataBase
        private async void SearchSapCraneAsynch()
        {
            try
            {
                var craneList =  _sapMaterialService.ZDTB_DIS_SELCODE(PartList);
               
                if (craneList == null || craneList.Count == 0)
                {
                    MessageBox.Show(
                        string.Format(McgWpfTools.GetStringResource("EDC_InfoCraneSearchNotFound"), "SAP"),
                        McgWpfTools.GetStringResource("EDC_InfoTitleErpBom"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    RaiseCallCloseEvent();
                    return;
                }

                CurrentDataContext.CraneList.Clear();
                CurrentDataContext.PlantList.Clear();

                var groupedItems = craneList
                    .GroupBy(x => new { x.CraneModel, x.CranePlant, x.Plant })
                    .Select(g => g.First())
                    .ToList();

                foreach (var item in groupedItems)
                {
                    var craneItem = new CraneSearchItem
                    {
                        CraneName = item.CraneModel,
                        PlantCrane = item.CranePlant,
                        Plant = item.Plant,
                        PartList = new System.Collections.ObjectModel.ObservableCollection<SapGenericObject>()
                    };

                    foreach (var part in craneList.Where(x =>
                        x.CraneModel == item.CraneModel &&
                        x.CranePlant == item.CranePlant &&
                        x.Plant == item.Plant))
                    {
                        craneItem.PartList.Add(part);
                    }

                    CurrentDataContext.CraneList.Add(craneItem);

                    AddPlantIfMissing(item.Plant);
                    AddPlantIfMissing(item.CranePlant);
                }

                List<string> DistinctCraneNames =  CurrentDataContext.CraneList
                         .Select(c => c.CraneName)
                         .Where(name => !string.IsNullOrWhiteSpace(name))
                         .Distinct()
                         .ToList();
                CurrentDataContext.EuropeEquivalent = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, string>>(_craneMapping.ConvertToEuropeCraneNameDictionary(DistinctCraneNames));
                CurrentDataContext.AsiaEquivalent = new System.Collections.ObjectModel.ObservableCollection<KeyValuePair<string, string>>(_craneMapping.ConvertToAsiaCraneNameDictionary(DistinctCraneNames));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void AddPlantIfMissing(string plantNumber)
        {
            try
            {
                var existingPlant = CurrentDataContext.PlantList.FirstOrDefault(p => p.Number == plantNumber);
                if (existingPlant == null)
                {
                    var configPlant = CurrentConfiguration.AllSapPlant.FirstOrDefault(p => p.Number == plantNumber);
                    if (configPlant != null)
                    {
                        CurrentDataContext.PlantList.Add(configPlant);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
