using CommunityToolkit.Mvvm.ComponentModel;
using MCG.Tools.EcnEcoFollowUp.Interfaces.Models;

namespace MCG.Tools.EcnEcoFollowUp.Models
{
    public class EFU_SearchTemplate : ObservableObject, IEFU_SearchTemplate
    {
        #region [REGION] Properties from Interface
        private string _Name = "None";
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    //OnPropertyChanged();
                }
            }
        }
        #endregion

        #region [REGION] Properties not from interface
        public string CompleteXmlFileName { get; set; } = "";
        public string CreatedOnBefore { get; set; } = "";
        public string CreatedOnAfter { get; set; } = "";
        public string EcnNumber { get; set; } = "";
        public string EcnState { get; set; } = "";
        public string Creator { get; set; } = "";
        public bool IsStatusNotCreated { get; set; } = true;
        public bool IsStatus99 { get; set; } = true;
        public bool IsStatus03 { get; set; } = true;
        public bool IsStatus02 { get; set; } = true;
        public bool IsStatus01 { get; set; } = true;
        public string KeyWords { get; set; } = "";
        public string Product { get; set; } = "";
        public string ResolvedOnAfter { get; set; } = "";
        public string ResolvedOnBefore { get; set; } = "";
        #endregion

        public override string ToString()
        {
            return Name;
        }
    }
}
