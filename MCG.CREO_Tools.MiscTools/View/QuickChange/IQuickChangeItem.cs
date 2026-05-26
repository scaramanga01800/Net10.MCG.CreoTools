namespace MCG.CREO_Tools.MiscTools.View.QuickChange
{
    public interface IQuickChangeItem
    {
        string CurrentNumber { get; set; }
        string NewNumber { get; set; }
        int Level { get; set; }
        int NbInstance { get; set; }
        string ParentNumber { get; set; }
    }
}
