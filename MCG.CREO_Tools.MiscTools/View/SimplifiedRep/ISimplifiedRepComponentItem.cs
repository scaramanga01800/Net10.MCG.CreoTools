using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.SimplifiedRep
{
    internal interface ISimplifiedRepComponentItem
    {
        int TreeIndex { get; set; }
        int ComponentId { get; set; }
        string Name { get; set; }
        bool IsIncluded { get; set; }
        bool IsExplicit { get; set; }
        string CurrentAction { get; set; }
        string SelectedComponentSimpRep { get; set; }
        ObservableCollection<string> ListComponentSimpRep { get; set; }
    }
}
