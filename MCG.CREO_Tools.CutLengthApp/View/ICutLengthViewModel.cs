using MCG.CREO_Tools.CutLengthApp.ViewModel;
using System.Windows.Input;

namespace MCG.CREO_Tools.CutLengthApp.View
{
    public interface ICutLengthViewModel
    {
        CutLenghtDataContext CurrentDataContext { get; set; }

        ICommand CommandInsertCutLength { get; }
        ICommand CommandUpdateCutLength { get; }
        ICommand CommandOpenHelp { get; }
        ICommand CommandOpenTemplate { get; }
        ICommand CommandUpdateActiveCadModel { get; }
        ICommand CommandEditPartNumber { get; }
        ICommand CommandAddNewPartNumber { get; }
    }

}
