using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.SimplifiedRep
{
    internal interface ISimplifiedRepComponentItem
    {
        int TreeIndex { get; set; }
        int ComponentId { get; set; }
        string Name { get; set; }
        string Rep { get; set; }
        string Description { get; set; }
        string Type { get; set; }
        string SubType { get; set; }
        bool IsIncluded { get; set; }
        bool IsExplicit { get; set; }
        string CurrentAction { get; }
        string SelectedComponentSimpRep { get; set; }
        string EffectiveSubstitution { get; }
        bool IsModified { get; }
        ObservableCollection<string> ListComponentSimpRep { get; set; }
    }
}
