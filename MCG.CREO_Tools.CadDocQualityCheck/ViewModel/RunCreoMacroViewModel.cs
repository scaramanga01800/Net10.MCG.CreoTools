using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CREO_Tools.CadDocQualityCheck.Exceptions;
using MCG.CREO_Tools.CadDocQualityCheck.View;
using pfcls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class RunCreoMacroViewModel : ObservableObject, IRunCreoMacroViewModel
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
        public RunCreoMacroDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private Dispatcher MainDispatcher { get; set; }
        #endregion

        #region [REGION] Commands
        public ICommand CommandPaste { get => new RelayCommand<KeyEventArgs>((obj) => ExecuteCommandPaste(obj)); }
        public ICommand CommandMenuPaste { get => new RelayCommand(() => ExecuteCommandMenuPaste()); }
        public ICommand CommandStart { get => new RelayCommand(() => ExecuteCommandStart()); }
        public ICommand CommandDeleteItem { get => new RelayCommand(() => { CurrentDataContext.ShownCadModels.Clear(); }); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        private readonly ICreoMacroService _creoMacroService;
        public RunCreoMacroViewModel(ICreoSessionProvider creoSessionProvider,
                                     ICreoModelService creoModelService,
                                     ICreoMacroService creoMacroService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;
                _creoMacroService = creoMacroService;

                CurrentDataContext = new RunCreoMacroDataContext();

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                MainDispatcher = Dispatcher.CurrentDispatcher;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDataContext.IsEnabledCreo = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDataContext.IsEnabledCreo = e;

                ActionInProgressEvent += (sender, e) => CurrentDataContext.IsEnabledActionButton = false;
                ActionDoneEvent += (sender, e) => CurrentDataContext.IsEnabledActionButton = true;
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteCommandPaste(KeyEventArgs e = null)
        {
            try
            {
                if (e == null || (Keyboard.Modifiers == ModifierKeys.Control && e != null && e.Key == Key.V))
                {
                    GetItemFromClipboard();
                }
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCommandMenuPaste()
        {
            try
            {
                GetItemFromClipboard();
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCommandStart()
        {
            try
            {
                RaiseActionInProgressEvent();
                Thread ThreadSearchPart = new Thread(() => UpdateAllItemAsynch());
                ThreadSearchPart.Start();
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc
        private void GetItemFromClipboard()
        {
            try
            {
                List<CadDocQualityCheckItem> ListItemInProgress = new List<CadDocQualityCheckItem>();

                string CompleteString = null;
                if (Clipboard.GetData(DataFormats.Text) != null)
                    CompleteString = Clipboard.GetData(DataFormats.Text).ToString();

                if (CompleteString != null)
                {
                    var AllLines = CompleteString.Split('\n');

                    CadDocQualityCheckItem NewValue = null;
                    string linePurged = null;
                    string TempNumber;

                    foreach (var line in AllLines)
                    {
                        linePurged = line.Split('\r').FirstOrDefault();
                        var AllValues = linePurged.Split('\t');
                        if (AllValues != null && AllValues.Count() > 0)
                        {
                            TempNumber = AllValues[0].Trim().ToUpper();
                            if (TempNumber != "" && CurrentDataContext.ShownCadModels.FirstOrDefault(item => item.Number == TempNumber) == null)
                            {
                                NewValue = new CadDocQualityCheckItem()
                                {
                                    Number = AllValues[0].Trim().ToUpper()
                                };
                                CurrentDataContext.ShownCadModels.Add(NewValue);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }

        private void UpdateAllItemAsynch()
        {
            try
            {
                foreach (var item in CurrentDataContext.ShownCadModels)
                {
                    UpdateAllItemAsynch(item);
                }
            }
            catch (Exception ex)
            {
                CadDocQualityCheckException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
            }
        }

        private void UpdateAllItemAsynch(CadDocQualityCheckItem CurrentItem)
        {
            try
            {
                if(CurrentItem!=null && CurrentItem.Number!=null)
                {
                    IpfcModel CurrentCadModel = null;
                    bool Is3DModel = true;
                    if (CurrentItem.Number.ToUpper().IndexOf(".DRW") > 0)
                    {
                        Is3DModel = false;
                        CurrentCadModel = _creoModelService.RetrieveModel(CurrentItem.Number, EpfcModelType.EpfcMDL_DRAWING);
                    }
                    else if (CurrentItem.Number.ToUpper().IndexOf(".ASM") > 0)
                    {
                        Is3DModel = true;
                        CurrentCadModel = _creoModelService.RetrieveModel( CurrentItem.Number, EpfcModelType.EpfcMDL_ASSEMBLY);
                    }
                    else if (CurrentItem.Number.ToUpper().IndexOf(".PRT") > 0)
                    {
                        Is3DModel = true;
                        CurrentCadModel = _creoModelService.RetrieveModel(CurrentItem.Number, EpfcModelType.EpfcMDL_PART);
                    }

                    if(CurrentCadModel != null)
                    {
                        if(Is3DModel)
                        {
                            CurrentCadModel.DisplayInNewWindow();
                            bool IsDisplayedWindow = _creoModelService.ActiveCadDocWindow(CurrentCadModel);
                            try
                            {
                                _creoSessionProvider.Session.RunMacro(CurrentDataContext.Macro);
                                Thread.Sleep(1000);
                                CCpfcRegenInstructions CreateInstrustions = new CCpfcRegenInstructions();
                                IpfcRegenInstructions Instruction = CreateInstrustions.Create(false, true, null);
                                Instruction.UpdateInstances = true;
                                ((IpfcSolid)CurrentCadModel).Regenerate(Instruction);
                                CurrentCadModel.Save();
                                CurrentItem.Status = "Update done";
                            }
                            catch (Exception)
                            {
                                CurrentItem.Status = "Issue to run macro";
                            }
                            IpfcWindow CurrentWindow = _creoModelService.GetCadDocWindow(CurrentCadModel);
                            CurrentWindow.Close();
                        }
                        else
                        {
                            CurrentItem.Status = "Not a 3D model";
                        }

                    }
                    else
                    {
                        CurrentItem.Status = "Issue to retrieve model";
                    }

                }
            }
            catch (Exception ex)
            {
              throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
