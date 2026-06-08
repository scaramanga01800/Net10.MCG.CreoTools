using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCG.CommonLib.CreoInteractionTools.Models;
using MCG.CommonLib.CreoInteractionTools.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.DxfExport.Configuration;
using MCG.CREO_Tools.DxfExport.Exceptions;
using MCG.CREO_Tools.DxfExport.View;
using pfcls;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MCG.CREO_Tools.DxfExport.ViewModel
{
    public class DxfDwgDrawingExportViewModel : ObservableObject, IDxfDwgDrawingExportViewModel
    {
        #region [REGION] Properties from Interface
        public DxfDwgDrawingExportDatacontext CurrentDatacontext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private Dispatcher MainDispatcher { get; set; } = null;
        private bool StopCurrentExport { get; set; } = false;
        private bool IsInProgress { get; set; } = false;
        #endregion

        #region [REGION] Commands
        public ICommand CommandDxfDwgDrawingExport { get => new RelayCommand(() => ExecuteDxfDwgDrawingExport()); }
        public ICommand CommandOpenFolder { get => new RelayCommand(() => ExecuteOpenFolder()); }
        public ICommand CommandPaste { get => new RelayCommand<KeyEventArgs>((obj) => ExecuteCommandPaste(obj)); }
        public ICommand CommandResetList { get => new RelayCommand(() => ExecuteCommandResetList()); }
        #endregion

        #region [REGION] Init
        private readonly ICreoSessionProvider _creoSessionProvider;
        private readonly ICreoModelService _creoModelService;
        private readonly ICreoMacroService _creoMacroService;

        public DxfDwgDrawingExportViewModel(ICreoSessionProvider creoSessionProvider, 
                                            ICreoModelService creoModelService,
                                            ICreoMacroService creoMacroService)
        {
            try
            {
                _creoSessionProvider = creoSessionProvider;
                _creoModelService = creoModelService;
                _creoMacroService = creoMacroService;

                CurrentDatacontext = new DxfDwgDrawingExportDatacontext();
                MainDispatcher = Dispatcher.CurrentDispatcher;

                var creoConnectionStatus = _creoSessionProvider.Connect(false);
                CurrentDatacontext.IsCreoEnable = creoConnectionStatus == CreoConnectionStatus.OK;
                _creoSessionProvider.ConnectionStateChanged += (sender, e) => CurrentDatacontext.IsCreoEnable = e;

                CurrentDatacontext.DrawingTemplate = DxfExportConstants.DxfDwgDrawingTemplate;
            }
            catch (Exception ex)
            {
                throw new DxfExportException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteDxfDwgDrawingExport()
        {
            try
            {
                _creoSessionProvider.CheckConnection();

                string ExportFolder = CurrentDatacontext.CurrentFolder;

                if (!CurrentDatacontext.IsDwgSelected && !CurrentDatacontext.IsDxfSelected)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("DXF_ErrorMsgDxfSelectionError"), McgWpfTools.GetStringResource("DXF_ErrorMsgTitleDxfFileFolder"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!Directory.Exists(ExportFolder))
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("DXF_ErrorMsgDxfFolderError"), McgWpfTools.GetStringResource("DXF_ErrorMsgTitleDxfFileFolder"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_creoModelService.RetrieveModelOrNothing(CurrentDatacontext.DrawingTemplate.ToLower().Trim(),EpfcModelType.EpfcMDL_DRAWING) == null)
                {
                    MessageBox.Show(McgWpfTools.GetStringResource("DXF_ErrorMsgDrwTemplateError"), McgWpfTools.GetStringResource("DXF_ErrorMsgTitleDxfFileFolder"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (IsInProgress)
                {
                    if (MessageBox.Show(McgWpfTools.GetStringResource("DXF_MsgDxfInProgress"), McgWpfTools.GetStringResource("DXF_MsgTitleDxfInProgress"), MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        StopCurrentExport = true;
                        IsInProgress = false;
                    }
                }
                else
                {
                    IsInProgress = true;

                    Thread aThread = new Thread(new ThreadStart(ExportDxfDwgAsynch));
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
        private void ExportDxfDwgAsynch()
        {
            try
            {
                string ExportFolder = CurrentDatacontext.CurrentFolder;
                string TemplateDrawingName = CurrentDatacontext.DrawingTemplate;
                string ExportFileName = "";

                // update Config.pro option drawing_view_origin_csys to Center
                _creoSessionProvider.SetConfigOption("drawing_view_origin_csys", "Center");

                foreach (var Item in CurrentDatacontext.ListItems)
                {
                    if (!StopCurrentExport)
                    {
                        Item.Status = McgWpfTools.GetStringResource("DXF_Status20");

                        string DrawingName = Item.Number.Split('.').FirstOrDefault();
                        string DrawingModelName = Item.Number;

                        // Create the drawing
                        IpfcDrawing CurrentIpfcDrawing = _creoModelService.CreateDrwFromTemplate(DrawingName, TemplateDrawingName, DrawingModelName);
                        if (CurrentIpfcDrawing != null)
                        {
                            // create dwg/dxf
                            ((IpfcModel)CurrentIpfcDrawing).Display();
                            IpfcWindow DrwWindow = _creoSessionProvider.Session.GetModelWindow((IpfcModel)CurrentIpfcDrawing);
                            if (DrwWindow != null)
                            {
                                DrwWindow.Activate();
                                if (CurrentDatacontext.IsDxfSelected)
                                {
                                    ExportFileName = $"{ExportFolder}\\{DrawingName}.dxf";
                                    if (File.Exists(ExportFileName))
                                        File.Delete(ExportFileName);

                                    _creoMacroService.ExportDxfAllSheets(ExportFileName);
                                    Item.Comment = $"{Item.Comment} Dxf exported.";
                                    System.Threading.Thread.Sleep(3000);
                                }
                                if (CurrentDatacontext.IsDwgSelected)
                                {
                                    ExportFileName = $"{ExportFolder}\\{DrawingName}.dwg";
                                    if (File.Exists(ExportFileName))
                                        File.Delete(ExportFileName);
                                    _creoMacroService.ExportDwgAllSheets(ExportFileName);
                                    Item.Comment = $"{Item.Comment} Dwg exported.";
                                    System.Threading.Thread.Sleep(3000);
                                }
                            }
                            ((IpfcModel)CurrentIpfcDrawing).Erase();
                            //System.Threading.Thread.Sleep(1000);
                            Item.Status = McgWpfTools.GetStringResource("DXF_Status21");
                        }
                        else
                            Item.Status = McgWpfTools.GetStringResource("DXF_Status22");

                    }
                }
                if (StopCurrentExport)
                {
                    MainDispatcher.Invoke(new Action(() => UpdateStatusBar(0, McgWpfTools.GetStringResource("DXF_Status04"))));
                    StopCurrentExport = false;
                }
                else
                    MainDispatcher.Invoke(new Action(() => UpdateStatusBar(0, McgWpfTools.GetStringResource("DXF_Status05"))));
                IsInProgress = false;
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
