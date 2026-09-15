namespace MCG.CREO_Tools.DxfExport.ViewModel
{
    /// <summary>
    /// Formats d'export disponibles pour le module DxfExport.
    /// La valeur par défaut (Dxf) garantit la non-régression du comportement existant.
    /// </summary>
    public enum ExportFormatType
    {
        Dxf,
        Iges,
        Step
    }
}
