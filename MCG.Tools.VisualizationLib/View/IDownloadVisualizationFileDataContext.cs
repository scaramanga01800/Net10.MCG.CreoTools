using MCG.CommonLib.Models.SAP;
using MCG.Tools.VisualizationLib.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCG.Tools.VisualizationLib.View
{
    public interface IDownloadVisualizationFileDataContext
    {
        bool ActionInProgress { get; set; }
        string FilterNumber { get; set; }
        bool IsAllPartSelected { get; set; }
        bool IsPdfTiffMainSelected { get; set; }
        bool IsPdfTiffSelected { get; set; }
        bool IsOfficeDocSelected { get; set; }
        bool IsPvzSelected { get; set; }
        bool IsDxfSelected { get; set; }
        bool IsStepSelected { get; set; }
        bool IsIgesSelected { get; set; }
        bool IsOtherSelected { get; set; }
        bool IsOptionalWatermark { get; set; }
        bool IsDefaultWatermark { get; set; }
        bool IsAdminActivated { get; set; }

        bool AddRevisionInFileName { get; set; }
        bool AddStateInFileName { get; set; }

        ObservableCollection<string> OptionalWatermarkValues { get; set; }
        string OptionalWatermark { get; set; }
        bool IsSearchInProgress { get; set; }
        int TotalStep { get; set; }
        int CurrentStep { get; set; }
        string StatusBarTextRight { get; set; }
        string StatusBarTextLeft { get; set; }


        bool IsColAddedFromShown { get; set; }
        bool IsColDescriptionEngShown { get; set; }
        bool IsColDescriptionLocalShown { get; set; }
        bool IsColPdmContextShown { get; set; }

        ObservableCollection<VisualizationItem> SearchedPartList { get; set; }

        VisualizationItem SelectedPart { get; set; }

        bool ActivatePdfSecurity { get; set; }
        string PdfUserPassword { get; set; }
        string PdfOwnerPassword { get; set; }
        bool PdfPermitAnnotation { get; set; }
        bool PdfPermitExtractContent { get; set; }
        bool PdfPermitModify { get; set; }
        bool PdfPermitPrint { get; set; }

        bool ActivatePdfConvert { get; set; }
        bool ActivateTiffConvert { get; set; }
        bool ActivateWordConvert { get; set; }
        bool ActivateExcelConvert { get; set; }
        bool ActivatePowerPointConvert { get; set; }

        string ExportFolder { get; set; }

        bool IsCreateZip { get; set; }

        ObservableCollection<SapPlant> AllSapPlants { get; set; }
        SapPlant Plant { get; set; }
        ObservableCollection<SapBomUsage> AllBomUsage { get; set; }
        SapBomUsage BomUsage { get; set; }
        DateTime DateValidity { get; set; }

        bool ShowLatestApproved { get; set; }
    }
}
