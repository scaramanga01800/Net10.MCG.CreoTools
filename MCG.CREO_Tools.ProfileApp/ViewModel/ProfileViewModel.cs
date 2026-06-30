using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WebtermLib.Models;
using MCG.CommonLib.WebtermLib.Services.Interfaces;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CREO_Tools.ProfileApp.Configuration;
using MCG.CREO_Tools.ProfileApp.Exceptions;
using MCG.CREO_Tools.ProfileApp.Interfaces;
using MCG.CREO_Tools.ProfileApp.View;
using pfcls;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.ProfileApp.ViewModel
{
    public class ProfileViewModel : ObservableObject, IProfileViewModel
    {
        #region [REGION] Properties from Interface
        public ProfileDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private MCGLanguage CurrentMcgLanguage { get; set; } 
        private ProfileConfiguration CurrentAppProfileConfiguration { get; set; }
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
        public ICommand CommandCreateProfile { get => new RelayCommand(() => ExecuteCreateProfile()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        public ICommand CommandEditProfile { get => new RelayCommand(() => ExecuteEditProfile()); }
        public ICommand CommandAddNewProfile { get => new RelayCommand<bool>((b) => ExecuteAddNewProfile(b)); }
        #endregion

        #region [REGION] Init
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoParameterService _creoParameterService;
        private readonly ICreoFeatureService _creoFeatureService;
        private readonly ICreoModelService _creoModelService;
        private readonly IUserAuthorizationService _userAuthorizationService;
        private readonly IProfileAppService _profileAppService;
        private readonly IProfileAppWindowService _profileAppWindowService;
        private readonly IMcgToolDictionary _mcgToolDictionary;
        private readonly IWebtermTools _webtermTools;
        private readonly ISharedAppContext _sharedAppContext;


        public ProfileViewModel(IXmlSerializeTools xmlSerializeTools,
                                ICreoSessionProvider creoSessionProvider,
                                ICreoParameterService creoParameterService,
                                ICreoFeatureService creoFeatureService,
                                ICreoModelService creoModelService,
                                IUserAuthorizationService userAuthorizationService,
                                IProfileAppService profileAppService,
                                IProfileAppWindowService profileAppWindowService,
                                IMcgToolDictionary mcgToolDictionary,
                                IWebtermTools webtermTools,
                                ISharedAppContext sharedAppContext)
        {
            try
            {
                _xmlSerializeTools = xmlSerializeTools;
                _creoSessionProvider = creoSessionProvider;
                _creoParameterService = creoParameterService;
                _creoFeatureService = creoFeatureService;
                _creoModelService = creoModelService;
                _userAuthorizationService = userAuthorizationService;
                _profileAppService = profileAppService;
                _profileAppWindowService = profileAppWindowService;
                _mcgToolDictionary = mcgToolDictionary;
                _webtermTools = webtermTools;
                _sharedAppContext = sharedAppContext;

                CurrentDataContext = new ProfileDataContext();

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;


                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDataContext.IsCreoEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDataContext.IsCreoEnable = e;


                CurrentMcgLanguage = _sharedAppContext.CurrentLanguage?.Language;
                if (CurrentMcgLanguage != null)
                    CurrentMcgLanguage.ChangeLanguageInterface += UpdateInterfaceLanguage;

                CurrentAppProfileConfiguration = _xmlSerializeTools.GetDeserializedXml<ProfileConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{ProfileAppConstants.ConfigurationFile}");

                CurrentDataContext.ChangeProfileTypeEvent += UpdateListProfileGenericFromSelectedType;
                CurrentDataContext.ChangeProfileTypeEvent += UpdateFastenerImageFromDb;
                CurrentDataContext.ChangeMaterialEvent += UpdateListProfileShown;

                ReadDataAllProfile();
                UpdateListProfileType();

                if (CurrentAppProfileConfiguration.ListDrwLocation != null)
                {
                    foreach (var item in CurrentAppProfileConfiguration.ListDrwLocation)
                        CurrentDataContext.ListDrwLocation.Add(item);
                    CurrentDataContext.SelectedDrwLocation = CurrentDataContext.ListDrwLocation.FirstOrDefault();
                }

                CurrentDataContext.ListDrwScale = CurrentAppProfileConfiguration.ListDrwScale;

                if (CurrentAppProfileConfiguration.ListGrpCreator != null)
                {
                    foreach (var item in CurrentAppProfileConfiguration.ListGrpCreator)
                        CurrentDataContext.ListGrpCreator.Add(item);
                }

                ActionInProgressEvent += (sender, e) => CurrentDataContext.ActionInProgress = true;
                ActionDoneEvent += (sender, e) => CurrentDataContext.ActionInProgress = false;

                CurrentDataContext.IsAdminToolsEnabled = CheckUserAuthorization(ProfileAppConstants.KeyUserUpdateAppName);
            }
            catch (Exception ex)
            {
                ProfileException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateInterfaceLanguage(object sender, EventArgs e)
        {
            try
            {
                // Translate with selected language all Profile Type
                foreach (var elem in CurrentDataContext.ListProfileType)
                    elem.DescriptionShown = _mcgToolDictionary.GetTerm(elem.OrigProfileType.Description);

                var tempList = CurrentDataContext.ListProfileType.OrderBy((item) => item.DescriptionShown).ToList();

                CurrentDataContext.ListProfileType.Clear();
                foreach (var elem in tempList)
                    CurrentDataContext.ListProfileType.Add(elem);

                CurrentDataContext.CurrentProfileType = CurrentDataContext.ListProfileType.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ProfileException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public bool CheckUserAuthorization(string AppName)
        {
            try
            {
                if (AppName == null) AppName = "";
                return _userAuthorizationService.GetAppAuthorization(Environment.UserName.ToUpper(), AppName.ToUpper());
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteCreateProfile()
        {
            try
            {
                RaiseActionInProgressEvent();
                Thread ThreadSearchPart = new Thread(() => StartCreateProfileAsynch());
                ThreadSearchPart.Start();
            }
            catch (Exception ex)
            {
                ProfileException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenHelp()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("APR_LinkHelpAppProfile"));

            }
            catch (Exception ex)
            {
                ProfileException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteEditProfile()
        {
            try
            {
                ProfileGenericItem CurrentProfile = CurrentDataContext.SelectedItem;
                CurrentProfile.UpdatedProfile = new ProfileGenericItem();
                ProfileGenericItem.UpdateProfileGenericItem(CurrentProfile.UpdatedProfile, CurrentProfile);
                bool IsCreationOk = false;

                while (!IsCreationOk)
                {

                    var returnWindow = _profileAppWindowService.ShowDialogProfileUpdateProfileView(CurrentProfile, CurrentAppProfileConfiguration);

                    if (returnWindow == MessageBoxResult.Yes)
                    {

                        if (CurrentProfile.UpdatedProfile.CheckProfileItem())
                        {
                            ProfileGenericItem.UpdateProfileGenericItem(CurrentProfile, CurrentProfile.UpdatedProfile);

                            var dbProfile = _profileAppService.GetProfileGeneric(CurrentProfile.OrigPartNumber, CurrentProfile.IdType);

                            if (dbProfile != null)
                            {
                                ProfileGenericItem.UpdateProfileGenericItem(dbProfile, CurrentProfile.UpdatedProfile);
                                _profileAppService.UpdateProfileGeneric(dbProfile);

                                IsCreationOk = true;
                                string CurrentType = CurrentDataContext.CurrentProfileType.OrigProfileType.Idtype;

                                ReadDataAllProfile();
                                UpdateListProfileType();

                                CurrentDataContext.CurrentProfileType = CurrentDataContext.ListProfileType.FirstOrDefault(item => item.OrigProfileType.Idtype == CurrentType);
                            }
                        }
                        else
                        {
                            MessageBox.Show(McgWpfTools.GetStringResource("APR_ErrorMsgNumberBlank"), McgWpfTools.GetStringResource("APR_ErrorMsgTitleIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                        IsCreationOk = true;
                }
            }
            catch (Exception ex)
            {
                ProfileException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteAddNewProfile(bool FromSelected = false)
        {
            try
            {
                if (CurrentDataContext.CurrentProfileType != null)
                {
                    ProfileGenericItem CurrentProfile = new ProfileGenericItem();
                    if (FromSelected)
                        ProfileGenericItem.UpdateProfileGenericItem(CurrentProfile, CurrentDataContext.SelectedItem);
                    else
                    {
                        CurrentProfile.TemplateProfileType = CurrentDataContext.CurrentProfileType;
                        CurrentProfile.IdType = CurrentDataContext.CurrentProfileType.OrigProfileType.Idtype;
                    }

                    CurrentProfile.UpdatedProfile = new ProfileGenericItem();
                    ProfileGenericItem.UpdateProfileGenericItem(CurrentProfile.UpdatedProfile, CurrentProfile);

                    bool IsCreationOk = false;

                    while (!IsCreationOk)
                    {
                        var returnWindow = _profileAppWindowService.ShowDialogProfileUpdateProfileView(CurrentProfile, CurrentAppProfileConfiguration);

                        if (returnWindow == MessageBoxResult.Yes)
                        {
                            if (CurrentProfile.UpdatedProfile.CheckProfileItem())
                            {
                                ProfileGenericItem.UpdateProfileGenericItem(CurrentProfile, CurrentProfile.UpdatedProfile);

                                var existingProfile = _profileAppService.GetProfileGeneric(CurrentProfile.PartNumber, CurrentProfile.IdType);

                                if (existingProfile != null)
                                    MessageBox.Show(McgWpfTools.GetStringResource("APR_ErrorMsgPartExist"), McgWpfTools.GetStringResource("APR_ErrorMsgTitlePartExist"), MessageBoxButton.OK, MessageBoxImage.Warning);
                                else
                                {
                                    var newDbProfile = CurrentProfile.GetProfilGenericDb();
                                    _profileAppService.AddProfileGeneric(newDbProfile);

                                    IsCreationOk = true;
                                    string CurrentType = CurrentDataContext.CurrentProfileType.OrigProfileType.Idtype;
                                    ReadDataAllProfile();
                                    UpdateListProfileType();
                                    CurrentDataContext.CurrentProfileType = CurrentDataContext.ListProfileType.FirstOrDefault(item => item.OrigProfileType.Idtype == CurrentType);
                                }
                            }
                            else
                            {
                                MessageBox.Show(McgWpfTools.GetStringResource("APR_ErrorMsgNumberBlank"), McgWpfTools.GetStringResource("APR_ErrorMsgTitleIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                            IsCreationOk = true;
                    }
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("APR_ErrorMsgClassNotSelected"), McgWpfTools.GetStringResource("APR_ErrorMsgTitleIssue"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                ProfileException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION]  Methods Search Data
        private void ReadDataAllProfile()
        {
            try
            {
                CurrentDataContext.AllListProfileGeneric = _profileAppService.GetAllProfileGeneric();
                CurrentDataContext.AllListProfileType = _profileAppService.GetAllProfileType();
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }

        private void UpdateListProfileType()
        {
            try
            {
                CurrentDataContext.CurrentProfileType = null;

                ProfileTypeItem CurrentType;
                List<ProfileTypeItem> TempListType = new List<ProfileTypeItem>();

                foreach (var item in CurrentDataContext.AllListProfileType)
                {
                    CurrentType = new ProfileTypeItem()
                    {
                        OrigProfileType = item,
                        DescriptionShown = _mcgToolDictionary.GetTerm(item.Description)
                    };
                    TempListType.Add(CurrentType);
                }

                CurrentDataContext.ListProfileType.Clear();
                foreach (var elem in TempListType.OrderBy((item) => item.DescriptionShown))
                    CurrentDataContext.ListProfileType.Add(elem);

                if (CurrentDataContext.CurrentProfileType == null)
                    CurrentDataContext.CurrentProfileType = CurrentDataContext.ListProfileType.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }

        private void UpdateListProfileGenericFromSelectedType(object sender = null, EventArgs e = null)
        {
            try
            {
                if (CurrentDataContext.CurrentProfileType != null && CurrentDataContext.CurrentProfileType.OrigProfileType != null)
                {
                    CurrentDataContext.ListProfileGenericFromSelectedType = new List<ProfileGenericItem>();

                    List<Profilegeneric> currentList = CurrentDataContext.AllListProfileGeneric.Where((item) => item.Idtype == CurrentDataContext.CurrentProfileType.OrigProfileType.Idtype).ToList();

                    ProfileGenericItem CurrentGeneric;
                    if (currentList != null)
                    {
                        foreach (var item in currentList)
                        {
                            CurrentGeneric = new ProfileGenericItem()
                            {
                                OrigProfileGeneric = item,
                                OrigPartNumber = item.Partnumber
                            };
                            CurrentGeneric.TemplateProfileType = CurrentDataContext.CurrentProfileType;
                            CurrentDataContext.ListProfileGenericFromSelectedType.Add(CurrentGeneric);
                        }
                    }

                    List<string> allMaterial = currentList.Select((item) => item.Material).Distinct().ToList();

                    CurrentDataContext.ListMaterial.Clear();
                    foreach (var mat in allMaterial.OrderBy((item) => item))
                        CurrentDataContext.ListMaterial.Add(mat);

                    if (CurrentDataContext.ListMaterial.Count > 0)
                        CurrentDataContext.SelectedMaterial = CurrentDataContext.ListMaterial.FirstOrDefault();

                    UpdateListProfileShown();
                }
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }

        private void UpdateListProfileShown(object sender = null, EventArgs e = null)
        {
            try
            {
                CurrentDataContext.ListProfileShown.Clear();

                List<ProfileGenericItem> currentListMaterialFilter = null;

                if (CurrentDataContext.SelectedMaterial != null)
                    currentListMaterialFilter = CurrentDataContext.ListProfileGenericFromSelectedType.Where((item) => item.OrigProfileGeneric.Material == CurrentDataContext.SelectedMaterial).ToList();

                if (currentListMaterialFilter != null)
                {
                    foreach (var elem in currentListMaterialFilter)
                    {
                        elem.TemplateProfileType = CurrentDataContext.CurrentProfileType;
                        CurrentDataContext.ListProfileShown.Add(elem);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }

        [Obsolete("Replace by Image from SQL DB")]
        private void UpdateFastenerImage(object sender = null, EventArgs e = null)
        {
            try
            {
                if (CurrentDataContext.CurrentProfileType != null && CurrentDataContext.CurrentProfileType.OrigProfileType != null)
                {
                    string FasternerImageFile = $@"{MainAppFolder}\{McgMiscTools.GetAppSetting(this, "PictureFolder")}\{CurrentDataContext.CurrentProfileType.OrigProfileType.Picture}";
                    if (System.IO.File.Exists(FasternerImageFile))
                        CurrentDataContext.ProfileTypeImage = McgWpfTools.GetBitmapImage(FasternerImageFile);
                    else
                        CurrentDataContext.ProfileTypeImage = McgWpfTools.GetBitmapImage($@"{MainAppFolder}\{McgMiscTools.GetAppSetting(this, "PictureFolder")}\{McgMiscTools.GetAppSetting(this, "PictureNotFound")}");
                }
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }

        private void UpdateFastenerImageFromDb(object sender = null, EventArgs e = null)
        {
            try
            {
                if (CurrentDataContext.CurrentProfileType != null && CurrentDataContext.CurrentProfileType.OrigProfileType != null && CurrentDataContext.CurrentProfileType.OrigProfileType.Picturebin != null)
                    CurrentDataContext.ProfileTypeImageFromDb = CurrentDataContext.CurrentProfileType.OrigProfileType.Picturebin;
                else
                {
                    CurrentDataContext.ProfileTypeImageFromDb = McgWpfTools.GetImageToByte($@"{MainAppFolder}\{CommonLibConstants.PictureFolder}\{CommonLibConstants.PictureNotFound}");
                }
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION]  CREO Interaction


        private void StartCreateProfileAsynch()
        {
            try
            {
                if (CurrentDataContext.CurrentPartNumber.Trim() == "")
                    MessageBox.Show(McgWpfTools.GetStringResource("APR_MsgPartNumberMissing"), McgWpfTools.GetStringResource("APR_MsgWinTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                else if (CurrentDataContext.SelectedItem == null)
                    MessageBox.Show(McgWpfTools.GetStringResource("APR_MsgPartSelectedGenMissing"), McgWpfTools.GetStringResource("APR_MsgWinTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                else if (CurrentDataContext.SelectedMaterial == null || CurrentDataContext.SelectedMaterial == "")
                    MessageBox.Show(McgWpfTools.GetStringResource("APR_MsgMaterialMissing"), McgWpfTools.GetStringResource("APR_MsgWinTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                {
                    _creoSessionProvider.CheckConnection();

                    if (_creoModelService.CheckModelAlreadyInSession(string.Concat(CurrentDataContext.CurrentPartNumber, ".PRT")))
                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("APR_MsgPartAlreadyInSession"), string.Concat(CurrentDataContext.CurrentPartNumber, ".PRT")), McgWpfTools.GetStringResource("APR_MsgWinTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    else if (_creoModelService.CheckModelAlreadyInSession(string.Concat(CurrentDataContext.CurrentPartNumber, ".DRW")))
                        MessageBox.Show(string.Format(McgWpfTools.GetStringResource("APR_MsgPartAlreadyInSession"), string.Concat(CurrentDataContext.CurrentPartNumber, ".DRW")), McgWpfTools.GetStringResource("APR_MsgWinTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    else
                    {
                        var profileItem = new ProfileCreatedItem()
                        {
                            GenericItem = CurrentDataContext.SelectedItem,
                            PartNumber = CurrentDataContext.CurrentPartNumber,
                            Length = CurrentDataContext.CurrentLength,
                            Material = CurrentDataContext.SelectedMaterial,
                            GroupCreator = CurrentDataContext.SelectedGrpCreator,
                            IsDrwBrokenView = CurrentDataContext.IsDrwBrokenView,
                            ScaleDrawing = CurrentDataContext.MainDrawingScale,
                            IsoViewName = ProfileAppConstants.IsoViewName,
                            IsoViewScale = CurrentDataContext.ThreeDViewScale,
                            CurrentLang = CurrentDataContext.SelectedDrwLocation,
                            UseWindchillTemplate = ProfileAppConstants.UseWindchillTemplate,
                            LocalTemplateDir = $@"{MainAppFolder}\{ProfileAppConstants.LocalTemplateFolder}\",
                            CreatedBy = McgActiveDirectoryTools.GetWindowsSessionUserShortName()
                        };

                        // Appel direct dans le ViewModel
                        var status = CreateProfile(profileItem);

                        if (status != CREOModelStatus.OK)
                            MessageBox.Show($"Profile creation issue: {status}",
                                McgWpfTools.GetStringResource("APR_MsgWinTitleWarning"),
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }


        //private void StartCreateProfileAsynch()
        //{
        //    try
        //    {
        //        if (CurrentDataContext.CurrentPartNumber.Trim() == "")
        //            MessageBox.Show(McgWpfTools.GetStringResource("APR_MsgPartNumberMissing"), McgWpfTools.GetStringResource("APR_MsgWinTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
        //        else if (CurrentDataContext.SelectedItem == null)
        //            MessageBox.Show(McgWpfTools.GetStringResource("APR_MsgPartSelectedGenMissing"), McgWpfTools.GetStringResource("APR_MsgWinTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
        //        else if (CurrentDataContext.SelectedMaterial == null && CurrentDataContext.SelectedMaterial == "")
        //            MessageBox.Show(McgWpfTools.GetStringResource("APR_MsgMaterialMissing"), McgWpfTools.GetStringResource("APR_MsgWinTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
        //        else
        //        {
        //            CurrentCREOConnection.CheckCREOCnx();
        //            if (CurrentCREOConnection.CheckModelAlreadyInSession(string.Concat(CurrentDataContext.CurrentPartNumber, ".PRT")))
        //                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("APR_MsgPartAlreadyInSession"), string.Concat(CurrentDataContext.CurrentPartNumber, ".PRT")), McgWpfTools.GetStringResource("APR_MsgWinTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
        //            else if (CurrentCREOConnection.CheckModelAlreadyInSession(string.Concat(CurrentDataContext.CurrentPartNumber, ".DRW")))
        //                MessageBox.Show(string.Format(McgWpfTools.GetStringResource("APR_MsgPartAlreadyInSession"), string.Concat(CurrentDataContext.CurrentPartNumber, ".DRW")), McgWpfTools.GetStringResource("APR_MsgWinTitleWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
        //            else
        //            {
        //                ProfileCreatedItem CurrentAppProfileCreatedItem = new ProfileCreatedItem()
        //                {
        //                    GenericItem = CurrentDataContext.SelectedItem,
        //                    PartNumber = CurrentDataContext.CurrentPartNumber,
        //                    Length = CurrentDataContext.CurrentLength,
        //                    Material = CurrentDataContext.SelectedMaterial,
        //                    GroupCreator = CurrentDataContext.SelectedGrpCreator,
        //                    IsDrwBrokenView = CurrentDataContext.IsDrwBrokenView,
        //                    ScaleDrawing = CurrentDataContext.MainDrawingScale,
        //                    IsoViewName = ProfileAppConstants.IsoViewName,
        //                    IsoViewScale = CurrentDataContext.ThreeDViewScale,
        //                    CurrentLang = CurrentDataContext.SelectedDrwLocation,
        //                    UseWindchillTemplate = ProfileAppConstants.UseWindchillTemplate,
        //                    LocalTemplateDir = $@"{MainAppFolder}\{ProfileAppConstants.LocalTemplateFolder}\",
        //                    CreatedBy = McgActiveDirectoryTools.GetWindowsSessionUserShortName()
        //                };

        //                CurrentAppProfileCreatedItem.CreateProfile(CurrentCREOConnection);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ProfileException(this.GetType().Name, ex);
        //    }
        //    finally
        //    {
        //        RaiseActionDoneEvent();
        //    }
        //}



        private CREOModelStatus CreateProfile(ProfileCreatedItem profileItem)
        {
            try
            {
                var session = _creoSessionProvider.Session;
                string localTemplateDir = profileItem.UseWindchillTemplate ? "" : profileItem.LocalTemplateDir;
                var origGeneric = profileItem.GenericItem.OrigProfileGeneric;

                // ── Step 1: Retrieve drawing template ──────────────────
                IpfcModel drwTemplateModel;
                try
                {
                    string drwName = profileItem.IsDrwBrokenView
                        ? origGeneric.Drwnumberbrokenview
                        : origGeneric.Drwnumbercompleteview;

                    drwTemplateModel = _creoModelService.RetrieveModel($"{localTemplateDir}{drwName}_{profileItem.CurrentLang.DrwSuffix}",
                        EpfcModelType.EpfcMDL_DRAWING);

                    if (drwTemplateModel == null)
                        return CREOModelStatus.RETRIEVEISSUE;
                }
                catch (Exception)
                {
                    return CREOModelStatus.RETRIEVEISSUE;
                }

                // ── Step 2: Copy drawing template with new number (rename) ─
                try
                {
                    drwTemplateModel.Rename(profileItem.PartNumber, true);
                }
                catch (Exception)
                {
                    return CREOModelStatus.RENAMEISSUE;
                }

                // ── Step 3: Replace drawing model with the right one ───
                IpfcModel generic3DModel;
                IpfcModel instance3DModel;
                try
                {
                    generic3DModel = _creoModelService.RetrieveModel($"{localTemplateDir}{origGeneric.Profilegeneric1}", EpfcModelType.EpfcMDL_PART);

                    if (profileItem.UseWindchillTemplate)
                        instance3DModel = _creoModelService.RetrieveModel(origGeneric.Partnumber, EpfcModelType.EpfcMDL_PART);
                    else
                        instance3DModel = _creoModelService.RetrieveModel($"{localTemplateDir}{origGeneric.Partnumber}<{origGeneric.Profilegeneric1}>.PRT", EpfcModelType.EpfcMDL_PART);

                    _creoModelService.ReplaceModelDrw(drwTemplateModel, generic3DModel, instance3DModel);
                }
                catch (Exception)
                {
                    return CREOModelStatus.REPLACEMODELDRWISSUE;
                }

                // ── Step 4: Remove instance from family table to cut link ─
                try
                {
                    IpfcFamilyMember genericFamily = (IpfcFamilyMember)generic3DModel;
                    IpfcFamilyTableRows allRows = genericFamily.ListRows();
                    string instanceName = origGeneric.Partnumber.Split('.').First();

                    for (int index = 0; index < allRows.Count; index++)
                    {
                        if (allRows[index].InstanceName.ToUpper() == instanceName.ToUpper())
                        {
                            genericFamily.RemoveRow(allRows[index]);
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    return CREOModelStatus.SESSIONISSUE;
                }

                // ── Step 5: Remove generic from session ────────────────
                try
                {
                    generic3DModel.Erase();
                }
                catch (Exception)
                {
                    return CREOModelStatus.SESSIONISSUE;
                }

                // ── Step 6: Rename instance with new part number ───────
                try
                {
                    instance3DModel.Rename(profileItem.PartNumber, true);
                }
                catch (Exception)
                {
                    return CREOModelStatus.SESSIONISSUE;
                }

                // ── Step 7: Change dimensions and attributes ───────────
                try
                {
                    _creoFeatureService.SetDimensionValues(instance3DModel, "Length", profileItem.Length);

                    IpfcSolid model3D = (IpfcSolid)instance3DModel;

                    _creoFeatureService.AssignMaterial(instance3DModel, profileItem.Material);

                    // Update Param Descriptions via Webterm service
                    var origType = profileItem.GenericItem.TemplateProfileType.OrigProfileType;

                    string descLocal = _webtermTools.GetTerm(
                        origType.Paramdescen,
                        WebtermLanguage.ENGLISH,
                        _webtermTools.GetWebtermLanguage(profileItem.CurrentLang.WebtermLang));

                    string descDetailEn = origType.Paramdescdetailen
                        .Replace("LENGTH", profileItem.Length.ToString())
                        .Replace("WIDTH", origGeneric.Width.ToString())
                        .Replace("HEIGHT", origGeneric.Height.ToString())
                        .Replace("THICKNESS", origGeneric.Thickness.ToString());

                    string descDetailLocal = origType.Paramdescdetaillocal
                        .Replace("LENGTH", profileItem.Length.ToString())
                        .Replace("WIDTH", origGeneric.Width.ToString())
                        .Replace("HEIGHT", origGeneric.Height.ToString())
                        .Replace("THICKNESS", origGeneric.Thickness.ToString());

                    _creoParameterService.SetParameter(instance3DModel, "DESCRIPTION_2", descDetailEn, true);
                    _creoParameterService.SetParameter(instance3DModel, "DESCRIPTION2_1", descLocal, true);
                    _creoParameterService.SetParameter(instance3DModel, "DESCRIPTION2_2", descDetailLocal, true);
                    _creoParameterService.SetParameter(instance3DModel, "GROUP_CREATOR", profileItem.GroupCreator, true);
                    _creoParameterService.SetParameter(instance3DModel, "MODIFIED_BY", profileItem.CreatedBy, true);

                    model3D.Regenerate(null);
                }
                catch (Exception)
                {
                    return CREOModelStatus.UPDATE3DISSUE;
                }

                // ── Step 8: Update scale of the drawings ───────────────
                IpfcWindow currentIpfcWindow = null;
                IpfcWindows allIpfcWindows = null;

                try
                {
                    allIpfcWindows = session.ListWindows();
                    if (allIpfcWindows.Count < 18)
                    {
                        currentIpfcWindow = session.CreateModelWindow(drwTemplateModel);
                        drwTemplateModel.Display();
                    }
                    else
                    {
                        drwTemplateModel.Display();
                        for (int index = 0; index < allIpfcWindows.Count; index++)
                        {
                            currentIpfcWindow = allIpfcWindows[index];
                            if (currentIpfcWindow.Model.FileName == drwTemplateModel.FileName)
                                currentIpfcWindow.Activate();
                        }
                    }
                }
                catch (Exception) { }

                if (currentIpfcWindow != null)
                    currentIpfcWindow.Activate();

                try
                {
                    // Main scale
                    IpfcSheetOwner drwSheetOwner = (IpfcSheetOwner)drwTemplateModel;
                    drwSheetOwner.SetSheetScale(1, profileItem.ScaleDrawing, null);

                    // ISO View Scale
                    IpfcView2D isoView = ((IpfcModel2D)drwTemplateModel).GetViewByName(profileItem.IsoViewName);
                    if (isoView != null)
                        isoView.Scale = profileItem.IsoViewScale;
                }
                catch (Exception)
                {
                    return CREOModelStatus.UPDATEDRWISSUE;
                }

                // ── Step 9: Show 3D model ─────────────────────────────
                try
                {
                    allIpfcWindows = session.ListWindows();
                    if (allIpfcWindows.Count < 18)
                    {
                        currentIpfcWindow = session.CreateModelWindow(instance3DModel);
                        instance3DModel.Display();
                    }
                    else
                    {
                        instance3DModel.Display();
                        for (int index = 0; index < allIpfcWindows.Count; index++)
                        {
                            currentIpfcWindow = allIpfcWindows[index];
                            if (currentIpfcWindow.Model.FileName == instance3DModel.FileName)
                                currentIpfcWindow.Activate();
                        }
                    }
                }
                catch (Exception) { }

                if (currentIpfcWindow != null)
                    currentIpfcWindow.Activate();

                return CREOModelStatus.OK;
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }




        #endregion
    }
}
