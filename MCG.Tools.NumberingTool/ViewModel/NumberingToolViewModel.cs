using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.WindchillCredential;
using MCG.Tools.NumberingTool.Configuration;
using MCG.Tools.NumberingTool.Exceptions;
using MCG.Tools.NumberingTool.Interfaces;
using MCG.Tools.NumberingTool.Messages;
using MCG.Tools.NumberingTool.View;
using MCG.Tools.VisualizationLib.Interfaces;
using MCG.WindchillRequestTool.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MCG.Tools.NumberingTool.ViewModel
{
    public class NumberingToolViewModel : ObservableObject, INumberingToolViewModel
    {
        #region [REGION] Properties from Interface
        public ObservableCollection<NumberingToolTemplate> ListNumberingTemplate { get; set; } = new ObservableCollection<NumberingToolTemplate>();
        private NumberingToolTemplate _SelectedNumberingTemplate;
        public NumberingToolTemplate SelectedNumberingTemplate
        {
            get { return _SelectedNumberingTemplate; }
            set
            {
                if (this._SelectedNumberingTemplate != value)
                {
                    this._SelectedNumberingTemplate = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _SearchNumber;
        public string SearchNumber
        {
            get { return _SearchNumber; }
            set
            {
                if (this._SearchNumber != value)
                {
                    this._SearchNumber = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _SearchDescription;
        public string SearchDescription
        {
            get { return _SearchDescription; }
            set
            {
                if (this._SearchDescription != value)
                {
                    this._SearchDescription = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _SearchProduct;
        public string SearchProduct
        {
            get { return _SearchProduct; }
            set
            {
                if (this._SearchProduct != value)
                {
                    this._SearchProduct = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _SearchCreatedBy;
        public string SearchCreatedBy
        {
            get { return _SearchCreatedBy; }
            set
            {
                if (this._SearchCreatedBy != value)
                {
                    this._SearchCreatedBy = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<string> SearchProductList { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> SearchCreatedByList { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> SearchFormatList { get; set; } = new ObservableCollection<string>();

        private DateTime? _SearchCreatedAfter = null;
        public DateTime? SearchCreatedAfter
        {
            get { return _SearchCreatedAfter; }
            set
            {
                if (this._SearchCreatedAfter != value)
                {
                    this._SearchCreatedAfter = value;
                    OnPropertyChanged();
                }

            }
        }

        private DateTime? _SearchCreatedBefore = null;
        public DateTime? SearchCreatedBefore
        {
            get { return _SearchCreatedBefore; }
            set
            {
                if (this._SearchCreatedBefore != value)
                {
                    this._SearchCreatedBefore = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<NumberingToolItem> ListSearchNumber { get; set; } = new ObservableCollection<NumberingToolItem>();

        private NumberingToolItem _SelectedSearchNumber;
        public NumberingToolItem SelectedSearchNumber
        {
            get { return _SelectedSearchNumber; }
            set
            {
                if (this._SelectedSearchNumber != value)
                {
                    this._SelectedSearchNumber = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<NumberingToolItem> ListNewNumber { get; set; } = new ObservableCollection<NumberingToolItem>();
        private NumberingToolItem _SelectedNewNumber;
        public NumberingToolItem SelectedNewNumber
        {
            get { return _SelectedNewNumber; }
            set
            {
                if (this._SelectedNewNumber != value)
                {
                    this._SelectedNewNumber = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<int> ListSizeBlock { get; set; } = new ObservableCollection<int>();
        private int _SelectedSizeBlock = 1;
        public int SelectedSizeBlock
        {
            get { return _SelectedSizeBlock; }
            set
            {
                if (this._SelectedSizeBlock != value)
                {
                    this._SelectedSizeBlock = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsSeveralNumberCreated = false;
        public bool IsSeveralNumberCreated
        {
            get { return _IsSeveralNumberCreated; }
            set
            {
                if (this._IsSeveralNumberCreated != value)
                {
                    this._IsSeveralNumberCreated = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsUpdateShown = false;
        public bool IsUpdateShown
        {
            get { return _IsUpdateShown; }
            set
            {
                if (this._IsUpdateShown != value)
                {
                    this._IsUpdateShown = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsCreateZip = false;
        public bool IsCreateZip
        {
            get { return _IsCreateZip; }
            set
            {
                if (this._IsCreateZip != value)
                {
                    this._IsCreateZip = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public NumberingToolItem NewCreatedItem { get; set; }
        public bool NoRangeAuthorized { get; set; } = false;
        private string MainAppFolder;

        private WindchillCredentialItem WindchillNetworkCredential { get; set; } = null;

        private readonly IHtmlTools _htmlTools;
        private readonly IRegExTools _regExTools;
        private readonly INumberingToolWindowService _numberingToolWindowService;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly INumberingRangeService _numberingRangeService;
        private readonly IWindchillNavigationService _windchillNavigationService;
        private readonly IWtDownloadViewableTools _wtDownloadViewableTools;
        #endregion

        #region [REGION] Commands
        public ICommand CommandCreateNumber { get => new RelayCommand(() => ExecuteCreateNumber()); }
        public ICommand CommandCreateSeveralNumbers { get => new RelayCommand(() => ExecuteCreateSeveralNumbers()); }
        public ICommand CommandUpdateNumber { get => new RelayCommand(() => ExecuteUpdateNumber()); }
        public ICommand CommandStartSearch { get => new RelayCommand(() => ExecuteStartSearch()); }
        public ICommand CommandStartCreateSeveralNumbers { get => new RelayCommand(() => ExecuteStartCreateSeveralNumbers()); }
        public ICommand CommandStartUpdateSeveralNumbers { get => new RelayCommand(() => ExecuteStartUpdateSeveralNumbers()); }
        public ICommand CommandCancel { get => new RelayCommand(() => { Application.Current.Windows.OfType<Window>().First((item) => item.IsActive).Close(); }); }
        public ICommand CommandUseNewNumber { get => new RelayCommand(() => ExecuteUseNewNumber()); }
        public ICommand CommandUseSearchNumber { get => new RelayCommand(() => ExecuteUseSearchNumber()); }
        public ICommand CommandDownloadDrawing { get => new RelayCommand(() => ExecuteDownloadDrawing()); }
        public ICommand CommandOpenPartDetail { get => new RelayCommand(() => ExecuteOpenPartDetail()); }
        #endregion

        #region [REGION] Events
        public event EventHandler CreateNumberEvent;
        public void RaiseCreateNumberEvent()
        {
            try
            {
                CreateNumberEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UseNumberEvent;
        public void RaiseUseNumberEvent(string CurrentNumber)
        {
            try
            {
                UseNumberEvent?.Invoke(CurrentNumber, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Init
        public NumberingToolViewModel(IHtmlTools htmlTools,
                                      IRegExTools regExTools,
                                      INumberingToolWindowService numberingToolWindowService,
                                      IWindchillCredentialService windchillCredentialService,
                                      INumberingRangeService numberingRangeService,
                                      IWindchillNavigationService windchillNavigationService,
                                      IWtDownloadViewableTools wtDownloadViewableTools)
        {
            try
            {
                //NoRangeAuthorized = pNoRangeAuthorized;
                _htmlTools = htmlTools;
                _regExTools = regExTools;
                _numberingToolWindowService = numberingToolWindowService;
                _windchillCredentialService = windchillCredentialService;
                _numberingRangeService = numberingRangeService;
                _windchillNavigationService = windchillNavigationService;
                _wtDownloadViewableTools = wtDownloadViewableTools;

                WeakReferenceMessenger.Default.Register<NumberUpdatedMessage>(this, (recipient, message) =>
                {
                    NumberingToolItem UpdatedItem = message.Item;
                    UpdateNumber(UpdatedItem);
                });
                WeakReferenceMessenger.Default.Register<NumberCreatedMessage>(this, (recipient, message) =>
                {
                    NumberingToolItem UpdatedItem = message.Item;
                    NumberingToolTemplate Template = message.Template;
                    CreateNumber(UpdatedItem, Template);
                });

                //ReadNumberingTemplateList();
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void SetNumberingToolViewModelProperties(bool pNoRangeAuthorized = false)
        {
            try
            {
                NoRangeAuthorized = pNoRangeAuthorized;
                ReadNumberingTemplateList();
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ReadNumberingTemplateList()
        {
            try
            {

                var TempList = _numberingRangeService.GetActiveTemplates();
                foreach (var item in TempList)
                    ListNumberingTemplate.Add(NumberingToolTemplate.GetNumberingToolTemplate(item, NoRangeAuthorized));
                SelectedNumberingTemplate = ListNumberingTemplate.FirstOrDefault();

                var TempListFormat = _numberingRangeService.GetDistinctFormats();
                TempListFormat.Sort();
                foreach (var item in TempListFormat)
                    SearchFormatList.Add(item);


                var TempListProduct = _numberingRangeService.GetDistinctProducts();
                TempListProduct.Sort();
                foreach (var item in TempListProduct)
                    SearchProductList.Add(item);

                var TempListCreatedBy = _numberingRangeService.GetDistinctCreatedBy();
                TempListCreatedBy.Sort();
                foreach (var item in TempListCreatedBy)
                    SearchCreatedByList.Add(item);
            }
            catch (Exception ex)
            {
                throw new NumberingToolException(this.GetType().Name, ex);
            }
        }

        private void CheckWindchillCredential()
        {
            try
            {
                if (WindchillNetworkCredential == null || !WindchillNetworkCredential.IsCredentialOk)
                {
                    WindchillNetworkCredential = _windchillCredentialService.GetWindchillCredential(CommonLibConstants.WindchillUrl);
                }
            }
            catch (Exception ex)
            {
                throw new NumberingToolException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteCreateNumber()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteCreateNumber Action, range {SelectedNumberingTemplate}");
                if (SelectedNumberingTemplate != null)
                {
                    if (SelectedNumberingTemplate.NumberingTemplate == "84XXXXXX")
                    {
                        PartNumberRange CurrentRange = Create84PartNumber();
                        if (CurrentRange != null && CurrentRange.Values != null)
                        {
                            NewCreatedItem = new NumberingToolItem() { Number = CurrentRange.Values.FirstOrDefault() };
                            RaiseCreateNumberEvent();
                            RaiseUseNumberEvent(NewCreatedItem.Number);
                            Clipboard.SetText(NewCreatedItem.Number);

                            MessageBox.Show($"{McgWpfTools.GetStringResource("NUT_MsgNewNumberCreated")} {NewCreatedItem.Number}" +
                                $"\n{McgWpfTools.GetStringResource("NUT_MsgNewNumberCreated2")}" +
                                $"\n{McgWpfTools.GetStringResource("NUT_MsgNewNumberCreated3")}");
                        }
                    }
                    else
                    {
                        _numberingToolWindowService.ShowNumberingToolUpdateCreateFluentView(true, SelectedNumberingTemplate, SearchProductList.ToList(), SearchFormatList.ToList());
                        //NumberingToolUpdateCreateFluentView CurrentNumberingToolUpdateCreate = new NumberingToolUpdateCreateFluentView(true, SelectedNumberingTemplate, SearchProductList.ToList(), SearchFormatList.ToList());
                        //CurrentNumberingToolUpdateCreate.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreateSeveralNumbers()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteCreateSeveralNumbers Action");
                if (SelectedNumberingTemplate != null)
                {
                    ListNewNumber.Clear();
                    var AlreadyCreatedWindow = McgWpfTools.IsWindowAlreadyCreated<NumberingToolCreateSeveralFluentView>(true);
                    if (AlreadyCreatedWindow == null)
                    {
                        IsSeveralNumberCreated = false;
                        IsUpdateShown = false;
                        ListSizeBlock.Clear();
                        for (int index = 1; index <= SelectedNumberingTemplate.MaxRange; index++)
                            ListSizeBlock.Add(index);
                        SelectedSizeBlock = 1;

                        _numberingToolWindowService.ShowNumberingToolCreateSeveralFluentView(this);

                        //NumberingToolCreateSeveralFluentView CurrentNumberingToolCreateSeveralView = new NumberingToolCreateSeveralFluentView(this);
                        //CurrentNumberingToolCreateSeveralView.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUpdateNumber()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteUpdateNumber Action");
                if (SelectedSearchNumber != null)
                {
                    _numberingToolWindowService.ShowNumberingToolUpdateCreateFluentView(false, null, SearchProductList.ToList(), SearchFormatList.ToList(), SelectedSearchNumber);
                    //NumberingToolUpdateCreateFluentView CurrentNumberingToolUpdateCreate = new NumberingToolUpdateCreateFluentView(false, null, SearchProductList.ToList(), SearchFormatList.ToList(), SelectedSearchNumber);
                    //CurrentNumberingToolUpdateCreate.Show();
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartSearch()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteStartSearch Action");
                ListSearchNumber.Clear();

                DateOnly CreatedAfter;
                DateOnly CreatedBefore;

                if (SearchCreatedAfter == null) CreatedAfter = new DateOnly(1, 1, 1);
                else CreatedAfter = DateOnly.FromDateTime(SearchCreatedAfter.Value);
                if (SearchCreatedBefore == null) CreatedBefore = new DateOnly(9999, 1, 1);
                else CreatedBefore = DateOnly.FromDateTime(SearchCreatedBefore.Value);

                if (SearchNumber == null)
                    SearchNumber = "";
                var ListOfRegexNumber = _regExTools.GetRegexList(SearchNumber.Trim().Replace(" ", "|"), true);
                if (SearchDescription == null)
                    SearchDescription = "";
                var ListOfRegexDescription = _regExTools.GetRegexList(SearchDescription.Trim().Replace(" ", "|"), true);
                if (SearchProduct == null)
                    SearchProduct = "";
                var ListOfRegexProduct = _regExTools.GetRegexList(SearchProduct.Trim().Replace(" ", "|"), true);
                if (SearchCreatedBy == null)
                    SearchCreatedBy = "";
                var ListOfRegexCreatedBy = _regExTools.GetRegexList(SearchCreatedBy.Trim().Replace(" ", "|"), true);

                var TempList = _numberingRangeService.GetNumberingItemsFromDates(CreatedAfter, CreatedBefore);
                foreach (var item in TempList)
                {
                    if (_regExTools.CheckStringWithRegExList(item.Description, ListOfRegexDescription)
                        && _regExTools.CheckStringWithRegExList(item.Number, ListOfRegexNumber)
                        && _regExTools.CheckStringWithRegExList(item.Product, ListOfRegexProduct)
                        && _regExTools.CheckStringWithRegExList(item.Createdbyfullname, ListOfRegexCreatedBy))
                        ListSearchNumber.Add(NumberingToolItem.GetNumberingToolItem(item));
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartCreateSeveralNumbers()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteStartCreateSeveralNumbers Action, range {SelectedNumberingTemplate}, size block {SelectedSizeBlock}");
                IsSeveralNumberCreated = true;
                IsUpdateShown = true;
                ListNewNumber.Clear();

                if (SelectedNumberingTemplate.NumberingTemplate == "84XXXXXX")
                {
                    PartNumberRange CurrentRange = Create84PartNumber(SelectedSizeBlock);
                    if (CurrentRange != null && CurrentRange.Values != null)
                    {
                        string CreatedBy = McgActiveDirectoryTools.GetWindowsSessionUserId();
                        foreach (var item in CurrentRange.Values)
                            ListNewNumber.Add(new NumberingToolItem()
                            {
                                Number = item,
                                CreatedBy = CreatedBy,
                                Format = "N/A",
                                Product = "N/A",
                                CreatedOn = DateTime.Today,
                                Description = " ",
                                IsUpdated = false
                            });

                        IsUpdateShown = false;
                    }
                }
                else
                {
                    NumberingToolItem CurrentItem = null;

                    for (int index = 0; index < SelectedSizeBlock; index++)
                    {
                        string CreatedBy = McgActiveDirectoryTools.GetWindowsSessionUserShortName();
                        CurrentItem = new NumberingToolItem()
                        {
                            CreatedBy = CreatedBy,
                            CreatedById = Environment.UserName,
                            Format = "N/A",
                            Product = "N/A",
                            CreatedOn = DateTime.Today,
                            Description = " "
                        };
                        CurrentItem.IsUpdated = false;
                        ListNewNumber.Add(CurrentItem);
                        AddNewNumberingItemDataBase(SelectedNumberingTemplate, CurrentItem);
                    }
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartUpdateSeveralNumbers()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteStartUpdateSeveralNumbers Action, range {SelectedNumberingTemplate}, size block {SelectedSizeBlock}");

                foreach (var CurrentItem in ListNewNumber.Where((item) => item.IsUpdated))
                    DbUpdateNumber(CurrentItem);
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUseNewNumber()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteUseNewNumber Action,  Number {SelectedNewNumber.Number}");
                if (SelectedNewNumber != null)
                {
                    Clipboard.SetText(SelectedNewNumber.Number);
                    RaiseUseNumberEvent(SelectedNewNumber.Number);
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteUseSearchNumber()
        {
            try
            {
                TraceLog.AddTraceLog($"Enter ExecuteUseSearchNumber Action, Number {SelectedSearchNumber.Number}");
                if (SelectedSearchNumber != null)
                {
                    Clipboard.SetText(SelectedSearchNumber.Number);
                    RaiseUseNumberEvent(SelectedSearchNumber.Number);
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDownloadDrawing()
        {
            try
            {
                if (SelectedSearchNumber != null)
                {

                    bool redult = _wtDownloadViewableTools.DownloadPartMainDrawing(SelectedSearchNumber.Number, "Latest", DocumentTypeEnum.PART, IsCreateZip);
                    if(!redult)
                        MessageBox.Show(McgWpfTools.GetStringResource("NUT_DownloadDrawingNotAvailable"), McgWpfTools.GetStringResource("NUT_TitleDownloadDrawingNotAvailable"), MessageBoxButton.OK, MessageBoxImage.Warning); 
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenPartDetail()
        {
            try
            {
                if (SelectedSearchNumber != null)
                {
                    CheckWindchillCredential();
                    bool IsWindchillPart = _windchillNavigationService.OpenWtPartDetailPage(WindchillNetworkCredential.WindchillCredential, SelectedSearchNumber.Number);
                    if (!IsWindchillPart)
                        MessageBox.Show(McgWpfTools.GetStringResource("NUT_WindchillPartNotAvailable"), McgWpfTools.GetStringResource("NUT_TitleWindchillPartNotAvailable"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        private void CreateNumber(NumberingToolItem currentItem, NumberingToolTemplate currentTemplate)
        {
            try
            {
                if (currentItem != null && currentTemplate != null)
                {
                    string Number = AddNewNumberingItemDataBase(currentTemplate, currentItem);

                    NewCreatedItem = currentItem;
                    RaiseCreateNumberEvent();
                    RaiseUseNumberEvent(Number);
                    Clipboard.SetText(Number);

                    MessageBox.Show($"{McgWpfTools.GetStringResource("NUT_MsgNewNumberCreated")} {Number}");
                }
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateNumber(NumberingToolItem currentItem)
        {
            try
            {
                if (currentItem != null)
                {
                    DbUpdateNumber(currentItem);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DbUpdateNumber(NumberingToolItem CurrentNumber)
        {
            try
            {
                NumberingItem CurrentDbItem = _numberingRangeService.GetNumberingItem(CurrentNumber.Number);
                if (CurrentDbItem != null)
                {
                    CurrentDbItem.Description = CurrentNumber.Description;
                    CurrentDbItem.Format = CurrentNumber.Format;
                    CurrentDbItem.Product = CurrentNumber.Product;
                    _numberingRangeService.UpdateNumberingItem(CurrentDbItem);
                    CurrentNumber.IsUpdated = false;
                }
            }
            catch (Exception ex)
            {
                throw new NumberingToolException(this.GetType().Name, ex);
            }
        }

        private string AddNewNumberingItemDataBase(NumberingToolTemplate CurrentTemplate, NumberingToolItem CurrentItem)
        {
            try
            {
                string Number = _numberingRangeService.GetNextFormattedSequenceValue(CurrentTemplate.SequenceName, CurrentTemplate.Prefix, CurrentTemplate.Suffix, CurrentTemplate.LeadingZeroFormat);

                if (CurrentItem != null)
                {
                    CurrentItem.Number = Number;
                    _numberingRangeService.InsertNumberingItems(new List<NumberingItem> { CurrentItem.GetNumberingItem() });
                }

                return Number;
            }
            catch (Exception ex)
            {
                throw new NumberingToolException(this.GetType().Name, ex);
            }
        }

        private PartNumberRange Create84PartNumber(int NbPartToCreate = 1)
        {
            try
            {
                PartNumberRange CurrentRange = null;
                string UrlPartGenerator = string.Format(NumberingToolConstants.HtmlLinkNewPatNumberGenerator, NbPartToCreate, $"{McgActiveDirectoryTools.GetWindowsSessionUserId()}_CREOTOOL");

                string RequestResult = _htmlTools.GetRestOdataJsonRequestAnonymous(UrlPartGenerator);
                CurrentRange = _htmlTools.GetJsonObject<PartNumberRange>(RequestResult);

                return CurrentRange;
            }
            catch (Exception ex)
            {
                throw new NumberingToolException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}




