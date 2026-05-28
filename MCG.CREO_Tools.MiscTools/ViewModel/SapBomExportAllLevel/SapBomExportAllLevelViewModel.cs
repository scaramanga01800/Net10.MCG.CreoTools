using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.SapTools.Exceptions;
using MCG.CommonLib.SapTools.Interfaces;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.SapBomExportAllLevel;
using MCG.WindchillRequestTool.Model.BomComparison;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.MiscTools.ViewModel.SapBomExportAllLevel
{
    public class SapBomExportAllLevelViewModel : ObservableObject, ISapBomExportAllLevelViewModel
    {
        #region [REGION] Properties from Interface
        public SapBomExportAllLevelDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private SapConfiguration CurrentConfiguration { get; set; }
        private Thread ThreadSearchBom { get; set; }
        private Dispatcher MainDispatcher { get; set; } = null;
        #endregion

        #region [REGION] Commands
        public ICommand CommandStartSapBomExport { get => new RelayCommand(() => ExecuteStartSapBomExport()); }
        public ICommand CommandStartExportExcel { get => new RelayCommand(() => ExecuteStartExportExcel()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        public ICommand CommandExit { get => new RelayCommand(() => ExecuteExit()); }
        public ICommand CommandExpandAll { get => new RelayCommand(() => ExecuteExpandAll()); }
        public ICommand CommandCollapseAll { get => new RelayCommand(() => ExecuteCollapseAll()); }
        public ICommand CommandToggleExpandCollapse { get => new RelayCommand<bool>((obj) => ExecuteToggleExpandCollapse(obj)); }

        #endregion

        #region [REGION] Init
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly ISapBomService _sapBomService;
        private readonly ISapHupService _sapHupService;
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        public SapBomExportAllLevelViewModel(IXmlSerializeTools xmlSerializeTools,
                                             ISapBomService sapBomService,
                                             IMcgCommonLibWindowService mcgCommonLibWindowService,
                                             ISapHupService sapHupService)
        {
            try
            {
                _xmlSerializeTools = xmlSerializeTools;
                _sapBomService = sapBomService;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;
                _sapHupService = sapHupService;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                MainDispatcher = Dispatcher.CurrentDispatcher;

                CurrentDataContext = new SapBomExportAllLevelDataContext();

                CurrentConfiguration = _xmlSerializeTools.GetDeserializedXml<SapConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.ConfigurationSapBomExport}");
                if (CurrentConfiguration != null && CurrentConfiguration.ListSapPlant != null && CurrentConfiguration.ListBomApplication != null)
                {
                    foreach (var item in CurrentConfiguration.ListSapPlant)
                        CurrentDataContext.AllSapPlants.Add(item);
                    CurrentDataContext.Plant = CurrentDataContext.AllSapPlants.FirstOrDefault();
                }

                var listBomUsage = McgBusinessTools.GetLIstSapBomUsage();
                if (listBomUsage != null && listBomUsage.Count > 0)
                    foreach (var usage in listBomUsage)
                        CurrentDataContext.AllBomUsage.Add(usage);
                CurrentDataContext.BomUsage = CurrentDataContext.AllBomUsage.FirstOrDefault(item => item.Usage == "3");
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteStartSapBomExport()
        {
            try
            {
                CurrentDataContext.MainStructure.Clear();
                CurrentDataContext.AllComponents.Clear();
                CurrentDataContext.FlatStructure.Clear();

                ThreadSearchBom = new Thread(() => SapBomExportAsynch());
                ThreadSearchBom.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartExportExcel()
        {
            try
            {
                ExportBomToExcel();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenHelp()
        {
            try
            {
                McgMiscTools.OpenSharePointDocument(McgMiscTools.GetStringResource("SEA_UserGuide"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteExit()
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteExpandAll()
        {
            try
            {
                foreach (var item in CurrentDataContext.MainStructure)
                {
                    ExpandRecursive(item);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCollapseAll()
        {
            try
            {
                foreach (var item in CurrentDataContext.MainStructure)
                {
                    CollapseRecursive(item);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteToggleExpandCollapse(bool IsExpanded)
        {
            try
            {
                if (IsExpanded)
                    ExecuteExpandAll();
                else
                    ExecuteCollapseAll();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        private void SapBomExportAsynch()
        {
            try
            {
                CurrentDataContext.IsPleaseWaitShown = true;

                if (!string.IsNullOrEmpty(CurrentDataContext.PartNumber))
                {
                    string tempNumber = CurrentDataContext.Plant.Number == "0000" ? "" : CurrentDataContext.Plant.Number;

                    try
                    {
                        List<BomComponent> extractedBom = _sapBomService.ExtractOneMaterialMasterSapBom(CurrentDataContext.PartNumber?.Trim(), CurrentDataContext.DateValidity.ToString("yyyyMMdd"), tempNumber, CurrentDataContext.BomUsage.Usage);
                        if (extractedBom != null && extractedBom.Count > 0)
                        {
                            UpdateColumnWidth(extractedBom);
                            if (extractedBom.Count < 1000)
                            {
                                var BomStructure = RestructureBomComponents(extractedBom);
                                MainDispatcher.Invoke(() => UpdateMainBom(BomStructure));
                            }
                            else
                            {
                                MainDispatcher.Invoke(() => UpdateMainBom(extractedBom));
                            }
                            MainDispatcher.Invoke(() => SearchAllComponents(extractedBom));
                            UpdateSapCostVolumeInformation();
                        }
                        else
                        {
                            MessageBox.Show(McgWpfTools.GetStringResource("SEA_InfoMsgNoBom"), McgWpfTools.GetStringResource("SEA_WindowInfoTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                            CurrentDataContext.IsPleaseWaitShown = false;
                        }
                    }
                    catch (SapToolsNoConnectionException)
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("SEA_InfoMsgErpConNotFound"), McgWpfTools.GetStringResource("SEA_WindowInfoTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        CurrentDataContext.IsPleaseWaitShown = false;
                    }
                }
                else
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("SEA_InfoMsgNoPart"), McgWpfTools.GetStringResource("SEA_WindowInfoTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    CurrentDataContext.IsPleaseWaitShown = false;
                }
            }
            catch (Exception ex)
            {
                CurrentDataContext.IsPleaseWaitShown = false;
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsPleaseWaitShown = false;
            }
        }

        private List<BomComponent> RestructureBomComponents(List<BomComponent> flatList)
        {
            try
            {
                List<BomComponent> rootElements = new List<BomComponent>();
                Stack<BomComponent> stack = new Stack<BomComponent>();
                BomComponent firstCompo = flatList.FirstOrDefault();
                if (firstCompo != null)
                    flatList.Remove(firstCompo);


                foreach (var component in flatList)
                {
                    CurrentDataContext.MaxBomLevel = Math.Max(CurrentDataContext.MaxBomLevel, component.Level);
                    if (component.Level == 1)
                    {
                        rootElements.Add(component);
                        stack.Clear();
                        stack.Push(component);
                    }
                    else
                    {
                        int diff = component.Level - (stack.Count > 0 ? stack.Peek().Level : 0);

                        if (diff == 1)
                        {
                            // Nouvelle structure à créer ou mettre à jour
                            if (stack.Peek().Structure == null)
                                stack.Peek().Structure = new List<BomComponent>();
                            stack.Peek().Structure.Add(component);
                            stack.Push(component);
                        }
                        else if (diff == 0)
                        {
                            // Structure actuelle à mettre à jour
                            stack.Pop(); // Remonter d'un niveau
                            stack.Peek().Structure.Add(component);
                            stack.Push(component);
                        }
                        else if (diff < 0)
                        {
                            // Remonter d'un ou plusieurs niveaux
                            while (stack.Count > 0 && stack.Peek().Level >= component.Level)
                            {
                                stack.Pop();
                            }

                            if (stack.Count > 0)
                            {
                                stack.Peek().Structure.Add(component);
                                stack.Push(component);
                            }
                        }
                        // Ignorer les niveaux suivants (diff > 1), vous pouvez ajuster selon vos besoins.
                    }
                }

                List<BomComponent> returnBom = new List<BomComponent>();
                returnBom.Add(firstCompo);
                firstCompo.Structure = new List<BomComponent>();
                foreach (var comp in rootElements)
                    firstCompo.Structure.Add(comp);
                return returnBom;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void UpdateColumnWidth(List<BomComponent> Structure)
        {
            try
            {
                CurrentDataContext.SizeColumns.Clear();

                // init size
                for (int index = 3; index <= 29; index++)
                {
                    if (index < 10)
                        CurrentDataContext.SizeColumns.Add(McgWpfTools.GetStringResource($"SBE_LabelCol0{index}").Length * 7 + 20);
                    else
                        CurrentDataContext.SizeColumns.Add(McgWpfTools.GetStringResource($"SBE_LabelCol{index}").Length * 7 + 20);
                }

                List<string> ListParams = new List<string>();
                ListParams.Add("Number");
                ListParams.Add("LineNumber");
                ListParams.Add("Description");
                ListParams.Add("Revision");
                ListParams.Add("Quantity");
                ListParams.Add("Unit");
                ListParams.Add("StandardPrice");
                ListParams.Add("Supplier");
                ListParams.Add("PriecMAss");

                int currentMaxCarac = 0;
                int indexParam = 0;
                foreach (var param in ListParams)
                {
                    currentMaxCarac = MaxCharacterColValue(Structure, param, 5);

                    if (indexParam < CurrentDataContext.SizeColumns.Count)
                        CurrentDataContext.SizeColumns[indexParam] = Math.Max(CurrentDataContext.SizeColumns.ElementAt(indexParam), currentMaxCarac * 7 + 20);

                    indexParam++;
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private int MaxCharacterColValue(List<BomComponent> Structure, string NameCol, int InitSize = 10)
        {
            try
            {
                PropertyInfo CurrentProp;
                int MaxCharac = InitSize;
                object temp;

                foreach (var comp in Structure)
                {
                    CurrentProp = comp.GetType().GetProperty(NameCol);
                    if (CurrentProp != null)
                    {
                        temp = CurrentProp.GetValue(comp);
                        if (temp != null) MaxCharac = Math.Max(MaxCharac, temp.ToString().Length);
                    }

                    if (comp.Structure != null && comp.Structure.Count > 0)
                        MaxCharac = Math.Max(MaxCharac, MaxCharacterColValue(comp.Structure, NameCol));
                }

                return MaxCharac;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void UpdateMainBom(List<BomComponent> Structure)
        {
            try
            {
                foreach (var item in Structure)
                {
                    CurrentDataContext.MainStructure.Add(item);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void SearchAllComponents(List<BomComponent> Structure)
        {
            try
            {
                var dicCompoQuantiy = (new BomComponent()
                {
                    Number = "HighLv",
                    Quantity = 1,
                    Structure = Structure
                }).GetCumulativeQuantities();

                var result = Structure
                 .GroupBy(c => c.Number)
                 .Select(g =>
                 {
                     var first = g.First(); // première occurrence
                     return new BomComponent
                     {
                         Number = first.Number,
                         Quantity = g.Sum(c => c.Quantity),
                         Description = first.Description,
                         StandardPrice = first.StandardPrice,
                         Revision = first.Revision,
                         Supplier = first.Supplier,
                     };
                 })
                 .ToList();

                foreach (var item in Structure)
                    CurrentDataContext.FlatStructure.Add(item);

                foreach (var item in result)
                {
                    item.Quantity = dicCompoQuantiy.FirstOrDefault(comp => comp.Key == item.Number).Value;
                    CurrentDataContext.AllComponents.Add(item);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExportBomToExcel()
        {
            try
            {
                if (CurrentDataContext.MainStructure != null && CurrentDataContext.MainStructure.Count > 0)
                {
                    Regex RegexProc = new Regex("Excel", RegexOptions.IgnoreCase);
                    List<Process> OldExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    string UserDocumentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string XlsFileName = $"{UserDocumentFolder}\\BOM_{CurrentDataContext.PartNumber}_{CurrentDataContext.Plant?.Number}.xlsx";


                    string templateFileName = $@"{MainAppFolder}\{CommonLibConstants.ResourcesFolder}\{MiscToolsConstants.TemplateSapBomExport2}".Replace("\\\\", "\\");
                    ExcelToolsClosedXml CurrentExcel = new ExcelToolsClosedXml() { CompleteFileName = XlsFileName, CompleteTemplateFileName = templateFileName };
                    if (CurrentExcel.OpenFile(templateFileName) != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("SEA_ExportXlsIssue"), XlsFileName), McgWpfTools.GetStringResource("SEA_TitleExportXlsIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    List<Process> NewExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    Process newExcelProcess = NewExcelCurrentProcess.FirstOrDefault((proc) => !OldExcelCurrentProcess.ToList().Exists((oldprc) => proc.Id == oldprc.Id));

                    // Update Main information
                    CurrentExcel.CurrentSheet = MiscToolsConstants.TemplateSapBomExportMainTab;
                    CurrentExcel.SetCellValue(CurrentDataContext.PartNumber, 1, 2);
                    CurrentExcel.SetCellValue(CurrentDataContext.Plant?.Number, 2, 2);
                    CurrentExcel.SetCellValue(CurrentDataContext.MainDescription, 3, 2);

                    int index = MiscToolsConstants.TemplateSapBomExportFirstCompIndex2;

                    // Update Structure
                    List<BomComponent> TempBom = new List<BomComponent>();
                    TempBom.AddRange(CurrentDataContext.MainStructure);

                    RecursiveWriteStructureXls(CurrentExcel, TempBom, index);

                    //Update Components
                    CurrentExcel.CurrentSheet = MiscToolsConstants.TemplateSapBomExportCompTab;
                    index = 2;
                    foreach (var comp in CurrentDataContext.AllComponents)
                    {
                        CurrentExcel.SetCellValue(comp.Number, index, 1);
                        CurrentExcel.SetCellValue(comp.Description, index, 2);
                        CurrentExcel.SetCellValue(comp.Revision, index, 3);
                        CurrentExcel.SetCellValue(comp.Quantity, index, 4);
                        CurrentExcel.SetCellValue(comp.StandardPrice, index, 5);
                        CurrentExcel.SetCellValue(comp.ProcurementType, index, 6);
                        CurrentExcel.SetCellValue(comp.PriceMass, index, 7);

                        index++;
                    }

                    if (CurrentExcel.SaveClose() != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("SEA_ExportXlsIssue"), XlsFileName), McgWpfTools.GetStringResource("SEA_TitleExportXlsIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    if (newExcelProcess != null)
                        newExcelProcess.Kill();

                    _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("SEA_BtBomExport"), String.Format(McgWpfTools.GetStringResource("SEA_ExportXls"), XlsFileName), McgWpfTools.GetStringResource("SBE_ToolTipOpen"), McgWpfTools.GetStringResource("SBE_ToolTipOpenFolder"), McgWpfTools.GetStringResource("SBE_ToolTipClose"), XlsFileName);
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private int RecursiveWriteStructureXls(ExcelToolsClosedXml CurrentExcel, List<BomComponent> Structure, int Index = 2)
        {
            try
            {
                foreach (var comp in Structure)
                {
                    CurrentExcel.SetCellValue($"{"".PadLeft((comp.Level) * 2)}{comp.Level}", Index, 2);

                    CurrentExcel.SetCellValue(comp.Level, Index, 1);
                    CurrentExcel.SetCellValue(comp.Number, Index, 2);
                    CurrentExcel.SetCellValue(comp.LineNumber, Index, 3);
                    CurrentExcel.SetCellValue(comp.Description, Index, 4);
                    CurrentExcel.SetCellValue(comp.Revision, Index, 5);
                    CurrentExcel.SetCellValue(comp.Quantity, Index, 6);
                    CurrentExcel.SetCellValue(comp.Unit.ToString(), Index, 7);
                    CurrentExcel.SetCellValue(comp.StandardPrice, Index, 8);
                    CurrentExcel.SetCellValue(comp.ProcurementType, Index, 9);
                    CurrentExcel.SetCellValue(comp.PriceMass, Index, 10);

                    Index++;
                    if (comp.Structure != null && comp.Structure.Count > 0)
                        Index = RecursiveWriteStructureXls(CurrentExcel, comp.Structure, Index);
                }
                return Index;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void ExpandRecursive(BomComponent item)
        {
            try
            {
                item.IsExpanded = true;

                if (item.Structure != null)
                {
                    foreach (var child in item.Structure)
                    {
                        ExpandRecursive(child);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void CollapseRecursive(BomComponent item)
        {
            try
            {
                item.IsExpanded = false;

                if (item.Structure != null)
                {
                    foreach (var child in item.Structure)
                    {
                        CollapseRecursive(child);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void UpdateSapCostVolumeInformation()
        {
            try
            {
                List<string> AllMaterial = CurrentDataContext.AllComponents.Where((item) => !string.IsNullOrWhiteSpace(item.Number)).Select(item => item.Number).Distinct().ToList();
                List<SapCostVolumeInfo> sapCostVolumeInfos = new List<SapCostVolumeInfo>();
                CurrentDataContext.SapSearchIndex = 0;
                int increment = 10;
                for (int i = 0; i <= AllMaterial.Count - 1; i += increment)
                {
                    var batch = AllMaterial.Skip(i).Take(increment).ToList();
                    var tmpSapCostVolumeInfos = _sapHupService.GetListMaterialMasterCostVolumeInfo(batch);
                    if (tmpSapCostVolumeInfos != null)
                        sapCostVolumeInfos.AddRange(tmpSapCostVolumeInfos.Select(x => new SapCostVolumeInfo(x)).ToList());
                    CurrentDataContext.SapSearchIndex = i;
                }

                //last 
                {
                    var batch = AllMaterial.Skip(CurrentDataContext.SapSearchIndex).Take(increment).ToList();
                    var tmpSapCostVolumeInfos = _sapHupService.GetListMaterialMasterCostVolumeInfo(batch);
                    if (tmpSapCostVolumeInfos != null)
                        sapCostVolumeInfos.AddRange(tmpSapCostVolumeInfos.Select(x => new SapCostVolumeInfo(x)).ToList());
                    CurrentDataContext.SapSearchIndex = AllMaterial.Count;
                }

                if (sapCostVolumeInfos != null)
                {
                    foreach (var comp in CurrentDataContext.FlatStructure)
                    {
                        SapCostVolumeInfo sapCostVolumeInfo = sapCostVolumeInfos.FirstOrDefault(item => item.MaterialMasterNumber == comp.Number && item.PlantNumber.Number == CurrentDataContext.Plant.Number);
                        if (sapCostVolumeInfo != null)
                        {
                            comp.Supplier = sapCostVolumeInfo.ProcurementType;
                            comp.StandardPrice = sapCostVolumeInfo.StdCost;
                            comp.PriceMass = sapCostVolumeInfo.StdCostPerKg;
                        }
                    }

                    foreach (var comp in CurrentDataContext.AllComponents)
                    {
                        SapCostVolumeInfo sapCostVolumeInfo = sapCostVolumeInfos.FirstOrDefault(item => item.MaterialMasterNumber == comp.Number && item.PlantNumber.Number == CurrentDataContext.Plant.Number);
                        if (sapCostVolumeInfo != null)
                        {
                            comp.Supplier = sapCostVolumeInfo.ProcurementType;
                            comp.StandardPrice = sapCostVolumeInfo.StdCost;
                            comp.PriceMass = sapCostVolumeInfo.StdCostPerKg;
                        }
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
