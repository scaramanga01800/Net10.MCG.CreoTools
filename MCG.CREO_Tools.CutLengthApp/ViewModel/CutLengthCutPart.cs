using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CREO_Tools.CutLengthApp.Exceptions;
using MCG.CREO_Tools.CutLengthApp.View;

namespace MCG.CREO_Tools.CutLengthApp.ViewModel
{
    public class CutLengthCutPart : ObservableObject, ICutLengthCutPart
    {
        #region [REGION] Properties from Interface
        private string _PartNumber;
        public string PartNumber
        {
            get { return _PartNumber; }
            set
            {
                if (this._PartNumber != value)
                {
                    this._PartNumber = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _PartName;
        public string PartName
        {
            get { return _PartName; }
            set
            {
                if (this._PartName != value)
                {
                    this._PartName = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _CadDocType;
        public string CadDocType
        {
            get { return _CadDocType; }
            set
            {
                if (this._CadDocType != value)
                {
                    this._CadDocType = value;
                    OnPropertyChanged();
                }
            }
        }


        private CutLengthCutPart _UpdatedPart;
        public CutLengthCutPart UpdatedPart
        {
            get { return _UpdatedPart; }
            set
            {
                if (this._UpdatedPart != value)
                {
                    this._UpdatedPart = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public int ID { get; set; }

        public string IdClass { get; set; }
        #endregion

        public string OrigPartNumber { get; set; }
        #region [REGION] Misc
        public override string ToString()
        {
            return $"{PartNumber}: {PartName}";
        }

        public static CutLengthCutPart GetCutLengthCutPart(Cutlengthpart CurrentPart)
        {
            try
            {
                return new CutLengthCutPart()
                {
                    CadDocType = CurrentPart.Caddoctype,
                    ID = CurrentPart.Id,
                    IdClass = CurrentPart.Idclass,
                    PartName = CurrentPart.Partname,
                    PartNumber = CurrentPart.Partnumber,
                    OrigPartNumber = CurrentPart.Partnumber
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Cutlengthpart GetCutLengthCutPart(CutLengthCutPart CurrentPart)
        {
            try
            {
                return new Cutlengthpart()
                {
                    Caddoctype = CurrentPart.CadDocType,
                    Idclass = CurrentPart.IdClass,
                    Partname = CurrentPart.PartName,
                    Partnumber = CurrentPart.PartNumber
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void UpdateCutLengthCutPart(Cutlengthpart ToBeUpdated, CutLengthCutPart From)
        {
            try
            {
                ToBeUpdated.Partnumber = From.PartNumber;
                ToBeUpdated.Partname = From.PartName;
                ToBeUpdated.Idclass = From.IdClass;
                ToBeUpdated.Caddoctype = From.CadDocType;
            }
            catch (Exception ex)
            {
                throw new CutLengthException("CutLengthCutPart.UpdateCutLengthCutPart", ex);
            }
        }

        public static void UpdateCutLengthCutPart(CutLengthCutPart ToBeUpdated, CutLengthCutPart From)
        {
            try
            {
                ToBeUpdated.PartNumber = From?.PartNumber;
                ToBeUpdated.PartName = From?.PartName;
                ToBeUpdated.ID = From.ID;
                ToBeUpdated.IdClass = From?.IdClass;
                ToBeUpdated.CadDocType = From?.CadDocType;
            }
            catch (Exception ex)
            {
                throw new CutLengthException("CutLengthCutPart.UpdateCutLengthCutPart", ex);
            }
        }
        #endregion
    }
}
