using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MCG.Tools.EcnEcoFollowUp.Models
{
    public class EFU_PdmContext : ObservableObject
    { 

        private int _ID;
        public int ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
                OnPropertyChanged();
            }
        }

        private string _Pdm_Context = string.Empty;
        public string Pdm_Context
        {
            get
            {
                return _Pdm_Context;
            }
            set
            {
                _Pdm_Context = value;
                Product = ExtractProduct(Pdm_Context);
                OnPropertyChanged();
            }
        }

        private string _Team_Role = string.Empty;
        public string Team_Role
        {
            get
            {
                return _Team_Role;
            }
            set
            {
                _Team_Role = value;
                OnPropertyChanged();
            }
        }

        private string _Participant_Type = string.Empty;
        public string Participant_Type
        {
            get
            {
                return _Participant_Type;
            }
            set
            {
                _Participant_Type = value;
                OnPropertyChanged();
            }
        }

        private string _Participant_Id = string.Empty;
        public string Participant_Id
        {
            get
            {
                return _Participant_Id;
            }
            set
            {
                _Participant_Id = value;
                OnPropertyChanged();
            }
        }

        private string _Participant_Name = string.Empty;
        public string Participant_Name
        {
            get
            {
                return _Participant_Name;
            }
            set
            {
                _Participant_Name = value;
                OnPropertyChanged();
            }
        }

        private string _Context_Link = string.Empty;
        public string Context_Link
        {
            get
            {
                return _Context_Link;
            }
            set
            {
                _Context_Link = value;
                OnPropertyChanged();
            }
        }

        private string _Product = string.Empty;
        public string Product
        {
            get
            {
                return _Product;
            }
            set
            {
                _Product = value;
                OnPropertyChanged();
            }
        }

        private string _Type = string.Empty;
        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
                OnPropertyChanged();
            }
        }

        private string _flag = "NEW";
        public string flag
        {
            get
            {
                return _flag;
            }
            set
            {
                _flag = value;
                OnPropertyChanged();
            }
        }

        public static string ExtractProduct(string Pdm_Context)
        {
            try
            {
                List<Regex> ListRegex = new List<Regex>()
                {
                    new Regex("^TWR_", RegexOptions.IgnoreCase), 
                    new Regex("^GMK_", RegexOptions.IgnoreCase), 
                    new Regex("^GOVT_", RegexOptions.IgnoreCase), 
                    new Regex("^GROVE_", RegexOptions.IgnoreCase), 
                    new Regex("^LIBRARY", RegexOptions.IgnoreCase), 
                    new Regex("^MLC_", RegexOptions.IgnoreCase), 
                    new Regex("^NAT_", RegexOptions.IgnoreCase), 
                    new Regex("^SG_", RegexOptions.IgnoreCase), 
                    new Regex("^SL_", RegexOptions.IgnoreCase),
                    new Regex("^WI_", RegexOptions.IgnoreCase)
                };
                int index;
                index = Pdm_Context.IndexOf("TWR_");

                Regex MatchRegex = ListRegex.FirstOrDefault((item) => item.IsMatch(Pdm_Context));
                if (MatchRegex != null)
                    return MatchRegex.Split(Pdm_Context).LastOrDefault();

                return "STANDARD";
            }
            catch (Exception)
            {
                return "UNKNOWN";
            }
        }
    }
}
