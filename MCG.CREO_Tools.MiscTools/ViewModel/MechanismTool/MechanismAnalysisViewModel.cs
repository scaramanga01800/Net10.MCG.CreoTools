using MCG.CREO_Tools.MiscTools.View.MechanismTool;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CommonLib.Models.Excel;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Configuration;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.WpfComponent.Interfaces;

namespace MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool
{
    public class MechanismAnalysisViewModel : ObservableObject, IMechanismAnalysisViewModel
    {
        #region [REGION] Properties from Interface
        public MechanismAnalysisDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Internal variables
        private string MainAppFolder { get; set; }
        private bool IsTraceLog { get; set; } = true;
        #endregion

        #region [REGION] Events
        public event EventHandler ClosingEvent;

        public void RaiseClosingEvent()
        {
            try
            {
                ClosingEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Commands
        public ICommand CommandClosing { get => new RelayCommand(() => RaiseClosingEvent()); }
        public ICommand CommandDrop { get => new RelayCommand<DragEventArgs>((obj) => ExecuteDrop(obj)); }
        public ICommand CommandCreateExcel { get => new RelayCommand(() => ExecuteCreateExcel()); }
        public ICommand CommandRemoveAll { get => new RelayCommand(() => CurrentDataContext.ListFile.Clear()); }
        public ICommand CommandOpenHelp { get => new RelayCommand(() => ExecuteOpenHelp()); }
        #endregion

        #region [REGION] Init
        private readonly IMcgCommonLibWindowService _mcgCommonLibWindowService;
        public MechanismAnalysisViewModel(IMcgCommonLibWindowService mcgCommonLibWindowService)
        {
            try
            {
                _mcgCommonLibWindowService = mcgCommonLibWindowService;
                MainAppFolder = Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                CurrentDataContext = new MechanismAnalysisDataContext();
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Execution Command Methods
        private void ExecuteDrop(DragEventArgs obj)
        {
            try
            {
                if (obj != null)
                    if (obj != null && obj.Data != null && obj.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        string[] files = (string[])obj.Data.GetData(DataFormats.FileDrop);
                        foreach (var file in files)
                        {
                            if (CurrentDataContext.ListFile.FirstOrDefault((item) => item.FileName != null && item.FileName == file) == null)
                                CurrentDataContext.ListFile.Add(new AnalysisFileItem()
                                {
                                    FileName = file,
                                    Status = "Not converted"
                                });
                        }
                    }
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ExecuteCreateExcel()
        {
            try
            {
                if (CurrentDataContext.ListFile != null && CurrentDataContext.ListFile.Count > 0)
                {
                    AnalysisItem CurrentAnalysis = new AnalysisItem() { Name = "CurrentAnalysis" };
                    foreach (var file in CurrentDataContext.ListFile)
                    {
                        CurrentAnalysis = ReadAnalysisFile(file.FileName, CurrentAnalysis);
                        file.Status = "Converted";
                    }
                    CreateXlsFromAnalysis(CurrentAnalysis);
                }
            }
            catch (AnalysisFileContentException)
            {
                MessageBox.Show("Issue with content file, check name of the different analysis", "Content File Issue", MessageBoxButton.OK, MessageBoxImage.Error);
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
                McgFileAndSystemTools.OpenSharePointDocument(McgWpfTools.GetStringResource("MMA_UserGuide"));
            }
            catch (Exception ex)
            {
                MiscToolsException.SendMessageBox(this.GetType().Name, ex);
            }
        }
        #endregion

        #region [REGION] Misc Methods
        private AnalysisItem ReadAnalysisFile(string FileName, AnalysisItem CurrentAnalysis = null)
        {
            try
            {
                if (CurrentAnalysis == null)
                    CurrentAnalysis = new AnalysisItem();

                if (CurrentAnalysis.AllAnalysisFiles.FirstOrDefault((item) => item == FileName) == null)
                {
                    CurrentAnalysis.AllAnalysisFiles.Add(FileName);

                    // Read file
                    var reader = new StreamReader(File.OpenRead(FileName));
                    List<string> AllPositions = new List<string>();
                    List<string> AllValues = new List<string>();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split('\t');

                        if (values.Length > 1)
                        {
                            AllPositions.Add(values[0]);
                            AllValues.Add(values[1]);
                        }
                    }
                    reader.Close();

                    // Search diff measures
                    string CurrentAxisPosition = null;
                    string CurrentMeasureName = null;
                    string CurrentAreaName = null;
                    string CurrentStateName = null;
                    string CurrentResultName = null;
                    AnalysisMeasureItem CurrentAnalysisMeasureItem = null;
                    AnalysisAreaItem CurrentAnalysisAreaItem = null;
                    AnalysisStateItem CurrentAnalysisStateItem = null;
                    AnalysisResultItem CurrentAnalysisResultItem = null;
                    for (int index = 0; index < AllPositions.Count; index++)
                    {
                        if (AllPositions[index].ToUpper() == "AXIS 0")
                        {
                            CurrentAxisPosition = AllValues[index];
                            TraceLog.AddTraceLog($"Read Axis 0 {CurrentAxisPosition}");
                        }
                        if (AllPositions[index].ToUpper().Contains("PLOT"))
                        {
                            CurrentMeasureName = AllValues[index].Split(':').LastOrDefault();
                            TraceLog.AddTraceLog($"Read Measure 0 {CurrentMeasureName}");
                            var tempList = AllValues[index].Split(':').FirstOrDefault().Split('_');
                            if (tempList.Length > 2)
                            {
                                CurrentAreaName = tempList[0];
                                CurrentStateName = tempList[1];
                                CurrentResultName = tempList[2];
                            }
                            else
                            {
                                throw new AnalysisFileContentException(this.GetType().Name);
                            }
                            // Search Analysis Measure if exists
                            CurrentAnalysisMeasureItem = CurrentAnalysis.AllMeasures.FirstOrDefault((item) => item.Name == CurrentMeasureName);
                            if (CurrentAnalysisMeasureItem == null)
                            {
                                CurrentAnalysisMeasureItem = new AnalysisMeasureItem()
                                {
                                    Name = CurrentMeasureName,
                                    AxisPosition = CurrentAxisPosition
                                };
                                CurrentAnalysis.AllMeasures.Add(CurrentAnalysisMeasureItem);
                            }

                            // Search Analysis Area if exists within Analysis Measure
                            CurrentAnalysisAreaItem = CurrentAnalysisMeasureItem.AllAreas.FirstOrDefault((item) => item.Name == CurrentAreaName);
                            if (CurrentAnalysisAreaItem == null)
                            {
                                CurrentAnalysisAreaItem = new AnalysisAreaItem() { Name = CurrentAreaName };
                                CurrentAnalysisMeasureItem.AllAreas.Add(CurrentAnalysisAreaItem);
                            }

                            // Search Analysis State if exists within Analysis Area
                            CurrentAnalysisStateItem = CurrentAnalysisAreaItem.AllStates.FirstOrDefault((item) => item.Name == CurrentStateName);
                            if (CurrentAnalysisStateItem == null)
                            {
                                CurrentAnalysisStateItem = new AnalysisStateItem() { Name = CurrentStateName };
                                CurrentAnalysisAreaItem.AllStates.Add(CurrentAnalysisStateItem);
                            }
                        }
                        // Add/insert all values in Analysis State
                        // Value orders by the position. 
                        // If position already exist, don't added.
                        double CurrentValue;
                        double CurrentPosition;
                        if (double.TryParse(AllPositions[index], out CurrentPosition) && double.TryParse(AllValues[index], out CurrentValue))
                        {
                            CurrentPosition = Math.Round(CurrentPosition);
                            CurrentValue = Math.Round(CurrentValue);
                            //if (Math.Abs(CurrentValue) < 100)
                            //    CurrentValue = Math.Round(CurrentValue, 2);
                            //else if (Math.Abs(CurrentValue) < 1000)
                            //    CurrentValue = Math.Round(CurrentValue, 1);
                            //else if (Math.Abs(CurrentValue) >= 1000)
                            //    CurrentValue = Math.Round(CurrentValue);

                            CurrentAnalysisResultItem = CurrentAnalysisStateItem.AllResults.FirstOrDefault((item) => item.Position == CurrentPosition);
                            if (CurrentAnalysisResultItem == null)
                            {
                                CurrentAnalysisResultItem = new AnalysisResultItem()
                                {
                                    Position = CurrentPosition,
                                    Value = CurrentValue,
                                    Name = CurrentResultName
                                };
                                CurrentAnalysisStateItem.AllResults.Add(CurrentAnalysisResultItem);
                            }
                        }
                    }
                }
                return CurrentAnalysis;
            }
            catch (AnalysisFileContentException Aex)
            {
                throw Aex;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }

        private void CreateXlsFromAnalysis(AnalysisItem CurrentAnalysis)
        {
            ExcelToolsClosedXml CurrentExcel = null;
            try
            {
                int LineIndex = 1;

                if (CurrentAnalysis != null && CurrentAnalysis.Name != null && CurrentAnalysis.AllMeasures != null && CurrentAnalysis.AllMeasures.Count > 0)
                {
                    Regex RegexProc = new Regex("Excel", RegexOptions.IgnoreCase);
                    List<Process> OldExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    string UserDocumentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string XlsFileName = $"{UserDocumentFolder}\\{CurrentAnalysis.Name}.xlsx";

                    CurrentExcel = new ExcelToolsClosedXml() { CompleteFileName = XlsFileName, CompleteTemplateFileName = $"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.ExcelTemplateAnalysis}" };
                    if (CurrentExcel.OpenFile($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MiscToolsConstants.ExcelTemplateAnalysis}") != ExcelStatus.OK)
                    {
                        MessageBox.Show("Issue to create Excel file", "Excel Export Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    List<Process> NewExcelCurrentProcess = Process.GetProcesses().ToList().FindAll((proc) => RegexProc.IsMatch(proc.ProcessName));
                    Process newExcelProcess = NewExcelCurrentProcess.FirstOrDefault((proc) => !OldExcelCurrentProcess.ToList().Exists((oldprc) => proc.Id == oldprc.Id));

                    // create one tab per Measure
                    AreaTableItem CurrentAreaTableItem = new AreaTableItem();
                    List<AnalysisCurveItem> AllCurves = new List<AnalysisCurveItem>();

                    foreach (var CurrentMeasure in CurrentAnalysis.AllMeasures)
                    {
                        LineIndex = 1;

                        if (IsTraceLog) TraceLog.AddTraceLog($"Create Tab {CurrentMeasure.Name}");

                        if (CurrentMeasure.Name.Length > 31)
                            CurrentMeasure.Name = CurrentMeasure.Name.Substring(0, 31);
                        CurrentExcel.CreateSheetFromTemplate(CurrentMeasure.Name, MiscToolsConstants.ExcelTabTemplateAnalysis, ExcelSheetPosition.START);
                        CurrentExcel.CurrentSheet = CurrentMeasure.Name;
                        CurrentExcel.SetCellValue(CurrentMeasure.Name, LineIndex, 2);

                        if (IsTraceLog) TraceLog.AddTraceLog($"Tab {CurrentMeasure.Name} Created");

                        CurrentAreaTableItem.ColIndex = 1;

                        // Update Min/Max value per Area
                        double MaxValue = 0;
                        double MinValue = 0;

                        if (IsTraceLog) TraceLog.AddTraceLog($"Update Area {CurrentMeasure.AllAreas.Count}");

                        foreach (var CurrentArea in CurrentMeasure.AllAreas.OrderBy((item) => item.Name))
                        {
                            MaxValue = 0;
                            MinValue = 0;
                            if (IsTraceLog) TraceLog.AddTraceLog($"Update Area {CurrentArea.Name} with nb states {CurrentArea.AllStates.Count}");
                            foreach (var CurrentState in CurrentArea.AllStates)
                            {
                                if (IsTraceLog) TraceLog.AddTraceLog($"Search Min/Max for {CurrentState.Name} - {CurrentState.AllResults.Count}");
                                MaxValue = Math.Max(MaxValue, CurrentState.AllResults.Select((item) => item.Value).Max());
                                MinValue = Math.Min(MinValue, CurrentState.AllResults.Select((item) => item.Value).Min());
                                if (IsTraceLog) TraceLog.AddTraceLog($"Min/Max searched for {CurrentState.Name}:{MinValue} - {MaxValue}");
                            }
                            if (MinValue > 0)
                                MinValue = 0;
                            if (MaxValue < 0)
                                MaxValue = 0;

                            if (IsTraceLog) TraceLog.AddTraceLog($"Min/max Area {CurrentArea.Name} {MinValue} - {MaxValue}");

                            LineIndex++;
                            CurrentExcel.MergeCells(LineIndex, 1, LineIndex, 3);
                            CurrentExcel.SetCellFontSize(LineIndex, 1, 15, true);
                            CurrentExcel.SetCellFontSize(LineIndex, 4, 15, true);
                            CurrentExcel.SetCellBorder(LineIndex, 1, McgExcelBorderStyle.Thin);
                            CurrentExcel.SetCellBorder(LineIndex, 2, McgExcelBorderStyle.Thin);
                            CurrentExcel.SetCellBorder(LineIndex, 3, McgExcelBorderStyle.Thin);
                            CurrentExcel.SetCellBorder(LineIndex, 4, McgExcelBorderStyle.Thin);
                            CurrentExcel.SetCellFontAlignment(LineIndex, 1, McgExcelHorizontalAlignment.Center);
                            CurrentExcel.SetCellFontAlignment(LineIndex, 2, McgExcelHorizontalAlignment.Center);
                            CurrentExcel.SetCellFontAlignment(LineIndex, 3, McgExcelHorizontalAlignment.Center);
                            CurrentExcel.SetCellFontAlignment(LineIndex, 4, McgExcelHorizontalAlignment.Center);
                            CurrentExcel.SetCellFontColor(LineIndex, 4, Color.Red);
                            CurrentExcel.SetCellValue($"Max négatif en {CurrentArea.Name}", LineIndex, 1);
                            CurrentExcel.SetCellValue($"{Math.Round(MinValue / 10)} daN", LineIndex, 4);
                            LineIndex++;
                            CurrentExcel.MergeCells(LineIndex, 1, LineIndex, 3);
                            CurrentExcel.SetCellFontSize(LineIndex, 1, 15, true);
                            CurrentExcel.SetCellFontSize(LineIndex, 4, 15, true);
                            CurrentExcel.SetCellFontAlignment(LineIndex, 1, McgExcelHorizontalAlignment.Center);
                            CurrentExcel.SetCellFontAlignment(LineIndex, 2, McgExcelHorizontalAlignment.Center);
                            CurrentExcel.SetCellFontAlignment(LineIndex, 3, McgExcelHorizontalAlignment.Center);
                            CurrentExcel.SetCellFontAlignment(LineIndex, 4, McgExcelHorizontalAlignment.Center);
                            CurrentExcel.SetCellBorder(LineIndex, 1, McgExcelBorderStyle.Thin);
                            CurrentExcel.SetCellBorder(LineIndex, 2, McgExcelBorderStyle.Thin);
                            CurrentExcel.SetCellBorder(LineIndex, 3, McgExcelBorderStyle.Thin);
                            CurrentExcel.SetCellBorder(LineIndex, 4, McgExcelBorderStyle.Thin);
                            CurrentExcel.SetCellValue($"Max positif en {CurrentArea.Name}", LineIndex, 1);
                            CurrentExcel.SetCellValue($"{Math.Round(MaxValue / 10)} daN", LineIndex, 4);
                        }

                        // create table per Area
                        // search all Position values
                        if (IsTraceLog) TraceLog.AddTraceLog($"Update Table Area");
                        List<double> AllPositions = new List<double>();
                        int nbStates = 1;
                        int MergeColStart = 0;
                        CurrentExcel.SetCellValue(CurrentMeasure.AxisPosition, LineIndex + 23, 1);
                        CurrentExcel.SetCellBackgroundColor(LineIndex + 23, 1, System.Drawing.ColorTranslator.FromHtml("#C6E0B4"));
                        CurrentExcel.SetCellFontSize(LineIndex + 23, 1, 11, true);
                        CurrentExcel.SetCellBorder(LineIndex + 23, 1, McgExcelBorderStyle.Thin);
                        foreach (var CurrentArea in CurrentMeasure.AllAreas)
                        {
                            if (IsTraceLog) TraceLog.AddTraceLog($"Update  table Area {CurrentArea.Name}");
                            CurrentExcel.SetCellValue(CurrentArea.Name, LineIndex + 22, nbStates + 1);
                            //CurrentExcel.SetCellBackgroundColor(LineIndex + 22, nbStates + 1, System.Drawing.Color.Green);
                            CurrentExcel.SetCellBackgroundColor(LineIndex + 22, nbStates + 1, System.Drawing.ColorTranslator.FromHtml("#C6E0B4"));
                            CurrentExcel.SetCellFontAlignment(LineIndex + 22, nbStates + 1, McgExcelHorizontalAlignment.Center);
                            CurrentExcel.SetCellFontSize(LineIndex + 22, nbStates + 1, 13, true);
                            MergeColStart = nbStates + 1;
                            foreach (var CurrentState in CurrentArea.AllStates)
                            {
                                if (IsTraceLog) TraceLog.AddTraceLog($"Update State {CurrentArea.AllStates.Count}");
                                nbStates++;
                                AllPositions = (AllPositions.Concat(CurrentState.AllResults.Select((item) => item.Position))).Distinct().ToList();
                                CurrentExcel.SetCellValue($"{CurrentState.Name}-{CurrentArea.Name}", LineIndex + 23, nbStates);
                                CurrentExcel.SetCellBackgroundColor(LineIndex + 23, nbStates, System.Drawing.ColorTranslator.FromHtml("#E2EFDA"));
                                CurrentExcel.SetCellBorder(LineIndex + 23, nbStates, McgExcelBorderStyle.Thin);
                                CurrentExcel.SetCellFontSize(LineIndex + 23, nbStates, 11, true);
                            }
                            CurrentExcel.MergeCells(LineIndex + 22, MergeColStart, LineIndex + 22, nbStates);
                            CurrentExcel.SetCellFontAlignment(LineIndex + 22, MergeColStart, McgExcelHorizontalAlignment.Center);
                            for (int indBorder = MergeColStart; indBorder <= nbStates; indBorder++)
                            {
                                CurrentExcel.SetCellBorder(LineIndex + 22, indBorder, McgExcelBorderStyle.Thin);
                            }

                        }

                        // Init Result table 
                        double?[,] AllResultTable = new double?[AllPositions.Count, nbStates];
                        int indResCol, indResLine;

                        // Update Result table
                        indResLine = 0;
                        if (IsTraceLog) TraceLog.AddTraceLog($"Update Table value ");
                        foreach (double position in AllPositions)
                        {
                            AllResultTable[indResLine, 0] = position;
                            indResLine++;
                        }
                        indResCol = 1;
                        indResLine = 0;
                        foreach (var CurrentArea in CurrentMeasure.AllAreas)
                        {
                            foreach (var CurrentState in CurrentArea.AllStates)
                            {
                                foreach (var CurrentResult in CurrentState.AllResults)
                                {
                                    indResLine = AllPositions.IndexOf(CurrentResult.Position);
                                    AllResultTable[indResLine, indResCol] = CurrentResult.Value;
                                }
                                indResCol++;
                            }
                        }

                        //Update Xls
                        if (IsTraceLog) TraceLog.AddTraceLog($"Update Excel Tab ");
                        for (int indLine = 0; indLine < AllPositions.Count; indLine++)
                        {
                            for (int indCol = 0; indCol < nbStates; indCol++)
                            {
                                CurrentExcel.SetCellValue(AllResultTable[indLine, indCol], indLine + LineIndex + 24, indCol + 1);
                                CurrentExcel.SetCellBorder(indLine + LineIndex + 24, indCol + 1, McgExcelBorderStyle.Thin);
                                CurrentExcel.SetCellFontAlignment(indLine + LineIndex + 24, indCol + 1, McgExcelHorizontalAlignment.Right);
                                if (indCol == 0)
                                    CurrentExcel.SetCellBackgroundColor(indLine + LineIndex + 24, indCol + 1, System.Drawing.ColorTranslator.FromHtml("#E2EFDA"));
                            }
                        }

                        // Create chart
                        AllCurves.Add(new AnalysisCurveItem()
                        {
                            Title = CurrentMeasure.Name,
                            XAxisTitle = CurrentMeasure.AxisPosition,
                            YAxisTitle = "",
                            CellStartRow = LineIndex + 23,
                            CellStartColumn = 1,
                            CellEndRow = AllPositions.Count + 23 + LineIndex,
                            CellEndColumn = nbStates,
                            Width = 830,
                            Height = 300,
                            RowPos = LineIndex,
                            ColPos = 0,
                            OffsetRowPos = 5,
                            OffsetColPos = 5,
                            FontSize = 10
                        });
                    }

                    CurrentExcel.HideSheet(MiscToolsConstants.ExcelTabTemplateAnalysis);
                    
                    if (IsTraceLog) TraceLog.AddTraceLog($"Save Excel");
                    if (CurrentExcel.SaveClose() != ExcelStatus.OK)
                    {
                        MessageBox.Show("Issue to close Excel file", "Excel Export Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }


                    if (IsTraceLog) TraceLog.AddTraceLog($"Create Charts");
                    CurrentExcel.OpenFileExcelWorkbook();
                    foreach (var curve in AllCurves)
                    {
                        CurrentExcel.CreateChart(curve.Title, curve.XAxisTitle, curve.YAxisTitle, curve.CellStartRow, curve.CellStartColumn, curve.CellEndRow, curve.CellEndColumn, curve.Width, curve.Height, curve.RowPos, curve.ColPos, curve.OffsetRowPos, curve.OffsetColPos, curve.FontSize);
                    }
                    if (IsTraceLog) TraceLog.AddTraceLog($"Save Excel");

                    if (CurrentExcel.SaveCloseFileExcelWorkbook() != ExcelStatus.OK)
                    {
                        MessageBox.Show("Issue to close Excel file", "Excel Export Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (newExcelProcess != null)
                        newExcelProcess.Kill();

                    _mcgCommonLibWindowService.ShowMcgWindowOkOpenFileView(McgWpfTools.GetStringResource("MMA_BtCreateExcel"), String.Format(McgWpfTools.GetStringResource("MMA_ExportXls"), XlsFileName), McgWpfTools.GetStringResource("MMA_ToolTipOpen"), McgWpfTools.GetStringResource("MMA_ToolTipOpenFolder"), McgWpfTools.GetStringResource("MMA_ToolTipClose"), XlsFileName);

                }
                else
                    throw new AnalysisFileContentException(this.GetType().Name);
            }
            catch (AnalysisFileContentException Aex)
            {
                if (CurrentExcel != null)
                    CurrentExcel.SaveClose();
                throw Aex;
            }
            catch (Exception ex)
            {
                if (CurrentExcel != null)
                    CurrentExcel.SaveClose();
                throw new MiscToolsException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
