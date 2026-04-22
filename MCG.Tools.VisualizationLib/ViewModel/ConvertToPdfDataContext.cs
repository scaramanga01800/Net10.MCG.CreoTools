using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib;
using MCG.CommonLib.Services;
using MCG.CommonLib.Services.Interfaces;
using MCG.Tools.VisualizationLib.View;
using System;
using System.Collections.ObjectModel;

namespace MCG.Tools.VisualizationLib.ViewModel
{
    public class ConvertToPdfDataContext : ObservableObject, IConvertToPdfDataContext
    {
        private readonly IMcgRandomPassword _mcgRandomPassword;

        public ObservableCollection<ConvertToPdfItem> ListConvertItem { get; set; } = new ObservableCollection<ConvertToPdfItem>();

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
                    RaiseSecurityWatermarkChangeEvent();
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
                    RaiseSecurityWatermarkChangeEvent();
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
                    RaiseSecurityWatermarkChangeEvent();
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
                    RaiseSecurityWatermarkChangeEvent();
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
                    RaiseSecurityWatermarkChangeEvent();
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
                    RaiseSecurityWatermarkChangeEvent();
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
                    RaiseSecurityWatermarkChangeEvent();
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
                    RaiseSecurityWatermarkChangeEvent();
                }

            }
        }
        public ObservableCollection<string> OptionalWatermarkValues { get; set; } = new ObservableCollection<string>();

        private string _OptionalWatermark;
        public string OptionalWatermark
        {
            get { return _OptionalWatermark; }
            set
            {
                if (this._OptionalWatermark != value)
                {
                    this._OptionalWatermark = value;
                    OnPropertyChanged();
                    RaiseSecurityWatermarkChangeEvent();
                }

            }
        }

        public event EventHandler SecurityWatermarkChangeEvent;
        public void RaiseSecurityWatermarkChangeEvent()
        {
            try
            {
                SecurityWatermarkChangeEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public ConvertToPdfDataContext(IMcgRandomPassword mcgRandomPassword)
        {
            _mcgRandomPassword = mcgRandomPassword;
            _PdfOwnerPassword = _mcgRandomPassword.Generate(15);
            _PdfUserPassword = _mcgRandomPassword.Generate(15);
        }
    }
}
