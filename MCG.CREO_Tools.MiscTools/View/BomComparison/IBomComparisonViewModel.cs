using MCG.CREO_Tools.MiscTools.ViewModel.BomComparison;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.BomComparison
{
    internal interface IBomComparisonViewModel
    {
        #region [REGION] Properties from Interface
        BomComparisonDataContext CurrentDataContext { get; set; }
        #endregion

        #region [REGION] Commands
        ICommand CommandStartBomSearch { get; }
        ICommand CommandStartExportXLS { get; }
        ICommand CommandHelp { get; }
        #endregion
    }
}
