using MCG.CommonLib.WpfComponent.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MCG.CREO_Tools.MassUpdateAttribute.View
{
    interface ICreateNewCadDocumentDataContext
    {
        bool CreoIsEnable { get; set; }
        bool PrtSelected { get; set; }
        bool PrtSmSelected { get; set; }
        bool AsmSelected { get; set; }
        bool DrwSelected { get; set; }

        string PartNumber { get; set; }

        ObservableCollection<string> ListWebterm { get; set; }
        ObservableCollection<string> ListWebtermLocal { get; set; }
        string SelectedWebterm { get; set; }

        int SelectedIndexLanguage { get; set; }
        Image CurrentLanguage { get; set; }

        string Description2_1 { get; set; }

        List<McgAttributeColumnHeaderInfo> ListOtherAttributes { get; set; }
    }
}
