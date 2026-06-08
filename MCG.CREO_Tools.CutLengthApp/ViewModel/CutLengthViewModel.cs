using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Wordprocessing;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.DataBaseAccess.Services;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WebtermLib.Services;
using MCG.CommonLib.WebtermLib.Services.Interfaces;
using MCG.CREO_Tools.CutLengthApp.Configuration;
using MCG.CREO_Tools.CutLengthApp.Exceptions;
using MCG.CREO_Tools.CutLengthApp.Interfaces;
using MCG.CREO_Tools.CutLengthApp.View;
using pfcls;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.CutLengthApp.ViewModel
{
    public class CutLengthViewModel : ObservableObject, ICutLengthViewModel
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
        private CutLenghtDataContext _CurrentDataContent;
        public CutLenghtDataContext CurrentDataContext
        {
            get { return _CurrentDataContent; }
            set
            {
                if (this._CurrentDataContent != value)
                {
                    this._CurrentDataContent = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private MCGLanguage CurrentMcgLanguage { get; set; } = McgMiscTools.GetPropertiesFromMainApp<MCGLanguage>("MCGLANGUAGE");
        private string CryptedLogin { get; set; }
        private string CryptedPassWord { get; set; }
        private string CryptedLoginUpdate { get; set; }
        private string CryptedPassWordUpdate { get; set; }
        #endregion

        #region [REGION] Commands
        public ICommand CommandInsertCutLength { get => new RelayCommand(() => ExecuteInsertCutLength()); }
        public ICommand CommandUpdateCutLength { get => new RelayCommand(() => ExecuteUpdateCutLength()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        public ICommand CommandOpenTemplate { get => new RelayCommand(() => ExecuteOpenTemplate()); }
        public ICommand CommandUpdateActiveCadModel { get => new RelayCommand(() => ExecuteUpdateActiveCadModel()); }
        public ICommand CommandEditPartNumber { get => new RelayCommand(() => ExecuteEditPartNumber()); }
        public ICommand CommandAddNewPartNumber { get => new RelayCommand<bool>((b) => ExecuteAddNewPartNumber(b)); }
        #endregion

        #region [REGION] Init
        private readonly ICutLengthWindchillService _cutLengthWindchillService;
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        private readonly IUserAuthorizationService _userAuthorizationService;
        private readonly ICutLengthAppService _cutLengthAppService;
        private readonly IMcgToolDictionary _mcgToolDictionary;

        public CutLengthViewModel(ICutLengthWindchillService cutLengthWindchillService,
                                  ICreoSessionProvider creoSessionProvider,
                                  ICreoModelService creoModelService,
                                  IUserAuthorizationService userAuthorizationService,
                                  ICutLengthAppService cutLengthAppService,
                                  IMcgToolDictionary mcgToolDictionary)
        {
            try
            {
                _cutLengthWindchillService = cutLengthWindchillService;
                _creoSessionProvider = creoSessionProvider;
                _userAuthorizationService = userAuthorizationService;
                _creoModelService = creoModelService;
                _cutLengthAppService = cutLengthAppService;
                _mcgToolDictionary = mcgToolDictionary;

                CurrentDataContext = new CutLenghtDataContext();

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDataContext.IsCreoEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDataContext.IsCreoEnable = e;

                if (CurrentMcgLanguage != null)
                    CurrentMcgLanguage.ChangeLanguageInterface += UpdateInterfaceLanguage;

                //CurrentMCGTranslation = new McgToolDictionary();

                UpdateListClass();

                CurrentDataContext.IsAdminToolsEnabled = CheckUserAuthorization(CutLengthAppConstants.KeyUserUpdateAppName);

            }
            catch (Exception ex)
            {
                throw new CutLengthException(this.GetType().Name, ex);
            }
        }

        public void InitApp()
        {
            throw new NotImplementedException();
        }

        public bool CheckUserAuthorization(string AppName)
        {
            try
            {
                if (AppName == null) AppName = "";

                return _userAuthorizationService.GetIsAppCadAdmin(Environment.UserName.ToUpper(), AppName.ToUpper());
                //using (var CurrentEntity = GetDataBaseEntity(false))
                //{
                //    return CurrentEntity.APPAUTHORIZATION.Any((item) => item.USERID.Trim().ToUpper() == Environment.UserName.ToUpper()
                //                                                         && (item.APPNAME.ToUpper() == "ALL"
                //                                                         || item.APPNAME.ToUpper() == AppName.ToUpper()));
                //}
            }
            catch (Exception ex)
            {
                throw new CutLengthException(this.GetType().Name, ex);
            }
        }

        private void UpdateInterfaceLanguage(object sender = null, EventArgs e = null)
        {
            try
            {
                UpdateListClass();
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteInsertCutLength()
        {
            try
            {
                if (CurrentDataContext.SelectedCutLengthPart != null)
                    InsertCutLength();
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("CT_MsgSelectItem"),
                            McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateCutLength()
        {
            try
            {
                UpdateBulkQuantity();
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenHelp()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("CT_LinkHelpCutLength"));
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenTemplate()
        {
            try
            {
                EPMDocument CurrentEpm = new EPMDocument(CurrentDataContext.SelectedCutLengthPart.PartNumber, 
                                                         $"{CurrentDataContext.SelectedCutLengthPart.PartNumber}.{CurrentDataContext.SelectedCutLengthPart.CadDocType}", 
                                                         CurrentDataContext.SelectedCutLengthPart.PartNumber);

                CurrentEpm.OpenInCreo(_creoSessionProvider, _creoModelService);
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateActiveCadModel()
        {
            try
            {
                CurrentDataContext.ActiveModelFileName = McgWpfTools.GetStringResource("CT_LabelNoActiveModel");
                if ( CurrentDataContext.IsCreoEnable )
                {
                    CurrentDataContext.ActiveModel = _creoModelService.GetActiveModel();
                    if (CurrentDataContext.ActiveModel != null)
                        CurrentDataContext.ActiveModelFileName = CurrentDataContext.ActiveModel.FileName;
                }
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteEditPartNumber()
        {
            try
            {
                CutLengthCutPart CurrentPart = CurrentDataContext.SelectedCutLengthPart;
                CurrentPart.UpdatedPart = new CutLengthCutPart();
                CutLengthCutPart.UpdateCutLengthCutPart(CurrentPart.UpdatedPart, CurrentPart);
                bool IsCrationOk = false;

                while (!IsCrationOk)
                {
                    var returnValue = _cutLengthWindchillService.ShowDialogCutLengthCutUpdatePartView(CurrentPart);

                    if (returnValue == MessageBoxResult.Yes)
                    {

                        if (CurrentPart.UpdatedPart.PartNumber != null && CurrentPart.UpdatedPart.PartNumber.Trim() != "")
                        {
                            CutLengthCutPart.UpdateCutLengthCutPart(CurrentPart, CurrentPart.UpdatedPart);

                            Cutlengthpart DbPart = _cutLengthAppService.GetOneCutLengthPartForUpdate(CurrentPart.OrigPartNumber, CurrentPart.IdClass);
                                if (DbPart != null)
                                {
                                    CutLengthCutPart.UpdateCutLengthCutPart(DbPart, CurrentPart.UpdatedPart);
                                    _cutLengthAppService.SaveChanges();
                                    IsCrationOk = true;
                                    string CurrentClass = CurrentDataContext.SelectedCutLengthType.IdClass;
                                    UpdateListClass();
                                    CurrentDataContext.SelectedCutLengthType = CurrentDataContext.ListCutLengthType.FirstOrDefault(item => item.IdClass == CurrentClass);
                                    CurrentDataContext.UdpateCurrentListPartNumber();
                                }
                        }
                        else
                        {
                            MessageBox.Show(McgWpfTools.GetStringResource("CT_ErrorMsgNumberBlank"), McgWpfTools.GetStringResource("CT_ErrorMsgTitleIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                        IsCrationOk = true;

                }
            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddNewPartNumber(bool FromSelected = false)
        {
            try
            {
                if (CurrentDataContext.SelectedCutLengthType != null)
                {
                    CutLengthCutPart CurrentPart = new CutLengthCutPart();


                    if (FromSelected)
                        CutLengthCutPart.UpdateCutLengthCutPart(CurrentPart, CurrentDataContext.SelectedCutLengthPart);
                    else
                    {
                        CurrentPart.IdClass = CurrentDataContext.SelectedCutLengthType.IdClass;
                        CurrentPart.CadDocType = "PRT";
                    }

                    CurrentPart.UpdatedPart = new CutLengthCutPart();
                    CutLengthCutPart.UpdateCutLengthCutPart(CurrentPart.UpdatedPart, CurrentPart);

                    bool IsCrationOk = false;

                    while (!IsCrationOk)
                    {
                        var returnValue = _cutLengthWindchillService.ShowDialogCutLengthCutUpdatePartView(CurrentPart);
                        if (returnValue == MessageBoxResult.Yes)
                        {
                            if (CurrentPart.UpdatedPart.PartNumber != null && CurrentPart.UpdatedPart.PartNumber.Trim() != "")
                            {
                                CutLengthCutPart.UpdateCutLengthCutPart(CurrentPart, CurrentPart.UpdatedPart);
                                Cutlengthpart DbPart = CutLengthCutPart.GetCutLengthCutPart(CurrentPart);

                                bool isPartCreated = _cutLengthAppService.AddCutLengthPart(DbPart);
                                if (!isPartCreated)
                                    MessageBox.Show(McgWpfTools.GetStringResource("CT_ErrorMsgPartExist"), McgWpfTools.GetStringResource("CT_ErrorMsgTitlePartExist"), MessageBoxButton.OK, MessageBoxImage.Warning);
                                else
                                {           
                                    IsCrationOk = true;
                                    string CurrentClass = CurrentDataContext.SelectedCutLengthType.IdClass;
                                    UpdateListClass();
                                    CurrentDataContext.SelectedCutLengthType = CurrentDataContext.ListCutLengthType.FirstOrDefault(item => item.IdClass == CurrentClass);
                                    CurrentDataContext.UdpateCurrentListPartNumber();
                                }
                            }
                            else
                            {
                                MessageBox.Show(McgWpfTools.GetStringResource("CT_ErrorMsgNumberBlank"), McgWpfTools.GetStringResource("CT_ErrorMsgTitleIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                            IsCrationOk = true;

                    }
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("CT_ErrorMsgClassNotSelected"), McgWpfTools.GetStringResource("CT_ErrorMsgTitleIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);

            }
            catch (Exception ex)
            {
                CutLengthException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Read/update information in SQL Server DataBase
        private void UpdateListClass()
        {
            try
            {

                CutLengthType CurrentCutLengthType = null;

                List<CutLengthType> TempList = new List<CutLengthType>();
                foreach (var type in _cutLengthAppService.GetAllCutLengthClass())
                {
                    CurrentCutLengthType = CutLengthType.GetCutLengthType(type);
                    CurrentCutLengthType.ClassNameShown = _mcgToolDictionary.GetTerm(type.Classname);
                    TempList.Add(CurrentCutLengthType);
                }

                CurrentDataContext.ListCutLengthType.Clear();
                foreach (var type in TempList.OrderBy((item) => item.ClassNameShown))
                    CurrentDataContext.ListCutLengthType.Add(type);

                CurrentDataContext.CompleteListPartNumber = new List<CutLengthCutPart>();
                foreach (var part in _cutLengthAppService.GetAllCutLengthPart())
                    CurrentDataContext.CompleteListPartNumber.Add(CutLengthCutPart.GetCutLengthCutPart(part));

                if (CurrentDataContext.CompleteListPartNumber == null)
                    CurrentDataContext.CompleteListPartNumber = new List<CutLengthCutPart>();
            }
            catch (Exception ex)
            {
                throw new CutLengthException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Read/update information in SQL Server DataBase
        private void InsertCutLength()
        {
            try
            {
                if (CurrentDataContext.SelectedCutLengthPart != null)
                {
                    if (CurrentDataContext.ActiveModel != null && CurrentDataContext.ActiveModelFileName.ToUpper().Contains(".ASM"))
                    {
                        CREOModelStatus CurrentCREOModelStatus = _creoModelService.GetModelStatus(CurrentDataContext.ActiveModel);
                        if (CurrentCREOModelStatus == CREOModelStatus.READONLYITEM || CurrentCREOModelStatus == CREOModelStatus.READONLYWORKSPACE)
                            MessageBox.Show(McgWpfTools.GetStringResource("CT_ErrorMsgCantModify"),
                                McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning,
                                MessageBoxResult.OK);

                        else if (!CurrentDataContext.BulkSelected && !CurrentDataContext.ThreeDSelected)
                            MessageBox.Show(McgWpfTools.GetStringResource("CT_MsgNothingInsertSelected"),
                                McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                                MessageBoxButton.OK,
                                MessageBoxImage.Information,
                                MessageBoxResult.OK);
                        else
                        {
                            IpfcFeature CL3DFeature = null;
                            IpfcFeature CLBulkFeature = null;
                            if (CurrentDataContext.BulkSelected)
                                CLBulkFeature = InsertBulkItem();
                            if (CurrentDataContext.ThreeDSelected)
                            {
                                CurrentDataContext.CurrentCutLenghtFileName = GetNextCutLengthFileName();
                                if (CurrentDataContext.CurrentCutLenghtFileName.Length <= CutLengthAppConstants.MaxCadNameLength)
                                    CL3DFeature = Insert3DPart();
                                else
                                    MessageBox.Show(McgWpfTools.GetStringResource("CT_Msg3DNameTooLong"),
                                                    McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                                                    MessageBoxButton.OK,
                                                    MessageBoxImage.Warning,
                                                    MessageBoxResult.OK);
                            }

                            if (CurrentDataContext.BulkSelected && CurrentDataContext.ThreeDSelected && CLBulkFeature != null && CL3DFeature != null)
                            {
                                List<IpfcFeature> FeatureList = new List<IpfcFeature>();
                                FeatureList.Add(CL3DFeature);
                                FeatureList.Add(CLBulkFeature);
                                CreateFeatureGroup(FeatureList, CurrentDataContext.CurrentCutLenghtFileName);
                            }

                        }
                    }
                    else
                        MessageBox.Show(McgWpfTools.GetStringResource("CT_MsgNotAnAssembly"),
                            McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Information,
                            MessageBoxResult.OK);
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("CT_MsgNoPartSelected"),
                        McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK);
            }
            catch (Exception ex)
            {
                throw new CutLengthException(this.GetType().Name, ex);
            }
        }

        private void CreateFeatureGroup(List<IpfcFeature> featureList, string currentCutLenghtFileName)
        {
            try
            {
                if (featureList != null)
                {
                    CpfcFeatures grpFeature = new CpfcFeatures();
                    foreach (var feature in featureList)
                        grpFeature.Append(feature);
                    ((IpfcSolid)CurrentDataContext.ActiveModel).CreateLocalGroup(grpFeature, currentCutLenghtFileName);
                }
            }
            catch (Exception ex)
            {
                throw new CutLengthException(this.GetType().Name, ex);
            }
        }

        private IpfcFeature Insert3DPart()
        {
            try
            {
                IpfcFeature NewIpfcFeature = null;
                IpfcModel CL3DModel = null;
                _creoSessionProvider.SetConfigOption("let_proe_rename_pdm_objects", "yes");

                try
                {
                    CL3DModel = _creoModelService.RetrieveModel(CurrentDataContext.SelectedCutLengthPart.PartNumber, EpfcModelType.EpfcMDL_PART);
                }
                catch (Exception)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("CT_MsgIssueOpen3D"),
                        McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK);
                }

                if (CL3DModel != null)
                {
                    //CL3DModel.Rename(CurrentDataContext.CurrentCutLenghtFileName, true);
                    var CopyCL3DModel = CL3DModel.CopyAndRetrieve(CurrentDataContext.CurrentCutLenghtFileName, null);
                    NewIpfcFeature = ((IpfcAssembly)CurrentDataContext.ActiveModel).AssembleComponent((IpfcSolid)CopyCL3DModel, null);
                }

                _creoSessionProvider.SetConfigOption("let_proe_rename_pdm_objects", "no");
                return NewIpfcFeature;
            }
            catch (Exception ex)
            {
                throw new CutLengthException(this.GetType().Name, ex);
            }
            finally
            {
                _creoSessionProvider.SetConfigOption("let_proe_rename_pdm_objects", "no");
            }
        }

        private string GetNextCutLengthFileName()
        {
            try
            {
                string CutLengthSuffix = CutLengthAppConstants.CutLengthSuffix;
                int index = 1;
                bool lastIndexFound = false;
                string NextCutLengthFileName = "NOTFOUND";
                string UpperAsmNumber = CurrentDataContext.ActiveModelFileName.Substring(0, CurrentDataContext.ActiveModelFileName.Length - 4);
                _creoModelService.SearchModelsInSession();

                while (!lastIndexFound)
                {
                    if (index < 10)
                    {
                        NextCutLengthFileName = $"{UpperAsmNumber}_{CutLengthSuffix}0{index}_{CurrentDataContext.SelectedCutLengthPart.PartNumber}.PRT".ToUpper();
                        var tempModel = _creoModelService.GetListModels().FirstOrDefault((epm) => ((IpfcModel)epm).FileName.ToUpper() == NextCutLengthFileName);

                        if (tempModel == null)
                        {
                            NextCutLengthFileName = $"{UpperAsmNumber}_{CutLengthSuffix}0{index}_{CurrentDataContext.SelectedCutLengthPart.PartNumber}.ASM".ToUpper();
                            tempModel = _creoModelService.GetListModels().FirstOrDefault((epm) => ((IpfcModel)epm).FileName.ToUpper() == NextCutLengthFileName);

                            if (tempModel == null)
                            {
                                lastIndexFound = true;
                                NextCutLengthFileName = $"{UpperAsmNumber}_{CutLengthSuffix}0{index}_{CurrentDataContext.SelectedCutLengthPart.PartNumber}".ToUpper();
                            }
                        }
                    }
                    else
                    {
                        NextCutLengthFileName = $"{UpperAsmNumber}_{CutLengthSuffix}{index}_{CurrentDataContext.SelectedCutLengthPart.PartNumber}.PRT".ToUpper();
                        var tempModel = _creoModelService.GetListModels().FirstOrDefault((epm) => ((IpfcModel)epm).FileName.ToUpper() == NextCutLengthFileName);

                        if (tempModel == null)
                        {
                            NextCutLengthFileName = $"{UpperAsmNumber}_{CutLengthSuffix}{index}_{CurrentDataContext.SelectedCutLengthPart.PartNumber}.ASM".ToUpper();
                            tempModel = _creoModelService.GetListModels().FirstOrDefault((epm) => ((IpfcModel)epm).FileName.ToUpper() == NextCutLengthFileName);

                            if (tempModel == null)
                            {
                                lastIndexFound = true;
                                NextCutLengthFileName = $"{UpperAsmNumber}_{CutLengthSuffix}{index}_{CurrentDataContext.SelectedCutLengthPart.PartNumber}".ToUpper();
                            }
                        }
                    }

                    index++;
                    if (index > 99)
                    {
                        NextCutLengthFileName = "NOTFOUND";
                        lastIndexFound = true;
                    }
                }

                return NextCutLengthFileName;
            }
            catch (Exception ex)
            {
                throw new CutLengthException(this.GetType().Name, ex);
            }
        }

        private IpfcFeature InsertBulkItem()
        {
            try
            {
                IpfcFeature NewIpfcFeature = null;
                IpfcModel BulkModel = null;

                BulkModel = _creoModelService.RetrieveModel($"{CurrentDataContext.SelectedCutLengthPart.PartNumber}_BULK.PRT", EpfcModelType.EpfcMDL_PART);
                NewIpfcFeature = ((IpfcAssembly)CurrentDataContext.ActiveModel).AssembleComponent((IpfcSolid)BulkModel, null);

                // Update BOM_REPORT_QUANTITY
                CMpfcModelItem aCMpfcModelItem = new CMpfcModelItem();
                IpfcParamValue NewIpfParamValue = aCMpfcModelItem.CreateDoubleParamValue(CurrentDataContext.Quantity);

                if (CurrentDataContext.SelectedCutLengthType.Unit.Trim() == "PC")
                    ((IpfcParameterOwner)NewIpfcFeature).CreateParam("BOM_REPORT_QUANTITY", NewIpfParamValue);
                else
                {
                    IpfcUnit NewIpfcUnit;
                    // several Try Catch to manage the fact that unit can be in UpperCase, LowerCase or both....
                    try
                    {
                        NewIpfcUnit = ((IpfcSolid)CurrentDataContext.ActiveModel).GetUnit(CurrentDataContext.SelectedCutLengthType.Unit.ToLower(), true);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            NewIpfcUnit = ((IpfcSolid)CurrentDataContext.ActiveModel).GetUnit(CurrentDataContext.SelectedCutLengthType.Unit.ToUpper(), true);
                        }
                        catch (Exception)
                        {
                            try
                            {
                                NewIpfcUnit = ((IpfcSolid)CurrentDataContext.ActiveModel).GetUnit(CurrentDataContext.SelectedCutLengthType.Unit, true);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(String.Concat(ex.Message, " - CutLengthViewModel.InsertBulkItem - UNIT Issue."));
                            }
                        }
                    }

                    ((IpfcParameterOwner)NewIpfcFeature).CreateParamWithUnits("BOM_REPORT_QUANTITY", NewIpfParamValue, NewIpfcUnit);
                }

                return NewIpfcFeature;
            }
            catch (Exception ex)
            {
                if (ex.Message == "pfcExceptions::XToolkitCantModify")
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("CT_ErrorMsgCantModify"),
                            McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning,
                            MessageBoxResult.OK);
                    return null;
                }
                else
                    throw new CutLengthException(this.GetType().Name, ex);
            }
        }

        private void UpdateBulkQuantity()
        {
            try
            {
                if (CurrentDataContext.ActiveModel != null && CurrentDataContext.ActiveModelFileName.ToUpper().Contains(".ASM"))
                {

                    CREOModelStatus CurrentCREOModelStatus = _creoModelService.GetModelStatus(CurrentDataContext.ActiveModel);
                    if (CurrentCREOModelStatus == CREOModelStatus.READONLYITEM || CurrentCREOModelStatus == CREOModelStatus.READONLYWORKSPACE)
                        MessageBox.Show(McgWpfTools.GetStringResource("CT_ErrorMsgCantModify"),
                            McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning,
                            MessageBoxResult.OK);
                    else
                    {
                        CCpfcSelectionOptions aCpfcSelectionOptions = new CCpfcSelectionOptions();
                        IpfcSelectionOptions aIpfcSelectionOptions = aCpfcSelectionOptions.Create("membfeat");
                        IpfcModelItem BulkModel = null;

                        aIpfcSelectionOptions.MaxNumSels = 1;

                        // Check if selection not aborted by user
                        try
                        {
                            var CreoSelection = _creoSessionProvider.Session.Select(aIpfcSelectionOptions, null);
                            BulkModel = CreoSelection[0].get_SelItem();
                        }
                        catch (Exception)
                        {
                            return;
                        }

                        if (BulkModel != null)
                        {
                            IpfcModelItem aIpfcModelItem = (IpfcModelItem)BulkModel;
                            IpfcModel aIpfcModel = _creoModelService.GetParentModelFromModelItem(aIpfcModelItem);

                            if (!_creoModelService.IsLocallyModifiable(aIpfcModel))
                                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("CT_ErrorMsgCantModifySubAssy"), aIpfcModel.FileName),
                                    McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning,
                                    MessageBoxResult.OK);
                            else
                            {
                                // Check if it's a Bulk
                                IpfcComponentFeat aIpfcComponentFeat = null;
                                try
                                {
                                    aIpfcComponentFeat = (IpfcComponentFeat)BulkModel;
                                }
                                catch (Exception)
                                {
                                    // Check if it's a Group

                                    //((IpfcModel)((CpfcModelItem)BulkModel).DBParent);
                                    IpfcFeature aIpfcFeature = (IpfcFeature)BulkModel;

                                    if (aIpfcFeature.FeatType == (int)EpfcFeatureType.EpfcFEATTYPE_GROUP_HEAD)
                                    {
                                        IpfcFeatureGroup aIpfcFeatureGroup = aIpfcFeature.Group;
                                        var ListGrpMembers = aIpfcFeatureGroup.ListMembers();

                                        // Search if a there is a Bulk in the Group
                                        for (int Index = 0; Index < ListGrpMembers.Count; Index++)
                                        {
                                            if (ListGrpMembers[Index].FeatType == (int)EpfcFeatureType.EpfcFEATTYPE_COMPONENT)
                                            {
                                                aIpfcComponentFeat = (IpfcComponentFeat)ListGrpMembers[Index];
                                                if (aIpfcComponentFeat.IsBulkitem)
                                                    Index = ListGrpMembers.Count;
                                            }
                                        }
                                    }
                                }

                                if (aIpfcComponentFeat != null && aIpfcComponentFeat.IsBulkitem)
                                {
                                    IpfcParameterOwner aIpfcSolid = (IpfcParameterOwner)aIpfcComponentFeat;
                                    IpfcParameter CurrentQuantityParam = aIpfcSolid.GetParam("BOM_REPORT_QUANTITY");

                                    if (CurrentQuantityParam != null)
                                    {
                                        IpfcParamValue CurrentQuantityValue = CurrentQuantityParam.GetScaledValue();
                                        double CurrentQuantity = CurrentQuantityValue.DoubleValue;

                                        var returnWindow = _cutLengthWindchillService.ShowDialogCutLengthBulkQuantity(CurrentQuantity);
                                        if (returnWindow.ReturnValue == MessageBoxResult.OK)
                                            UpdateParameterDouble(aIpfcComponentFeat, "BOM_REPORT_QUANTITY", returnWindow.Quantity);
                                    }
                                    else
                                    {
                                        Window aWindow = new Window();
                                        aWindow.Topmost = true;
                                        MessageBox.Show(aWindow,
                                            McgWpfTools.GetStringResource("CT_MsgBulkParamIssue"),
                                            McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Warning,
                                            MessageBoxResult.OK);
                                    }
                                }
                                else
                                {
                                    Window aWindow = new Window();
                                    aWindow.Topmost = true;
                                    MessageBox.Show(aWindow,
                                        McgWpfTools.GetStringResource("CT_MsgSelectBulk"),
                                        McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning,
                                        MessageBoxResult.OK);
                                }
                            }
                        }
                    }
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("CT_MsgNotAnAssembly"),
                        McgWpfTools.GetStringResource("CT_MsgTitleCutLenghtIssue"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK);
            }
            catch (Exception ex)
            {
                throw new CutLengthException(this.GetType().Name, ex);
            }
        }

        private void UpdateParameterDouble(IpfcComponentFeat aIpfcComponentFeat, string ParamName, double Quantity)
        {
            try
            {
                IpfcParamValue NewIpfParamValue;
                CMpfcModelItem aCMpfcModelItem = new CMpfcModelItem();
                IpfcParameter CurrentParam = ((IpfcParameterOwner)aIpfcComponentFeat).GetParam(ParamName);
                NewIpfParamValue = aCMpfcModelItem.CreateDoubleParamValue(Quantity);
                CurrentParam.SetScaledValue(NewIpfParamValue, CurrentParam.Units);
            }
            catch (Exception ex)
            {
                throw new CutLengthException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
