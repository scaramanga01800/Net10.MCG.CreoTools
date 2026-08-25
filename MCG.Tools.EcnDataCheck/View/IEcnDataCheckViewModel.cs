using MCG.Tools.EcnDataCheck.ViewModel;
using System.Windows.Input;

namespace MCG.Tools.EcnDataCheck.View
{
    /// <summary>
    /// Interface to define requirement for the View for the EcnDataCheck Vie Model
    /// <para>Define mainly all Commands <see cref="ICommand"/></para>
    /// </summary>
    interface IEcnDataCheckViewModel
    {
        EcnDataCheckDataContext CurrentEcnDataCheckDataContext { get; set; }

        // ICommand for help button
        ICommand CommandBtHelpMouseLeftButtonUpEvent { get; }

        // ICommand for main buttons
        ICommand CommandStartEcnDataCheck { get; }
        ICommand CommandStartSapBOMComparison { get; }
        ICommand CommandStartExportXLS { get; }

        // ICommand for the Context Menus
        ICommand CommandRestartCheck { get; }
        ICommand CommandOpenEcn { get; }
        ICommand CommandOpenEca { get; }
        ICommand CommandOpenPart { get; }
        ICommand CommandOpenBomPdmComp { get; }
        ICommand CommandOpenBomSapComp { get; }

        // ICommand for Move Tab
        ICommand CommandMoveSendMail { get; }
        ICommand CommandMoveUdpateContextList { get; }
        ICommand CommandSetContextToAll { get; }

        // ICommand for Rename Tab
        ICommand CommandRenameSendMail { get; }

        // Icommand Menu Copy Number
        ICommand CommandCopyPartNumber { get; }
        ICommand CommandStartSapCraneSearch { get; }
  
        ICommand CommandOpenLink { get; }
    }
}
