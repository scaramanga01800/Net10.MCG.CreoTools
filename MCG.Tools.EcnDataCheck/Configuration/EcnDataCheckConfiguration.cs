using MCG.CommonLib.Models.Email;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Models.SAP;
using MCG.Tools.EcnDataCheck.Models;
using MCG.WindchillRequestTool.Model.Windchill;

namespace MCG.Tools.EcnDataCheck.Configuration
{
    public class EcnDataCheckConfiguration
    {
        // Properties for Main interface
        public List<MCGLanguage> LocalLanguageList { get; set; }

        public string ErpSystem { get; set; }

        public List<string> ErpList { get; set; }

        public string SelectedSapPlant { get; set; }
        public List<SapPlant> ListSapPlant { get; set; }
        public int NumericalLineMaxNumberDigit { get; set; }
        public int NumericalLineNumberDigit { get; set; }
        public List<int> NumericalLineNumberDigitList { get; set; }

        // Property all different checks
        public List<DataCheckRule> AllDataCheckRules { get; set; }
        public List<string> ListExcludedState { get; set; }
        public List<WindchillContext> ListExcludedContext { get; set; }

        // Properties for Move Tab
        public string Location { get; set; }
        public List<string> ListLocation { get; set; }
        public string ContextFilter { get; set; }
        public bool IsCheckBoxProductSelected { get; set; }
        public bool IsCheckBoxLibraySelected { get; set; }

        // Email information
        public McgEMail CadAminEMail { get; set; }


    }
}
