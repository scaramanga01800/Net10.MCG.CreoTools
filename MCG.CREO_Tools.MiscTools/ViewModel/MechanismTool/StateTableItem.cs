namespace MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool
{
    public class StateTableItem
    {
        public string StateName { get; set; }
        public List<double?> AllValues { get; set; } = new List<double?>();
        public int ColIndex { get; set; }
    }
}
