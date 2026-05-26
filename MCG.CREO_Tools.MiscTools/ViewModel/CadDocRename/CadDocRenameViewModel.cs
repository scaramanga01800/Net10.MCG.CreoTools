using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.CadDocRename;
using pfcls;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.MiscTools.ViewModel.CadDocRename
{
    public class CadDocRenameViewModel : ObservableObject, ICadDocRenameViewModel
    {
        #region [REGION] Properties from Interface
        public CadDocRenameDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private Dispatcher MainDispatcher { get; set; }
        private List<string> AllAsm { get; set; }
        private IpfcModel CurrentCadModel { get; set; }
        #endregion

        #region [REGION] Commands
        public ICommand CommandReadAsm { get => new RelayCommand(() => ExecuteReadAsm()); }
        public ICommand CommandRenameCadDoc { get => new RelayCommand(() => ExecuteRenameCadDoc()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        public CadDocRenameViewModel(ICreoSessionProvider creoSessionProvider,
                                     ICreoModelService creoModelService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;

                CurrentDataContext = new CadDocRenameDataContext();
                MainDispatcher = Dispatcher.CurrentDispatcher;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDataContext.IsCreoConnected = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDataContext.IsCreoConnected = e;

                CurrentDataContext.CadNumberChangedEvent += CurrentDataContext_CadNumberChangedEvent;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }

            _creoModelService = creoModelService;
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

                Thread ListModelThread = new Thread(new ThreadStart(GetActiveModelDependenciesAsynch));
                ListModelThread.IsBackground = true;
                ListModelThread.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRenameCadDoc()
        {
            try
            {
                CurrentDataContext.IsPleaseWaitShown = true;
                Thread RenameCadDocThread = new Thread(new ThreadStart(RenameCadDocAsynch));
                RenameCadDocThread.IsBackground = true;
                RenameCadDocThread.Start();
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
                McgFileAndSystemTools.OpenSharePointDocument(McgMiscTools.GetStringResource("CDR_UserGuide"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc Methods
        private void GetActiveModelDependenciesAsynch()
        {
            try
            {
                AllAsm = new List<string>();

                // Check if Active Model available (3D)
                IpfcModel activeModel = _creoModelService.GetActiveModel() ?? _creoSessionProvider.Session.get_CurrentWindow()?.Model;

                if (activeModel == null)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("CDR_MsgNotCadDocActivated"),
                                    McgWpfTools.GetStringResource("CDR_MsgTitleCadDocRenameIssue"),
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information,
                                    MessageBoxResult.OK);
                }
                else if (activeModel.Type != (int)EpfcModelType.EpfcMDL_ASSEMBLY)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("CDR_MsgNotAnAssembly"),
                                    McgWpfTools.GetStringResource("CDR_MsgTitleCadDocRenameIssue"),
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
                CurrentDataContext.IsRenamedPossible = true;
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

                    if (model.Type == (int)EpfcModelType.EpfcMDL_ASSEMBLY && !AllAsm.Contains(model.FileName))
                    {
                        AllAsm.Add(model.FileName);
                        GetAllDependenciesRecursive(model, level + 1);
                    }
                    CurrentDataContext.AllCadModels.Add(model);

                    if (count > 0)
                        MainDispatcher.Invoke(() => AddCadModelInformation(model));

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

        private void AddCadModelInformation(IpfcModel activeModel)
        {
            try
            {
                CurrentDataContext.ListItem.Add(new CadDocRenameItem()
                {
                    OldNumber = activeModel.FileName,
                    CreoModel = activeModel,
                    IsRenamed = false
                });
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void RenameCadDocAsynch()
        {
            try
            {
                // back up current model
                string backupFolder = Path.GetTempPath();
                CurrentDataContext.IsRenamedPossible = false;
                string backupFileName = CurrentCadModel.FileName;
                string backupFullPath = Path.Combine(backupFolder, backupFileName);
                IpfcModelDescriptor BackupDir = new CCpfcModelDescriptor().Create(CurrentCadModel.Type, null, null);

                BackupDir.Path = backupFolder;

                CurrentCadModel.Backup(BackupDir);

                IpfcWindow CurrentWindow = _creoModelService.GetCadDocWindow(CurrentCadModel);
                if (CurrentWindow != null)
                    CurrentWindow.Close();

                _creoSessionProvider.Session.EraseUndisplayedModels();

                // Open back up model
                CurrentCadModel = _creoModelService.RetrieveModelFromLocalDir(backupFolder, backupFileName);
                CurrentCadModel.Display();
                CurrentWindow = _creoModelService.GetCadDocWindow(CurrentCadModel);
                CurrentWindow.Activate();

                // rename models
                if (backupFileName.ToLower().Split('.').FirstOrDefault() != CurrentDataContext.CadNumber.ToLower())
                    CurrentCadModel.Rename(CurrentDataContext.CadNumber, true);

                EpfcModelType epmType;
                foreach (var item in CurrentDataContext.ListItem)
                {
                    item.Comment = McgWpfTools.GetStringResource("CDR_MsgRenamedInProgress");
                    if (item.CreoModel != null && !item.IsRenamed && item.OldNumber.ToLower().Split('.').FirstOrDefault() != item.NewNumber.ToLower())
                    {
                        try
                        {
                            epmType = _creoModelService.GetEpmType(item.OldNumber);

                            CurrentCadModel = _creoModelService.RetrieveModel(item.OldNumber, epmType);

                            CurrentCadModel.Rename(item.NewNumber, true);
                            item.IsRenamed = true;
                            item.Comment = McgWpfTools.GetStringResource("CDR_MsgRenamedSuccess");

                        }
                        catch (Exception exItem)
                        {
                            item.Comment = $"{McgWpfTools.GetStringResource("CDR_MsgRenamedError")} : {exItem.Message}";
                        }
                    }
                    if (item.Comment == McgWpfTools.GetStringResource("CDR_MsgRenamedInProgress"))
                        item.Comment = McgWpfTools.GetStringResource("CDR_MsgRenamedSuccess");
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

        private void CurrentDataContext_CadNumberChangedEvent(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentDataContext.CadNumber) || CurrentDataContext.ListItem == null || CurrentDataContext.ListItem.Count == 0)
                    return;

                int leadingZero = CurrentDataContext.SelectedLeadingZero > 0 ? CurrentDataContext.SelectedLeadingZero : 3;
                for (int i = 0; i < CurrentDataContext.ListItem.Count; i++)
                {
                    string indexStr = (i + 1).ToString().PadLeft(leadingZero, '0');
                    CurrentDataContext.ListItem[i].NewNumber = $"{CurrentDataContext.CadNumber}_COMP{indexStr}";
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
