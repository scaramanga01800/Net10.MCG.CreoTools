using MCG.Tools.VisualizationLib.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCG.Tools.VisualizationLib.View
{
    public interface IDownloadVisualizationFileViewModel
    {
        DownloadVisualizationFileDataContext CurrentDataContext { get; set; }

        ICommand CommandSearchEcn { get; }
        ICommand CommandSearchPart { get; }
        ICommand CommandSearchFromPressEnterKey { get; }
        ICommand CommandDeleteSelectedParts { get; }
        ICommand CommandSearchVisuFile { get; }
        ICommand CommandExportZip { get; }
        ICommand CommandMenuItemPastePart { get; }
        ICommand CommandMenuItemOpenPart { get; }
        ICommand CommandMenuItemOpenEcn { get; }
        ICommand CommandMenuItemDeletePart { get; }
        ICommand CommandCheckUncheckAll { get; }
        ICommand CommandUncheckAll { get; }
        ICommand CommandUpdateCheckAllPart { get; }
        ICommand CommandPaste { get; }
        ICommand CommandApplyFilters { get; }
        ICommand CommandDownloadVisuFiles { get; }
        ICommand CommandUpdateColumn { get; }
        ICommand CommandMenuItemSearchBom { get; }
        ICommand CommandMenuItemSearchSapBom { get; }
        ICommand CommandOpenHelp { get; }
        ICommand CommandChangeExportFolder { get; }
        ICommand CommandOpenFolder { get; }
        ICommand CommandDownloadEcnFlash { get; }
        ICommand CommandDownloadPartFlash { get; }
        ICommand CommandExportExcel { get; }
    }
}
