using CommunityToolkit.Mvvm.ComponentModel;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CREO_Tools.CutLengthApp.View;

namespace MCG.CREO_Tools.CutLengthApp.ViewModel
{
    public class CutLengthType : ObservableObject, ICutLengthType
    {
        #region [REGION] Properties from Interface
        private string _ClassNameShown;
        public string ClassNameShown
        {
            get { return _ClassNameShown; }
            set
            {
                if (this._ClassNameShown != value)
                {
                    this._ClassNameShown = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Unit;
        public string Unit
        {
            get { return _Unit; }
            set
            {
                if (this._Unit != value)
                {
                    this._Unit = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public string ClassName { get; set; }
        public int ID { get; set; }
        public string IdClass { get; set; }
        public string IsActivated { get; set; }
        public string BulkOnly { get; set; }
        public string Product { get; set; }
        #endregion

        #region [REGION] Misc
        public override string ToString()
        {
            return ClassNameShown;
        }

        public static CutLengthType GetCutLengthType(Cutlengthclass CurrentClass)
        {
            try
            {
                return new CutLengthType
                {
                    BulkOnly = CurrentClass.Bulkonly,
                    ClassName = CurrentClass.Classname,
                    ID = CurrentClass.Id,
                    IdClass = CurrentClass.Idclass,
                    IsActivated = CurrentClass.Isactivated,
                    Product = CurrentClass.Product,
                    Unit = CurrentClass.Unit
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion
    }
}
