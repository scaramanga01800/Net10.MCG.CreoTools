using MCG.CommonLib.Configuration;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.PurchaseOrderFollowUp.Configuration;
using MCG.Tools.PurchaseOrderFollowUp.Interfaces;
using MCG.Tools.PurchaseOrderFollowUp.Models;
using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System.IO;

namespace MCG.Tools.PurchaseOrderFollowUp.Services
{
    /// <summary>
    /// Charge les données de référence à partir des mêmes sources que PurchaseOrderFollowUpViewModel :
    /// fichier XML de configuration sous MainAppFolder\Resources et IPurchaseOrderService.
    /// </summary>
    public sealed class PurchaseOrderReferenceDataProvider : IPurchaseOrderReferenceDataProvider
    {
        private readonly IXmlSerializeTools _xmlSerializeTools;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrderReferenceDataProvider(IXmlSerializeTools xmlSerializeTools, IPurchaseOrderService purchaseOrderService)
        {
            _xmlSerializeTools = xmlSerializeTools ?? throw new ArgumentNullException(nameof(xmlSerializeTools));
            _purchaseOrderService = purchaseOrderService ?? throw new ArgumentNullException(nameof(purchaseOrderService));
        }

        public PurchaseOrderReferenceData Load(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string mainAppFolder = Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
            if (string.IsNullOrEmpty(mainAppFolder))
                mainAppFolder = CommonLibConstants.MainAppFolder;

            string configurationFile = Path.Combine(mainAppFolder, CommonLibConstants.ResourcesFolder, PurchaseOrderFollowUpConstants.ConfigurationFile);
            TraceLog.AddTraceLog($"PurchaseOrderReferenceDataProvider: chargement de {configurationFile}");

            PurchaseOrderFollowUpConfiguration configuration;
            try
            {
                configuration = _xmlSerializeTools.GetDeserializedXml<PurchaseOrderFollowUpConfiguration>(configurationFile);
            }
            catch (Exception ex)
            {
                throw new Exceptions.PurchaseOrderSapSyncException($"Échec du chargement de la configuration Purchase Order '{configurationFile}'.", ex);
            }

            if (configuration == null)
                TraceLog.AddTraceLog($"PurchaseOrderReferenceDataProvider: configuration absente ou vide ({configurationFile}), lieux de livraison et centres de coût vides.");

            cancellationToken.ThrowIfCancellationRequested();

            List<PurchaseOrderMaterial> materials;
            try
            {
                materials = _purchaseOrderService.GetAllPoMatDienNlags()
                    .Select(PurchaseOrderMaterial.GetMaterialFromDbItem)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exceptions.PurchaseOrderSapSyncException("Échec du chargement des matériaux DIEN/NLAG (GetAllPoMatDienNlags).", ex);
            }

            var referenceData = new PurchaseOrderReferenceData(
                configuration?.ListDeliveryLocation,
                configuration?.ListCostCenter,
                materials);

            TraceLog.AddTraceLog($"PurchaseOrderReferenceDataProvider: {referenceData.DeliveryLocations.Count} lieux, {referenceData.CostCenters.Count} centres de coût, {referenceData.DienNlagMaterials.Count} matériaux DIEN/NLAG.");
            return referenceData;
        }
    }
}
