namespace MCG.CREO_Tools.DxfExport.View
{
    interface IDxfExportItem
    {
        string Number { get; set; }
        string Status { get; set; }
        string Comment { get; set; }
    }
}
