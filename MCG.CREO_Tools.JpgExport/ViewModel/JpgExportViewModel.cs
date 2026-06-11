using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.CreoInteractionTools.CREOExceptions;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.JpgExport.Exceptions;
using MCG.CREO_Tools.JpgExport.View;
using pfcls;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.JpgExport.ViewModel
{
    public class JpgExportViewModel : ObservableObject, IJpgExportViewModel
    {
        #region [REGION] Properties from Interface
        public JpgExportDataContext CurrentJpgExportDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder;
        private Dispatcher MainDispatcher = null;
        private bool StopCurrentExport = false;
        private bool isInProgress = false;
        private int TotalJpgToCreate = 0;
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
        public ICommand CommandExportJpg { get => new RelayCommand(() => ExecuteExportJpg()); }
        public ICommand CommandOpenModelInCreo { get => new RelayCommand(() => ExecuteOpenModelInCreo()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        private readonly ICreoMacroService _creoMacroService;
        public JpgExportViewModel(ICreoSessionProvider creoSessionProvider,
                                  ICreoModelService creoModelService,
                                  ICreoMacroService creoMacroService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;
                _creoMacroService = creoMacroService;

                CurrentJpgExportDataContext = new JpgExportDataContext();
                MainDispatcher = Dispatcher.CurrentDispatcher;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentJpgExportDataContext.IsCreoEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentJpgExportDataContext.IsCreoEnable = e;
            }
            catch (Exception ex)
            {
                throw new JpgExportException(this.GetType().Name, ex);
            }
        }

        public void Update()
        {
            try
            {
                CurrentJpgExportDataContext.CurrentFolder = McgWpfTools.GetStringResource("JPG_TbExportFolder");
                CurrentJpgExportDataContext.CurrentFileName = McgWpfTools.GetStringResource("JPG_TbExportFile");

                CurrentJpgExportDataContext.ListView3D.Add(new JpgExportComboBoxValue { Value = "Standard Orientation", ValueShown = McgWpfTools.GetStringResource("JPG_ValView1") });
                CurrentJpgExportDataContext.ListView3D.Add(new JpgExportComboBoxValue { Value = "Default Orientation", ValueShown = McgWpfTools.GetStringResource("JPG_ValView2") });
                CurrentJpgExportDataContext.SelectedView3D = CurrentJpgExportDataContext.ListView3D.ElementAt(0);

                CurrentJpgExportDataContext.ListDisplayStyle.Add(new JpgExportComboBoxValue { Value = "ProCmdEnvShadedEdges", ValueShown = McgWpfTools.GetStringResource("JPG_ValDisplayStyle1") });
                CurrentJpgExportDataContext.ListDisplayStyle.Add(new JpgExportComboBoxValue { Value = "ProCmdEnvShadedReflect", ValueShown = McgWpfTools.GetStringResource("JPG_ValDisplayStyle2") });
                CurrentJpgExportDataContext.ListDisplayStyle.Add(new JpgExportComboBoxValue { Value = "ProCmdEnvShaded", ValueShown = McgWpfTools.GetStringResource("JPG_ValDisplayStyle3") });
                CurrentJpgExportDataContext.ListDisplayStyle.Add(new JpgExportComboBoxValue { Value = "ProCmdEnvNoHidden", ValueShown = McgWpfTools.GetStringResource("JPG_ValDisplayStyle4") });
                CurrentJpgExportDataContext.SelectedDisplayStyle = CurrentJpgExportDataContext.ListDisplayStyle.ElementAt(0);

                CurrentJpgExportDataContext.ListResolution.Add(new JpgExportComboBoxValue { Value = "dpi100", ValueShown = McgWpfTools.GetStringResource("JPG_ValResolution1") });
                CurrentJpgExportDataContext.ListResolution.Add(new JpgExportComboBoxValue { Value = "dpi200", ValueShown = McgWpfTools.GetStringResource("JPG_ValResolution2") });
                CurrentJpgExportDataContext.ListResolution.Add(new JpgExportComboBoxValue { Value = "dpi300", ValueShown = McgWpfTools.GetStringResource("JPG_ValResolution3") });
                CurrentJpgExportDataContext.ListResolution.Add(new JpgExportComboBoxValue { Value = "dpi400", ValueShown = McgWpfTools.GetStringResource("JPG_ValResolution4") });
                CurrentJpgExportDataContext.ListResolution.Add(new JpgExportComboBoxValue { Value = "dpi500", ValueShown = McgWpfTools.GetStringResource("JPG_ValResolution5") });
                CurrentJpgExportDataContext.ListResolution.Add(new JpgExportComboBoxValue { Value = "dpi600", ValueShown = McgWpfTools.GetStringResource("JPG_ValResolution6") });
                CurrentJpgExportDataContext.SelectedResolution = CurrentJpgExportDataContext.ListResolution.ElementAt(0);
            }
            catch (Exception ex)
            {
                throw new JpgExportException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteBtHelpMouseLeftButtonUpEvent()
        {
            try
            {
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("JPG_LinkHelpJpgExport"));
            }
            catch (Exception ex)
            {
                JpgExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenFolder()
        {
            try
            {
                FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
                openFolderDialog.ShowDialog();
                if (openFolderDialog.SelectedPath != "")
                    CurrentJpgExportDataContext.CurrentFolder = openFolderDialog.SelectedPath;
            }
            catch (Exception ex)
            {
                JpgExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenFile()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = McgWpfTools.GetStringResource("JPG_MsgFileFilter");
                openFileDialog.ShowDialog();

                if (openFileDialog.FileName != "")
                {
                    CurrentJpgExportDataContext.CurrentFileName = openFileDialog.FileName;
                    UpdateListItems();

                    if (CurrentJpgExportDataContext.CurrentFolder == null || CurrentJpgExportDataContext.CurrentFolder == "" || CurrentJpgExportDataContext.CurrentFolder == McgWpfTools.GetStringResource("JPG_TbExportFolder"))
                        CurrentJpgExportDataContext.CurrentFolder = CurrentJpgExportDataContext.CurrentFileName.Substring(0, CurrentJpgExportDataContext.CurrentFileName.LastIndexOf("\\"));
                }
            }
            catch (Exception ex)
            {
                JpgExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteExportJpg()
        {
            try
            {
                StartExportJpg();
            }
            catch (Exception ex)
            {
                JpgExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteOpenModelInCreo()
        {
            try
            {
                if (CurrentJpgExportDataContext.SelectedItem != null && CurrentJpgExportDataContext.SelectedItem.CurrentEpmDocument != null)
                    CurrentJpgExportDataContext.SelectedItem.CurrentEpmDocument.OpenInCreo(_creoSessionProvider, _creoModelService);
            }
            catch (Exception ex)
            {
                JpgExportException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Methods Creo Interaction
        private void StartExportJpg()
        {
            try
            {
                RaiseActionInProgressEvent();
                if (CurrentJpgExportDataContext.CurrentFileName == null || CurrentJpgExportDataContext.CurrentFileName.Trim() == "" || CurrentJpgExportDataContext.CurrentFileName == McgWpfTools.GetStringResource("JPG_TbExportFile") ||
                    CurrentJpgExportDataContext.CurrentFolder == null || CurrentJpgExportDataContext.CurrentFolder.Trim() == "" || CurrentJpgExportDataContext.CurrentFolder == McgWpfTools.GetStringResource("JPG_TbExportFolder"))
                    MessageBox.Show(McgWpfTools.GetStringResource("JPG_ErrorMsgDxfFileFolder"), McgWpfTools.GetStringResource("JPG_ErrorMsgTitleDxfFileFolder"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    TotalJpgToCreate = CurrentJpgExportDataContext.ListItems.Count((item) => !item.JpgCreated);

                    if (isInProgress)
                    {
                        if (MessageBox.Show(McgWpfTools.GetStringResource("JPG_MsgDxfInProgress"), McgWpfTools.GetStringResource("JPG_MsgTitleDxfInProgress"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            StopCurrentExport = true;
                            isInProgress = false;
                        }
                    }
                    else
                    {
                        isInProgress = true;
                        Thread aThread = new Thread(new ThreadStart(ExportAllJpgAsync));
                        aThread.IsBackground = true;
                        aThread.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseActionDoneEvent();
                isInProgress = false;
                StopCurrentExport = false;
                throw new JpgExportException(this.GetType().Name, ex);
            }
        }

        private void ExportAllJpgAsync()
        {
            try
            {
                string msgReturn;

                foreach (JpgExportItem CurrentItem in CurrentJpgExportDataContext.ListItems)
                {
                    if (!CurrentItem.JpgCreated && !StopCurrentExport)
                    {
                        MainDispatcher.Invoke(new Action(() => UpdateStatusBar(TotalJpgToCreate)));

                        CurrentItem.Status = McgWpfTools.GetStringResource("JPG_Status01");
                        CurrentItem.Comment = "";
                        msgReturn = ExportOneJpg(CurrentItem.CurrentEpmDocument);
                        CurrentItem.Comment = msgReturn;

                        if (msgReturn == McgWpfTools.GetStringResource("JPG_Status02"))
                        {
                            CurrentItem.Status = McgWpfTools.GetStringResource("JPG_Status02");
                            CurrentItem.JpgCreated = true;
                        }
                        else 
                        {
                            CurrentItem.Status = McgWpfTools.GetStringResource("JPG_Status03");
                            CurrentItem.JpgCreated = false;
                        }
                        TotalJpgToCreate--;
                    }
                    if (!StopCurrentExport)
                        Thread.Sleep(2000);
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
                JpgExportException.SendMessageBox(this.GetType().Name, ex);
            }
            finally
            {
                RaiseActionDoneEvent();
                StopCurrentExport = false;
                isInProgress = false;
            }
        }

        private string ExportOneJpg(EPMDocument CurrentEpmDoc)
        {
            try
            {
                if (!_creoSessionProvider.CheckConnection())
                    return McgWpfTools.GetStringResource("JPG_Status10");

                string ReturnMessage = McgWpfTools.GetStringResource("JPG_Status02");
                string TempCompleteJPGFileName;
                string TempJPGFileName;

                if (CurrentEpmDoc.PartNumber.LastIndexOf('.') < 1)
                {
                    TempJPGFileName = $"{CurrentEpmDoc.PartNumber}.jpg";
                    TempCompleteJPGFileName = $"{CurrentJpgExportDataContext.CurrentFolder}\\{CurrentEpmDoc.PartNumber}.jpg";
                }
                else
                {
                    TempJPGFileName = $"{CurrentEpmDoc.PartNumber.Split('.')[0]}.jpg";
                    TempCompleteJPGFileName = $"{CurrentJpgExportDataContext.CurrentFolder}\\{CurrentEpmDoc.PartNumber.Split('.')[0]}.jpg";
                }

                if (File.Exists(TempCompleteJPGFileName))
                    File.Delete(TempCompleteJPGFileName);

                // Start Creo Jpg Export
                _creoSessionProvider.Session.EraseUndisplayedModels();
                IpfcModel ThreeDmodelRetrieve = CurrentEpmDoc.RetrieveModel(_creoSessionProvider, _creoModelService);

                // If model is a fmily table, remove all instance before doing the copy
                IpfcFamilyMember GenericModel = (IpfcFamilyMember)ThreeDmodelRetrieve;
                IpfcFamilyTableRows ListRows = GenericModel.ListRows();
                if (ListRows != null && ListRows.Count > 0)
                {
                    foreach (IpfcFamilyTableRow row in ListRows)
                    {
                        GenericModel.RemoveRow(row);
                    }
                }

                IpfcModel ThreeDmodel = ThreeDmodelRetrieve;

                string backupFolder = Path.GetTempPath();
                string backupFileName = ThreeDmodelRetrieve.FileName;
                string backupFullPath = Path.Combine(backupFolder, backupFileName);

                IpfcModelDescriptor BackupDir = new CCpfcModelDescriptor().Create(ThreeDmodelRetrieve.Type, null, null);

                // Create backup
                BackupDir.Path = backupFolder;

                ThreeDmodelRetrieve.Backup(BackupDir);

                IpfcWindow CurrentWindow = _creoModelService.GetCadDocWindow(ThreeDmodelRetrieve);
                if(CurrentWindow != null)
                    CurrentWindow.Close();

                _creoSessionProvider.Session.EraseUndisplayedModels();
                //ThreeDmodelRetrieve.Erase();

                ThreeDmodel = _creoModelService.RetrieveModelFromLocalDir(backupFolder, backupFileName);
                ThreeDmodel.Display();

                // Active the model
                _creoMacroService.ActiveWindow();

                // Set orientation
                _creoMacroService.Select3DView(CurrentJpgExportDataContext.SelectedView3D.Value);

                // Refit view
                _creoMacroService.Refit3DView();

                // with or without edges
                _creoMacroService.Set3DDisplayStyle(CurrentJpgExportDataContext.SelectedDisplayStyle.Value);

                Thread.Sleep(1000);
                // Create the jpg
                _creoMacroService.CreateJpg(CurrentJpgExportDataContext.CurrentFolder, TempJPGFileName, CurrentJpgExportDataContext.SelectedResolution.Value);

                // Wait for complete creation
                int TotalWait = 0;
                while (!File.Exists(TempCompleteJPGFileName) && TotalWait < 11)
                {
                    Thread.Sleep(1000);
                    TotalWait++;
                }

                if (TotalWait > 10)
                    ReturnMessage = McgWpfTools.GetStringResource("JPG_Status06");

                // erase from session 3D model
                CurrentWindow = _creoModelService.GetCadDocWindow(ThreeDmodel);
                if (CurrentWindow != null)
                    CurrentWindow.Close();
                _creoSessionProvider.Session.EraseUndisplayedModels();
                //ThreeDmodel.Erase();

                // Delete backup file
                int index = 1;
                string indexedFilePath =$"{backupFullPath}.{index}";
                while (File.Exists(indexedFilePath))
                {
                    File.Delete(indexedFilePath);
                    index++;
                    indexedFilePath = $"{backupFullPath}.{index}";
                }

                return ReturnMessage;
            }
            catch (CREORetrieveModelException)
            {
                return McgWpfTools.GetStringResource("JPG_Status08");
            }
            catch (Exception ex)
            {
                
                return McgWpfTools.GetStringResource("JPG_Status10");
            }
        }
        #endregion

        #region [REGION] Methods Update Data
        private void UpdateListItems()
        {
            try
            {
                if (File.Exists(CurrentJpgExportDataContext.CurrentFileName))
                {
                    CurrentJpgExportDataContext.ListItems.Clear();
                    using (StreamReader CurrentStream = new StreamReader(CurrentJpgExportDataContext.CurrentFileName))
                    {
                        string NewLine = "";
                        while (CurrentStream.Peek() != -1)
                        {
                            NewLine = CurrentStream.ReadLine();

                            CurrentJpgExportDataContext.ListItems.Add(new JpgExportItem()
                            {
                                Number = NewLine,
                                Status = McgWpfTools.GetStringResource("JPG_Status09"),
                                Comment = "",
                                JpgCreated = false,
                                CurrentEpmDocument = new EPMDocument(NewLine, NewLine, NewLine) 
                            });
                        }
                    }
                }
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                throw new JpgExportException(this.GetType().Name, ex);
            }
        }

        private void UpdateStatusBar(int nb = 0, string str = null)
        {
            try
            {
                if (nb == 0 && str == null)
                    CurrentJpgExportDataContext.StatusBarMessage = String.Concat(McgWpfTools.GetStringResource("JPG_SbMsg1"), CurrentJpgExportDataContext.ListItems.Count);
                else if (nb != 0 && str == null)
                    CurrentJpgExportDataContext.StatusBarMessage = String.Format(McgWpfTools.GetStringResource("JPG_SbMsg2"), CurrentJpgExportDataContext.ListItems.Count, nb);
                else if (str != null)
                    CurrentJpgExportDataContext.StatusBarMessage = String.Format(McgWpfTools.GetStringResource("JPG_SbMsg3"), CurrentJpgExportDataContext.ListItems.Count, str);
            }
            catch (Exception ex)
            {
                throw new JpgExportException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
