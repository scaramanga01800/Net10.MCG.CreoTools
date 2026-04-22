using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Pdf;
using MCG.Tools.VisualizationLib.Exceptions;
using MCG.Tools.VisualizationLib.View;
using MCG.WindchillRequestTool;
using MCG.WindchillRequestTool.Model.RestOdata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCG.Tools.VisualizationLib.ViewModel
{
    public class VisualizationDocument : ObservableObject, IVisualizationDocument
    {
        #region [REGION] Properties from Interface
        private string _DocumentNumber;
        public string DocumentNumber
        {
            get { return _DocumentNumber; }
            set
            {
                if (this._DocumentNumber != value)
                {
                    this._DocumentNumber = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _DocumentRevision;
        public string DocumentRevision
        {
            get { return _DocumentRevision; }
            set
            {
                if (this._DocumentRevision != value)
                {
                    this._DocumentRevision = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Comment;
        public string Comment
        {
            get { return _Comment; }
            set
            {
                if (this._Comment != value)
                {
                    this._Comment = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsSelected = false;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (this._IsSelected != value)
                {
                    this._IsSelected = value;
                    OnPropertyChanged();
                }

            }
        }

        private DocumentTypeEnum _DocumentType = DocumentTypeEnum.OTHER;
        public DocumentTypeEnum DocumentType
        {
            get { return _DocumentType; }
            set
            {
                if (this._DocumentType != value)
                {
                    this._DocumentType = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        private string _FileName;
        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (this._FileName != value)
                {
                    this._FileName = value;
                    UpdateDocumentType();
                }

            }
        }

        public RestOdataAttachment WindchillEcn { get; set; }
        public WindchillObjectViewableItem WindchillDocument { get; set; }
        public WindchillObjectViewable WindchillPartViewable { get; set; }

        public bool IsMainDrawing { get; set; } = false;

        public bool IsDefaultWatermark { get; set; } = false; 
        public bool IsOptionaltWatermark { get; set; } = false;
        public string OptionalWatermark { get; set; } = "";

        public List<PdfToolsWatermarkItem> ListWatermark { get; set; }

        public bool IsAlreadyDownloaded { get; set; } = false;
        public ViewableResult Viewable { get; set; }
        #endregion

        #region [REGION] Misc
        private void UpdateDocumentType()
        {
            try
            {
                if (FileName != null)
                {
                    switch (FileName.ToUpper().Split('.').LastOrDefault())
                    {
                        case "DOC":
                        case "DOCX":
                        case "DOCM":
                            DocumentType = DocumentTypeEnum.WORD;
                            break;
                        case "XLS":
                        case "XLSX":
                        case "XLSM":
                            DocumentType = DocumentTypeEnum.EXCEL;
                            break;
                        case "TIF":
                        case "TIFF":
                            DocumentType = DocumentTypeEnum.TIFF;
                            break;
                        case "PDF":
                            DocumentType = DocumentTypeEnum.PDF;
                            break;
                        case "PPT":
                        case "PPTX":
                        case "PPTM":
                            DocumentType = DocumentTypeEnum.POWERPOINT;
                            break;
                        case "IGS":
                        case "IGES":
                            DocumentType = DocumentTypeEnum.IGES;
                            break;
                        case "STP":
                        case "STEP":
                            DocumentType = DocumentTypeEnum.STEP;
                            break;
                        case "DXF":
                            DocumentType = DocumentTypeEnum.DXF;
                            break;
                        case "DWG":
                            DocumentType = DocumentTypeEnum.DWG;
                            break;
                        case "PVZ":
                            DocumentType = DocumentTypeEnum.PVZ;
                            break;
                        case "ZIP":
                            if (FileName.ToUpper().Contains("IGS") || FileName.ToUpper().Contains("IGES"))
                                DocumentType = DocumentTypeEnum.IGES;
                            else
                                DocumentType = DocumentTypeEnum.PVZ;
                            break;
                        default:
                            DocumentType = DocumentTypeEnum.OTHER;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new VisualizationException(this.GetType().Name, ex);

            }
        }
        #endregion

    }
}
