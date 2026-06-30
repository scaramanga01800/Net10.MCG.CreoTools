using MCG.CommonLib.Models.Main;
using MCG.CommonLib.WpfComponent;
using MCG.CommonLib.WpfComponent.Models;

namespace MCG.Tools.CREOToolsFluentInterface.Configuration
{
    public class CREOToolsConfiguration
    {
        public CREOToolsLanguageSelection LangCn { get; set; } = new CREOToolsLanguageSelection() { Language = new MCGLanguage() { CultureInfo = "zh-CN", Language = "Chinese" }, IsSelected = false };
        public CREOToolsLanguageSelection LangEn { get; set; } = new CREOToolsLanguageSelection() { Language = new MCGLanguage() { CultureInfo = "en-US", Language = "English" }, IsSelected = false };
        public CREOToolsLanguageSelection LangFr { get; set; } = new CREOToolsLanguageSelection() { Language = new MCGLanguage() { CultureInfo = "fr-FR", Language = "French" }, IsSelected = true };
        public CREOToolsLanguageSelection LangDe { get; set; } = new CREOToolsLanguageSelection() { Language = new MCGLanguage() { CultureInfo = "de-DE", Language = "German" }, IsSelected = false };

        // Define if Applications are available in CREO Tool
        public CREOToolsAppAvailabilityConfig AppAvailable { get; set; }

        // Define if Applications are shown by default in CREO Tool
        public CREOToolsAppAvailabilityConfig AppVisible { get; set; }

        public bool IsScrollingTextVisible { get; set; }
    }
}
