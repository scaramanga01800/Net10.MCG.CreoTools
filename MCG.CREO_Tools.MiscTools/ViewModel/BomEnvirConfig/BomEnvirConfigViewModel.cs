using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.BomEnvirConfig;
using pfcls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.MiscTools.ViewModel.BomEnvirConfig
{
    public class BomEnvirConfigViewModel : ObservableObject, IBomEnvirConfigViewModel
    {
        #region [REGION] Properties from Interface
        public BomEnvirConfigDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private Dispatcher MainDispatcher { get; set; }
        private IpfcModel CurrentCadModel { get; set; }
        private List<string> AllAsm { get; set; }
        #endregion

        #region [REGION] Commands
        public ICommand CommandReadAsm { get => new RelayCommand(() => ExecuteReadAsm()); }
        public ICommand CommandUpdateCadDoc { get => new RelayCommand(() => ExecuteUpdateCadDoc()); }
        public ICommand CommandUpdateActiveCadModel { get => new RelayCommand(() => ExecuteUpdateActiveCadModel()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        public BomEnvirConfigViewModel(ICreoSessionProvider creoSessionProvider, ICreoModelService creoModelService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                CurrentDataContext = new BomEnvirConfigDataContext();
                MainDispatcher = Dispatcher.CurrentDispatcher;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDataContext.IsCreoEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDataContext.IsCreoEnable = e;

                CurrentDataContext.AsmNameChangedEvent += (sender, e) =>
                {
                    foreach (var item in CurrentDataContext.ListItem)
                    {
                        item.AsmName = CurrentDataContext.AsmNameValue;
                    }
                };
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

                Thread ListModelThread = new Thread(new ThreadStart(GetActiveModelDependenciesAsynch));
                ListModelThread.IsBackground = true;
                ListModelThread.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateCadDoc()
        {
            try
            {
                CurrentDataContext.IsPleaseWaitShown = true;
                Thread RenameCadDocThread = new Thread(new ThreadStart(UpdateCadDocAsynch));
                RenameCadDocThread.IsBackground = true;
                RenameCadDocThread.Start();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateActiveCadModel()
        {
            try
            {
                CurrentDataContext.ActiveModelFileName = McgWpfTools.GetStringResource("BEC_LabelNoActiveModel");
                if (CurrentDataContext.IsCreoEnable)
                {
                    var activeModel = _creoModelService.GetActiveModel();
                    CurrentDataContext.ActiveModel = activeModel;

                    if (activeModel != null)
                    {
                        var fileName = activeModel.FileName;
                        CurrentDataContext.ActiveModelFileName = fileName;
                        var extIndex = fileName.LastIndexOf('.');
                        CurrentDataContext.CadDocType = extIndex >= 0 ? fileName.Substring(extIndex + 1).ToUpper() : string.Empty;
                        CurrentDataContext.AsmNameValue = activeModel.InstanceName;

                    }
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
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("BEC_UserGuide"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc methods
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
                    CurrentDataContext.AllCadModels = new List<IpfcModel>();
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

        private void GetAllDependenciesRecursive(IpfcModel currentModel, int level, bool allLevel = false)
        {
            try
            {
                if (currentModel == null) return;

                //Dictionary<string, int> instanceNameCountMap = new Dictionary<string, int>();
                Dictionary<IpfcFeature, string> instanceNameCountMap = new Dictionary<IpfcFeature, string>();
                IpfcSolid solid = currentModel as IpfcSolid;

                if (solid != null)
                {
                    IpfcFeatures features = solid.ListFeaturesByType(false, EpfcFeatureType.EpfcFEATTYPE_COMPONENT);
                    foreach (IpfcFeature feat in features)
                    {
                        IpfcComponentFeat compFeat = feat as IpfcComponentFeat;
                        if (compFeat == null) continue;

                        string instanceName = compFeat.ModelDescr.GetFileName().ToLower();

                        instanceNameCountMap[feat] = instanceName;
                    }
                    CurrentDataContext.NbModels += instanceNameCountMap.Count;
                }
                int compOrder = 1;
                foreach (var comp in instanceNameCountMap)
                {
                    IpfcModel model = _creoModelService.RetrieveModelFromStdDir(comp.Value, GetEpmType(comp.Value));

                    if (model == null) continue;

                    if (allLevel && model.Type == (int)EpfcModelType.EpfcMDL_ASSEMBLY && !AllAsm.Contains(model.FileName))
                    {
                        AllAsm.Add(model.FileName);
                        GetAllDependenciesRecursive(model, level + 1);
                    }
                    CurrentDataContext.AllCadModels.Add(model);

                    MainDispatcher.Invoke(() => AddCadModelInformation(model, level, currentModel, comp.Key, null, compOrder));
                    compOrder++;
                    CurrentDataContext.NbModelsInProgress++;
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void AddCadModelInformation(IpfcModel activeModel, int level, IpfcModel parentCadModel, IpfcFeature creoFeat, List<IpfcFeature> listFeature, int compOrder)
        {
            try
            {
                IpfcParameterOwner aIpfcSolid = (IpfcParameterOwner)creoFeat;
                IpfcParameter CurrentAsmNameParam = aIpfcSolid.GetParam("ASM_NAME");
                IpfcParameter CurrentRepParam = aIpfcSolid.GetParam("REP");
                IpfcParameter CurrentRepFctParam = aIpfcSolid.GetParam("REP_FCT");

                string CurrentAsmName = "";
                if (CurrentAsmNameParam != null)
                {
                    IpfcParamValue CurrentQuantityValue = CurrentAsmNameParam.GetScaledValue();
                    CurrentAsmName = CurrentQuantityValue.StringValue;
                }

                string CurrentRep = "";
                if (CurrentRepParam != null)
                {
                    IpfcParamValue CurrentRepValue = CurrentRepParam.GetScaledValue();
                    CurrentRep = CurrentRepValue.StringValue;
                }

                string CurrentRepFct = "";
                if (CurrentRepFctParam != null)
                {
                    IpfcParamValue CurrentRepFctValue = CurrentRepFctParam.GetScaledValue();
                    CurrentRepFct = CurrentRepFctValue.StringValue;
                }

                CurrentDataContext.ListItem.Add(new BomEnvirConfigItem()
                {
                    Number = activeModel.FileName,
                    OldAsmName = CurrentAsmName,
                    AsmName = CurrentDataContext.AsmNameValue,
                    CreoFeature = creoFeat,
                    Level = level,
                    Rep = CurrentRep,
                    RepFct = CurrentRepFct,
                    CompOrder = compOrder

                });
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

        private void UpdateCadDocAsynch()
        {
            try
            {
                foreach (var item in CurrentDataContext.ListItem)
                {
                    item.Comment = McgWpfTools.GetStringResource("BEC_MsgRenamedInProgress");
                    UpdateParameterString((IpfcComponentFeat)item.CreoFeature, "ASM_NAME", item.AsmName);
                    item.Comment = McgWpfTools.GetStringResource("BEC_MsgRenamedSuccess");
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

        private void UpdateParameterString(IpfcComponentFeat aIpfcComponentFeat, string paramName, string value)
        {
            try
            {
                if (aIpfcComponentFeat == null || string.IsNullOrEmpty(paramName))
                    return;

                // Récupérer l'owner du paramètre (le composant dans l'assemblage de plus haut niveau)
                IpfcParameterOwner paramOwner = aIpfcComponentFeat as IpfcParameterOwner;
                if (paramOwner == null)
                    return;

                // Création de la valeur du paramètre
                CMpfcModelItem modelItemHelper = new CMpfcModelItem();
                IpfcParamValue newParamValue = modelItemHelper.CreateStringParamValue(value);

                // Recherche du paramètre existant
                IpfcParameter param = paramOwner.GetParam(paramName);

                if (param != null)
                {
                    param.SetScaledValue(newParamValue, param.Units);
                }
                else
                {
                    paramOwner.CreateParam(paramName, newParamValue);
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
