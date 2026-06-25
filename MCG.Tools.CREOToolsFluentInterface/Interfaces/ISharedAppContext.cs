using MCG.CommonLib.WpfComponent;
using MCG.Tools.CREOToolsFluentInterface.ViewModel;

namespace MCG.Tools.CREOToolsFluentInterface.Interfaces
{

    public interface ISharedAppContext
    {
        CREOToolsAppAvailability AppAvailable { get; set; }
        CREOToolsAppAvailability AppVisible { get; set; }
        CREOToolsLanguageSelection CurrentLanguage { get; set; }
    }

}
