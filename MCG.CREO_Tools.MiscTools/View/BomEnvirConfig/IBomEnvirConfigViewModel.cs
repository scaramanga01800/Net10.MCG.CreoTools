using MCG.CREO_Tools.MiscTools.ViewModel.BomEnvirConfig;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.BomEnvirConfig
{
    public interface IBomEnvirConfigViewModel
    {
        BomEnvirConfigDataContext CurrentDataContext { get; set; }

        ICommand CommandReadAsm { get; }
        ICommand CommandUpdateCadDoc { get; }
        ICommand CommandUpdateActiveCadModel { get; }
        ICommand CommandOpenHelp { get; }
    }
}
