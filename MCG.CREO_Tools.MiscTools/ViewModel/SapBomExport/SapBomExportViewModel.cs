using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.CREO_Tools.MiscTools.View.SapBomExport;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.WindchillRequestTool.Model.BomComparison;
using MCG.CommonLib.Models.Excel;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.SapTools.Interfaces;

namespace MCG.CREO_Tools.MiscTools.ViewModel.SapBomExport
{
    public class SapBomExportViewModel : ObservableObject, ISapBomExportViewModel
    {
        #region [REGION] Properties from Interface
        private SapBomExportDataContext _CurrentDataContext;
        public SapBomExportDataContext CurrentDataContext
        {
            get { return _CurrentDataContext; }
            set
            {
                if (this._CurrentDataContext != value)
                {
                    this._CurrentDataContext = value;
                    OnPropertyChanged();
                }

            }
        }
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
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        private readonly ISapBomService _sapBomService;
        public SapBomExportViewModel(IXmlSerializeTools xmlSerializeTools,
                                     IMcgCommonLibWindowService mcgCommonLibWindowService,
                                     ISapBomService sapBomService)
        {
            try
            {
                _xmlSerializeTools = xmlSerializeTools;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;
                _sapBomService = sapBomService;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                MainDispatcher = Dispatcher.CurrentDispatcher;

                CurrentDataContext = new SapBomExportDataContext();

                CurrentConfiguration = _xmlSerializeTools.GetDeserializedXml<SapConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.ConfigurationSapBomExport}");
                if (CurrentConfiguration != null && CurrentConfiguration.ListSapPlant != null && CurrentConfiguration.ListBomApplication != null)
                {
                    foreach (var item in CurrentConfiguration.ListSapPlant)
                        CurrentDataContext.AllSapPlants.Add(item);
                    foreach (var item in CurrentConfiguration.ListBomApplication)
                        CurrentDataContext.AllBomApplication.Add(item);
                    CurrentDataContext.Plant = CurrentDataContext.AllSapPlants.FirstOrDefault();
                    CurrentDataContext.BomApplication = CurrentDataContext.AllBomApplication.FirstOrDefault(item => item.Name == "PP01");
                }

            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }

            _mcgCommonLibWindowService = mcgCommonLibWindowService;
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteStartSapBomExport()
        {
            try
            {
                CurrentDataContext.MainStructure.Clear();

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
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("SBE_UserGuide"));
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

                string tempFolderPath = Path.GetTempPath();
                string tempFileName = "export_bom_sap.txt";
                string tempFullFileName = $"{tempFolderPath}\\{tempFileName}";

                if (File.Exists(tempFullFileName))
                    File.Delete(tempFullFileName);

                if (!string.IsNullOrEmpty(CurrentDataContext.PartNumber))
                {
                    SAPMaterialMaster currentSapMat = new SAPMaterialMaster()
                    {
                        PartNumber = CurrentDataContext.PartNumber?.Trim(),
                        PlantNumber = CurrentDataContext.Plant.Number,
                        Alternative = CurrentDataContext.AlternativeBom,
                        Application = CurrentDataContext.BomApplication.Name,
                        DateValidity = CurrentDataContext.DateValidity.ToString("dd.MM.yyyy"),
                        DatePriceValidity = CurrentDataContext.DateValidityCost.ToString("dd.MM.yyyy"),
                        EcoNumber = CurrentDataContext.EcoNumber?.Trim(),
                        Revision = CurrentDataContext.Revision,
                        Is_CB_RLT_Selected = CurrentDataContext.Is_CB_RLT_Selected,
                        Is_CB_PUR_Selected = CurrentDataContext.Is_CB_PUR_Selected
                    };

                    if (CurrentDataContext.Is_RB_ALL_Selected)
                        currentSapMat.BomExportOption = SAPBomExportOption.RB_ALL;
                    else if (CurrentDataContext.Is_RB_MRT_Selected)
                        currentSapMat.BomExportOption = SAPBomExportOption.RB_MRT;
                    else if (CurrentDataContext.Is_RB_RT_Selected)
                        currentSapMat.BomExportOption = SAPBomExportOption.RB_RT;
                    else if (CurrentDataContext.Is_RB_SC_Selected)
                        currentSapMat.BomExportOption = SAPBomExportOption.RB_SC;

                    if (currentSapMat.PlantNumber == "0000")
                        currentSapMat.PlantNumber = "";
                    if (currentSapMat.Revision == "BLANK")
                        currentSapMat.Revision = "";

                    var result = _sapBomService.ZDTB_CS12(currentSapMat, tempFolderPath, tempFileName);
                    if (result == SAPBomMsg.NOSAPCONNECTION)
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("SBE_InfoMsgErpConNotFound"), McgWpfTools.GetStringResource("SBE_WindowInfoTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        CurrentDataContext.IsPleaseWaitShown = false;
                        return;
                    }

                    if (File.Exists(tempFullFileName))
                    {
                        string tempDesc = "";
                        List<BomComponent> FlatBomStructure = _sapBomService.ExtractSapBomCs12($"{tempFolderPath}\\{tempFileName}", ref tempDesc);
                        CurrentDataContext.MainDescription = tempDesc;
                        var BomStructure = RestructureBomComponents(FlatBomStructure);

                        if (BomStructure != null)
                        {
                            UpdateColumnWidth(BomStructure);
                            MainDispatcher.Invoke(() => UpdateMainBom(BomStructure));
                        }
                        else
                            CurrentDataContext.IsPleaseWaitShown = false;
                    }
                    else
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("SBE_InfoMsgNoBom"), McgWpfTools.GetStringResource("SBE_WindowInfoTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        CurrentDataContext.IsPleaseWaitShown = false;
                    }
                }
                else
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("SBE_InfoMsgNoPart"), McgWpfTools.GetStringResource("SBE_WindowInfoTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    CurrentDataContext.IsPleaseWaitShown = false;
                }
            }
            catch (Exception ex)
            {
                CurrentDataContext.IsPleaseWaitShown = false;

                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
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
            finally
            {
                CurrentDataContext.IsPleaseWaitShown = false;
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
                ListParams.Add("AssemblyIndicator");
                ListParams.Add("PhamtomItem");
                ListParams.Add("ParentNumber");
                ListParams.Add("LineNumber");
                ListParams.Add("ItemCategory");
                ListParams.Add("MaterialType");
                ListParams.Add("Number");
                ListParams.Add("Description");
                ListParams.Add("Quantity");
                ListParams.Add("Unit");
                ListParams.Add("ProcurementType");
                ListParams.Add("SpecialProcurement");
                ListParams.Add("MrpController");
                ListParams.Add("PreferredSupplier");
                ListParams.Add("Supplier");
                ListParams.Add("PurchasingInfoRec");
                ListParams.Add("MaterialGroup");
                ListParams.Add("MaterialGroupDescription");
                ListParams.Add("StandardPrice");
                ListParams.Add("TotalItemStdPrice");
                ListParams.Add("PurchasingPrice");
                ListParams.Add("Currency");
                ListParams.Add("PriceUnit");
                ListParams.Add("QualInspGrp");
                ListParams.Add("QualityValidation");
                ListParams.Add("SerialNoProfil");
                ListParams.Add("Agreement");

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

        private void UpdateColumnWidth()
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
                ListParams.Add("AssemblyIndicator");
                ListParams.Add("PhamtomItem");
                ListParams.Add("ParentNumber");
                ListParams.Add("LineNumber");
                ListParams.Add("ItemCategory");
                ListParams.Add("MaterialType");
                ListParams.Add("Number");
                ListParams.Add("Description");
                ListParams.Add("Quantity");
                ListParams.Add("Unit");
                ListParams.Add("ProcurementType");
                ListParams.Add("SpecialProcurement");
                ListParams.Add("MrpController");
                ListParams.Add("PreferredSupplier");
                ListParams.Add("Supplier");
                ListParams.Add("PurchasingInfoRec");
                ListParams.Add("MaterialGroup");
                ListParams.Add("MaterialGroupDescription");
                ListParams.Add("StandardPrice");
                ListParams.Add("TotalItemStdPrice");
                ListParams.Add("PurchasingPrice");
                ListParams.Add("Currency");
                ListParams.Add("PriceUnit");
                ListParams.Add("QualInspGrp");
                ListParams.Add("QualityValidation");
                ListParams.Add("SerialNoProfil");
                ListParams.Add("Agreement");

                int currentMaxCarac = 0;
                int indexParam = 0;
                foreach (var param in ListParams)
                {
                    currentMaxCarac = MaxCharacterColValue(CurrentDataContext.MainStructure.ToList(), param, 5);

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


                    string templateFileName = $@"{MainAppFolder}\{CommonLibConstants.ResourcesFolder}\{MiscToolsConstants.TemplateSapBomExport}".Replace("\\\\", "\\");
                    ExcelToolsClosedXml CurrentExcel = new ExcelToolsClosedXml() { CompleteFileName = XlsFileName, CompleteTemplateFileName = templateFileName };
                    if (CurrentExcel.OpenFile(templateFileName) != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("SBE_ExportXlsIssue"), XlsFileName), McgWpfTools.GetStringResource("SBE_TitleExportXlsIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    List<Process> NewExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    Process newExcelProcess = NewExcelCurrentProcess.FirstOrDefault((proc) => !OldExcelCurrentProcess.ToList().Exists((oldprc) => proc.Id == oldprc.Id));

                    // Update Main information
                    CurrentExcel.CurrentSheet = MiscToolsConstants.TemplateSapBomExportMainTab;
                    CurrentExcel.SetCellValue(CurrentDataContext.PartNumber, 1, 2);
                    CurrentExcel.SetCellValue(CurrentDataContext.Plant?.Number, 2, 2);
                    CurrentExcel.SetCellValue(CurrentDataContext.MainDescription, 3, 2);

                    int index = MiscToolsConstants.TemplateSapBomExportFirstCompIndex;

                    // Update Structure
                    List<BomComponent> TempBom = new List<BomComponent>();
                    TempBom.AddRange(CurrentDataContext.MainStructure);

                    RecursiveWriteStructureXls(CurrentExcel, TempBom, index);

                    if (CurrentExcel.SaveClose() != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("SBE_ExportXlsIssue"), XlsFileName), McgWpfTools.GetStringResource("SBE_TitleExportXlsIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    if (newExcelProcess != null)
                        newExcelProcess.Kill();

                    _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("SBE_BtBomExport"), String.Format(McgWpfTools.GetStringResource("SBE_ExportXls"), XlsFileName), McgWpfTools.GetStringResource("SBE_ToolTipOpen"), McgWpfTools.GetStringResource("SBE_ToolTipOpenFolder"), McgWpfTools.GetStringResource("SBE_ToolTipClose"), XlsFileName);
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
                    //if (CurrentBomExportWindowDataContext.IsLevelIndented)
                    CurrentExcel.SetCellValue($"{"".PadLeft((comp.Level) * 2)}{comp.Level}", Index, 2);
                    //else
                    //    CurrentExcel.SetCellValue(comp.BomLevel, Index, 1);

                    CurrentExcel.SetCellValue(comp.AssemblyIndicator, Index, 3);
                    CurrentExcel.SetCellValue(comp.PhamtomItem, Index, 4);
                    CurrentExcel.SetCellValue(comp.ParentNumber, Index, 5);
                    CurrentExcel.SetCellValue(comp.LineNumber, Index, 6);
                    CurrentExcel.SetCellValue(comp.ItemCategory, Index, 7);
                    CurrentExcel.SetCellValue(comp.MaterialType, Index, 8);
                    CurrentExcel.SetCellValue(comp.Number, Index, 9);
                    CurrentExcel.SetCellValue(comp.Description, Index, 10);
                    CurrentExcel.SetCellValue(comp.Quantity, Index, 11);
                    CurrentExcel.SetCellValue(comp.Unit.ToString(), Index, 12);
                    CurrentExcel.SetCellValue(comp.ProcurementType, Index, 13);
                    CurrentExcel.SetCellValue(comp.SpecialProcurement, Index, 14);
                    CurrentExcel.SetCellValue(comp.MrpController, Index, 15);
                    CurrentExcel.SetCellValue(comp.PreferredSupplier, Index, 16);
                    CurrentExcel.SetCellValue(comp.Supplier, Index, 17);
                    CurrentExcel.SetCellValue(comp.PurchasingInfoRec, Index, 18);
                    CurrentExcel.SetCellValue(comp.MaterialGroup, Index, 19);
                    CurrentExcel.SetCellValue(comp.MaterialGroupDescription, Index, 20);
                    CurrentExcel.SetCellValue(comp.StandardPrice, Index, 21);
                    CurrentExcel.SetCellValue(comp.TotalItemStdPrice, Index, 22);
                    CurrentExcel.SetCellValue(comp.PurchasingPrice, Index, 23);
                    CurrentExcel.SetCellValue(comp.Currency, Index, 24);
                    CurrentExcel.SetCellValue(comp.PriceUnit, Index, 25);
                    CurrentExcel.SetCellValue(comp.QualInspGrp, Index, 26);
                    CurrentExcel.SetCellValue(comp.QualityValidation, Index, 27);
                    CurrentExcel.SetCellValue(comp.SerialNoProfil, Index, 28);
                    CurrentExcel.SetCellValue(comp.Agreement, Index, 29);

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

        private List<BomComponent> RestructureBomComponents(List<BomComponent> flatList)
        {
            try
            {
                List<BomComponent> rootElements = new List<BomComponent>();
                Stack<BomComponent> stack = new Stack<BomComponent>();

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

                return rootElements;
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
        #endregion
    }
}
