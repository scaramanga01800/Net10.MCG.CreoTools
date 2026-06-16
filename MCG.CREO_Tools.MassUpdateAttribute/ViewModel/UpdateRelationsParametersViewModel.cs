using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MassUpdateAttribute.Configuration;
using MCG.CREO_Tools.MassUpdateAttribute.Exceptions;
using MCG.CREO_Tools.MassUpdateAttribute.View;
using pfcls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class UpdateRelationsParametersViewModel : ObservableObject, IUpdateRelationsParametersViewModel
    {
        #region [REGION] Properties from Interface
        public UpdateRelationsParametersDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private Dispatcher MainDispatcher { get; set; }
        private List<string> AllAsm { get; set; }
        private IpfcModel CurrentCadModel { get; set; }
        private List<IpfcModel> ListCadModelInSession { get; set; }
        private MassUpdateAttributeConfiguration CurrentMassUpdAttriConfiguration { get; set; }
        #endregion

        #region [REGION] Commands
        public ICommand CommandUpdateActiveCadModel { get => new RelayCommand(() => ExecuteUpdateActiveCadModel()); }
        public ICommand CommandReadAndUpdateCadDocument { get => new RelayCommand(() => ExecuteReadAndUpdateCadDocumentl()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoParameterService _creoParameterService;
        private readonly ICreoFeatureService _creoFeatureService;
        private readonly ICreoModelService _creoModelService;
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly IXmlSerializeTools _xmlSerializeTools;

        public UpdateRelationsParametersViewModel(ICreoParameterService creoParameterService,
                                                  ICreoFeatureService creoFeatureService,
                                                  ICreoModelService creoModelService,
                                                  ICreoSessionProvider creoSessionProvider,
                                                  IXmlSerializeTools xmlSerializeTools)
        {
            try
            {
                _creoParameterService = creoParameterService;
                _creoFeatureService = creoFeatureService;
                _creoModelService = creoModelService;
                _creoSessionProvider = creoSessionProvider;
                _xmlSerializeTools = xmlSerializeTools; 

                CurrentDataContext = new UpdateRelationsParametersDataContext();

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                MainDispatcher = Dispatcher.CurrentDispatcher;

                CurrentMassUpdAttriConfiguration = _xmlSerializeTools.GetDeserializedXml<MassUpdateAttributeConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MassUpdateAttributeConstants.ConfigurationFile}");

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDataContext.IsCreoEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDataContext.IsCreoEnable = e;
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteUpdateActiveCadModel()
        {
            try
            {
                CurrentDataContext.ActiveModelFileName = McgWpfTools.GetStringResource("MUA_LabelNoActiveModel");
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
                    }
                }
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteReadAndUpdateCadDocumentl()
        {
            try
            {
                CurrentDataContext.IsPleaseWaitShown = true;
                CurrentDataContext.NbModels = 0;
                CurrentDataContext.NbModelsInProgress = 0;
                CurrentDataContext.ListItem.Clear();
                AllAsm = new List<string>();
                if (CurrentDataContext.ActiveModel != null)
                {
                    CurrentDataContext.ListItem.Add(new UpdateRelationsParametersItem
                    {
                        CadModel = CurrentDataContext.ActiveModel,
                        Number = CurrentDataContext.ActiveModel.FileName
                    });
                    //if (!CurrentDataContext.IsUpperLevelSelected)
                    //{
                    var listModelThread = new Thread(GetActiveModelDependenciesAsynch)
                    {
                        IsBackground = true
                    };
                    listModelThread.Start();
                    //}
                }
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION]Misc functions
        private void GetActiveModelDependenciesAsynch()
        {
            try
            {
                if (!CurrentDataContext.IsUpperLevelSelected)
                {

                    List<object> tempList  = _creoModelService.GetOpenModels();
                    ListCadModelInSession= new List<IpfcModel>();
                    if (tempList != null && tempList.Count > 0)
                    {
                        foreach (var cadModel in tempList)
                        {
                            ListCadModelInSession.Add((IpfcModel) cadModel);
                        }
                    }   
                    GetAllDependenciesRecursive(CurrentDataContext.ActiveModel, CurrentDataContext.IsAllLevelsSelected);
                }

                UpdateRelationsParametersAsynch();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentDataContext.IsPleaseWaitShown = false;
            }
        }

        private void GetAllDependenciesRecursive(IpfcModel CurrentModel, bool isAllLevel)
        {
            try
            {

                List<string> allCadFileName = new List<string>();

                Dictionary<string, int> instanceNameCountMap = new Dictionary<string, int>();
                IpfcSolid solid = CurrentModel as IpfcSolid;
                IpfcDrawing drawing = CurrentModel as IpfcDrawing;

                if (drawing != null)
                {
                    IpfcDependencies AllDependencies = CurrentModel.ListDependencies();
                    if (AllDependencies != null)
                    {
                        IpfcDependency TempDep = null;
                        IpfcModelDescriptor TempModDesc = null;
                        IpfcModel TempModel = null;
                        foreach (var item in AllDependencies)
                        {
                            TempDep = (IpfcDependency)item;
                            TempModDesc = TempDep.DepModel;
                            TempModel = _creoSessionProvider.Session.GetModelFromDescr(TempModDesc);
                            if (TempModel != null)
                            {
                                if (isAllLevel && !allCadFileName.Any(asm => asm == TempModel.FileName))
                                {
                                    allCadFileName.Add(TempModel.FileName);
                                    if (TempModel.Type == (int)EpfcModelType.EpfcMDL_ASSEMBLY)
                                        GetAllDependenciesRecursive(TempModel, isAllLevel);
                                }
                            }
                        }
                    }
                }

                else if (solid != null)
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


                    IpfcModel TempModel = null;
                    foreach (var comp in instanceNameCountMap)
                    {
                        //TempModel = CurrentCREOConnection.RetrieveModelFromStdDir(comp.Key);
                        TempModel = ListCadModelInSession.FirstOrDefault(cad => cad.FileName == comp.Key);
                        if (TempModel != null)
                        {
                            MainDispatcher.Invoke(() => AddCadModel(TempModel));

                            if (isAllLevel && !allCadFileName.Any(asm => asm == comp.Key))
                            {
                                allCadFileName.Add(comp.Key);
                                if (TempModel.Type == (int)EpfcModelType.EpfcMDL_ASSEMBLY)
                                    GetAllDependenciesRecursive(TempModel, isAllLevel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateRelationsParametersAsynch()
        {
            try
            {
                CurrentDataContext.NbModels = CurrentDataContext.ListItem.Count;
                foreach (var cadModel in CurrentDataContext.ListItem)
                {
                    cadModel.Comment = McgWpfTools.GetStringResource("MUA_MsgRenamedInProgress"); ;
                    UpdateOneCadDocParamRelation(cadModel.CadModel);
                    cadModel.Comment = McgWpfTools.GetStringResource("MUA_MsgRenamedSuccess"); ;
                    CurrentDataContext.NbModelsInProgress++;
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateOneCadDocParamRelation(IpfcModel CurrentCadModel)
        {
            try
            {
                IpfcModel TemplateCadModel = null;
                IpfcModel2D Model2D = null;
                IpfcSolid SolidModel = null;
                bool Is3DModel = true;

                // Check that CAD document is still in session
                if (CurrentCadModel.FileName.IndexOf(".drw") > 0)
                {
                    Is3DModel = false;
                }
                else
                {
                    Is3DModel = true;
                }

                TemplateCadModel = GetCadTemplate(CurrentCadModel);


                if (CurrentCadModel != null && TemplateCadModel != null)
                {
                    _creoParameterService.UpdateRelationsAndParametersFromTemplate(CurrentCadModel, TemplateCadModel);
                }

                // regenerate Model
                TraceLog.AddTraceLog($"UpdateOneCadDocParamRelation: Generating CAD Model");
                if (Is3DModel)
                    SolidModel = (IpfcSolid)CurrentCadModel;
                else
                    Model2D = (IpfcModel2D)CurrentCadModel;

                if (Is3DModel)
                    try
                    {
                        SolidModel.Regenerate(null);
                    }
                    catch (Exception)
                    { }
                else
                    try
                    {
                        Model2D.Regenerate();
                    }
                    catch { }

                TraceLog.AddTraceLog($"UpdateOneCadDocParamRelation: Generated CAD Model");

                CurrentCadModel.Save();

                TraceLog.AddTraceLog($"UpdateOneCadDocParamRelation: Saved CAD Model {CurrentCadModel.FileName}");
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private IpfcModel GetCadTemplate(IpfcModel CurrentCadModel)
        {
            try
            {
                IpfcModel TemplateCadModel = null;

                if (CurrentCadModel != null)
                {
                    MassUpdateAttributeCadTemplate CurrentTemplate = null;


                    string cadTemplate = _creoParameterService.GetParameterAsString(CurrentCadModel, "TEMPLATE");
                    string cadType = "PRT";

                    if (CurrentCadModel.Type == (int)EpfcModelType.EpfcMDL_ASSEMBLY)
                        cadType = "ASM";
                    else if (CurrentCadModel.Type == (int)EpfcModelType.EpfcMDL_DRAWING)
                        cadType = "DRW";

                    if (cadTemplate != null)
                        CurrentTemplate = CurrentMassUpdAttriConfiguration.ListTemplate.FirstOrDefault(item => item.CadDocType == cadType && item.Template == cadTemplate);

                    if (CurrentTemplate == null)
                    {
                        if (CurrentCadModel.Type == (int)EpfcModelType.EpfcMDL_ASSEMBLY)
                            CurrentTemplate = CurrentMassUpdAttriConfiguration.ListTemplate.FirstOrDefault(item => item.IsDefaultAsm);
                        else if (CurrentCadModel.Type == (int)EpfcModelType.EpfcMDL_PART)
                        {
                            if (CurrentCadModel.Type == (int)EpfcModelType.EpfcMDL_PART)
                                CurrentTemplate = CurrentMassUpdAttriConfiguration.ListTemplate.FirstOrDefault(item => item.IsDefaultPrt);
                            else if (_creoFeatureService.IsSheetMetal(CurrentCadModel))
                                CurrentTemplate = CurrentMassUpdAttriConfiguration.ListTemplate.FirstOrDefault(item => item.IsDefaultSheetMetal);
                            else if (_creoFeatureService.IsBulkItem(CurrentCadModel))
                                CurrentTemplate = CurrentMassUpdAttriConfiguration.ListTemplate.FirstOrDefault(item => item.IsDefaultBulk);
                        }
                    }

                    if (CurrentTemplate != null)
                        TemplateCadModel = _creoModelService.RetrieveModel(CurrentTemplate.FileName);
                }
                return TemplateCadModel;
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void AddCadModel(IpfcModel currentModel)
        {
            try
            {

                CurrentDataContext.ListItem.Add(new UpdateRelationsParametersItem
                {
                    CadModel = currentModel,
                    Number = currentModel.FileName
                });
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
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
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
