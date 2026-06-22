using MCG.CREO_Tools.ShearedTube.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.ShearedTube.View
{
    interface IShearedTubeViewModel
    {
        ShearedTubeDataContext CurrentShearedTubeDataContext { get; set; }

        ICommand CommandBtHelpMouseLeftButtonUpEvent { get; }
        ICommand CommandCreateTube { get; }
    }
}
