namespace MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool
{
    public class AreaTableItem
    {
        public List<double> AllPositions { get; set; } = new List<double>();
        public List<StateTableItem> AllStates { get; set; }
        public int ColIndex { get; set; }
    }
}
