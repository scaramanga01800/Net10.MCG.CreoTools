using MCG.Tools.VisualizationLib.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCG.Tools.VisualizationLib.View
{
    public interface IConvertToPdfMergeWindowViewModel
    {
        ObservableCollection<ConvertToPdfItem> ListFiles { get; set; }
        string FileName { get; set; }

        ICommand CommandStartMerge { get; }
        ICommand CommandMoveUpParameter { get; }
        ICommand CommandMoveDownParameter { get; }
    }
}
