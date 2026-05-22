using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.Models.Main;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.CadAutoColor;

namespace MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColor
{
    public class CadAutoColorCreoColor : ObservableObject, ICadAutoColorCreoColor
    {
        #region [REGION] Properties from Interface
        private string _ColorCode;
        public string ColorCode
        {
            get { return _ColorCode; }
            set
            {
                if (this._ColorCode != value)
                {
                    this._ColorCode = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _ColorName;
        public string ColorName
        {
            get { return _ColorName; }
            set
            {
                if (this._ColorName != value)
                {
                    this._ColorName = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public bool IsAlreadyAssigned { get; set; } = false;
        #endregion

        #region [REGION] Misc
        public static CadAutoColorCreoColor GetCadAutoColorCreoColor(McgAppearanceItem pMcgAppearanceItem)
        {
            try
            {
                if (pMcgAppearanceItem != null)
                    return new CadAutoColorCreoColor()
                    {
                        ColorName = pMcgAppearanceItem.Name,
                        ColorCode = pMcgAppearanceItem.ColorHexa,
                        IsAlreadyAssigned = false
                    };
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new MiscToolsException("CadAutoColorItem.GetCadAutoColorItem", ex);
            }
        }
        #endregion

        public override string ToString()
        {
            return "";
        }
    }
}
