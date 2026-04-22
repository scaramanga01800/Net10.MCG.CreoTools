using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.Enums;
using MCG.Tools.VisualizationLib.Exceptions;
using MCG.Tools.VisualizationLib.View;
using System;
using System.Linq;

namespace MCG.Tools.VisualizationLib.ViewModel
{
    public class ConvertToPdfItem : ObservableObject, IConvertToPdfItem
    {
        #region [REGION] Properties from Interface
        private string _OrigFileName = string.Empty;
        public string OrigFileName
        {
            get { return _OrigFileName; }
            set
            {
                if (this._OrigFileName != value)
                {
                    this._OrigFileName = value;
                    OnPropertyChanged();
                    UpdateDocumentType();
                }

            }
        }

        private string _ShortFileName = string.Empty;
        public string ShortFileName
        {
            get { return _ShortFileName; }
            set
            {
                if (this._ShortFileName != value)
                {
                    this._ShortFileName = value;
                    OnPropertyChanged();
                }

            }
        }


        private string _Status = string.Empty;
        public string Status
        {
            get { return _Status; }
            set
            {
                if (this._Status != value)
                {
                    this._Status = value;
                    OnPropertyChanged();
                }

            }
        }

        private bool _IsSelected = true;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (this._IsSelected != value)
                {
                    this._IsSelected = value;
                    OnPropertyChanged();
                    RaiseIsSelectedEvent();
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
        public string ConvertedFileName { get; set; } = string.Empty;

        public bool IsConvertToPdfSuccesfull { get; set; }

        public int Order { get; set; } = 0;
        #endregion

        #region [REGION] Events
        public event EventHandler IsSelectedEvent;

        public void RaiseIsSelectedEvent()
        {
            try
            {
                IsSelectedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region [REGION] Misc
        private void UpdateDocumentType()
        {
            try
            {
                if (OrigFileName != null)
                {
                    ShortFileName = OrigFileName.Split('\\').LastOrDefault();

                    switch (OrigFileName.ToUpper().Split('.').LastOrDefault())
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
                            if (OrigFileName.ToUpper().Contains("IGS") || OrigFileName.ToUpper().Contains("IGES"))
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
