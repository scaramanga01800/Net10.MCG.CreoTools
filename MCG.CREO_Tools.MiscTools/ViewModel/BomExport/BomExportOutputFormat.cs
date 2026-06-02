
namespace MCG.CREO_Tools.MiscTools.ViewModel.BomExport
{
    public class BomExportOutputFormat
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public bool NeedFieldSeparator { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
