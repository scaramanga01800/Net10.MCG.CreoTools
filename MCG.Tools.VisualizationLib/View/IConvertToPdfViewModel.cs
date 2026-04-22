using MCG.Tools.VisualizationLib.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCG.Tools.VisualizationLib.View
{
    public interface IConvertToPdfViewModel
    {
        ConvertToPdfDataContext CurrentDataContext { get; set; }

        ICommand CommandDrop { get; }
        ICommand CommandStartConvert { get; }
        ICommand CommandRemoveAll { get; }
        ICommand CommandChangeExportFolder { get; }
        ICommand CommandOpenFolder { get; }
        ICommand CommandMergeTiff { get; }
        ICommand CommandMergePdf { get; }
        ICommand CommandCheckUncheckAll { get; }
        ICommand CommandOpenHelp { get; }

    }
}
