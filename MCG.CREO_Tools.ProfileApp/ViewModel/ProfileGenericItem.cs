using MCG.CREO_Tools.ProfileApp.View;
using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CREO_Tools.ProfileApp.Exceptions;

namespace MCG.CREO_Tools.ProfileApp.ViewModel
{
    public class ProfileGenericItem : ObservableObject, IProfileGenericItem
    {
        #region [REGION] Properties from Interface
        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (this._Description != value)
                {
                    if (value != null)
                        this._Description = value.ToUpper();
                    else
                        this._Description = value;
                    OnPropertyChanged();
                }

            }
        }

        private double? _Width;
        public double? Width
        {
            get { return _Width; }
            set
            {
                if (this._Width != value)
                {
                    this._Width = value;
                    OnPropertyChanged();
                    UpdatePartNumber();
                }

            }
        }

        private double? _Height;
        public double? Height
        {
            get { return _Height; }
            set
            {
                if (this._Height != value)
                {
                    this._Height = value;
                    OnPropertyChanged();
                    UpdatePartNumber();
                }

            }
        }

        private double? _Thickness;
        public double? Thickness
        {
            get { return _Thickness; }
            set
            {
                if (this._Thickness != value)
                {
                    this._Thickness = value;
                    OnPropertyChanged();
                    UpdatePartNumber();
                }

            }
        }

        private string _StandardType;
        public string StandardType
        {
            get { return _StandardType; }
            set
            {
                if (this._StandardType != value)
                {
                    this._StandardType = value;
                    OnPropertyChanged();
                }

            }
        }


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

        private string _IdType;
        public string IdType
        {
            get { return _IdType; }
            set
            {
                if (this._IdType != value)
                {
                    this._IdType = value;
                    OnPropertyChanged();
                    UpdatePartNumber();
                }

            }
        }

        private string _ProfileGeneric;
        public string ProfileGeneric
        {
            get { return _ProfileGeneric; }
            set
            {
                if (this._ProfileGeneric != value)
                {
                    this._ProfileGeneric = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _DrwNumberCompleteView;
        public string DrwNumberCompleteView
        {
            get { return _DrwNumberCompleteView; }
            set
            {
                if (this._DrwNumberCompleteView != value)
                {
                    this._DrwNumberCompleteView = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _DrwNumberBrokenView;
        public string DrwNumberBrokenView
        {
            get { return _DrwNumberBrokenView; }
            set
            {
                if (this._DrwNumberBrokenView != value)
                {
                    this._DrwNumberBrokenView = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Material;
        public string Material
        {
            get { return _Material; }
            set
            {
                if (this._Material != value)
                {
                    this._Material = value;
                    OnPropertyChanged();
                    UpdatePartNumber();
                }

            }
        }

        private ProfileGenericItem _UpdatedProfile;
        public ProfileGenericItem UpdatedProfile
        {
            get { return _UpdatedProfile; }
            set
            {
                if (this._UpdatedProfile != value)
                {
                    this._UpdatedProfile = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        private Profilegeneric _OrigProfileGeneric;
        public Profilegeneric OrigProfileGeneric
        {
            get { return _OrigProfileGeneric; }
            set
            {
                if (this._OrigProfileGeneric != value && value != null)
                {
                    _OrigProfileGeneric = value;
                    Description = OrigProfileGeneric.Description;
                    Width = OrigProfileGeneric.Width;
                    Height = OrigProfileGeneric.Height;
                    Thickness = OrigProfileGeneric.Thickness;
                    StandardType = OrigProfileGeneric.Standardtype;
                    PartNumber = OrigProfileGeneric.Partnumber;
                    IdType = OrigProfileGeneric.Idtype;
                    ProfileGeneric = OrigProfileGeneric.Profilegeneric1;
                    DrwNumberBrokenView = OrigProfileGeneric.Drwnumberbrokenview;
                    DrwNumberCompleteView = OrigProfileGeneric.Drwnumbercompleteview;
                    Material = OrigProfileGeneric.Material;
                }
            }
        }

        public ProfileTypeItem TemplateProfileType { get; set; }

        public string OrigPartNumber { get; set; }
        #endregion

        #region [REGION] Misc
        public static void UpdateProfileGenericItem(ProfileGenericItem ToUpdate, ProfileGenericItem From)
        {
            try
            {
                if (From != null && ToUpdate != null)
                {
                    ToUpdate.Description = From.Description;
                    ToUpdate.Width = From.Width;
                    ToUpdate.Height = From.Height;
                    ToUpdate.Thickness = From.Thickness;
                    ToUpdate.StandardType = From.StandardType;
                    ToUpdate.PartNumber = From.PartNumber;
                    ToUpdate.TemplateProfileType = From.TemplateProfileType;
                    ToUpdate.IdType = From.IdType;
                    ToUpdate.ProfileGeneric = From.ProfileGeneric;
                    ToUpdate.DrwNumberBrokenView = From.DrwNumberBrokenView;
                    ToUpdate.DrwNumberCompleteView = From.DrwNumberCompleteView;
                    ToUpdate.Material = From.Material;
                    ToUpdate.OrigProfileGeneric = From.OrigProfileGeneric;
                }

            }
            catch (Exception ex)
            {
                throw new ProfileException("ProfileGenericItem.UpdateProfileGenericItem", ex);
            }
        }

        public static void UpdateProfileGenericItem(Profilegeneric ToUpdate, ProfileGenericItem From)
        {
            try
            {
                if (From != null && ToUpdate != null)
                {
                    ToUpdate.Description = From.Description;
                    ToUpdate.Width = From.Width;
                    ToUpdate.Height = From.Height;
                    ToUpdate.Thickness = From.Thickness;
                    ToUpdate.Standardtype = From.StandardType;
                    ToUpdate.Partnumber = From.PartNumber;
                    ToUpdate.Idtype = From.IdType;
                    ToUpdate.Profilegeneric1 = From.ProfileGeneric;
                    ToUpdate.Drwnumberbrokenview = From.DrwNumberBrokenView;
                    ToUpdate.Drwnumbercompleteview = From.DrwNumberCompleteView;
                    ToUpdate.Material = From.Material;
                }
            }
            catch (Exception ex)
            {
                throw new ProfileException("ProfileGenericItem.UpdateProfileGenericItem", ex);
            }
        }

        public Profilegeneric GetProfilGenericDb()
        {
            try
            {
                Profilegeneric CurrentProfGen = new Profilegeneric()
                {
                    Description = Description,
                    Drwnumberbrokenview = DrwNumberBrokenView,
                    Drwnumbercompleteview = DrwNumberCompleteView,
                    Height = Height,
                    Idtype = IdType,
                    Material = Material,
                    Partnumber = PartNumber,
                    Profilegeneric1 = ProfileGeneric,
                    Standardtype = StandardType,
                    Thickness = Thickness,
                    Width = Width
                };

                return CurrentProfGen;
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }

        public bool CheckProfileItem()
        {
            try
            {
                if (DrwNumberBrokenView == null ||
                    DrwNumberCompleteView == null ||
                    ProfileGeneric == null ||
                    Description == null ||
                    Material == null ||
                    StandardType == null ||
                    IdType == null ||
                    PartNumber == null ||
                    DrwNumberBrokenView.Trim() == "" ||
                    DrwNumberCompleteView.Trim() == "" ||
                    ProfileGeneric.Trim() == "" ||
                    Description.Trim() == "" ||
                    Material.Trim() == "" ||
                    StandardType.Trim() == "" ||
                    IdType.Trim() == "" ||
                    PartNumber.Trim() == "")
                    return false;

                if (TemplateProfileType == null)
                    return false;
                if (TemplateProfileType.ColHeightVisibility.ToUpper().Trim() == "TRUE" && (Height == null || Height <= 0))
                    return false;
                if (TemplateProfileType.ColThicknessVisibility.ToUpper().Trim() == "TRUE" && (Thickness == null || Thickness <= 0))
                    return false;
                if (TemplateProfileType.ColWidthVisibility.ToUpper().Trim() == "TRUE" && (Width == null || Width <= 0))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }

        private void UpdatePartNumber()
        {
            try
            {
                PartNumber = $"{IdType}_{Width}";
                if (TemplateProfileType != null)
                {
                    if (TemplateProfileType.ColHeightVisibility != null && TemplateProfileType.ColHeightVisibility.ToUpper().Trim() == "TRUE")
                        PartNumber = $"{PartNumber}X{Height}";
                    if (TemplateProfileType.ColThicknessVisibility != null && TemplateProfileType.ColThicknessVisibility.ToUpper().Trim() == "TRUE")
                        PartNumber = $"{PartNumber}X{Thickness}";
                }
                PartNumber = $"{PartNumber}_{Material}";
            }
            catch (Exception ex)
            {
                throw new ProfileException(this.GetType().Name, ex);
            }
        }
        #endregion
    }
}
