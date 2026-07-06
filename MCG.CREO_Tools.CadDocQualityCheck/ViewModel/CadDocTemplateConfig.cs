using MCG.CREO_Tools.CadDocQualityCheck.Exceptions;

namespace MCG.CREO_Tools.CadDocQualityCheck.ViewModel
{
    public class CadDocTemplateConfig
    {
        public string FileName { get; set; }
        public string Template { get; set; }
        public string CadDocType { get; set; }
        public bool IsDefaultAsm { get; set; } = false;
        public bool IsDefaultPrt { get; set; } = false;
        public bool IsDefaultSheetMetal { get; set; } = false;
        public bool IsDefaultBulk { get; set; } = false;

        public CadDocTemplate GetCadDocTemplate()
        {
            try
            {
                return new CadDocTemplate()
                {
                    FileName = FileName,
                    Template = Template,
                    CadDocType = CadDocType,
                    IsDefaultAsm = IsDefaultAsm,
                    IsDefaultPrt = IsDefaultPrt,
                    IsDefaultSheetMetal = IsDefaultSheetMetal,
                    IsDefaultBulk = IsDefaultBulk
                };
            }
            catch (Exception ex)
            {
                throw new CadDocQualityCheckException(this.GetType().Name, ex);
            }
        }
    }
}
