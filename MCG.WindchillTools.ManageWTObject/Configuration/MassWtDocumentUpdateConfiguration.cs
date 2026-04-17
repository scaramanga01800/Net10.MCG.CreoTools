using MCG.CommonLib;
using MCG.CommonLib.Models.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCG.WindchillTools.ManageWTObject.Configuration
{
    public class MassWtDocumentUpdateConfiguration
    {
        public List<MCGLanguage> LocalLanguageList { get; set; }
        public List<string> ListGroup { get; set; }
        public List<string> ListBrand { get; set; }
    }
}
