using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;

namespace MCG.CREO_Tools.MassUpdateAttribute.Configuration
{
    public class MassUpdateAttributeConfiguration
    {
        public List<McgAttributeColumnHeaderInfo> ListColumns { get; set; }

        public string CurrentLanguage { get; set; }

        public List<string> ListLanguages { get; set; }

        public List<CadDocLayerItemConfig> ListStandardLayers { get; set; }

        public List<MassUpdateAttributeCadTemplate> ListTemplate { get; set; }
    }
}