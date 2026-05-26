using MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.MechanismTool
{
    public interface IMechanismAnalysisDataContext
    {
        ObservableCollection<AnalysisFileItem> ListFile { get; set; }
    }
}
