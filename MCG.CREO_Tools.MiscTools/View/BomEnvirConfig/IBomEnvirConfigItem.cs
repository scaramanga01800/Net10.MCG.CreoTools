namespace MCG.CREO_Tools.MiscTools.View.BomEnvirConfig
{
    public interface IBomEnvirConfigItem
    {
        string Number { get; set; }
        string AsmName { get; set; }
        string OldAsmName { get; set; }
        string Rep { get; set; }
        string RepFct { get; set; }
        string Comment { get; set; }
        int Level { get; set; }
        int CompOrder { get; set; }
    }
}
