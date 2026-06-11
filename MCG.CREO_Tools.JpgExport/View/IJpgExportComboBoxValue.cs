namespace MCG.CREO_Tools.JpgExport.View
{
    public interface IJpgExportComboBoxValue
    {
        string Value { get; set; }
        string ValueShown { get; set; }

        string ToString();
    }
}