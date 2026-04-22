using MCG.Tools.VisualizationLib.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCG.Tools.VisualizationLib.View
{
    public interface IConvertToPdfDataContext
    {
        ObservableCollection<ConvertToPdfItem> ListConvertItem { get; set; }
        bool ActionInProgress { get; set; }
        string ExportFolder { get; set; }

        bool ActivatePdfSecurity { get; set; }
        string PdfUserPassword { get; set; }
        string PdfOwnerPassword { get; set; }
        bool PdfPermitAnnotation { get; set; }
        bool PdfPermitExtractContent { get; set; }
        bool PdfPermitModify { get; set; }
        bool PdfPermitPrint { get; set; }

        bool IsOptionalWatermark { get; set; }
        ObservableCollection<string> OptionalWatermarkValues { get; set; }
        string OptionalWatermark { get; set; }
    }
}
