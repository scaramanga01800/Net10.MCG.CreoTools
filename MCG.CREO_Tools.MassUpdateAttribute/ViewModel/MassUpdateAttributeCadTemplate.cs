namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class MassUpdateAttributeCadTemplate
    {
        public string FileName { get; set; }
        public string Template { get; set; }
        public string CadDocType { get; set; }
        public bool IsDefaultAsm { get; set; } = false;
        public bool IsDefaultPrt { get; set; } = false;
        public bool IsDefaultSheetMetal { get; set; } = false;
        public bool IsDefaultBulk { get; set; } = false;
    }
}
