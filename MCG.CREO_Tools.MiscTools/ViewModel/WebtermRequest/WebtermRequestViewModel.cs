using MCG.CREO_Tools.MiscTools.View.WebtermRequest;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CommonLib.Models.Email;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.Configuration;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CommonLib.WebtermLib.Services.Interfaces;

namespace MCG.CREO_Tools.MiscTools.ViewModel.WebtermRequest
{
    public class WebtermRequestViewModel : ObservableObject, IWebtermRequestViewModel
    {
        #region [REGION] Properties from Interface
        public WebtermRequestDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        #endregion

        #region [REGION] Commands
        public ICommand CommandDrop { get => new RelayCommand<DragEventArgs>((obj) => ExecuteDrop(obj)); }
        public ICommand CommandStartClassOrder { get => new RelayCommand<string>((obj) => ExecuteStartClassOrder(obj)); }
        public ICommand CommandSendRequest { get => new RelayCommand(() => ExecuteSendRequest()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        public ICommand CommandDeletePicture { get => new RelayCommand<KeyEventArgs>((obj) => ExecuteDeletePicture(obj)); }
        #endregion

        #region [REGION] Events Action
        public event EventHandler CallCloseEvent;

        public void RaiseCallCloseEvent()
        {
            try
            {
                CallCloseEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Init
        private readonly IWebtermTools _webtermTools;
        public WebtermRequestViewModel(IWebtermTools webtermTools)
        {
            try
            {
                _webtermTools = webtermTools;

                CurrentDataContext = new WebtermRequestDataContext();
                CurrentDataContext.CurrentRequest = new WebtermRequestItem();

                foreach (var item in _webtermTools.GetWebtermClasses().OrderBy(t => t.NameEn))
                    CurrentDataContext.ListClass.Add(WebtermRequestClass.GetClass(item));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteDrop(DragEventArgs obj)
        {
            try
            {
                if (obj != null && obj.Data != null && obj.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])obj.Data.GetData(DataFormats.FileDrop);

                    foreach (var file in files)
                    {
                        if (McgFileAndSystemTools.IsImage(file) && CurrentDataContext.CurrentRequest.ListImage.FirstOrDefault(item => item == file) == null)
                            CurrentDataContext.CurrentRequest.ListImage.Add(file);

                    }
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteStartClassOrder(string obj)
        {
            try
            {
                List<WebtermRequestClass> list = null;
                if (obj == "En")
                    list = CurrentDataContext.ListClass.OrderBy(t => t.NameEn).ToList();
                else if (obj == "Fr")
                    list = CurrentDataContext.ListClass.OrderBy(t => t.NameFr).ToList();
                else if (obj == "De")
                    list = CurrentDataContext.ListClass.OrderBy(t => t.NameDe).ToList();

                CurrentDataContext.ListClass.Clear();
                if (list != null)
                    foreach (var item in list)
                        CurrentDataContext.ListClass.Add(item);
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteSendRequest()
        {
            try
            {
                if (CurrentDataContext.CurrentRequest.MinMass > CurrentDataContext.CurrentRequest.MaxMass
                   || string.IsNullOrEmpty(CurrentDataContext.CurrentRequest.DescriptionEn)
                   || string.IsNullOrEmpty(CurrentDataContext.CurrentRequest.TermAbbrevitationEn)
                   || string.IsNullOrEmpty(CurrentDataContext.CurrentRequest.TermAbbrevitationFr)
                   || string.IsNullOrEmpty(CurrentDataContext.CurrentRequest.TermAbbrevitationDe)
                   || string.IsNullOrEmpty(CurrentDataContext.CurrentRequest.TermEn)
                   || string.IsNullOrEmpty(CurrentDataContext.CurrentRequest.TermFr)
                   || string.IsNullOrEmpty(CurrentDataContext.CurrentRequest.TermDe)
                   || string.IsNullOrEmpty(CurrentDataContext.CurrentRequest.TermUpperCaseEn)
                   || string.IsNullOrEmpty(CurrentDataContext.CurrentRequest.TermUpperCaseFr)
                   || string.IsNullOrEmpty(CurrentDataContext.CurrentRequest.TermUpperCaseDe)
                   || CurrentDataContext.CurrentRequest.SelectedClass == null
                   || CurrentDataContext.CurrentRequest.ListImage.Count == 0)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("WTR_MsgMissingInformation"), McgWpfTools.GetStringResource("WTR_MsgTitleIssue"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (CurrentDataContext.CurrentRequest.TermAbbrevitationEn?.Count() > 19
                          || CurrentDataContext.CurrentRequest.TermAbbrevitationFr?.Count() > 19
                          || CurrentDataContext.CurrentRequest.TermAbbrevitationDe?.Count() > 19)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("WTR_MsgExceed19Carac"), McgWpfTools.GetStringResource("WTR_MsgTitleIssue"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("WTR_MsgSendMail"), McgWpfTools.GetStringResource("WTR_MsgTitleIssue"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        CreateMailRequest();
                        RaiseCallCloseEvent();
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
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("WTR_UserGuide"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteDeletePicture(KeyEventArgs obj = null)
        {
            try
            {
                if (CurrentDataContext.CurrentRequest != null && CurrentDataContext.CurrentRequest.SelectedImage != null)
                {
                    if (obj != null && obj.Key == Key.Delete)
                        CurrentDataContext.CurrentRequest.ListImage.Remove(CurrentDataContext.CurrentRequest.SelectedImage);
                    else if (obj == null)
                        CurrentDataContext.CurrentRequest.ListImage.Remove(CurrentDataContext.CurrentRequest.SelectedImage);
                }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        private void CreateMailRequest()
        {
            try
            {
                string MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);

                if (MainAppFolder == null || MainAppFolder == "" || !Directory.Exists(MainAppFolder))
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                string templateFile = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.TemplateWebtermRequest}";

                if (File.Exists(templateFile))
                {
                    string templateText = File.ReadAllText(templateFile);
                    if (templateText != null)
                    {
                        templateText = templateText.Replace("ValUnit", CurrentDataContext.CurrentRequest.DefaulUnit)
                            .Replace("ValQualInspGrp", CurrentDataContext.CurrentRequest.QualInspGrp)
                            .Replace("ValMinMass", CurrentDataContext.CurrentRequest.MinMass.ToString())
                            .Replace("ValMinMass", CurrentDataContext.CurrentRequest.MaxMass.ToString())
                            .Replace("ValTermEn", CurrentDataContext.CurrentRequest.TermEn)
                            .Replace("ValDescEn", CurrentDataContext.CurrentRequest.DescriptionEn)
                            .Replace("ValUpperCaseEn", CurrentDataContext.CurrentRequest.TermUpperCaseEn)
                            .Replace("ValAbbrevEn", CurrentDataContext.CurrentRequest.TermAbbrevitationEn)
                            .Replace("ValAttributeEn", CurrentDataContext.CurrentRequest.AttributeEn)
                            .Replace("ValAttributeExampleEn", CurrentDataContext.CurrentRequest.AttributeExampleEn)
                            .Replace("ValTermFr", CurrentDataContext.CurrentRequest.TermFr)
                            .Replace("ValDescFr", CurrentDataContext.CurrentRequest.DescriptionFr)
                            .Replace("ValUpperCaseFr", CurrentDataContext.CurrentRequest.TermUpperCaseFr)
                            .Replace("ValAbbrevFr", CurrentDataContext.CurrentRequest.TermAbbrevitationFr)
                            .Replace("ValAttributeFr", CurrentDataContext.CurrentRequest.AttributeFr)
                            .Replace("ValAttributeExampleFr", CurrentDataContext.CurrentRequest.AttributeExampleFr)
                            .Replace("ValTermDe", CurrentDataContext.CurrentRequest.TermDe)
                            .Replace("ValDescDe", CurrentDataContext.CurrentRequest.DescriptionDe)
                            .Replace("ValUpperCaseDe", CurrentDataContext.CurrentRequest.TermUpperCaseDe)
                            .Replace("ValAbbrevDe", CurrentDataContext.CurrentRequest.TermAbbrevitationDe)
                            .Replace("ValAttributeDe", CurrentDataContext.CurrentRequest.AttributeDe)
                            .Replace("ValAttributeExampleDe", CurrentDataContext.CurrentRequest.AttributeExampleDe);
                    }
                    SendEmail(templateText);
                }

            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void SendEmail(string MailBody)
        {
            try
            {
                string MailObject = McgWpfTools.GetStringResource("WTR_MailTitle");
                var SendEmail = MiscToolsConstants.WebtermRequestMail.Split(';');
                string SendEmailCC = MiscToolsConstants.WebtermRequestMailCC;
                string MailFrom = McgActiveDirectoryTools.GetWindowsSessionUserEMail();

                McgEMail NewEmail = new McgEMail()
                {
                    MailBody = MailBody,
                    MailFrom = MailFrom,
                    MailSender = MailFrom,
                    Mailsubject = MailObject,
                    MailRestritedListAddress = new List<McgEMailItem>(),
                    MailRestritedListAddressCC = new List<McgEMailItem>()
                };

                foreach (var mail in SendEmail)
                    if (!string.IsNullOrEmpty(mail))
                        NewEmail.MailRestritedListAddress.Add(new McgEMailItem() { Location = "ALL", MailAddress = mail, Name = mail });
                NewEmail.MailRestritedListAddressCC.Add(new McgEMailItem() { Location = "ALL", MailAddress = SendEmailCC, Name = SendEmailCC });
                NewEmail.MailRestritedListAddressCC.Add(new McgEMailItem() { Location = "ALL", MailAddress = MailFrom, Name = MailFrom });

                List<string> ListFileName = CurrentDataContext.CurrentRequest.ListImage.ToList();

                NewEmail.SendMail(ListFileName);
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
