using MCG.CREO_Tools.QuickSearch.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace MCG.CREO_Tools.QuickSearch.View
{
    public interface IQuickSearchUpdatePartViewModel
    {
        QuickSearchPart PartItem { get; set; }

        MessageBoxResult Return { get; set; }

        bool IsPartPictureShow { get; set; }

        ICommand CommandCreateUpdatePart { get; }
        ICommand CommandDragAndDropImage { get; }
        ICommand CommandChangeImage{ get; }
    }
}
