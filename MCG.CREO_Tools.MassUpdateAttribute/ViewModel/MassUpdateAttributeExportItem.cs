using System.ComponentModel.DataAnnotations.Schema;

namespace MCG.CREO_Tools.MassUpdateAttribute.ViewModel
{
    public class MassUpdateAttributeExportItem
    {
        public string NUMBER { get; set; }

        [Column("DESCRIPTION_2")]
        public string DESCRIPTION2 { get; set; }

        [Column("DESCRIPTION2_1")]
        public string DESCRIPTION21 { get; set; }

        [Column("DESCRIPTION2_2")]
        public string DESCRIPTION22 { get; set; }

        [Column("MODIFIED_BY")]
        public string MODIFIEDBY { get; set; }

        [Column("GROUP_CREATOR")]
        public string GROUPCREATOR { get; set; }
        public string QUALINSPGRP { get; set; }
        public string MATERIAL { get; set; }
        public string ADDITIONALPUBFORMAT { get; set; }
        public string TYPE { get; set; }

        [Column("SUB_TYPE")]
        public string SUBTYPE { get; set; }
        public bool? RFID { get; set; }

        public override string ToString()
        {
            if (NUMBER != null)
                return NUMBER;
            else
                return base.ToString();
        }
    }
}
