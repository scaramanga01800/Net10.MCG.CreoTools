using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.CreoInteractionTools.CREOExceptions;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.DxfExport.Exceptions;
using MCG.CREO_Tools.DxfExport.View;
using pfcls;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.DxfExport.ViewModel
{
    public class DxfExportViewModel : ObservableObject, IDxfExportViewModel
    {
        #region [REGION] Properties from Interface
        public DxfExportDataContext CurrentDxfExportDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder;
        private Dispatcher MainDispatcher = null;
        private bool StopCurrentExport = false;
        private bool isInProgress = false;
        private int TotalDxfToCreate = 0;
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
        public ICommand CommandOpenFolder { get => new RelayCommand(() => ExecuteOpenFolder()); }
        public ICommand CommandOpenFile { get => new RelayCommand(() => ExecuteOpenFile()); }
        public ICommand CommandExportDxf { get => new RelayCommand(() => ExecuteExportDxf()); }
        public ICommand CommandOpenModelInCreo { get => new RelayCommand(() => ExecuteOpenModelInCreo()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        private readonly ICreoMacroService _creoMacroService;

        public DxfExportViewModel(ICreoSessionProvider creoSessionProvider,
                                  ICreoModelService creoModelService,
                                  ICreoMacroService creoMacroService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;
                _creoMacroService = creoMacroService;

                CurrentDxfExportDataContext = new DxfExportDataContext();
                MainDispatcher = Dispatcher.CurrentDispatcher;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDxfExportDataContext.IsCreoEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDxfExportDataContext.IsCreoEnable = e;

                CurrentDxfExportDataContext.CurrentFolder =McgWpfTools.GetStringResource("DXF_TbExportFolder");
                CurrentDxfExportDataContext.CurrentFileName = McgWpfTools.GetStringResource("DXF_TbExportFile");
            }
            catch (Exception ex)
            {
                throw new DxfExportException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteBtHelpMouseLeftButtonUpEvent()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("DXF_LinkHelpDxfExport"));
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenFolder()
        {
            try
            {
                FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
                openFolderDialog.ShowDialog();
                if (openFolderDialog.SelectedPath != "")
                    CurrentDxfExportDataContext.CurrentFolder = openFolderDialog.SelectedPath;
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenFile()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = McgWpfTools.GetStringResource("DXF_MsgFileFilter");
                openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "")
                {
                    CurrentDxfExportDataContext.CurrentFileName = openFileDialog.FileName;
                    UpdateListItems();

                    if (CurrentDxfExportDataContext.CurrentFolder == null || CurrentDxfExportDataContext.CurrentFolder == "" || CurrentDxfExportDataContext.CurrentFolder == McgWpfTools.GetStringResource("DXF_TbExportFolder"))
                        CurrentDxfExportDataContext.CurrentFolder = CurrentDxfExportDataContext.CurrentFileName.Substring(0, CurrentDxfExportDataContext.CurrentFileName.LastIndexOf("\\"));
                }
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteExportDxf()
        {
            try
            {
                StartExportDxf();
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenModelInCreo()
        {
            try
            {
                if (CurrentDxfExportDataContext.SelectedItem != null && CurrentDxfExportDataContext.SelectedItem.CurrentEpmDocument != null)
                    CurrentDxfExportDataContext.SelectedItem.CurrentEpmDocument.OpenInCreo(_creoSessionProvider, _creoModelService);
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods Creo Interaction
        private void StartExportDxf()
        {
            try
            {
                RaiseActionInProgressEvent();
                if (CurrentDxfExportDataContext.CurrentFileName == null || CurrentDxfExportDataContext.CurrentFileName.Trim() == "" || CurrentDxfExportDataContext.CurrentFileName == McgWpfTools.GetStringResource("DXF_TbExportFile") ||
                    CurrentDxfExportDataContext.CurrentFolder == null || CurrentDxfExportDataContext.CurrentFolder.Trim() == "" || CurrentDxfExportDataContext.CurrentFolder == McgWpfTools.GetStringResource("DXF_TbExportFolder"))
                    MessageBox.Show(McgWpfTools.GetStringResource("DXF_ErrorMsgDxfFileFolder"), McgWpfTools.GetStringResource("DXF_ErrorMsgTitleDxfFileFolder"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    TotalDxfToCreate = CurrentDxfExportDataContext.ListItems.Count((item) => !item.DxfCreated);

                    if (isInProgress)
                    {
                        if (MessageBox.Show(McgWpfTools.GetStringResource("DXF_MsgDxfInProgress"), McgWpfTools.GetStringResource("DXF_MsgTitleDxfInProgress"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            StopCurrentExport = true;
                            isInProgress = false;
                        }
                    }
                    else
                    {
                        isInProgress = true;
                        Thread aThread = new Thread(new ThreadStart(ExportAllDxfAsync));
                        aThread.IsBackground = true;
                        aThread.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                isInProgress = false;
                throw new DxfExportException(this.GetType().Name, ex);
            }
        }

        private void ExportAllDxfAsync()
        {
            try
            {
                string msgReturn;

                foreach (DxfExportItem CurrentItem in CurrentDxfExportDataContext.ListItems)
                {
                    if (!CurrentItem.DxfCreated && !StopCurrentExport)
                    {
                        MainDispatcher.Invoke(new Action(() => UpdateStatusBar(TotalDxfToCreate)));

                        CurrentItem.Status = McgWpfTools.GetStringResource("DXF_Status01");
                        CurrentItem.Comment = "";
                        msgReturn = ExportOneDxf(CurrentItem.CurrentEpmDocument);
                        CurrentItem.Comment = msgReturn;

                        if (msgReturn == McgWpfTools.GetStringResource("DXF_Status02"))
                        {
                            CurrentItem.Status = McgWpfTools.GetStringResource("DXF_Status02");
                            CurrentItem.DxfCreated = true;
                        }
                        else
                        {
                            CurrentItem.Status = McgWpfTools.GetStringResource("DXF_Status03");
                            CurrentItem.DxfCreated = false;
                        }
                        TotalDxfToCreate--;
                    }
                    if (!StopCurrentExport)
                        Thread.Sleep(3000);
                }

                if (StopCurrentExport)
                {
                    MainDispatcher.Invoke(new Action(() => UpdateStatusBar(0, McgWpfTools.GetStringResource("DXF_Status04"))));
                    StopCurrentExport = false;
                }
                else
                    MainDispatcher.Invoke(new Action(() => UpdateStatusBar(0, McgWpfTools.GetStringResource("DXF_Status05"))));
            }
            catch (Exception ex)
            {
                DxfExportException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                isInProgress = false;
            }
        }

        private string ExportOneDxf(EPMDocument CurrentEpmDoc)
        {
            try
            {
                if (!_creoSessionProvider.CheckConnection())
                    return McgWpfTools.GetStringResource("DXF_Status10");

                string ReturnMessage = McgWpfTools.GetStringResource("DXF_Status02");
                string TempNumber;
                if (CurrentEpmDoc.PartNumber.LastIndexOf('.') < 1)
                {
                    TempNumber = CurrentEpmDoc.PartNumber;

                    CurrentEpmDoc.PartNumber = $"{CurrentEpmDoc.PartNumber}.PRT";
                    CurrentEpmDoc.FileName = $"{CurrentEpmDoc.FileName}.PRT";
                    //CurrentEpmDoc.EPMType = "PRT";
                }
                else
                    TempNumber = CurrentEpmDoc.PartNumber.Split('.')[0];

                // Start Creo DXF Export
                _creoSessionProvider.Session.EraseUndisplayedModels();
                IpfcModel ThreeDmodelRetrieve = CurrentEpmDoc.RetrieveModel(_creoSessionProvider);

                // Backup model
                string backupFolder = Path.GetTempPath();
                string backupFileName = $"{ThreeDmodelRetrieve.InstanceName}.{ThreeDmodelRetrieve?.FileName?.Split('.').LastOrDefault()}";
                string backupFullPath = Path.Combine(backupFolder, backupFileName);
                IpfcModelDescriptor BackupDir = new CCpfcModelDescriptor().Create(ThreeDmodelRetrieve.Type, null, null);
                BackupDir.Path = backupFolder;
                ThreeDmodelRetrieve.Backup(BackupDir);
                IpfcWindow CurrentWindow = _creoModelService.GetCadDocWindow(ThreeDmodelRetrieve);
                if (CurrentWindow != null)
                    CurrentWindow.Close();
                _creoSessionProvider.Session.EraseUndisplayedModels();

                ThreeDmodelRetrieve = _creoModelService.RetrieveModelFromLocalDir(backupFolder, backupFileName);

                // Check file
                string FinalDXFFileName = $"{CurrentDxfExportDataContext.CurrentFolder}\\{TempNumber}_{ThreeDmodelRetrieve.ReleaseLevel}_{ThreeDmodelRetrieve.Revision}.dxf";
                string TempDXFFileName = $"{CurrentDxfExportDataContext.CurrentFolder}\\{TempNumber}.dxf";

                if (File.Exists(TempDXFFileName))
                    File.Delete(TempDXFFileName);

                // If IsFlatSelected, search Flat instead of 3D
                IpfcFamilyMember GenericModel;
                IpfcFamilyTableRows ListRows;
                if (CurrentDxfExportDataContext.IsFlatSelected)
                {
                    GenericModel = (IpfcFamilyMember)ThreeDmodelRetrieve;
                    ListRows = GenericModel.ListRows();
                    bool FirstFlatFound = false;
                    foreach (IpfcFamilyTableRow row in ListRows)
                    {
                        if (!FirstFlatFound && row.InstanceName.Contains("_FLAT"))
                        {
                            CurrentEpmDoc.FileName = row.InstanceName;
                            CurrentEpmDoc.PartNumber = row.InstanceName;
                            ThreeDmodelRetrieve = CurrentEpmDoc.RetrieveModel(_creoSessionProvider);
                            FirstFlatFound = true;
                        }
                    }
                }

                // If model is a family table, remove all instance before doing the copy
                GenericModel = (IpfcFamilyMember)ThreeDmodelRetrieve;
                ListRows = GenericModel.ListRows();
                if (ListRows != null && ListRows.Count > 0)
                {
                    foreach (IpfcFamilyTableRow row in ListRows)
                    {
                        GenericModel.RemoveRow(row);
                    }
                }

                // backup

                IpfcModel ThreeDmodel = ThreeDmodelRetrieve;
                ThreeDmodel.Display();
                ThreeDmodel.Rename("DXF_3D", true);

                _creoSessionProvider.Session.GetModelWindow(ThreeDmodel).Activate();

                // Change 3D model dimensions to "average"
                _creoMacroService.ChangeDimensionToAverage();
                Thread.Sleep(1000);

                // Check if view "9_DECOUPE" exists
                IpfcViewOwner ModelViewOwner = (IpfcViewOwner)ThreeDmodel;
                IpfcView ExportDxfView = ModelViewOwner.GetView("9_DECOUPE");
                if (ExportDxfView == null)
                {
                    IpfcView TopView = ModelViewOwner.GetView("1_TOP");
                    ReturnMessage = $"{ReturnMessage} - {McgWpfTools.GetStringResource("DXF_Status11")}";
                    if (TopView != null)
                    {
                        _creoMacroService.Select3DView("1_TOP");
                        ModelViewOwner.SaveView("9_DECOUPE");
                        ReturnMessage = $"{ReturnMessage} - {McgWpfTools.GetStringResource("DXF_Status12")}";
                    }
                    else
                    {
                        ThreeDmodel.Erase();
                        _creoSessionProvider.Session.EraseUndisplayedModels();
                        return $"{ReturnMessage} - {McgWpfTools.GetStringResource("DXF_Status03")}";
                    }
                }

                // Create Drawing
                IpfcModel drwModel = _creoMacroService.CreateDrawing("DXF_3D", "TO_BE_DELETED", "template_dxf");

                // regen drawing
                _creoMacroService.RegenDrawingInSession(drwModel);
                Thread.Sleep(1000);

                // Export DXF
                _creoMacroService.ExportDxf(TempDXFFileName);

                // Wait for complete creation
                int TotalWait = 0;
                while (!File.Exists(TempDXFFileName) && TotalWait < 11)
                {
                    Thread.Sleep(1000);
                    TotalWait++;
                }

                if (TotalWait > 10)
                    ReturnMessage = McgWpfTools.GetStringResource("DXF_Status06");

                // delete from session DRW
                drwModel.Erase();
                Thread.Sleep(1000);

                // erase from session 3D model
                CurrentWindow = _creoModelService.GetCadDocWindow(ThreeDmodel);
                if (CurrentWindow != null)
                    CurrentWindow.Close();
                //ThreeDmodel.Erase();
                _creoSessionProvider.Session.EraseUndisplayedModels();

                // Delete backup file
                int index = 1;
                string indexedFilePath = $"{backupFullPath}.{index}";
                while (File.Exists(indexedFilePath))
                {
                    File.Delete(indexedFilePath);
                    index++;
                    indexedFilePath = $"{backupFullPath}.{index}";
                }

                if (File.Exists(TempDXFFileName))
                {
                    FileInfo Fi = new FileInfo(TempDXFFileName);
                    if (Fi.Length < 10350)
                        ReturnMessage = $"{ReturnMessage} - {McgWpfTools.GetStringResource("DXF_Status07")}";
                    if (File.Exists(FinalDXFFileName))
                        File.Delete(FinalDXFFileName);
                    File.Move(TempDXFFileName, FinalDXFFileName);
                }
                return ReturnMessage;
            }
            catch (CREORetrieveModelException)
            {
                return McgWpfTools.GetStringResource("DXF_Status08");
            }
            catch (Exception ex)
            {
                return McgWpfTools.GetStringResource("DXF_Status10");
            }
        }
        #endregion

        #region [REGION] Methods Update Data
        private void UpdateListItems()
        {
            try
            {
                if (File.Exists(CurrentDxfExportDataContext.CurrentFileName))
                {
                    CurrentDxfExportDataContext.ListItems.Clear();
                    using (StreamReader CurrentStream = new StreamReader(CurrentDxfExportDataContext.CurrentFileName))
                    {
                        string NewLine = "";
                        while (CurrentStream.Peek() != -1)
                        {
                            NewLine = CurrentStream.ReadLine();

                            CurrentDxfExportDataContext.ListItems.Add(new DxfExportItem()
                            {
                                Number = NewLine,
                                Status = McgWpfTools.GetStringResource("DXF_Status09"),
                                Comment = "",
                                DxfCreated = false,
                                CurrentEpmDocument = new EPMDocument(NewLine, NewLine, NewLine) 
                            });
                        }
                    }
                }
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                throw new DxfExportException(this.GetType().Name, ex);
            }
        }

        private void UpdateStatusBar(int nb = 0, string str = null)
        {
            try
            {
                if (nb == 0 && str == null)
                    CurrentDxfExportDataContext.StatusBarMessage = String.Format(McgWpfTools.GetStringResource("DXF_SbMsg1"), CurrentDxfExportDataContext.ListItems.Count);
                else if (nb != 0 && str == null)
                    CurrentDxfExportDataContext.StatusBarMessage = String.Format(McgWpfTools.GetStringResource("DXF_SbMsg2"), CurrentDxfExportDataContext.ListItems.Count, nb);
                else if (str != null)
                    CurrentDxfExportDataContext.StatusBarMessage = String.Format(McgWpfTools.GetStringResource("DXF_SbMsg3"), CurrentDxfExportDataContext.ListItems.Count, str);
            }
            catch (Exception ex)
            {
                throw new DxfExportException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
