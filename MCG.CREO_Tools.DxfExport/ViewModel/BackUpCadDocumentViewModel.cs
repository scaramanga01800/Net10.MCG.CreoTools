using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.DxfExport.Exceptions;
using MCG.CREO_Tools.DxfExport.View;
using pfcls;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.DxfExport.ViewModel
{
    public class BackUpCadDocumentViewModel: ObservableObject, IBackUpCadDocumentViewModel
    {
        #region [REGION] Properties from Interface
        public BackUpCadDocumentViewDataContext CurrentDatacontext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder;
        private Dispatcher MainDispatcher = null;
        private bool StopCurrentExport = false;
        private bool isInProgress = false;
        #endregion

        #region [REGION] Commands
        public ICommand CommandCadDocumentBackup { get => new RelayCommand(() => ExecuteCadDocumentBackup()); }
        public ICommand CommandOpenFolder { get => new RelayCommand(() => ExecuteOpenFolder()); }
        public ICommand CommandPaste { get => new RelayCommand<KeyEventArgs>((obj) => ExecuteCommandPaste(obj)); }
        public ICommand CommandResetList { get => new RelayCommand(() => ExecuteCommandResetList()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        public BackUpCadDocumentViewModel(ICreoSessionProvider creoSessionProvider,
                                          ICreoModelService creoModelService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;

                CurrentDatacontext = new BackUpCadDocumentViewDataContext();
                MainDispatcher = Dispatcher.CurrentDispatcher;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDatacontext.IsCreoEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDatacontext.IsCreoEnable = e;
            }
            catch (Exception ex)
            {
                throw new DxfExportException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods

        private void ExecuteCadDocumentBackup()
        {
            try
            {
                string ExportFolder = CurrentDatacontext.CurrentFolder;

                if (!Directory.Exists(ExportFolder))
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("DXF_ErrorMsgDxfFolderError"), McgWpfTools.GetStringResource("DXF_ErrorMsgTitleDxfFileFolder"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (isInProgress)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("DXF_MsgDxfInProgress"), McgWpfTools.GetStringResource("DXF_MsgTitleDxfInProgress"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        StopCurrentExport = true;
                        isInProgress = false;
                    }
                }
                else
                {
                    isInProgress = true;
                    Thread aThread = new Thread(new ThreadStart(BackUpCadDocumentAsynch));
                    aThread.IsBackground = true;
                    aThread.Start();
                }
            }
            catch (Exception ex)
            {
                throw new DxfExportException(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenFolder()
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
                openFolderDialog.ShowDialog();
                if (openFolderDialog.SelectedPath != "")
                    CurrentDatacontext.CurrentFolder = openFolderDialog.SelectedPath;
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCommandResetList()
        {
            try
            {
                CurrentDatacontext.ListItems.Clear();
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCommandPaste(KeyEventArgs e = null)
        {
            try
            {
                if (e == null || (Keyboard.Modifiers == ModifierKeys.Control && e != null && e.Key == Key.V))
                {
                    string CompleteString = null;
                    if (Clipboard.GetData(DataFormats.Text) != null)
                        CompleteString = Clipboard.GetData(DataFormats.Text).ToString();

                    if (CompleteString != null)
                    {
                        var AllLines = CompleteString.Split('\n');

                        DxfExportItem NewValue = null;
                        string linePurged = null;
                        foreach (var line in AllLines)
                        {
                            linePurged = line.Split('\r').FirstOrDefault();
                            var AllValues = linePurged.Split('\t');
                            if (AllValues != null && AllValues.Count() > 0)
                            {
                                if (AllValues.FirstOrDefault().Trim() != "")
                                {
                                    NewValue = new DxfExportItem()
                                    {
                                        Number = AllValues.FirstOrDefault().Trim(),
                                        Status = McgWpfTools.GetStringResource("DXF_Status09"),
                                        Comment = "",
                                        DxfCreated = false
                                    };
                                    CurrentDatacontext.ListItems.Add(NewValue);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] CREO Methods
        private void BackUpCadDocumentAsynch()
        {
            try
            {
                string ExportFolder = CurrentDatacontext.CurrentFolder;

                foreach (var Item in CurrentDatacontext.ListItems)
                {
                    if (!StopCurrentExport)
                    {
                        Item.Status = McgWpfTools.GetStringResource("DXF_Status20");

                        string CadDocumentFileName = Item.Number;

                        // Retreave Cad Document in CREO
                        IpfcModel CurrentModel= _creoModelService.RetrieveModelOrNothing(CadDocumentFileName);

                        if (CurrentModel != null)
                        {
                            // Backup CAD Document on local folder
                            // First, do a copy of the Cad Doc to manage instances of family tables
                            // If not, backup the complete family and not only this instance.
                            IpfcModel CopyModel = CurrentModel.CopyAndRetrieve("backupcaddoccreotools",null);
                            CurrentModel.Erase();
                            CopyModel.Rename(CadDocumentFileName,null);
                            IpfcModelDescriptor CurrentIpfcModelDescriptor = (new CCpfcModelDescriptor()).CreateFromFileName(CadDocumentFileName);
                            CurrentIpfcModelDescriptor.Path = ExportFolder;
                            CopyModel.Backup(CurrentIpfcModelDescriptor);

                            // Erase CAD Document from session
                            CopyModel.Erase();
                            Item.Status = McgWpfTools.GetStringResource("DXF_Status21");
                            Thread.Sleep(3000);
                        }
                        else
                            Item.Status = McgWpfTools.GetStringResource("DXF_Status23");

                    }
                }
                if (StopCurrentExport)
                {
                    MainDispatcher.Invoke(new Action(() => UpdateStatusBar(0, McgWpfTools.GetStringResource("DXF_Status04"))));
                    StopCurrentExport = false;
                }
                else
                    MainDispatcher.Invoke(new Action(() => UpdateStatusBar(0, McgWpfTools.GetStringResource("DXF_Status05"))));
                isInProgress = false;
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateStatusBar(int nb = 0, string str = null)
        {
            try
            {
                if (nb == 0 && str == null)
                    CurrentDatacontext.StatusBarMessage = String.Format(McgWpfTools.GetStringResource("DXF_SbMsg1"), CurrentDatacontext.ListItems.Count);
                else if (nb != 0 && str == null)
                    CurrentDatacontext.StatusBarMessage = String.Format(McgWpfTools.GetStringResource("DXF_SbMsg2"), CurrentDatacontext.ListItems.Count, nb);
                else if (str != null)
                    CurrentDatacontext.StatusBarMessage = String.Format(McgWpfTools.GetStringResource("DXF_SbMsg3"), CurrentDatacontext.ListItems.Count, str);
            }
            catch (Exception ex)
            {
                throw new DxfExportException(this.GetType().Name, ex);
            }
        }
        #endregion

    }
}
