using MCG.CREO_Tools.MiscTools.ViewModel.WebtermRequest;
using System.Windows.Input;

namespace MCG.CREO_Tools.MiscTools.View.WebtermRequest
{
    public interface IWebtermRequestViewModel
    {
        WebtermRequestDataContext CurrentDataContext { get; set; }

        ICommand CommandDrop { get; }
        ICommand CommandStartClassOrder { get; }
        ICommand CommandSendRequest { get; }
        ICommand CommandOpenHelp { get; }
        ICommand CommandDeletePicture { get; }

        event EventHandler CallCloseEvent;
    }
}
