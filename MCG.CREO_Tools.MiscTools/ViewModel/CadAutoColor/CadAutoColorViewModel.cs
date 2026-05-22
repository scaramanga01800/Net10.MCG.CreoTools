using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.CadAutoColor;
using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColor;
using MCG.CREO_Tools.MiscTools.ViewModel.Configuration;
using pfcls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColr
{
    public class CadAutoColorViewModel : ObservableObject, ICadAutoColorViewModel
    {
        #region [REGION] Properties from Interface
        public CadAutoColorDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private string AppearanceFileName { get; set; }
        // private CREOConnection CurrentCREOConnection { get; set; } = McgMiscTools.GetPropertiesFromMainApp<CREOConnection>("CREOSESSION");
        private IpfcModel CurrentCadModel { get; set; }
        public bool IsMainClearAppearancesDone { get; set; } = false;
        private Dispatcher MainDispatcher { get; set; }
        private CadAutoColorConfiguration CurrentConfiguration { get; set; }
        private List<string> AllAsm { get; set; }
        private bool ChangeColorPaletteInProgress { get; set; } = false;
        #endregion

        #region [REGION] Commands
        public ICommand CommandReadAsm { get => new RelayCommand(() => ExecuteReadAsm()); }
        public ICommand CommandUpdateColor { get => new RelayCommand(() => ExecuteUpdateColor()); }
        public ICommand CommandStartExcelExport { get => new RelayCommand(() => ExecuteStartExcelExport()); }
        public ICommand CommandOpenCadDoc { get => new RelayCommand(() => ExecuteOpenCadDoc()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        public ICommand CommandUpdateColorPalette { get => new RelayCommand(() => ExecuteUpdateColorPalette()); }
        public ICommand CommandCheckUncheckAll { get => new RelayCommand<bool>((ischecked) => ExecuteCheckUncheckAll(ischecked)); }
        public ICommand CommandCheckUncheckAllName { get => new RelayCommand<bool>((ischecked) => ExecuteCheckUncheckAllName(ischecked)); }
        public ICommand CommandCheckUncheckAllPart { get => new RelayCommand<bool>((ischecked) => ExecuteCheckUncheckAllPart(ischecked)); }
        public ICommand CommandMultiAssignColor { get => new RelayCommand(() => ExecuteMultiAssignColor()); }
        public ICommand CommandRemoveColor { get => new RelayCommand(() => ExecuteRemoveColor()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        private readonly ICreoMacroService _creoMacroService;
        private readonly ICreoParameterService _creoParameterService;
        private readonly ICreoFeatureService _creoFeatureService;
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;

        public CadAutoColorViewModel(ICreoSessionProvider creoSessionProvider,
                                     ICreoModelService creoModelService,
                                     ICreoMacroService creoMacroService,
                                     ICreoParameterService creoParameterService,
                                     ICreoFeatureService creoFeatureService,
                                     IMcgCommonLibWindowService mcgCommonLibWindowService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;
                _creoMacroService = creoMacroService;
                _creoParameterService = creoParameterService;
                _creoFeatureService = creoFeatureService;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;

                MainAppFolder = Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                //AppearanceFileName = $"{MainAppFolder}\\{McgMiscTools.GetAppSetting(this, "ResourcesFolder")}\\{McgMiscTools.GetAppSetting(this, "AppearanceFileName")}";

                CurrentDataContext = new CadAutoColorDataContext();
                MainDispatcher = Dispatcher.CurrentDispatcher;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDataContext.IsCreoEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDataContext.IsCreoEnable = e;

                List<McgAppearanceItem> ListAppearances;// = McgMiscTools.GetListAppearancesFromFile(AppearanceFileName);

                AppearanceFileName = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.AppearanceFileName01}";
                ListAppearances = McgBusinessTools.GetListAppearancesFromFile(AppearanceFileName);
                if (ListAppearances != null && ListAppearances.Count > 0)
                {
                    CurrentDataContext.ColorPalette01 = new CadAutoColorPalette() { IsSelected = true, Name = "Default", ColorPaletteFile = AppearanceFileName };
                    foreach (var item in ListAppearances.OrderBy(color => color.Name))
                        CurrentDataContext.ColorPalette01.ListColor.Add(CadAutoColorCreoColor.GetCadAutoColorCreoColor(item));
                }
                if (ListAppearances != null && ListAppearances.Count > 0)
                    foreach (var item in ListAppearances.OrderBy(color => color.Name))
                        CurrentDataContext.ListCreoColor.Add(CadAutoColorCreoColor.GetCadAutoColorCreoColor(item));


                AppearanceFileName = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.AppearanceFileName02}";
                ListAppearances = McgBusinessTools.GetListAppearancesFromFile(AppearanceFileName);
                if (ListAppearances != null && ListAppearances.Count > 0)
                {
                    CurrentDataContext.ColorPalette02 = new CadAutoColorPalette() { IsSelected = false, Name = "CREO Tools", ColorPaletteFile = AppearanceFileName };
                    foreach (var item in ListAppearances.OrderBy(color => color.Name))
                        CurrentDataContext.ColorPalette02.ListColor.Add(CadAutoColorCreoColor.GetCadAutoColorCreoColor(item));
                }

                AppearanceFileName = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.AppearanceFileName03}";
                ListAppearances = McgBusinessTools.GetListAppearancesFromFile(AppearanceFileName);
                if (ListAppearances != null && ListAppearances.Count > 0)
                {
                    CurrentDataContext.ColorPalette03 = new CadAutoColorPalette() { IsSelected = false, Name = "Marketing", ColorPaletteFile = AppearanceFileName };
                    foreach (var item in ListAppearances.OrderBy(color => color.Name))
                        CurrentDataContext.ColorPalette03.ListColor.Add(CadAutoColorCreoColor.GetCadAutoColorCreoColor(item));
                }

                AppearanceFileName = CurrentDataContext.ColorPalette01.ColorPaletteFile;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }

        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteReadAsm()
        {
            try
            {
                CurrentDataContext.IsPleaseWaitShown = true;
                CurrentDataContext.IsAllPartSelected = false;
                CurrentDataContext.IsAllPartSelectedName = false;
                CurrentDataContext.IsAllPartSelectedPart = false;
                CurrentDataContext.ListItem.Clear();
                CurrentDataContext.ListItemName.Clear();
                CurrentDataContext.ListItemPart.Clear();
                CurrentDataContext.AllCadModels = new List<IpfcModel>();
                CurrentDataContext.NbModels = 0;
                CurrentDataContext.NbModelsInProgress = 0;
                IsMainClearAppearancesDone = false;

                Thread ListModelThread = new Thread(new ThreadStart(GetActiveModelDependenciesAsynch));
                ListModelThread.IsBackground = true;
                ListModelThread.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateColor()
        {
            try
            {
                if (CurrentCadModel != null)
                {
                    if (CurrentDataContext.CurrentList.Count(item => item.IsSelected) > 0)
                    {
                        CurrentDataContext.IsPleaseWaitShown = true;
                        Thread ListModelThread = new Thread(new ThreadStart(StartColorizationAsynch));
                        ListModelThread.IsBackground = true;
                        ListModelThread.Start();
                    }
                    else
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("CAC_MsgNoPartSelected"),
                                        McgWpfTools.GetStringResource("CAC_MsgTitleCadAutoColorIssue"),
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information,
                                        MessageBoxResult.OK);
                    }
                }
                else
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("CAC_MsgSearchAsmNotStarted"),
                                    McgWpfTools.GetStringResource("CAC_MsgTitleCadAutoColorIssue"),
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information,
                                    MessageBoxResult.OK);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartExcelExport()
        {
            try
            {
                ExportCumulativeBomToExcel();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenCadDoc()
        {
            try
            {
                if (CurrentDataContext.SelectedCadDoc != null)
                {
                    EPMDocument CurrentEpm = new EPMDocument()
                    {
                        PartNumber = CurrentDataContext.SelectedCadDoc,
                        FileName = CurrentDataContext.SelectedCadDoc
                    };
                    CurrentEpm.OpenInCreo(_creoSessionProvider, _creoModelService);
                }
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
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("CAC_UserGuide"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateColorPalette()
        {
            try
            {
                if (!ChangeColorPaletteInProgress)
                {
                    ChangeColorPaletteInProgress = true;
                    if (MessageBox.Show(McgWpfTools.GetStringResource("CAC_TextChangeColorPalette"), McgWpfTools.GetStringResource("CAC_TitleChangeColorPalette"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        CadAutoColorPalette CurrentPalette;
                        ObservableCollection<CadAutoColorCreoColor> TempPalette;
                        if (CurrentDataContext.ColorPalette01.IsSelected)
                        {
                            CurrentPalette = CurrentDataContext.ColorPalette01;
                            TempPalette = CurrentDataContext.ColorPalette01.ListColor;
                        }
                        else if (CurrentDataContext.ColorPalette02.IsSelected)
                        {
                            CurrentPalette = CurrentDataContext.ColorPalette02;
                            TempPalette = CurrentDataContext.ColorPalette02.ListColor;
                        }
                        else if (CurrentDataContext.ColorPalette03.IsSelected)
                        {
                            CurrentPalette = CurrentDataContext.ColorPalette03;
                            TempPalette = CurrentDataContext.ColorPalette03.ListColor;
                        }
                        else
                        {
                            CurrentPalette = CurrentDataContext.ColorPalette01;
                            TempPalette = CurrentDataContext.ColorPalette01.ListColor;
                        }

                        CurrentDataContext.ListCreoColor.Clear();
                        AppearanceFileName = CurrentPalette.ColorPaletteFile;
                        foreach (CadAutoColorCreoColor color in TempPalette)
                        {
                            CurrentDataContext.ListCreoColor.Add(color);
                        }
                        UpdateColorItem();
                        ChangeColorPaletteInProgress = false;
                    }
                    else
                    {
                        bool TempIsSelected = CurrentDataContext.ColorPalette02.IsSelected;
                        CurrentDataContext.ColorPalette01.IsSelected = !CurrentDataContext.ColorPalette01.IsSelected;
                        if (TempIsSelected == CurrentDataContext.ColorPalette02.IsSelected)
                            CurrentDataContext.ColorPalette02.IsSelected = !CurrentDataContext.ColorPalette02.IsSelected;
                        ChangeColorPaletteInProgress = false;
                    }
                }

            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckUncheckAll(bool IsChecked)
        {
            try
            {
                if (CurrentDataContext.ListItem != null)
                {
                    foreach (var item in CurrentDataContext.ListItem)
                    {
                        item.IsSelected = IsChecked;
                    }
                    CurrentDataContext.IsAllPartSelected = IsChecked;
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckUncheckAllPart(bool IsChecked)
        {
            try
            {
                if (CurrentDataContext.ListItemPart != null)
                {
                    foreach (var item in CurrentDataContext.ListItemPart)
                    {
                        item.IsSelected = IsChecked;
                    }
                    CurrentDataContext.IsAllPartSelectedPart = IsChecked;
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckUncheckAllName(bool IsChecked)
        {
            try
            {
                if (CurrentDataContext.ListItemName != null)
                {
                    foreach (var item in CurrentDataContext.ListItemName)
                    {
                        item.IsSelected = IsChecked;
                    }
                    CurrentDataContext.IsAllPartSelectedName = IsChecked;
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemoveColor()
        {
            try
            {
                if (MessageBox.Show(McgWpfTools.GetStringResource("CAC_WindowTextRemoveColor"), McgWpfTools.GetStringResource("CAC_WindowTitleRemoveColor"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _creoMacroService.ClearAppearances();
                    IsMainClearAppearancesDone = true;
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMultiAssignColor()
        {
            try
            {
                if (CurrentDataContext.CurrentList != null)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("CAC_WindowTextAssignColor"), McgWpfTools.GetStringResource("CAC_WindowTitleAssignColor"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        foreach (var item in CurrentDataContext.CurrentList.Where(part => part.IsSelected))
                        {
                            item.IsColorAssigned = true;
                            item.SelectedCreoColor = CurrentDataContext.SelectedCreoColor;
                        }
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        private void GetActiveModelDependenciesAsynch()
        {
            try
            {

                AllAsm = new List<string>();

                foreach (var item in CurrentDataContext.ListCreoColor)
                    item.IsAlreadyAssigned = false;

                // Check if Active Model available (3D)
                IpfcModel ActiveModel = _creoModelService.GetActiveModel();

                // if Active model not found, check if currentWindow is a drawxing
                if (ActiveModel == null)
                {
                    var CurrentMWindow = _creoSessionProvider.Session.get_CurrentWindow();
                    if (CurrentMWindow != null)
                        ActiveModel = CurrentMWindow.Model;
                }

                if (ActiveModel == null)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("CAC_MsgNotCadDocActivated"),
                                    McgWpfTools.GetStringResource("CAC_MsgTitleCadAutoColorIssue"),
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information,
                                    MessageBoxResult.OK);
                }
                else if (ActiveModel.Type != 0)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("CAC_MsgNotAnAssembly"),
                                    McgWpfTools.GetStringResource("CAC_MsgTitleCadAutoColorIssue"),
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information,
                                    MessageBoxResult.OK);
                    ActiveModel = null;
                }

                if (ActiveModel != null)
                {
                    GetAllDependenciesRecursive(ActiveModel);
                    UpdateColorItem();
                }
                CurrentCadModel = ActiveModel;
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

        private void GetAllDependenciesRecursive(IpfcModel CurrentModel)
        {
            try
            {
                if (CurrentModel == null) return;

                List<string> allCadFileName = new List<string>();
                Dictionary<string, int> instanceNameCountMap = new Dictionary<string, int>();
                IpfcSolid solid = CurrentModel as IpfcSolid;

                if (solid != null)
                {
                    IpfcFeatures features = solid.ListFeaturesByType(false, EpfcFeatureType.EpfcFEATTYPE_COMPONENT);
                    foreach (IpfcFeature feat in features)
                    {
                        IpfcComponentFeat compFeat = feat as IpfcComponentFeat;
                        if (compFeat == null) continue;

                        string instanceName = compFeat.ModelDescr.GetFileName().ToLower();

                        if (!instanceNameCountMap.ContainsKey(instanceName))
                            instanceNameCountMap[instanceName] = 0;

                        instanceNameCountMap[instanceName]++;
                    }
                    CurrentDataContext.NbModels += instanceNameCountMap.Count;

                    IpfcModel TempModel = null;
                    foreach (var comp in instanceNameCountMap)
                    {
                        TempModel = _creoModelService.RetrieveModelFromStdDir(comp.Key);
                        if (TempModel != null)
                        {
                            // Assembly
                            if (TempModel.Type == (int)EpfcModelType.EpfcMDL_ASSEMBLY)
                            {
                                if (!AllAsm.Any(asm => asm == comp.Key))
                                {
                                    AllAsm.Add(comp.Key);
                                    GetAllDependenciesRecursive(TempModel);
                                }
                            }
                            if (TempModel.Type == (int)EpfcModelType.EpfcMDL_PART)
                                if (!allCadFileName.Any((cad) => cad == comp.Key))
                                {
                                    allCadFileName.Add(comp.Key);
                                    CurrentDataContext.AllCadModels.Add(TempModel);
                                    MainDispatcher.Invoke(new Action(() => AddCadModelInformation(TempModel)));
                                }
                        }
                        CurrentDataContext.NbModelsInProgress++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private CadAutoColorItem AddCadModelInformation(IpfcModel ActiveModel)
        {
            try
            {
                CadAutoColorItem CurrentItem = null;
                string CurrentMaterialValue = _creoParameterService.GetParameterAsString(ActiveModel, "MATERIAL");
                string CurrentAssignedMaterial;
                try
                {
                    CurrentAssignedMaterial = _creoFeatureService.GetCurrentMaterial(ActiveModel)?.Condition;
                }
                catch
                {
                    CurrentAssignedMaterial = "Unknown";
                }

                if (CurrentMaterialValue == null || CurrentMaterialValue.Trim() == "")
                    CurrentMaterialValue = "Unknown";

                // List MATERIAL
                CurrentItem = CurrentDataContext.ListItem.FirstOrDefault(item => item.Material == CurrentAssignedMaterial);
                if (CurrentItem == null)
                {
                    CurrentItem = new CadAutoColorItem()
                    {
                        Material = CurrentAssignedMaterial,
                        AsssignedMaterial = CurrentAssignedMaterial,
                        CadModels = new List<IpfcModel>(),
                        //SelectedCreoColor = GetNextAvailableColorRdm()
                    };
                    CurrentDataContext.ListItem.Add(CurrentItem);
                }
                CurrentItem.CadModels.Add(ActiveModel);
                CurrentItem.ListCadDoc.Add(ActiveModel.FileName);

                // List PTC_COMMON_NAME
                CurrentMaterialValue = _creoParameterService.GetParameterAsString(ActiveModel, "PTC_COMMON_NAME");
                if (CurrentMaterialValue == null || CurrentMaterialValue.Trim() == "")
                    CurrentMaterialValue = "Unknown";
                CurrentItem = CurrentDataContext.ListItemName.FirstOrDefault(item => item.Ptc_Common_Name == CurrentMaterialValue);
                if (CurrentItem == null)
                {
                    CurrentItem = new CadAutoColorItem()
                    {
                        Ptc_Common_Name = CurrentMaterialValue,
                        CadModels = new List<IpfcModel>(),
                        //SelectedCreoColor = GetNextAvailableColorRdm()
                    };
                    CurrentDataContext.ListItemName.Add(CurrentItem);
                }
                CurrentItem.CadModels.Add(ActiveModel);
                CurrentItem.ListCadDoc.Add(ActiveModel.FileName);

                // List Part
                string CurrentPtcCommonName;
                string CurrentDescr2;
                CurrentPtcCommonName = _creoParameterService.GetParameterAsString(ActiveModel, "PTC_COMMON_NAME");
                if (CurrentPtcCommonName == null || CurrentPtcCommonName.Trim() == "")
                    CurrentPtcCommonName = "Unknown";
                CurrentDescr2 = _creoParameterService.GetParameterAsString(ActiveModel, "DESCRIPTION_2");
                if (CurrentDescr2 == null || CurrentPtcCommonName.Trim() == "")
                    CurrentDescr2 = "Unknown";
                CurrentItem = CurrentDataContext.ListItemPart.FirstOrDefault(item => item.Number == CurrentMaterialValue);
                if (CurrentItem == null)
                {
                    CurrentItem = new CadAutoColorItem()
                    {
                        Ptc_Common_Name = $"{CurrentPtcCommonName} {CurrentDescr2}",
                        Number = ActiveModel.FileName,
                        CadModels = new List<IpfcModel>(),
                        //SelectedCreoColor = GetNextAvailableColorRdm()
                    };
                    CurrentDataContext.ListItemPart.Add(CurrentItem);
                }

                CurrentItem.CadModels.Add(ActiveModel);
                CurrentItem.ListCadDoc.Add(ActiveModel.FileName);

                return CurrentItem;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private CadAutoColorCreoColor GetNextAvailableColorRdm()
        {
            int intrandom;
            try
            {
                CadAutoColorCreoColor CurrentColor = null;
                var AvailableList = CurrentDataContext.ListCreoColor.Where(item => !item.IsAlreadyAssigned).ToList();
                if (AvailableList.Count > 0)
                {
                    Random random = new Random();
                    intrandom = random.Next(AvailableList.Count);
                    CurrentColor = AvailableList[intrandom];
                    CurrentColor.IsAlreadyAssigned = true;
                }
                else
                    CurrentColor = CurrentDataContext.ListCreoColor.FirstOrDefault();

                return CurrentColor;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void StartColorizationAsynch()
        {
            try
            {
                bool IsDisplayedWindow = false;
                bool IsMainDisplayedWindow = _creoModelService.ActiveCadDocWindow(CurrentCadModel);
                bool IsClearOk = true;

                CurrentDataContext.NbModels = CurrentDataContext.AllCadModels.Count;
                CurrentDataContext.NbModelsInProgress = 0;

                // Upload Appearance File for the CREO session
                _creoMacroService.LoadAppearanceFile(AppearanceFileName);

                if (IsClearOk)
                {
                    foreach (var item in CurrentDataContext.CurrentList.Where(colorItem => !colorItem.IsColorAssigned && colorItem.IsSelected))
                    {
                        foreach (var cad in item.CadModels)
                        {
                            if (!_creoFeatureService.IsBulkItem(cad))
                            {
                                IsDisplayedWindow = _creoModelService.ActiveCadDocWindow(cad);
                                _creoMacroService.ClearAppearances();
                                _creoMacroService.AssignedColorPrt(item.SelectedCreoColor.ColorName);
                                if (!IsDisplayedWindow)
                                {
                                    IpfcWindow CurrentWindow = _creoModelService.GetCadDocWindow(cad);
                                    CurrentWindow.Close();
                                }
                            }
                            CurrentDataContext.NbModelsInProgress++;
                        }
                        item.IsColorAssigned = true;
                    }
                }
                IsMainDisplayedWindow = _creoModelService.ActiveCadDocWindow(CurrentCadModel);
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

        private void ExportCumulativeBomToExcel()
        {
            try
            {
                if (CurrentCadModel != null && CurrentDataContext.ListItem != null && CurrentDataContext.ListItem.Count > 0)
                {
                    Regex RegexProc = new Regex("Excel", RegexOptions.IgnoreCase);
                    List<Process> OldExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    string UserDocumentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string XlsFileName = $"{UserDocumentFolder}\\ASM_PARTS_{CurrentCadModel.FileName.Trim('.').FirstOrDefault()}.xlsx";

                    ExcelToolsClosedXml CurrentExcel = new ExcelToolsClosedXml() { CompleteFileName = XlsFileName };
                    if (CurrentExcel.CreateNewFile("MAIN") != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("CAC_ExportXlsIssue"), XlsFileName), McgWpfTools.GetStringResource("CAC_TitleExportXlsIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    List<Process> NewExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    Process newExcelProcess = NewExcelCurrentProcess.FirstOrDefault((proc) => !OldExcelCurrentProcess.ToList().Exists((oldprc) => proc.Id == oldprc.Id));

                    // Update Columns
                    CurrentExcel.CurrentSheet = "MAIN";

                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("CAC_ColHeader01"), 1, 1);

                    // Update Structure
                    int index = 2;
                    int index2 = 2;

                    Regex NonAlphaCaracRegex = new Regex(@"[^a-zA-Z0-9_\- ]", RegexOptions.IgnoreCase);
                    string CumulativeBomFromValue;
                    foreach (var comp in CurrentDataContext.ListItem)
                    {
                        CurrentExcel.CurrentSheet = "MAIN";
                        if (comp.Material == null || comp.Material == "")
                            comp.Material = "Unknown";

                        CumulativeBomFromValue = NonAlphaCaracRegex.Replace(comp.Material, @"");

                        CurrentExcel.SetCellValue(CumulativeBomFromValue, index, 1);

                        // Update one Sheet for list of items
                        index2 = 2;
                        CurrentExcel.CreateSheet(CumulativeBomFromValue);
                        CurrentExcel.CurrentSheet = CumulativeBomFromValue;
                        CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("CAC_ColHeader02"), 1, 1);
                        foreach (var item in comp.ListCadDoc)
                        {
                            CurrentExcel.SetCellValue(item, index2, 1);
                            index2++;
                        }

                        index++;
                    }

                    if (CurrentExcel.SaveClose() != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("CAC_ExportXlsIssue"), XlsFileName), McgWpfTools.GetStringResource("CAC_TitleExportXlsIssue"), MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    if (newExcelProcess != null)
                        newExcelProcess.Kill();

                    _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("CAC_BtBomExport"), String.Format(McgWpfTools.GetStringResource("CAC_ExportXls"), XlsFileName), McgWpfTools.GetStringResource("CAC_ToolTipOpen"), McgWpfTools.GetStringResource("CAC_ToolTipOpenFolder"), McgWpfTools.GetStringResource("CAC_ToolTipClose"), XlsFileName);
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void UpdateColorItem()
        {
            try
            {
                foreach (var item in CurrentDataContext.ListCreoColor)
                    item.IsAlreadyAssigned = false;
                foreach (var item in CurrentDataContext.ListItem)
                    item.SelectedCreoColor = GetNextAvailableColorRdm();

                foreach (var item in CurrentDataContext.ListCreoColor)
                    item.IsAlreadyAssigned = false;
                foreach (var item in CurrentDataContext.ListItemName)
                    item.SelectedCreoColor = GetNextAvailableColorRdm();

                foreach (var item in CurrentDataContext.ListCreoColor)
                    item.IsAlreadyAssigned = false;
                foreach (var item in CurrentDataContext.ListItemPart)
                    item.SelectedCreoColor = GetNextAvailableColorRdm();
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
