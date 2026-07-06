using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.CreoInteractionTools.CREOExceptions;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WebtermLib.Models;
using MCG.CommonLib.WebtermLib.Services.Interfaces;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.CommonLib.WpfComponent.WindchillCredential;
using MCG.CREO_Tools.MassUpdateAttribute.Configuration;
using MCG.CREO_Tools.MassUpdateAttribute.Exceptions;
using MCG.CREO_Tools.MassUpdateAttribute.Interfaces;
using MCG.CREO_Tools.MassUpdateAttribute.View;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColor;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.Services.Interfaces;
using pfcls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class MassUpdateAttributeViewModel : ObservableObject, IMassUpdateAttributeViewModel
    {
        #region [REGION] Properties from Interface
        public MassUpdateAttributeDataContext CurrentMassUpdAttribDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private MassUpdateAttributeConfiguration CurrentMassUpdAttriConfiguration { get; set; }
        private Dispatcher MainDispatcher { get; set; }
        private string NamingConventionXmlFile { get; set; }
        private bool StopCurrentProcess { get; set; } = false;
        private Thread ListModelThread { get; set; } = null;
        private string AppearanceFileName { get; set; }
        private bool ChangeColorPaletteInProgress { get; set; } = false;
        private WindchillCredentialItem WindchillNetworkCredential { get; set; } = null;
        private List<BrandGroupSubGroupItem> ListBrandGroupSubGroup { get; set; }
        private List<IpfcModel> ListCadModelInSession { get; set; }
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
        #endregion

        #region [REGION] Commands
        public ICommand CommandBtHelpMouseLeftButtonUpEvent { get => new RelayCommand(() => ExecuteBtHelpMouseLeftButtonUpEvent()); }
        public ICommand CommandListCadDoc { get => new RelayCommand(() => ExecuteListCadDoc()); }
        public ICommand CommandUpdateCadDoc { get => new RelayCommand(() => ExecuteUpdateCadDoc()); }
        public ICommand CommandDisplayedModelsOnly { get => new RelayCommand(() => CurrentMassUpdAttribDataContext.IsOnlyDisplayedModels = !CurrentMassUpdAttribDataContext.IsOnlyDisplayedModels); }
        public ICommand CommandActiveModelOnly { get => new RelayCommand(() => CurrentMassUpdAttribDataContext.IsOnlyActiveModel = !CurrentMassUpdAttribDataContext.IsOnlyActiveModel); }
        public ICommand CommandShowCheckedOut { get => new RelayCommand(() => CurrentMassUpdAttribDataContext.IsCheckedOutShown = !CurrentMassUpdAttribDataContext.IsCheckedOutShown); }
        public ICommand CommandShowLocallyModified { get => new RelayCommand(() => CurrentMassUpdAttribDataContext.IsLocallyModifiedShown = !CurrentMassUpdAttribDataContext.IsLocallyModifiedShown); }
        public ICommand CommandShowReadOnly { get => new RelayCommand(() => CurrentMassUpdAttribDataContext.IsReadOnlyShown = !CurrentMassUpdAttribDataContext.IsReadOnlyShown); }
        public ICommand CommandApplyWebtermSelectedItem { get => new RelayCommand(() => ExecuteApplyWebtermSelectedItem()); }
        public ICommand CommandApplyToAllSamePartNumber { get => new RelayCommand(() => ExecuteApplyToAllSamePartNumber()); }
        public ICommand CommandCheckOutOneCadDoc { get => new RelayCommand(() => ExecuteCheckOutOneCadDoc()); }
        public ICommand CommandCheckInOneCadDoc { get => new RelayCommand(() => ExecuteCheckInOneCadDoc()); }
        public ICommand CommandResetUpdateSelectedItem { get => new RelayCommand(() => ExecuteResetUpdateSelectedItem()); }
        public ICommand CommandSelectUnselectAll { get => new RelayCommand<bool>((isselected) => ExecuteSelectUnselectAll(isselected)); }
        public ICommand CommandSelectUnselectAllRename { get => new RelayCommand<bool>((isselected) => ExecuteSelectUnselectAllRename(isselected)); }
        public ICommand CommandApplyWebtermToSelected { get => new RelayCommand(() => ExecuteApplyWebtermToSelected()); }
        public ICommand CommandCheckInAllCadDoc { get => new RelayCommand(() => ExecuteCheckInAllCadDoc()); }
        public ICommand CommandCheckOutAllCadDoc { get => new RelayCommand(() => ExecuteCheckOutAllCadDoc()); }
        public ICommand CommandResetUpdateAllSelectedItem { get => new RelayCommand(() => ExecuteResetUpdateAllSelectedItem()); }
        public ICommand CommandApplyAllHeaderValue { get => new RelayCommand<McgAttributeColumnHeaderInfo>((col) => ExecuteApplyAllHeaderValue(col)); }
        public ICommand CommandImportFromExcel { get => new RelayCommand(() => ExecuteImportFromExcel()); }
        public ICommand CommandUpdateParamRelation { get => new RelayCommand(() => ExecuteUpdateParamRelation()); }
        public ICommand CommandUpdateLayers { get => new RelayCommand(() => ExecuteUpdateLayers()); }
        public ICommand CommandOpenModelInCreo { get => new RelayCommand<bool>((isasynch) => ExecuteOpenModelInCreo(isasynch)); }
        public ICommand CommandRenameObject { get => new RelayCommand(() => ExecuteRenameObject()); }
        public ICommand CommandRemoveColor { get => new RelayCommand<bool>((obj) => ExecuteUpdateColor(true)); }
        public ICommand CommandUpdateColorPalette { get => new RelayCommand(() => ExecuteUpdateColorPalette()); }
        public ICommand CommandUpdateColor { get => new RelayCommand<bool>((obj) => ExecuteUpdateColor(false)); }
        public ICommand CommandDeleteSelectedCadDoc { get => new RelayCommand(() => ExecuteDeleteSelectedCadDoc()); }
        public ICommand CommandStartRename { get => new RelayCommand(() => ExecuteStartRename()); }
        // public ICommand CommandClose { get => new RelayCommand(() => ExecuteClose()); }
        public ICommand CommandRemove { get => new RelayCommand(() => ExecuteRemove()); }
        #endregion

        #region [REGION] Init
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly ICreoParameterService _creoParameterService;
        private readonly ICreoFeatureService _creoFeatureService;
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        private readonly ICreoMacroService _creoMacroService;
        private readonly ICreoLayerService _creoLayerService;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly IWebtermTools _webtermTools;
        private readonly IMassUpdateAttributeWindowService _massUpdateAttributeWindowService;
        private readonly IWindchillPartManagementService _windchillPartManagementService;
        private readonly IWindchillEpmDocumentManagementService _windchillEpmDocumentManagementService;
        private readonly IWindchillDocumentManagementService _windchillDocumentManagementService;
        private readonly IWindchillCustomManagementService _windchillCustomManagementService;
        private readonly IWindchillCheckNumberService _windchillCheckNumberService;
        private readonly ISharedAppContext _sharedAppContext;

        public MassUpdateAttributeViewModel(IXmlSerializeTools xmlSerializeTools,
                                            ICreoSessionProvider creoSessionProvider,
                                            IWindchillCredentialService windchillCredentialService,
                                            IWebtermTools webtermTools,
                                            ICreoParameterService creoParameterService,
                                            ICreoFeatureService creoFeatureService,
                                            ICreoModelService creoModelService,
                                            IMassUpdateAttributeWindowService massUpdateAttributeWindowService,
                                            ICreoMacroService creoMacroService,
                                            ICreoLayerService creoLayerService,
                                            IWindchillPartManagementService windchillPartManagementService,
                                            IWindchillEpmDocumentManagementService windchillEpmDocumentManagementService,
                                            IWindchillDocumentManagementService windchillDocumentManagementService,
                                            IWindchillCustomManagementService windchillCustomManagementService,
                                            IWindchillCheckNumberService windchillCheckNumberService,
                                            ISharedAppContext sharedAppContext)
        {
            try
            {
                _xmlSerializeTools = xmlSerializeTools;
                _creoSessionProvider = creoSessionProvider;
                _windchillCredentialService = windchillCredentialService;
                _webtermTools = webtermTools;
                _creoParameterService = creoParameterService;
                _creoFeatureService = creoFeatureService;
                _creoModelService = creoModelService;
                _massUpdateAttributeWindowService = massUpdateAttributeWindowService;
                _creoMacroService = creoMacroService;
                _creoLayerService = creoLayerService;
                _windchillPartManagementService = windchillPartManagementService;
                _windchillEpmDocumentManagementService = windchillEpmDocumentManagementService;
                _windchillDocumentManagementService = windchillDocumentManagementService;
                _windchillCustomManagementService = windchillCustomManagementService;
                _windchillCheckNumberService = windchillCheckNumberService;
                _sharedAppContext = sharedAppContext;

                CurrentMassUpdAttribDataContext = new MassUpdateAttributeDataContext();

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                MainDispatcher = Dispatcher.CurrentDispatcher;

                // Read configuration
                CurrentMassUpdAttriConfiguration = _xmlSerializeTools.GetDeserializedXml<MassUpdateAttributeConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MassUpdateAttributeConstants.ConfigurationFile}");

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentMassUpdAttribDataContext.ShowActionButton = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentMassUpdAttribDataContext.ShowActionButton = e;

                MCGLanguage CurrentMCGLanguage = _sharedAppContext.CurrentLanguage?.Language;
                if (CurrentMCGLanguage != null)
                    CurrentMCGLanguage.ChangeLanguageInterface += UpdateInterfaceLanguage;

                foreach (McgAttributeColumnHeaderInfo NewCol in CurrentMassUpdAttriConfiguration.ListColumns)
                {
                    NewCol.BtText = McgWpfTools.GetStringResource("MUA_BtApplySelection");
                    if (NewCol.AttributeID == "MODIFIED_BY")
                        NewCol.AttributeValue = McgActiveDirectoryTools.GetWindowsSessionUserShortName();
                }

                CurrentMassUpdAttribDataContext.ListColumns = CurrentMassUpdAttriConfiguration.ListColumns;
                CurrentMassUpdAttribDataContext.MandatoryLayers = CurrentMassUpdAttriConfiguration.ListStandardLayers.Where(layer => layer.ToBeCreatedIfMissing).ToList();

                UpdateInterfaceLanguage(null, null);
                if (CurrentMassUpdAttriConfiguration.ListLanguages != null)
                    foreach (var lang in CurrentMassUpdAttriConfiguration.ListLanguages)
                        CurrentMassUpdAttribDataContext.ListLanguages.Add(lang);

                UpdateListWebterm();
                UpdateListFromFolder();

                NamingConventionXmlFile = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{CommonLibConstants.NamingConventionFile}";

                // update data for appearances
                List<McgAppearanceItem> ListAppearances;

                AppearanceFileName = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.AppearanceFileName01}";
                ListAppearances = McgBusinessTools.GetListAppearancesFromFile(AppearanceFileName);
                if (ListAppearances != null && ListAppearances.Count > 0)
                {
                    CurrentMassUpdAttribDataContext.ColorPalette01 = new CadAutoColorPalette() { IsSelected = true, Name = "Default", ColorPaletteFile = AppearanceFileName };
                    foreach (var item in ListAppearances.OrderBy(color => color.Name))
                        CurrentMassUpdAttribDataContext.ColorPalette01.ListColor.Add(CadAutoColorCreoColor.GetCadAutoColorCreoColor(item));
                }
                if (ListAppearances != null && ListAppearances.Count > 0)
                    foreach (var item in ListAppearances.OrderBy(color => color.Name))
                        CurrentMassUpdAttribDataContext.ListCreoColor.Add(CadAutoColorCreoColor.GetCadAutoColorCreoColor(item));


                AppearanceFileName = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.AppearanceFileName02}";
                ListAppearances = McgBusinessTools.GetListAppearancesFromFile(AppearanceFileName);
                if (ListAppearances != null && ListAppearances.Count > 0)
                {
                    CurrentMassUpdAttribDataContext.ColorPalette02 = new CadAutoColorPalette() { IsSelected = false, Name = "CREO Tools", ColorPaletteFile = AppearanceFileName };
                    foreach (var item in ListAppearances.OrderBy(color => color.Name))
                        CurrentMassUpdAttribDataContext.ColorPalette02.ListColor.Add(CadAutoColorCreoColor.GetCadAutoColorCreoColor(item));
                }

                AppearanceFileName = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.AppearanceFileName03}";
                ListAppearances = McgBusinessTools.GetListAppearancesFromFile(AppearanceFileName);
                if (ListAppearances != null && ListAppearances.Count > 0)
                {
                    CurrentMassUpdAttribDataContext.ColorPalette03 = new CadAutoColorPalette() { IsSelected = false, Name = "Marketing", ColorPaletteFile = AppearanceFileName };
                    foreach (var item in ListAppearances.OrderBy(color => color.Name))
                        CurrentMassUpdAttribDataContext.ColorPalette03.ListColor.Add(CadAutoColorCreoColor.GetCadAutoColorCreoColor(item));
                }

                AppearanceFileName = CurrentMassUpdAttribDataContext.ColorPalette01.ColorPaletteFile;

                ListBrandGroupSubGroup = McgBusinessTools.GetLIstBrandGroupSubGroup();
                CurrentMassUpdAttribDataContext.ListBrand.Clear();
                var brands = ListBrandGroupSubGroup.Select(i => i.Brand).Distinct();
                foreach (var brand in brands)
                {
                    CurrentMassUpdAttribDataContext.ListBrand.Add(brand);
                }

                CurrentMassUpdAttribDataContext.UpdateBrandEvent += UpdateGroups;
                CurrentMassUpdAttribDataContext.UpdateGroupEvent += UpdateSubGroups;
                CurrentMassUpdAttribDataContext.UpdateSubGroupEvent += UpdateOptions;
                CurrentMassUpdAttribDataContext.SelectedBrand = CurrentMassUpdAttribDataContext.ListBrand.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }

            _creoLayerService = creoLayerService;
            _windchillCustomManagementService = windchillCustomManagementService;
            _windchillCheckNumberService = windchillCheckNumberService;
        }

        private void UpdateInterfaceLanguage(object sender, EventArgs e)
        {
            try
            {
                switch (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName)
                {
                    case "zh":
                        CurrentMassUpdAttribDataContext.CurrentLanguage = "CHINESE";
                        break;
                    case "fr":
                        CurrentMassUpdAttribDataContext.CurrentLanguage = "FRENCH";
                        break;
                    case "de":
                        CurrentMassUpdAttribDataContext.CurrentLanguage = "GERMAN";
                        break;
                    case "it":
                        CurrentMassUpdAttribDataContext.CurrentLanguage = "ITALIAN";
                        break;
                    case "pt":
                        CurrentMassUpdAttribDataContext.CurrentLanguage = "PORTUGUESE";
                        break;
                    case "en":
                        CurrentMassUpdAttribDataContext.CurrentLanguage = "ENGLISH";
                        break;
                    default:
                        CurrentMassUpdAttribDataContext.CurrentLanguage = "ENGLISH";
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateListFromFolder()
        {
            try
            {
                var ColumnWithFolder = CurrentMassUpdAttriConfiguration.ListColumns.Where((elem) => elem.FolderSource != "None");

                foreach (var column in ColumnWithFolder)
                {
                    var AllFiles = Directory.EnumerateFiles(column.FolderSource, column.FolderFileFilter);
                    column.ListValue.Clear();
                    column.ListValue.Add("NONE");
                    foreach (var elem in AllFiles)
                    {
                        var value = elem.Substring(elem.LastIndexOf("\\") + 1);
                        column.ListValue.Add(value.Split('.').FirstOrDefault());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void CheckWindchillCredential()
        {
            try
            {
                if (WindchillNetworkCredential == null || !WindchillNetworkCredential.IsCredentialOk)
                {
                    WindchillNetworkCredential = _windchillCredentialService.GetWindchillCredential(CommonLibConstants.WindchillUrl, CommonLibConstants.WindchillUrl);
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateGroups(object sender, EventArgs e)
        {
            try
            {
                CurrentMassUpdAttribDataContext.ListGroup.Clear();
                var groups = ListBrandGroupSubGroup.Where(i => i.Brand == CurrentMassUpdAttribDataContext.SelectedBrand).Select(i => i.Group).Distinct();
                foreach (var group in groups)
                {
                    CurrentMassUpdAttribDataContext.ListGroup.Add(group);
                }
                CurrentMassUpdAttribDataContext.SelectedGroup = CurrentMassUpdAttribDataContext.ListGroup.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateSubGroups(object sender, EventArgs e)
        {
            try
            {
                CurrentMassUpdAttribDataContext.ListSubGroup.Clear();

                var subGroups = ListBrandGroupSubGroup.Where(i => i.Brand == CurrentMassUpdAttribDataContext.SelectedBrand && i.Group == CurrentMassUpdAttribDataContext.SelectedGroup).Select(i => i.SubGroup).Distinct();
                foreach (var subGroup in subGroups)
                {
                    CurrentMassUpdAttribDataContext.ListSubGroup.Add(subGroup);
                }
                CurrentMassUpdAttribDataContext.SelectedSubGroup = CurrentMassUpdAttribDataContext.ListSubGroup.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateOptions(object sender, EventArgs e)
        {
            try
            {
                CurrentMassUpdAttribDataContext.ListOption.Clear();

                var options = ListBrandGroupSubGroup.FirstOrDefault(i => i.Brand == CurrentMassUpdAttribDataContext.SelectedBrand && i.Group == CurrentMassUpdAttribDataContext.SelectedGroup && i.SubGroup == CurrentMassUpdAttribDataContext.SelectedSubGroup)?.OptionList;
                foreach (var option in options)
                {
                    CurrentMassUpdAttribDataContext.ListOption.Add(option);
                }
                CurrentMassUpdAttribDataContext.SelectedOption = CurrentMassUpdAttribDataContext.ListOption.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteBtHelpMouseLeftButtonUpEvent()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("MUA_LinkHelpMassUpdateAttribute"));
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteListCadDoc()
        {
            try
            {
                if (!CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress)
                {

                    RaiseActionInProgressEvent();
                    CurrentMassUpdAttribDataContext.IsLoadedFromCreo = true;

                    ListModelThread = new Thread(new ThreadStart(SearchListModelsInSession));
                    ListModelThread.IsBackground = true;
                    ListModelThread.Start();
                }
                else if (System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("MUA_AbordProcessMsg"), McgWpfTools.GetStringResource("MUA_AbordProcessTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && ListModelThread != null)
                {
                    StopCurrentProcess = true;
                    CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateCadDoc()
        {
            try
            {
                if (!CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress)
                {
                    RaiseActionInProgressEvent();
                    ListModelThread = new Thread(new ThreadStart(UpdateAllCadDocuments));
                    ListModelThread.IsBackground = true;
                    ListModelThread.Start();
                }
                else if (System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("MUA_AbordProcessUpdateMsg"), McgWpfTools.GetStringResource("MUA_AbordProcessTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && ListModelThread != null)
                {
                    StopCurrentProcess = true;
                    CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteApplyWebtermSelectedItem()
        {
            try
            {
                ApplyWebtermToSelected();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteApplyToAllSamePartNumber()
        {
            try
            {
                ApplyToAllSamePartNumber();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckOutOneCadDoc()
        {
            try
            {
                CheckOutOneCadDoc();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckInOneCadDoc()
        {
            try
            {
                CheckInOneCadDoc();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteResetUpdateSelectedItem()
        {
            try
            {
                ResetUpdateSelectedItem();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSelectUnselectAll(bool IsSelected)
        {
            try
            {
                foreach (var elem in CurrentMassUpdAttribDataContext.ShownCadModels)
                    elem.IsSelected = IsSelected;
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSelectUnselectAllRename(bool IsSelected)
        {
            try
            {
                foreach (var elem in CurrentMassUpdAttribDataContext.ListToBeRenamedObject.Where(item => !item.IsReadOnly))
                    elem.IsSelected = IsSelected;
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteApplyWebtermToSelected()
        {
            try
            {
                ApplyWebtermSelectedItem();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckInAllCadDoc()
        {
            try
            {
                CheckInAllCadDoc();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckOutAllCadDoc()
        {
            try
            {
                CheckOutAllCadDoc();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteResetUpdateAllSelectedItem()
        {
            try
            {
                ResetUpdateAllSelectedItem();
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteApplyAllHeaderValue(McgAttributeColumnHeaderInfo CurrentColumnHeaderInfo)
        {
            try
            {
                ApplyValueToSelected(CurrentColumnHeaderInfo);
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteImportFromExcel()
        {
            try
            {
                System.Windows.Forms.OpenFileDialog CurrentOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
                CurrentOpenFileDialog.Filter = "Excel files (*.xls,*.xlsx)|*.xls;*.xlsx";
                CurrentOpenFileDialog.ShowDialog();
                if (CurrentOpenFileDialog.FileName != null && CurrentOpenFileDialog.FileName != "")
                {
                    CurrentMassUpdAttribDataContext.IsLoadedFromCreo = false;
                    McgLinkToExcel CurrentMcgLinkToExcel = new McgLinkToExcel(CurrentOpenFileDialog.FileName);
                    List<MassUpdateAttributeExportItem> CurrentList = CurrentMcgLinkToExcel.Read<MassUpdateAttributeExportItem>("NUMBERS");


                    string PropName = McgReflectionTools.GetPropertyAttributeValue<MassUpdateAttributeExportItem>("DESCRIPTION2", "ColumnAttribute");

                    CurrentMassUpdAttribDataContext.ShownCadModels.Clear();
                    CurrentMassUpdAttribDataContext.AllCadModels.Clear();
                    MassUpdateAttributeItem CurrentMassUpdateAttributeItem = null;
                    foreach (var item in CurrentList.Where(obj => !string.IsNullOrEmpty(obj.NUMBER)))
                    {
                        CurrentMassUpdateAttributeItem = new MassUpdateAttributeItem()
                        {
                            PartNumber = item.NUMBER.ToLower(),
                            PTC_COMMON_NAME = "",
                            IsUpdated = true,
                            IsPtcCommonNameModifiable = false,
                            FromExcelImport = true,
                            IsCheckedIn = true,
                            IsCheckedOut = true,
                            IsLocallyModified = true
                        };
                        UpdateOnItemAttributeFromXlsExport(CurrentMassUpdateAttributeItem, item);

                        CurrentMassUpdAttribDataContext.ShownCadModels.Add(CurrentMassUpdateAttributeItem);
                        CurrentMassUpdAttribDataContext.AllCadModels.Add(CurrentMassUpdateAttributeItem);
                    }
                }
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateParamRelation()
        {
            try
            {
                if (!CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress)
                {
                    RaiseActionInProgressEvent();
                    CurrentMassUpdAttribDataContext.IsLoadedFromCreo = true;

                    ListModelThread = new Thread(new ThreadStart(UpdateParamRelationAsynch));
                    ListModelThread.IsBackground = true;
                    ListModelThread.Start();
                }
                else if (System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("MUA_AbordProcessMsg"), McgWpfTools.GetStringResource("MUA_AbordProcessTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && ListModelThread != null)
                {
                    StopCurrentProcess = true;
                    CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                }
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateLayers()
        {
            try
            {
                if (!CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress)
                {
                    RaiseActionInProgressEvent();
                    CurrentMassUpdAttribDataContext.IsLoadedFromCreo = true;

                    ListModelThread = new Thread(new ThreadStart(UpdateLayersAsynch));
                    ListModelThread.IsBackground = true;
                    ListModelThread.Start();
                }
                else if (System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("MUA_AbordProcessMsg"), McgWpfTools.GetStringResource("MUA_AbordProcessTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && ListModelThread != null)
                {
                    StopCurrentProcess = true;
                    CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                }
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenModelInCreo(bool InAsynch = false)
        {
            try
            {
                if (CurrentMassUpdAttribDataContext.SelectedItem != null)
                {
                    EPMDocument CurrentEpm = new EPMDocument(CurrentMassUpdAttribDataContext.SelectedItem.PartNumber, CurrentMassUpdAttribDataContext.SelectedItem.PartNumber, CurrentMassUpdAttribDataContext.SelectedItem.PartNumber);

                    if (InAsynch)
                    {
                        RaiseActionInProgressEvent();


                        Thread aThread = new Thread(new ThreadStart(() =>
                        {
                            CurrentEpm.OpenInCreo(_creoSessionProvider, _creoModelService);
                            RaiseActionDoneEvent();
                        }));
                        aThread.IsBackground = true;
                        aThread.Start();
                    }
                    else
                        CurrentEpm.OpenInCreo(_creoSessionProvider, _creoModelService);
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRenameObject()
        {
            try
            {
                CurrentMassUpdAttribDataContext.ListToBeRenamedObject.Clear();

                // search objects
                string SearchNumber = "";
                bool IsAccurateNumber = false;
                if (!CurrentMassUpdAttribDataContext.SelectedItem.CurrentWindchillCheckedObject.IsAccurate)
                    SearchNumber = CurrentMassUpdAttribDataContext.SelectedItem.PartNumber;
                else
                {
                    SearchNumber = CurrentMassUpdAttribDataContext.SelectedItem.BasePartNumber;
                    IsAccurateNumber = true;
                }

                CheckWindchillCredential();
                // search Parts
                RestOdataWtPart currentPart = null;
                if (IsAccurateNumber)
                {
                    currentPart = _windchillPartManagementService.GetOnePart(WindchillNetworkCredential.WindchillCredential, SearchNumber, "Latest", CommonLibConstants.WindchillUrl);
                    if (currentPart != null)
                    {
                        CurrentMassUpdAttribDataContext.ListToBeRenamedObject.Add(new MassUpdateAttributeRenameItem()
                        {
                            OldName = currentPart.Name,
                            Number = currentPart.Number,
                            ObjectId = currentPart.ID,
                            ObjectType = WindchillObjectType.PART,
                            ToBeRenamed = currentPart.State?.Value == "INWORK" || currentPart.State?.Value == "REWORK",
                            IsReadOnly = !(currentPart.State?.Value == "INWORK" || currentPart.State?.Value == "REWORK"),
                            OdataObject = currentPart,
                            State = currentPart.State?.Value
                        });
                    }
                }

                // search CAD Document
                List<RestOdataEpmDocument> listEpmDocument;
                Dictionary<string, string> numbers = new Dictionary<string, string>();
                numbers.Add($"{SearchNumber}*", "Latest");
                //listEpmDocument = WindchillRestOdataTool.GetListEpmDocument(WindchillNetworkCredential.WindchillCredential, numbers, McgMiscTools.GetAppSetting(this, "WindchillUrl"));
                listEpmDocument = _windchillEpmDocumentManagementService.GetListEpmDocumentWithFilter(WindchillNetworkCredential.WindchillCredential, new List<McgObjectNumber>() { new McgObjectNumber() { Number = SearchNumber, Revision = "Latest" } }, RestOdataEnumFilterType.STARTS_WITH, CommonLibConstants.WindchillUrl);

                if (listEpmDocument != null)
                {
                    foreach (var document in listEpmDocument)
                    {
                        CurrentMassUpdAttribDataContext.ListToBeRenamedObject.Add(new MassUpdateAttributeRenameItem()
                        {
                            OldName = document.Name,
                            Number = document.Number,
                            ObjectId = document.ID,
                            ObjectType = document.ObjectWindchillSubType,
                            ToBeRenamed = document.State?.Value == "INWORK" || document.State?.Value == "REWORK",
                            IsReadOnly = !(document.State?.Value == "INWORK" || document.State?.Value == "REWORK"),
                            OdataObject = document,
                            State = document.State?.Value
                        });
                    }
                }

                // search WT Document
                List<RestOdataWtDocument> listWtDocument;
                if (IsAccurateNumber)
                {
                    listWtDocument = _windchillDocumentManagementService.GetListWtDocumentStartWithFilter(WindchillNetworkCredential.WindchillCredential, SearchNumber, CommonLibConstants.WindchillUrl);
                    if (listWtDocument != null)
                    {
                        foreach (var document in listWtDocument)
                        {
                            CurrentMassUpdAttribDataContext.ListToBeRenamedObject.Add(new MassUpdateAttributeRenameItem()
                            {
                                OldName = document.Name,
                                Number = document.Number,
                                ObjectId = document.ID,
                                ObjectType = WindchillObjectType.WTDOC,
                                ToBeRenamed = document.State?.Value == "INWORK" || document.State?.Value == "REWORK",
                                IsReadOnly = !(document.State?.Value == "INWORK" || document.State?.Value == "REWORK"),
                                OdataObject = document,
                                State = document.State?.Value
                            });
                        }
                    }
                }
                if (CurrentMassUpdAttribDataContext.ListToBeRenamedObject.Count > 0)
                {
                    _massUpdateAttributeWindowService.ShowDialogMassUpdateAttributeChangeName(this);
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("MUA_LabeNoObjectToRename"), McgWpfTools.GetStringResource("MUA_TitleWindowRename"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateColorPalette()
        {
            try
            {
                if (!ChangeColorPaletteInProgress)
                {
                    ChangeColorPaletteInProgress = true;

                    CadAutoColorPalette CurrentPalette;
                    ObservableCollection<CadAutoColorCreoColor> TempPalette;
                    if (CurrentMassUpdAttribDataContext.ColorPalette01.IsSelected)
                    {
                        CurrentPalette = CurrentMassUpdAttribDataContext.ColorPalette01;
                        TempPalette = CurrentMassUpdAttribDataContext.ColorPalette01.ListColor;
                    }
                    else if (CurrentMassUpdAttribDataContext.ColorPalette02.IsSelected)
                    {
                        CurrentPalette = CurrentMassUpdAttribDataContext.ColorPalette02;
                        TempPalette = CurrentMassUpdAttribDataContext.ColorPalette02.ListColor;
                    }
                    else if (CurrentMassUpdAttribDataContext.ColorPalette03.IsSelected)
                    {
                        CurrentPalette = CurrentMassUpdAttribDataContext.ColorPalette03;
                        TempPalette = CurrentMassUpdAttribDataContext.ColorPalette03.ListColor;
                    }
                    else
                    {
                        CurrentPalette = CurrentMassUpdAttribDataContext.ColorPalette01;
                        TempPalette = CurrentMassUpdAttribDataContext.ColorPalette01.ListColor;
                    }

                    CurrentMassUpdAttribDataContext.ListCreoColor.Clear();
                    AppearanceFileName = CurrentPalette.ColorPaletteFile;
                    foreach (CadAutoColorCreoColor color in TempPalette)
                    {
                        CurrentMassUpdAttribDataContext.ListCreoColor.Add(color);
                    }
                    ChangeColorPaletteInProgress = false;

                }
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateColor(bool isOnlyRemove)
        {
            try
            {
                if (!CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress)
                {
                    if (isOnlyRemove || CurrentMassUpdAttribDataContext.SelectedCreoColor != null)
                    {
                        if (CurrentMassUpdAttribDataContext.ShownCadModels.Where((item) => item.IsSelected).Count() > 0)
                        {
                            RaiseActionInProgressEvent();
                            CurrentMassUpdAttribDataContext.IsLoadedFromCreo = true;

                            ListModelThread = new Thread(new ThreadStart(() => { RemoveUpdateColorAsynch(isOnlyRemove); }));
                            ListModelThread.IsBackground = true;
                            ListModelThread.Start();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("MUA_MsgNoCadSelected"), McgWpfTools.GetStringResource("MUA_TitleNoCadSelected"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("MUA_MsgNoAppearence"), McgWpfTools.GetStringResource("MUA_TitleNoAppearence"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else if (System.Windows.MessageBox.Show(McgWpfTools.GetStringResource("MUA_AbordProcessMsg"), McgWpfTools.GetStringResource("MUA_AbordProcessTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes && ListModelThread != null)
                {
                    StopCurrentProcess = true;
                    CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                }
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDeleteSelectedCadDoc()
        {
            try
            {
                var toBeRemoved = CurrentMassUpdAttribDataContext.ShownCadModels.Where(item => item.IsSelected).ToList();
                foreach (var cad in toBeRemoved)
                    CurrentMassUpdAttribDataContext.ShownCadModels.Remove(cad);
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartRename()
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentMassUpdAttribDataContext.NewTerm))
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("MUA_LabeStartRename"), McgWpfTools.GetStringResource("MUA_TitleWindowRename"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        List<RestOdataWtObject> tempList = new List<RestOdataWtObject>();
                        foreach (var item in CurrentMassUpdAttribDataContext.ListToBeRenamedObject.Where(item => item.ToBeRenamed))
                            tempList.Add(item.OdataObject);
                        CheckWindchillCredential();

                        _windchillCustomManagementService.RenameWtObject(WindchillNetworkCredential.WindchillCredential, tempList, CurrentMassUpdAttribDataContext.NewTerm, CommonLibConstants.WindchillUrl);

                        // ExecuteClose();
                    }
                }
                else
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("MUA_LabeSelectTerm"), McgWpfTools.GetStringResource("MUA_TitleWindowRename"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemove()
        {
            try
            {
                foreach (var item in CurrentMassUpdAttribDataContext.ListToBeRenamedObject.Where(item => item.IsSelected))
                    item.ToBeRenamed = !item.ToBeRenamed;
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                StopCurrentProcess = false;
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods Data Management
        private void UpdateListWebterm()
        {
            try
            {
                var temWebtermList = _webtermTools.GetListTerm(WebtermLanguage.ENGLISH, null, null).OrderBy((elem) => elem);

                foreach (var term in temWebtermList)
                    CurrentMassUpdAttribDataContext.WebtermList.Add(term);
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void ApplyWebtermSelectedItem()
        {
            try
            {
                foreach (var elem in CurrentMassUpdAttribDataContext.ShownCadModels.Where((elem) => elem.IsSelected))
                {
                    CurrentMassUpdAttribDataContext.SelectedItem = elem;
                    ApplyWebtermToSelected();
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void ApplyWebtermToSelected()
        {
            try
            {
                if (CurrentMassUpdAttribDataContext.SelectedItem != null)
                {
                    string CurrentGenericAttrib = CurrentMassUpdAttribDataContext.ListColumns.Where((elem) => elem.AttributeID == "DESCRIPTION2_1").Select((elem) => elem.ClassAttributeID).FirstOrDefault();
                    string CurrentValue;
                    var CurrentItem = CurrentMassUpdAttribDataContext.SelectedItem;

                    if (CurrentGenericAttrib != null)
                        if (CurrentMassUpdAttribDataContext.CurrentLanguage == "ENGLISH")
                            CurrentItem.GetType().GetProperty(CurrentGenericAttrib).SetValue(CurrentItem, "-");
                        else
                        {
                            CurrentValue = _webtermTools.GetTerm(CurrentItem.PTC_COMMON_NAME, WebtermLanguage.ENGLISH, _webtermTools.GetWebtermLanguage(CurrentMassUpdAttribDataContext.CurrentLanguage));
                            if (CurrentValue != null && CurrentItem.GetType().GetProperty(CurrentGenericAttrib) != null)
                                CurrentItem.GetType().GetProperty(CurrentGenericAttrib).SetValue(CurrentItem, CurrentValue);
                        }
                }

            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void ResetUpdateSelectedItem()
        {
            try
            {
                if (CurrentMassUpdAttribDataContext.SelectedItem != null)
                    foreach (var attrib in CurrentMassUpdAttribDataContext.SelectedItem.ListAttribute)
                    {
                        attrib.NewValue = attrib.OldValue;
                        if (CurrentMassUpdAttribDataContext.SelectedItem.GetType().GetProperty(attrib.ParentAttribute.ClassAttributeID) != null)
                            CurrentMassUpdAttribDataContext.SelectedItem.GetType().GetProperty(attrib.ParentAttribute.ClassAttributeID).SetValue(CurrentMassUpdAttribDataContext.SelectedItem, attrib.OldValue);
                    }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void ResetUpdateAllSelectedItem()
        {
            try
            {
                var ListCadSelected = CurrentMassUpdAttribDataContext.ShownCadModels.Where((elem) => elem.IsSelected);
                foreach (var elem in ListCadSelected)
                {
                    CurrentMassUpdAttribDataContext.SelectedItem = elem;
                    ResetUpdateSelectedItem();
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void ApplyToAllSamePartNumber()
        {
            try
            {
                if (CurrentMassUpdAttribDataContext.SelectedItem != null)
                {
                    var ListAllSamePartNumber = CurrentMassUpdAttribDataContext.ShownCadModels.Where((elem) => elem.BasePartNumber == CurrentMassUpdAttribDataContext.SelectedItem.BasePartNumber && elem.BasePartNumber != "NOTFOUND");
                    MassUpdateAttributeItem CurrentItem = CurrentMassUpdAttribDataContext.SelectedItem;

                    foreach (var CadModel in ListAllSamePartNumber)
                        if (CadModel.PartNumber != CurrentItem.PartNumber)
                        {
                            if (CadModel.IsPtcCommonNameModifiable)
                                CadModel.PTC_COMMON_NAME = CurrentItem.PTC_COMMON_NAME;

                            foreach (McgAttributeColumnHeaderInfo Attrib in CurrentMassUpdAttribDataContext.ListColumns)
                                if (Attrib.AttributeID != "MATERIAL"
                                    && Attrib.AttributeID != "ADDITIONALPUBFORMAT"
                                    && CadModel.GetType().GetProperty(Attrib.ClassAttributeID) != null
                                    && CurrentItem.GetType().GetProperty(Attrib.ClassAttributeID) != null)
                                    CadModel.GetType().GetProperty(Attrib.ClassAttributeID).SetValue(CadModel, CurrentItem.GetType().GetProperty(Attrib.ClassAttributeID).GetValue(CurrentItem));
                        }
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void ApplyValueToSelected(McgAttributeColumnHeaderInfo CurrentColumnHeaderInfo)
        {
            try
            {
                if (CurrentColumnHeaderInfo != null && CurrentColumnHeaderInfo.AttributeValue != null && CurrentColumnHeaderInfo.AttributeValue.Trim() != "")
                {
                    string CurrentGenericAttrib = CurrentColumnHeaderInfo.ClassAttributeID;
                    string CurrentValue = CurrentColumnHeaderInfo.AttributeValue;

                    foreach (var elem in CurrentMassUpdAttribDataContext.ShownCadModels.Where((elem) => elem.IsSelected))
                        if (elem.GetType().GetProperty(CurrentGenericAttrib) != null)
                            elem.GetType().GetProperty(CurrentGenericAttrib).SetValue(elem, CurrentValue);
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateOnItemAttributeFromXlsExport(MassUpdateAttributeItem CurrentItem, MassUpdateAttributeExportItem CurrentExportItem)
        {
            try
            {
                if (CurrentItem != null && CurrentExportItem != null)
                {
                    string CurrentAttribValue = "";
                    MassUpdateAttributeValue CurrentMassUpdateAttributeValue = null;
                    foreach (McgAttributeColumnHeaderInfo newAttrib in CurrentMassUpdAttriConfiguration.ListColumns)
                    {
                        CurrentAttribValue = "";
                        if (newAttrib.AttributeType == AttributeTypeEnum.INT)
                        { }
                        else if (newAttrib.AttributeType == AttributeTypeEnum.REAL)
                        { }
                        else if (newAttrib.AttributeType == AttributeTypeEnum.TEXT)
                        {
                            PropertyInfo PropExportItem = McgMiscTools.GetPropertyFromAttributeValue<MassUpdateAttributeExportItem>("ColumnAttribute", newAttrib.AttributeID);
                            if (PropExportItem != null)
                            {
                                object valueObj = PropExportItem.GetValue(CurrentExportItem);
                                if (valueObj != null)
                                    CurrentAttribValue = valueObj.ToString();
                            }
                        }
                        else
                            CurrentAttribValue = "";
                        if (CurrentAttribValue != null && CurrentAttribValue.Trim() != "")
                        {
                            if (newAttrib.ColumnType == McgColumnType.COMBOBOX && !newAttrib.ListValue.Contains(CurrentAttribValue))
                                CurrentAttribValue = "";
                        }

                        PropertyInfo PropItem = typeof(MassUpdateAttributeItem).GetProperty(newAttrib.ClassAttributeID);
                        if (PropItem != null)
                            PropItem.SetValue(CurrentItem, CurrentAttribValue);


                        CurrentMassUpdateAttributeValue = new MassUpdateAttributeValue(CurrentAttribValue, newAttrib, CurrentExportItem.NUMBER);

                        // Add only attribute without empty value should be shown as updated
                        if (CurrentAttribValue != null && CurrentAttribValue.Trim() != "")
                            CurrentMassUpdateAttributeValue.IsUpdated = true;
                        else
                            CurrentMassUpdateAttributeValue.IsUpdated = false;

                        CurrentItem.ListAttribute.Add(CurrentMassUpdateAttributeValue);

                        CurrentItem.Status = McgWpfTools.GetStringResource("MUA_UpdateStatus01");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods Creo Interaction
        private void SearchListModelsInSession()
        {
            try
            {
                CurrentMassUpdAttribDataContext.TextStatusBar = McgWpfTools.GetStringResource("MUA_SearchCadDocInProgress");
                CurrentMassUpdAttribDataContext.MessageModelsInSessionInProgress = McgWpfTools.GetStringResource("MUA_SearchCadDocInProgressMsg");
                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = true;

                _creoSessionProvider.CheckConnection();
                _creoModelService.SearchModelsInSession();

                List<object> ListModels;
                if (CurrentMassUpdAttribDataContext.IsOnlyActiveModel)
                {
                    GetActiveModelDependenciesAsynch();
                    MainDispatcher.Invoke(new Action(UpdateShownModelsList));
                    if (StopCurrentProcess)
                    {
                        CurrentMassUpdAttribDataContext.AllCadModels.Clear();
                        CurrentMassUpdAttribDataContext.NbModelsInSession = 1;
                        CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress = 0;
                        StopCurrentProcess = false;
                        MainDispatcher.Invoke(new Action(UpdateShownModelsList));
                        return;
                    }
                }
                else
                {
                    if (CurrentMassUpdAttribDataContext.IsOnlyDisplayedModels)
                        ListModels = _creoModelService.ListModelsWindow;
                    else
                        ListModels = _creoModelService.ListModels;

                    if (ListModels != null)
                    {

                        MassUpdateAttributeItem CurrentMassUpdateAttributeItem = null;

                        CurrentMassUpdAttribDataContext.AllCadModels.Clear();
                        CurrentMassUpdAttribDataContext.NbModelsInSession = ListModels.Count;
                        CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress = 0;

                        Regex RegexCadDoc = new Regex(@"\.prt|\.asm|\.drw", RegexOptions.IgnoreCase);
                        int Index = 0;

                        foreach (IpfcModel CurrentModel in ListModels)
                        {
                            if (RegexCadDoc.IsMatch(CurrentModel.FileName))
                            {

                                if (!StopCurrentProcess)
                                {
                                    CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress = Index;
                                    CurrentMassUpdateAttributeItem = SearchCadModelInformation(CurrentModel);
                                    CurrentMassUpdAttribDataContext.AllCadModels.Add(CurrentMassUpdateAttributeItem);

                                    Index++;
                                }
                                else
                                {
                                    CurrentMassUpdAttribDataContext.AllCadModels.Clear();
                                    CurrentMassUpdAttribDataContext.NbModelsInSession = 1;
                                    CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress = 0;
                                    StopCurrentProcess = false;
                                    MainDispatcher.Invoke(new Action(UpdateShownModelsList));
                                    return;
                                }
                            }
                        }

                        MainDispatcher.Invoke(new Action(UpdateShownModelsList));
                    }
                }
                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress = 0;
                CurrentMassUpdAttribDataContext.TextStatusBar = "";

            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void GetActiveModelDependenciesAsynch()
        {
            try
            {
                CurrentMassUpdAttribDataContext.AllCadModels.Clear();
                CurrentMassUpdAttribDataContext.NbModelsInSession = 0;
                CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress = 0;

                // Check if Active Model available (3D)
                IpfcModel ActiveModel = _creoModelService.GetActiveModel();

                // if Active model not found, check if currentWindow is a drawxing
                if (ActiveModel == null)
                {
                    var CurrentMWindow = _creoSessionProvider.Session.get_CurrentWindow();
                    if (CurrentMWindow != null)
                        ActiveModel = CurrentMWindow.Model;
                }
                if (ActiveModel != null)
                {
                    CurrentMassUpdAttribDataContext.AllCadModels.Add(SearchCadModelInformation(ActiveModel));

                    var tempList = _creoModelService.GetOpenModels();
                    ListCadModelInSession = new List<IpfcModel>();
                    if (tempList != null && tempList.Count > 0)
                    {
                        foreach (var cadModel in tempList)
                        {
                            ListCadModelInSession.Add((IpfcModel)cadModel);
                        }
                    }

                    GetAllDependenciesRecursive(ActiveModel);

                    // Check if drawing for active model+compo is in session
                    GetAllDependenciesDrawing();
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void GetAllDependenciesRecursive(IpfcModel CurrentModel)
        {
            try
            {
                if (StopCurrentProcess) return;

                if (CurrentModel == null) return;

                List<string> allCadFileName = new List<string>();

                Dictionary<string, int> instanceNameCountMap = new Dictionary<string, int>();
                IpfcSolid solid = CurrentModel as IpfcSolid;
                IpfcDrawing drawing = CurrentModel as IpfcDrawing;

                if (drawing != null)
                {
                    IpfcDependencies AllDependencies = CurrentModel.ListDependencies();
                    if (AllDependencies != null)
                    {
                        CurrentMassUpdAttribDataContext.NbModelsInSession += AllDependencies.Count;
                        IpfcDependency TempDep = null;
                        IpfcModelDescriptor TempModDesc = null;
                        IpfcModel TempModel = null;
                        foreach (var item in AllDependencies)
                        {

                            if (StopCurrentProcess) return;

                            TempDep = (IpfcDependency)item;
                            TempModDesc = TempDep.DepModel;
                            TempModel = _creoSessionProvider.Session.GetModelFromDescr(TempModDesc);
                            if (TempModel != null)
                            {
                                if (!allCadFileName.Any(asm => asm == TempModel.FileName))
                                {
                                    allCadFileName.Add(TempModel.FileName);
                                    CurrentMassUpdAttribDataContext.AllCadModels.Add(SearchCadModelInformation(TempModel));
                                    // Assembly
                                    if (TempModel.Type == (int)EpfcModelType.EpfcMDL_ASSEMBLY)
                                        GetAllDependenciesRecursive(TempModel);
                                }
                            }
                            CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress++;
                        }
                    }
                }

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
                    CurrentMassUpdAttribDataContext.NbModelsInSession += instanceNameCountMap.Count;


                    IpfcModel TempModel = null;
                    foreach (var comp in instanceNameCountMap)
                    {
                        //TempModel = CurrentCREOConnection.RetrieveModelFromStdDir(comp.Key);

                        TempModel = ListCadModelInSession.FirstOrDefault(cad => cad.FileName == comp.Key);

                        if (TempModel != null)
                        {
                            if (!allCadFileName.Any(asm => asm == comp.Key))
                            {
                                allCadFileName.Add(comp.Key);
                                CurrentMassUpdAttribDataContext.AllCadModels.Add(SearchCadModelInformation(TempModel));
                                // Assembly
                                if (TempModel.Type == (int)EpfcModelType.EpfcMDL_ASSEMBLY)
                                    GetAllDependenciesRecursive(TempModel);
                            }
                        }
                        CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void GetAllDependenciesDrawing()
        {
            try
            {
                _creoModelService.SearchModelsInSession();
                var ListModels = _creoModelService.ListModels;
                Dictionary<string, IpfcModel> ListModelsDic = new Dictionary<string, IpfcModel>();
                foreach (var item in ListModels)
                    ListModelsDic.Add(((IpfcModel)item).FileName, (IpfcModel)item);

                List<MassUpdateAttributeItem> NewList = new List<MassUpdateAttributeItem>();
                Regex RegexNumber = null;

                foreach (var cad in CurrentMassUpdAttribDataContext.AllCadModels)
                {
                    RegexNumber = new Regex($@"^{cad.BasePartNumber}.+drw$", RegexOptions.IgnoreCase);
                    var TempList = ListModelsDic.Where((item) => RegexNumber.IsMatch(item.Key)).ToList();
                    if (TempList != null)
                        foreach (var extraCad in TempList)
                            if (!CurrentMassUpdAttribDataContext.AllCadModels.Any((item) => item.PartNumber == extraCad.Key) && !NewList.Any((item) => item.PartNumber == extraCad.Key))
                            {
                                NewList.Add(SearchCadModelInformation(extraCad.Value));
                            }
                }

                foreach (var cad in NewList)
                    CurrentMassUpdAttribDataContext.AllCadModels.Add(cad);
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private MassUpdateAttributeItem SearchCadModelInformation(IpfcModel CurrentModel)
        {
            try
            {
                MassUpdateAttributeItem CurrentMassUpdateAttributeItem = new MassUpdateAttributeItem();
                MassUpdateAttributeValue CurrentMassUpdateAttributeValue = null;
                CREOModelStatus CurrentCREOModelStatus;
                string CurrentAttribValue = "";

                CurrentMassUpdateAttributeItem.CurrentCadModel = CurrentModel;
                CurrentMassUpdateAttributeItem.PartNumber = CurrentModel.FileName;
                CurrentMassUpdateAttributeItem.PTC_COMMON_NAME = CurrentModel.CommonName;
                CurrentMassUpdateAttributeValue = new MassUpdateAttributeValue(CurrentModel.CommonName, new McgAttributeColumnHeaderInfo()
                {
                    AttributeID = "PTC_COMMON_NAME",
                    ClassAttributeID = "PTC_COMMON_NAME",
                    AttributeName = "PTC_COMMON_NAME",
                    AttributeType = AttributeTypeEnum.TEXT,
                    AttributeValue = ""
                },
                CurrentModel.FileName);

                CurrentMassUpdateAttributeItem.ListAttribute.Add(CurrentMassUpdateAttributeValue);

                // check the partnumber
                WindchillCheckedObject CurrentWindchillCheckedObject = _windchillCheckNumberService.CheckObject(CurrentMassUpdateAttributeItem.PartNumber, WindchillObjectType.CADDOC, _windchillCheckNumberService.GetWindchillNamingConvention(NamingConventionXmlFile));
                CurrentMassUpdateAttributeItem.BasePartNumberTemplate = CurrentWindchillCheckedObject.NumberTemplate;
                CurrentMassUpdateAttributeItem.CurrentWindchillCheckedObject = CurrentWindchillCheckedObject;

                if (CurrentMassUpdateAttributeItem.BasePartNumberTemplate == null)
                    CurrentMassUpdateAttributeItem.IsBasePartNumberFound = false;
                else
                {
                    CurrentMassUpdateAttributeItem.IsBasePartNumberFound = true;
                    CurrentMassUpdateAttributeItem.BasePartNumber = CurrentWindchillCheckedObject.ExtractedNumber;
                }

                CurrentMassUpdateAttributeItem.IsCheckedOut = false;
                CurrentMassUpdateAttributeItem.IsSelected = false;
                CurrentMassUpdateAttributeItem.IsPtcCommonNameModifiable = false;
                CurrentCREOModelStatus = _creoModelService.GetModelStatus(CurrentModel);

                if (CurrentCREOModelStatus == CREOModelStatus.CHECKEDOUT || CurrentCREOModelStatus == CREOModelStatus.NEWINSESSION)
                {
                    CurrentMassUpdateAttributeItem.IsCheckedOut = true;
                    CurrentMassUpdateAttributeItem.IsModifiable = true;
                    if (CurrentCREOModelStatus == CREOModelStatus.NEWINSESSION)
                    {
                        CurrentMassUpdateAttributeItem.IsPtcCommonNameModifiable = true;
                        CurrentMassUpdateAttributeItem.WebtermList = CurrentMassUpdAttribDataContext.WebtermList;
                    }
                }
                else if (CurrentCREOModelStatus == CREOModelStatus.LOCALLYMODIFIED)
                {
                    CurrentMassUpdateAttributeItem.IsLocallyModified = true;
                    CurrentMassUpdateAttributeItem.IsModifiable = true;
                }
                else
                {
                    CurrentMassUpdateAttributeItem.IsReadOnly = true;
                    CurrentMassUpdateAttributeItem.IsModifiable = false;
                }

                CurrentMassUpdateAttributeItem.ListBrandGroupSubGroup = ListBrandGroupSubGroup;
                CurrentMassUpdateAttributeItem.IsUpdated = false;


                //McgAttributeColumnHeaderInfo newAttrib;
                foreach (McgAttributeColumnHeaderInfo newAttrib in CurrentMassUpdAttriConfiguration.ListColumns)
                {
                    if (newAttrib.AttributeType == AttributeTypeEnum.INT)
                        CurrentAttribValue = _creoParameterService.GetParameterAsDouble(CurrentModel, newAttrib.AttributeID).ToString();
                    else if (newAttrib.AttributeType == AttributeTypeEnum.REAL)
                        CurrentAttribValue = _creoParameterService.GetParameterAsDouble(CurrentModel, newAttrib.AttributeID).ToString();
                    else if (newAttrib.AttributeType == AttributeTypeEnum.TEXT)
                    {
                        // check if MATERIAL param to search for the filename of the meterial instead of the param

                        if (newAttrib.AttributeID == "MATERIAL" && CurrentModel.FileName.Contains(".prt"))
                        {
                            CurrentAttribValue = _creoFeatureService.GetCurrentMaterialFileName(CurrentModel);
                        }
                        else
                        {
                            CurrentAttribValue = _creoParameterService.GetParameterAsString(CurrentModel, newAttrib.AttributeID);
                        }
                    }
                    else
                        CurrentAttribValue = "";

                    CurrentMassUpdateAttributeValue = new MassUpdateAttributeValue(CurrentAttribValue, newAttrib, CurrentModel.FileName);

                    // Update generic properties PARAMSTRxx with accurate value
                    if (CurrentMassUpdateAttributeItem.GetType().GetProperty(newAttrib.ClassAttributeID) != null)
                        CurrentMassUpdateAttributeItem.GetType().GetProperty(newAttrib.ClassAttributeID).SetValue(CurrentMassUpdateAttributeItem, CurrentAttribValue);

                    CurrentMassUpdateAttributeItem.ListAttribute.Add(CurrentMassUpdateAttributeValue);
                }


                return CurrentMassUpdateAttributeItem;
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }

        }

        private void UpdateShownModelsList()
        {
            try
            {
                foreach (var elem in CurrentMassUpdAttribDataContext.ShownCadModels)
                    elem.PropertyChanged -= MassUpdateAttributeItem_PropertyChanged;

                CurrentMassUpdAttribDataContext.UpdateShownModelsList();

                foreach (var elem in CurrentMassUpdAttribDataContext.ShownCadModels)
                    elem.PropertyChanged += MassUpdateAttributeItem_PropertyChanged;
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void MassUpdateAttributeItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(MassUpdateAttributeItem) && e.PropertyName == "PTC_COMMON_NAME")
                    ApplyWebtermToSelected();
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateAllCadDocuments()
        {
            try
            {
                CurrentMassUpdAttribDataContext.TextStatusBar = McgWpfTools.GetStringResource("MUA_UpdateCadDocInProgress");
                CurrentMassUpdAttribDataContext.MessageModelsInSessionInProgress = McgWpfTools.GetStringResource("MUA_UpdateCadDocInProgressMsg");
                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = true;
                CurrentMassUpdAttribDataContext.NbModelsInSession = CurrentMassUpdAttribDataContext.ShownCadModels.Where((item) => item.IsUpdated).Count();
                CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress = 0;

                _creoSessionProvider.CheckConnection();

                var ListCadToUpdate = CurrentMassUpdAttribDataContext.ShownCadModels.Where((elem) => elem.IsUpdated).ToList();
                //CurrentCREOConnection.session.SetConfigOption("regen_failure_handling", "resolve_mode");

                foreach (var CadDoc in ListCadToUpdate)
                {
                    CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress++;
                    UpdateCadDocumentAttribute(CadDoc);
                }

                //CurrentCREOConnection.session.SetConfigOption("regen_failure_handling", "no_resolve_mode");
                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
            }
            catch (Exception ex)
            {
                _creoSessionProvider.CheckConnection();
                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void UpdateOneCadDocument()
        {
            try
            {
                _creoSessionProvider.CheckConnection();
                UpdateCadDocumentAttribute(CurrentMassUpdAttribDataContext.SelectedItem);
            }
            catch (Exception ex)
            {
                _creoSessionProvider.CheckConnection();
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateCadDocumentAttribute(MassUpdateAttributeItem CadDoc)
        {
            try
            {
                if (StopCurrentProcess)
                {
                    StopCurrentProcess = false;
                    return;
                }

                TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: -------------------- {CadDoc.PartNumber} --------------------");

                IpfcModel CurrentCadModel = null;
                IpfcModel2D Model2D = null;
                IpfcSolid SolidModel = null;
                bool Is3DModel = true;
                bool ToBeRegenerate = true;

                CadDoc.IsUpdateInProgress = true;

                try
                {
                    // Check that CAD document is still in session
                    TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: Check Cad Doc {CadDoc.PartNumber}");
                    if (CadDoc.PartNumber.IndexOf(".drw") > 0)
                    {
                        Is3DModel = false;
                        CurrentCadModel = _creoModelService.RetrieveModel(CadDoc.PartNumber, EpfcModelType.EpfcMDL_DRAWING);
                        TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: EpfcModelType.EpfcMDL_DRAWING");
                    }
                    else if (CadDoc.PartNumber.IndexOf(".asm") > 0)
                    {
                        Is3DModel = true;
                        CurrentCadModel = _creoModelService.RetrieveModel(CadDoc.PartNumber, EpfcModelType.EpfcMDL_ASSEMBLY);
                        TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: EpfcModelType.EpfcMDL_ASSEMBLY");
                    }
                    else if (CadDoc.PartNumber.IndexOf(".prt") > 0)
                    {
                        Is3DModel = true;
                        CurrentCadModel = _creoModelService.RetrieveModel(CadDoc.PartNumber, EpfcModelType.EpfcMDL_PART);
                        TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: EpfcModelType.EpfcMDL_PART");
                    }

                    if (CurrentCadModel == null)
                    {
                        CadDoc.Status = "CAD Doc not found";
                        TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: CurrentCadModel null");
                        return;
                    }
                }
                catch (CREORetrieveModelException)
                {
                    CadDoc.Status = McgWpfTools.GetStringResource("MUA_UpdateStatus03");
                    TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: CurrentCadModel null");
                    return;
                }


                if (Is3DModel)
                    SolidModel = (IpfcSolid)CurrentCadModel;
                else
                    Model2D = (IpfcModel2D)CurrentCadModel;


                if (CadDoc.FromExcelImport)
                {
                    // force regen to put the model in locally modified

                    if (Is3DModel)
                    {
                        SolidModel.Regenerate(null);
                        CurrentCadModel.Save();
                    }
                    else
                        try
                        {
                            Model2D.Regenerate();
                        }
                        catch { }
                }

                string paramValue;
                string paramName;
                foreach (var attrib in CadDoc.ListAttribute)
                {
                    // Update only if Attribute has been updated
                    if (attrib.IsUpdated)
                    {


                        // Check if Attrubte is Material
                        paramName = attrib.ParentAttribute.AttributeID;
                        paramValue = attrib.NewValue;
                        TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: Update Attrib {paramName}  with {paramValue}");

                        if (paramName == "MATERIAL")
                        {
                            // Assigned MATERIAL directly in the param, if no relation drive it
                            _creoParameterService.SetParameter(CurrentCadModel, paramName, paramValue, attrib.ParentAttribute.IsDesignated);
                            TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: Updated {paramName}");
                            if (CadDoc.PartNumber.Contains(".prt") && paramValue != "NONE")
                            {
                                // Assigned the Material to the PRT
                                if (!_creoFeatureService.AssignMaterial(CurrentCadModel, paramValue, true))
                                    System.Windows.MessageBox.Show(String.Format(McgWpfTools.GetStringResource("MUA_MsgIssueAssignMaterial"), attrib.NewValue, CadDoc));
                                ToBeRegenerate = true;
                                TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: Assigned {paramName} to PRT");
                            }
                        }
                        else if (paramName == "PTC_COMMON_NAME")
                        {
                            if (CadDoc.IsPtcCommonNameModifiable)
                            {
                                _creoParameterService.SetParameter(CurrentCadModel, paramName, paramValue);
                                ToBeRegenerate = true;
                                TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: Changed {paramName}");
                            }
                        }
                        else
                        {
                            if (attrib.ParentAttribute.AttributeType == AttributeTypeEnum.INT || attrib.ParentAttribute.AttributeType == AttributeTypeEnum.REAL)
                                _creoParameterService.SetParameter(CurrentCadModel, paramName, paramValue, attrib.ParentAttribute.IsDesignated);
                            else if (attrib.ParentAttribute.AttributeType == AttributeTypeEnum.TEXT)
                                _creoParameterService.SetParameter(CurrentCadModel, paramName, paramValue, attrib.ParentAttribute.IsDesignated);
                            TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: Updated {paramName}");
                        }
                        attrib.OldValue = attrib.NewValue;

                        // Update generic properties PARAMSTRxx with accurate value
                        if (CadDoc.GetType().GetProperty(attrib.ParentAttribute.ClassAttributeID) != null)
                            CadDoc.GetType().GetProperty(attrib.ParentAttribute.ClassAttributeID).SetValue(CadDoc, attrib.OldValue);
                    }
                }

                // regenretare Model
                TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: Generating CAD Model");
                if (ToBeRegenerate)
                    if (Is3DModel)
                        SolidModel.Regenerate(null);
                    else
                        try
                        {
                            Model2D.Regenerate();
                        }
                        catch { }

                TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: Generated CAD Model");

                // Save CAD Document if check box save is checked
                // if (CurrentMassUpdAttribDataContext.IsSaveCadDoc)
                try
                {
                    CurrentCadModel.Save();
                    if (CadDoc.FromExcelImport)
                        CadDoc.IsUpdated = false;

                    CadDoc.Status = McgWpfTools.GetStringResource("MUA_UpdateStatus02");
                }
                catch (Exception)
                {
                    CadDoc.Status = McgWpfTools.GetStringResource("MUA_UpdateStatus04");
                }

                CadDoc.IsUpdateInProgress = false;

                TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: Saved CAD Model {CadDoc.PartNumber}");
                Thread.Sleep(1000);

            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateParamRelationAsynch()
        {
            try
            {
                if (StopCurrentProcess)
                {
                    StopCurrentProcess = false;
                    return;
                }

                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = true;
                CurrentMassUpdAttribDataContext.NbModelsInSession = CurrentMassUpdAttribDataContext.ShownCadModels.Where((item) => item.IsSelected).Count();
                CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress = 0;

                IpfcModel CurrentCadModel = null;
                IpfcModel TemplateCadModel = null;
                IpfcModel2D Model2D = null;
                IpfcSolid SolidModel = null;
                bool Is3DModel = true;


                foreach (var CadDoc in CurrentMassUpdAttribDataContext.ShownCadModels.Where((item) => item.IsSelected))
                {
                    CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress++;
                    try
                    {
                        // Check that CAD document is still in session
                        TraceLog.AddTraceLog($"UpdateParamRelationAsynch: Check Cad Doc {CadDoc.PartNumber}");
                        if (CadDoc.PartNumber.IndexOf(".drw") > 0)
                        {
                            Is3DModel = false;
                            CurrentCadModel = _creoModelService.RetrieveModel(CadDoc.PartNumber, EpfcModelType.EpfcMDL_DRAWING);
                            TraceLog.AddTraceLog($"UpdateParamRelationAsynch: EpfcModelType.EpfcMDL_DRAWING");
                        }
                        else if (CadDoc.PartNumber.IndexOf(".asm") > 0)
                        {
                            Is3DModel = true;
                            CurrentCadModel = _creoModelService.RetrieveModel(CadDoc.PartNumber, EpfcModelType.EpfcMDL_ASSEMBLY);
                            TraceLog.AddTraceLog($"UpdateParamRelationAsynch: EpfcModelType.EpfcMDL_ASSEMBLY");
                        }
                        else if (CadDoc.PartNumber.IndexOf(".prt") > 0)
                        {
                            Is3DModel = true;
                            CurrentCadModel = _creoModelService.RetrieveModel(CadDoc.PartNumber, EpfcModelType.EpfcMDL_PART);
                            TraceLog.AddTraceLog($"UpdateParamRelationAsynch: EpfcModelType.EpfcMDL_PART");
                        }
                        TemplateCadModel = GetCadTemplate(CurrentCadModel);

                    }
                    catch (CREORetrieveModelException)
                    {
                        CadDoc.Status = McgWpfTools.GetStringResource("MUA_UpdateStatus03");
                        TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: CurrentCadModel null");
                        CurrentCadModel = null;
                    }

                    if (CurrentCadModel != null && TemplateCadModel != null)
                    {
                        _creoParameterService.UpdateRelationsAndParametersFromTemplate(CurrentCadModel, TemplateCadModel);
                    }


                    // regenerate Model
                    TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: Generating CAD Model");
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

                    TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: Generated CAD Model");

                    // Save CAD Document if check box save is checked
                    // if (CurrentMassUpdAttribDataContext.IsSaveCadDoc)
                    CurrentCadModel.Save();
                    if (CadDoc.FromExcelImport)
                        CadDoc.IsUpdated = false;

                    CadDoc.Status = McgWpfTools.GetStringResource("MUA_UpdateStatus02");

                    TraceLog.AddTraceLog($"UpdateCadDocumentAttribute: Saved CAD Model {CadDoc.PartNumber}");

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                RaiseActionDoneEvent();
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

        private void UpdateLayersAsynch()
        {
            try
            {
                if (StopCurrentProcess)
                {
                    StopCurrentProcess = false;
                    return;
                }

                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = true;
                CurrentMassUpdAttribDataContext.NbModelsInSession = CurrentMassUpdAttribDataContext.ShownCadModels.Where((item) => item.IsSelected).Count();
                CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress = 0;

                IpfcModel CurrentCadModel = null;
                IpfcModel TemplateCadModel = null;
                IpfcModel2D Model2D = null;
                IpfcSolid SolidModel = null;
                bool Is3DModel = true;


                foreach (var CadDoc in CurrentMassUpdAttribDataContext.ShownCadModels.Where((item) => item.IsSelected))
                {
                    CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress++;
                    try
                    {
                        // Check that CAD document is still in session
                        TraceLog.AddTraceLog($"UpdateLayersAsynch: Check Cad Doc {CadDoc.PartNumber}");
                        if (CadDoc.PartNumber.IndexOf(".drw") > 0)
                        {
                            Is3DModel = false;
                            CurrentCadModel = _creoModelService.RetrieveModel(CadDoc.PartNumber, EpfcModelType.EpfcMDL_DRAWING);
                            TemplateCadModel = _creoModelService.RetrieveModel(CommonLibConstants.TemplateCadDrw, EpfcModelType.EpfcMDL_DRAWING);
                            TraceLog.AddTraceLog($"UpdateLayersAsynch: EpfcModelType.EpfcMDL_DRAWING");
                        }
                        else if (CadDoc.PartNumber.IndexOf(".asm") > 0)
                        {
                            Is3DModel = true;
                            CurrentCadModel = _creoModelService.RetrieveModel(CadDoc.PartNumber, EpfcModelType.EpfcMDL_ASSEMBLY);
                            TemplateCadModel = _creoModelService.RetrieveModel(CommonLibConstants.TemplateCadAsm, EpfcModelType.EpfcMDL_ASSEMBLY);
                            TraceLog.AddTraceLog($"UpdateLayersAsynch: EpfcModelType.EpfcMDL_ASSEMBLY");
                        }
                        else if (CadDoc.PartNumber.IndexOf(".prt") > 0)
                        {
                            Is3DModel = true;
                            CurrentCadModel = _creoModelService.RetrieveModel(CadDoc.PartNumber, EpfcModelType.EpfcMDL_PART);
                            TemplateCadModel = _creoModelService.RetrieveModel(CommonLibConstants.TemplateCadPrt, EpfcModelType.EpfcMDL_PART);
                            TraceLog.AddTraceLog($"UpdateLayersAsynch: EpfcModelType.EpfcMDL_PART");
                        }

                    }
                    catch (CREORetrieveModelException)
                    {
                        CadDoc.Status = McgWpfTools.GetStringResource("MUA_UpdateStatus03");
                        TraceLog.AddTraceLog($"UpdateLayersAsynch: CurrentCadModel null");
                        CurrentCadModel = null;
                    }



                    if (CurrentCadModel != null && !_creoFeatureService.IsBulkItem(CurrentCadModel))
                    {
                        if (CurrentCadModel != null && TemplateCadModel != null)
                        {
                            if (!CurrentMassUpdAttribDataContext.IsTemplateInformationReaded)
                                GetTemplateInformation();
                            GetCadDocLayers(CadDoc);
                            CheckCadDocLayers(CadDoc);
                            UpdateCadDocLayers(CadDoc);
                        }

                        // regenretare Model
                        TraceLog.AddTraceLog($"UpdateLayersAsynch: Generating CAD Model");
                        if (Is3DModel)
                            SolidModel = (IpfcSolid)CurrentCadModel;
                        else
                            Model2D = (IpfcModel2D)CurrentCadModel;

                        if (Is3DModel)
                            try
                            {
                                SolidModel.Regenerate(null);
                            }
                            catch { }
                        else
                            try
                            {
                                Model2D.Regenerate();
                            }
                            catch { }

                        TraceLog.AddTraceLog($"UpdateLayersAsynch: Generated CAD Model");

                        // Save CAD Document if check box save is checked
                        // if (CurrentMassUpdAttribDataContext.IsSaveCadDoc)
                        if (CurrentCadModel != null)
                            CurrentCadModel.Save();
                        if (CadDoc.FromExcelImport)
                            CadDoc.IsUpdated = false;

                        CadDoc.Status = McgWpfTools.GetStringResource("MUA_UpdateStatus02");

                        TraceLog.AddTraceLog($"UpdateLayersAsynch: Saved CAD Model {CadDoc.PartNumber}");

                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                RaiseActionDoneEvent();
            }
        }

        private void GetCadDocLayers(MassUpdateAttributeItem CurrentItem)
        {
            try
            {
                IpfcModelItems CadModelItems;

                CadModelItems = ((IpfcModelItemOwner)CurrentItem.CurrentCadModel).ListItems(EpfcModelItemType.EpfcITEM_LAYER);
                if (CadModelItems != null)
                {
                    CurrentItem.ListLayers.Clear();
                    foreach (var layer in CadModelItems)
                    {
                        CurrentItem.ListLayers.Add(new CadDocLayerItem()
                        {
                            LayerItem = (IpfcLayer)layer,
                            Name = ((IpfcModelItem)layer).GetName(),
                            State = ObjectState.CREATED,
                            DisplayStatus = (EpfcDisplayStatus)((IpfcLayer)layer).Status,
                            IsDisplayed = ((EpfcDisplayStatus)((IpfcLayer)layer).Status) == EpfcDisplayStatus.EpfcLAYER_NORMAL
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void CheckCadDocLayers(MassUpdateAttributeItem CurrentItem)
        {
            try
            {

                var ListLayerToRemove = CurrentItem.ListLayers.Where((layer) => !CurrentMassUpdAttriConfiguration.ListStandardLayers.Select((item) => item.Name).Contains(layer.Name)).ToList();
                foreach (var layer in ListLayerToRemove)
                {
                    layer.State = ObjectState.TO_BE_REMOVED;
                }

                foreach (var layer in CurrentItem.ListLayers.Where((item) => item.State == ObjectState.CREATED))
                {
                    var tempLayer = CurrentMassUpdAttriConfiguration.ListStandardLayers.FirstOrDefault((item) => item.Name == layer.Name);
                    if (tempLayer.IsDisplayed)
                        layer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_NORMAL;
                    else
                        layer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_BLANK;

                    layer.ListModelItems = _creoLayerService.GetModelItemsFromLayer(layer.LayerItem).Select(item => item.Item).ToList();
                }

                CurrentItem.ListRefAxis = _creoFeatureService.GetRefAxisFromCadModel(CurrentItem.CurrentCadModel);
                CurrentItem.ListRefPlans = _creoFeatureService.GetRefPlanFromCadModel(CurrentItem.CurrentCadModel);
                CurrentItem.ListRefPoints = _creoFeatureService.GetRefPointFromCadModel(CurrentItem.CurrentCadModel);
                CurrentItem.ListRefCSys = _creoFeatureService.GetRefCoordinateSystemFromCadModel(CurrentItem.CurrentCadModel);

                // Check Mandatory Layers that alredy exist: 
                CadDocLayerItem CurrentLayer;
                CREOCadModelItem CurrentModelItem;
                List<CREOCadModelItem> TempList = new List<CREOCadModelItem>();
                if (CurrentItem.ListRefAxis != null)
                    TempList.AddRange(CurrentItem.ListRefAxis);
                if (CurrentItem.ListRefPlans != null)
                    TempList.AddRange(CurrentItem.ListRefPlans);
                if (CurrentItem.ListRefPoints != null)
                    TempList.AddRange(CurrentItem.ListRefPoints);
                if (CurrentItem.ListRefCSys != null)
                    TempList.AddRange(CurrentItem.ListRefCSys);

                foreach (var layer in CurrentMassUpdAttribDataContext.MandatoryLayers)
                {
                    CurrentLayer = CurrentItem.ListLayers.FirstOrDefault(item => item.Name == layer.Name);
                    if (CurrentLayer != null)
                    {
                        CurrentLayer.RefType = layer.RefType;
                        foreach (var item in CurrentLayer.ListModelItems)
                        {
                            CurrentModelItem = null;
                            CurrentModelItem = TempList.FirstOrDefault(elem => ((object)item.GetName()) != DBNull.Value && elem.Name == item.GetName());
                            if (CurrentModelItem != null)
                                CurrentModelItem.IsInLayer = true;
                        }
                    }
                }

                // Add New Layers
                foreach (var layer in CurrentMassUpdAttribDataContext.MandatoryLayers)
                {
                    if (CurrentItem.ListLayers.FirstOrDefault(item => item.Name == layer.Name) == null)
                        CheckIfLayerToBeCreated(CurrentItem, layer.GetCadDocLayerItem());
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void GetTemplateInformation()
        {
            try
            {
                IpfcModel TemplateCadModel = null;
                Regex CommentLineRegex = new Regex(@"^/\*");
                CurrentMassUpdAttribDataContext.IsTemplateInformationReaded = true;
                CREOCadModelItem CSysItem;
                List<CREOCadModelItem> RefPlans;

                CurrentMassUpdAttribDataContext.TemplateMainRefPlans = new List<string>();
                CurrentMassUpdAttribDataContext.TemplateMainCoordSystem = new List<string>();

                TemplateCadModel = _creoModelService.RetrieveModel(CommonLibConstants.TemplateCadAsm, EpfcModelType.EpfcMDL_ASSEMBLY);
                if (TemplateCadModel != null)
                {
                    RefPlans = _creoFeatureService.GetRefPlanFromCadModel(TemplateCadModel);
                    if (RefPlans != null && RefPlans.Count >= 3)
                        for (int i = 0; i < 3; i++)
                            CurrentMassUpdAttribDataContext.TemplateMainRefPlans.Add(RefPlans.ElementAt(i).Name);

                    CSysItem = _creoFeatureService.GetRefCoordinateSystemFromCadModel(TemplateCadModel).FirstOrDefault();
                    if (CSysItem != null)
                        CurrentMassUpdAttribDataContext.TemplateMainCoordSystem.Add(CSysItem.Name);

                    TemplateCadModel.Erase();
                }
                else
                    CurrentMassUpdAttribDataContext.IsTemplateInformationReaded = false;

                TemplateCadModel = _creoModelService.RetrieveModel(CommonLibConstants.TemplateCadPrt, EpfcModelType.EpfcMDL_PART);
                if (TemplateCadModel != null)
                {

                    RefPlans = _creoFeatureService.GetRefPlanFromCadModel(TemplateCadModel);
                    if (RefPlans != null && RefPlans.Count >= 3)
                        for (int i = 0; i < 3; i++)
                            CurrentMassUpdAttribDataContext.TemplateMainRefPlans.Add(RefPlans.ElementAt(i).Name);

                    CSysItem = _creoFeatureService.GetRefCoordinateSystemFromCadModel(TemplateCadModel).FirstOrDefault();
                    if (CSysItem != null)
                        CurrentMassUpdAttribDataContext.TemplateMainCoordSystem.Add(CSysItem.Name);
                    TemplateCadModel.Erase();
                }
                else
                    CurrentMassUpdAttribDataContext.IsTemplateInformationReaded = false;

                TemplateCadModel = _creoModelService.RetrieveModel(CommonLibConstants.TemplateCadSheetMetal, EpfcModelType.EpfcMDL_PART);
                if (TemplateCadModel != null)
                {

                    RefPlans = _creoFeatureService.GetRefPlanFromCadModel(TemplateCadModel);
                    if (RefPlans != null && RefPlans.Count >= 3)
                        for (int i = 0; i < 3; i++)
                            CurrentMassUpdAttribDataContext.TemplateMainRefPlans.Add(RefPlans.ElementAt(i).Name);

                    CSysItem = _creoFeatureService.GetRefCoordinateSystemFromCadModel(TemplateCadModel).FirstOrDefault();
                    if (CSysItem != null)
                        CurrentMassUpdAttribDataContext.TemplateMainCoordSystem.Add(CSysItem.Name);
                    TemplateCadModel.Erase();
                }
                else
                    CurrentMassUpdAttribDataContext.IsTemplateInformationReaded = false;

                TemplateCadModel = _creoModelService.RetrieveModel(CommonLibConstants.TemplateCadBulkItem, EpfcModelType.EpfcMDL_PART);
                if (TemplateCadModel != null)
                {

                    RefPlans = _creoFeatureService.GetRefPlanFromCadModel(TemplateCadModel);
                    if (RefPlans != null && RefPlans.Count >= 3)
                        for (int i = 0; i < 3; i++)
                            CurrentMassUpdAttribDataContext.TemplateMainRefPlans.Add(RefPlans.ElementAt(i).Name);

                    CSysItem = _creoFeatureService.GetRefCoordinateSystemFromCadModel(TemplateCadModel).FirstOrDefault();
                    if (CSysItem != null)
                        CurrentMassUpdAttribDataContext.TemplateMainCoordSystem.Add(CSysItem.Name);
                    TemplateCadModel.Erase();
                }
                else
                    CurrentMassUpdAttribDataContext.IsTemplateInformationReaded = false;

            }
            catch (Exception ex)
            {
                CurrentMassUpdAttribDataContext.IsTemplateInformationReaded = false;
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private bool CheckIfLayerToBeCreated(MassUpdateAttributeItem CurrentItem, CadDocLayerItem Layer)
        {
            try
            {
                List<CREOCadModelItem> ListItems = new List<CREOCadModelItem>();
                CREOCadModelItem CurrentModelItem;
                CadDocLayerItem CurrentLayer;
                switch (Layer.RefType)
                {
                    case "MAINPLAN":
                        foreach (var plan in CurrentMassUpdAttribDataContext.TemplateMainRefPlans)
                        {
                            CurrentModelItem = CurrentItem.ListRefPlans?.FirstOrDefault(item => item.Name == plan);
                            if (CurrentModelItem != null)
                            {
                                ListItems.Add(CurrentModelItem);
                                CurrentModelItem.IsInLayer = true;
                            }
                        }
                        if (ListItems?.Count > 0)
                        {
                            CurrentLayer = new CadDocLayerItem()
                            {
                                IsDisplayed = Layer.IsDisplayed,
                                State = ObjectState.NEW,
                                ListModelItems = new List<IpfcModelItem>(),
                                Name = Layer.Name,
                                RefType = "MAINPLAN"
                            };
                            if (CurrentLayer.IsDisplayed)
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_NORMAL;
                            else
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_BLANK;
                            CurrentLayer.ListModelItems.AddRange(ListItems.Select(layer => layer.Item));
                            CurrentItem.ListLayers.Add(CurrentLayer);

                            return true;
                        }
                        break;
                    case "PLAN":
                        ListItems = CurrentItem.ListRefPlans?.Where(layer => !layer.IsInLayer).ToList();
                        if (ListItems?.Count > 0)
                        {
                            CurrentLayer = new CadDocLayerItem()
                            {
                                IsDisplayed = Layer.IsDisplayed,
                                State = ObjectState.NEW,
                                ListModelItems = new List<IpfcModelItem>(),
                                Name = Layer.Name,
                                RefType = "PLAN"
                            };
                            if (CurrentLayer.IsDisplayed)
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_NORMAL;
                            else
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_BLANK;
                            CurrentLayer.ListModelItems.AddRange(ListItems.Select(layer => layer.Item));
                            CurrentItem.ListLayers.Add(CurrentLayer);
                            return true;
                        }
                        break;
                    case "CSYS":
                        ListItems = CurrentItem.ListRefCSys?.Where(layer => !layer.IsInLayer).ToList();
                        if (ListItems?.Count > 0)
                        {
                            CurrentLayer = new CadDocLayerItem()
                            {
                                IsDisplayed = Layer.IsDisplayed,
                                State = ObjectState.NEW,
                                ListModelItems = new List<IpfcModelItem>(),
                                Name = Layer.Name,
                                RefType = "CSYS"
                            };
                            if (CurrentLayer.IsDisplayed)
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_NORMAL;
                            else
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_BLANK;
                            CurrentLayer.ListModelItems.AddRange(ListItems.Select(layer => layer.Item));
                            CurrentItem.ListLayers.Add(CurrentLayer);
                            return true;
                        }
                        break;
                    case "AXIS":
                        ListItems = CurrentItem.ListRefAxis?.Where(layer => !layer.IsInLayer).ToList();
                        if (ListItems?.Count > 0)
                        {
                            CurrentLayer = new CadDocLayerItem()
                            {
                                IsDisplayed = Layer.IsDisplayed,
                                State = ObjectState.NEW,
                                ListModelItems = new List<IpfcModelItem>(),
                                Name = Layer.Name,
                                RefType = "AXIS"
                            };
                            if (CurrentLayer.IsDisplayed)
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_NORMAL;
                            else
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_BLANK;
                            CurrentLayer.ListModelItems.AddRange(ListItems.Select(layer => layer.Item));
                            CurrentItem.ListLayers.Add(CurrentLayer);
                            return true;
                        }
                        break;
                    case "POINTS":
                        ListItems = CurrentItem.ListRefPoints?.Where(layer => !layer.IsInLayer).ToList();
                        if (ListItems?.Count > 0)
                        {
                            CurrentLayer = new CadDocLayerItem()
                            {
                                IsDisplayed = Layer.IsDisplayed,
                                State = ObjectState.NEW,
                                ListModelItems = new List<IpfcModelItem>(),
                                Name = Layer.Name,
                                RefType = "POINTS"
                            };
                            if (CurrentLayer.IsDisplayed)
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_NORMAL;
                            else
                                CurrentLayer.DisplayStatus = EpfcDisplayStatus.EpfcLAYER_BLANK;
                            CurrentLayer.ListModelItems.AddRange(ListItems.Select(layer => layer.Item));
                            CurrentItem.ListLayers.Add(CurrentLayer);
                            return true;
                        }
                        break;
                    default:
                        break;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateCadDocLayers(MassUpdateAttributeItem CurrentItem)
        {
            try
            {
                if (CurrentItem.CurrentCadModel != null)
                {
                    bool IsDisplayedWindow = false;

                    IsDisplayedWindow = _creoModelService.ActiveCadDocWindow(CurrentItem.CurrentCadModel);

                    foreach (var item in CurrentItem.ListLayers.Where((layer) => layer.State == ObjectState.TO_BE_REMOVED))
                    {
                        try
                        {
                            item.LayerItem.Delete();
                            item.State = ObjectState.REMOVED;
                        }
                        catch (Exception)
                        {

                        }
                    }

                    IpfcLayer CurrentLayer;
                    List<CREOCadModelItem> CurrentListItems;
                    foreach (var item in CurrentItem.ListLayers.Where((layer) => layer.State == ObjectState.NEW))
                    {
                        CurrentLayer = _creoLayerService.CreateCadModelLayer(CurrentItem.CurrentCadModel, item.Name);
                        if (CurrentLayer != null)
                        {
                            CurrentListItems = new List<CREOCadModelItem>();
                            foreach (var cadItem in item.ListModelItems)
                            {
                                CurrentListItems.Add(new CREOCadModelItem() { Item = cadItem });
                            }
                            _creoLayerService.AddItemsToLayer(CurrentLayer, CurrentListItems?.Select(item => item.Item));

                            CurrentLayer.Status = (int)item.DisplayStatus;
                        }
                    }

                    // Update Layers with none affected Ref Features (PLANS, CSYS, POINTS, AXIS)
                    CadDocLayerItem CurrentCadDocLayerItem;
                    foreach (var item in CurrentItem.ListRefAxis.Concat(CurrentItem.ListRefPlans).Concat(CurrentItem.ListRefCSys).Concat(CurrentItem.ListRefPoints).Where(feature => !feature.IsInLayer))
                    {
                        CurrentCadDocLayerItem = CurrentItem.ListLayers.Where((layer) => item.Type == layer.RefType).FirstOrDefault();
                        if (CurrentCadDocLayerItem != null)
                        {
                            CurrentLayer = CurrentCadDocLayerItem.LayerItem;
                            _creoLayerService.AddItemsToLayer(CurrentLayer, new List<CREOCadModelItem>() { item }.Select(item => item.Item));
                        }
                    }

                    //foreach (var item in CurrentItem.ListLayers.Where((layer) => layer.State == ObjectState.CREATED && layer.DisplayStatus == EpfcDisplayStatus.EpfcLAYER_BLANK))
                    foreach (var item in CurrentItem.ListLayers.Where((layer) => layer.State == ObjectState.CREATED))
                    {
                        item.LayerItem.Status = (int)item.DisplayStatus;
                    }
                    //foreach (var item in CurrentItem.ListLayers.Where((layer) => layer.State == ObjectState.CREATED && layer.DisplayStatus == EpfcDisplayStatus.))
                    //{
                    //    item.LayerItem.Status = (int)item.DisplayStatus;
                    //}
                    _creoMacroService.SaveLayerStatus();

                    if (!IsDisplayedWindow)
                    {
                        IpfcWindow CurrentWindow = _creoModelService.GetCadDocWindow(CurrentItem.CurrentCadModel);
                        CurrentWindow.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void RemoveUpdateColorAsynch(bool isOnlyRemove = false)
        {
            try
            {
                if (StopCurrentProcess)
                {
                    StopCurrentProcess = false;
                    return;
                }

                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = true;
                CurrentMassUpdAttribDataContext.NbModelsInSession = CurrentMassUpdAttribDataContext.ShownCadModels.Where((item) => item.IsSelected).Count();
                CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress = 0;

                IpfcModel CurrentCadModel = null;
                bool Is3DModel = true;

                // update color list in CREO
                _creoMacroService.LoadAppearanceFile(AppearanceFileName);

                // close all CREO windows
                List<CREOCadModelItem> allCadOpened = _creoModelService.CloseAllWindows();

                foreach (var CadDoc in CurrentMassUpdAttribDataContext.ShownCadModels.Where((item) => item.IsSelected))
                {
                    CurrentMassUpdAttribDataContext.NbModelsInSessionInProgress++;
                    try
                    {
                        // Check that CAD document is still in session
                        TraceLog.AddTraceLog($"RemoveUpdateColorAsynch: Check Cad Doc {CadDoc.PartNumber}");
                        if (CadDoc.PartNumber.IndexOf(".drw") > 0)
                        {
                            Is3DModel = false;
                        }
                        else if (CadDoc.PartNumber.IndexOf(".asm") > 0)
                        {
                            Is3DModel = true;
                            CurrentCadModel = _creoModelService.RetrieveModel(CadDoc.PartNumber, EpfcModelType.EpfcMDL_ASSEMBLY);
                            //TemplateCadModel = CREOConnection.retrieveModel(CurrentCREOConnection.Session, McgMiscTools.GetAppSetting(this, "TemplateCadAsm"), EpfcModelType.EpfcMDL_ASSEMBLY);
                            TraceLog.AddTraceLog($"RemoveUpdateColorAsynch: EpfcModelType.EpfcMDL_ASSEMBLY");
                        }
                        else if (CadDoc.PartNumber.IndexOf(".prt") > 0)
                        {
                            Is3DModel = true;
                            CurrentCadModel = _creoModelService.RetrieveModel(CadDoc.PartNumber, EpfcModelType.EpfcMDL_PART);
                            //TemplateCadModel = CREOConnection.retrieveModel(CurrentCREOConnection.Session, McgMiscTools.GetAppSetting(this, "TemplateCadPrt"), EpfcModelType.EpfcMDL_PART);
                            TraceLog.AddTraceLog($"RemoveUpdateColorAsynch: EpfcModelType.EpfcMDL_PART");
                        }

                    }
                    catch (CREORetrieveModelException)
                    {
                        CadDoc.Status = McgWpfTools.GetStringResource("MUA_UpdateStatus03");
                        TraceLog.AddTraceLog($"RemoveUpdateColorAsynch: CurrentCadModel null");
                        CurrentCadModel = null;
                    }



                    if (CurrentCadModel != null && !_creoFeatureService.IsBulkItem(CurrentCadModel) && Is3DModel)
                    {
                        bool IsDisplayedWindow = false;
                        IsDisplayedWindow = _creoModelService.ActiveCadDocWindow(CurrentCadModel);
                        IpfcWindow currentWindow = _creoModelService.GetCadDocWindow(CurrentCadModel);
                        currentWindow.Activate();
                        // remove appearences
                        _creoMacroService.ClearAppearances();

                        // apply new color
                        if (!isOnlyRemove)
                            _creoMacroService.AssignedColorPrt(CurrentMassUpdAttribDataContext.SelectedCreoColor.ColorName);


                        // Save CAD Document
                        if (CurrentCadModel != null)
                            CurrentCadModel.Save();
                        if (CadDoc.FromExcelImport)
                            CadDoc.IsUpdated = false;

                        CadDoc.Status = McgWpfTools.GetStringResource("MUA_UpdateStatus02");

                        TraceLog.AddTraceLog($"RemoveUpdateColorAsynch: Saved CAD Model {CadDoc.PartNumber}");

                        // close current window if not displayed previously
                        if (!IsDisplayedWindow)
                        {
                            IpfcWindow CurrentWindow = _creoModelService.GetCadDocWindow(CurrentCadModel);
                            CurrentWindow.Close();
                        }

                        Thread.Sleep(100);
                    }
                }

                // reopen all models
                _creoModelService.OpenAllWindows(allCadOpened);

            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                CurrentMassUpdAttribDataContext.IsSearchCadModelInProgress = false;
                RaiseActionDoneEvent();
            }
        }
        #endregion

        #region [REGION] Methods Windchill Interaction
        private void CheckInAllCadDoc()
        {
            try
            {
                // Extract list of CAD document to update
                var ListCadSelected = CurrentMassUpdAttribDataContext.ShownCadModels.Where((item) => item.IsSelected).ToList();

                foreach (var elem in ListCadSelected)
                {
                    CurrentMassUpdAttribDataContext.SelectedItem = elem;
                    CheckInOneCadDoc(false);
                }
                CurrentMassUpdAttribDataContext.UpdateShownModelsList();
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void CheckInOneCadDoc(bool UpdateListShown = true)
        {
            try
            {
                if (CurrentMassUpdAttribDataContext.SelectedItem != null && !CurrentMassUpdAttribDataContext.SelectedItem.IsCheckedIn)
                {
                    UpdateOneCadDocument();
                    _creoSessionProvider.Session.GetActiveServer().CheckinObjects(CurrentMassUpdAttribDataContext.SelectedItem.CurrentCadModel, null);
                    UpdateCadDocInformation(CurrentMassUpdAttribDataContext.SelectedItem, true);
                    if (UpdateListShown)
                        CurrentMassUpdAttribDataContext.UpdateShownModelsList();
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void CheckOutAllCadDoc()
        {
            try
            {
                // Extract list of CAD document to update
                var ListCadSelected = from elem in CurrentMassUpdAttribDataContext.ShownCadModels
                                      where elem.IsSelected == true
                                      select elem;

                foreach (var elem in ListCadSelected)
                {
                    CurrentMassUpdAttribDataContext.SelectedItem = elem;
                    CheckOutOneCadDoc(false);
                }
                CurrentMassUpdAttribDataContext.UpdateShownModelsList();
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void CheckOutOneCadDoc(bool UpdateListShown = true)
        {
            try
            {
                if (CurrentMassUpdAttribDataContext.SelectedItem != null && !CurrentMassUpdAttribDataContext.SelectedItem.IsCheckedOut)
                {
                    _creoSessionProvider.Session.GetActiveServer().CheckoutObjects(CurrentMassUpdAttribDataContext.SelectedItem.CurrentCadModel, null, true, null);
                    UpdateCadDocInformation(CurrentMassUpdAttribDataContext.SelectedItem);
                    if (UpdateListShown)
                        CurrentMassUpdAttribDataContext.UpdateShownModelsList();
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void UpdateCadDocInformation(MassUpdateAttributeItem pCadDoc, bool UpdateAttribute = false)
        {
            try
            {
                if (pCadDoc.PartNumber.IndexOf(".drw") > 0)
                    pCadDoc.CurrentCadModel = _creoModelService.RetrieveModel(pCadDoc.PartNumber, EpfcModelType.EpfcMDL_DRAWING);
                else if (pCadDoc.PartNumber.IndexOf(".asm") > 0)
                    pCadDoc.CurrentCadModel = _creoModelService.RetrieveModel(pCadDoc.PartNumber, EpfcModelType.EpfcMDL_ASSEMBLY);
                else if (pCadDoc.PartNumber.IndexOf(".prt") > 0)
                    pCadDoc.CurrentCadModel = _creoModelService.RetrieveModel(pCadDoc.PartNumber, EpfcModelType.EpfcMDL_PART);

                if (pCadDoc.CurrentCadModel != null)
                {
                    // Search PTC_COMMON_NAME
                    pCadDoc.PTC_COMMON_NAME = pCadDoc.CurrentCadModel.CommonName;


                    // check if model check out/in, modifiable....
                    pCadDoc.IsCheckedOut = false;
                    pCadDoc.IsPtcCommonNameModifiable = false;
                    CREOModelStatus aCREOModelStatus = _creoModelService.GetModelStatus(pCadDoc.CurrentCadModel);
                    if (aCREOModelStatus == CREOModelStatus.CHECKEDOUT || aCREOModelStatus == CREOModelStatus.NEWINSESSION)
                    {
                        pCadDoc.IsReadOnly = false;
                        pCadDoc.IsLocallyModified = false;
                        pCadDoc.IsCheckedOut = true;
                        pCadDoc.IsModifiable = true;
                        if (aCREOModelStatus == CREOModelStatus.NEWINSESSION)
                        {
                            pCadDoc.IsPtcCommonNameModifiable = true;
                            pCadDoc.WebtermList = CurrentMassUpdAttribDataContext.WebtermList;
                        }
                    }
                    else if (aCREOModelStatus == CREOModelStatus.LOCALLYMODIFIED)
                    {
                        pCadDoc.IsCheckedOut = false;
                        pCadDoc.IsReadOnly = false;
                        pCadDoc.IsLocallyModified = true;
                        pCadDoc.IsModifiable = true;
                    }
                    else
                    {
                        pCadDoc.IsCheckedOut = false;
                        pCadDoc.IsLocallyModified = false;
                        pCadDoc.IsReadOnly = true;
                        pCadDoc.IsModifiable = false;
                    }

                    // search all attributes
                    if (UpdateAttribute)
                    {
                        pCadDoc.IsUpdated = false;
                        pCadDoc.PTC_COMMON_NAME = pCadDoc.CurrentCadModel.CommonName;

                        string CurrentAttribValue = "";

                        foreach (McgAttributeColumnHeaderInfo newAttrib in CurrentMassUpdAttriConfiguration.ListColumns)
                        {
                            if (newAttrib.AttributeType == AttributeTypeEnum.INT)
                                CurrentAttribValue = _creoParameterService.GetParameterAsDouble(pCadDoc.CurrentCadModel, newAttrib.AttributeID).ToString();
                            else if (newAttrib.AttributeType == AttributeTypeEnum.REAL)
                                CurrentAttribValue = _creoParameterService.GetParameterAsDouble(pCadDoc.CurrentCadModel, newAttrib.AttributeID).ToString();
                            else if (newAttrib.AttributeType == AttributeTypeEnum.TEXT)
                                CurrentAttribValue = _creoParameterService.GetParameterAsString(pCadDoc.CurrentCadModel, newAttrib.AttributeID);
                            else
                                CurrentAttribValue = "";

                            // Update generic properties PARAMSTRxx with accurate value
                            if (pCadDoc.GetType().GetProperty(newAttrib.ClassAttributeID) != null)
                                pCadDoc.GetType().GetProperty(newAttrib.ClassAttributeID).SetValue(pCadDoc, CurrentAttribValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
