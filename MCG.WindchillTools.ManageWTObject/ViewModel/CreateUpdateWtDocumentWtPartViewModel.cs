using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WebtermLib.Services.Interfaces;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.WindchillCredential;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.Services.Interfaces;
using MCG.WindchillRequestTool.ViewModel;
using MCG.WindchillTools.ManageWTObject.Configuration;
using MCG.WindchillTools.ManageWTObject.Exceptions;
using MCG.WindchillTools.ManageWTObject.Interfaces;
using MCG.WindchillTools.ManageWTObject.View;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.WindchillTools.ManageWTObject.ViewModel
{
    public class CreateUpdateWtDocumentWtPartViewModel : ObservableObject, ICreateUpdateWtDocumentWtPartViewModel
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
        public CreateUpdateWtDocumentWtPartDataContext CurrentDataContext { get; set; }
        private bool SearchOk { get; set; } = false;

        private Dispatcher MainDispatcher { get; set; } = null;

        private readonly IWebtermTools _webtermTools;
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly IWindchillDocumentManagementService _windchillDocumentManagementService;
        private readonly IWindchillPartManagementService _windchillPartManagementService;
        private readonly IWindchillDataAdminManagementService _windchillDataAdminManagementService;
        private readonly IMcgWindchillToolsManageWTObjectWindowService _mcgWindchillToolsManageWTObjectWindowService;
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly IWindchillRequestMiscService _windchillRequestMiscService;
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private WindchillCredentialItem WindchillCredential { get; set; } = null;
        public List<Webterm> ListWebterm { get; set; }
        private MassWtDocumentUpdateConfiguration ApplicationConfiguration { get; set; }

        private MgtContentItem CurrentPrimaryContent = null;
        private string WebtermDb { get; set; } = McgWpfTools.GetPropertiesFromMainApp<string>("WEBTERMDB");
        private List<BrandGroupSubGroupItem> ListBrandGroupSubGroup { get; set; }
        #endregion

        #region [REGION] Events
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

        #region [REGION] Commands
        public ICommand CommandClosing { get => new RelayCommand(() => RaiseClosingEvent()); }
        public ICommand CommandDrop { get => new RelayCommand<DragEventArgs>((obj) => ExecuteDrop(obj)); }
        public ICommand CommandRemoveContent { get => new RelayCommand<object>((obj) => ExecuteRemoveContent(obj)); }
        public ICommand CommandDownloadContent { get => new RelayCommand<object>((obj) => ExecuteDownloadContent(obj)); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        public ICommand CommandCopyWtDocToPartNumber { get => new RelayCommand(() => ExecuteCopyWtDocToPartNumber()); }
        public ICommand CommandCopyWtDocToPartContext { get => new RelayCommand(() => ExecuteCopyWtDocToPartContext()); }
        public ICommand CommandCopyWtDocToPartParam { get => new RelayCommand(() => ExecuteCopyWtDocToPartParam()); }
        public ICommand CommandCopyPartToWtDocNumber { get => new RelayCommand(() => ExecuteCopyPartToWtDocNumber()); }
        public ICommand CommandCopyPartToWtDocContext { get => new RelayCommand(() => ExecuteCopyPartToWtDocContext()); }
        public ICommand CommandCopyPartToWtDocParam { get => new RelayCommand(() => ExecuteCopyPartToWtDocParam()); }
        public ICommand CommandSearchWtObject { get => new RelayCommand(() => ExecuteSearchWtObject()); }
        public ICommand CommandChangeContext { get => new RelayCommand<string>((obj) => ExecuteChangeContext(obj)); }
        public ICommand CommandSearchOk { get => new RelayCommand(() => SearchOk = true); }
        public ICommand CommandSearchCancel { get => new RelayCommand(() => SearchOk = false); }
        public ICommand CommandCheckWtDocument { get => new RelayCommand(() => ExecuteCheckWtDocument()); }
        public ICommand CommandCheckWtPart { get => new RelayCommand(() => ExecuteCheckWtPart()); }
        public ICommand CommandCreateUpdateWtDocument { get => new RelayCommand(() => ExecuteCreateUpdateWtDocument()); }
        public ICommand CommandCreateUpdateWtPart { get => new RelayCommand(() => ExecuteCreateUpdateWtPart()); }
        public ICommand CommandResetWtDocument { get => new RelayCommand<KeyEventArgs>((obj) => ExecuteResetWtDocument(obj)); }
        public ICommand CommandResetWtPart { get => new RelayCommand<KeyEventArgs>((obj) => ExecuteResetWtPart(obj)); }
        public ICommand CommandCreateCreateLink { get => new RelayCommand<KeyEventArgs>((obj) => ExecuteCreateCreateLink(obj)); }
        #endregion

        #region [REGION] Init
        public CreateUpdateWtDocumentWtPartViewModel(IWebtermTools webtermTools,
                                                     IXmlSerializeTools xmlSerializeTools,
                                                     IWindchillDocumentManagementService windchillDocumentManagementService,
                                                     IWindchillPartManagementService windchillPartManagementService,
                                                     IWindchillDataAdminManagementService windchillDataAdminManagementService,
                                                     IMcgCommonLibWindowService mcgCommonLibWindowService,
                                                     IWindchillCredentialService windchillCredentialService,
                                                     IWindchillRequestMiscService windchillRequestMiscService)
        {
            try
            {
                MainDispatcher = Dispatcher.CurrentDispatcher;
                _webtermTools = webtermTools;
                _xmlSerializeTools = xmlSerializeTools;
                _windchillDocumentManagementService = windchillDocumentManagementService;
                _windchillPartManagementService = windchillPartManagementService;
                _windchillDataAdminManagementService = windchillDataAdminManagementService;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;
                _windchillCredentialService = windchillCredentialService;
                _windchillRequestMiscService = windchillRequestMiscService;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;


                CurrentDataContext = new CreateUpdateWtDocumentWtPartDataContext();

                // update Webterm list
                ListWebterm = _webtermTools.GetWebtermList(null, null, WebtermDb)?.OrderBy((item) => item.English).ToList();
                foreach (var term in ListWebterm)
                    CurrentDataContext.ListWebterm.Add(term.English);

                // Update language list and local webterm list
                CurrentDataContext.ChangeLanguageEvent += ChangeLanguageEventAction;
                ApplicationConfiguration = _xmlSerializeTools.GetDeserializedXml<MassWtDocumentUpdateConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{ManageWTObjectConstants.MassWtDocumentUpdateConfigurationFile}");
                foreach (MCGLanguage Lang in ApplicationConfiguration.LocalLanguageList)
                    CurrentDataContext.ListLanguage.Add(Lang);
                foreach (string grp in ApplicationConfiguration.ListGroup)
                    CurrentDataContext.ListGroup.Add(grp);
                foreach (string brand in ApplicationConfiguration.ListBrand)
                    CurrentDataContext.ListBrand.Add(brand);


                CurrentDataContext.SelectedLanguage = McgWpfTools.GetPropertiesFromMainApp<MCGLanguage>("MCGLANGUAGE");
                if (CurrentDataContext.SelectedLanguage == null || CurrentDataContext.SelectedLanguage.Language == null)
                    CurrentDataContext.SelectedLanguage = (from elem in CurrentDataContext.ListLanguage where Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpper() == elem.SAPCode select elem).FirstOrDefault();
                if (CurrentDataContext.SelectedLanguage == null || CurrentDataContext.SelectedLanguage.Language == null)
                    CurrentDataContext.SelectedLanguage = CurrentDataContext.ListLanguage.FirstOrDefault();

                ActionInProgressEvent += (sender, e) => CurrentDataContext.ActionInProgress = true;
                ActionDoneEvent += (sender, e) => CurrentDataContext.ActionInProgress = false;

                //if (CryptedLoginRO == null) CryptedLoginRO = McgBusinessTools.GetAppSetting(this, "CryptedLoginRO");
                //if (CryptedPassWordRO == null) CryptedPassWordRO = McgBusinessTools.GetAppSetting(this, "CryptedPassWordRO");
                SearchAllContext();

                // Search units list
                string strUnit = ManageWTObjectConstants.Units;
                if (strUnit != null)
                {
                    foreach (var unit in strUnit.Split('|'))
                        CurrentDataContext.AllUnits.Add(unit);
                }

                //init CurrentObject
                CurrentDataContext.CurrentWtObject = new MgtWtDocumentItem()
                {
                    Revision = McgRevisionSchemaEnum.A,
                    WindchillDocumentType = CurrentDataContext.ListWindchillDocumentType.FirstOrDefault(),
                    WindchillPartType = CurrentDataContext.ListWindchillPartType.FirstOrDefault(),
                    WtDocumentObject = new MgtWtObject(),
                    WtPartObject = new MgtWtObject()
                    {
                        Unit = CurrentDataContext.AllUnits.FirstOrDefault(),
                        BRAND = CurrentDataContext.ListBrand.FirstOrDefault()
                    },
                };

                CurrentDataContext.CurrentWtObject.WtDocumentObject.Description21ChangeEvent += Description21ChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtPartObject.Description21ChangeEvent += Description21ChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtPartObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.CommonParamChangeEvent += ParamChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.VersionParamChangeEvent += ParamChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtPartObject.CommonParamChangeEvent += ParamChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtPartObject.VersionParamChangeEvent += ParamChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.NumberChangeEvent += WtDocumentNumberChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtPartObject.NumberChangeEvent += WtPartNumberChangeEventAction;

                ListBrandGroupSubGroup = McgBusinessTools.GetLIstBrandGroupSubGroup();
                CurrentDataContext.ListBrand.Clear();
                var brands = ListBrandGroupSubGroup.Select(i => i.Brand).Distinct();
                foreach (var brand in brands)
                {
                    CurrentDataContext.ListBrand.Add(brand);
                }

                CurrentDataContext.UpdateBrandEvent += UpdateGroups;
                CurrentDataContext.UpdateGroupEvent += UpdateSubGroups;
                CurrentDataContext.UpdateSubGroupEvent += UpdateOptions;
                CurrentDataContext.SelectedBrand = CurrentDataContext.ListBrand.FirstOrDefault();

                UpdateMaterialListFromFolder();
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }

        }

        private void UpdateGroups(object sender, EventArgs e)
        {
            try
            {
                CurrentDataContext.ListGroup.Clear();
                var groups = ListBrandGroupSubGroup.Where(i => i.Brand == CurrentDataContext.SelectedBrand).Select(i => i.Group).Distinct();
                foreach (var group in groups)
                {
                    CurrentDataContext.ListGroup.Add(group);
                }
                CurrentDataContext.SelectedGroup = CurrentDataContext.ListGroup.FirstOrDefault();
                CurrentDataContext.CurrentWtObject.WtPartObject.BRAND = CurrentDataContext.SelectedBrand;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void UpdateSubGroups(object sender, EventArgs e)
        {
            try
            {
                CurrentDataContext.ListSubGroup.Clear();

                var subGroups = ListBrandGroupSubGroup.Where(i => i.Brand == CurrentDataContext.SelectedBrand && i.Group == CurrentDataContext.SelectedGroup).Select(i => i.SubGroup).Distinct();
                foreach (var subGroup in subGroups)
                {
                    CurrentDataContext.ListSubGroup.Add(subGroup);
                }
                CurrentDataContext.SelectedSubGroup = CurrentDataContext.ListSubGroup.FirstOrDefault();
                CurrentDataContext.CurrentWtObject.WtPartObject.GROUP = CurrentDataContext.SelectedGroup;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void UpdateOptions(object sender, EventArgs e)
        {
            try
            {
                CurrentDataContext.ListOption.Clear();

                var subOptions = ListBrandGroupSubGroup.FirstOrDefault(i => i.Brand == CurrentDataContext.SelectedBrand && i.Group == CurrentDataContext.SelectedGroup && i.SubGroup == CurrentDataContext.SelectedSubGroup)?.OptionList;
                if (subOptions != null)
                    foreach (var subOption in subOptions)
                    {
                        CurrentDataContext.ListOption.Add(subOption);
                    }
                CurrentDataContext.CurrentWtObject.WtPartObject.OPTION = CurrentDataContext.ListOption.FirstOrDefault();
                CurrentDataContext.CurrentWtObject.WtPartObject.SUB_GROUP = CurrentDataContext.SelectedSubGroup;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteDrop(DragEventArgs obj)
        {
            try
            {
                //if (obj != null && CurrentDataContext.WtDocumentSelected)
                if (obj != null && obj.Data != null && obj.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])obj.Data.GetData(DataFormats.FileDrop);

                    if (CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.CREATED)
                        CurrentDataContext.CurrentWtObject.WtDocumentObject.Status = ObjectState.UPDATED;

                    MgtContentItem CurrentContentMgtContentItem = null;
                    foreach (var file in files)
                    {
                        if (CurrentDataContext.ListContentItem.FirstOrDefault((item) => item.CompleteFilename != null && item.CompleteFilename == file) == null)
                        {
                            CurrentContentMgtContentItem = new MgtContentItem() { CompleteFilename = file, State = ObjectState.NEW, IsPrimaryContent = false };
                            CurrentDataContext.ListContentItem.Add(CurrentContentMgtContentItem);
                            CurrentContentMgtContentItem.IsPrimaryContentEvent += PrimaryContentEventAction;
                            CurrentContentMgtContentItem.PreviousIsPrimaryContentEvent += PreviousPrimaryContentEventAction;
                        }
                    }

                    if (CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.NEW
                        && CurrentDataContext.ListContentItem.Count > 0
                        && CurrentDataContext.ListContentItem.FirstOrDefault((item) => item.IsPrimaryContent) == null)
                        CurrentDataContext.ListContentItem.FirstOrDefault().IsPrimaryContent = true;

                    if (!CurrentDataContext.WtDocumentSelected)
                    {
                        CurrentDataContext.WtDocumentSelected = true;
                        string firstFile = files.FirstOrDefault();
                        string tempNumber = firstFile.Split('\\').LastOrDefault()?.ToUpper();
                        if (firstFile != null)
                        {
                            if (CurrentDataContext.CurrentWtObject.WtDocumentObject.NUMBER == null || CurrentDataContext.CurrentWtObject.WtDocumentObject.NUMBER == "")
                                CurrentDataContext.CurrentWtObject.WtDocumentObject.NUMBER = tempNumber;
                            if (CurrentDataContext.CurrentWtObject.WtPartObject.NUMBER == null || CurrentDataContext.CurrentWtObject.WtPartObject.NUMBER == "")
                                CurrentDataContext.CurrentWtObject.WtPartObject.NUMBER = tempNumber;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemoveContent(object obj)
        {
            try
            {
                if (obj != null && obj.GetType() == typeof(MgtContentItem))
                {
                    MgtContentItem item = (MgtContentItem)obj;

                    if (item.State != ObjectState.REMOVED && MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgRemoveItem"), McgWpfTools.GetStringResource("MWT_MsgTitleRemoveItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        if (item.State == ObjectState.CREATED)
                            item.State = ObjectState.REMOVED;
                        else
                        {
                            CurrentDataContext.ListContentItem.Remove(item);
                            //item.ParentWtDocument.ListContentItem.Remove(item);
                            if (item.IsPrimaryContent)
                            {
                                MgtContentItem newItem = CurrentDataContext.ListContentItem.FirstOrDefault((content) => content.State != ObjectState.REMOVED && content.State != ObjectState.CREATED);
                                if (newItem != null)
                                    newItem.IsPrimaryContent = true;
                            }
                        }
                        if (CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.CREATED)
                            CurrentDataContext.CurrentWtObject.WtDocumentObject.Status = ObjectState.UPDATED;
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDownloadContent(object obj)
        {
            try
            {
                if (obj != null && obj.GetType() == typeof(MgtContentItem))
                {
                    MgtContentItem item = (MgtContentItem)obj;

                    string DownloadFolder = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads";
                    Boolean ToBeDownload = true;
                    if (Directory.Exists(DownloadFolder))
                    {
                        if (File.Exists($"{DownloadFolder}\\{item.Filename}"))
                            if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgDownloadItem"), McgWpfTools.GetStringResource("MWT_MsgTitleDownloadItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                File.Delete($"{DownloadFolder}\\{item.Filename}");
                                ToBeDownload = true;
                            }
                            else
                                ToBeDownload = false;
                        if (ToBeDownload)
                        {
                            _windchillRequestMiscService.WindchillObjectViewableItemDownloadDownload(item.Filecontent, WindchillCredential.WindchillCredential, DownloadFolder, true, false, false);
                            //item.Filecontent.Download(WindchillCredential.WindchillCredential, DownloadFolder, true, false, false);
                            item.IsCanbeDownloaded = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenHelp()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("MWT_UserGuideCreateUpdateWtObj"));
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCopyWtDocToPartNumber()
        {
            try
            {
                CurrentDataContext.CurrentWtObject.WtPartObject.NUMBER = CurrentDataContext.CurrentWtObject.WtDocumentObject.NUMBER;
                CurrentDataContext.CurrentWtObject.WtPartObject.REVISION = CurrentDataContext.CurrentWtObject.WtDocumentObject.REVISION;
                //ExecuteResetWtPart(null);
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCopyWtDocToPartContext()
        {
            try
            {
                CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext = CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext;

            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCopyWtDocToPartParam()
        {
            try
            {
                if (CurrentDataContext.CurrentWtObject.WtPartObject.IsWtNonVersionAttributeEditable)
                    CurrentDataContext.CurrentWtObject.WtPartObject.PTCCOMMONNAME = CurrentDataContext.CurrentWtObject.WtDocumentObject.PTCCOMMONNAME;

                CurrentDataContext.CurrentWtObject.WtPartObject.DESCRIPTION2 = CurrentDataContext.CurrentWtObject.WtDocumentObject.DESCRIPTION2;
                CurrentDataContext.CurrentWtObject.WtPartObject.DESCRIPTION21 = CurrentDataContext.CurrentWtObject.WtDocumentObject.DESCRIPTION21;
                CurrentDataContext.CurrentWtObject.WtPartObject.DESCRIPTION22 = CurrentDataContext.CurrentWtObject.WtDocumentObject.DESCRIPTION22;
                CurrentDataContext.CurrentWtObject.WtPartObject.GROUPCREATOR = CurrentDataContext.CurrentWtObject.WtDocumentObject.GROUPCREATOR;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCopyPartToWtDocNumber()
        {
            try
            {
                CurrentDataContext.CurrentWtObject.WtDocumentObject.NUMBER = CurrentDataContext.CurrentWtObject.WtPartObject.NUMBER;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.REVISION = CurrentDataContext.CurrentWtObject.WtPartObject.REVISION;
                //ExecuteResetWtDocument(null);
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCopyPartToWtDocContext()
        {
            try
            {
                CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext = CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCopyPartToWtDocParam()
        {
            try
            {
                if (CurrentDataContext.CurrentWtObject.WtDocumentObject.IsWtNonVersionAttributeEditable)
                    CurrentDataContext.CurrentWtObject.WtDocumentObject.PTCCOMMONNAME = CurrentDataContext.CurrentWtObject.WtPartObject.PTCCOMMONNAME;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.DESCRIPTION2 = CurrentDataContext.CurrentWtObject.WtPartObject.DESCRIPTION2;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.DESCRIPTION21 = CurrentDataContext.CurrentWtObject.WtPartObject.DESCRIPTION21;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.DESCRIPTION22 = CurrentDataContext.CurrentWtObject.WtPartObject.DESCRIPTION22;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.GROUPCREATOR = CurrentDataContext.CurrentWtObject.WtPartObject.GROUPCREATOR;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSearchWtObject()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(CurrentDataContext.FilterNumber))
                {
                    List<RestOdataWtDocument> AllDocument = SearchWAllWtDocument().OrderBy(item => item.Number).ToList();
                    List<RestOdataWtPart> AllWtPart = SearchWAllWtPart().OrderBy(item => item.Number).ToList();

                    CurrentDataContext.ListSearchWtDocument.Clear();
                    CurrentDataContext.ListSearchWtPart.Clear();
                    if (AllDocument != null)
                    {
                        foreach (var obj in AllDocument)
                            CurrentDataContext.ListSearchWtDocument.Add(obj);
                    }
                    if (AllWtPart != null)
                    {
                        foreach (var obj in AllWtPart)
                            CurrentDataContext.ListSearchWtPart.Add(obj);
                    }
                    SearchOk = false;

                    _mcgWindchillToolsManageWTObjectWindowService.ShowDialogSearchWtDocumentPartView(this);

                    //SearchWtDocumentPartView CurrentSearchWtDocumentPartView = new SearchWtDocumentPartView();
                    //CurrentSearchWtDocumentPartView.DataContext = this;
                    //CurrentSearchWtDocumentPartView.ShowDialog();

                    if (SearchOk)
                    {
                        RestOdataWtDocument SearchedWtDocument = CurrentDataContext.ListSearchWtDocument.FirstOrDefault((item) => item.IsSelected);
                        RestOdataWtPart SearchedWtPart = CurrentDataContext.ListSearchWtPart.FirstOrDefault((item) => item.IsSelected);
                        if (SearchedWtDocument != null)
                        {
                            bool doCheck = true;
                            if (CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.NEW || CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.UPDATED)
                            {
                                if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgMsgSearchWtDocumentUpdated"), McgWpfTools.GetStringResource("MWT_MsgTitleSearchWtObject"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                                    doCheck = false;
                            }
                            if (doCheck)
                            {
                                CurrentDataContext.CurrentWtObject.WtDocumentObject.CommonParamChangeEvent -= ParamChangeEventAction;
                                CurrentDataContext.CurrentWtObject.WtDocumentObject.VersionParamChangeEvent -= ParamChangeEventAction;
                                UpdateInstanceOfWtDocument(SearchedWtDocument);
                                CurrentDataContext.CurrentWtObject.WtDocumentObject.CommonParamChangeEvent += ParamChangeEventAction;
                                CurrentDataContext.CurrentWtObject.WtDocumentObject.VersionParamChangeEvent += ParamChangeEventAction;
                            }
                        }

                        if (SearchedWtPart != null)
                        {
                            bool doCheck = true;
                            if (CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.NEW || CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.UPDATED)
                            {
                                if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgMsgSearchWtPartUpdated"), McgWpfTools.GetStringResource("MWT_MsgTitleSearchWtObject"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                                    doCheck = false;
                            }
                            if (doCheck)
                            {
                                CurrentDataContext.CurrentWtObject.WtPartObject.CommonParamChangeEvent -= ParamChangeEventAction;
                                CurrentDataContext.CurrentWtObject.WtPartObject.VersionParamChangeEvent -= ParamChangeEventAction;
                                UpdateInstanceOfWtPart(SearchedWtPart);
                                CurrentDataContext.CurrentWtObject.WtPartObject.CommonParamChangeEvent += ParamChangeEventAction;
                                CurrentDataContext.CurrentWtObject.WtPartObject.VersionParamChangeEvent += ParamChangeEventAction;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteChangeContext(string WTObjectType)
        {
            try
            {
                MgtWtObject CurrentObject = null;
                if (WTObjectType != null && WTObjectType == "WTPART")
                    CurrentObject = CurrentDataContext.CurrentWtObject.WtPartObject;
                else if (WTObjectType != null && WTObjectType == "WTDOCUMENT")
                    CurrentObject = CurrentDataContext.CurrentWtObject.WtDocumentObject;

                if (CurrentObject != null)
                {
                    var returnWindow = _mcgCommonLibWindowService.ShowDialogMcgWindchillContextSelection(CurrentDataContext.WindchillContextList, CurrentDataContext.WindchillContextList.FirstOrDefault((item) => item.Name == CurrentObject.SelectedWindchillContext?.Name));
                    //McgWindchillContextSelection ContextWindow = new McgWindchillContextSelection(CurrentDataContext.WindchillContextList, CurrentDataContext.WindchillContextList.FirstOrDefault((item) => item.Name == CurrentObject.SelectedWindchillContext?.Name));
                    //ContextWindow.ShowDialog();
                    if (returnWindow.DialogValue == MessageBoxResult.OK)
                    {
                        WindchillContext SelectedContext = returnWindow.SelectedContext.Clone();
                        CurrentObject.SelectedWindchillContext = SelectedContext;
                        CurrentObject.SelectedWindchillContext.Folder = SelectedContext.OdataFolder.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckWtDocument()
        {
            try
            {

                if (CurrentDataContext.CurrentWtObject.WtDocumentObject.NUMBER != null && CurrentDataContext.CurrentWtObject.WtDocumentObject.NUMBER.Trim() != "" && !CurrentDataContext.CurrentWtObject.WtDocumentObject.NUMBER.Contains("*"))
                {
                    bool doCheck = true;
                    if (CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.NEW || CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.UPDATED)
                    {
                        if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgMsgSearchWtDocumentUpdated"), McgWpfTools.GetStringResource("MWT_MsgTitleSearchWtObject"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                            doCheck = false;
                    }

                    if (doCheck)
                    {
                        CurrentDataContext.CurrentWtObject.WtDocumentObject.CommonParamChangeEvent -= ParamChangeEventAction;
                        CurrentDataContext.CurrentWtObject.WtDocumentObject.VersionParamChangeEvent -= ParamChangeEventAction;
                        CurrentDataContext.WtDocumentSelected = true;
                        CheckWindchillCredential();
                        RaiseActionInProgressEvent();
                        CurrentDataContext.StatusBarText = McgWpfTools.GetStringResource("MWT_StatusBarSearchDocInProgress");
                        SearchWtdocument();
                        CurrentDataContext.StatusBarText = McgWpfTools.GetStringResource("MWT_StatusBarSearchDocDone");
                        CurrentDataContext.CurrentWtObject.WtDocumentObject.CommonParamChangeEvent += ParamChangeEventAction;
                        CurrentDataContext.CurrentWtObject.WtDocumentObject.VersionParamChangeEvent += ParamChangeEventAction;
                    }
                    //Thread CurrentThread = new Thread(() => SearchWtdocumentAsynch());
                    //CurrentThread.IsBackground = true;
                    //CurrentThread.Start();
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgMsgSearchWtPartUpdated"), McgWpfTools.GetStringResource("MWT_MsgTitleSearchWtObject"), MessageBoxButton.OK, MessageBoxImage.Warning);

            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckWtPart()
        {
            try
            {

                if (CurrentDataContext.CurrentWtObject.WtPartObject.NUMBER != null && CurrentDataContext.CurrentWtObject.WtPartObject.NUMBER.Trim() != "" && !CurrentDataContext.CurrentWtObject.WtPartObject.NUMBER.Contains("*"))
                {
                    bool doCheck = true;
                    if (CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.NEW || CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.UPDATED)
                    {
                        if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNoContext"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                            doCheck = false;
                    }
                    if (doCheck)
                    {
                        CurrentDataContext.CurrentWtObject.WtPartObject.CommonParamChangeEvent -= ParamChangeEventAction;
                        CurrentDataContext.CurrentWtObject.WtPartObject.VersionParamChangeEvent -= ParamChangeEventAction;
                        CurrentDataContext.WtPartSelected = true;
                        CheckWindchillCredential();
                        RaiseActionInProgressEvent();
                        CurrentDataContext.StatusBarText = McgWpfTools.GetStringResource("MWT_StatusBarSearchPartInProgress");
                        SearchPart();
                        CurrentDataContext.StatusBarText = McgWpfTools.GetStringResource("MWT_StatusBarSearchPartDone");
                        CurrentDataContext.CurrentWtObject.WtPartObject.CommonParamChangeEvent += ParamChangeEventAction;
                        CurrentDataContext.CurrentWtObject.WtPartObject.VersionParamChangeEvent += ParamChangeEventAction;
                    }
                    //Thread CurrentThread = new Thread(() => SearchPartAsynch());
                    //CurrentThread.IsBackground = true;
                    //CurrentThread.Start();
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNoContext"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.OK, MessageBoxImage.Warning);

            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreateUpdateWtDocument()
        {
            try
            {
                CurrentDataContext.CurrentWtObject.ListContentItem = CurrentDataContext.ListContentItem;
                if (CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.UNKNOWN)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNoCheckWtDocument"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.UPDATED)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgStartCreationWtDoc"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        bool CreateDocument = true;
                        if (CurrentDataContext.ListContentItem.FirstOrDefault((item) => item.IsPrimaryContent) == null)
                        {
                            if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNoPrimaryContent"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                                CreateDocument = false;
                        }
                        if (CreateDocument)
                        {
                            CheckWindchillCredential();
                            RaiseActionInProgressEvent();
                            CurrentDataContext.StatusBarText = McgWpfTools.GetStringResource("MWT_StatusBarUpdateInProgress");
                            Thread CurrentThread = new Thread(() => CreateUpdateWtDocumentAsynch(false));
                            CurrentThread.IsBackground = true;
                            CurrentThread.Start();
                        }
                    }
                }

                else if (CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.NEW)
                {
                    // Check if Context provided
                    if (CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext == null
                        || CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext.Name == null
                        || CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext.Name.Trim() == ""
                        || CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext.Folder == null
                        || CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext.Folder.Trim() == "")
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNoContext"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // Check if PTC COMMON NAME provided
                    else if (CurrentDataContext.CurrentWtObject.WtDocumentObject.PTCCOMMONNAME == null
                        || CurrentDataContext.CurrentWtObject.WtDocumentObject.PTCCOMMONNAME.Trim() == "")
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNoPtcCommonName"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // Create Document
                    else
                    {
                        if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgStartCreationWtDoc"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            bool CreateDocument = true;
                            if (CurrentDataContext.ListContentItem.FirstOrDefault((item) => item.IsPrimaryContent) == null)
                            {
                                if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNoPrimaryContent"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                                    CreateDocument = false;
                            }
                            if (CreateDocument)
                            {
                                CheckWindchillCredential();
                                RaiseActionInProgressEvent();
                                CurrentDataContext.StatusBarText = McgWpfTools.GetStringResource("MWT_StatusBarUpdateInProgress");
                                Thread CurrentThread = new Thread(() => CreateUpdateWtDocumentAsynch(true));
                                CurrentThread.IsBackground = true;
                                CurrentThread.Start();
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreateUpdateWtPart()
        {
            try
            {

                if (CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.UNKNOWN)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNoCheckWtPart"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                else if (CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.UPDATED
                         && MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgStartCreationWtPart"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // Check if QUAL INSPECTION GROUP provided
                    if (CurrentDataContext.CurrentWtObject.WtPartObject.QUALINSPGRP == null
                            || CurrentDataContext.CurrentWtObject.WtPartObject.QUALINSPGRP.Trim() == "")
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNoQualInspGrp"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // Update WtPart

                    CheckWindchillCredential();
                    RaiseActionInProgressEvent();
                    CurrentDataContext.StatusBarText = McgWpfTools.GetStringResource("MWT_StatusBarUpdateInProgress");
                    Thread CurrentThread = new Thread(() => CreateUpdateWtPartAsynch(false));
                    CurrentThread.IsBackground = true;
                    CurrentThread.Start();
                }
                else if (CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.NEW)
                {
                    // Check if Context provided
                    if (CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext == null
                        || CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext.Name == null
                        || CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext.Name.Trim() == ""
                        || CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext.Folder == null
                        || CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext.Folder.Trim() == "")
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNoContext"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // Check if PTC COMMON NAME provided
                    else if (CurrentDataContext.CurrentWtObject.WtPartObject.PTCCOMMONNAME == null
                        || CurrentDataContext.CurrentWtObject.WtPartObject.PTCCOMMONNAME.Trim() == "")
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNoPtcCommonName"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }


                    // Check if QUAL INSPECTION GROUP provided
                    else if (CurrentDataContext.CurrentWtObject.WtPartObject.QUALINSPGRP == null
                            || CurrentDataContext.CurrentWtObject.WtPartObject.QUALINSPGRP.Trim() == "")
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNoQualInspGrp"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // Create WtPart
                    else if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgStartCreationWtPart"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        CheckWindchillCredential();
                        RaiseActionInProgressEvent();
                        CurrentDataContext.StatusBarText = McgWpfTools.GetStringResource("MWT_StatusBarUpdateInProgress");
                        Thread CurrentThread = new Thread(() => CreateUpdateWtPartAsynch(true));
                        CurrentThread.IsBackground = true;
                        CurrentThread.Start();
                    }
                }

            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteResetWtDocument(KeyEventArgs e)
        {
            try
            {
                if (e != null
                    && e.Key != Key.Enter
                    && e.Key != Key.LeftCtrl
                    && e.Key != Key.RightCtrl
                    && e.Key != Key.LeftShift
                    && e.Key != Key.RightShift)
                {
                    if (!((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && (e.Key == Key.C || e.Key == Key.A)))
                    {
                        ClearWtDocument();
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteResetWtPart(KeyEventArgs e)
        {
            try
            {
                if (e == null || e.Key != Key.Enter)
                {
                    ClearWtPart();
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreateCreateLink(KeyEventArgs e)
        {
            try
            {
                if (CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.CREATED
                    && CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.CREATED
                    && CurrentDataContext.CurrentWtObject.LinkStatus == ObjectState.UNLINKED)
                {
                    CheckWindchillCredential();
                    RaiseActionInProgressEvent();
                    Thread CurrentThread = new Thread(() => LinkWtPartWtDocumentAsynch());
                    CurrentThread.IsBackground = true;
                    CurrentThread.Start();
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Event Methods
        private void ChangeLanguageEventAction(object sender, EventArgs e)
        {
            try
            {
                if (CurrentDataContext.SelectedLanguage != null && CurrentDataContext.SelectedLanguage.DataTableColonne != null)
                {
                    CurrentDataContext.ListWebtermLocal.Clear();
                    if (CurrentDataContext.SelectedLanguage?.DataTableColonne.ToUpper() == "ENGLISH")
                        CurrentDataContext.ListWebtermLocal.Add("-");
                    else
                    {
                        PropertyInfo LangProp = typeof(Webterm).GetProperty(CurrentDataContext.SelectedLanguage?.DataTableColonne);
                        if (LangProp != null)
                        {
                            List<string> TempLocalList = new List<string>();
                            foreach (var term in ListWebterm)
                                TempLocalList.Add(LangProp.GetValue(term).ToString());
                            foreach (var term in TempLocalList.Where((item) => item != null && item.Trim() != "").OrderBy((item) => item))
                                CurrentDataContext.ListWebtermLocal.Add(term);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void Description21ChangeEventAction(object sender, EventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(MgtWtObject) && sender != null)
                {
                    MgtWtObject CurrentObject = (MgtWtObject)sender;
                    if (CurrentObject.IsWtNonVersionAttributeEditable)
                    {
                        CurrentObject.Description21ChangeEvent -= Description21ChangeEventAction;
                        CurrentObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;
                        PropertyInfo LangProp = typeof(Webterm).GetProperty(CurrentDataContext.SelectedLanguage?.DataTableColonne);
                        if (LangProp != null)
                        {
                            Webterm CurrentWebterm = ListWebterm.FirstOrDefault((item) => LangProp.GetValue(item).ToString() == CurrentObject.DESCRIPTION21);
                            CurrentObject.PTCCOMMONNAME = CurrentWebterm?.English;
                        }

                        CurrentObject.Description21ChangeEvent += Description21ChangeEventAction;
                        CurrentObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PtcCommonNameChangeEventAction(object sender, EventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(MgtWtObject) && sender != null)
                {
                    MgtWtObject CurrentObject = (MgtWtObject)sender;

                    CurrentObject.Description21ChangeEvent -= Description21ChangeEventAction;
                    CurrentObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;

                    if (CurrentDataContext.SelectedLanguage?.DataTableColonne.ToUpper() == "ENGLISH")
                    {
                        CurrentObject.DESCRIPTION21 = CurrentDataContext.ListWebtermLocal.FirstOrDefault();
                    }
                    else
                    {
                        PropertyInfo LangProp = typeof(Webterm).GetProperty(CurrentDataContext.SelectedLanguage?.DataTableColonne);
                        if (LangProp != null)
                        {
                            Webterm CurrentWebterm = ListWebterm.FirstOrDefault((item) => item.English == CurrentObject.PTCCOMMONNAME);
                            if (CurrentWebterm != null)
                                CurrentObject.DESCRIPTION21 = CurrentDataContext.ListWebtermLocal.FirstOrDefault((item) => item == LangProp.GetValue(CurrentWebterm).ToString());
                            else
                                CurrentObject.DESCRIPTION21 = "-";
                        }
                    }

                    CurrentObject.Description21ChangeEvent += Description21ChangeEventAction;
                    CurrentObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                }

            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PreviousPrimaryContentEventAction(object sender, EventArgs e)
        {
            try
            {
                if (sender != null && sender.GetType() == typeof(MgtContentItem))
                    CurrentPrimaryContent = CurrentDataContext.ListContentItem.FirstOrDefault((item) => item.IsPrimaryContent); ;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PrimaryContentEventAction(object sender, EventArgs e)
        {
            try
            {
                if (sender != null && sender.GetType() == typeof(MgtContentItem))
                {
                    MgtContentItem CurrentContent = (MgtContentItem)sender;

                    foreach (var content in CurrentDataContext.ListContentItem)
                    {
                        content.IsPrimaryContentEvent -= PrimaryContentEventAction;
                        content.PreviousIsPrimaryContentEvent -= PreviousPrimaryContentEventAction;
                    }

                    if (CurrentPrimaryContent != null && CurrentContent.IsPrimaryContent && CurrentPrimaryContent.GetHashCode() != CurrentContent.GetHashCode())
                    {
                        if (CurrentContent.State == ObjectState.CREATED)
                        {
                            MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNotPossibleSecondarytoPrimaryContent"), McgWpfTools.GetStringResource("MWT_MsgTitleRemoveItem"), MessageBoxButton.OK, MessageBoxImage.Warning);
                            CurrentContent.IsPrimaryContent = false;
                            CurrentPrimaryContent.IsPrimaryContent = true;
                        }
                        else if (CurrentContent.State == ObjectState.NEW)
                        {
                            if (CurrentPrimaryContent.State == ObjectState.CREATED)
                            {
                                if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgChangePrimarycontent"), McgWpfTools.GetStringResource("MWT_MsgTitleRemoveItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                    CurrentPrimaryContent.State = ObjectState.REMOVED;
                                else
                                {
                                    CurrentContent.IsPrimaryContent = false;
                                    CurrentPrimaryContent.IsPrimaryContent = true;
                                }
                            }

                        }
                        else if (CurrentContent.State == ObjectState.REMOVED)
                        {
                            CurrentContent.IsPrimaryContent = false;
                            CurrentPrimaryContent.IsPrimaryContent = true;
                        }
                    }
                    else if (CurrentPrimaryContent == null)
                    {
                        if (CurrentContent.State == ObjectState.REMOVED)
                            CurrentContent.IsPrimaryContent = false;
                        else if (CurrentContent.State == ObjectState.CREATED)
                        {
                            MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgNotPossibleSecondarytoPrimaryContent"), McgWpfTools.GetStringResource("MWT_MsgTitleRemoveItem"), MessageBoxButton.OK, MessageBoxImage.Warning);
                            CurrentContent.IsPrimaryContent = false;
                        }
                    }

                    foreach (var content in CurrentDataContext.ListContentItem)
                    {
                        content.IsPrimaryContentEvent += PrimaryContentEventAction;
                        content.PreviousIsPrimaryContentEvent += PreviousPrimaryContentEventAction;
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ParamChangeEventAction(object sender, EventArgs e)
        {
            try
            {
                if (sender != null && sender.GetType() == typeof(MgtWtObject))
                {
                    MgtWtObject CurrentWtObj = (MgtWtObject)sender;
                    if (CurrentWtObj.Status == ObjectState.CREATED)
                        CurrentWtObj.Status = ObjectState.UPDATED;
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void WtPartNumberChangeEventAction(object sender, EventArgs e)
        {
            try
            {
                ClearWtPart();
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void WtDocumentNumberChangeEventAction(object sender, EventArgs e)
        {
            try
            {
                ClearWtDocument();
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Windchill Methods
        private void CheckWindchillCredential()
        {
            try
            {
                if (WindchillCredential == null || !WindchillCredential.IsCredentialOk)
                {
                    WindchillCredential = _windchillCredentialService.GetWindchillCredential(CommonLibConstants.WindchillUrl, CommonLibConstants.WindchillUrl);
                }
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private List<RestOdataWtDocument> SearchWAllWtDocument()
        {
            try
            {
                List<RestOdataWtDocument> AllDocument = null;
                if (CurrentDataContext.FilterNumber != null && CurrentDataContext.FilterNumber.Trim() != "")
                {
                    CheckWindchillCredential();

                    CurrentDataContext.FilterNumber = CurrentDataContext.FilterNumber.Trim().ToUpper();
                    AllDocument = _windchillDocumentManagementService.GetListWtDocumentStartWithFilter(WindchillCredential.WindchillCredential, CurrentDataContext.FilterNumber, CommonLibConstants.WindchillUrl);
                }
                return AllDocument;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private List<RestOdataWtPart> SearchWAllWtPart()
        {
            try
            {
                List<RestOdataWtPart> AllWtPart = null;
                if (CurrentDataContext.FilterNumber != null && CurrentDataContext.FilterNumber.Trim() != "")
                {
                    CheckWindchillCredential();

                    CurrentDataContext.FilterNumber = CurrentDataContext.FilterNumber.Trim().ToUpper();
                    AllWtPart = _windchillPartManagementService.GetListWtPartStartWithFilter(WindchillCredential.WindchillCredential, CurrentDataContext.FilterNumber, CommonLibConstants.WindchillUrl);
                }
                return AllWtPart;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void SearchAllContext()
        {
            try
            {
                var TempContextList = _webtermTools.GetPdmContextList();
                List<WindchillContext> AllContext = new List<WindchillContext>();
                foreach (var context in TempContextList)
                    AllContext.Add(new WindchillContext()
                    {
                        Name = context.PdmContext,
                        ParticipantId = context.ParticipantId,
                        ParticipantName = context.ParticipantName,
                        ParticipantType = context.ParticipantType,
                        TeamRole = context.TeamRole,
                        Type = context.Type == "PRODUCT" ? WindchillContextType.PRODUCT : WindchillContextType.LIBRARY
                    });
                CurrentDataContext.AllWindchillContextList = AllContext.OrderBy((c) => c.Name).ToList();
                CurrentDataContext.WindchillContextList.Clear();
                foreach (var item in CurrentDataContext.AllWindchillContextList)
                    CurrentDataContext.WindchillContextList.Add(item);
                CurrentDataContext.SelectedWindchillContext = CurrentDataContext.WindchillContextList.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void SearchWtdocument()
        {
            try
            {
                RestOdataWtDocument LastDocument = null;
                LastDocument = _windchillDocumentManagementService.GetOneWtDocumentWithContent(WindchillCredential.WindchillCredential, CurrentDataContext.CurrentWtObject.WtDocumentObject.NUMBER, "Latest", CommonLibConstants.WindchillUrl);
                //LastDocument = WindchillRestOdataTool.GetOneWtDocument(WindchillCredential.WindchillCredential, CurrentDataContext.CurrentWtObject.WtDocumentObject.NUMBER, "Latest", McgMiscTools.GetAppSetting(this, "WindchillUrl"));
                if (LastDocument != null && LastDocument.Number != null)
                {
                    UpdateInstanceOfWtDocument(LastDocument);
                }
                else
                {
                    CurrentDataContext.CurrentWtObject.WtDocumentObject.Status = ObjectState.NEW;
                    CurrentDataContext.CurrentWtObject.WtDocumentObject.IsWtCommonAttributeEditable = true;
                    CurrentDataContext.CurrentWtObject.WtDocumentObject.IsWtNonVersionAttributeEditable = true;
                    CurrentDataContext.CurrentWtObject.WtDocumentObject.IsWtVersionAttributeEditable = true;
                    CurrentDataContext.CurrentWtObject.WtDocumentObject.IsObjectEditable = true;
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void SearchPart()
        {
            try
            {
                RestOdataWtPart LastPart = null;
                LastPart = _windchillPartManagementService.GetOnePart(WindchillCredential.WindchillCredential, CurrentDataContext.CurrentWtObject.WtPartObject.NUMBER, "Latest", CommonLibConstants.WindchillUrl);
                if (LastPart != null && LastPart.Number != null)
                {
                    UpdateInstanceOfWtPart(LastPart);
                }
                else
                {
                    CurrentDataContext.CurrentWtObject.WtPartObject.Status = ObjectState.NEW;
                    CurrentDataContext.CurrentWtObject.WtPartObject.Status = ObjectState.NEW;
                    CurrentDataContext.CurrentWtObject.WtPartObject.IsWtCommonAttributeEditable = true;
                    CurrentDataContext.CurrentWtObject.WtPartObject.IsWtNonVersionAttributeEditable = true;
                    CurrentDataContext.CurrentWtObject.WtPartObject.IsWtVersionAttributeEditable = true;
                    CurrentDataContext.CurrentWtObject.WtPartObject.IsObjectEditable = true;
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void SearchPartLinkedDocument()
        {
            try
            {
                RestOdataWtPart LastPart = null;
                LastPart = _windchillPartManagementService.GetPartWtDocumentDescribedBy(WindchillCredential.WindchillCredential, CurrentDataContext.CurrentWtObject.WindchillWtPart, CommonLibConstants.WindchillUrl);
                if (LastPart != null && LastPart.Number != null)
                {
                    if (LastPart.DescribedBy.Any(o => o.DescribedBy.Number == CurrentDataContext.CurrentWtObject.WindchillWtDocument.Number && o.DescribedBy.Revision == CurrentDataContext.CurrentWtObject.WindchillWtDocument.Revision))
                        CurrentDataContext.CurrentWtObject.LinkStatus = ObjectState.LINKED;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void SearchPartLinkedDocument(RestOdataWtPart currentPart)
        {
            try
            {

                CurrentDataContext.CurrentWtObject.LinkStatus = ObjectState.UNLINKED;
                RestOdataWtPart LastPart = null;
                LastPart = _windchillPartManagementService.GetPartWtDocumentDescribedBy(WindchillCredential.WindchillCredential, currentPart, CommonLibConstants.WindchillUrl);
                if (LastPart != null && LastPart.Number != null)
                {
                    if (LastPart.DescribedBy.Any(o => o.DescribedBy.Number == LastPart.Number && o.DescribedBy.Revision == LastPart.Revision))
                        CurrentDataContext.CurrentWtObject.LinkStatus = ObjectState.LINKED;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void UpdateInstanceOfWtDocument(RestOdataWtDocument SearchedWtDocument)
        {
            try
            {
                CurrentDataContext.CurrentWtObject.LinkStatus = ObjectState.UNLINKED;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.Description21ChangeEvent -= Description21ChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;

                CurrentDataContext.CurrentWtObject.WtDocumentObject.IsWtNonVersionAttributeEditable = false;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.NUMBER = SearchedWtDocument.Number;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.REVISION = SearchedWtDocument.Revision;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.PTCCOMMONNAME = SearchedWtDocument.Name;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.DESCRIPTION2 = SearchedWtDocument.DESCRIPTION_2;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.DESCRIPTION21 = SearchedWtDocument.DESCRIPTION2_1;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.DESCRIPTION22 = SearchedWtDocument.DESCRIPTION2_2;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.GROUPCREATOR = SearchedWtDocument.GROUP_CREATOR;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.State = SearchedWtDocument.State?.Value;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.Status = ObjectState.CREATED;
                CurrentDataContext.CurrentWtObject.WindchillWtDocument = SearchedWtDocument;
                CurrentDataContext.CurrentWtObject.WindchillDocumentType = SearchedWtDocument.DocTypeName;
                if (CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext == null)
                    CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext = new WindchillContext();
                CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext.Name = SearchedWtDocument.FolderLocation?.Substring(1).Split('/').FirstOrDefault();
                if (SearchedWtDocument.FolderName == null)
                    CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext.Folder = "Default";
                else
                    CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext.Folder = SearchedWtDocument.FolderName;

                // search RestOdataContext
                RestOdataFolder CurrentRestOdataFolder = _windchillDataAdminManagementService.GetContextFolderFromName(WindchillCredential.WindchillCredential, SearchedWtDocument.FolderLocation, CommonLibConstants.WindchillUrl);
                CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext.OdataContext = CurrentRestOdataFolder?.ParentContext;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext.OdataFolder = CurrentRestOdataFolder;

                CurrentDataContext.ListContentItem.Clear();
                if (SearchedWtDocument.PrimaryContent != null && SearchedWtDocument.PrimaryContent.FileName != null)
                    CurrentDataContext.ListContentItem.Add(new MgtContentItem()
                    {
                        CompleteFilename = SearchedWtDocument.PrimaryContent.FileName,
                        ContentType = WindchillContentType.PRIMARY_CONTENT,
                        IsPrimaryContent = true,
                        State = ObjectState.CREATED,
                        ItemId = SearchedWtDocument.PrimaryContent.ID,
                        Filecontent = new WindchillObjectViewableItemDownload()
                        {
                            CompleteFileName = SearchedWtDocument.PrimaryContent.FileName,
                            FileName = SearchedWtDocument.PrimaryContent.FileName,
                            Number = SearchedWtDocument.Number,
                            DownloadDirectLink = SearchedWtDocument.PrimaryContent.Content?.URL,
                            IsAlreadyDownloaded = false
                        }
                    });
                if (SearchedWtDocument.Attachments != null && SearchedWtDocument.Attachments.Count > 0)
                {
                    foreach (var doc in SearchedWtDocument.Attachments.Where((item) => item.FileName != null))
                    {
                        CurrentDataContext.ListContentItem.Add(new MgtContentItem()
                        {
                            CompleteFilename = doc.FileName,
                            ContentType = WindchillContentType.SECONDARY_CONTENT,
                            IsPrimaryContent = false,
                            State = ObjectState.CREATED,
                            ItemId = doc.ID,
                            Filecontent = new WindchillObjectViewableItemDownload()
                            {
                                CompleteFileName = doc.FileName,
                                FileName = doc.FileName,
                                Number = SearchedWtDocument.Number,
                                DownloadDirectLink = doc.Content?.URL,
                                IsAlreadyDownloaded = false
                            }
                        });
                    }
                }
                foreach (var content in CurrentDataContext.ListContentItem)
                {
                    content.IsPrimaryContentEvent += PrimaryContentEventAction;
                    content.PreviousIsPrimaryContentEvent += PreviousPrimaryContentEventAction;
                }

                CurrentDataContext.WtDocumentSelected = true;

                // check if WtDocument can be updated by user
                var tempObj = _windchillDocumentManagementService.IsWtDocumentCheckOutAllowed(WindchillCredential.WindchillCredential, CurrentDataContext.CurrentWtObject.WindchillWtDocument, CommonLibConstants.WindchillUrl);
                if (tempObj != null && tempObj.Number != null)
                    CurrentDataContext.CurrentWtObject.WtDocumentObject.IsObjectEditable = true;
                else
                {
                    CurrentDataContext.CurrentWtObject.WtDocumentObject.IsObjectEditable = false;
                    CurrentDataContext.CurrentWtObject.WtDocumentObject.Status = ObjectState.CREATED_RO;
                }



                CurrentDataContext.CurrentWtObject.WtDocumentObject.Description21ChangeEvent += Description21ChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;

                if ((CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.UPDATED
                    || CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.CREATED
                    || CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.CREATED_RO)
                    && (CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.UPDATED
                    || CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.CREATED
                    || CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.CREATED_RO))
                    //SearchPartLinkedDocument(CurrentDataContext.CurrentWtObject.WindchillWtPart);
                    SearchPartLinkedDocument();

            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void UpdateInstanceOfWtPart(RestOdataWtPart SearchedWtPart)
        {
            try
            {
                CurrentDataContext.CurrentWtObject.LinkStatus = ObjectState.UNLINKED;
                CurrentDataContext.CurrentWtObject.WtPartObject.Description21ChangeEvent -= Description21ChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtPartObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;

                CurrentDataContext.CurrentWtObject.WtPartObject.IsWtNonVersionAttributeEditable = false;
                CurrentDataContext.CurrentWtObject.WtPartObject.NUMBER = SearchedWtPart.Number;
                CurrentDataContext.CurrentWtObject.WtPartObject.REVISION = SearchedWtPart.Revision;
                CurrentDataContext.CurrentWtObject.WtPartObject.PTCCOMMONNAME = SearchedWtPart.Name;
                CurrentDataContext.CurrentWtObject.WtPartObject.DESCRIPTION2 = SearchedWtPart.DESCRIPTION_2;
                CurrentDataContext.CurrentWtObject.WtPartObject.DESCRIPTION21 = SearchedWtPart.DESCRIPTION2_1;
                CurrentDataContext.CurrentWtObject.WtPartObject.DESCRIPTION22 = SearchedWtPart.DESCRIPTION2_2;
                CurrentDataContext.CurrentWtObject.WtPartObject.GROUPCREATOR = SearchedWtPart.GROUP_CREATOR;
                CurrentDataContext.CurrentWtObject.WtPartObject.State = SearchedWtPart.State?.Value;
                CurrentDataContext.CurrentWtObject.WtPartObject.Unit = SearchedWtPart.DefaultUnit?.Value;
                CurrentDataContext.CurrentWtObject.WtPartObject.Status = ObjectState.CREATED;
                if (SearchedWtPart.MASS != null)
                    CurrentDataContext.CurrentWtObject.WtPartObject.MASS = SearchedWtPart.MASS.Value;
                CurrentDataContext.CurrentWtObject.WtPartObject.QUALINSPGRP = SearchedWtPart.QUALINSPGRP;
                CurrentDataContext.CurrentWtObject.WtPartObject.MATERIAL = SearchedWtPart.MATERIAL;
                CurrentDataContext.CurrentWtObject.WtPartObject.BRAND = SearchedWtPart.BRAND;
                CurrentDataContext.SelectedBrand = SearchedWtPart.BRAND;
                CurrentDataContext.CurrentWtObject.WtPartObject.GROUP = SearchedWtPart.GROUP;
                CurrentDataContext.SelectedGroup = SearchedWtPart.GROUP;
                CurrentDataContext.CurrentWtObject.WtPartObject.SUB_GROUP = SearchedWtPart.SUB_GROUP;
                CurrentDataContext.CurrentWtObject.WtPartObject.MODEL = SearchedWtPart.MODEL;
                CurrentDataContext.CurrentWtObject.WtPartObject.OPTION = SearchedWtPart.OPTION;
                CurrentDataContext.CurrentWtObject.WindchillWtPart = SearchedWtPart;
                if (CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext == null)
                    CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext = new WindchillContext();
                CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext.Name = SearchedWtPart.FolderLocation?.Substring(1).Split('/').FirstOrDefault();
                if (SearchedWtPart.FolderName == null)
                    CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext.Folder = "Default";
                else
                    CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext.Folder = SearchedWtPart.FolderName;

                // search RestOdataContext
                RestOdataFolder CurrentRestOdataFolder = _windchillDataAdminManagementService.GetContextFolderFromName(WindchillCredential.WindchillCredential, SearchedWtPart.FolderLocation, CommonLibConstants.WindchillUrl);
                CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext.OdataContext = CurrentRestOdataFolder?.ParentContext;
                CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext.OdataFolder = CurrentRestOdataFolder;

                CurrentDataContext.WtPartSelected = true;
                // check if WtPart can be updated by user
                var tempObj = _windchillPartManagementService.IsPartCheckOutAllowed(WindchillCredential.WindchillCredential, CurrentDataContext.CurrentWtObject.WindchillWtPart, CommonLibConstants.WindchillUrl);
                if (tempObj != null && tempObj.Number != null)
                    CurrentDataContext.CurrentWtObject.WtPartObject.IsObjectEditable = true;
                else
                {
                    CurrentDataContext.CurrentWtObject.WtPartObject.IsObjectEditable = false;
                    CurrentDataContext.CurrentWtObject.WtPartObject.Status = ObjectState.CREATED_RO;
                }

                CurrentDataContext.CurrentWtObject.WtPartObject.Description21ChangeEvent += Description21ChangeEventAction;
                CurrentDataContext.CurrentWtObject.WtPartObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                if ((CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.UPDATED
                    || CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.CREATED
                    || CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.CREATED_RO)
                    && (CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.UPDATED
                    || CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.CREATED
                    || CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.CREATED_RO))
                    //SearchPartLinkedDocument(CurrentDataContext.CurrentWtObject.WindchillWtPart);
                    SearchPartLinkedDocument();
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void CreateUpdateWtDocumentAsynch(bool IsNew = true)
        {
            try
            {
                // Create WtDocument
                if (IsNew)
                {
                    CurrentDataContext.StatusBarText = McgWpfTools.GetStringResource("MWT_MsgStartWtDocCreateUpdate");
                    if (CreateWtDocument(CurrentDataContext.CurrentWtObject))
                    {
                        CurrentDataContext.StatusBarText = $"{CurrentDataContext.StatusBarText} - {McgWpfTools.GetStringResource("MWT_MsgWtObjectCreated")}";
                        CurrentDataContext.CurrentWtObject.StatusWtDocument = "Doc Created";
                        if (CheckOutWtDocument(CurrentDataContext.CurrentWtObject))
                        {
                            CurrentDataContext.StatusBarText = $"{CurrentDataContext.StatusBarText} - {McgWpfTools.GetStringResource("MWT_MsgWtObjectCheckedOut")}";
                            CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Doc Checked out";
                            if (UpdateWtDocument(CurrentDataContext.CurrentWtObject))
                            {
                                CurrentDataContext.StatusBarText = $"{CurrentDataContext.StatusBarText} - {McgWpfTools.GetStringResource("MWT_MsgWtObjectUpdated")}";
                                CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Doc Updated";
                                if (UpdateContentWtDocument(CurrentDataContext.CurrentWtObject))
                                {
                                    CurrentDataContext.StatusBarText = $"{CurrentDataContext.StatusBarText} - {McgWpfTools.GetStringResource("MWT_MsgWtDocContentUpdated")}";
                                    CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Content Updated";
                                    if (CheckInWtDocument(CurrentDataContext.CurrentWtObject))
                                    {
                                        CurrentDataContext.StatusBarText = $"{CurrentDataContext.StatusBarText} - {McgWpfTools.GetStringResource("MWT_MsgWtObjectCheckedIn")}";
                                        CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Checked in";
                                        CurrentDataContext.CurrentWtObject.RequiredActionWtDocument = MgtRequiredActionEnum.UPDATE;
                                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgWtDocumentCreated"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleWtDocumentCreated"), MessageBoxButton.OK, MessageBoxImage.Information);
                                        CurrentDataContext.CurrentWtObject.WtDocumentObject.Status = ObjectState.CREATED;

                                    }
                                    else
                                    {
                                        CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Check in issue";
                                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckInNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                                else
                                {
                                    CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Content Update issue";
                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateContentNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateContentNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Update issue";
                                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Check out issue";
                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckOutNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        CurrentDataContext.CurrentWtObject.StatusWtDocument = $"Create issue";
                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCreateNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCreateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // Update WtDocument
                else
                {
                    if (CheckOutWtDocument(CurrentDataContext.CurrentWtObject))
                    {
                        CurrentDataContext.StatusBarText = $"{CurrentDataContext.StatusBarText} - {McgWpfTools.GetStringResource("MWT_MsgWtObjectCheckedOut")}";
                        CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Doc Checked out";
                        if (UpdateWtDocument(CurrentDataContext.CurrentWtObject))
                        {
                            CurrentDataContext.StatusBarText = $"{CurrentDataContext.StatusBarText} - {McgWpfTools.GetStringResource("MWT_MsgWtObjectUpdated")}";
                            CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Doc Updated";
                            if (UpdateContentWtDocument(CurrentDataContext.CurrentWtObject))
                            {
                                CurrentDataContext.StatusBarText = $"{CurrentDataContext.StatusBarText} - {McgWpfTools.GetStringResource("MWT_MsgWtDocContentUpdated")}";
                                CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Content Updated";
                                if (CheckInWtDocument(CurrentDataContext.CurrentWtObject))
                                {
                                    CurrentDataContext.StatusBarText = $"{CurrentDataContext.StatusBarText} - {McgWpfTools.GetStringResource("MWT_MsgWtObjectCheckedIn")}";
                                    CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Checked in";
                                    CurrentDataContext.CurrentWtObject.RequiredActionWtDocument = MgtRequiredActionEnum.UPDATE;
                                    if ((CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.UPDATED
                                        || CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.CREATED
                                        || CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.CREATED_RO)
                                        && (CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.UPDATED
                                        || CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.CREATED
                                        || CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.CREATED_RO))
                                        //SearchPartLinkedDocument(CurrentDataContext.CurrentWtObject.WindchillWtPart);
                                        SearchPartLinkedDocument();

                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgWtDocumentUpdated"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleWtDocumentUpdated"), MessageBoxButton.OK, MessageBoxImage.Information);
                                    CurrentDataContext.CurrentWtObject.WtDocumentObject.Status = ObjectState.CREATED;
                                }
                                else
                                {
                                    CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Check in issue";
                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckInNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Content Update issue";
                                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateContentNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateContentNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Update issue";
                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        CurrentDataContext.CurrentWtObject.StatusWtDocument = $"{CurrentDataContext.CurrentWtObject.StatusWtDocument} - Check out issue";
                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckOutNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }

            }
            catch (Exception ex)
            {
                CurrentDataContext.StatusBarText = McgWpfTools.GetStringResource("MWT_MsgErrorWtDocCreateUpdate");
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private bool CreateWtDocument(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                CheckWindchillCredential();
                RestOdataWtDocument NewDocument = new RestOdataWtDocument()
                {
                    Name = WtDocItem.WtDocumentObject.PTCCOMMONNAME,
                    Number = WtDocItem.WtDocumentObject.NUMBER,
                    DESCRIPTION_2 = WtDocItem.WtDocumentObject.DESCRIPTION2,
                    DESCRIPTION2_2 = WtDocItem.WtDocumentObject.DESCRIPTION22,
                    DESCRIPTION2_1 = WtDocItem.WtDocumentObject.DESCRIPTION21,
                    GROUP_CREATOR = WtDocItem.WtDocumentObject.GROUPCREATOR,
                    MASS = WtDocItem.WtDocumentObject.MASS,
                    //OdataType = $"#{WtDocItem.WtDocumentOdataType.ToString().Replace('_', '.')}"
                    OdataType = $"#PTC.DocMgmt.{WtDocItem.WtDocumentOdataType}"
                };
                NewDocument = _windchillDocumentManagementService.CreateWtDocument(WindchillCredential.WindchillCredential,
                                    NewDocument,
                                    WtDocItem.WtDocumentObject.SelectedWindchillContext.OdataContext.ID,
                                    WtDocItem.WtDocumentObject.SelectedWindchillContext.OdataFolder.ID,
                                    CommonLibConstants.WindchillUrl);
                if (NewDocument == null)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateNotDone"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateNotDone"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                bool ReviseOk = true;
                WtDocItem.WindchillWtDocument = NewDocument;
                if (NewDocument.Revision.Replace("#", "BLANK") != WtDocItem.WtDocumentObject.REVISION)
                {
                    NewDocument = ReviseWtDocument(WtDocItem);
                    ReviseOk = NewDocument != null;
                }

                if (ReviseOk && NewDocument != null)
                {
                    WtDocItem.WtDocumentFound = true;
                    WtDocItem.WtDocumentRevisionFound = true;
                    WtDocItem.WindchillWtDocument = NewDocument;
                    return true;
                }
                else
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateNotDone"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateNotDone"), MessageBoxButton.YesNo, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private RestOdataWtDocument ReviseWtDocument(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                RestOdataWtDocument ReviseDocument = _windchillDocumentManagementService.ReviseWtDocument(WindchillCredential.WindchillCredential,
                                                            WtDocItem.WindchillWtDocument,
                                                            WtDocItem.WtDocumentObject.REVISION,
                                                            CommonLibConstants.WindchillUrl);
                if (ReviseDocument != null)
                {
                    WtDocItem.WindchillWtDocument = ReviseDocument;
                    WtDocItem.WtDocumentRevisionFound = true;
                    return ReviseDocument;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private bool CheckOutWtDocument(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                RestOdataWtDocument NewDocument = _windchillDocumentManagementService.IsWtDocumentCheckOutAllowed(WindchillCredential.WindchillCredential,
                                                                            WtDocItem.WindchillWtDocument,
                                                                            CommonLibConstants.WindchillUrl);
                if (NewDocument != null)
                {
                    NewDocument = _windchillDocumentManagementService.CheckOutWtDocument(WindchillCredential.WindchillCredential,
                                                                            WtDocItem.WindchillWtDocument,
                                                                            CommonLibConstants.WindchillUrl);
                    if (NewDocument != null)
                    {
                        WtDocItem.WindchillWtDocument = NewDocument;
                        //NewDocument.UpdateOOTBPropertiesWtDocument(WtDocItem.WindchillWtDocument);
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private bool CheckInWtDocument(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                RestOdataWtDocument NewDocument = _windchillDocumentManagementService.CheckInWtDocument(WindchillCredential.WindchillCredential,
                                                            WtDocItem.WindchillWtDocument,
                                                            CommonLibConstants.WindchillUrl);
                if (NewDocument != null)
                {
                    WtDocItem.WindchillWtDocument = NewDocument;
                    //NewDocument.UpdateOOTBPropertiesWtDocument(WtDocItem.WindchillWtDocument);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private bool UpdateWtDocument(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                if (WtDocItem.WtDocumentObject.DESCRIPTION2 == null) WtDocItem.WtDocumentObject.DESCRIPTION2 = "";
                if (WtDocItem.WtDocumentObject.DESCRIPTION21 == null) WtDocItem.WtDocumentObject.DESCRIPTION21 = "";
                if (WtDocItem.WtDocumentObject.DESCRIPTION22 == null) WtDocItem.WtDocumentObject.DESCRIPTION22 = "";
                if (WtDocItem.WtDocumentObject.GROUPCREATOR == null) WtDocItem.WtDocumentObject.GROUPCREATOR = "";
                if (WtDocItem.WtDocumentObject.QUALINSPGRP == null) WtDocItem.WtDocumentObject.QUALINSPGRP = "";
                WtDocItem.WindchillWtDocument.DESCRIPTION_2 = WtDocItem.WtDocumentObject.DESCRIPTION2?.Trim().ToUpper();
                WtDocItem.WindchillWtDocument.DESCRIPTION2_1 = WtDocItem.WtDocumentObject.DESCRIPTION21?.Trim().ToUpper();
                WtDocItem.WindchillWtDocument.DESCRIPTION2_2 = WtDocItem.WtDocumentObject.DESCRIPTION22?.Trim().ToUpper();
                WtDocItem.WindchillWtDocument.GROUP_CREATOR = WtDocItem.WtDocumentObject.GROUPCREATOR?.Trim().ToUpper();
                WtDocItem.WindchillWtDocument.QUALINSPGRP = WtDocItem.WtDocumentObject.QUALINSPGRP?.Trim().ToUpper();
                WtDocItem.WindchillWtDocument.MASS = WtDocItem.WtDocumentObject.MASS;

                RestOdataWtDocument NewDocument = _windchillDocumentManagementService.UpdateWtDocument(WindchillCredential.WindchillCredential,
                                                            WtDocItem.WindchillWtDocument,
                                                            CommonLibConstants.WindchillUrl);
                if (NewDocument != null)
                {
                    WtDocItem.WindchillWtDocument = NewDocument;
                    //NewDocument.UpdateOOTBPropertiesWtDocument(WtDocItem.WindchillWtDocument);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private bool UpdateContentWtDocument(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                List<RestOdataReplicaContentFileItem> FilesNew = new List<RestOdataReplicaContentFileItem>();
                List<RestOdataReplicaContentFileItem> FilesRemoved = new List<RestOdataReplicaContentFileItem>();

                // If no content to upload return true
                if (WtDocItem.ListContentItem == null || WtDocItem.ListContentItem.Count == 0)
                    return true;

                // Add new content
                foreach (var file in WtDocItem.ListContentItem.Where((item) => item.State == ObjectState.NEW))
                {
                    FilesNew.Add(new RestOdataReplicaContentFileItem()
                    {
                        CompleteFileName = file.CompleteFilename,
                        FileName = file.Filename,
                        PrimaryContent = file.IsPrimaryContent,
                        ID = file.ItemId
                    });
                }
                RestOdataWtDocument NewDocument = null;
                if (FilesNew.Count != 0)
                    NewDocument = _windchillDocumentManagementService.UploadContentWtDocument(WindchillCredential.WindchillCredential,
                                                            WtDocItem.WindchillWtDocument,
                                                            FilesNew,
                                                            CommonLibConstants.WindchillUrl);

                // Removed Content
                foreach (var file in WtDocItem.ListContentItem.Where((item) => item.State == ObjectState.REMOVED))
                {
                    FilesRemoved.Add(new RestOdataReplicaContentFileItem()
                    {
                        CompleteFileName = file.CompleteFilename,
                        FileName = file.Filename,
                        PrimaryContent = file.IsPrimaryContent,
                        ID = file.ItemId
                    });
                }
                if (FilesRemoved.Count != 0)
                    NewDocument = _windchillDocumentManagementService.DeleteSecondaryContentWtDocument(WindchillCredential.WindchillCredential,
                                                            WtDocItem.WindchillWtDocument,
                                                            FilesRemoved,
                                                            CommonLibConstants.WindchillUrl);

                if (FilesNew.Count == 0 && FilesRemoved.Count == 0)
                    return true;

                if (NewDocument != null)
                {
                    WtDocItem.WindchillWtDocument = NewDocument;
                    foreach (var file in WtDocItem.ListContentItem.Where((item) => item.State == ObjectState.NEW))
                        file.State = ObjectState.CREATED;

                    MainDispatcher.Invoke(new Action(UpdateListContent));

                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void CreateUpdateWtPartAsynch(bool IsNew = true)
        {
            try
            {
                // Create new WTPart
                if (IsNew)
                {
                    if (CreateWtPart(CurrentDataContext.CurrentWtObject))
                    {
                        CurrentDataContext.CurrentWtObject.StatusPart = $"Created";
                        CurrentDataContext.CurrentWtObject.RequiredActionPart = MgtRequiredActionEnum.UPDATE;
                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgWtPartCreated"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleWtPartCreated"), MessageBoxButton.OK, MessageBoxImage.Information);
                        CurrentDataContext.CurrentWtObject.WtPartObject.Status = ObjectState.CREATED;
                    }
                    else
                    {
                        CurrentDataContext.CurrentWtObject.StatusPart = $"Create issue";
                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCreateNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCreateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // Update  WTPart
                else
                {
                    if (CheckOutWtPart(CurrentDataContext.CurrentWtObject))
                    {
                        CurrentDataContext.CurrentWtObject.StatusPart = $"Checked out";
                        if (UpdateWtPart(CurrentDataContext.CurrentWtObject))
                        {
                            CurrentDataContext.CurrentWtObject.StatusPart = $"{CurrentDataContext.CurrentWtObject.StatusPart} - Updated";
                            if (CheckInWtPart(CurrentDataContext.CurrentWtObject))
                            {
                                CurrentDataContext.CurrentWtObject.StatusPart = $"{CurrentDataContext.CurrentWtObject.StatusPart} - Checked in";
                                if (UpdateWtPartCommonProperties(CurrentDataContext.CurrentWtObject))
                                {
                                    CurrentDataContext.CurrentWtObject.StatusPart = $"{CurrentDataContext.CurrentWtObject.StatusPart} - Unit Updated";
                                    CurrentDataContext.CurrentWtObject.RequiredActionPart = MgtRequiredActionEnum.UPDATE;
                                    if ((CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.UPDATED
                                        || CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.CREATED
                                        || CurrentDataContext.CurrentWtObject.WtDocumentObject.Status == ObjectState.CREATED_RO)
                                        && (CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.UPDATED
                                        || CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.CREATED
                                        || CurrentDataContext.CurrentWtObject.WtPartObject.Status == ObjectState.CREATED_RO))
                                        //SearchPartLinkedDocument(CurrentDataContext.CurrentWtObject.WindchillWtPart);
                                        SearchPartLinkedDocument();

                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgWtPartUpdated"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleWtPartUpdated"), MessageBoxButton.OK, MessageBoxImage.Information);
                                    CurrentDataContext.CurrentWtObject.WtPartObject.Status = ObjectState.CREATED;
                                }
                                else
                                {
                                    CurrentDataContext.CurrentWtObject.StatusPart = $"{CurrentDataContext.CurrentWtObject.StatusPart} - Unit Updated issue";
                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckInNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                CurrentDataContext.CurrentWtObject.StatusPart = $"{CurrentDataContext.CurrentWtObject.StatusPart} - Check in issue";
                                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckInNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            CurrentDataContext.CurrentWtObject.StatusPart = $"{CurrentDataContext.CurrentWtObject.StatusPart} - Update issue";
                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        CurrentDataContext.CurrentWtObject.StatusPart = $"Check out issue";
                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckOutNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                CurrentDataContext.StatusBarText = McgWpfTools.GetStringResource("MWT_MsgErrorWtPartCreateUpdate");
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void LinkWtPartWtDocumentAsynch()
        {
            try
            {

                if (CheckOutWtPart(CurrentDataContext.CurrentWtObject))
                {
                    CurrentDataContext.CurrentWtObject.StatusPart = $"Checked out";
                    RestOdataWtPart LastPart = null;
                    LastPart = _windchillPartManagementService.LinkPartWtDocumentDescribedBy(WindchillCredential.WindchillCredential,
                                                                                    CurrentDataContext.CurrentWtObject.WindchillWtPart,
                                                                                    CurrentDataContext.CurrentWtObject.WindchillWtDocument,
                                                                                    CommonLibConstants.WindchillUrl);
                    if (LastPart != null)
                    {
                        if (CheckInWtPart(CurrentDataContext.CurrentWtObject))
                        {
                            CurrentDataContext.CurrentWtObject.StatusPart = $"{CurrentDataContext.CurrentWtObject.StatusPart} - Checked in";
                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgTitleWtPartWtDocumentLinked"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleWtPartUpdated"), MessageBoxButton.OK, MessageBoxImage.Information);
                            CurrentDataContext.CurrentWtObject.LinkStatus = ObjectState.LINKED;
                        }
                        else
                        {
                            CurrentDataContext.CurrentWtObject.StatusPart = $"{CurrentDataContext.CurrentWtObject.StatusPart} - Check in issue";
                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckInNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        CurrentDataContext.CurrentWtObject.StatusPart = $"{CurrentDataContext.CurrentWtObject.StatusPart} - link issue";
                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgLinkNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    CurrentDataContext.CurrentWtObject.StatusPart = $"Check out issue";
                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckOutNotPossible"), CurrentDataContext.CurrentWtObject.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {
                CurrentDataContext.StatusBarText = McgWpfTools.GetStringResource("MWT_MsgErrorWtPartCreateUpdate");
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private bool CreateWtPart(MgtWtDocumentItem WtDocItem)
        {
            try
            {

                CheckWindchillCredential();
                if (WtDocItem.WtPartObject.DESCRIPTION2 == null)
                    WtDocItem.WtPartObject.DESCRIPTION2 = "";
                if (WtDocItem.WtPartObject.DESCRIPTION22 == null)
                    WtDocItem.WtPartObject.DESCRIPTION22 = "";
                if (WtDocItem.WtPartObject.DESCRIPTION21 == null)
                    WtDocItem.WtPartObject.DESCRIPTION21 = "";
                if (WtDocItem.WtPartObject.GROUPCREATOR == null)
                    WtDocItem.WtPartObject.GROUPCREATOR = "";
                if (WtDocItem.WtPartObject.QUALINSPGRP == null)
                    WtDocItem.WtPartObject.QUALINSPGRP = "";
                if (WtDocItem.WtPartObject.GROUP == null)
                    WtDocItem.WtPartObject.GROUP = "";
                if (WtDocItem.WtPartObject.SUB_GROUP == null)
                    WtDocItem.WtPartObject.SUB_GROUP = "";
                if (WtDocItem.WtPartObject.BRAND == null)
                    WtDocItem.WtPartObject.BRAND = "";
                if (WtDocItem.WtPartObject.MODEL == null)
                    WtDocItem.WtPartObject.MODEL = "";

                RestOdataWtPart NewPart = new RestOdataWtPart()
                {
                    Name = WtDocItem.WtPartObject.PTCCOMMONNAME?.Trim().ToUpper(),
                    Number = WtDocItem.WtPartObject.NUMBER?.Trim().ToUpper(),
                    DESCRIPTION_2 = WtDocItem.WtPartObject.DESCRIPTION2?.Trim().ToUpper(),
                    DESCRIPTION2_2 = WtDocItem.WtPartObject.DESCRIPTION22?.Trim().ToUpper(),
                    DESCRIPTION2_1 = WtDocItem.WtPartObject.DESCRIPTION21?.Trim().ToUpper(),
                    GROUP_CREATOR = WtDocItem.WtPartObject.GROUPCREATOR?.Trim().ToUpper(),
                    QUALINSPGRP = WtDocItem.WtPartObject.QUALINSPGRP?.Trim().ToUpper(),
                    MATERIAL = WtDocItem.WtPartObject.MATERIAL?.Trim().ToUpper(),
                    GROUP = WtDocItem.WtPartObject.GROUP?.Trim().ToUpper(),
                    SUB_GROUP = WtDocItem.WtPartObject.SUB_GROUP?.Trim().ToUpper(),
                    BRAND = WtDocItem.WtPartObject.BRAND?.Trim().ToUpper(),
                    MODEL = WtDocItem.WtPartObject.MODEL?.Trim().ToUpper(),
                    MASS = WtDocItem.WtPartObject.MASS,
                    OdataType = $"#{WtDocItem.WtPartOdataType.ToString().Replace('_', '.')}",
                    DefaultUnit = new RestOdataUnit() { Value = WtDocItem.WtPartObject.Unit }
                };
                if (NewPart.Name.Length > 60)
                    NewPart.Name = NewPart.Name.Substring(0, 60);
                NewPart = _windchillPartManagementService.CreatePart(WindchillCredential.WindchillCredential,
                                    NewPart,
                                    WtDocItem.WtPartObject.SelectedWindchillContext.OdataContext.ID,
                                    WtDocItem.WtPartObject.SelectedWindchillContext.OdataFolder.ID,
                                    CommonLibConstants.WindchillUrl);

                if (NewPart != null)
                {
                    bool ReviseOk = true;
                    WtDocItem.WindchillWtPart = NewPart;
                    if (NewPart.Revision.Replace("#", "BLANK") != WtDocItem.WtPartObject.REVISION)
                    {
                        NewPart = ReviseWtPart(WtDocItem);
                        ReviseOk = NewPart != null;
                    }

                    if (ReviseOk && NewPart != null)
                    {
                        WtDocItem.PartFound = true;
                        WtDocItem.PartRevisionFound = true;
                        WtDocItem.WindchillWtPart = NewPart;
                        return true;
                    }
                    else
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateNotDone"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateNotDone"), MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateNotDone"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateNotDone"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private RestOdataWtPart ReviseWtPart(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                RestOdataWtPart RevisedPart = _windchillPartManagementService.RevisePart(WindchillCredential.WindchillCredential,
                                                            WtDocItem.WindchillWtPart,
                                                            WtDocItem.WtPartObject.REVISION,
                                                            CommonLibConstants.WindchillUrl);
                if (RevisedPart != null)
                {
                    WtDocItem.WindchillWtPart = RevisedPart;
                    WtDocItem.WtDocumentRevisionFound = true;
                    return RevisedPart;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private bool CheckOutWtPart(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                RestOdataWtPart NewPart = _windchillPartManagementService.IsPartCheckOutAllowed(WindchillCredential.WindchillCredential,
                                                                            WtDocItem.WindchillWtPart,
                                                                            CommonLibConstants.WindchillUrl);
                if (NewPart != null)
                {
                    NewPart = _windchillPartManagementService.CheckOutPart(WindchillCredential.WindchillCredential,
                                                                            WtDocItem.WindchillWtPart,
                                                                            CommonLibConstants.WindchillUrl);
                    if (NewPart != null)
                    {
                        WtDocItem.WindchillWtPart = NewPart;
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private bool CheckInWtPart(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                RestOdataWtPart NewPart = _windchillPartManagementService.CheckInPart(WindchillCredential.WindchillCredential,
                                                            WtDocItem.WindchillWtPart,
                                                            CommonLibConstants.WindchillUrl);
                if (NewPart != null)
                {
                    WtDocItem.WindchillWtPart = NewPart;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private bool UpdateWtPart(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                WtDocItem.WindchillWtPart.DESCRIPTION_2 = WtDocItem.WtPartObject.DESCRIPTION2?.Trim().ToUpper();
                WtDocItem.WindchillWtPart.DESCRIPTION2_1 = WtDocItem.WtPartObject.DESCRIPTION21?.Trim().ToUpper();
                WtDocItem.WindchillWtPart.DESCRIPTION2_2 = WtDocItem.WtPartObject.DESCRIPTION22?.Trim().ToUpper();
                WtDocItem.WindchillWtPart.GROUP_CREATOR = WtDocItem.WtPartObject.GROUPCREATOR?.Trim().ToUpper();
                WtDocItem.WindchillWtPart.QUALINSPGRP = WtDocItem.WtPartObject.QUALINSPGRP?.Trim().ToUpper();
                WtDocItem.WindchillWtPart.MASS = WtDocItem.WtPartObject.MASS;
                WtDocItem.WindchillWtPart.MATERIAL = WtDocItem.WtPartObject.MATERIAL;
                WtDocItem.WindchillWtPart.GROUP = WtDocItem.WtPartObject.GROUP;
                WtDocItem.WindchillWtPart.SUB_GROUP = WtDocItem.WtPartObject.SUB_GROUP;
                WtDocItem.WindchillWtPart.BRAND = WtDocItem.WtPartObject.BRAND;
                WtDocItem.WindchillWtPart.MODEL = WtDocItem.WtPartObject.MODEL;
                WtDocItem.WindchillWtPart.OPTION = WtDocItem.WtPartObject.OPTION;
                WtDocItem.WindchillWtPart.DefaultUnit = new RestOdataUnit() { Value = WtDocItem.WtPartObject.Unit };

                RestOdataWtPart NewPart = _windchillPartManagementService.UpdatePart(WindchillCredential.WindchillCredential,
                                                            WtDocItem.WindchillWtPart,
                                                            CommonLibConstants.WindchillUrl);
                if (NewPart != null)
                {
                    WtDocItem.WindchillWtPart = NewPart;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private bool UpdateWtPartCommonProperties(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                WtDocItem.WindchillWtPart.DESCRIPTION_2 = WtDocItem.WtPartObject.DESCRIPTION2?.Trim().ToUpper();
                WtDocItem.WindchillWtPart.DESCRIPTION2_1 = WtDocItem.WtPartObject.DESCRIPTION21?.Trim().ToUpper();
                WtDocItem.WindchillWtPart.DESCRIPTION2_2 = WtDocItem.WtPartObject.DESCRIPTION22?.Trim().ToUpper();
                WtDocItem.WindchillWtPart.GROUP_CREATOR = WtDocItem.WtPartObject.GROUPCREATOR?.Trim().ToUpper();
                WtDocItem.WindchillWtPart.QUALINSPGRP = WtDocItem.WtPartObject.QUALINSPGRP?.Trim().ToUpper();
                WtDocItem.WindchillWtPart.MASS = WtDocItem.WtPartObject.MASS;
                WtDocItem.WindchillWtPart.MATERIAL = WtDocItem.WtPartObject.MATERIAL;
                WtDocItem.WindchillWtPart.DefaultUnit = new RestOdataUnit() { Value = WtDocItem.WtPartObject.Unit };

                RestOdataWtPart NewPart = _windchillPartManagementService.UpdatePartCommonProperties(WindchillCredential.WindchillCredential,
                                                            WtDocItem.WindchillWtPart,
                                                            CommonLibConstants.WindchillUrl);
                if (NewPart != null)
                {
                    WtDocItem.WindchillWtPart = NewPart;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void UpdateListContent()
        {
            try
            {
                var ListContentRemoved = CurrentDataContext.ListContentItem.Where((item) => item.State == ObjectState.REMOVED).ToList();
                foreach (var file in ListContentRemoved)
                    CurrentDataContext.ListContentItem.Remove(file);
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void ClearWtDocument()
        {
            try
            {
                CurrentDataContext.CurrentWtObject.WtDocumentObject.Status = ObjectState.UNKNOWN;
                CurrentDataContext.WtDocumentSelected = false;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.REVISION = McgRevisionSchemaEnum.A.ToString();
                CurrentDataContext.CurrentWtObject.WtDocumentObject.State = "Unknown";
                CurrentDataContext.CurrentWtObject.WtDocumentObject.PTCCOMMONNAME = null;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.DESCRIPTION2 = null;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.DESCRIPTION21 = null;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.DESCRIPTION22 = null;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.GROUPCREATOR = null;
                CurrentDataContext.CurrentWtObject.WtDocumentObject.QUALINSPGRP = null;
                //if (CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext == null)
                CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext = new WindchillContext();
                CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext.Name = "";
                CurrentDataContext.CurrentWtObject.WtDocumentObject.SelectedWindchillContext.Folder = "";
                CurrentDataContext.ListContentItem.Clear();
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void ClearWtPart()
        {
            try
            {
                CurrentDataContext.CurrentWtObject.WtPartObject.Status = ObjectState.UNKNOWN;
                CurrentDataContext.WtPartSelected = false;
                CurrentDataContext.CurrentWtObject.WtPartObject.REVISION = McgRevisionSchemaEnum.A.ToString();
                CurrentDataContext.CurrentWtObject.WtPartObject.State = "Unknown";
                CurrentDataContext.CurrentWtObject.WtPartObject.PTCCOMMONNAME = null;
                CurrentDataContext.CurrentWtObject.WtPartObject.DESCRIPTION2 = null;
                CurrentDataContext.CurrentWtObject.WtPartObject.DESCRIPTION21 = null;
                CurrentDataContext.CurrentWtObject.WtPartObject.DESCRIPTION22 = null;
                CurrentDataContext.CurrentWtObject.WtPartObject.GROUPCREATOR = null;
                CurrentDataContext.CurrentWtObject.WtPartObject.QUALINSPGRP = null;
                CurrentDataContext.CurrentWtObject.WtPartObject.MASS = 0;
                CurrentDataContext.CurrentWtObject.WtPartObject.MATERIAL = null;
                //if (CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext == null)
                CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext = new WindchillContext();
                CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext.Name = "";
                CurrentDataContext.CurrentWtObject.WtPartObject.SelectedWindchillContext.Folder = "";
                CurrentDataContext.CurrentWtObject.WtPartObject.Unit = CurrentDataContext.AllUnits.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void UpdateMaterialListFromFolder()
        {
            try
            {
                string MaterialFolder = ManageWTObjectConstants.MaterialFolder;
                string MaterialFilter = ManageWTObjectConstants.MaterialFilter;

                if (MaterialFolder != null && MaterialFilter != null)
                {

                    var AllFiles = Directory.EnumerateFiles(MaterialFolder, MaterialFilter).ToList();
                    string tempFile;
                    foreach (var elem in AllFiles)
                    {
                        tempFile = elem.Split('\\').LastOrDefault();
                        tempFile = tempFile.Split('.').FirstOrDefault();
                        CurrentDataContext.MaterialList.Add(tempFile);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
