using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.QuickChange;
using pfcls;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.MiscTools.ViewModel.QuickChange
{
    public class QuickChangeViewModel : ObservableObject, IQuickChangeViewModel
    {
        #region [REGION] Properties from Interface
        public QuickChangeDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private Dispatcher MainDispatcher { get; set; }
        private List<string> AllAsm { get; set; }
        private IpfcModel CurrentCadModel { get; set; }
        #endregion

        #region [REGION] Commands
        public ICommand CommandReadAsm { get => new RelayCommand(() => ExecuteReadAsm()); }
        public ICommand CommandStartExcelExport { get => new RelayCommand(() => ExecuteStartExcelExport()); }
        public ICommand CommandReplaceComponent { get => new RelayCommand(() => ExecuteReplaceComponent()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        private readonly ICreoParameterService _creoParameterService;
        private readonly ICreoFeatureService _creoFeatureService;
        public QuickChangeViewModel(ICreoSessionProvider creoSessionProvider,
                                    ICreoModelService creoModelService,
                                    IMcgCommonLibWindowService mcgCommonLibWindowService,
                                    ICreoParameterService creoParameterService,
                                    ICreoFeatureService creoFeatureService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;
                _creoParameterService = creoParameterService;
                _creoFeatureService = creoFeatureService;

                CurrentDataContext = new QuickChangeDataContext();
                MainDispatcher = Dispatcher.CurrentDispatcher;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDataContext.IsCreoEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDataContext.IsCreoEnable = e;
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
                CurrentDataContext.NbModels = 0;
                CurrentDataContext.NbModelsInProgress = 0;

                CurrentDataContext.ListItem.Clear();
                CurrentDataContext.AllCadModels = new List<IpfcModel>();

                Thread ListModelThread = new Thread(new ThreadStart(GetActiveModelDependenciesAsynch));
                ListModelThread.IsBackground = true;
                ListModelThread.Start();
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

        private void ExecuteReplaceComponent()
        {
            try
            {
                ExportReplaceComponentAsynch();
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
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("QCH_UserGuide"));
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

                // Check if Active Model available (3D)
                IpfcModel activeModel = _creoModelService.GetActiveModel() ?? _creoSessionProvider.Session.get_CurrentWindow()?.Model;

                if (activeModel == null)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("CAC_MsgNotCadDocActivated"),
                                    McgWpfTools.GetStringResource("CAC_MsgTitleCadAutoColorIssue"),
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information,
                                    MessageBoxResult.OK);
                }
                else if (activeModel.Type != (int)EpfcModelType.EpfcMDL_ASSEMBLY)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("CAC_MsgNotAnAssembly"),
                                    McgWpfTools.GetStringResource("CAC_MsgTitleCadAutoColorIssue"),
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information,
                                    MessageBoxResult.OK);
                    activeModel = null;
                }

                if (activeModel != null)
                {
                    GetAllDependenciesRecursive(activeModel, 1);
                }
                CurrentCadModel = activeModel;
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

        private void GetAllDependenciesRecursive(IpfcModel currentModel, int level)
        {
            try
            {
                if (currentModel == null) return;

                Dictionary<string, int> instanceNameCountMap = new Dictionary<string, int>();
                IpfcSolid solid = currentModel as IpfcSolid;

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
                }

                foreach (var comp in instanceNameCountMap)
                {
                    int count = comp.Value;

                    IpfcModel model = _creoModelService.RetrieveModelFromStdDir(comp.Key, GetEpmType(comp.Key));

                    if (model == null) continue;

                    if (CurrentDataContext.AllLevel && model.Type == (int)EpfcModelType.EpfcMDL_ASSEMBLY && !AllAsm.Contains(model.FileName))
                    {
                        AllAsm.Add(model.FileName);
                        GetAllDependenciesRecursive(model, level + 1);
                    }
                    CurrentDataContext.AllCadModels.Add(model);

                    if (count > 0)
                        MainDispatcher.Invoke(() => AddCadModelInformation(model, level, currentModel, count, null));

                    CurrentDataContext.NbModelsInProgress++;
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private string GetEpmType(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return null;

                return fileName.ToUpper().Split('.').LastOrDefault();
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void AddCadModelInformation(IpfcModel activeModel, int level, IpfcModel parentCadModel, int nbInstance, List<IpfcFeature> listFeature)
        {
            try
            {
                string newNumber = _creoParameterService.GetParameterAsString(activeModel, "SUPERSEDED");

                if (!string.IsNullOrEmpty(newNumber))
                {
                    CurrentDataContext.ListItem.Add(new QuickChangeItem()
                    {
                        CurrentNumber = activeModel.FileName,
                        NewNumber = newNumber,
                        CadModel = activeModel,
                        Level = level,
                        ParentNumber = parentCadModel.FileName,
                        ParentCadModel = parentCadModel,
                        NbInstance = nbInstance,
                        ListFeature = listFeature
                    });
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void ExportReplaceComponentAsynch()
        {
            try
            {
                foreach (var comp in CurrentDataContext.ListItem)
                {
                    ReplaceOneComponent(comp);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ReplaceOneComponent(QuickChangeItem currentItem)
        {
            try
            {

                IpfcFeature[] featureArray = currentItem.ListFeature.ToArray();
                _creoFeatureService.CreateFeatureSelection(featureArray);
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void ExportCumulativeBomToExcel()
        {
            try
            {
                if (CurrentCadModel == null || CurrentDataContext.ListItem == null || !CurrentDataContext.ListItem.Any())
                    return;

                var excelProcessesBefore = Process.GetProcessesByName("Excel").ToList();
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string fileName = $"QUICK_CHANGE_{CurrentCadModel.FileName.Trim('.').FirstOrDefault()}.xlsx";
                string filePath = Path.Combine(documentsPath, fileName);

                var excelTool = new ExcelToolsClosedXml { CompleteFileName = filePath };
                if (excelTool.CreateNewFile("MAIN") != ExcelStatus.OK)
                {
                    ShowExportError(filePath);
                    return;
                }

                // Définir les en-têtes
                string sheetName = "MAIN";
                excelTool.CurrentSheet = sheetName;

                string[] headers = {
            "QCH_ColLevel", "QCH_ColCurrentNumber", "QCH_ColNewNumber",
            "QCH_ColNbInstance", "QCH_ColParentNumber"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    excelTool.SetCellValue(McgWpfTools.GetStringResource(headers[i]), 1, i + 1);
                }

                // Remplir les données
                int rowIndex = 2;
                foreach (var item in CurrentDataContext.ListItem)
                {
                    excelTool.SetCellValue(item.Level, rowIndex, 1);
                    excelTool.SetCellValue(item.CurrentNumber, rowIndex, 2);
                    excelTool.SetCellValue(item.NewNumber, rowIndex, 3);
                    excelTool.SetCellValue(item.NbInstance, rowIndex, 4);
                    excelTool.SetCellValue(item.ParentNumber, rowIndex, 5);
                    rowIndex++;
                }

                if (excelTool.SaveClose() != ExcelStatus.OK)
                {
                    ShowExportError(filePath);
                    return;
                }

                // Fermer le nouveau processus Excel si nécessaire
                var excelProcessesAfter = Process.GetProcessesByName("Excel").ToList();
                var newExcelProcess = excelProcessesAfter.FirstOrDefault(p => !excelProcessesBefore.Any(old => old.Id == p.Id));
                newExcelProcess?.Kill();

                _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("QCH_BtBomExport"),
                                                                       string.Format(McgWpfTools.GetStringResource("QCH_ExportXls"), filePath),
                                                                       McgWpfTools.GetStringResource("QCH_ToolTipOpen"),
                                                                       McgWpfTools.GetStringResource("QCH_ToolTipOpenFolder"),
                                                                       McgWpfTools.GetStringResource("QCH_ToolTipClose"),
                                                                       filePath);
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(GetType().Name, ex);
            }
        }

        private void ShowExportError(string filePath)
        {
            MessageBox.Show(
                string.Format(McgWpfTools.GetStringResource("QCH_ExportXlsIssue"), filePath),
                McgWpfTools.GetStringResource("QCH_TitleExportXlsIssue"),
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
        #endregion
    }
}
