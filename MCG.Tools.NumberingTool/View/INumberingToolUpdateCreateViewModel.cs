using MCG.Tools.NumberingTool.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MCG.Tools.NumberingTool.View
{
    interface INumberingToolUpdateCreateViewModel
    {
        string LabelBtCreateUpdate { get; set; }
        NumberingToolItem CurrentItem { get; set; }
        NumberingToolTemplate SelectedNumberingTemplate { get; set; }

        ObservableCollection<string> ListProduct { get; set; }
        ObservableCollection<string> ListFormat { get; set; }
        
        bool IsUpdateShown { get; set; }
        bool IsDetailShown { get; set; }

        ICommand CommandCreateNumber { get; }
        ICommand CommandCancel { get; }
        ICommand CommandUpdateNumber { get; }

    }
}
