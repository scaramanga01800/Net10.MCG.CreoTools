using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.PurchaseOrderFollowUp.View;
using System.Collections.Generic;

namespace MCG.Tools.PurchaseOrderFollowUp.ViewModel
{
    public class PurchaseOrderLocation: ObservableObject, IPurchaseOrderLocation
    {
        #region [REGION] Properties from Interface
        private string _Name = string.Empty;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Description = string.Empty;
        public string Description
        {
            get { return _Description; }
            set
            {
                if (this._Description != value)
                {
                    this._Description = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Country = string.Empty;
        public string Country
        {
            get { return _Country; }
            set
            {
                if (this._Country != value)
                {
                    this._Country = value;
                    OnPropertyChanged();
                }

            }
        }

        private string _Adress = string.Empty;
        public string Adress
        {
            get { return _Adress; }
            set
            {
                if (this._Adress != value)
                {
                    this._Adress = value;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region [REGION] Internal variables
        public string Number { get; set; } = string.Empty;

        public List<PurchaseOrderMail> ListMail { get; set; }
        #endregion

        #region [REGION] Misc
        public override string ToString()
        {
            return $"{Name}";
        }
        #endregion
    }
}
