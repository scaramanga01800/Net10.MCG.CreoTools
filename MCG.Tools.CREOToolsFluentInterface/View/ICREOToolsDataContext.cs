using MCG.CommonLib.WpfComponent;
using MCG.Tools.CREOToolsFluentInterface.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.Tools.CREOToolsFluentInterface.View
{
    public interface ICREOToolsDataContext
    {
        CREOToolsLanguageSelection LangCn { get; set; }
        CREOToolsLanguageSelection LangEn { get; set; }
        CREOToolsLanguageSelection LangFr { get; set; }
        CREOToolsLanguageSelection LangDe { get; set; }

        // Define if Applications are available in CREO Tool
        CREOToolsAppAvailability AppAvailable { get; set; }

        // Define if Applications are shown by default in CREO Tool
        CREOToolsAppAvailability AppVisible { get; set; }

        bool IsScrollingTextVisible { get; set; }


        bool IsCreoConnected { get; set; }

        bool IsPleaseWaitShown { get; set; }
        bool IsDark { get; set; }
        bool IsLight { get; set; }
        string SelectedColorScheme { get; set; }

        ObservableCollection<string> ListFont { get; set; }
        string SelectedFont { get; set; }

    }
}
