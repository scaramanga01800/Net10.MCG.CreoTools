using MCG.Tools.NumberingTool.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MCG.Tools.NumberingTool.View
{
    interface INumberingToolViewModel
    {
        ObservableCollection<NumberingToolTemplate> ListNumberingTemplate { get; set; }
        NumberingToolTemplate SelectedNumberingTemplate { get; set; }

        string SearchNumber { get; set; }
        string SearchDescription { get; set; }
        string SearchProduct { get; set; }
        string SearchCreatedBy { get; set; }

        ObservableCollection<string> SearchProductList { get; set; }
        ObservableCollection<string> SearchCreatedByList { get; set; }
        ObservableCollection<string> SearchFormatList { get; set; }

        DateTime? SearchCreatedAfter { get; set; }
        DateTime? SearchCreatedBefore { get; set; }

        ObservableCollection<NumberingToolItem> ListSearchNumber { get; set; }
        NumberingToolItem SelectedSearchNumber { get; set; }

        ObservableCollection<NumberingToolItem> ListNewNumber { get; set; }
        NumberingToolItem SelectedNewNumber { get; set; }

        ObservableCollection<int> ListSizeBlock { get; set; }
        int SelectedSizeBlock { get; set; }

        bool IsSeveralNumberCreated { get; set; }
        bool IsUpdateShown { get; set; }

        bool IsCreateZip { get; set; }

        ICommand CommandCreateNumber { get; }
        ICommand CommandCreateSeveralNumbers { get; }
        ICommand CommandUpdateNumber { get; }
        ICommand CommandStartSearch { get; }
        ICommand CommandStartCreateSeveralNumbers { get; }
        ICommand CommandStartUpdateSeveralNumbers { get; }
        ICommand CommandCancel { get; }
        ICommand CommandUseNewNumber { get; }
        ICommand CommandUseSearchNumber { get; }
        ICommand CommandDownloadDrawing { get; }
        ICommand CommandOpenPartDetail { get; }

    }
}
