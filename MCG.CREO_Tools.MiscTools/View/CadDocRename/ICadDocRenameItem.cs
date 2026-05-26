namespace MCG.CREO_Tools.MiscTools.View.CadDocRename
{
    public interface ICadDocRenameItem
    {
        string OldNumber { get; set; }
        string NewNumber { get; set; }
        string Comment { get; set; }
    }
}
