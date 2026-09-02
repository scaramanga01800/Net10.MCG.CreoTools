using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.WindchillCredential;
using MCG.CREO_Tools.QuickSearch.Exceptions;
using MCG.CREO_Tools.QuickSearch.View;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Services.Interfaces;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace MCG.CREO_Tools.QuickSearch.ViewModel
{
    public class QuickSearchWindowRefDocFromNumberViewModel : ObservableObject, IQuickSearchWindowRefDocFromNumberViewModel
    {
        #region [REGION] Properties from Interface
        private string _Number;
        public string Number
        {
            get { return this._Number; }
            set
            {
                if (this._Number != value)
                {
                    this._Number = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _RefDocNumber;
        public string RefDocNumber
        {
            get { return this._RefDocNumber; }
            set
            {
                if (this._RefDocNumber != value)
                {
                    this._RefDocNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsRefDocFound = false;
        public bool IsRefDocFound
        {
            get { return this._IsRefDocFound; }
            set
            {
                if (this._IsRefDocFound != value)
                {
                    this._IsRefDocFound = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region [REGION] Internal variables
        //private QuickSearchWindowRefDocFromNumberView CurrentQuickSearchWindowRefDocFromNumberView = new QuickSearchWindowRefDocFromNumberView();
        //private NetworkCredential WindchillNetworkCredential = new NetworkCredential();
        string CompleteReferenceFileName = null;
        string UrlReferenceFileName = null;
        bool IsFirstDownload = true;
        #endregion

        #region [REGION] Commands
        public ICommand CommandOpenRefDoc { get => new RelayCommand(() => ExecuteOpenRefDoc()); }
        public ICommand CommandSearchRefDoc { get => new RelayCommand(() => ExecuteSearchRefDoc()); }
        public ICommand CommandClose { get => new RelayCommand(() => RaiseCloseEvent()); }
        #endregion

        public event EventHandler CloseEvent;

        public void RaiseCloseEvent()
        {
            try
            {
                CloseEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        #region [REGION] Init
        private readonly IHtmlTools _htmlTools;
        private readonly IWindchillCredentialService _windchillCredentialService;
        private readonly IWindchillPartManagementService _windchillPartManagementService;

        public QuickSearchWindowRefDocFromNumberViewModel(IHtmlTools htmlTools,
                                                          IWindchillCredentialService windchillCredentialService,
                                                          IWindchillPartManagementService windchillPartManagementService)
        {
            _htmlTools = htmlTools;
            _windchillCredentialService = windchillCredentialService;
            _windchillPartManagementService = windchillPartManagementService;
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteOpenRefDoc()
        {
            try
            {
                if (CompleteReferenceFileName != null)
                {
                    if (!IsFirstDownload)
                        ExecuteSearchRefDoc();
                    IsFirstDownload = false;
                    WindchillCredentialItem WindchillCrendential = _windchillCredentialService.GetWindchillCredential(CommonLibConstants.WindchillUrl, CommonLibConstants.WindchillUrl);
                    _htmlTools.DownloadFileFromUrl(UrlReferenceFileName, WindchillCrendential.WindchillCredential, CompleteReferenceFileName);
                    McgFileAndSystemTools.OpenFile(CompleteReferenceFileName);
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSearchRefDoc()
        {
            try
            {
                IsRefDocFound = false;
                IsFirstDownload = true;
                //WindchillNetworkCredential = new NetworkCredential();

                WindchillCredentialItem WindchillCrendential = _windchillCredentialService.GetWindchillCredential(CommonLibConstants.WindchillUrl, CommonLibConstants.WindchillUrl);

                if (Number == null || Number.Contains("*"))
                {

                }
                else
                {

                    RestOdataWtPart CurrentRestOdataWtPart = _windchillPartManagementService.GetOneWtPartWithWtDocumentAssociation(WindchillCrendential.WindchillCredential, Number.Trim().ToUpper());

                    if (CurrentRestOdataWtPart != null && CurrentRestOdataWtPart.References != null && CurrentRestOdataWtPart.References.Count > 0)
                    {
                        Regex RegexRefDoc = new Regex("^TDFC|^GEI", RegexOptions.IgnoreCase);
                        RestOdataWtObjectAssociation CurrentDoc = CurrentRestOdataWtPart.References.Where((doc) => doc.References != null &&
                                                RegexRefDoc.IsMatch(doc.References.Number) &&
                                                doc.References.PrimaryContent != null &&
                                                doc.References.PrimaryContent.Content != null).FirstOrDefault();
                        if (CurrentDoc != null)
                        {
                            CompleteReferenceFileName = McgFileAndSystemTools.BuildSafeFilePath(System.Environment.GetEnvironmentVariable("TEMP"), CurrentDoc.References.PrimaryContent.Content.Label);
                            UrlReferenceFileName = CurrentDoc.References.PrimaryContent.Content.URL;
                            RefDocNumber = CurrentDoc.References.Number;
                            IsRefDocFound = true;
                        }
                        else
                            RefDocNumber = McgWpfTools.GetStringResource("QS_RefDocNotfound");
                    }
                    else
                        RefDocNumber = McgWpfTools.GetStringResource("QS_RefDocNotfound");

                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
