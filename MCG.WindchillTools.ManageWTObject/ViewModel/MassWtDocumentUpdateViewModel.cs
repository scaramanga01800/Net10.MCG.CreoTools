using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WebtermLib.Services.Interfaces;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.View.WindchillContextSelection;
using MCG.CommonLib.WpfComponent.WindchillCredential;
using MCG.WindchillRequestTool.Configuration;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillRequestTool.Services.Interfaces;
using MCG.WindchillTools.ManageWTObject.Configuration;
using MCG.WindchillTools.ManageWTObject.Exceptions;
using MCG.WindchillTools.ManageWTObject.Interfaces;
using MCG.WindchillTools.ManageWTObject.View;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.WindchillTools.ManageWTObject.ViewModel
{
    public class MassWtDocumentUpdateViewModel : ObservableObject, IMassWtDocumentUpdateViewModel
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
        public MassWtDocumentUpdateDataContext CurrentDataContext { get; set; }

        private bool _IsSingleWtDocumentDragDropInProgress = false;
        public bool IsSingleWtDocumentDragDropInProgress
        {
            get { return _IsSingleWtDocumentDragDropInProgress; }
            set
            {
                if (this._IsSingleWtDocumentDragDropInProgress != value)
                {
                    this._IsSingleWtDocumentDragDropInProgress = value;
                    OnPropertyChanged();
                }

            }
        }

        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private Dispatcher MainDispatcher { get; set; } = null;
        private MassWtDocumentUpdateConfiguration ApplicationConfiguration { get; set; }
        private WindchillCredentialItem WindchillCredential { get; set; } = null;
        public List<Webterm> ListWebterm { get; set; }
        private List<MgtWtObject> listItemFromClipboard { get; set; } = new List<MgtWtObject>();
        private bool FromClipboard { get; set; } = false;

        private readonly IWebtermTools _webtermTools;
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly IWindchillDocumentManagementService _windchillDocumentManagementService;
        private readonly IWindchillPartManagementService _windchillPartManagementService;
        private readonly IWindchillDataAdminManagementService _windchillDataAdminManagementService;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly IMcgWindchillToolsManageWTObjectWindowService _mcgWindchillToolsManageWTObjectWindowService;

        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        #endregion

        #region [REGION] Commands
        public ICommand CommandPaste { get => new RelayCommand<KeyEventArgs>((obj) => ExecutePaste(obj)); }
        public ICommand CommandDragAndDrop { get => new RelayCommand<DragEventArgs>((obj) => ExecuteDragAndDrop(obj)); }
        public ICommand CommandCheckUncheckAll { get => new RelayCommand<bool>((obj) => ExecuteCheckUncheckAll(obj)); }
        public ICommand CommandDragAndDropWtDocument { get => new RelayCommand<object>((obj) => ExecuteDragAndDropWtDocument(obj)); }
        public ICommand CommandAddWtDocument { get => new RelayCommand(() => ExecuteAddWtDocument()); }
        public ICommand CommandCheckWtDocument { get => new RelayCommand(() => ExecuteCheckWtDocument()); }
        public ICommand CommandCheckPart { get => new RelayCommand(() => ExecuteCheckPart()); }
        public ICommand CommandRemoveWtDocument { get => new RelayCommand(() => ExecuteRemoveWtDocument()); }
        public ICommand CommandApplyWtDocumentType { get => new RelayCommand(() => ExecuteApplyWtDocumentType()); }
        public ICommand CommandApplyWtPartType { get => new RelayCommand(() => ExecuteApplyWtPartType()); }
        public ICommand CommandApplyContext { get => new RelayCommand(() => ExecuteApplyContext()); }
        public ICommand CommandCreateUpdateWtDocument { get => new RelayCommand(() => ExecuteCreateUpdateWtDocument()); }
        public ICommand CommandCreateUpdateWtPart { get => new RelayCommand(() => ExecuteCreateUpdateWtPart()); }
        public ICommand CommandCreateUpdateLink { get => new RelayCommand(() => ExecuteCreateUpdateLink()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        public ICommand CommandReviseItem { get => new RelayCommand<object>((obj) => ExecuteReviseItem(obj)); }
        public ICommand CommandRemoveItem { get => new RelayCommand<object>((obj) => ExecuteRemoveItem(obj)); }
        public ICommand CommandDragAndDropXls { get => new RelayCommand<DragEventArgs>((obj) => ExecuteDragAndDropXls(obj)); }
        public ICommand CommandDragAndDropSecondaryContent { get => new RelayCommand<DragEventArgs>((obj) => ExecuteDragAndDropSecondaryContent(obj)); }
        public ICommand CommandRemoveContent { get => new RelayCommand<object>((obj) => ExecuteRemoveContent(obj)); }
        public ICommand CommandChangeContext { get => new RelayCommand<object>((obj) => ExecuteChangeContext(obj)); }
        public ICommand CommandApplyWebterm { get => new RelayCommand(() => ExecuteApplyWebterm()); }
        public ICommand CommandRenameWtDocument { get => new RelayCommand(() => ExecuteRenameWtDocument()); }
        public ICommand CommandRenameUpdateWtPart { get => new RelayCommand(() => ExecuteRenameWtPart()); }
        #endregion

        #region [REGION] Init
        public MassWtDocumentUpdateViewModel(IWebtermTools webtermTools,
                                             IXmlSerializeTools xmlSerializeTools,
                                             IWindchillDocumentManagementService windchillDocumentManagementService,
                                             IWindchillPartManagementService windchillPartManagementService,
                                             IWindchillDataAdminManagementService windchillDataAdminManagementService,
                                             IWindchillCredentialService windchillCredentialService,
                                             IMcgWindchillToolsManageWTObjectWindowService mcgWindchillToolsManageWTObjectWindowService,
                                             IMcgCommonLibWindowService mcgCommonLibWindowService)
        {
            try
            {
                _webtermTools = webtermTools;
                _xmlSerializeTools = xmlSerializeTools;
                _windchillDocumentManagementService = windchillDocumentManagementService;
                _windchillPartManagementService = windchillPartManagementService;
                _windchillDataAdminManagementService = windchillDataAdminManagementService;
                _windchillCredentialService = windchillCredentialService;
                _mcgWindchillToolsManageWTObjectWindowService = mcgWindchillToolsManageWTObjectWindowService;
                _mcgCommonLibWindowService = mcgCommonLibWindowService;

                CurrentDataContext = new MassWtDocumentUpdateDataContext();

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                MainDispatcher = Dispatcher.CurrentDispatcher;

                // update Webterm list
                ListWebterm = _webtermTools.GetWebtermList()?.OrderBy((item) => item.English).ToList();
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

                CurrentDataContext.SelectedLanguage = (from elem in CurrentDataContext.ListLanguage where Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpper() == elem.SAPCode select elem).FirstOrDefault();

                // Add event listeners for webterm change
                CurrentDataContext.ChangeWebtermEvent += ChangeSelectedWebtermEvent;
                CurrentDataContext.ChangeLocalWebtermEvent += ChangeSelectedLocalWebtermEvent;
                CurrentDataContext.SelectedWebterm = CurrentDataContext.ListWebterm.FirstOrDefault();

                ActionInProgressEvent += (sender, e) => CurrentDataContext.ActionInProgress = true;
                ActionDoneEvent += (sender, e) => CurrentDataContext.ActionInProgress = false;

                SearchAllContext();

                // Update QualInspGrp list
                CurrentDataContext.ListQualInspGrp.Add("X");
                CurrentDataContext.ListQualInspGrp.Add("A");
                CurrentDataContext.ListQualInspGrp.Add("B");
                CurrentDataContext.ListQualInspGrp.Add("C");

                CurrentDataContext.WtDocumentList.CollectionChanged += WtDocumentListCollectionChangedAction;
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex); ;
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecutePaste(KeyEventArgs e = null)
        {
            try
            {
                listItemFromClipboard.Clear();

                if (e == null || (Keyboard.Modifiers == ModifierKeys.Control && e != null && e.Key == Key.V))
                {
                    GetItemFromClipboard();

                    FromClipboard = true;

                    if (listItemFromClipboard != null)
                    {
                        foreach (var item in listItemFromClipboard)
                        {
                            if (item.REVISION == "#")
                                item.REVISION = "BLANK";
                            MgtWtDocumentItem currentMgtWtDocumentItem = CurrentDataContext.WtDocumentList.FirstOrDefault((doc) => doc.Number == item.NUMBER);
                            if (currentMgtWtDocumentItem != null)
                                item.UpdateMgtWtDocumentItem(currentMgtWtDocumentItem);
                            else
                            {
                                currentMgtWtDocumentItem = item.GetMgtWtDocumentItem();
                                currentMgtWtDocumentItem.WindchillPartType = CurrentDataContext.ListWindchillPartType.FirstOrDefault();
                                CurrentDataContext.WtDocumentList.Add(currentMgtWtDocumentItem);
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

        private void ExecuteDragAndDrop(DragEventArgs e = null)
        {
            try
            {
                if (!IsSingleWtDocumentDragDropInProgress)
                    if (e != null && e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        FromClipboard = false;
                        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                        AddWtDocumentFromDragAndDrop(files);
                    }
                IsSingleWtDocumentDragDropInProgress = false;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckUncheckAll(bool IsChecked)
        {
            try
            {
                if (CurrentDataContext.WtDocumentList != null)
                    foreach (var item in CurrentDataContext.WtDocumentList)
                    {
                        item.IsSelected = IsChecked;
                    }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDragAndDropWtDocument(object obj)
        {
            try
            {
                if (obj != null && obj.GetType() == typeof(object[]))
                {
                    object[] tabobj = (object[])obj;
                    if (tabobj.Length > 1)
                    {
                        var sender = tabobj[0];
                        var e = tabobj[1];

                        if (sender.GetType() == typeof(MgtWtDocumentItem) && e.GetType() == typeof(DragEventArgs))
                        {
                            MgtWtDocumentItem MgtDoc = (MgtWtDocumentItem)sender;
                            DragEventArgs dragEvent = (DragEventArgs)e;
                            if (dragEvent != null && dragEvent.Data != null && dragEvent.Data.GetDataPresent(DataFormats.FileDrop))
                            {
                                string[] files = (string[])dragEvent.Data.GetData(DataFormats.FileDrop);
                                AddFilesToWtDocument(MgtDoc, files);
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

        private void ExecuteAddWtDocument()
        {
            try
            {
                var returnWindow = _mcgWindchillToolsManageWTObjectWindowService.ShowDialogCreateWtDocumentMainView(CurrentDataContext.ListWindchillDocumentType.ToList(), CurrentDataContext.ListWindchillPartType.ToList(), ListWebterm, CurrentDataContext.SelectedLanguage, CurrentDataContext.WindchillContextList.ToList(), CurrentDataContext.ListGroup.ToList(), CurrentDataContext.ListBrand.ToList());
                if (returnWindow.DialogResult.Value && CurrentDataContext.WtDocumentList.FirstOrDefault((item) => item.Number == returnWindow.WtDocItem.Number) == null)
                    CurrentDataContext.WtDocumentList.Add(returnWindow.WtDocItem);
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
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("MWT_StatusBarSearchDocInProgress");
                Thread CurrentThread = new Thread(() => SearchWtdocumentAsynch());
                CurrentThread.IsBackground = true;
                CurrentThread.Start();
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCheckPart()
        {
            try
            {
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("MWT_StatusBarSearchPartInProgress");
                Thread CurrentThread = new Thread(() => SearchPartAsynch());
                CurrentThread.IsBackground = true;
                CurrentThread.Start();
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemoveWtDocument()
        {
            try
            {
                if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgRemoveItem"), McgWpfTools.GetStringResource("MWT_MsgTitleRemoveItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var ListToRemove = CurrentDataContext.WtDocumentList.Where((item) => item.IsSelected).ToList();
                    foreach (var doc in ListToRemove)
                        CurrentDataContext.WtDocumentList.Remove(doc);
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteApplyWtDocumentType()
        {
            try
            {
                foreach (var doc in CurrentDataContext.WtDocumentList.Where((item) => item.IsSelected))
                    doc.WindchillDocumentType = CurrentDataContext.SelectedWindchillDocumentType;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteApplyWtPartType()
        {
            try
            {
                foreach (var doc in CurrentDataContext.WtDocumentList.Where((item) => item.IsSelected))
                    doc.WindchillPartType = CurrentDataContext.SelectedWindchillPartType;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteApplyContext()
        {
            try
            {
                var returnWindow = _mcgCommonLibWindowService.ShowDialogMcgWindchillContextSelection(CurrentDataContext.WindchillContextList);

                if (returnWindow.DialogValue == MessageBoxResult.OK)
                {
                    WindchillContext SelectedContext = returnWindow.SelectedContext.Clone();


                    foreach (var doc in CurrentDataContext.WtDocumentList.Where((item) => item.IsSelected))
                    {
                        doc.WtPartObject.SelectedWindchillContext = SelectedContext;
                        doc.WtDocumentObject.SelectedWindchillContext = SelectedContext;
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreateUpdateWtDocument()
        {
            try
            {
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("MWT_StatusBarUpdateInProgress");
                Thread CurrentThread = new Thread(() => CreateUpdateWtDocumentAsynch());
                CurrentThread.IsBackground = true;
                CurrentThread.Start();
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
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("MWT_StatusBarUpdateInProgress");
                Thread CurrentThread = new Thread(() => CreateUpdateWtPartAsynch());
                CurrentThread.IsBackground = true;
                CurrentThread.Start();
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreateUpdateLink()
        {
            try
            {
                if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateLink"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdate"), MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    CheckWindchillCredential();
                    RaiseActionInProgressEvent();
                    CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("MWT_StatusBarUpdateInProgress");
                    Thread CurrentThread = new Thread(() => CreateUpdateLinkWtPartWtDocumentAsynch());
                    CurrentThread.IsBackground = true;
                    CurrentThread.Start();
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
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("MWT_UserGuide"));
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteReviseItem(object obj)
        {
            try
            {
                MgtWtDocumentItem CurrentItem = null;
                if (obj != null && obj.GetType() == typeof(MgtWtDocumentItem))
                {
                    CurrentItem = (MgtWtDocumentItem)obj;
                    if (CurrentItem.PartSearchDone && CurrentItem.WtDocumentSearchDone)
                    {
                        if (CurrentItem.WtDocumentFound || CurrentItem.PartFound)
                        {
                            McgRevisionSchemaEnum NewRev = (McgRevisionSchemaEnum)Math.Max((sbyte)CurrentItem.LastPartRevision.GetValueOrDefault(), (sbyte)CurrentItem.LastWtDocumentRevision.GetValueOrDefault()) + 1;
                            CurrentItem.Revision = NewRev;
                            CurrentItem.PartSearchDone = true;
                            CurrentItem.WtDocumentSearchDone = true;
                            CurrentItem.IsNewRevision = false;
                            CurrentItem.PartRevisionFound = false;
                            CurrentItem.WtDocumentRevisionFound = false;
                            CurrentItem.RequiredActionWtDocument = MgtRequiredActionEnum.REVISE;
                            CurrentItem.RequiredActionPart = MgtRequiredActionEnum.REVISE;
                        }
                    }
                    else
                    {
                        MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgSearchNotDone"), McgWpfTools.GetStringResource("MWT_MsgTitleSearchNotDone"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRemoveItem(object obj)
        {
            try
            {
                if (obj != null && obj.GetType() == typeof(MgtWtDocumentItem))
                    if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgRemoveItem"), McgWpfTools.GetStringResource("MWT_MsgTitleRemoveItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        CurrentDataContext.WtDocumentList.Remove((MgtWtDocumentItem)obj);
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDragAndDropXls(DragEventArgs e = null)
        {
            try
            {
                if (!IsSingleWtDocumentDragDropInProgress)
                    if (e != null && e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        FromClipboard = false;
                        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                        string xlsFile = null;
                        if (files != null && files.Length > 0)
                        {
                            xlsFile = files.FirstOrDefault();
                            McgLinkToExcel currentMcgLinkToExcel = new McgLinkToExcel(xlsFile);
                            List<MgtWtObject> CurrentListItem = currentMcgLinkToExcel.Read<MgtWtObject>("WTOBJECT");
                            AddUpdateItemFromXls(CurrentListItem);
                        }
                    }
                IsSingleWtDocumentDragDropInProgress = false;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDragAndDropSecondaryContent(DragEventArgs e = null)
        {
            try
            {
                if (!IsSingleWtDocumentDragDropInProgress)
                    if (e != null && e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                        AddWtDocumentFromDragAndDropSecondary(files);
                    }
                IsSingleWtDocumentDragDropInProgress = false;
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
                    if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgRemoveItem"), McgWpfTools.GetStringResource("MWT_MsgTitleRemoveItem"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        item.ParentWtDocument.ListContentItem.Remove(item);
                        if (item.IsPrimaryContent)
                        {
                            MgtContentItem newItem = item.ParentWtDocument.ListContentItem.FirstOrDefault();
                            if (newItem != null)
                                newItem.IsPrimaryContent = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteChangeContext(object obj)
        {
            try
            {
                MgtWtDocumentItem CurrentItem = null;
                if (obj != null && obj.GetType() == typeof(MgtWtDocumentItem))
                {
                    CurrentItem = (MgtWtDocumentItem)obj;

                    var returnWindow = _mcgCommonLibWindowService.ShowDialogMcgWindchillContextSelection(CurrentDataContext.WindchillContextList, CurrentDataContext.WindchillContextList.FirstOrDefault((item) => item.Name == CurrentItem.WtDocumentObject?.SelectedWindchillContext?.Name));
                    if (returnWindow.DialogValue == MessageBoxResult.OK)
                    {
                        WindchillContext SelectedContext = returnWindow.SelectedContext.Clone();
                        CurrentItem.WtDocumentObject.SelectedWindchillContext = SelectedContext;
                        CurrentItem.WtPartObject.SelectedWindchillContext = SelectedContext;
                        CurrentItem.WtDocumentObject.SelectedWindchillContext.Folder = SelectedContext.OdataFolder.Name;
                        CurrentItem.WtPartObject.SelectedWindchillContext.Folder = SelectedContext.OdataFolder.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteApplyWebterm()
        {
            try
            {
                if (CurrentDataContext.SelectedWebterm != null && CurrentDataContext.SelectedWebterm.Trim() != "")
                    foreach (var doc in CurrentDataContext.WtDocumentList.Where((item) => item.IsSelected))
                    {
                        doc.WtDocumentObject.Description21ChangeEvent -= Description21ChangeEventAction;
                        doc.WtPartObject.Description21ChangeEvent -= Description21ChangeEventAction;
                        doc.WtDocumentObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;
                        doc.WtPartObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;
                        doc.WtDocumentObject.PTCCOMMONNAME = CurrentDataContext.SelectedWebterm;
                        doc.WtPartObject.PTCCOMMONNAME = CurrentDataContext.SelectedWebterm;
                        doc.WtDocumentObject.DESCRIPTION21 = CurrentDataContext.SelectedLocalWebterm;
                        doc.WtPartObject.DESCRIPTION21 = CurrentDataContext.SelectedLocalWebterm;
                        doc.WtDocumentObject.Description21ChangeEvent += Description21ChangeEventAction;
                        doc.WtPartObject.Description21ChangeEvent += Description21ChangeEventAction;
                        doc.WtDocumentObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                        doc.WtPartObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                    }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRenameWtDocument()
        {
            try
            {
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("MWT_StatusBarUpdateInProgress");
                Thread CurrentThread = new Thread(() => RenameWtDocumentAsynch());
                CurrentThread.IsBackground = true;
                CurrentThread.Start();

            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteRenameWtPart()
        {
            try
            {
                CheckWindchillCredential();
                RaiseActionInProgressEvent();
                CurrentDataContext.StatusBarTextRight = McgWpfTools.GetStringResource("MWT_StatusBarUpdateInProgress");
                Thread CurrentThread = new Thread(() => RenameWtPartAsynch());
                CurrentThread.IsBackground = true;
                CurrentThread.Start();
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
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

        private void SearchWtdocumentAsynch()
        {
            try
            {

                string CurrentRev = null;
                RestOdataWtDocument LastDocument = null;
                RestOdataWtDocument CurrentDocument = null;
                WindchillContext WtDocContext = null;
                CurrentDataContext.CurrentStep = 0;
                CurrentDataContext.TotalStep = CurrentDataContext.WtDocumentList.Where((item) => !item.WtDocumentSearchDone).Count();
                foreach (var doc in CurrentDataContext.WtDocumentList.Where((item) => !item.WtDocumentSearchDone))
                {
                    CurrentDataContext.CurrentStep++;
                    doc.WtDocumentFound = false;
                    doc.WtDocumentRevisionFound = false;

                    if (doc.Revision == McgRevisionSchemaEnum.BLANK)
                        CurrentRev = "#";
                    else
                        CurrentRev = doc.Revision.ToString();

                    LastDocument = _windchillDocumentManagementService.GetOneWtDocument(WindchillCredential.WindchillCredential, doc.Number, "Latest", CommonLibConstants.WindchillUrl);
                    if (LastDocument != null && LastDocument.Number != null)
                    {
                        doc.WtDocumentFound = true;
                        WtDocContext = new WindchillContext(_windchillDataAdminManagementService.GetOneContextFoldersLvX(WindchillCredential.WindchillCredential, LastDocument.FolderLocation?.Substring(1).Split('/').FirstOrDefault(), 1, CommonLibConstants.WindchillUrl));
                        WtDocContext.Folder = LastDocument.FolderName;
                        doc.RequiredActionWtDocument = MgtRequiredActionEnum.REVISE;

                        if (LastDocument.Revision == "#") LastDocument.Revision = "BLANK";
                        doc.LastWtDocumentRevision = McgReflectionTools.GetEnumValue<McgRevisionSchemaEnum>(LastDocument.Revision);
                        doc.WindchillWtDocument = LastDocument;
                        if (!FromClipboard)
                            doc.WtDocumentObject = MgtWtObject.CreateMgtWtObject(LastDocument);
                        doc.WtDocumentObject.ParentDocument = doc;
                        doc.WtDocumentObject.SelectedWindchillContext = WtDocContext;
                        doc.WtDocumentObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                        doc.WtDocumentObject.Description21ChangeEvent += Description21ChangeEventAction;
                        //doc.UpdateDocumentType(McgMiscTools.GetEnumValue<WindchillWtDocumentOdataTypeEnum>(LastDocument.OdataType?.Replace('#', ' ').Replace('.', '_').Trim()));
                        doc.UpdateDocumentType(McgReflectionTools.GetEnumValue<WindchillWtDocumentOdataTypeEnum>(LastDocument.OdataType?.Replace('#', ' ').Split('.').LastOrDefault()?.Trim()));

                        if (LastDocument.Revision == CurrentRev)
                        {
                            doc.WtDocumentRevisionFound = true;
                            doc.RequiredActionWtDocument = MgtRequiredActionEnum.UPDATE;
                        }
                        else
                        {
                            CurrentDocument = _windchillDocumentManagementService.GetOneWtDocument(WindchillCredential.WindchillCredential, doc.Number, CurrentRev, CommonLibConstants.WindchillUrl);
                            if (CurrentDocument != null && CurrentDocument.Number != null)
                            {
                                doc.WtDocumentRevisionFound = true;
                                doc.WindchillWtDocument = CurrentDocument;
                                if (!FromClipboard)
                                    doc.WtDocumentObject = MgtWtObject.CreateMgtWtObject(CurrentDocument);
                                doc.WtDocumentObject.ParentDocument = doc;
                                doc.WtDocumentObject.SelectedWindchillContext = WtDocContext;
                                doc.WtDocumentObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                                doc.WtDocumentObject.Description21ChangeEvent += Description21ChangeEventAction;

                                doc.RequiredActionWtDocument = MgtRequiredActionEnum.UPDATE;
                            }

                        }
                    }
                    else
                        doc.RequiredActionWtDocument = MgtRequiredActionEnum.CREATE;

                    SearchPartLinkedDocument(doc);
                    doc.WtDocumentSearchDone = true;
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

        private void SearchPartAsynch()
        {
            try
            {
                string CurrentRev = null;
                RestOdataWtPart LastPart = null;
                RestOdataWtPart CurrentPart = null;
                WindchillContext PartContext = null;
                CurrentDataContext.CurrentStep = 0;
                CurrentDataContext.TotalStep = CurrentDataContext.WtDocumentList.Where((item) => !item.PartSearchDone).Count();
                foreach (var doc in CurrentDataContext.WtDocumentList.Where((item) => !item.PartSearchDone))
                {
                    CurrentDataContext.CurrentStep++;
                    doc.PartFound = false;
                    doc.PartRevisionFound = false;

                    if (doc.Revision == McgRevisionSchemaEnum.BLANK)
                        CurrentRev = "#";
                    else
                        CurrentRev = doc.Revision.ToString();

                    LastPart = _windchillPartManagementService.GetOnePart(WindchillCredential.WindchillCredential, doc.Number, "Latest", CommonLibConstants.WindchillUrl);
                    if (LastPart != null && LastPart.Number != null)
                    {
                        doc.PartFound = true;
                        doc.RequiredActionPart = MgtRequiredActionEnum.REVISE;
                        PartContext = new WindchillContext(_windchillDataAdminManagementService.GetOneContextFoldersLvX(WindchillCredential.WindchillCredential, LastPart.FolderLocation?.Substring(1).Split('/').FirstOrDefault(), 1, CommonLibConstants.WindchillUrl));
                        PartContext.Folder = LastPart.FolderName;
                        if (LastPart.Revision == "#") LastPart.Revision = "BLANK";
                        doc.LastPartRevision = McgReflectionTools.GetEnumValue<McgRevisionSchemaEnum>(LastPart.Revision);

                        doc.WindchillWtPart = LastPart;
                        if (!FromClipboard)
                            doc.WtPartObject = MgtWtObject.CreateMgtWtObject(LastPart);
                        doc.WtPartObject.ParentDocument = doc;
                        doc.WtPartObject.SelectedWindchillContext = PartContext;
                        doc.WtPartObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                        doc.WtPartObject.Description21ChangeEvent += Description21ChangeEventAction;
                        doc.UpdatePartType(McgReflectionTools.GetEnumValue<WindchillWtPartOdataTypeEnum>(LastPart.OdataType?.Replace('#', ' ').Replace('.', '_').Trim()));

                        if (LastPart.Revision == CurrentRev)
                        {
                            doc.PartRevisionFound = true;
                            doc.RequiredActionPart = MgtRequiredActionEnum.UPDATE;
                        }
                        else
                        {
                            CurrentPart = _windchillPartManagementService.GetOnePart(WindchillCredential.WindchillCredential, doc.Number, CurrentRev, CommonLibConstants.WindchillUrl);
                            if (CurrentPart != null && CurrentPart.Number != null)
                            {
                                doc.PartRevisionFound = true;
                                doc.WindchillWtPart = CurrentPart;
                                if (!FromClipboard)
                                    doc.WtPartObject = MgtWtObject.CreateMgtWtObject(CurrentPart);
                                doc.WtPartObject.ParentDocument = doc;
                                doc.WtPartObject.SelectedWindchillContext = PartContext;
                                doc.WtPartObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                                doc.WtPartObject.Description21ChangeEvent += Description21ChangeEventAction;

                                doc.RequiredActionPart = MgtRequiredActionEnum.UPDATE;
                            }
                        }
                    }
                    else
                        doc.RequiredActionPart = MgtRequiredActionEnum.CREATE;

                    SearchPartLinkedDocument(doc);

                    doc.PartSearchDone = true;
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

        private void SearchPartLinkedDocument(MgtWtDocumentItem CurrentWtObject)
        {
            try
            {
                if (CurrentWtObject.WtDocumentFound
                    && CurrentWtObject.PartFound)
                {
                    CurrentWtObject.LinkStatus = ObjectState.UNLINKED;
                    RestOdataWtPart LastPart = null;
                    LastPart = _windchillPartManagementService.GetPartWtDocumentDescribedBy(WindchillCredential.WindchillCredential, CurrentWtObject.WindchillWtPart, CommonLibConstants.WindchillUrl);
                    if (LastPart != null && LastPart.Number != null)
                    {
                        if (LastPart.DescribedBy.Any(o => o.DescribedBy.Number == CurrentWtObject.WindchillWtDocument.Number && o.DescribedBy.Revision == CurrentWtObject.WindchillWtDocument.Revision))
                            CurrentWtObject.LinkStatus = ObjectState.LINKED;
                    }
                    else
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                //RaiseActionDoneEvent();
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

        private void CreateUpdateWtDocumentAsynch()
        {
            try
            {
                var ListItems = CurrentDataContext.WtDocumentList.Where((item) => item.IsSelected).ToList();
                if (ListItems != null && ListItems.Count > 0)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateDocument"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdate"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {

                        CurrentDataContext.CurrentStep = 0;
                        CurrentDataContext.TotalStep = ListItems.Count;

                        foreach (var item in ListItems)
                        {
                            CurrentDataContext.CurrentStep++;
                            // throw new Exception();

                            if (item.WtDocumentSearchDone)
                            {
                                if (item.RequiredActionWtDocument == MgtRequiredActionEnum.CREATE)
                                {
                                    if (item.WtDocumentObject.SelectedWindchillContext == null ||
                                        item.WtDocumentObject.SelectedWindchillContext.OdataContext == null ||
                                        item.WtDocumentObject.SelectedWindchillContext.OdataFolder == null)
                                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateMissingContext"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdate"), MessageBoxButton.OK, MessageBoxImage.Warning);
                                    else if (item.WtDocumentOdataType == WindchillWtDocumentOdataTypeEnum.UNKNOWN)
                                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateMissingDocType"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdate"), MessageBoxButton.OK, MessageBoxImage.Warning);
                                    else if (item.WtDocumentObject.PTCCOMMONNAME == null ||
                                             item.WtDocumentObject.PTCCOMMONNAME.Trim() == "")
                                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateMissingPtcCommonName"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdate"), MessageBoxButton.OK, MessageBoxImage.Warning);
                                    else
                                    {
                                        if (CreateWtDocument(item))
                                        {
                                            item.StatusWtDocument = "Doc Created";
                                            if (CheckOutWtDocument(item))
                                            {
                                                item.StatusWtDocument = $"{item.StatusWtDocument} - Doc Checked out";
                                                if (UpdateWtDocument(item))
                                                {
                                                    item.StatusWtDocument = $"{item.StatusWtDocument} - Doc Updated";
                                                    if (UpdateContentWtDocument(item))
                                                    {
                                                        item.StatusWtDocument = $"{item.StatusWtDocument} - Content Updated";
                                                        if (CheckInWtDocument(item))
                                                        {
                                                            item.StatusWtDocument = $"{item.StatusWtDocument} - Checked in";
                                                            item.RequiredActionWtDocument = MgtRequiredActionEnum.UPDATE;
                                                        }
                                                        else
                                                        {
                                                            item.StatusWtDocument = $"{item.StatusWtDocument} - Check in issue";
                                                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckInNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        item.StatusWtDocument = $"{item.StatusWtDocument} - Content Update issue";
                                                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateContentNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateContentNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                                    }
                                                }
                                                else
                                                {
                                                    item.StatusWtDocument = $"{item.StatusWtDocument} - Update issue";
                                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                                }
                                            }
                                            else
                                            {
                                                item.StatusWtDocument = $"{item.StatusWtDocument} - Check out issue";
                                                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckOutNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                            }
                                        }
                                        else
                                        {
                                            item.StatusWtDocument = $"Create issue";
                                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCreateNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCreateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                }
                                else if (item.RequiredActionWtDocument == MgtRequiredActionEnum.REVISE)
                                {
                                    if (item.WindchillWtDocument != null && item.Revision > McgReflectionTools.GetEnumValue<McgRevisionSchemaEnum>(item.WindchillWtDocument.Revision))
                                        if (ReviseWtDocument(item) != null)
                                        {
                                            item.StatusWtDocument = "Doc Revised";
                                            if (CheckOutWtDocument(item))
                                            {
                                                item.StatusWtDocument = $"{item.StatusWtDocument} - Checked out";
                                                if (UpdateWtDocument(item))
                                                {
                                                    item.StatusWtDocument = $"{item.StatusWtDocument} - Doc Updated";
                                                    if (UpdateContentWtDocument(item))
                                                    {
                                                        item.StatusWtDocument = $"{item.StatusWtDocument} - Content Updated";
                                                        if (CheckInWtDocument(item))
                                                        {
                                                            item.StatusWtDocument = $"{item.StatusWtDocument} - Checked in";
                                                            item.RequiredActionWtDocument = MgtRequiredActionEnum.UPDATE;
                                                        }
                                                        else
                                                        {
                                                            item.StatusWtDocument = $"{item.StatusWtDocument} - Check in issue";
                                                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckInNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        item.StatusWtDocument = $"{item.StatusWtDocument} - Content Update issue";
                                                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateContentNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateContentNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                                    }
                                                }
                                                else
                                                {
                                                    item.StatusWtDocument = $"{item.StatusWtDocument} - Update issue";
                                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                                }
                                            }
                                            else
                                            {
                                                item.StatusWtDocument = $"{item.StatusWtDocument} - Check Out issue";
                                                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckOutNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                            }
                                        }
                                        else
                                        {
                                            item.StatusWtDocument = $"{item.StatusWtDocument} - Revise issue";
                                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgReviseNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleReviseNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    else
                                    {
                                        item.StatusWtDocument = "Wrong revision";
                                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgReviseNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleReviseNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                                else if (item.RequiredActionWtDocument == MgtRequiredActionEnum.UPDATE)
                                {
                                    if (CheckOutWtDocument(item))
                                    {
                                        item.StatusWtDocument = "Checked out";
                                        if (UpdateWtDocument(item))
                                        {
                                            item.StatusWtDocument = $"{item.StatusWtDocument} - Doc Updated";
                                            if (UpdateContentWtDocument(item))
                                            {
                                                item.StatusWtDocument = $"{item.StatusWtDocument} - Content Updated";
                                                if (CheckInWtDocument(item))
                                                {
                                                    item.StatusWtDocument = $"{item.StatusWtDocument} - Checked in";
                                                }
                                                else
                                                {
                                                    item.StatusWtDocument = $"{item.StatusWtDocument} - Check in issue";
                                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckInNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                                }
                                            }
                                            else
                                            {
                                                item.StatusWtDocument = $"{item.StatusWtDocument} - Content Update issue";
                                                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateContentNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateContentNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                            }
                                        }
                                        else
                                        {
                                            item.StatusWtDocument = $"{item.StatusWtDocument} - Update issue";
                                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                    else
                                    {
                                        item.StatusWtDocument = "Check out issue";
                                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckOutNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgSearchNotDoneWtDoc"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleSearchNotDone"), MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateNoSelection"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdate"), MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void CreateUpdateWtPartAsynch()
        {
            try
            {
                var ListItems = CurrentDataContext.WtDocumentList.Where((item) => item.IsSelected).ToList();
                if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgCreateUpdatePart"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdate"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    CurrentDataContext.CurrentStep = 0;
                    CurrentDataContext.TotalStep = ListItems.Count;

                    foreach (var item in ListItems)
                    {
                        CurrentDataContext.CurrentStep++;
                        if (item.PartSearchDone)
                        {
                            if (item.RequiredActionPart == MgtRequiredActionEnum.CREATE)
                            {
                                if (CreateWtPart(item))
                                {
                                    item.StatusPart = $"Created";
                                    item.RequiredActionPart = MgtRequiredActionEnum.UPDATE;
                                }
                                else
                                {
                                    item.StatusPart = $"Create issue";
                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCreateNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCreateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else if (item.RequiredActionPart == MgtRequiredActionEnum.REVISE)
                            {

                                if (ReviseWtPart(item) != null)
                                {
                                    item.StatusPart = $"Revised";
                                    if (CheckOutWtPart(item))
                                    {
                                        item.StatusPart = $"{item.StatusPart} - Checked out";
                                        if (UpdateWtPart(item))
                                        {
                                            item.StatusPart = $"{item.StatusPart} - Updated";
                                            if (CheckInWtPart(item))
                                            {
                                                item.StatusPart = $"{item.StatusPart} - Checked in";
                                                if (UpdateWtPartCommonProperties(item))
                                                {
                                                    item.StatusPart = $"{item.StatusPart} - Unit Update in";
                                                    item.RequiredActionPart = MgtRequiredActionEnum.UPDATE;
                                                }
                                                else
                                                {
                                                    item.StatusPart = $"{item.StatusPart} - Unit Update issue";
                                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckInNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                                }
                                            }
                                            else
                                            {
                                                item.StatusPart = $"{item.StatusPart} - Check in issue";
                                                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckInNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                            }
                                        }
                                        else
                                        {
                                            item.StatusPart = $"{item.StatusPart} - Update issue";
                                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                    else
                                    {
                                        item.StatusPart = $"{item.StatusPart} - Check out issue";
                                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckOutNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                                else
                                {
                                    item.StatusPart = $"Create issue";
                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCreateNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCreateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                }

                            }
                            else if (item.RequiredActionPart == MgtRequiredActionEnum.UPDATE)
                            {
                                if (CheckOutWtPart(item))
                                {
                                    item.StatusPart = $"Checked out";
                                    if (UpdateWtPart(item))
                                    {
                                        item.StatusPart = $"{item.StatusPart} - Updated";
                                        if (CheckInWtPart(item))
                                        {
                                            item.StatusPart = $"{item.StatusPart} - Checked in";
                                            if (UpdateWtPartCommonProperties(item))
                                            {
                                                item.StatusPart = $"{item.StatusPart} - Unit Update in";
                                                item.RequiredActionPart = MgtRequiredActionEnum.UPDATE;
                                            }
                                            else
                                            {
                                                item.StatusPart = $"{item.StatusPart} - Unit Update issue";
                                                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckInNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                            }
                                        }
                                        else
                                        {
                                            item.StatusPart = $"{item.StatusPart} - Check in issue";
                                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckInNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                    else
                                    {
                                        item.StatusPart = $"{item.StatusPart} - Update issue";
                                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgUpdateNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleUpdateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                                else
                                {
                                    item.StatusPart = $"Check out issue";
                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckOutNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgSearchNotDoneWtPart"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleSearchNotDone"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
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

        private void CreateUpdateLinkWtPartWtDocumentAsynch()
        {
            try
            {
                var ListItems = CurrentDataContext.WtDocumentList.Where((item) => item.IsSelected && item.LinkStatus == ObjectState.UNLINKED).ToList();
                CurrentDataContext.CurrentStep = 0;
                CurrentDataContext.TotalStep = ListItems.Count;

                foreach (var CurrentWtObject in ListItems)
                {

                    CurrentDataContext.CurrentStep++;
                    if (CheckOutWtPart(CurrentWtObject))
                    {
                        CurrentWtObject.StatusPart = $"Checked out";
                        RestOdataWtPart LastPart = null;
                        LastPart = _windchillPartManagementService.LinkPartWtDocumentDescribedBy(WindchillCredential.WindchillCredential,
                                                                                        CurrentWtObject.WindchillWtPart,
                                                                                        CurrentWtObject.WindchillWtDocument,
                                                                                        CommonLibConstants.WindchillUrl);
                        if (LastPart != null)
                        {
                            if (CheckInWtPart(CurrentWtObject))
                            {
                                CurrentWtObject.StatusPart = $"{CurrentWtObject.StatusPart} - Checked in";
                                CurrentWtObject.LinkStatus = ObjectState.LINKED;
                            }
                            else
                            {
                                CurrentWtObject.StatusPart = $"{CurrentWtObject.StatusPart} - Check in issue";
                            }
                        }
                        else
                        {
                            CurrentWtObject.StatusPart = $"{CurrentWtObject.StatusPart} - link issue";
                        }
                    }
                    else
                    {
                        CurrentWtObject.StatusPart = $"Check out issue";
                    }
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

        private bool CreateWtDocument(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                CheckWindchillCredential();
                RestOdataWtDocument NewDocument = new RestOdataWtDocument()
                {
                    Name = WtDocItem.WtDocumentObject.PTCCOMMONNAME,
                    Number = WtDocItem.Number,
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
                    MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateNotDone"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdateNotDone"), MessageBoxButton.YesNo, MessageBoxImage.Error);
                    return false;
                }
                bool ReviseOk = true;
                WtDocItem.WindchillWtDocument = NewDocument;
                if (NewDocument.Revision.Replace("#", "BLANK") != WtDocItem.Revision.ToString())
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
                                                            WtDocItem.Revision.ToString(),
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
                List<RestOdataReplicaContentFileItem> Files = new List<RestOdataReplicaContentFileItem>();

                // If no content to upload return true
                if (WtDocItem.ListContentItem == null || WtDocItem.ListContentItem.Count == 0)
                    return true;

                foreach (var file in WtDocItem.ListContentItem)
                {
                    Files.Add(new RestOdataReplicaContentFileItem()
                    {
                        CompleteFileName = file.CompleteFilename,
                        FileName = file.Filename,
                        PrimaryContent = file.IsPrimaryContent
                    });
                }

                RestOdataWtDocument NewDocument = _windchillDocumentManagementService.UploadContentWtDocument(WindchillCredential.WindchillCredential,
                                                            WtDocItem.WindchillWtDocument,
                                                            Files,
                                                            CommonLibConstants.WindchillUrl);
                if (NewDocument != null)
                {
                    WtDocItem.WindchillWtDocument = NewDocument;
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
                WtDocItem.WindchillWtPart.GROUP = WtDocItem.WtPartObject.GROUP;
                WtDocItem.WindchillWtPart.SUB_GROUP = WtDocItem.WtPartObject.SUB_GROUP;
                WtDocItem.WindchillWtPart.MODEL = WtDocItem.WtPartObject.MODEL;
                WtDocItem.WindchillWtPart.BRAND = WtDocItem.WtPartObject.BRAND;
                WtDocItem.WindchillWtPart.OPTION = WtDocItem.WtPartObject.OPTION;

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

        private bool CreateWtPart(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                CheckWindchillCredential();
                RestOdataWtPart NewPart = new RestOdataWtPart()
                {
                    Name = WtDocItem.WtPartObject.PTCCOMMONNAME?.Trim().ToUpper(),
                    Number = WtDocItem.Number?.Trim().ToUpper(),
                    DESCRIPTION_2 = WtDocItem.WtPartObject.DESCRIPTION2?.Trim().ToUpper(),
                    DESCRIPTION2_2 = WtDocItem.WtPartObject.DESCRIPTION22?.Trim().ToUpper(),
                    DESCRIPTION2_1 = WtDocItem.WtPartObject.DESCRIPTION21?.Trim().ToUpper(),
                    GROUP_CREATOR = WtDocItem.WtPartObject.GROUPCREATOR?.Trim().ToUpper(),
                    QUALINSPGRP = WtDocItem.WtPartObject.QUALINSPGRP?.Trim().ToUpper(),
                    MASS = WtDocItem.WtPartObject.MASS,
                    OdataType = $"#{WtDocItem.WtPartOdataType.ToString().Replace('_', '.')}"
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
                    if (NewPart.Revision.Replace("#", "BLANK") != WtDocItem.Revision.ToString())
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
                                                            WtDocItem.Revision.ToString(),
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

        private void RenameWtDocumentAsynch()
        {
            try
            {
                var ListItems = CurrentDataContext.WtDocumentList.Where((item) => item.IsSelected).ToList();
                if (ListItems != null && ListItems.Count > 0)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateDocument"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdate"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {

                        CurrentDataContext.CurrentStep = 0;
                        CurrentDataContext.TotalStep = ListItems.Count;

                        foreach (var item in ListItems)
                        {
                            CurrentDataContext.CurrentStep++;

                            if (item.WtDocumentSearchDone)
                            {
                                if (item.RequiredActionWtDocument == MgtRequiredActionEnum.UPDATE)
                                {
                                    if (RenameWtDocument(item))
                                    {
                                        item.StatusWtDocument = "Renamed";
                                    }
                                    else
                                    {
                                        item.StatusWtDocument = "Rename issue";
                                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCheckOutNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCheckOutNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgSearchNotDoneWtDoc"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleSearchNotDone"), MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgCreateUpdateNoSelection"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdate"), MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void RenameWtPartAsynch()
        {
            try
            {
                var ListItems = CurrentDataContext.WtDocumentList.Where((item) => item.IsSelected).ToList();
                if (MessageBox.Show(McgWpfTools.GetStringResource("MWT_MsgCreateUpdatePart"), McgWpfTools.GetStringResource("MWT_MsgTitleCreateUpdate"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    CurrentDataContext.CurrentStep = 0;
                    CurrentDataContext.TotalStep = ListItems.Count;

                    foreach (var item in ListItems)
                    {
                        CurrentDataContext.CurrentStep++;
                        if (item.PartSearchDone)
                        {
                            if (item.RequiredActionPart == MgtRequiredActionEnum.UPDATE)
                            {
                                if (RenameWtPart(item))
                                {
                                    item.StatusPart = $"Renamed";
                                    item.RequiredActionPart = MgtRequiredActionEnum.UPDATE;
                                }
                                else
                                {
                                    item.StatusPart = $"Rename issue";
                                    MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgCreateNotPossible"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleCreateNotPossible"), MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(string.Format(McgWpfTools.GetStringResource("MWT_MsgSearchNotDoneWtPart"), item.Number), McgWpfTools.GetStringResource("MWT_MsgTitleSearchNotDone"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
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

        private bool RenameWtPart(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                CheckWindchillCredential();
                WtDocItem.WindchillWtPart.Name = WtDocItem.WtPartObject.PTCCOMMONNAME?.Trim().ToUpper();

                RestOdataWtPart NewPart = _windchillPartManagementService.RenamePartCommonProperties(WindchillCredential.WindchillCredential,
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

        private bool RenameWtDocument(MgtWtDocumentItem WtDocItem)
        {
            try
            {
                CheckWindchillCredential();

                WtDocItem.WindchillWtDocument.Name = WtDocItem.WtDocumentObject.PTCCOMMONNAME?.Trim().ToUpper();

                RestOdataWtDocument NewDocument = _windchillDocumentManagementService.RenameWtDocument(WindchillCredential.WindchillCredential,
                                                                WtDocItem.WindchillWtDocument,
                                                                CommonLibConstants.WindchillUrl);
                if (NewDocument != null)
                {
                    WtDocItem.WindchillWtDocument = NewDocument;
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
        #endregion

        #region [REGION] Misc Methods
        private void AddWtDocumentFromDragAndDrop(string[] Files)
        {
            try
            {
                if (Files != null)
                {
                    MgtWtDocumentItem CurrentDocItem = null;
                    MgtWtDocumentItem AlreadyDocItem = null;
                    foreach (var item in Files)
                    {
                        CurrentDocItem = new MgtWtDocumentItem();
                        CurrentDocItem.UpdateMainInformation(item);


                        AlreadyDocItem = CurrentDataContext.WtDocumentList.FirstOrDefault((file) => file.Number == CurrentDocItem.Number
                                                                                                 && file.Revision == CurrentDocItem.Revision);
                        if (AlreadyDocItem == null)
                        {
                            CurrentDocItem.ListContentItem.Add(new MgtContentItem()
                            {
                                CompleteFilename = item,
                                ContentType = WindchillContentType.PRIMARY_CONTENT,
                                IsPrimaryContent = true,
                                ItemId = $"{CurrentDocItem.GetHashCode()}",
                                ParentWtDocument = CurrentDocItem
                            });
                            CurrentDocItem.WtDocumentObject = new MgtWtObject() { ParentDocument = CurrentDocItem };
                            CurrentDocItem.WtPartObject = new MgtWtObject() { ParentDocument = CurrentDocItem };
                            CurrentDataContext.WtDocumentList.Add(CurrentDocItem);
                            CurrentDocItem.WindchillPartType = CurrentDataContext.ListWindchillPartType.FirstOrDefault();
                        }
                        else
                        {
                            if (AlreadyDocItem.ListContentItem.FirstOrDefault((file) => file.CompleteFilename == item) == null)
                                if (AlreadyDocItem.ListContentItem.Count == 0)
                                    AlreadyDocItem.ListContentItem.Add(new MgtContentItem()
                                    {
                                        CompleteFilename = item,
                                        ContentType = WindchillContentType.PRIMARY_CONTENT,
                                        IsPrimaryContent = true,
                                        ItemId = $"{AlreadyDocItem.GetHashCode()}",
                                        ParentWtDocument = AlreadyDocItem,
                                    });
                                else
                                    AlreadyDocItem.ListContentItem.Add(new MgtContentItem()
                                    {
                                        CompleteFilename = item,
                                        ContentType = WindchillContentType.SECONDARY_CONTENT,
                                        IsPrimaryContent = false,
                                        ItemId = $"{AlreadyDocItem.GetHashCode()}",
                                        ParentWtDocument = AlreadyDocItem,
                                    });
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void AddWtDocumentFromDragAndDropSecondary(string[] Files)
        {
            try
            {
                if (Files != null)
                {
                    MgtWtDocumentItem CurrentDocItem = null;
                    MgtWtDocumentItem AlreadyDocItem = null;
                    foreach (var item in Files)
                    {
                        CurrentDocItem = new MgtWtDocumentItem();
                        CurrentDocItem.UpdateMainInformation(item);


                        AlreadyDocItem = CurrentDataContext.WtDocumentList.FirstOrDefault((file) => file.Number == CurrentDocItem.Number);
                        if (AlreadyDocItem == null)
                        {
                            CurrentDocItem.ListContentItem.Add(new MgtContentItem()
                            {
                                CompleteFilename = item,
                                ContentType = WindchillContentType.PRIMARY_CONTENT,
                                IsPrimaryContent = false,
                                ItemId = $"{CurrentDocItem.GetHashCode()}",
                                ParentWtDocument = CurrentDocItem
                            });
                            CurrentDocItem.WtDocumentObject = new MgtWtObject() { ParentDocument = CurrentDocItem };
                            CurrentDocItem.WtPartObject = new MgtWtObject() { ParentDocument = CurrentDocItem };
                            CurrentDataContext.WtDocumentList.Add(CurrentDocItem);
                            CurrentDocItem.WindchillPartType = CurrentDataContext.ListWindchillPartType.FirstOrDefault();
                        }
                        else
                        {
                            if (AlreadyDocItem.ListContentItem.FirstOrDefault((file) => file.CompleteFilename == item) == null)
                                if (AlreadyDocItem.ListContentItem.Count == 0)
                                    AlreadyDocItem.ListContentItem.Add(new MgtContentItem()
                                    {
                                        CompleteFilename = item,
                                        ContentType = WindchillContentType.SECONDARY_CONTENT,
                                        IsPrimaryContent = false,
                                        ItemId = $"{AlreadyDocItem.GetHashCode()}",
                                        ParentWtDocument = AlreadyDocItem,
                                    });
                                else
                                    AlreadyDocItem.ListContentItem.Add(new MgtContentItem()
                                    {
                                        CompleteFilename = item,
                                        ContentType = WindchillContentType.SECONDARY_CONTENT,
                                        IsPrimaryContent = false,
                                        ItemId = $"{AlreadyDocItem.GetHashCode()}",
                                        ParentWtDocument = AlreadyDocItem,
                                    });
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void AddUpdateItemFromXls(List<MgtWtObject> ListItem)
        {
            try
            {
                if (ListItem != null)
                {
                    foreach (var item in ListItem)
                    {
                        if (item.REVISION == "#")
                            item.REVISION = "BLANK";
                        MgtWtDocumentItem currentMgtWtDocumentItem = CurrentDataContext.WtDocumentList.FirstOrDefault((doc) => doc.Number == item.NUMBER);
                        if (currentMgtWtDocumentItem != null)
                            item.UpdateMgtWtDocumentItem(currentMgtWtDocumentItem);
                        else
                        {
                            currentMgtWtDocumentItem = item.GetMgtWtDocumentItem();
                            currentMgtWtDocumentItem.WindchillPartType = CurrentDataContext.ListWindchillPartType.FirstOrDefault();
                            CurrentDataContext.WtDocumentList.Add(currentMgtWtDocumentItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }

        private void AddFilesToWtDocument(MgtWtDocumentItem CurrentWtDoc, string[] Files)
        {
            try
            {
                if (Files != null && CurrentWtDoc != null && CurrentWtDoc.ListContentItem != null)
                {
                    foreach (var item in Files)
                    {
                        if (CurrentWtDoc.ListContentItem.FirstOrDefault((content) => content.CompleteFilename == item) == null)
                        {
                            CurrentWtDoc.ListContentItem.Add(new MgtContentItem()
                            {
                                CompleteFilename = item,
                                ItemId = $"{CurrentWtDoc.GetHashCode()}",
                                ParentWtDocument = CurrentWtDoc,
                                IsPrimaryContent = CurrentWtDoc.ListContentItem.Count == 0
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ManageWTObjectException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Event Methods
        private void ChangeLanguageEventAction(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void WtDocumentListCollectionChangedAction(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (sender != null)
                {
                    foreach (var CurrentItem in (ObservableCollection<MgtWtDocumentItem>)sender)
                    {
                        if (e != null & e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                        {
                            CurrentItem.WtPartObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;
                            CurrentItem.WtDocumentObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;
                            CurrentItem.WtPartObject.Description21ChangeEvent -= Description21ChangeEventAction;
                            CurrentItem.WtDocumentObject.Description21ChangeEvent -= Description21ChangeEventAction;
                        }
                        else if (e != null & e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                        {
                            CurrentItem.WtPartObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                            CurrentItem.WtDocumentObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                            CurrentItem.WtPartObject.Description21ChangeEvent += Description21ChangeEventAction;
                            CurrentItem.WtDocumentObject.Description21ChangeEvent += Description21ChangeEventAction;
                        }
                    }
                    //CurrentItem=
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
                    MgtWtDocumentItem CurrentItem = CurrentObject.ParentDocument;
                    CurrentItem.WtDocumentObject.Description21ChangeEvent -= Description21ChangeEventAction;
                    CurrentItem.WtPartObject.Description21ChangeEvent -= Description21ChangeEventAction;
                    CurrentItem.WtDocumentObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;
                    CurrentItem.WtPartObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;

                    PropertyInfo LangProp = typeof(Webterm).GetProperty(CurrentDataContext.SelectedLanguage?.DataTableColonne);
                    if (LangProp != null)
                    {
                        Webterm CurrentWebterm = ListWebterm.FirstOrDefault((item) => LangProp.GetValue(item).ToString() == CurrentObject.DESCRIPTION21);
                        CurrentItem.WtDocumentObject.DESCRIPTION21 = CurrentObject.DESCRIPTION21;
                        CurrentItem.WtPartObject.DESCRIPTION21 = CurrentObject.DESCRIPTION21;
                        CurrentItem.WtDocumentObject.PTCCOMMONNAME = CurrentWebterm?.English;
                        CurrentItem.WtPartObject.PTCCOMMONNAME = CurrentWebterm?.English;
                    }

                    CurrentItem.WtDocumentObject.Description21ChangeEvent += Description21ChangeEventAction;
                    CurrentItem.WtPartObject.Description21ChangeEvent += Description21ChangeEventAction;
                    CurrentItem.WtDocumentObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                    CurrentItem.WtPartObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
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
                    MgtWtDocumentItem CurrentItem = CurrentObject.ParentDocument;
                    CurrentItem.WtDocumentObject.Description21ChangeEvent -= Description21ChangeEventAction;
                    CurrentItem.WtPartObject.Description21ChangeEvent -= Description21ChangeEventAction;
                    CurrentItem.WtDocumentObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;
                    CurrentItem.WtPartObject.PtcCommonNameChangeEvent -= PtcCommonNameChangeEventAction;

                    CurrentItem.WtDocumentObject.PTCCOMMONNAME = CurrentObject.PTCCOMMONNAME;
                    CurrentItem.WtPartObject.PTCCOMMONNAME = CurrentObject.PTCCOMMONNAME;
                    if (CurrentDataContext.SelectedLanguage?.DataTableColonne.ToUpper() == "ENGLISH")
                    {
                        CurrentItem.WtDocumentObject.DESCRIPTION21 = CurrentDataContext.ListWebtermLocal.FirstOrDefault();
                        CurrentItem.WtPartObject.DESCRIPTION21 = CurrentDataContext.ListWebtermLocal.FirstOrDefault();
                    }
                    else
                    {
                        PropertyInfo LangProp = typeof(Webterm).GetProperty(CurrentDataContext.SelectedLanguage?.DataTableColonne);
                        if (LangProp != null)
                        {
                            Webterm CurrentWebterm = ListWebterm.FirstOrDefault((item) => item.English == CurrentObject.PTCCOMMONNAME);
                            if (CurrentWebterm != null)
                            {
                                CurrentItem.WtDocumentObject.DESCRIPTION21 = CurrentDataContext.ListWebtermLocal.FirstOrDefault((item) => item == LangProp.GetValue(CurrentWebterm).ToString());
                                CurrentItem.WtPartObject.DESCRIPTION21 = CurrentDataContext.ListWebtermLocal.FirstOrDefault((item) => item == LangProp.GetValue(CurrentWebterm).ToString());
                            }
                            else
                                CurrentItem.WtPartObject.DESCRIPTION21 = "-";
                        }
                    }

                    CurrentItem.WtDocumentObject.Description21ChangeEvent += Description21ChangeEventAction;
                    CurrentItem.WtPartObject.Description21ChangeEvent += Description21ChangeEventAction;
                    CurrentItem.WtDocumentObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                    CurrentItem.WtPartObject.PtcCommonNameChangeEvent += PtcCommonNameChangeEventAction;
                }

            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ChangeSelectedLocalWebtermEvent(object sender, EventArgs e)
        {
            try
            {
                CurrentDataContext.ChangeWebtermEvent -= ChangeSelectedWebtermEvent;
                CurrentDataContext.ChangeLocalWebtermEvent -= ChangeSelectedLocalWebtermEvent;

                CurrentDataContext.SelectedWebterm = null;
                PropertyInfo LangProp = typeof(Webterm).GetProperty(CurrentDataContext.SelectedLanguage?.DataTableColonne);
                if (LangProp != null)
                {
                    Webterm CurrentWebterm = ListWebterm.FirstOrDefault((item) => LangProp.GetValue(item).ToString() == CurrentDataContext.SelectedLocalWebterm);
                    CurrentDataContext.SelectedWebterm = CurrentWebterm?.English;
                }

                CurrentDataContext.ChangeWebtermEvent += ChangeSelectedWebtermEvent;
                CurrentDataContext.ChangeLocalWebtermEvent += ChangeSelectedLocalWebtermEvent;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ChangeSelectedWebtermEvent(object sender, EventArgs e)
        {
            try
            {
                CurrentDataContext.ChangeWebtermEvent -= ChangeSelectedWebtermEvent;
                CurrentDataContext.ChangeLocalWebtermEvent -= ChangeSelectedLocalWebtermEvent;

                CurrentDataContext.SelectedLocalWebterm = null;
                Webterm CurrentWebterm = ListWebterm.FirstOrDefault((item) => item.English == CurrentDataContext.SelectedWebterm);
                if (CurrentDataContext.SelectedLanguage != null)
                {
                    if (CurrentDataContext.SelectedLanguage.DataTableColonne.ToUpper() == "ENGLISH")
                        CurrentDataContext.SelectedLocalWebterm = CurrentDataContext.ListWebtermLocal.FirstOrDefault();
                    else
                    {
                        PropertyInfo LangProp = typeof(Webterm).GetProperty(CurrentDataContext.SelectedLanguage?.DataTableColonne);
                        if (LangProp != null && CurrentWebterm != null)
                            CurrentDataContext.SelectedLocalWebterm = CurrentDataContext.ListWebtermLocal.FirstOrDefault((item) => item == LangProp.GetValue(CurrentWebterm).ToString());
                    }
                }
                CurrentDataContext.ChangeWebtermEvent += ChangeSelectedWebtermEvent;
                CurrentDataContext.ChangeLocalWebtermEvent += ChangeSelectedLocalWebtermEvent;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void GetItemFromClipboard()
        {
            try
            {
                listItemFromClipboard = new List<MgtWtObject>();
                string CompleteString = null;
                if (Clipboard.GetData(DataFormats.Text) != null)
                    CompleteString = Clipboard.GetData(DataFormats.Text).ToString();

                if (CompleteString != null)
                {
                    var AllLines = CompleteString.Split('\n');

                    MgtWtObject NewValue = null;
                    string linePurged = null;
                    string TempNumber;
                    double tempMass = 0;
                    foreach (var line in AllLines)
                    {
                        linePurged = line.Split('\r').FirstOrDefault();
                        var AllValues = linePurged.Split('\t');
                        if (AllValues != null && AllValues.Count() > 8)
                        {
                            TempNumber = AllValues.FirstOrDefault().Trim().ToUpper();
                            if (!string.IsNullOrWhiteSpace(TempNumber))
                            {
                                try { tempMass = Convert.ToDouble(AllValues[7].Trim().ToUpper()); }
                                catch { tempMass = 0; }
                                NewValue = new MgtWtObject()
                                {
                                    NUMBER = TempNumber,
                                    REVISION = AllValues[1].Trim().ToUpper(),
                                    PTCCOMMONNAME = AllValues[2].Trim().ToUpper(),
                                    DESCRIPTION2 = AllValues[3].Trim().ToUpper(),
                                    DESCRIPTION21 = AllValues[4].Trim().ToUpper(),
                                    DESCRIPTION22 = AllValues[5].Trim().ToUpper(),
                                    GROUPCREATOR = AllValues[6].Trim().ToUpper(),
                                    MASS = tempMass,
                                    QUALINSPGRP = AllValues[8].Trim().ToUpper(),
                                };
                                listItemFromClipboard.Add(NewValue);
                            }
                        }
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
