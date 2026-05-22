using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColor;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MCG.CREO_Tools.MiscTools.View.CadAutoColor
{
    public interface ICadAutoColorDataContext
    {
        ObservableCollection<CadAutoColorCreoColor> ListCreoColor { get; set; }
        ObservableCollection<CadAutoColorItem> ListItem { get; set; }
        ObservableCollection<CadAutoColorItem> ListItemName { get; set; }
        ObservableCollection<CadAutoColorItem> ListItemPart { get; set; }

        string SelectedCadDoc { get; set; }

        CadAutoColorCreoColor SelectedCreoColor { get; set; }

        int NbModels { get; set; }
        int NbModelsInProgress { get; set; }

        bool IsCreoEnable { get; set; }
        bool IsPleaseWaitShown { get; set; }

        bool IsAllPartSelected { get; set; }

        bool IsAllPartSelectedName { get; set; }

        bool IsAllPartSelectedPart{ get; set; }

        CadAutoColorPalette ColorPalette01 { get; set; }
        CadAutoColorPalette ColorPalette02 { get; set; }
        CadAutoColorPalette ColorPalette03 { get; set; }

        TabItem SelectedTab { get; set; }
    }
}
