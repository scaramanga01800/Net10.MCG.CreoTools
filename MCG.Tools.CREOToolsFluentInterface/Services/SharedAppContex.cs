using MCG.CommonLib.WpfComponent;
using MCG.Tools.CREOToolsFluentInterface.ViewModel;
using MCG.Tools.CREOToolsFluentInterface.Interfaces;

namespace MCG.Tools.CREOToolsFluentInterface.Services
{

    public class SharedAppContext : ISharedAppContext
    {
        public CREOToolsAppAvailability AppAvailable { get; set; }
        public CREOToolsAppAvailability AppVisible { get; set; }
        public CREOToolsLanguageSelection CurrentLanguage { get; set; }
    }

}
