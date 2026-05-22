using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColor;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.CadAutoColor
{
    public interface ICadAutoColorItem
    {
        string Material { get; set; }

        string AsssignedMaterial { get; set; }

        string Ptc_Common_Name { get; set; }

        string Number { get; set; }

        bool IsSelected { get; set; }

        CadAutoColorCreoColor SelectedCreoColor { get; set; }

        ObservableCollection<string> ListCadDoc { get; set; }

        event EventHandler IsSelectedEvent;
    }
}