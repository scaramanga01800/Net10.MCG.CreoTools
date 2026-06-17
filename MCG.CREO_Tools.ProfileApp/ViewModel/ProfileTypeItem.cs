using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CREO_Tools.ProfileApp.View;

namespace MCG.CREO_Tools.ProfileApp.ViewModel
{
    public class ProfileTypeItem : ObservableObject, IProfileTypeItem
    {
        #region [REGION] Properties from Interface
        private string _ColWidthVisibility;
        public string ColWidthVisibility
        {
            get { return _ColWidthVisibility; }
            set
            {
                if (this._ColWidthVisibility != value)
                {
                    this._ColWidthVisibility = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _ColHeightVisibility;
        public string ColHeightVisibility
        {
            get { return _ColHeightVisibility; }
            set
            {
                if (this._ColHeightVisibility != value)
                {
                    this._ColHeightVisibility = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _ColThicknessVisibility;
        public string ColThicknessVisibility
        {
            get { return _ColThicknessVisibility; }
            set
            {
                if (this._ColThicknessVisibility != value)
                {
                    this._ColThicknessVisibility = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        private Profiletype _OrigProfileType;
        public Profiletype OrigProfileType
        {
            get { return _OrigProfileType; }
            set
            {
                if (this._OrigProfileType != value && value != null)
                {
                    _OrigProfileType = value;
                    ColHeightVisibility = OrigProfileType.Colheightvisibility;
                    ColThicknessVisibility = OrigProfileType.Colthicknessvisibility;
                    ColWidthVisibility = OrigProfileType.Colwidthvisibility;
                }
            }
        }

        public string DescriptionShown { get; set; }
        #endregion

        #region [REGION]Misc
        public override string ToString()
        {
            if (DescriptionShown != null)
                return DescriptionShown;
            else if (OrigProfileType != null && OrigProfileType.Description != null)
                return OrigProfileType.Description;
            else
                return base.ToString();
        }
        #endregion
    }
}
