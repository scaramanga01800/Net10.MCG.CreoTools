using System.ComponentModel;

namespace MCG.Tools.EcnEcoFollowUp.Models
{
    public class EFU_EcnEcoCopyPaste : IEquatable<EFU_EcnEcoCopyPaste>
    {
        private string _EcnEcoNumber = string.Empty;
        [DisplayName("ECN/ECO Number")]
        public string EcnEcoNumber
        {
            get { return _EcnEcoNumber; }
            set {
               if (value!=null) value = value.Trim().ToUpper();
               _EcnEcoNumber = value;
            }
        }

        private string _Priority = string.Empty;
        public string Priority
        {
            get { return _Priority; }
            set
            {
                if (value != null) value = value.Trim().ToUpper();
                _Priority = value;
            }
        }

        public string Comment { get; set; } = string.Empty;

        public string Information { get; set; } = string.Empty;

        [DisplayName("SAP Order")]
        public string SapOrder { get; set; } = string.Empty;

        public override bool Equals(object obj)
        {
            return Equals(obj as EFU_EcnEcoCopyPaste);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();  
        }

        public bool Equals(EFU_EcnEcoCopyPaste other)
        {
            return other != null &&
                   EcnEcoNumber == other.EcnEcoNumber;
        }
    }
}
