using MCG.CREO_Tools.MiscTools.ViewModel.NumberCumulation;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.NumberCumulation
{
    internal interface INumberCumulationViewModel
    {
         NumberCumulationDataContext CurrentDataContext { get; set; }

        ICommand CommandPaste { get; }
        ICommand CommandMenuItemPaste { get; }
        ICommand CommandUpdateNumberCumul { get; }
        ICommand CommandRemoveAll { get; }
        ICommand CommandCopy { get; }
        ICommand CommandMenuRemoveItem { get; }
        ICommand CommandOpenHelp { get; }
    }
}
