using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib;
using MCG.CommonLib.Models.SAP;
using MCG.CommonLib.Services.Interfaces;
using MCG.Tools.VisualizationLib.View;
using System;
using System.Collections.ObjectModel;

namespace MCG.Tools.VisualizationLib.ViewModel
{
    public class DownloadVisualizationFileDataContext : ObservableObject, IDownloadVisualizationFileDataContext
    {
        private readonly IMcgRandomPassword _mcgRanvdomPassword;
       
        private bool _ActionInProgress = false;
        public bool ActionInProgress
        {
            get { return _ActionInProgress; }
            set
            {
                if (this._ActionInProgress != value)
                {
                    this._ActionInProgress = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _FilterNumber = string.Empty;
        public string FilterNumber
        {
            get { return _FilterNumber; }
            set
            {
                if (this._FilterNumber != value)
                {
                    this._FilterNumber = value.ToUpper();
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsAllPartSelected = false;
        public bool IsAllPartSelected
        {
            get { return _IsAllPartSelected; }
            set
            {
                if (this._IsAllPartSelected != value)
                {
                    this._IsAllPartSelected = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsPdfTiffMainSelected = true;
        public bool IsPdfTiffMainSelected
        {
            get { return _IsPdfTiffMainSelected; }
            set
            {
                if (this._IsPdfTiffMainSelected != value)
                {
                    this._IsPdfTiffMainSelected = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }
            }
        }

        private bool _IsPdfTiffSelected = true;
        public bool IsPdfTiffSelected
        {
            get { return _IsPdfTiffSelected; }
            set
            {
                if (this._IsPdfTiffSelected != value)
                {
                    this._IsPdfTiffSelected = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }

            }
        }

        private bool _IsOfficeDocSelected = true;
        public bool IsOfficeDocSelected
        {
            get { return _IsOfficeDocSelected; }
            set
            {
                if (this._IsOfficeDocSelected != value)
                {
                    this._IsOfficeDocSelected = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }

            }
        }

        private bool _IsPvzSelected = true;
        public bool IsPvzSelected
        {
            get { return _IsPvzSelected; }
            set
            {
                if (this._IsPvzSelected != value)
                {
                    this._IsPvzSelected = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }

            }
        }

        private bool _IsDxfSelected = true;
        public bool IsDxfSelected
        {
            get { return _IsDxfSelected; }
            set
            {
                if (this._IsDxfSelected != value)
                {
                    this._IsDxfSelected = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }

            }
        }

        private bool _IsStepSelected = true;
        public bool IsStepSelected
        {
            get { return _IsStepSelected; }
            set
            {
                if (this._IsStepSelected != value)
                {
                    this._IsStepSelected = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }

            }
        }

        private bool _IsIgesSelected = true;
        public bool IsIgesSelected
        {
            get { return _IsIgesSelected; }
            set
            {
                if (this._IsIgesSelected != value)
                {
                    this._IsIgesSelected = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }

            }
        }

        private bool _IsOtherSelected = true;
        public bool IsOtherSelected
        {
            get { return _IsOtherSelected; }
            set
            {
                if (this._IsOtherSelected != value)
                {
                    this._IsOtherSelected = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }

            }
        }

        private bool _IsOptionalWatermark = false;
        public bool IsOptionalWatermark
        {
            get { return _IsOptionalWatermark; }
            set
            {
                if (this._IsOptionalWatermark != value)
                {
                    this._IsOptionalWatermark = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsDefaultWatermark = true;
        public bool IsDefaultWatermark
        {
            get { return _IsDefaultWatermark; }
            set
            {
                if (this._IsDefaultWatermark != value)
                {
                    this._IsDefaultWatermark = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsAdminActivated = false;
        public bool IsAdminActivated
        {
            get { return _IsAdminActivated; }
            set
            {
                if (this._IsAdminActivated != value)
                {
                    this._IsAdminActivated = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _AddRevisionInFileName = true;
        public bool AddRevisionInFileName
        {
            get { return _AddRevisionInFileName; }
            set
            {
                if (this._AddRevisionInFileName != value)
                {
                    this._AddRevisionInFileName = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _AddStateInFileName = true;
        public bool AddStateInFileName
        {
            get { return _AddStateInFileName; }
            set
            {
                if (this._AddStateInFileName != value)
                {
                    this._AddStateInFileName = value;
                    OnPropertyChanged();
                }

            }
        }


        public ObservableCollection<string> OptionalWatermarkValues { get; set; } = new ObservableCollection<string>();

        private string _OptionalWatermark = string.Empty;
        public string OptionalWatermark
        {
            get { return _OptionalWatermark; }
            set
            {
                if (this._OptionalWatermark != value)
                {
                    this._OptionalWatermark = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsSearchInProgress = false;
        public bool IsSearchInProgress
        {
            get { return _IsSearchInProgress; }
            set
            {
                if (this._IsSearchInProgress != value)
                {
                    this._IsSearchInProgress = value;
                    OnPropertyChanged();
                }

            }
        }

        private int _TotalStep = 1;
        public int TotalStep
        {
            get { return _TotalStep; }
            set
            {
                if (this._TotalStep != value)
                {
                    this._TotalStep = value;
                    OnPropertyChanged();
                }

            }
        }

        private int _CurrentStep = 0;
        public int CurrentStep
        {
            get { return _CurrentStep; }
            set
            {
                if (this._CurrentStep != value)
                {
                    this._CurrentStep = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _StatusBarTextRight = "";
        public string StatusBarTextRight
        {
            get { return _StatusBarTextRight; }
            set
            {
                if (this._StatusBarTextRight != value)
                {
                    this._StatusBarTextRight = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _StatusBarTextLeft="";
        public string StatusBarTextLeft
        {
            get { return _StatusBarTextLeft; }
            set
            {
                if (this._StatusBarTextLeft != value)
                {
                    this._StatusBarTextLeft = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsColAddedFromShown = true;
        public bool IsColAddedFromShown
        {
            get { return _IsColAddedFromShown; }
            set
            {
                if (this._IsColAddedFromShown != value)
                {
                    this._IsColAddedFromShown = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }

            }
        }

        private bool _IsColDescriptionEngShown = false;
        public bool IsColDescriptionEngShown
        {
            get { return _IsColDescriptionEngShown; }
            set
            {
                if (this._IsColDescriptionEngShown != value)
                {
                    this._IsColDescriptionEngShown = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }

            }
        }

        private bool _IsColDescriptionLocalShown = false;
        public bool IsColDescriptionLocalShown
        {
            get { return _IsColDescriptionLocalShown; }
            set
            {
                if (this._IsColDescriptionLocalShown != value)
                {
                    this._IsColDescriptionLocalShown = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }

            }
        }

        private bool _IsColPdmContextShown = false;
        public bool IsColPdmContextShown
        {
            get { return _IsColPdmContextShown; }
            set
            {
                if (this._IsColPdmContextShown != value)
                {
                    this._IsColPdmContextShown = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }

            }
        }

        public ObservableCollection<VisualizationItem> SearchedPartList { get; set; } = new ObservableCollection<VisualizationItem>();

        private VisualizationItem _SelectedPart;
        public VisualizationItem SelectedPart
        {
            get { return _SelectedPart; }
            set
            {
                if (this._SelectedPart != value)
                {
                    this._SelectedPart = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ActivatePdfSecurity = false;
        public bool ActivatePdfSecurity
        {
            get { return _ActivatePdfSecurity; }
            set
            {
                if (this._ActivatePdfSecurity != value)
                {
                    this._ActivatePdfSecurity = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _PdfUserPassword;
        public string PdfUserPassword
        {
            get { return _PdfUserPassword; }
            set
            {
                if (this._PdfUserPassword != value)
                {
                    this._PdfUserPassword = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _PdfOwnerPassword;
        public string PdfOwnerPassword
        {
            get { return _PdfOwnerPassword; }
            set
            {
                if (this._PdfOwnerPassword != value)
                {
                    this._PdfOwnerPassword = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _PdfPermitAnnotation = true;
        public bool PdfPermitAnnotation
        {
            get { return _PdfPermitAnnotation; }
            set
            {
                if (this._PdfPermitAnnotation != value)
                {
                    this._PdfPermitAnnotation = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _PdfPermitExtractContent = false;
        public bool PdfPermitExtractContent
        {
            get { return _PdfPermitExtractContent; }
            set
            {
                if (this._PdfPermitExtractContent != value)
                {
                    this._PdfPermitExtractContent = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _PdfPermitModify = false;
        public bool PdfPermitModify
        {
            get { return _PdfPermitModify; }
            set
            {
                if (this._PdfPermitModify != value)
                {
                    this._PdfPermitModify = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _PdfPermitPrint = true;
        public bool PdfPermitPrint
        {
            get { return _PdfPermitPrint; }
            set
            {
                if (this._PdfPermitPrint != value)
                {
                    this._PdfPermitPrint = value;
                    OnPropertyChanged();
                }

            }
        }


        private bool _ActivatePdfConvert = true;
        public bool ActivatePdfConvert
        {
            get { return _ActivatePdfConvert; }
            set
            {
                if (this._ActivatePdfConvert != value)
                {
                    this._ActivatePdfConvert = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ActivateTiffConvert = true;
        public bool ActivateTiffConvert
        {
            get { return _ActivateTiffConvert; }
            set
            {
                if (this._ActivateTiffConvert != value)
                {
                    this._ActivateTiffConvert = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ActivateWordConvert = false;
        public bool ActivateWordConvert
        {
            get { return _ActivateWordConvert; }
            set
            {
                if (this._ActivateWordConvert != value)
                {
                    this._ActivateWordConvert = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ActivateExcelConvert = false;
        public bool ActivateExcelConvert
        {
            get { return _ActivateExcelConvert; }
            set
            {
                if (this._ActivateExcelConvert != value)
                {
                    this._ActivateExcelConvert = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ActivatePowerPointConvert = false;
        public bool ActivatePowerPointConvert
        {
            get { return _ActivatePowerPointConvert; }
            set
            {
                if (this._ActivatePowerPointConvert != value)
                {
                    this._ActivatePowerPointConvert = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _ExportFolder;
        public string ExportFolder
        {
            get { return _ExportFolder; }
            set
            {
                if (this._ExportFolder != value)
                {
                    this._ExportFolder = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsCreateZip = true;
        public bool IsCreateZip
        {
            get { return _IsCreateZip; }
            set
            {
                if (this._IsCreateZip != value)
                {
                    this._IsCreateZip = value;
                    OnPropertyChanged();
                    RaiseUserConfigurationUpdateEvent();
                }

            }
        }

        public ObservableCollection<SapPlant> AllSapPlants { get; set; } = new ObservableCollection<SapPlant>();

        private SapPlant _Plant;
        public SapPlant Plant
        {
            get { return _Plant; }
            set
            {
                if (this._Plant != value)
                {
                    this._Plant = value;
                    OnPropertyChanged();
                }

            }
        }

        public ObservableCollection<SapBomUsage> AllBomUsage { get; set; } = new ObservableCollection<SapBomUsage>();

        private SapBomUsage _BomUsage;
        public SapBomUsage BomUsage
        {
            get { return _BomUsage; }
            set
            {
                if (this._BomUsage != value)
                {
                    this._BomUsage = value;
                    OnPropertyChanged();
                }

            }
        }

        private DateTime _DateValidity = DateTime.Today;
        public DateTime DateValidity
        {
            get { return _DateValidity; }
            set
            {
                if (this._DateValidity != value)
                {
                    this._DateValidity = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _ShowLatestApproved = false;
        public bool ShowLatestApproved
        {
            get { return _ShowLatestApproved; }
            set
            {
                if (this._ShowLatestApproved != value)
                {
                    this._ShowLatestApproved = value;
                    OnPropertyChanged();
                }
            }
        }

        public event EventHandler UserConfigurationUpdateEvent;
        public void RaiseUserConfigurationUpdateEvent()
        {
            try
            {
                UserConfigurationUpdateEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        event EventHandler SecurityChangeEvent;
        public void RaiseSecurityChangeEvent()
        {
            try
            {
                SecurityChangeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public DownloadVisualizationFileDataContext(IMcgRandomPassword mcgRandomPassword)
        {
            _mcgRanvdomPassword = mcgRandomPassword;
            _PdfOwnerPassword = _mcgRanvdomPassword.Generate(15);
            _PdfUserPassword = _mcgRanvdomPassword.Generate(15);
        }
    }
}
