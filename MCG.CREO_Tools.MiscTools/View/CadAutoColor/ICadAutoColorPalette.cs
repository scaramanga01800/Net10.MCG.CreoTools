using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColor;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.CadAutoColor
{
    internal interface ICadAutoColorPalette
    {
         string Name { get; set; }
         bool IsSelected { get; set; }
         ObservableCollection<CadAutoColorCreoColor> ListColor { get; set; }
    }
}
