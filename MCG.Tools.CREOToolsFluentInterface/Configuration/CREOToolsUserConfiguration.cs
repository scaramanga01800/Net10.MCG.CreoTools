using MCG.CommonLib.WpfComponent;
using MCG.CommonLib.WpfComponent.Models;

namespace MCG.Tools.CREOToolsFluentInterface.Configuration
{
    public class CREOToolsUserConfiguration
    {
        public string ConfigVersion { get; set; }

        public CREOToolsLanguageSelection CurrentLang { get; set; }

        // Define if Applications are shown by default in CREO Tool
        public CREOToolsAppAvailabilityConfig AppVisible { get; set; }

        public string ColorScheme { get; set; }
        public bool IsDark { get; set; }
        public bool IsLight { get; set; }

        public string DefaultFont { get; set; }
    }
}
