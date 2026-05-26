namespace MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool
{
    public class AnalysisCurveItem
    {
        public string Title { get; set; }
        public string XAxisTitle { get; set; }
        public string YAxisTitle { get; set; }
        public int CellStartRow { get; set; }
        public int CellStartColumn { get; set; }
        public int CellEndRow { get; set; }
        public int CellEndColumn { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int RowPos { get; set; }
        public int ColPos { get; set; }
        public int OffsetRowPos { get; set; }
        public int OffsetColPos { get; set; }
        public float FontSize { get; set; }
    }
}
