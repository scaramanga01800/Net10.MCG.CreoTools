using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.Models.SAP;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.WindchillCredential;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.Interfaces;
using MCG.CREO_Tools.MiscTools.View.BomExport;
using MCG.CREO_Tools.MiscTools.ViewModel.Configuration;
using MCG.Tools.VisualizationLib.Interfaces;
using MCG.WindchillRequestTool.Model.BomComparison;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.MiscTools.ViewModel.BomExport
{
    public class BomExportWindowViewModel : ObservableObject, IBomExportWindowViewModel
    {
        #region [REGION] Properties from Interface
        public BomExportWindowDataContext CurrentBomExportWindowDataContext { get; set; } = new BomExportWindowDataContext();
        public Window ParentWindow { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; } = "";
        private BomExportConfiguration CurrentBomExportConfiguration { get; set; }
        private WindchillCredentialItem WindchillNetworkCredential { get; set; }
        private WindchillObjectType CurrentWindchillType { get; set; } = WindchillObjectType.UNKNOWN;

        public List<WindchillObjStructureComponent> EndItemList { get; set; }

        private List<WindchillObjStructureComponent> AllComponent { get; set; } = new List<WindchillObjStructureComponent>();
        private List<WindchillObjStructureComponent> CompleteBom { get; set; } = new List<WindchillObjStructureComponent>();
        private Thread ThreadSearchSapInfo { get; set; }
        private Thread ThreadSearchBom { get; set; }
        private bool IsSapInformationSearched { get; set; } = false;
        private List<SapCostVolumeInfo> CurrentAllCostVolume { get; set; }
        private Dispatcher MainDispatcher { get; set; } = null;
        private WindchillObjStructureComponent RawBom { get; set; }
        private WindchillObject UpperWindchillObject { get; set; }

        private BomExportCumulativeView bomExportCumulativeView { get; set; }
        private string CumulativeBomFrom { get; set; }
        #endregion

        #region [REGION] Commands
        public ICommand CommandAddParameter { get => new RelayCommand(() => ExecuteAddParameter()); }
        public ICommand CommandRemoveParameter { get => new RelayCommand(() => ExecuteRemoveParameter()); }
        public ICommand CommandMoveUpParameter { get => new RelayCommand<BomExportParameter>((param) => ExecuteMoveUpParameter(param)); }
        public ICommand CommandMoveDownParameter { get => new RelayCommand<BomExportParameter>((param) => ExecuteMoveDownParameter(param)); }
        public ICommand CommandStartBomSearch { get => new RelayCommand(() => ExecuteStartBomSearch()); }
        public ICommand CommandStartBomExport { get => new RelayCommand(() => ExecuteStartBomExport()); }
        public ICommand CommandSelectedBomItem { get => new RelayCommand<RoutedPropertyChangedEventArgs<object>>((obj) => ExecuteSelectedBomItem(obj)); }
        public ICommand CommandSortBom { get => new RelayCommand<BomExportParameter>((col) => ExecuteSortBom(col)); }
        public ICommand CommandSapPlantSelectionChanged { get => new RelayCommand(() => ExecuteSapPlantSelectionChanged()); }
        public ICommand CommandCopyPartNumber { get => new RelayCommand<WindchillObjStructureComponent>((obj) => ExecuteCopyPartNumber(obj)); }
        public ICommand CommandStartPendingEcnSearch { get => new RelayCommand(() => ExecuteStartPendingEcnSearch()); }
        public ICommand CommandShowOccurrencesChanged { get => new RelayCommand(() => ExecuteShowOccurrencesChanged()); }
        public ICommand CommandClosing { get => new RelayCommand(() => ExecuteClosing()); }
        public ICommand CommandBtHelpMouseLeftButtonUpEvent { get => new RelayCommand(() => UpdateBtHelpMouseLeftButtonUpEvent()); }
        public ICommand CommandHelpVisuTool { get => new RelayCommand(() => UpdateHelpVisuTool()); }
        public ICommand CommandRemoveLine { get => new RelayCommand(() => ExecuteRemoveLine()); }
        public ICommand CommandResetBom { get => new RelayCommand(() => ExecuteResetBom()); }
        public ICommand CommandDownloadDrawing { get => new RelayCommand<string>((from) => ExecuteDownloadDrawing(from)); }
        public ICommand CommandStartCumulativeMaterial { get => new RelayCommand(() => ExecuteStartCumulativeMaterial()); }
        public ICommand CommandStartCumulativeName { get => new RelayCommand(() => ExecuteStartCumulativeName()); }
        public ICommand CommandStartCumulativeBomExport { get => new RelayCommand(() => ExecuteStartCumulativeBomExport()); }
        public ICommand CommandCloseCumulativeBomExport { get => new RelayCommand(() => ExecuteCloseCumulativeBomExport()); }
        public ICommand CommandCumulateInWorkNumber { get => new RelayCommand(() => ExecuteCumulateInWorkNumber()); }
        public ICommand CommandToggleExpandCollapse { get => new RelayCommand<bool>((obj) => ExecuteToggleExpandCollapse(obj)); }
        #endregion

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

        public event EventHandler ClosingEvent;

        public void RaiseClosingEvent()
        {
            try
            {
                ClosingEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Init
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly IWindchillEpmDocumentManagementService _windchillEpmDocumentManagementService;
        private readonly IWindchillPartManagementService _windchillPartManagementService;
        private readonly IWindchillBomManagementService _windchillBomManagementService;
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        private readonly ISapHupService _sapHupService;
        private readonly IWindchillReportingManagementService _windchillReportingManagementService;
        private readonly IWindchillRequestMiscService _windchillRequestMiscService;
        private readonly IMiscToolsWindchillService _miscToolsWindchillService;
        private readonly IWtDownloadViewableTools _wtDownloadViewableTools;
        public BomExportWindowViewModel(IXmlSerializeTools xmlSerializeTools,
                                        IWindchillCredentialService windchillCredentialService,
                                        IMcgCommonLibWindowService mcgCommonLibWindowService,
                                        ISapHupService sapHupService,
                                        IWindchillEpmDocumentManagementService windchillEpmDocumentManagementService,
                                        IWindchillPartManagementService windchillPartManagementService,
                                        IWindchillBomManagementService windchillBomManagementService,
                                        IWindchillReportingManagementService windchillReportingManagementService,
                                        IWindchillRequestMiscService windchillRequestMiscService,
                                        IMiscToolsWindchillService miscToolsWindchillService,
                                        IWtDownloadViewableTools wtDownloadViewableTools)
        {
            try
            {
                _xmlSerializeTools = xmlSerializeTools;
                _windchillCredentialService = windchillCredentialService;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;
                _sapHupService = sapHupService;
                _windchillEpmDocumentManagementService = windchillEpmDocumentManagementService;
                _windchillPartManagementService = windchillPartManagementService;
                _windchillBomManagementService = windchillBomManagementService;
                _windchillReportingManagementService = windchillReportingManagementService;
                _windchillRequestMiscService = windchillRequestMiscService;
                _miscToolsWindchillService = miscToolsWindchillService;
                _wtDownloadViewableTools = wtDownloadViewableTools;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                MainDispatcher = Dispatcher.CurrentDispatcher;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.MainDictionary}", UriKind.Absolute);

                CurrentBomExportConfiguration = _xmlSerializeTools.GetDeserializedXml<BomExportConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.BomExportConfigurationFile}");
                BomExportUserPreferences CurrentBomExportUserPreferences = GetUserConfigFromXmlFile();

                if (CurrentBomExportUserPreferences == null)
                    CurrentBomExportUserPreferences = new BomExportUserPreferences();



                // Update SAP plant Info
                if (CurrentBomExportConfiguration.ShowSapCostVolumeInfo)
                {
                    CurrentBomExportWindowDataContext.ShowSapCostVolumeInfo = CurrentBomExportConfiguration.ShowSapCostVolumeInfo;
                    foreach (var item in CurrentBomExportConfiguration.ListSapPlant)
                        CurrentBomExportWindowDataContext.ListSapPlant.Add(item);
                    SapPlant UserSapPlant = null;
                    if (CurrentBomExportUserPreferences.CurrentSapPlant != null)
                        UserSapPlant = CurrentBomExportConfiguration.ListSapPlant.FirstOrDefault((item) => item.Name == CurrentBomExportUserPreferences.CurrentSapPlant.Name);
                    if (UserSapPlant != null)
                        CurrentBomExportWindowDataContext.SelectedSapPlant = UserSapPlant;
                    else
                        CurrentBomExportWindowDataContext.SelectedSapPlant = CurrentBomExportWindowDataContext.ListSapPlant.FirstOrDefault();
                }

                // Update Parameter Lists
                foreach (var param in CurrentBomExportConfiguration.ListAvailableParameter)
                {
                    CurrentBomExportWindowDataContext.ListAllParameters.Add(new BomExportParameter()
                    {
                        MainApp = this,
                        Order = param.Order,
                        ParamId = param.ParamId,
                        ParamName = param.ParamName,
                        ParamNameShown = param.ParamName,
                        Source = param.Source,
                        IsAPrice = param.IsAPrice,
                        IsVisible = false,
                        IsSelected = false
                    });
                }

                List<BomExportParameterData> TempParamList;
                if (CurrentBomExportUserPreferences != null && CurrentBomExportUserPreferences.ListSelectedParameter != null)
                {
                    TempParamList = CurrentBomExportUserPreferences.ListSelectedParameter;
                    CurrentBomExportWindowDataContext.IsLevelIndented = CurrentBomExportUserPreferences.IsLevelIndented;
                    CurrentBomExportWindowDataContext.IsStateInWork = CurrentBomExportUserPreferences.IsStateInWork;
                    CurrentBomExportWindowDataContext.IsStateObsolete = CurrentBomExportUserPreferences.IsStateObsolete;
                    CurrentBomExportWindowDataContext.IsStatePreReleased = CurrentBomExportUserPreferences.IsStatePreReleased;
                    CurrentBomExportWindowDataContext.IsStatePrototype = CurrentBomExportUserPreferences.IsStatePrototype;
                    CurrentBomExportWindowDataContext.IsStateReleased = CurrentBomExportUserPreferences.IsStateReleased;
                    CurrentBomExportWindowDataContext.IsStateRework = CurrentBomExportUserPreferences.IsStateRework;
                    CurrentBomExportWindowDataContext.IsStateSuperseded = CurrentBomExportUserPreferences.IsStateSuperseded;
                    CurrentBomExportWindowDataContext.IsStateUnderReview = CurrentBomExportUserPreferences.IsStateUnderReview;
                }
                else
                    TempParamList = CurrentBomExportConfiguration.ListSelectedParameter;

                foreach (var param in TempParamList)
                {
                    BomExportParameter CurrentBomExportParameter = CurrentBomExportWindowDataContext.ListAllParameters.FirstOrDefault((item) => item.ParamId == param.ParamId);
                    if (CurrentBomExportParameter != null)
                    {
                        CurrentBomExportParameter.IsSelected = param.IsSelected;
                        CurrentBomExportParameter.IsVisible = param.IsSelected;
                        CurrentBomExportParameter.Order = param.Order;
                        CurrentBomExportWindowDataContext.ListSelectedParameters.Add(CurrentBomExportParameter);
                    }
                }

                CurrentBomExportWindowDataContext.BomColumnNumber = new BomExportParameter() { IsVisible = true, ParamName = "Number", ParamNameShown = "Number", ParamId = "Number" };
                CurrentBomExportWindowDataContext.BomColumnLevel = new BomExportParameter() { IsVisible = true, ParamName = "Lv", ParamNameShown = "Lv", ParamId = "BomLevel" };
                CurrentBomExportWindowDataContext.SubscribeListAllParametersEvents();
                ReorderParameter();

                // Update File Export Info
                if (CurrentBomExportConfiguration.ListOutputFormat != null)
                {
                    foreach (var outputFormat in CurrentBomExportConfiguration.ListOutputFormat)
                        CurrentBomExportWindowDataContext.ListOutputFormat.Add(outputFormat);
                    BomExportOutputFormat TempFormat;
                    if (CurrentBomExportUserPreferences != null && CurrentBomExportUserPreferences.SelectedOutputFormat != null)
                        TempFormat = CurrentBomExportUserPreferences.SelectedOutputFormat;
                    else
                        TempFormat = CurrentBomExportConfiguration.SelectedOutputFormat;
                    CurrentBomExportWindowDataContext.SelectedOutputFormat = CurrentBomExportWindowDataContext.ListOutputFormat.FirstOrDefault((format) => format.Name == TempFormat.Name);
                }

                if (CurrentBomExportUserPreferences != null && CurrentBomExportUserPreferences.FieldSeparator != 0)
                    CurrentBomExportWindowDataContext.FieldSeparator = CurrentBomExportUserPreferences.FieldSeparator;
                else
                    CurrentBomExportWindowDataContext.FieldSeparator = CurrentBomExportConfiguration.FieldSeparator;

                // Update Bom Level Combo Box
                int MaxLevel = MiscToolsConstants.MaxBomLevel;
                CurrentBomExportWindowDataContext.BomLevelList = new List<int>();
                for (int i = 1; i <= MaxLevel; i++)
                    CurrentBomExportWindowDataContext.BomLevelList.Add(i);
                CurrentBomExportWindowDataContext.BomLevel = 1;


                // update Nb Digit for REP information
                CurrentBomExportWindowDataContext.NumericalLineNumberDigit = MiscToolsConstants.DefaultRepDigit;
                for (int index = MiscToolsConstants.MinRepDigit; index <= MiscToolsConstants.MaxRepDigit; index++)
                    CurrentBomExportWindowDataContext.NumericalLineNumberDigitList.Add(index);

                CurrentBomExportWindowDataContext.NumericalLineNumberDigitEvent += UpdateRepValue;
                CurrentBomExportWindowDataContext.SapPlantChangeEvent += UpdateUserConfigXmlFile;
                CurrentBomExportWindowDataContext.IsParameterUpdateEvent += ReorderParameter;
                CurrentBomExportWindowDataContext.IsParameterUpdateEvent += UpdateUserConfigXmlFile;

                CurrentBomExportWindowDataContext.ListAllParameters.CollectionChanged += SubcribeListAllParametersCollectionChanged;
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }

            _mcgCommonLibWindowService = mcgCommonLibWindowService;
            _windchillPartManagementService = windchillPartManagementService;
            _windchillRequestMiscService = windchillRequestMiscService;
        }

        private void SubcribeListAllParametersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                int index = 0;
                foreach (var item in CurrentBomExportWindowDataContext.ListAllParameters)
                    item.Order = index++;
                CurrentBomExportWindowDataContext.ListAllParameters.CollectionChanged -= SubcribeListAllParametersCollectionChanged;
                ReorderParameter();
                CurrentBomExportWindowDataContext.ListAllParameters.CollectionChanged += SubcribeListAllParametersCollectionChanged;
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void SubcribeCloseEvent()
        {
            try
            {
                if (ParentWindow != null)
                    ParentWindow.Closing += KillCurrentThread;
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void KillCurrentThread(object sender, CancelEventArgs e)
        {
            try
            {
                if (ThreadSearchBom != null)
                    ThreadSearchBom.Abort();
                if (ThreadSearchSapInfo != null)
                    ThreadSearchSapInfo.Abort();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ReorderParameter(object sender = null, EventArgs e = null)
        {
            try
            {
                List<BomExportParameter> TempListParam = CurrentBomExportWindowDataContext.ListAllParameters.OrderBy((param) => param.Order).ToList();
                CurrentBomExportWindowDataContext.ListAllParametersAuthorized.Clear();
                if (TempListParam != null && TempListParam.Count > 0)
                {
                    int Index = 1;
                    CurrentBomExportWindowDataContext.ListAllParameters.Clear();
                    CurrentBomExportWindowDataContext.ListSelectedParameters.Clear();

                    foreach (var param in TempListParam)
                    {
                        param.Order = Index;
                        CurrentBomExportWindowDataContext.ListAllParameters.Add(param);
                        Index++;
                        if (param.IsSelected)
                            CurrentBomExportWindowDataContext.ListSelectedParameters.Add(param);
                        if (param.Source != null && param.Source == "SAP")
                        {
                            param.IsAuthorized = CurrentBomExportWindowDataContext.ShowSapCostVolumeInfo;
                            if (!CurrentBomExportWindowDataContext.ShowSapCostVolumeInfo)
                                param.IsSelected = false;
                        }
                        if (param.IsAuthorized)
                            CurrentBomExportWindowDataContext.ListAllParametersAuthorized.Add(param);
                    }
                }

                // Update List of available parameter
                List<BomExportParameter> TempAvailableParamList = CurrentBomExportWindowDataContext.ListAllParameters.Where((aparam) => !aparam.IsSelected).ToList();
                CurrentBomExportWindowDataContext.ListAvailableParameters.Clear();
                foreach (var param in TempAvailableParamList.OrderBy((param) => param.ParamName))
                {
                    CurrentBomExportWindowDataContext.ListAvailableParameters.Add(param);
                }

                // Update column name and order parameters
                // Hide all column 
                for (int index = 1; index <= 20; index++)
                {
                    PropertyInfo CurrentProp = CurrentBomExportWindowDataContext.GetType().GetProperty($"BomColumn{index}");

                    if (CurrentProp != null)
                    {
                        object temp = CurrentProp.GetValue(CurrentBomExportWindowDataContext);
                        if (temp != null) ((BomExportParameter)temp).IsVisible = false;
                    }
                }

                // Change only selected ones
                foreach (var param in CurrentBomExportWindowDataContext.ListSelectedParameters)
                {
                    PropertyInfo CurrentProp = CurrentBomExportWindowDataContext.GetType().GetProperty($"BomColumn{param.Order}");
                    if (CurrentProp != null) CurrentProp.SetValue(CurrentBomExportWindowDataContext, new BomExportParameter()
                    {
                        IsVisible = true,
                        ParamName = param.ParamName,
                        Order = param.Order,
                        ParamNameShown = param.ParamNameShown,
                        ParamId = param.ParamId
                    });

                }

                UpdateBomComponentParameter(CurrentBomExportWindowDataContext.MainBom);

                UpdateColumnWidth();

                UpdateUserConfigXmlFile();
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void InvokeReorderParameter()
        {
            try
            {
                ReorderParameter();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateColumnWidth()
        {
            try
            {
                PropertyInfo CurrentProp;
                object temp;
                int InitSize = 0;

                // init size
                for (int index = 1; index <= 20; index++)
                {
                    CurrentProp = CurrentBomExportWindowDataContext.GetType().GetProperty($"BomColumn{index}");
                    if (CurrentProp != null)
                    {
                        temp = CurrentProp.GetValue(CurrentBomExportWindowDataContext);
                        if (temp != null) ((BomExportParameter)temp).Width = 0;
                    }
                }

                foreach (var param in CurrentBomExportWindowDataContext.ListSelectedParameters)
                {
                    CurrentProp = CurrentBomExportWindowDataContext.GetType().GetProperty($"BomColumn{param.Order}");
                    if (CurrentProp != null)
                    {
                        temp = CurrentProp.GetValue(CurrentBomExportWindowDataContext);
                        if (temp != null && ((BomExportParameter)temp).ParamNameShown != null) InitSize = ((BomExportParameter)temp).ParamNameShown.ToString().Length;
                        else InitSize = 5;
                        if (temp != null)
                            ((BomExportParameter)temp).Width = MaxCharacterColValue(CurrentBomExportWindowDataContext.MainBom, $"ValueCol{param.Order}", InitSize) * 7 + 20;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void UpdateUserConfigXmlFile(object sender = null, EventArgs e = null)
        {
            try
            {

                CurrentBomExportWindowDataContext.SapPlantChangeEvent -= UpdateUserConfigXmlFile;
                CurrentBomExportWindowDataContext.IsParameterUpdateEvent -= UpdateUserConfigXmlFile;

                if (CurrentBomExportWindowDataContext.ListSelectedParameters != null)
                {
                    BomExportUserPreferences CurrentConfig = new BomExportUserPreferences
                    {
                        //ListSelectedParameter = CurrentBomExportWindowDataContext.ListSelectedParameters.Select((param) => param.GetBomExportParameterData()).ToList(),
                        ListSelectedParameter = CurrentBomExportWindowDataContext.ListAllParameters.Select((param) => param.GetBomExportParameterData()).ToList(),
                        FieldSeparator = CurrentBomExportWindowDataContext.FieldSeparator,
                        SelectedOutputFormat = CurrentBomExportWindowDataContext.SelectedOutputFormat,
                        CurrentSapPlant = CurrentBomExportWindowDataContext.SelectedSapPlant,
                        IsLevelIndented = CurrentBomExportWindowDataContext.IsLevelIndented,
                        IsStateInWork = CurrentBomExportWindowDataContext.IsStateInWork,
                        IsStateObsolete = CurrentBomExportWindowDataContext.IsStateObsolete,
                        IsStatePreReleased = CurrentBomExportWindowDataContext.IsStatePreReleased,
                        IsStatePrototype = CurrentBomExportWindowDataContext.IsStatePrototype,
                        IsStateReleased = CurrentBomExportWindowDataContext.IsStateReleased,
                        IsStateRework = CurrentBomExportWindowDataContext.IsStateRework,
                        IsStateSuperseded = CurrentBomExportWindowDataContext.IsStateSuperseded,
                        IsStateUnderReview = CurrentBomExportWindowDataContext.IsStateUnderReview
                    };

                    _xmlSerializeTools.SerializedXmlInAppData<BomExportUserPreferences>(CurrentConfig, MiscToolsConstants.BomExportUserPreferencesFile);
                }

                CurrentBomExportWindowDataContext.SapPlantChangeEvent += UpdateUserConfigXmlFile;
                CurrentBomExportWindowDataContext.IsParameterUpdateEvent += UpdateUserConfigXmlFile;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private BomExportUserPreferences GetUserConfigFromXmlFile()
        {
            try
            {
                BomExportUserPreferences CurrentConfig = _xmlSerializeTools.GetDeserializedXmlFromAppData<BomExportUserPreferences>(MiscToolsConstants.BomExportUserPreferencesFile);

                return CurrentConfig;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteAddParameter()
        {
            try
            {
                if (CurrentBomExportWindowDataContext.SelectedParameterAvailable != null)
                {
                    CurrentBomExportWindowDataContext.SelectedParameterAvailable.Order = 1000;
                    CurrentBomExportWindowDataContext.SelectedParameterAvailable.IsSelected = true;
                    CurrentBomExportWindowDataContext.ListSelectedParameters.Add(CurrentBomExportWindowDataContext.SelectedParameterAvailable);
                    //CurrentBomExportWindowDataContext.ListAvailableParameters.Remove(CurrentBomExportWindowDataContext.SelectedParameterAvailable);
                    CurrentBomExportWindowDataContext.SelectedParameterAvailable = null;
                    ReorderParameter();
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemoveParameter()
        {
            try
            {
                if (CurrentBomExportWindowDataContext.SelectedParameter != null)
                {
                    CurrentBomExportWindowDataContext.SelectedParameter.Order = 1000;
                    CurrentBomExportWindowDataContext.SelectedParameter.IsSelected = false;
                    //CurrentBomExportWindowDataContext.ListAvailableParameters.Add(CurrentBomExportWindowDataContext.SelectedParameter);
                    CurrentBomExportWindowDataContext.ListSelectedParameters.Remove(CurrentBomExportWindowDataContext.SelectedParameter);
                    CurrentBomExportWindowDataContext.SelectedParameter = null;
                    ReorderParameter();
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMoveUpParameter(BomExportParameter CurrentParam)
        {
            try
            {
                SwitchParameter(CurrentParam, -1);
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteMoveDownParameter(BomExportParameter CurrentParam)
        {
            try
            {
                SwitchParameter(CurrentParam, +1);
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartBomSearch()
        {
            try
            {
                // param: Number,Name,DESCRIPTION_2,DESCRIPTION2_1,DESCRIPTION2_2,GROUP_CREATOR,MASS,QUALINSPGRP,versionInfo.identifier.versionId,State,REP,depType,quantity.amount,quantity.unit
                // http://cranesplm.prod.manitowoc.com:8585/Windchill/servlet/rest/structure/objects/OR%3Awt.part.WTPart%3A3333116502/descendants?%24select=Number%2CName%2CDESCRIPTION_2%2CDESCRIPTION2_1%2CDESCRIPTION2_2%2CGROUP_CREATOR%2CMASS%2CQUALINSPGRP%2CversionInfo.identifier.versionId%2CState%2CREP%2CdepType%2Cquantity.amount%2Cquantity.unit&inline=false&levels=1
                // Data for test
                //CurrentBomExportWindowDataContext.Number = "82020189.ASM";
                //CurrentBomExportWindowDataContext.Revision = "J";

                //CurrentBomExportWindowDataContext.Number = "82550100.ASM";
                //CurrentBomExportWindowDataContext.Revision = "A";

                //CurrentBomExportWindowDataContext.Number = "82009427.ASM";
                //CurrentBomExportWindowDataContext.Revision = "C";

                //82020192, TOWER CRANE GMA
                CurrentBomExportWindowDataContext.IsSearchBomDone = false;

                CurrentBomExportWindowDataContext.StatusBarMsg = "";
                if (CurrentBomExportWindowDataContext.Number == null || CurrentBomExportWindowDataContext.Number == "" || CurrentBomExportWindowDataContext.Number.Contains("*"))
                    MessageBox.Show(String.Format(McgWpfTools.GetStringResource("BCE_EnterNumber"), CurrentWindchillType, CurrentBomExportWindowDataContext.Number, CurrentBomExportWindowDataContext.Revision), "BOM Search Issue", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                else
                {
                    CurrentBomExportWindowDataContext.IsActionProgress = true;
                    if (CurrentBomExportWindowDataContext.IsAssemblyChecked)
                    {
                        //CurrentWindchillType = WindchillObjectInternalType.EPMDoc;
                        CurrentWindchillType = WindchillObjectType.ASM;
                        if (!CurrentBomExportWindowDataContext.Number.Contains(".")) CurrentBomExportWindowDataContext.Number = $"{CurrentBomExportWindowDataContext.Number}.ASM";
                    }
                    else if (CurrentBomExportWindowDataContext.IsPartChecked)
                    {
                        //CurrentWindchillType = WindchillObjectInternalType.WTPart;
                        CurrentWindchillType = WindchillObjectType.PART;
                        if (CurrentBomExportWindowDataContext.Number.Contains(".")) CurrentBomExportWindowDataContext.Number = CurrentBomExportWindowDataContext.Number.Split('.').FirstOrDefault();
                    }

                    WindchillNetworkCredential = _windchillCredentialService.GetWindchillCredential(CommonLibConstants.WindchillUrl, CommonLibConstants.WindchillUrl);
                    if (CurrentBomExportWindowDataContext.IsLatestRevision) CurrentBomExportWindowDataContext.Revision = "LATEST";
                    if (CurrentBomExportWindowDataContext.Revision == null || CurrentBomExportWindowDataContext.Revision == "") CurrentBomExportWindowDataContext.Revision = "LATEST";

                    CurrentBomExportWindowDataContext.MainBom.Clear();

                    RaiseActionInProgressEvent();

                    ThreadSearchBom = new Thread(() => SearchBomAsynch());
                    ThreadSearchBom.Start();
                }

            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                CurrentBomExportWindowDataContext.IsSearchBomDone = true;
                CurrentBomExportWindowDataContext.IsActionProgress = false;
                CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg11");
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartBomExport()
        {
            try
            {
                CurrentBomExportWindowDataContext.IsActionProgress = true;
                if (CurrentBomExportWindowDataContext.SelectedOutputFormat.Name == "Excel")
                    ExportBomToExcel();
                else if (CurrentBomExportWindowDataContext.SelectedOutputFormat.Name == "CSV File")
                    ExportBomToCsvFile();
                UpdateUserConfigXmlFile();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentBomExportWindowDataContext.IsActionProgress = false;
            }
        }

        private void ExecuteSelectedBomItem(RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (e != null && e.NewValue != null)
                    CurrentBomExportWindowDataContext.SelectedBomItem = (WindchillObjStructureComponent)e.NewValue;
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSortBom(BomExportParameter CurrentColumn)
        {
            try
            {
                RecursiveBomSort(CurrentBomExportWindowDataContext.MainBom, CurrentColumn, CurrentColumn.OrderByAscending);
                CurrentColumn.OrderByAscending = !CurrentColumn.OrderByAscending;
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSapPlantSelectionChanged()
        {
            try
            {
                CurrentBomExportWindowDataContext.IsActionProgress = true;
                UpdateSapCostVolumeInformation();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentBomExportWindowDataContext.IsActionProgress = false;
            }
        }

        private void ExecuteCopyPartNumber(WindchillObjStructureComponent obj)
        {
            try
            {
                if (obj != null)
                    McgWpfTools.CopyTextClipboard(obj.Number);

                if (CurrentBomExportWindowDataContext.SelectedBomItem != null)
                    McgWpfTools.CopyTextClipboard(CurrentBomExportWindowDataContext.SelectedBomItem.Number);
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartPendingEcnSearch()
        {
            try
            {

                // search all components with state not approved. ("In Work", "Rework", "Under Review")
                List<WindchillObjStructureComponent> ListComp;
                if (AllComponent != null && AllComponent.Count > 0)
                {
                    CurrentBomExportWindowDataContext.IsActionProgress = true;
                    ListComp = AllComponent.Where((item) => CurrentBomExportConfiguration.UnapprovedState.Contains(item.MainWindchillObject.State)).ToList();
                    ThreadSearchSapInfo = new Thread((obj) => SearchEcnInformationAsynch(ListComp));
                    ThreadSearchSapInfo.Start();
                }
            }
            catch (Exception ex)
            {
                CurrentBomExportWindowDataContext.IsActionProgress = false;
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteShowOccurrencesChanged()
        {
            try
            {
                UpdateMainBom(true);
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteClosing()
        {
            try
            {
                UpdateUserConfigXmlFile();
                RaiseClosingEvent();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateBtHelpMouseLeftButtonUpEvent()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("BCE_LinkHelpBomExport"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateHelpVisuTool()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("BCE_LinkHelpBomExportVisuTool"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void SwitchParameter(BomExportParameter CurrentParam, int increment)
        {
            try
            {
                CurrentBomExportWindowDataContext.ListAllParameters.CollectionChanged -= SubcribeListAllParametersCollectionChanged;
                BomExportParameter TempParam = CurrentBomExportWindowDataContext.ListAllParameters.FirstOrDefault((param) => param.Order == CurrentParam.Order + increment);
                if (TempParam != null)
                {
                    TempParam.Order = CurrentParam.Order;
                    CurrentParam.Order += increment;
                    ReorderParameter();
                }
                CurrentBomExportWindowDataContext.ListAllParameters.CollectionChanged += SubcribeListAllParametersCollectionChanged;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemoveLine()
        {
            try
            {
                if (CurrentBomExportWindowDataContext.SelectedBomItem != null && CurrentBomExportWindowDataContext.SelectedBomItem.ParentStructure != null)
                    CurrentBomExportWindowDataContext.SelectedBomItem.ParentStructure.Remove(CurrentBomExportWindowDataContext.SelectedBomItem);
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteResetBom()
        {
            try
            {
                RecursiveResetMainBom(CompleteBom, CurrentBomExportWindowDataContext.MainBom);
                UpdateSapCostVolumeInformation();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDownloadDrawing(string From = "Selected")
        {
            try
            {
                string partNumber = "";
                string partRevision = "";
                if (From == "Selected" && CurrentBomExportWindowDataContext.SelectedBomItem != null)
                {
                    partNumber = CurrentBomExportWindowDataContext.SelectedBomItem.Number;
                    partRevision = "Latest";
                }
                else if (From == "Main" && CurrentBomExportWindowDataContext.Number != null
                    && CurrentBomExportWindowDataContext.Number.Trim() != ""
                    && CurrentBomExportWindowDataContext.Revision != null
                    && CurrentBomExportWindowDataContext.Revision.Trim() != "")
                {
                    partNumber = CurrentBomExportWindowDataContext.Number;
                    partRevision = CurrentBomExportWindowDataContext.Revision;
                }
                else if (From == "Component" && CurrentBomExportWindowDataContext.Number != null
                    && CurrentBomExportWindowDataContext.Number.Trim() != ""
                    && CurrentBomExportWindowDataContext.Revision != null
                    && CurrentBomExportWindowDataContext.Revision.Trim() != "")
                {
                    partNumber = CurrentBomExportWindowDataContext.SelectedComponent.Number;
                    partRevision = "Latest";
                }

                bool redult = _wtDownloadViewableTools.DownloadPartMainDrawing(partNumber, partRevision, DocumentTypeEnum.PART, CurrentBomExportWindowDataContext.IsCreateZip);

                if (!redult)
                    MessageBox.Show(McgWpfTools.GetStringResource("BCE_DownloadDrawingNotAvailable"), McgWpfTools.GetStringResource("BCE_TitleDownloadDrawingNotAvailable"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartCumulativeMaterial()
        {
            try
            {
                if (CurrentBomExportWindowDataContext.MainBom.Count > 0)
                {
                    CumulativeBomFrom = "MATERIAL";
                    CurrentBomExportWindowDataContext.IsColMaterialShown = true;
                    CurrentBomExportWindowDataContext.IsColNameShown = false;
                    UpdateClassificationBom(CumulativeBomFrom);

                    _miscToolsWindchillService.ShowBomExportCumulativeView(this, true);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartCumulativeName()
        {
            try
            {
                if (CurrentBomExportWindowDataContext.MainBom.Count > 0)
                {
                    CumulativeBomFrom = "NAME";
                    CurrentBomExportWindowDataContext.IsColMaterialShown = false;
                    CurrentBomExportWindowDataContext.IsColNameShown = true;
                    CurrentBomExportWindowDataContext.ClassificationItemList.Clear();
                    UpdateClassificationBom(CumulativeBomFrom);

                    _miscToolsWindchillService.ShowBomExportCumulativeView(this, true);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartCumulativeBomExport()
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

        private void ExecuteCloseCumulativeBomExport()
        {
            try
            {
                bomExportCumulativeView?.Close();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCumulateInWorkNumber()
        {
            try
            {
                List<WindchillObjStructureComponent> ListComp;
                string StringRegex = "";
                if (CurrentBomExportWindowDataContext.IsStateInWork) StringRegex = $"{StringRegex}INWORK";
                if (CurrentBomExportWindowDataContext.IsStateObsolete) StringRegex = $"{StringRegex}|OSOLETE";
                if (CurrentBomExportWindowDataContext.IsStatePreReleased) StringRegex = $"{StringRegex}|PRERELEASED";
                if (CurrentBomExportWindowDataContext.IsStatePrototype) StringRegex = $"{StringRegex}|PROTOTYPE";
                if (CurrentBomExportWindowDataContext.IsStateReleased) StringRegex = $"{StringRegex}|RELEASED";
                if (CurrentBomExportWindowDataContext.IsStateRework) StringRegex = $"{StringRegex}|REWORK";
                if (CurrentBomExportWindowDataContext.IsStateSuperseded) StringRegex = $"{StringRegex}|SUPERSEDED";
                if (CurrentBomExportWindowDataContext.IsStateUnderReview) StringRegex = $"{StringRegex}|UNDERREVIEW";
                if (StringRegex != "")
                {
                    Regex RgCumulNumber = new Regex(StringRegex);
                    string Numbers = "";
                    if (AllComponent != null && AllComponent.Count > 0)
                    {
                        //ListComp = AllComponent.Where((item) => CurrentBomExportConfiguration.UnapprovedState.Contains(item.MainWindchillObject.State)).ToList();
                        ListComp = AllComponent.Where((item) => RgCumulNumber.IsMatch(item.MainWindchillObject.State)).ToList();

                        if (ListComp.Count > 0)
                        {
                            foreach (WindchillObjStructureComponent component in ListComp)
                            {
                                Numbers = $"{Numbers}{component.Number}*;";
                            }
                            Clipboard.SetText(Numbers);
                            MessageBox.Show($"{McgWpfTools.GetStringResource("BCE_MsgBoxCumulativeNumber")}");
                        }
                        else
                        {
                            MessageBox.Show($"{McgWpfTools.GetStringResource("BCE_MsgBoxNoneCumulativeNumber")}");
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"{McgWpfTools.GetStringResource("BCE_MsgBoxNoneCumulativeNumber")}");
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteToggleExpandCollapse(bool IsCollapsed)
        {
            try
            {
                foreach (var item in CurrentBomExportWindowDataContext.MainBom)
                    CollapseExpandRecursive(item, IsCollapsed);


            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods Update Data
        private void CollapseExpandRecursive(WindchillObjStructureComponent item, bool IsCollapsed)
        {
            try
            {
                item.IsExpanded = IsCollapsed;

                if (item.Structure != null)
                {
                    foreach (var child in item.Structure)
                    {
                        CollapseExpandRecursive(child, IsCollapsed);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void SearchBomAsynch()
        {
            try
            {
                WindchillObject CurrentWindchillObject = null;
                if (CurrentBomExportWindowDataContext.IsAssemblyChecked)
                {
                    CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg01");
                    RestOdataEpmDocument CurrentRestOdataEpmDocument;
                    if (CurrentBomExportWindowDataContext.IsLatestRevision || CurrentBomExportWindowDataContext.Revision == "LATEST")
                        CurrentRestOdataEpmDocument = _windchillEpmDocumentManagementService.GetOneEpmDocument(WindchillNetworkCredential.WindchillCredential, CurrentBomExportWindowDataContext.Number);
                    else
                        CurrentRestOdataEpmDocument = _windchillEpmDocumentManagementService.GetOneEpmDocument(WindchillNetworkCredential.WindchillCredential, CurrentBomExportWindowDataContext.Number, CurrentBomExportWindowDataContext.Revision);
                    if (CurrentRestOdataEpmDocument != null)
                        CurrentWindchillObject = _windchillRequestMiscService.GetWindchillEpmDocument(CurrentRestOdataEpmDocument);
                }

                else if (CurrentBomExportWindowDataContext.IsPartChecked)
                {
                    CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg02");
                    RestOdataWtPart CurrentRestOdataWtPart;
                    if (CurrentBomExportWindowDataContext.IsLatestRevision || CurrentBomExportWindowDataContext.Revision == "LATEST")
                        CurrentRestOdataWtPart = _windchillPartManagementService.GetOnePart(WindchillNetworkCredential.WindchillCredential, CurrentBomExportWindowDataContext.Number);
                    else
                        CurrentRestOdataWtPart = _windchillPartManagementService.GetOnePart(WindchillNetworkCredential.WindchillCredential, CurrentBomExportWindowDataContext.Number, CurrentBomExportWindowDataContext.Revision);

                    if (CurrentRestOdataWtPart != null)
                        CurrentWindchillObject = _windchillRequestMiscService.GetWindchillPart(CurrentRestOdataWtPart);
                }

                if (CurrentWindchillObject == null)
                {
                    CurrentBomExportWindowDataContext.IsSearchBomDone = true;
                    CurrentBomExportWindowDataContext.IsActionProgress = false;
                    MessageBox.Show(String.Format(McgWpfTools.GetStringResource("BCE_ObjectNotFound"), CurrentWindchillType, CurrentBomExportWindowDataContext.Number, CurrentBomExportWindowDataContext.Revision), "BOM Search Issue", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                }
                else
                {
                    UpperWindchillObject = CurrentWindchillObject;
                    if (CurrentBomExportWindowDataContext.Revision == "LATEST")
                        CurrentBomExportWindowDataContext.Revision = CurrentWindchillObject.Revision;

                    RawBom = null;
                    if (CurrentBomExportWindowDataContext.IsAssemblyChecked)
                        RawBom = SearchBomAsynchLevelByLevelEpmDoc();
                    else if (CurrentBomExportWindowDataContext.IsPartChecked)
                        RawBom = SearchBomAsynchLevelByLevelPart();

                    //  MainDispatcher.Invoke(UpdateMainBom);
                    MainDispatcher.Invoke(() => UpdateMainBom(true, true));
                    MainDispatcher.Invoke(() => UpdateAllComponent());
                }

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                CurrentBomExportWindowDataContext.IsActionProgress = false;
                CurrentBomExportWindowDataContext.IsSearchBomDone = true;
                CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg03");
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private WindchillObjStructureComponent SearchBomAsynchLevelByLevelPart()
        {
            try
            {
                WindchillNamingConvention NamingConvention = null;

                NamingConvention = _xmlSerializeTools.GetDeserializedXml<WindchillNamingConvention>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{CommonLibConstants.NamingConventionFile}");

                CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg04");
                int MaxLevel = CurrentBomExportWindowDataContext.BomLevel;
                int FirstLevel = 2 - (MaxLevel % 2);
                // switch back to one level by level with Windchill 12.0.2.16: not working if level sup to 1
                FirstLevel = 1;

                WindchillObjStructureComponent FinalStructure = _windchillBomManagementService.GetBomMultiLevelNamingConventionOneOccurrence(CurrentBomExportWindowDataContext.Number,
                                                                               CurrentBomExportWindowDataContext.Revision,
                                                                               CurrentWindchillType,
                                                                               FirstLevel,
                                                                               WindchillNetworkCredential.WindchillCredential, NamingConvention, true);

                RawBom = FinalStructure;
                MainDispatcher.Invoke(() => UpdateMainBom(false));

                List<WindchillObjStructureComponent> AllSearchedCompList = new List<WindchillObjStructureComponent>();
                List<WindchillObjStructureComponent> CompToBeSearchedList = new List<WindchillObjStructureComponent>();
                List<WindchillObjStructureComponent> CompLevelSearchedList = new List<WindchillObjStructureComponent>();

                CompLevelSearchedList.Add(FinalStructure);
                int StructureIndex = 0;
                Regex GlobalFastenerNumberRegex = new Regex(MiscToolsConstants.RegexPartNumberToExcludeBomSearch);
                //for (int CurrentLevel = FirstLevel + 1; CurrentLevel <= MaxLevel; CurrentLevel = CurrentLevel + 2)
                for (int CurrentLevel = FirstLevel + 1; CurrentLevel <= MaxLevel; CurrentLevel = CurrentLevel + 1)
                {
                    // Create list of Structure to search at this level
                    CompToBeSearchedList.Clear();
                    foreach (WindchillObjStructureComponent CurrentComp in CompLevelSearchedList)
                    {
                        if (CurrentComp.Structure != null)
                        {
                            foreach (WindchillObjStructureComponent comp in CurrentComp.Structure)
                            {
                                if (comp.Structure != null)
                                {
                                    foreach (WindchillObjStructureComponent lowerComp in comp.Structure)
                                    {
                                        if (!GlobalFastenerNumberRegex.IsMatch(lowerComp.Number) && AllSearchedCompList.FirstOrDefault((item) => item.Number == lowerComp.Number) == null)
                                            CompToBeSearchedList.Add(lowerComp);
                                        AllSearchedCompList.Add(comp);
                                    }
                                }
                                else if (!GlobalFastenerNumberRegex.IsMatch(comp.Number) && AllSearchedCompList.FirstOrDefault((item) => item.Number == comp.Number) == null)
                                    CompToBeSearchedList.Add(comp);
                            }
                        }
                    }

                    // Start Struture Search at this level
                    CompLevelSearchedList.Clear();
                    StructureIndex = 0;
                    foreach (WindchillObjStructureComponent CurrentComp in CompToBeSearchedList)
                    {
                        StructureIndex++;
                        //CurrentBomExportWindowDataContext.StatusBarMsg = $"BOM Level {CurrentLevel} in progress: Search struture {StructureIndex} on {CompToBeSearchedList.Count}";
                        CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg05", new string[3] { CurrentLevel.ToString(), StructureIndex.ToString(), CompToBeSearchedList.Count.ToString() });
                        WindchillObjStructureComponent TempStructure = AllSearchedCompList.FirstOrDefault((item) => item.Number == CurrentComp.Number);
                        if (TempStructure == null)
                        {
                            // switch back to one level by level with Windchill 12.0.2.16: not working if level sup to 1
                            TempStructure = _windchillBomManagementService.GetBomMultiLevelNamingConventionOneOccurrence(CurrentComp.Number,
                                                                                  CurrentComp.MainWindchillObject.Revision,
                                                                                  CurrentWindchillType,
                                                                                  1,
                                                                                  WindchillNetworkCredential.WindchillCredential, NamingConvention, true);
                            AllSearchedCompList.Add(TempStructure);

                            // Add nb Structure search.
                        }
                        if (TempStructure != null)
                        {
                            CurrentComp.Structure = new ObservableCollection<WindchillObjStructureComponent>();
                            foreach (var comp in TempStructure.Structure)
                            {
                                comp.BomLevel = CurrentLevel;
                                CurrentComp.Structure.Add(comp);
                                if (comp.Structure != null)
                                {
                                    foreach (var lowerComp in comp.Structure)
                                        lowerComp.BomLevel = CurrentLevel + 1;
                                }
                            }
                        }
                        CompLevelSearchedList.Add(CurrentComp);
                    }
                    // RawBom = FinalStructure;
                    MainDispatcher.Invoke(() => UpdateMainBom(false));
                }
                return FinalStructure;
            }
            catch (ThreadAbortException)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private WindchillObjStructureComponent SearchBomAsynchLevelByLevelEpmDoc()
        {
            try
            {
                WindchillNamingConvention NamingConvention = null;

                NamingConvention = _xmlSerializeTools.GetDeserializedXml<WindchillNamingConvention>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{CommonLibConstants.NamingConventionFile}");

                CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg04");

                WindchillObjStructureComponent FinalStructure = _windchillBomManagementService.GetBomMultiLevelNamingConventionOneOccurrence(CurrentBomExportWindowDataContext.Number,
                                                                               CurrentBomExportWindowDataContext.Revision,
                                                                               CurrentWindchillType,
                                                                               1,
                                                                               WindchillNetworkCredential.WindchillCredential, NamingConvention, true);

                RawBom = FinalStructure;
                MainDispatcher.Invoke(() => UpdateMainBom(false));

                List<WindchillObjStructureComponent> AllSearchedCompList = new List<WindchillObjStructureComponent>();
                List<WindchillObjStructureComponent> CompToBeSearchedList = new List<WindchillObjStructureComponent>();
                List<WindchillObjStructureComponent> CompLevelSearchedList = new List<WindchillObjStructureComponent>();

                int MaxLevel = CurrentBomExportWindowDataContext.BomLevel;
                CompLevelSearchedList.Add(FinalStructure);
                int StructureIndex = 0;
                for (int CurrentLevel = 2; CurrentLevel <= MaxLevel; CurrentLevel++)
                {
                    // Create list of Structure to search at this level
                    CompToBeSearchedList.Clear();
                    foreach (WindchillObjStructureComponent CurrentComp in CompLevelSearchedList)
                    {
                        if (CurrentComp.Structure != null)
                        {
                            foreach (WindchillObjStructureComponent comp in CurrentComp.Structure)
                            {
                                //comp.BomLevel = CurrentLevel;
                                if (CurrentWindchillType == WindchillObjectType.PART && AllSearchedCompList.FirstOrDefault((item) => item.Number == comp.Number) == null)
                                {
                                    CompToBeSearchedList.Add(comp);
                                }
                                else if (CurrentWindchillType == WindchillObjectType.ASM
                                    && AllSearchedCompList.FirstOrDefault((item) => item.Number == comp.Number) == null
                                    && comp.MainWindchillObject != null
                                    && comp.MainWindchillObject.Number != null
                                    && comp.MainWindchillObject.Number.ToUpper().Contains(".ASM"))
                                {
                                    CompToBeSearchedList.Add(comp);
                                }
                            }
                        }
                    }

                    // Start Struture Search at this level
                    CompLevelSearchedList.Clear();
                    StructureIndex = 0;
                    foreach (WindchillObjStructureComponent CurrentComp in CompToBeSearchedList)
                    {
                        StructureIndex++;
                        //CurrentBomExportWindowDataContext.StatusBarMsg = $"BOM Level {CurrentLevel} in progress: Search struture {StructureIndex} on {CompToBeSearchedList.Count}";
                        CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg05", new string[3] { CurrentLevel.ToString(), StructureIndex.ToString(), CompToBeSearchedList.Count.ToString() });
                        WindchillObjStructureComponent TempStructure = AllSearchedCompList.FirstOrDefault((item) => item.Number == CurrentComp.Number);
                        if (TempStructure == null)
                        {
                            TempStructure = _windchillBomManagementService.GetBomMultiLevelNamingConventionOneOccurrence(CurrentComp.Number,
                                                                                              CurrentComp.MainWindchillObject.Revision,
                                                                                              CurrentWindchillType,
                                                                                              1,
                                                                                              WindchillNetworkCredential.WindchillCredential, NamingConvention, true);
                            AllSearchedCompList.Add(TempStructure);

                            // Add nb Structure search.
                        }
                        if (TempStructure != null)
                        {
                            CurrentComp.Structure = new ObservableCollection<WindchillObjStructureComponent>();
                            foreach (var comp in TempStructure.Structure)
                            {
                                comp.BomLevel = CurrentLevel;
                                CurrentComp.Structure.Add(comp);
                            }
                        }
                        CompLevelSearchedList.Add(CurrentComp);
                    }

                    // RawBom = FinalStructure;
                    MainDispatcher.Invoke(() => UpdateMainBom(false));

                }


                return FinalStructure;
            }
            catch (ThreadAbortException)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void UpdateMainBom(bool LastUpdate = false, bool SortByRep = false)
        {
            try
            {
                if (RawBom != null)
                {
                    CurrentBomExportWindowDataContext.MainBom.Clear();
                    foreach (var item in RawBom.Structure)
                    {
                        item.Parent = RawBom;
                        CurrentBomExportWindowDataContext.MainBom.Add(item);
                    }

                    // purge occurences if not shown
                    if (!CurrentBomExportWindowDataContext.IsShowOccurrences)
                        RecursivePurgeOccuenrrences(CurrentBomExportWindowDataContext.MainBom, RawBom);

                    AllComponent.Clear();
                    RecursiveSearchAllObject(CurrentBomExportWindowDataContext.MainBom);

                    // Update Rep Value
                    RecursiveUpdateRepValue(CurrentBomExportWindowDataContext.MainBom);

                    // Update Classification info
                    RecursiveUpdateClassificationInfo(CurrentBomExportWindowDataContext.MainBom, 1);

                    CurrentBomExportWindowDataContext.MaxBomLevel = UpdateBomComponentParameter(CurrentBomExportWindowDataContext.MainBom);
                    if (CurrentBomExportWindowDataContext.MaxBomLevel < 1) CurrentBomExportWindowDataContext.MaxBomLevel = 1;

                    // Update Col size
                    UpdateColumnWidth();

                    // Search SAP info
                    if (CurrentBomExportWindowDataContext.ShowSapCostVolumeInfo && LastUpdate)
                    {
                        IsSapInformationSearched = false;
                        ThreadSearchSapInfo = new Thread(() => UpdateSapCostVolumeInformation());
                        ThreadSearchSapInfo.Start();
                    }
                }
                if (LastUpdate)
                {
                    CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg06");
                    CurrentBomExportWindowDataContext.IsSearchBomDone = true;
                    CurrentBomExportWindowDataContext.IsActionProgress = false;
                    foreach (var comp in CurrentBomExportWindowDataContext.MainBom)
                        CompleteBom.Add(comp);
                }

                if (SortByRep)
                {
                    BomExportParameter repParam = CurrentBomExportWindowDataContext.ListSelectedParameters.FirstOrDefault(item => item.ParamId == "REP");
                    if (repParam != null)
                        RecursiveBomSort(CurrentBomExportWindowDataContext.MainBom, repParam, false);
                }
            }
            catch (Exception ex)
            {
                CurrentBomExportWindowDataContext.IsActionProgress = false;
                CurrentBomExportWindowDataContext.IsSearchBomDone = true;
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateAllComponent()
        {
            try
            {
                var cumulCompQty = WindchillObjStructureComponent.GetCumulativeQuantities(new WindchillObjStructureComponent() { Structure = CurrentBomExportWindowDataContext.MainBom, Quantity = 1, Number = "UpperLv" });
                CurrentBomExportWindowDataContext.AllComponents.Clear();
                foreach (var comp in AllComponent)
                {
                    comp.CumulativeQuantity = cumulCompQty.FirstOrDefault(item => item.Key == comp.Number).Value;
                    CurrentBomExportWindowDataContext.AllComponents.Add(comp);
                }
            }
            catch (Exception ex)
            {
                CurrentBomExportWindowDataContext.IsActionProgress = false;
                CurrentBomExportWindowDataContext.IsSearchBomDone = true;
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void RecursiveResetMainBom(List<WindchillObjStructureComponent> Structure, ObservableCollection<WindchillObjStructureComponent> ShowStructure)
        {
            try
            {
                if (Structure != null && ShowStructure != null)
                {
                    ShowStructure.Clear();

                    foreach (var comp in Structure)
                    {
                        if (comp.RowParentStructure != null)
                            RecursiveResetMainBom(comp.RowParentStructure, comp.Structure);
                        ShowStructure.Add(comp);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void UpdateRepValue(object sender = null, EventArgs e = null)
        {
            try
            {
                RecursiveUpdateRepValue(CurrentBomExportWindowDataContext.MainBom);
                UpdateBomComponentParameter(CurrentBomExportWindowDataContext.MainBom);
                UpdateColumnWidth();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void RecursiveUpdateRepValue(ObservableCollection<WindchillObjStructureComponent> Structure)
        {
            try
            {
                foreach (var comp in Structure)
                {
                    if (comp.Structure != null && comp.Structure.Count > 0)
                        RecursiveUpdateRepValue(comp.Structure);
                    UpdateOneRep(comp);
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void RecursiveUpdateClassificationInfo(ObservableCollection<WindchillObjStructureComponent> Structure, double MassMultiplicator)
        {
            try
            {
                foreach (var comp in Structure)
                {
                    if (comp.MainWindchillObject != null)
                    {
                        comp.Material = comp.MainWindchillObject.Material;
                        comp.PtcCommonName = comp.MainWindchillObject.Name;
                        if (comp.Unit.ToUpper() == "EA")
                        {
                            comp.CumulativeMass = comp.MainWindchillObject.Mass * comp.Quantity * MassMultiplicator;
                            comp.CumulativeQuantity = comp.Quantity * MassMultiplicator;
                        }
                        else
                        {
                            comp.CumulativeMass = comp.MainWindchillObject.Mass * MassMultiplicator;
                            comp.CumulativeQuantity = MassMultiplicator;
                        }
                    }

                    if (comp.Structure != null && comp.Structure.Count > 0)
                    {
                        RecursiveUpdateClassificationInfo(comp.Structure, MassMultiplicator * comp.Quantity);
                        comp.IsEndItem = false;
                    }
                    else
                        comp.IsEndItem = true;

                    UpdateOneRep(comp);
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void UpdateOneRep(WindchillObjStructureComponent StructComp)
        {
            try
            {
                StructComp.REP_Orig = StructComp.REP;
                string TempLineNumber = StructComp.REP_Orig;
                int index;
                if (Int32.TryParse(StructComp.REP_Orig, out index))
                {
                    for (index = StructComp.REP_Orig.Length; index < CurrentBomExportWindowDataContext.NumericalLineNumberDigit; index++)
                        TempLineNumber = $"0{TempLineNumber}";
                }
                StructComp.REP = TempLineNumber;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void RecursivePurgeOccuenrrences(ObservableCollection<WindchillObjStructureComponent> Structure, WindchillObjStructureComponent parent = null)
        {
            try
            {
                // Purge Occurences
                List<WindchillObjStructureComponent> TempStructure = new List<WindchillObjStructureComponent>();
                WindchillObjStructureComponent TempBomItem = null;
                foreach (var comp in Structure)
                {
                    comp.Parent = parent;
                    TempBomItem = TempStructure.FirstOrDefault((item) => item.Number == comp.Number);
                    if (TempBomItem != null)
                        TempBomItem.Quantity += comp.Quantity;
                    else
                        TempStructure.Add(comp);
                }

                Structure.Clear();
                foreach (var comp in TempStructure)
                {
                    comp.ParentStructure = Structure;
                    Structure.Add(comp);
                }


                // purge lower level
                foreach (var comp in Structure)
                {
                    if (comp.Structure != null && comp.Structure.Count > 0)
                        RecursivePurgeOccuenrrences(comp.Structure, comp);
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private int UpdateBomComponentParameter(ObservableCollection<WindchillObjStructureComponent> Structure)
        {
            try
            {
                if (Structure == null || Structure.Count == 0) return 0;
                int MaxBomLevel = Structure.Max((item) => item.BomLevel); ;
                PropertyInfo CompPropertyInfo;
                PropertyInfo ColPropertyInfo;
                object temp;
                object tempObj;

                string[] BomAttributes = new string[3] { "REP", "Quantity", "Unit" };

                foreach (var comp in Structure)
                {
                    if (comp.Structure != null && comp.Structure.Count > 0)
                        MaxBomLevel = Math.Max(MaxBomLevel, UpdateBomComponentParameter(comp.Structure));

                    foreach (var param in CurrentBomExportWindowDataContext.ListSelectedParameters)
                    {
                        if (BomAttributes.Contains(param.ParamId))
                        {
                            CompPropertyInfo = comp.GetType().GetProperty(param.ParamId);
                            tempObj = comp;
                        }
                        else
                        {
                            CompPropertyInfo = comp.MainWindchillObject.GetType().GetProperty(param.ParamId);
                            tempObj = comp.MainWindchillObject;
                        }
                        ColPropertyInfo = comp.GetType().GetProperty($"ValueCol{param.Order}");
                        if (CompPropertyInfo != null && ColPropertyInfo != null)
                        {
                            temp = CompPropertyInfo.GetValue(tempObj);
                            if (temp != null) ColPropertyInfo.SetValue(comp, temp.ToString());
                            else ColPropertyInfo.SetValue(comp, "");
                        }
                        ColPropertyInfo = comp.GetType().GetProperty($"IsAPrice{param.Order}");
                        if (CompPropertyInfo != null && ColPropertyInfo != null)
                        {
                            ColPropertyInfo.SetValue(comp, param.IsAPrice);
                        }
                    }

                }

                return MaxBomLevel;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private int MaxCharacterColValue(ObservableCollection<WindchillObjStructureComponent> Structure, string NameCol, int InitSize = 10)
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

        private void RecursiveBomSort(ObservableCollection<WindchillObjStructureComponent> Structure, BomExportParameter CurrentParam, bool SortOrder = false)
        {
            try
            {
                if (Structure == null || Structure.Count == 0) return;

                // Search PropertyInfo to be sorted
                PropertyInfo CurrentProp;
                if (CurrentParam.ParamId == "Number")
                    CurrentProp = typeof(WindchillObjStructureComponent).GetProperty($"Number");
                else
                    CurrentProp = typeof(WindchillObjStructureComponent).GetProperty($"ValueCol{CurrentParam.Order}");

                if (CurrentProp != null)
                {
                    foreach (var comp in Structure.Where((comp) => comp.Structure != null && comp.Structure.Count > 0))
                        RecursiveBomSort(comp.Structure, CurrentParam, SortOrder);

                    List<WindchillObjStructureComponent> TempList;
                    if (!SortOrder)
                        TempList = Structure.OrderBy((item) => CurrentProp.GetValue(item)).ToList();
                    else
                        TempList = Structure.OrderByDescending((item) => CurrentProp.GetValue(item)).ToList();

                    if (TempList != null && TempList.Count == Structure.Count)
                    {
                        Structure.Clear();
                        foreach (var comp in TempList)
                            Structure.Add(comp);
                    }
                }

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
                if (CurrentBomExportWindowDataContext.MainBom != null && CurrentBomExportWindowDataContext.MainBom.Count > 0)
                {
                    Regex RegexProc = new Regex("Excel", RegexOptions.IgnoreCase);
                    List<Process> OldExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    string UserDocumentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string XlsFileName = $"{UserDocumentFolder}\\BOM_{CurrentBomExportWindowDataContext.Number}_{CurrentBomExportWindowDataContext.Revision}.xlsx";

                    ExcelToolsClosedXml CurrentExcel = new ExcelToolsClosedXml() { CompleteFileName = XlsFileName };
                    if (CurrentExcel.CreateNewFile("BOM") != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("BCE_ExportXlsIssue"), XlsFileName), "Excel Export Issue", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    List<Process> NewExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    Process newExcelProcess = NewExcelCurrentProcess.FirstOrDefault((proc) => !OldExcelCurrentProcess.ToList().Exists((oldprc) => proc.Id == oldprc.Id));

                    // Update Columns
                    CurrentExcel.CurrentSheet = "BOM";
                    CurrentExcel.SetCellValue(CurrentBomExportWindowDataContext.BomColumnLevel.ParamNameShown, 1, 1);
                    CurrentExcel.SetCellValue(CurrentBomExportWindowDataContext.BomColumnNumber.ParamNameShown, 1, 2);
                    int index = 3;
                    foreach (var col in CurrentBomExportWindowDataContext.ListSelectedParameters.OrderBy((item) => item.Order))
                    {
                        CurrentExcel.SetCellValue(col.ParamNameShown, 1, index);
                        index++;
                    }

                    // Update Structure
                    ObservableCollection<WindchillObjStructureComponent> TempBom = new ObservableCollection<WindchillObjStructureComponent>();
                    TempBom.Add(new WindchillObjStructureComponent()
                    {
                        Number = CurrentBomExportWindowDataContext.Number,
                        REP = "",
                        Quantity = 0,
                        Unit = "ea",
                        BomLevel = 0,
                        MainWindchillObject = UpperWindchillObject
                    });

                    foreach (var comp in CurrentBomExportWindowDataContext.MainBom)
                    {
                        TempBom.Add(comp);
                    }
                    RecursiveUpdteErpInformation(TempBom);
                    UpdateBomComponentParameter(TempBom);
                    RecursiveWriteStructureXls(CurrentExcel, TempBom);

                    // Update Tab components
                    CurrentExcel.CreateSheet("COMPONENTS");
                    CurrentExcel.CurrentSheet = "COMPONENTS";

                    // update column headers
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("BCE_ColHeader01"), 1, 1);
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("BCE_ColHeader06"), 1, 2);
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("BCE_ColHeader02"), 1, 3);
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("BCE_ColHeader11"), 1, 4);
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("BCE_ColHeader13"), 1, 5);
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("BCE_ColHeader14"), 1, 6);
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("BCE_ColHeader15"), 1, 7);

                    //Update components
                    index = 2;
                    foreach (var comp in CurrentBomExportWindowDataContext.AllComponents)
                    {
                        CurrentExcel.SetCellValue(comp.Number, index, 1);
                        CurrentExcel.SetCellValue(comp.MainWindchillObject.DescriptionEn, index, 2);
                        CurrentExcel.SetCellValue(comp.MainWindchillObject.Revision, index, 3);
                        CurrentExcel.SetCellValue(comp.CumulativeQuantity, index, 4);
                        CurrentExcel.SetCellValue(comp.MainWindchillObject.ErpStdCost, index, 5);
                        CurrentExcel.SetCellValue(comp.MainWindchillObject.ErpProvider, index, 6);
                        CurrentExcel.SetCellValue(comp.MainWindchillObject.ErpPriceMass, index, 7);

                        index++;
                    }

                    if (CurrentExcel.SaveClose() != ExcelStatus.OK)
                    {
                        MessageBox.Show(String.Format(McgWpfTools.GetStringResource("BCE_ExportXlsIssue"), XlsFileName), "Excel Export Issue", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return;
                    }

                    if (newExcelProcess != null)
                        newExcelProcess.Kill();

                    _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("BCE_BtBomExport"), String.Format(McgWpfTools.GetStringResource("BCE_ExportXls"), XlsFileName), McgWpfTools.GetStringResource("BCE_ToolTipOpen"), McgWpfTools.GetStringResource("BCE_ToolTipOpenFolder"), McgWpfTools.GetStringResource("BCE_ToolTipClose"), XlsFileName);
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private int RecursiveWriteStructureXls(ExcelToolsClosedXml CurrentExcel, ObservableCollection<WindchillObjStructureComponent> Structure, int Index = 2)
        {
            try
            {
                PropertyInfo CompPropertyInfo; ;
                int IndexCol = 3;
                foreach (var comp in Structure)
                {
                    IndexCol = 3;
                    if (CurrentBomExportWindowDataContext.IsLevelIndented)
                        CurrentExcel.SetCellValue($"{"".PadLeft((comp.BomLevel) * 2)}{comp.BomLevel}", Index, 1);
                    //CurrentExcel.SetCellValue($"{"".PadLeft((comp.BomLevel - 1) * 2)}{comp.BomLevel}", Index, 1);
                    else
                        CurrentExcel.SetCellValue(comp.BomLevel, Index, 1);
                    CurrentExcel.SetCellValue(comp.Number, Index, 2);
                    foreach (var param in CurrentBomExportWindowDataContext.ListSelectedParameters.OrderBy((item) => item.Order))
                    {
                        CompPropertyInfo = comp.GetType().GetProperty($"ValueCol{param.Order}");
                        if (param.ParamName == "Quantity" && CompPropertyInfo.GetValue(comp).ToString() == "0")
                            CompPropertyInfo.SetValue(comp, "");
                        if (CompPropertyInfo != null && CompPropertyInfo.GetValue(comp) != null)
                            CurrentExcel.SetCellValue(CompPropertyInfo.GetValue(comp).ToString(), Index, IndexCol);
                        IndexCol++;
                    }
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

        private void ExportBomToCsvFile()
        {
            try
            {
                if (CurrentBomExportWindowDataContext.MainBom != null && CurrentBomExportWindowDataContext.MainBom.Count > 0)
                {
                    string UserDocumentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string XlsFileName = $"{UserDocumentFolder}\\BOM_{CurrentBomExportWindowDataContext.Number}_{CurrentBomExportWindowDataContext.Revision}.csv";
                    char CsvSep = CurrentBomExportWindowDataContext.FieldSeparator;

                    string CsvText = $"{CurrentBomExportWindowDataContext.BomColumnLevel.ParamNameShown}{CsvSep}{CurrentBomExportWindowDataContext.BomColumnNumber.ParamNameShown}";

                    // Update Columns
                    foreach (var col in CurrentBomExportWindowDataContext.ListSelectedParameters.OrderBy((item) => item.Order))
                        CsvText = $"{CsvText}{CsvSep}{col.ParamNameShown}";

                    // Update Structure
                    CsvText = $"{CsvText}{RecursiveWriteStructureCsvFile(CurrentBomExportWindowDataContext.MainBom, CsvSep)}";

                    FileInfo NewExcelFile = new FileInfo(XlsFileName);
                    if (NewExcelFile.Exists)
                        NewExcelFile.Delete();

                    StreamWriter CurrentStream = new StreamWriter(XlsFileName);
                    CurrentStream.Write(CsvText);
                    CurrentStream.Close();

                    _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("BCE_BtBomExport"), String.Format(McgWpfTools.GetStringResource("BCE_ExportXls"), XlsFileName), McgWpfTools.GetStringResource("BCE_ToolTipOpen"), McgWpfTools.GetStringResource("BCE_ToolTipOpenFolder"), McgWpfTools.GetStringResource("BCE_ToolTipClose"), XlsFileName);
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private string RecursiveWriteStructureCsvFile(ObservableCollection<WindchillObjStructureComponent> Structure, char CsvSep)
        {
            try
            {
                PropertyInfo CompPropertyInfo; ;
                string CsvText = "";
                foreach (var comp in Structure)
                {
                    if (CurrentBomExportWindowDataContext.IsLevelIndented)
                        CsvText = $"{CsvText}\n{$"{"".PadLeft((comp.BomLevel - 1) * 2)}{comp.BomLevel}"}{CsvSep}{comp.Number}";
                    else
                        CsvText = $"{CsvText}\n{comp.BomLevel}{CsvSep}{comp.Number}";

                    foreach (var param in CurrentBomExportWindowDataContext.ListSelectedParameters.OrderBy((item) => item.Order))
                    {
                        CompPropertyInfo = comp.GetType().GetProperty($"ValueCol{param.Order}");
                        if (CompPropertyInfo != null && CompPropertyInfo.GetValue(comp) != null)
                            CsvText = $"{CsvText}{CsvSep}{CompPropertyInfo.GetValue(comp).ToString()}";
                        else
                            CsvText = $"{CsvText}{CsvSep}";

                    }

                    if (comp.Structure != null && comp.Structure.Count > 0)
                        CsvText = $"{CsvText}{RecursiveWriteStructureCsvFile(comp.Structure, CsvSep)}";
                }

                return CsvText;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void RecursiveSearchAllObject(ObservableCollection<WindchillObjStructureComponent> Structure)
        {
            try
            {
                foreach (var comp in Structure)
                {
                    if (!AllComponent.Exists((item) => item.Number == comp.Number)) AllComponent.Add(comp);

                    if (comp.Structure != null && comp.Structure.Count > 0)
                        RecursiveSearchAllObject(comp.Structure);
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void SearchEcnInformationAsynch(List<WindchillObjStructureComponent> ListBomItem)
        {
            try
            {
                CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg07");
                if (ListBomItem != null && ListBomItem.Count > 0)
                {
                    if (WindchillNetworkCredential == null)
                        WindchillNetworkCredential = _windchillCredentialService.GetWindchillCredential(CommonLibConstants.WindchillUrl);
                    foreach (var bomItem in ListBomItem)
                    {
                        bomItem.MainWindchillObject.EcnNumber = "None";
                        if (CurrentBomExportWindowDataContext.IsPartChecked)
                        {
                            List<RestOdataWtPart> CurrentList = _windchillReportingManagementService.GetQueryBuilderLinkPartEcnList<RestOdataWtPart>(WindchillNetworkCredential.WindchillCredential, bomItem.Number, bomItem.MainWindchillObject.Revision);
                            if (CurrentList != null && CurrentList.Count > 0)
                                bomItem.MainWindchillObject.EcnNumber = CurrentList.First().EcnNumber;
                        }
                        else
                        {
                            List<RestOdataEpmDocument> CurrentList = _windchillReportingManagementService.GetQueryBuilderLinkEpmDocEcnList<RestOdataEpmDocument>(WindchillNetworkCredential.WindchillCredential, bomItem.Number, bomItem.MainWindchillObject.Revision);
                            if (CurrentList != null && CurrentList.Count > 0)
                                bomItem.MainWindchillObject.EcnNumber = CurrentList.First().EcnNumber;
                        }
                    }

                    RecursiveUpdateEcnInformation(CurrentBomExportWindowDataContext.MainBom);

                    UpdateBomComponentParameter(CurrentBomExportWindowDataContext.MainBom);
                    UpdateColumnWidth();
                }
                CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg06");
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg08");
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentBomExportWindowDataContext.IsActionProgress = false;
            }
        }

        private void RecursiveUpdateEcnInformation(ObservableCollection<WindchillObjStructureComponent> Structure)
        {
            try
            {
                WindchillObjStructureComponent CurrentBomItem;
                foreach (var comp in Structure)
                {
                    CurrentBomItem = AllComponent.Where((item) => item.Number == comp.Number).FirstOrDefault();
                    if (CurrentBomItem != null)
                        comp.MainWindchillObject.EcnNumber = CurrentBomItem.MainWindchillObject.EcnNumber;
                    if (comp.Structure != null && comp.Structure.Count > 0)
                        RecursiveUpdateEcnInformation(comp.Structure);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void RecursiveSearchEndItem(ObservableCollection<WindchillObjStructureComponent> Structure)
        {
            try
            {
                foreach (var comp in Structure)
                {
                    if (comp.IsEndItem)
                        EndItemList.Add(comp);
                    if (comp.Structure != null && comp.Structure.Count > 0)
                        RecursiveSearchEndItem(comp.Structure);
                }
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void UpdateClassificationBom(string SortBy = "NAME")
        {
            try
            {
                EndItemList = new List<WindchillObjStructureComponent>();
                RecursiveSearchEndItem(CurrentBomExportWindowDataContext.MainBom);
                BomExportClassificationItem CurrentClassItem = null;
                CurrentBomExportWindowDataContext.ClassificationItemList.Clear();
                CurrentBomExportWindowDataContext.CumulativeEndItemMass = 0;
                foreach (var comp in EndItemList)
                {
                    CurrentClassItem = null;
                    switch (SortBy)
                    {
                        case "NAME":
                            CurrentClassItem = CurrentBomExportWindowDataContext.ClassificationItemList.FirstOrDefault(item => item.PtcCommonName == comp.PtcCommonName);
                            break;
                        case "MATERIAL":
                            CurrentClassItem = CurrentBomExportWindowDataContext.ClassificationItemList.FirstOrDefault(item => item.Material == comp.Material);
                            break;
                        default:
                            break;
                    }
                    if (CurrentClassItem == null)
                    {
                        CurrentClassItem = new BomExportClassificationItem()
                        {
                            Material = comp.Material,
                            CumulativeMass = comp.CumulativeMass,
                            CumulativeQuantity = comp.CumulativeQuantity,
                            PtcCommonName = comp.PtcCommonName

                        };
                        CurrentClassItem.ListItem.Add(comp);
                        CurrentBomExportWindowDataContext.ClassificationItemList.Add(CurrentClassItem);
                    }
                    else
                    {
                        CurrentClassItem.CumulativeMass += comp.CumulativeMass;
                        CurrentClassItem.CumulativeQuantity += comp.CumulativeQuantity;
                        CurrentClassItem.ListItem.Add(comp);
                    }
                    CurrentBomExportWindowDataContext.CumulativeEndItemMass += comp.CumulativeMass;
                }

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
                Regex RegexProc = new Regex("Excel", RegexOptions.IgnoreCase);
                List<Process> OldExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                string UserDocumentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string XlsFileName = $"{UserDocumentFolder}\\CUMUL_{CurrentBomExportWindowDataContext.Number}_{CurrentBomExportWindowDataContext.Revision}_{CumulativeBomFrom}.xlsx";

                ExcelToolsClosedXml CurrentExcel = new ExcelToolsClosedXml() { CompleteFileName = XlsFileName };
                if (CurrentExcel.CreateNewFile("MAIN") != ExcelStatus.OK)
                {
                    MessageBox.Show(String.Format(McgWpfTools.GetStringResource("BCE_ExportXlsIssue"), XlsFileName), "Excel Export Issue", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }

                List<Process> NewExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                Process newExcelProcess = NewExcelCurrentProcess.FirstOrDefault((proc) => !OldExcelCurrentProcess.ToList().Exists((oldprc) => proc.Id == oldprc.Id));

                // Update Columns
                CurrentExcel.CurrentSheet = "MAIN";
                if (CumulativeBomFrom == "NAME")
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("BCE_ColHeader08"), 1, 1);
                else
                    CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("BCE_ColHeader09"), 1, 1);

                CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("BCE_ColHeader10"), 1, 2);
                CurrentExcel.SetCellValue(McgWpfTools.GetStringResource("BCE_ColHeader11"), 1, 3);


                // Update Structure
                int index = 2;
                int index2 = 2;
                //Regex NonAlphaCaracRegex = new Regex(@"[\W]", RegexOptions.IgnoreCase);
                Regex NonAlphaCaracRegex = new Regex(@"[^a-zA-Z0-9_\- ]", RegexOptions.IgnoreCase);
                string CumulativeBomFromValue;
                foreach (var comp in CurrentBomExportWindowDataContext.ClassificationItemList)
                {
                    CurrentExcel.CurrentSheet = "MAIN";
                    if (comp.PtcCommonName == null || comp.PtcCommonName == "")
                        comp.PtcCommonName = "Unknown";
                    if (comp.Material == null || comp.Material == "")
                        comp.Material = "Unknown";

                    if (CumulativeBomFrom == "NAME")
                        CumulativeBomFromValue = NonAlphaCaracRegex.Replace(comp.PtcCommonName, @"");
                    else
                        CumulativeBomFromValue = NonAlphaCaracRegex.Replace(comp.Material, @"");



                    CurrentExcel.SetCellValue(CumulativeBomFromValue, index, 1);
                    CurrentExcel.SetCellValue(comp.CumulativeMass, index, 2);
                    CurrentExcel.SetCellValue(comp.CumulativeQuantity, index, 3);

                    // Update one Sheet for list of items
                    index2 = 2;
                    CurrentExcel.CreateSheet(CumulativeBomFromValue);
                    CurrentExcel.CurrentSheet = CumulativeBomFromValue;
                    CurrentExcel.SetCellValue("NUMBER", 1, 1);
                    CurrentExcel.SetCellValue("PTC_COMMON_NAME", 1, 2);
                    CurrentExcel.SetCellValue("DESCRIPTION 2 EN", 1, 3);
                    CurrentExcel.SetCellValue("DESCRIPTION 1 LOCAL", 1, 4);
                    CurrentExcel.SetCellValue("DESCRIPTION 2 LOCAL", 1, 5);
                    CurrentExcel.SetCellValue("MATERIAL", 1, 6);
                    CurrentExcel.SetCellValue("MASS", 1, 7);
                    CurrentExcel.SetCellValue("QUANTITY", 1, 8);
                    foreach (var item in comp.ListItem)
                    {
                        CurrentExcel.SetCellValue(item.MainWindchillObject.Number, index2, 1);
                        CurrentExcel.SetCellValue(item.MainWindchillObject.Name, index2, 2);
                        CurrentExcel.SetCellValue(item.MainWindchillObject.DescriptionEn2, index2, 3);
                        CurrentExcel.SetCellValue(item.MainWindchillObject.DescriptionLocal1, index2, 4);
                        CurrentExcel.SetCellValue(item.MainWindchillObject.DescriptionLocal2, index2, 5);
                        CurrentExcel.SetCellValue(item.MainWindchillObject.Material, index2, 6);
                        CurrentExcel.SetCellValue(item.MainWindchillObject.Mass, index2, 7);
                        CurrentExcel.SetCellValue(item.CumulativeQuantity, index2, 8);
                        index2++;
                    }

                    index++;
                }

                if (CurrentExcel.SaveClose() != ExcelStatus.OK)
                {
                    MessageBox.Show(String.Format(McgWpfTools.GetStringResource("BCE_ExportXlsIssue"), XlsFileName), "Excel Export Issue", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }

                if (newExcelProcess != null)
                    newExcelProcess.Kill();

                _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("BCE_BtBomExport"), String.Format(McgWpfTools.GetStringResource("BCE_ExportXls"), XlsFileName), McgWpfTools.GetStringResource("BCE_ToolTipOpen"), McgWpfTools.GetStringResource("BCE_ToolTipOpenFolder"), McgWpfTools.GetStringResource("BCE_ToolTipClose"), XlsFileName);

            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void SearchAllComponents(List<WindchillObjStructureComponent> Structure)
        {
            try
            {

                var result = Structure
                 .GroupBy(c => c.Number)
                 .Select(g =>
                 {
                     var first = g.First(); // première occurrence
                     return new BomComponent
                     {
                         Number = first.Number,
                         Quantity = g.Sum(c => c.Quantity),
                         Description = $"{first.MainWindchillObject.Name}|{first.MainWindchillObject.DescriptionLocal1}",
                         //StandardPrice = first.StandardPrice,
                         Revision = first.MainWindchillObject.Revision,
                         //Supplier = first.Supplier,
                     };
                 })
                 .ToList();
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods Search SAP Information
        private void UpdateSapCostVolumeInformation()
        {
            try
            {

                CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg09");

                CurrentBomExportWindowDataContext.IsMsgSearchSap = true;

                // Search SAP Information if not done yet
                if (!IsSapInformationSearched)
                {
                    List<string> AllMaterial = AllComponent.Select((item) => item.GetSapNumber()).ToList();
                    AllMaterial.Add(CurrentBomExportWindowDataContext.Number);

                    var tmpSapCostVolumeInfos = _sapHupService.GetListMaterialMasterCostVolumeInfo(AllMaterial);
                    CurrentAllCostVolume = new List<SapCostVolumeInfo>();
                    if (tmpSapCostVolumeInfos != null)
                        CurrentAllCostVolume.AddRange(tmpSapCostVolumeInfos.Select(x => new SapCostVolumeInfo(x)).ToList());
                }

                IsSapInformationSearched = true;

                // Update all components with SAP information
                RecursiveUpdteErpInformation(CurrentBomExportWindowDataContext.MainBom);
                CurrentBomExportWindowDataContext.IsMsgSearchSap = false;
                MainDispatcher.Invoke(InvokeReorderParameter);
                // Update Main 
                if (CurrentAllCostVolume != null)
                {
                    SapCostVolumeInfo MainSapCost = CurrentAllCostVolume.FirstOrDefault((item) => item.MaterialMasterNumber.ToUpper() == CurrentBomExportWindowDataContext.Number.ToUpper() && item.PlantNumber.Number == CurrentBomExportWindowDataContext.SelectedSapPlant.Number);
                    if (MainSapCost != null)
                    {
                        CurrentBomExportWindowDataContext.MainSapCost = Math.Round(McgBusinessTools.GetCurrencyFromUsd(McgBusinessTools.GetCurrencyToUsd(MainSapCost.StdCost, GetPlantCurrency(CurrentBomExportWindowDataContext.SelectedSapPlant)), CurrentBomExportWindowDataContext.SelectedSapPlant.Currency), 2);
                        CurrentBomExportWindowDataContext.MainSapProvider = MainSapCost.ProcurementType;
                    }
                    else
                    {
                        CurrentBomExportWindowDataContext.MainSapCost = 0;
                        CurrentBomExportWindowDataContext.MainSapProvider = "Unknown";
                    }
                }
                CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg06");
            }
            catch (ThreadAbortException)
            {
                // Can happen when switching or applying filter
                // Seems not raise other issue so just catch to avoid error messages
            }
            catch (InvalidOperationException)
            {
                // Can happen when switching or applying filter
                // Seems not raise other issue so just catch to avoid error messages
            }
            catch (Exception ex)
            {
                CurrentBomExportWindowDataContext.StatusBarMsg = McgWpfTools.GetStringResource("BCE_StBarMsg10");
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentBomExportWindowDataContext.IsMsgSearchSap = false;
            }
        }

        private void RecursiveUpdteErpInformation(ObservableCollection<WindchillObjStructureComponent> Structure)
        {
            try
            {
                SapCostVolumeInfo CurrentSapCostVolInfo = null;
                foreach (var comp in Structure)
                {
                    if (comp.Structure != null && comp.Structure.Count > 0)
                        RecursiveUpdteErpInformation(comp.Structure);
                    if (CurrentBomExportWindowDataContext.SelectedSapPlant != null)
                    {
                        if (CurrentAllCostVolume != null)
                            CurrentSapCostVolInfo = CurrentAllCostVolume.FirstOrDefault((item) => item.MaterialMasterNumber == comp.GetSapNumber() && item.PlantNumber.Number == CurrentBomExportWindowDataContext.SelectedSapPlant.Number);
                        if (CurrentSapCostVolInfo != null)
                        {
                            comp.MainWindchillObject.ErpProvider = CurrentSapCostVolInfo.ProcurementType;
                            comp.MainWindchillObject.ErpStdCost = CurrentSapCostVolInfo.StdCost;
                            comp.MainWindchillObject.ErpStdCost = Math.Round(McgBusinessTools.GetCurrencyFromUsd(McgBusinessTools.GetCurrencyToUsd(CurrentSapCostVolInfo.StdCost, GetPlantCurrency(CurrentBomExportWindowDataContext.SelectedSapPlant)), CurrentBomExportWindowDataContext.SelectedSapPlant.Currency), 2);
                            if (comp.MainWindchillObject.Mass > 0)
                                comp.MainWindchillObject.ErpPriceMass = Math.Round(comp.MainWindchillObject.ErpStdCost / comp.MainWindchillObject.Mass, 2);
                            else
                                comp.MainWindchillObject.ErpPriceMass = 0;

                        }
                        else
                        {
                            comp.MainWindchillObject.ErpProvider = "";
                            comp.MainWindchillObject.ErpStdCost = 0;
                            comp.MainWindchillObject.ErpPriceMass = 0;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private McgCurrency GetPlantCurrency(SapPlant plantNumber)
        {
            try
            {
                return CurrentBomExportConfiguration.ListSapPlant.FirstOrDefault((item) => item.Number == plantNumber.Number).Currency;
            }
            catch (Exception)
            {
                return McgCurrency.USD;
            }

        }
        #endregion

    }

}
