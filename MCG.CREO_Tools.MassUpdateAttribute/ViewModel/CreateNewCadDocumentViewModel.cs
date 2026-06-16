using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WebtermLib.Models;
using MCG.CommonLib.WebtermLib.Services.Interfaces;
using MCG.CommonLib.WpfComponent.View;
using MCG.CommonLib.WpfComponent.View.Attributecolumn;
using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.CREO_Tools.MassUpdateAttribute.Configuration;
using MCG.CREO_Tools.MassUpdateAttribute.Exceptions;
using MCG.CREO_Tools.MassUpdateAttribute.View;
using MCG.Tools.NumberingTool.Interfaces;
using MCG.WindchillRequestTool.Model.Windchill;
using pfcls;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class CreateNewCadDocumentViewModel : ObservableObject, ICreateNewCadDocumentViewModel
    {
        #region [REGION] Properties not from interface
        public CreateNewCadDocumentDataContext CurrentCreateNewCadDocumentDataContext { get; set; }
        #endregion

        #region [REGION] Properties not from interface
        private string MainAppFolder { get; set; }
        private MassUpdateAttributeConfiguration CurrentMassUpdAttriConfiguration { get; set; }
        //private DispatcherTimer TimerPartNumberGenWindow { get; set; } = new DispatcherTimer();
        #endregion

        #region [REGION] Commands
        public ICommand CommandCreateCadDoc { get => new RelayCommand(() => ExecuteCreateCadDoc()); }
        public ICommand CommandCancel { get => new RelayCommand(() => ExecuteCancel()); }
        public ICommand CommandPartNumberGenerator { get => new RelayCommand(() => ExecutePartNumberGenerator()); }
        #endregion


        #region [REGION] Events
        public event EventHandler RequestCloseEvent;

        public void RaiseRequestCloseEvent()
        {
            try
            {
                RequestCloseEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion


        #region [REGION] Init
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly ICreoParameterService _creoParameterService;
        private readonly ICreoFeatureService _creoFeatureService;
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        private readonly IWebtermTools _webtermTools;
        private readonly INumberingToolWindowService _numberingToolWindowService;

        public CreateNewCadDocumentViewModel(IXmlSerializeTools xmlSerializeTools, 
                                             ICreoParameterService creoParameterService,
                                             ICreoFeatureService creoFeatureService,
                                             ICreoSessionProvider creoSessionProvider,
                                             ICreoModelService creoModelService,
                                             IWebtermTools webtermTools,
                                             INumberingToolWindowService numberingToolWindowService)
        {
            try
            {
                _xmlSerializeTools = xmlSerializeTools;
                _creoParameterService = creoParameterService;
                _creoFeatureService = creoFeatureService;
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;
                _webtermTools = webtermTools;
                _numberingToolWindowService = numberingToolWindowService;

                CurrentCreateNewCadDocumentDataContext = new CreateNewCadDocumentDataContext();

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                CurrentMassUpdAttriConfiguration = _xmlSerializeTools.GetDeserializedXml<MassUpdateAttributeConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MassUpdateAttributeConstants.ConfigurationFile}");

                // Update Webterm List
                var temWebtermList = _webtermTools.GetListTerm(WebtermLanguage.ENGLISH, null).OrderBy((item) => item).ToList();
                CurrentCreateNewCadDocumentDataContext.ListWebterm.Clear();

                foreach (var term in temWebtermList)
                    if (term.IndexOf(" - SAP") < 0)
                        CurrentCreateNewCadDocumentDataContext.ListWebterm.Add(term);

                // Check local language
                switch (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpper())
                {
                    case "ZH":
                        CurrentCreateNewCadDocumentDataContext.SelectedIndexLanguage = 0;
                        break;
                    case "FR":
                        CurrentCreateNewCadDocumentDataContext.SelectedIndexLanguage = 1;
                        break;
                    case "DE":
                        CurrentCreateNewCadDocumentDataContext.SelectedIndexLanguage = 2;
                        break;
                    case "IT":
                        CurrentCreateNewCadDocumentDataContext.SelectedIndexLanguage = 3;
                        break;
                    case "PT":
                        CurrentCreateNewCadDocumentDataContext.SelectedIndexLanguage = 4;
                        break;
                    case "EN":
                        CurrentCreateNewCadDocumentDataContext.SelectedIndexLanguage = 5;
                        break;
                    default:
                        CurrentCreateNewCadDocumentDataContext.SelectedIndexLanguage = 5;
                        break;
                }

                // Update List of other attributes
                CurrentCreateNewCadDocumentDataContext.ListOtherAttributes = new List<McgAttributeColumnHeaderInfo>();
                foreach (McgAttributeColumnHeaderInfo attrib in CurrentMassUpdAttriConfiguration.ListColumns)
                {
                    if (attrib.AttributeID == "MODIFIED_BY")
                        attrib.AttributeValue = McgActiveDirectoryTools.GetWindowsSessionUserShortName();
                    if (attrib.AttributeID != "DESCRIPTION2_1")
                        CurrentCreateNewCadDocumentDataContext.ListOtherAttributes.Add(attrib);
                }

                UpdateListFromFolder();

                CurrentCreateNewCadDocumentDataContext.PropertyChanged += UpdateLocalLanguage;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentCreateNewCadDocumentDataContext.CreoIsEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentCreateNewCadDocumentDataContext.CreoIsEnable = e;


                _numberingToolWindowService.CreateNumberRequested += CloseCreateNumberWindow;
                _numberingToolWindowService.UseNumberRequested += UpdatePartNumber;



                //// Init Timer to check if Window to create new PartNumber is active
                //// If yes, subscribe to event.
                //TimerPartNumberGenWindow.Interval = new TimeSpan(0, 0, 1);
                //TimerPartNumberGenWindow.Tick += CheckPartNumberGenWindowCreated;
                //TimerPartNumberGenWindow.Start();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        //private void CheckPartNumberGenWindowCreated(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //var CreatedCadDocWindow = McgWpfTools.GetListWindows<CreateNewCadDocumentWindow>().FirstOrDefault();
        //        var CreatedCadDocWindow = McgWpfTools.GetListWindows<CreateNewCadDocumentFluentWindow>().FirstOrDefault();
        //        if (CreatedCadDocWindow == null)
        //            TimerPartNumberGenWindow.Stop();


        //        //var PartNumbWindows1 = McgWpfTools.GetListWindows<NumberingToolMainView>().FirstOrDefault();
        //        //var PartNumbWindows2 = McgWpfTools.GetListWindows<NumberingToolUpdateCreateView>().FirstOrDefault();
        //        //var PartNumbWindows3 = McgWpfTools.GetListWindows<NumberingToolCreateSeveralView>().FirstOrDefault();
        //        var PartNumbWindows1 = McgWpfTools.GetListWindows<NumberingToolFluentMainView>().FirstOrDefault();
        //        var PartNumbWindows2 = McgWpfTools.GetListWindows<NumberingToolUpdateCreateFluentView>().FirstOrDefault();
        //        var PartNumbWindows3 = McgWpfTools.GetListWindows<NumberingToolCreateSeveralFluentView>().FirstOrDefault();
        //        if (PartNumbWindows1 != null)
        //        {
        //            PartNumbWindows1.CreateNumberEvent -= CloseCreateNumberWindow;
        //            PartNumbWindows1.CreateNumberEvent += CloseCreateNumberWindow;

        //            PartNumbWindows1.UseNumberEvent -= UpdatePartNumber;
        //            PartNumbWindows1.UseNumberEvent += UpdatePartNumber;
        //        }

        //        if (PartNumbWindows2 != null)
        //        {
        //            PartNumbWindows2.CreateNumberEvent -= CloseCreateNumberWindow;
        //            PartNumbWindows2.CreateNumberEvent += CloseCreateNumberWindow;
        //        }

        //        if (PartNumbWindows3 != null)
        //        {
        //            PartNumbWindows3.UseNumberEvent -= UpdatePartNumber;
        //            PartNumbWindows3.UseNumberEvent += UpdatePartNumber;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
        //    }
        //}

        private void UpdateListFromFolder()
        {
            try
            {
                var ColumnWithFolder = CurrentMassUpdAttriConfiguration.ListColumns.Where((item) => item.FolderSource.ToUpper() != "NONE").ToList();

                foreach (var column in ColumnWithFolder)
                {
                    var AllFiles = Directory.EnumerateFiles(column.FolderSource, column.FolderFileFilter).ToList();

                    column.ListValue.Clear();
                    column.ListValue.Add("NONE");
                    string tempFile;
                    foreach (var elem in AllFiles)
                    {
                        tempFile = elem.Split('\\').LastOrDefault();
                        tempFile = tempFile.Split('.').FirstOrDefault();
                        column.ListValue.Add(tempFile);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateLocalLanguage(object sender, PropertyChangedEventArgs e)
        {
            try
            {

                if (e.PropertyName == "CurrentLanguage" | e.PropertyName == "SelectedWebterm")
                {
                    if (CurrentCreateNewCadDocumentDataContext.CurrentLanguage.Name == "LangChina")
                        CurrentCreateNewCadDocumentDataContext.CurrentLanguageText = "CHINESE";
                    else if (CurrentCreateNewCadDocumentDataContext.CurrentLanguage.Name == "LangFrance")
                        CurrentCreateNewCadDocumentDataContext.CurrentLanguageText = "FRENCH";
                    else if (CurrentCreateNewCadDocumentDataContext.CurrentLanguage.Name == "LangGermany")
                        CurrentCreateNewCadDocumentDataContext.CurrentLanguageText = "GERMAN";
                    else if (CurrentCreateNewCadDocumentDataContext.CurrentLanguage.Name == "LangItalia")
                        CurrentCreateNewCadDocumentDataContext.CurrentLanguageText = "ITALIAN";
                    else if (CurrentCreateNewCadDocumentDataContext.CurrentLanguage.Name == "LangPortugal")
                        CurrentCreateNewCadDocumentDataContext.CurrentLanguageText = "PORTUGUESE";
                    else
                        CurrentCreateNewCadDocumentDataContext.CurrentLanguageText = "ENGLISH";

                    if (CurrentCreateNewCadDocumentDataContext.CurrentLanguageText == "ENGLISH")
                        CurrentCreateNewCadDocumentDataContext.Description2_1 = "-";
                    else
                        CurrentCreateNewCadDocumentDataContext.Description2_1 = _webtermTools.GetTerm(CurrentCreateNewCadDocumentDataContext.SelectedWebterm, WebtermLanguage.ENGLISH, _webtermTools.GetWebtermLanguage(CurrentCreateNewCadDocumentDataContext.CurrentLanguageText), true);
                }

                if (e.PropertyName == "CurrentLanguage")
                {
                    if (CurrentCreateNewCadDocumentDataContext.CurrentLanguageText == "ENGLISH")
                    {
                        CurrentCreateNewCadDocumentDataContext.ListWebtermLocal.Clear();
                        CurrentCreateNewCadDocumentDataContext.ListWebtermLocal.Add("-");
                        CurrentCreateNewCadDocumentDataContext.Description2_1 = CurrentCreateNewCadDocumentDataContext.ListWebtermLocal.First();
                    }
                    else
                    {
                        var temWebtermList = _webtermTools.GetListTerm(_webtermTools.GetWebtermLanguage(CurrentCreateNewCadDocumentDataContext.CurrentLanguageText), true).OrderBy((item) => item).ToList();
                        CurrentCreateNewCadDocumentDataContext.ListWebtermLocal.Clear();
                        foreach (var term in temWebtermList)
                            if (term != null && term.IndexOf(" SAP") < 0 && term.Trim() != "")
                                CurrentCreateNewCadDocumentDataContext.ListWebtermLocal.Add(term);
                        CurrentCreateNewCadDocumentDataContext.Description2_1 = _webtermTools.GetTerm(CurrentCreateNewCadDocumentDataContext.SelectedWebterm, WebtermLanguage.ENGLISH, _webtermTools.GetWebtermLanguage(CurrentCreateNewCadDocumentDataContext.CurrentLanguageText), true);
                    }
                }
                else if (e.PropertyName == "Description2_1")
                {
                    CurrentCreateNewCadDocumentDataContext.SelectedWebterm = _webtermTools.GetTerm(CurrentCreateNewCadDocumentDataContext.Description2_1, _webtermTools.GetWebtermLanguage(CurrentCreateNewCadDocumentDataContext.CurrentLanguageText), WebtermLanguage.ENGLISH, true);

                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteCreateCadDoc()
        {
            try
            {
                //CurrentCreateNewCadDocumentWindow.Close();
                RaiseRequestCloseEvent();
                StartCadDocumentCreation();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCancel()
        {
            try
            {
                //CurrentCreateNewCadDocumentWindow.Close();
                RaiseRequestCloseEvent();
                //TimerPartNumberGenWindow.Stop();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecutePartNumberGenerator()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecutePartNumberCreator App");


                _numberingToolWindowService.ShowNumberingToolFluentMainView(true, true);

                //var AlreadyCreatedWindow = McgWpfTools.IsWindowAlreadyCreated<NumberingToolFluentMainView>(true);
                //if (AlreadyCreatedWindow == null)
                //{
                //    NumberingToolFluentMainView CurrentNumberingToolMainView = new NumberingToolFluentMainView(true);
                //    CurrentNumberingToolMainView.CreateNumberEvent += CloseCreateNumberWindow;
                //    CurrentNumberingToolMainView.UseNumberEvent += UpdatePartNumber;
                //    CurrentNumberingToolMainView.Show();
                //}
                //else
                //{
                //    AlreadyCreatedWindow.UseNumberEvent += UpdatePartNumber;
                //    AlreadyCreatedWindow.CreateNumberEvent += CloseCreateNumberWindow;
                //}
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] CREO interaction Methods
        private void StartCadDocumentCreation()
        {
            try
            {
                _creoSessionProvider.CheckConnection();
                string CadDocTemplateName = null;
                string CadDocFolder = _creoSessionProvider.Session.GetConfigOption("start_model_dir").ToString();
                IpfcModel CadModel = null;

                // Retreave template following the CAD docuemnt type
                switch (CurrentCreateNewCadDocumentDataContext.SelectedCadDocumentType)
                {
                    case WindchillObjectType.ASM:
                        CadDocTemplateName = _creoSessionProvider.Session.GetConfigOption("template_designasm").ToString();
                        CadModel = _creoModelService.RetrieveModel($"{CadDocFolder}/{CadDocTemplateName}", EpfcModelType.EpfcMDL_ASSEMBLY);
                        break;
                    case WindchillObjectType.PHYSICAL_PART:
                        CadDocTemplateName = _creoSessionProvider.Session.GetConfigOption("template_solidpart").ToString();
                        CadModel = _creoModelService.RetrieveModel($"{CadDocFolder}/{CadDocTemplateName}", EpfcModelType.EpfcMDL_PART);
                        break;
                    case WindchillObjectType.SHEETMETAL:
                        CadDocTemplateName = _creoSessionProvider.Session.GetConfigOption("template_sheetmetalpart").ToString();
                        CadModel = _creoModelService.RetrieveModel($"{CadDocFolder}/{CadDocTemplateName}", EpfcModelType.EpfcMDL_PART);
                        break;
                    case WindchillObjectType.DRW:
                        CadDocTemplateName = _creoSessionProvider.Session.GetConfigOption("template_drawing").ToString();
                        CadModel = _creoModelService.RetrieveModel($"{CadDocFolder}/{CadDocTemplateName}", EpfcModelType.EpfcMDL_DRAWING);
                        break;
                }

                // rename the CAD Document
                if (CurrentCreateNewCadDocumentDataContext.PartNumber == null || CurrentCreateNewCadDocumentDataContext.PartNumber.Trim() == "")
                    CurrentCreateNewCadDocumentDataContext.PartNumber = GetRandomCadName();

                // If PartNumber ok, rename, display and update attribute
                if (CurrentCreateNewCadDocumentDataContext.PartNumber != null)
                {
                    CadModel.Rename(CurrentCreateNewCadDocumentDataContext.PartNumber, true);

                    // Show 3D model
                    try
                    {
                        IpfcWindows allIpfcWindows = _creoSessionProvider.Session.ListWindows();
                        IpfcWindow currentIpfcWindow = null;
                        if (allIpfcWindows.Count < 18)
                        {
                            currentIpfcWindow = _creoSessionProvider.Session.CreateModelWindow(CadModel);
                            CadModel.Display();
                            currentIpfcWindow.Activate();
                        }
                        else
                        {
                            CadModel.Display();
                            _creoModelService.SearchModelsInSession();
                            currentIpfcWindow = (IpfcWindow)_creoModelService.ListWindow.FirstOrDefault((item) => ((IpfcWindow)item).Model.FileName == CadModel.FileName);
                            if (currentIpfcWindow != null)
                                currentIpfcWindow.Activate();
                        }
                    }
                    catch (Exception)
                    { }

                    // Udpate attributes
                    UpdateAttributeCadDocument(CadModel);
                }

            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateAttributeCadDocument(IpfcModel CurrentCadModel)
        {
            try
            {
                //CurrentCREOConnection.session.SetConfigOption("regen_failure_handling", "resolve_mode");

                // update PTC_COMMON_NAME
                if (CurrentCreateNewCadDocumentDataContext.SelectedWebterm != null && CurrentCreateNewCadDocumentDataContext.SelectedWebterm.Trim() != "")
                    _creoParameterService.SetParameter(CurrentCadModel, "PTC_COMMON_NAME", CurrentCreateNewCadDocumentDataContext.SelectedWebterm);

                // update PTC_COMMON_NAME in local language
                if (CurrentCreateNewCadDocumentDataContext.Description2_1 != null && CurrentCreateNewCadDocumentDataContext.Description2_1.Trim() != "")
                    _creoParameterService.SetParameter(CurrentCadModel, "DESCRIPTION2_1", CurrentCreateNewCadDocumentDataContext.Description2_1, true);

                foreach (var attrib in CurrentCreateNewCadDocumentDataContext.ListOtherAttributes)
                {
                    // Search for the right CurrentAttributeColumnHeaderInfo depending the type of attribute
                    McgAttributeColumnHeaderInfo CurrentAttrib = null;
                    if (attrib.ColumnType == McgColumnType.COMBOBOX || attrib.ColumnType == McgColumnType.TEMPLATECOMBOBOX)
                    {
                        if (attrib.ParentAttributeObject?.GetType() == typeof(McgAttributeGridComboBox))
                            CurrentAttrib = ((McgAttributeGridComboBox)(attrib.ParentAttributeObject)).CurrentMcgAttributeHeaderViewModel.CurrentAttributeColumnHeaderInfo;
                        else if (attrib.ParentAttributeObject.GetType() == typeof(McgAttributeGridComboBoxFluent))
                            CurrentAttrib = ((McgAttributeGridComboBoxFluent)(attrib.ParentAttributeObject)).CurrentMcgAttributeHeaderViewModel.CurrentAttributeColumnHeaderInfo;
                    }
                    else if (attrib.ColumnType == McgColumnType.TEXT)
                    {
                        if (attrib.ParentAttributeObject.GetType() == typeof(McgAttributeGridText))
                            CurrentAttrib = ((McgAttributeGridText)(attrib.ParentAttributeObject)).CurrentMcgAttributeHeaderViewModel.CurrentAttributeColumnHeaderInfo;
                        else if (attrib.ParentAttributeObject.GetType() == typeof(McgAttributeGridTextFluent))
                            CurrentAttrib = ((McgAttributeGridTextFluent)(attrib.ParentAttributeObject)).CurrentMcgAttributeHeaderViewModel.CurrentAttributeColumnHeaderInfo;
                    }

                    // Check if Attrubte is Material
                    if (CurrentAttrib.AttributeID == "MATERIAL")
                    {
                        if ((CurrentCreateNewCadDocumentDataContext.SelectedCadDocumentType == WindchillObjectType.SHEETMETAL | CurrentCreateNewCadDocumentDataContext.SelectedCadDocumentType == WindchillObjectType.PHYSICAL_PART) & CurrentAttrib.AttributeValue != "NONE")
                        {
                            // Assigned MATERIAL directly in the param, if in case, no relation drive it
                            _creoParameterService.SetParameter(CurrentCadModel, CurrentAttrib.AttributeID, CurrentAttrib.AttributeValue, CurrentAttrib.IsDesignated);
                            // Assigned the Material to the PRT
                            if (!_creoFeatureService.AssignMaterial(CurrentCadModel, CurrentAttrib.AttributeValue))
                                MessageBox.Show($"Issue to assigned Material {CurrentAttrib.AttributeID} to {CurrentCadModel.FileName}");
                        }
                        else
                            _creoParameterService.SetParameter(CurrentCadModel, CurrentAttrib.AttributeID, CurrentAttrib.AttributeValue, CurrentAttrib.IsDesignated);
                    }
                    else if (CurrentAttrib.AttributeType == AttributeTypeEnum.INT)
                        _creoParameterService.SetParameter(CurrentCadModel, CurrentAttrib.AttributeID, CurrentAttrib.AttributeValue, CurrentAttrib.IsDesignated);
                    else if (CurrentAttrib.AttributeType == AttributeTypeEnum.REAL)
                        _creoParameterService.SetParameter(CurrentCadModel, CurrentAttrib.AttributeID, CurrentAttrib.AttributeValue, CurrentAttrib.IsDesignated);
                    else if (CurrentAttrib.AttributeType == AttributeTypeEnum.TEXT)
                        _creoParameterService.SetParameter(CurrentCadModel, CurrentAttrib.AttributeID, CurrentAttrib.AttributeValue, CurrentAttrib.IsDesignated);
                }

                IpfcSolid Model3D = (IpfcSolid)CurrentCadModel;
                Model3D.Regenerate(null/* TODO Change to default(_) if this is not a reference type */);
                //CurrentCREOConnection.session.SetConfigOption("regen_failure_handling", "no_resolve_mode");
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private string GetRandomCadName()
        {
            try
            {
                Random generator = new Random();
                int index = generator.Next(1, 10000);
                return string.Format("NEWCADDOC{0}", index);
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        private void UpdatePartNumber(object sender, EventArgs e)
        {
            try
            {
                if (sender != null && sender.GetType() == typeof(string))
                    CurrentCreateNewCadDocumentDataContext.PartNumber = (string)sender;
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void CloseCreateNumberWindow(object sender, EventArgs e)
        {
            try
            {
                //if (sender != null && sender.GetType() == typeof(NumberingToolMainView))
                //    ((NumberingToolMainView)sender).Close();
                _numberingToolWindowService.CloseNumberingToolFluentMainView();

                //if (sender != null && sender.GetType() == typeof(NumberingToolFluentMainView))
                //    ((NumberingToolFluentMainView)sender).Close();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
