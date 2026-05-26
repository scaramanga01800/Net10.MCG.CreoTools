using MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.MechanismTool
{
    public interface IMechanismAnalysisViewModel
    {
        MechanismAnalysisDataContext CurrentDataContext { get; set; }
        ICommand CommandClosing { get; }
        ICommand CommandDrop { get; }
        ICommand CommandCreateExcel { get; }
        ICommand CommandRemoveAll { get; }
        ICommand CommandOpenHelp { get; }
    }
}
