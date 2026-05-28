using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.WebtermLib.Models;
using MCG.CREO_Tools.MiscTools.Exceptions;
using MCG.CREO_Tools.MiscTools.View.WebtermRequest;

namespace MCG.CREO_Tools.MiscTools.ViewModel.WebtermRequest
{
    public class WebtermRequestClass : ObservableObject, IWebtermRequestClass
    {
        private string _NameEn;
        public string NameEn
        {
            get { return _NameEn; }
            set
            {
                if (this._NameEn != value)
                {
                    this._NameEn = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _NameFr;
        public string NameFr
        {
            get { return _NameFr; }
            set
            {
                if (this._NameFr != value)
                {
                    this._NameFr = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _NameDe;
        public string NameDe
        {
            get { return _NameDe; }
            set
            {
                if (this._NameDe != value)
                {
                    this._NameDe = value;
                    OnPropertyChanged();
                }

            }
        }

        public static WebtermRequestClass GetClass(WebtermClass currentClass)
        {
            try
            {
                return new WebtermRequestClass()
                {
                    NameEn = currentClass.NameEn,
                    NameFr = currentClass.NameFr,
                    NameDe = currentClass.NameDe
                };
            }
            catch (Exception ex)
            {
               throw new MiscToolsException("WebtermRequestClass", ex);
            }

        }
    }
}


