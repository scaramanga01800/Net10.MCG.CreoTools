using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.CreoInteractionTools.CREOExceptions;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.ShearedTube.Configuration;
using MCG.CREO_Tools.ShearedTube.Exceptions;
using MCG.CREO_Tools.ShearedTube.View;
using pfcls;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.ShearedTube.ViewModel
{
    public class ShearedTubeViewModel : ObservableObject, IShearedTubeViewModel
    {
        #region [REGION] Properties from Interface
        public ShearedTubeDataContext CurrentShearedTubeDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }

        private ShearedTubeConfiguration CurrentShearedTubeConfiguration { get; set; }
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
        public ICommand CommandCreateTube { get => new RelayCommand(() => ExecuteCreateTube()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly ICreoModelService _creoModelService;
        private readonly ICreoParameterService _creoParameterService;
        public ShearedTubeViewModel(ICreoSessionProvider creoSessionProvider,
                                    IXmlSerializeTools xmlSerializeTools,
                                    ICreoModelService creoModelService,
                                    ICreoParameterService creoParameterService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _xmlSerializeTools = xmlSerializeTools;
                _creoModelService = creoModelService;
                _creoParameterService = creoParameterService;

                CurrentShearedTubeDataContext = new ShearedTubeDataContext();

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentShearedTubeDataContext.IsCreoEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentShearedTubeDataContext.IsCreoEnable = e;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{ShearedTubeConstants.MainDictionary}", UriKind.Absolute);

                CurrentShearedTubeConfiguration = _xmlSerializeTools.GetDeserializedXml<ShearedTubeConfiguration>($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{ShearedTubeConstants.ConfigurationFile}");
                CurrentShearedTubeDataContext.CompleteListTube = CurrentShearedTubeConfiguration.CompleteListTube;
                foreach (string value in CurrentShearedTubeConfiguration.ListGroupCreator)
                    CurrentShearedTubeDataContext.ListGroupCreator.Add(value);
                foreach (string value in CurrentShearedTubeConfiguration.ListQualInspGrp)
                    CurrentShearedTubeDataContext.ListQualInspGroup.Add(value);
                CurrentShearedTubeDataContext.SelectedGroupCreator = CurrentShearedTubeDataContext.ListGroupCreator.FirstOrDefault();
                CurrentShearedTubeDataContext.SelectedQualInspGroup = CurrentShearedTubeDataContext.ListQualInspGroup.FirstOrDefault();
                CurrentShearedTubeDataContext.SelectedDiameter = CurrentShearedTubeDataContext.ListDiameter.FirstOrDefault();
                CurrentShearedTubeDataContext.SelectedThickness = CurrentShearedTubeDataContext.ListThickness.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ShearedTubeException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteBtHelpMouseLeftButtonUpEvent()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("ST_LinkHelpShearedTube"));
            }
            catch (Exception ex)
            {
                ShearedTubeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreateTube()
        {
            try
            {
                CreateUpdateOneTube();
            }
            catch (Exception ex)
            {
                ShearedTubeException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods Creo Interaction
        private void CreateUpdateOneTube()
        {
            try
            {
                IpfcModel CurrentIpfcModel;

                if (CurrentShearedTubeDataContext.Number != null && CurrentShearedTubeDataContext.Number.Trim() != "")
                {
                    try
                    {
                        // Try to retrieve the model, if exists
                        CurrentIpfcModel = _creoModelService.RetrieveModel(CurrentShearedTubeDataContext.Number, EpfcModelType.EpfcMDL_PART);
                        CurrentIpfcModel.Display();
                        UpdateTube(CurrentIpfcModel);
                    }
                    catch (CREORetrieveModelException)
                    {
                        // If not retrieve, create it
                        if (CurrentShearedTubeDataContext.ExtremityAngle == null || CurrentShearedTubeDataContext.ExtremityAngle.Trim() == "")
                            CurrentShearedTubeDataContext.ExtremityAngle = "0";
                        if (Convert.ToDouble(CurrentShearedTubeDataContext.ExtremityAngle) == 0)
                            CurrentIpfcModel = _creoModelService.CreatePartDrwFromLocal(ShearedTubeConstants.TemplateTubeWithoutAngle, CurrentShearedTubeDataContext.Number);
                        else
                            CurrentIpfcModel = _creoModelService.CreatePartDrwFromLocal( ShearedTubeConstants.TemplateTubeAngle, CurrentShearedTubeDataContext.Number);

                        CurrentIpfcModel = _creoModelService.RetrieveModel(CurrentShearedTubeDataContext.Number, EpfcModelType.EpfcMDL_PART);
                        CurrentIpfcModel.Display();
                        UpdateTube(CurrentIpfcModel);
                    }
                }
                else
                    MessageBox.Show(McgWpfTools.GetStringResource("ST_MsgEnterPartNumber"), McgWpfTools.GetStringResource("ST_MsgTitleEnterPartNumber"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                throw new ShearedTubeException(this.GetType().Name, ex);
            }
        }

        public double TryParseDouble(string input)
        {
            double result = 0;

            if (string.IsNullOrWhiteSpace(input))
                return result;

            // 1. Format standard (CREO / invariant)
            if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;

            // 2. Format français (virgule)
            if (double.TryParse(input, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out result))
                return result;

            return result;
        }

        private void UpdateTube(IpfcModel currentIpfcModel)
        {
            try
            {

                CREOModelStatus CurrentCREOModelStatus = _creoModelService.GetModelStatus(currentIpfcModel);
                if (CurrentCREOModelStatus == CREOModelStatus.READONLYITEM || CurrentCREOModelStatus == CREOModelStatus.READONLYWORKSPACE)
                    MessageBox.Show(McgWpfTools.GetStringResource("ST_ErrorMsgCantModify"),
                        McgWpfTools.GetStringResource("ST_ErrorMsgTitleSUpdateIssue"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning,
                        MessageBoxResult.OK);
                else
                {

                    string paramValue;
                    double doubleParamValue;
                    string paramName;
                    IpfcSolid SolidModel;
                    //CurrentCREOConnection.session.SetConfigOption("regen_failure_handling", "resolve_mode");
                    SolidModel = (IpfcSolid)currentIpfcModel;

                    // Update Angle between extremities
                    paramValue = CurrentShearedTubeDataContext.ExtremityAngle;
                    paramName = "TUBE_RIGHT_ORIENTATION";
                    _creoParameterService.SetParameter(currentIpfcModel, paramName, TryParseDouble(paramValue), false);

                    // Add Try-Catch block due to an unexcepted exception - Working on it to find the root cause.
                    try
                    {
                        SolidModel.Regenerate(null);
                    }
                    catch (Exception)
                    {
                    }

                    // Update Main Dimensions
                    doubleParamValue = CurrentShearedTubeDataContext.SelectedDiameter;
                    paramName = "TUBE_DIAMETER";
                    _creoParameterService.SetParameter(currentIpfcModel, paramName, doubleParamValue, false);

                    doubleParamValue = CurrentShearedTubeDataContext.SelectedThickness;
                    paramName = "TUBE_THICKNESS";
                    _creoParameterService.SetParameter(currentIpfcModel, paramName, doubleParamValue, false);

                    paramValue = CurrentShearedTubeDataContext.TotalLength;
                    paramName = "TUBE_LENGTH";
                    _creoParameterService.SetParameter(currentIpfcModel, paramName, TryParseDouble(paramValue), false);

                    // Add Try-Catch block due to an unexcepted exception - Working on it to find the root cause.
                    try
                    {
                        SolidModel.Regenerate(null);
                    }
                    catch (Exception)
                    {
                    }

                    // Update Left Cut
                    paramValue = CurrentShearedTubeDataContext.LeftAngle;
                    paramName = "TUBE_LEFT_CUT_ANGLE";
                    _creoParameterService.SetParameter(currentIpfcModel, paramName, TryParseDouble(paramValue), false);

                    // Add Try-Catch block due to an unexcepted exception - Working on it to find the root cause.
                    try
                    {
                        SolidModel.Regenerate(null);
                    }
                    catch (Exception)
                    {
                    }

                    // Update Right Cut
                    paramValue = CurrentShearedTubeDataContext.RightAngle;
                    paramName = "TUBE_RIGHT_CUT_ANGLE";
                    _creoParameterService.SetParameter(currentIpfcModel, paramName, TryParseDouble(paramValue), false);

                    // Add Try-Catch block due to an unexcepted exception - Working on it to find the root cause.
                    try
                    {
                        SolidModel.Regenerate(null);
                    }
                    catch (Exception)
                    {
                    }

                    // Update Hole Param
                    paramName = "TUBE_HOLE";
                    if (CurrentShearedTubeDataContext.IsHoleSelected)
                    {
                        paramValue = "YES";
                        paramName = "TUBE_HOLE";
                        _creoParameterService.SetParameter(currentIpfcModel, paramName, paramValue == "YES", false);

                        paramValue = CurrentShearedTubeDataContext.HoleDiameter;
                        paramName = "TUBE_HOLE_DIAMETER";
                        _creoParameterService.SetParameter(currentIpfcModel, paramName, TryParseDouble(paramValue), false);

                        paramValue = CurrentShearedTubeDataContext.HoleLength;
                        paramName = "TUBE_HOLE_POSITION";
                        _creoParameterService.SetParameter(currentIpfcModel, paramName, TryParseDouble(paramValue), false);
                    }
                    else
                    {
                        paramValue = "NO";
                        paramName = "TUBE_HOLE";
                        _creoParameterService.SetParameter(currentIpfcModel, paramName, paramValue == "NO", false);
                    }

                    // Add Try-Catch block due to an unexcepted exception - Working on it to find the root cause.
                    try
                    {
                        SolidModel.Regenerate(null);
                    }
                    catch (Exception)
                    {
                    }

                    // Update Parameters: DESCRIPTION, GROU CREATOR...
                    paramValue = CurrentShearedTubeDataContext.Description_2;
                    paramName = "DESCRIPTION_2";
                    _creoParameterService.SetParameter(currentIpfcModel, paramName, paramValue, true);

                    paramValue = CurrentShearedTubeDataContext.Description2_1;
                    paramName = "DESCRIPTION2_1";
                    _creoParameterService.SetParameter(currentIpfcModel, paramName, paramValue, true);

                    paramValue = CurrentShearedTubeDataContext.Description2_2;
                    paramName = "DESCRIPTION2_2";
                    _creoParameterService.SetParameter(currentIpfcModel, paramName, paramValue, true);

                    paramValue = CurrentShearedTubeDataContext.SelectedGroupCreator;
                    paramName = "GROUP_CREATOR";
                    _creoParameterService.SetParameter(currentIpfcModel, paramName, paramValue, true);

                    paramValue = CurrentShearedTubeDataContext.SelectedQualInspGroup;
                    paramName = "QUALINSPGRP";
                    _creoParameterService.SetParameter(currentIpfcModel, paramName, paramValue, true);

                    paramValue = McgActiveDirectoryTools.GetWindowsSessionUserShortName(); ;
                    paramName = "MODIFIED_BY";
                    _creoParameterService.SetParameter(currentIpfcModel, paramName, paramValue);

                    currentIpfcModel.RegeneratePostRegenerationRelations();

                    // Add Try-Catch block due to an unexcepted exception - Working on it to find the root cause.
                    try
                    {
                        SolidModel.Regenerate(null);
                    }
                    catch (Exception)
                    {
                    }

                    //CurrentCREOConnection.session.SetConfigOption("regen_failure_handling", "no_resolve_mode");
                    currentIpfcModel.Display();
                    currentIpfcModel.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(McgWpfTools.GetStringResource("ST_ErrorMsgUpdateIssue"), ex.Message), McgWpfTools.GetStringResource("ST_ErrorMsgTitleSUpdateIssue"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
