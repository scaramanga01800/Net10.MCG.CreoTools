using CommunityToolkit.Mvvm.ComponentModel;

namespace MCG.Tools.EcnEcoFollowUp.Models
{
    public class EFU_NewEcn : ObservableObject
    {
        private string _Ecn_Number = string.Empty;
        public string Ecn_Number
        {
            get { return _Ecn_Number; }
            set
            {
                if (this._Ecn_Number != value)
                {
                    this._Ecn_Number = value;
                    OnPropertyChanged();
                }

            }
        }

        private EFU_EcnEcoFollowUp _CurrentEcnEcoFollowUp = new EFU_EcnEcoFollowUp();
        public EFU_EcnEcoFollowUp CurrentEcnEcoFollowUp
        {
            get { return _CurrentEcnEcoFollowUp; }
            set
            {
                if (this._CurrentEcnEcoFollowUp != value)
                {
                    this._CurrentEcnEcoFollowUp = value;
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

        public override string ToString()
        {
            try
            {
                return string.Format("{0} - {1}", Ecn_Number, CurrentEcnEcoFollowUp.Ecn_Name);
            }
            catch (Exception )
            {
                return "";
            }
        }
    }
}
