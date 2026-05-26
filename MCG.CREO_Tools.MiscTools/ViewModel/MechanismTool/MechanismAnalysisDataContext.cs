using MCG.CREO_Tools.MiscTools.View.MechanismTool;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool
{
    public class MechanismAnalysisDataContext: IMechanismAnalysisDataContext
    {
        public ObservableCollection<AnalysisFileItem> ListFile { get; set; } = new ObservableCollection<AnalysisFileItem>();
    }
}
