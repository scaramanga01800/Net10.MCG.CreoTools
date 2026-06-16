using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;
using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColor;
using System.Collections.ObjectModel;


namespace MCG.CREO_Tools.MassUpdateAttribute.View
{
    public interface IMassUpdateAttributeDataContext
    {

        bool ShowActionButton { get; set; }

        bool IsSearchCadModelInProgress { get; set; }
        bool IsOnlyDisplayedModels { get; set; }
        bool IsOnlyActiveModel { get; set; }
        bool IsCheckedOutShown { get; set; }
        bool IsLocallyModifiedShown { get; set; }
        bool IsReadOnlyShown { get; set; }
        bool IsLoadedFromCreo { get; set; }

        ObservableCollection<MassUpdateAttributeItem> ShownCadModels { get; set; }
        MassUpdateAttributeItem SelectedItem { get; set; }
        bool IsAllSelected { get; set; }
        bool IsAllSelectedRename { get; set; }
        int SelectedIndex { get; set; }

        string TextStatusBar { get; set; }

        ObservableCollection<string> ListLanguages { get; set; }
        string CurrentLanguage { get; set; }

        // Propertie for MassUpdateAttributeWorkInProgress 
        long NbModelsInSession { get; set; }
        long NbModelsInSessionInProgress { get; set; }
        string MessageModelsInSessionInProgress { get; set; }

        List<McgAttributeColumnHeaderInfo> ListColumns { get; set; }

        ObservableCollection<CadAutoColorCreoColor> ListCreoColor { get; set; }
        CadAutoColorCreoColor SelectedCreoColor { get; set; }

        CadAutoColorPalette ColorPalette01 { get; set; }
        CadAutoColorPalette ColorPalette02 { get; set; }
        CadAutoColorPalette ColorPalette03 { get; set; }

        ObservableCollection<MassUpdateAttributeRenameItem> ListToBeRenamedObject { get; set; }

        MassUpdateAttributeRenameItem SelectedRenameItem { get; set; }

        ObservableCollection<string> WebtermList { get; set; }

        string NewTerm { get; set; }


        ObservableCollection<string> ListGroup { get; set; }
        ObservableCollection<string> ListSubGroup { get; set; }
        ObservableCollection<string> ListBrand { get; set; }
        ObservableCollection<string> ListOption { get; set; }

        string SelectedBrand { get; set; }
        string SelectedGroup { get; set; }
        string SelectedSubGroup { get; set; }
        string SelectedOption { get; set; }
    }
}
