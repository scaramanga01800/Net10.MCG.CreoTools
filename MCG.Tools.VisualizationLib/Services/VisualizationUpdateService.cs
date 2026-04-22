using CommunityToolkit.Mvvm.Messaging;
using MCG.Tools.VisualizationLib.Interfaces;
using MCG.Tools.VisualizationLib.Messages;

namespace MCG.Tools.VisualizationLib.Services
{
    public class VisualizationUpdateService
    {
        private readonly IWtDownloadViewableTools _wtTools;

        // L'outil est injecté proprement ici
        public VisualizationUpdateService(IWtDownloadViewableTools wtTools)
        {
            _wtTools = wtTools;

            // On s'abonne à tous les messages de ce type
            WeakReferenceMessenger.Default.Register<PartRevisionChangedMessage>(this, (recipient, message) =>
            {
                _wtTools.UpdateSelectedRevisionInformation(message.Item);
            });
        }
    }
}

