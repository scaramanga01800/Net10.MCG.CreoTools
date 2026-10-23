using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.DataBaseAccess.Models.SapHupDbResult;
using MCG.CommonLib.DataBaseAccess.OracleQuery;
using MCG.CommonLib.DataBaseAccess.Shadow;
using MCG.CommonLib.Models.SAP;
using MCG.CommonLib.SapTools.ViewModel;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.Interfaces;
using MCG.Tools.PurchaseOrderFollowUp.Models;
using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System.Text.RegularExpressions;

namespace MCG.Tools.PurchaseOrderFollowUp.Services
{
    /// <summary>
    /// Logique mÃ©tier de synchronisation SAP HUP extraite de PurchaseOrderFollowUpViewModel
    /// (CreateSapRequestFromDate et StartUpdateAllRequestFromSapAcynch), sans dÃ©pendance UI.
    /// </summary>
    public sealed class PurchaseOrderSapSyncService : IPurchaseOrderSapSyncService
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISapHupService _sapHupService;
        private readonly IOracleShadowRecorder? _shadowRecorder;

        private readonly IOracleQueryRouter? _queryRouter;

        public PurchaseOrderSapSyncService(IPurchaseOrderService purchaseOrderService, ISapHupService sapHupService, IOracleShadowRecorder? shadowRecorder = null, IOracleQueryRouter? queryRouter = null)
        {
            _queryRouter = queryRouter;
            _purchaseOrderService = purchaseOrderService ?? throw new ArgumentNullException(nameof(purchaseOrderService));
            _sapHupService = sapHupService ?? throw new ArgumentNullException(nameof(sapHupService));
            _shadowRecorder = shadowRecorder;
        }

        /// <summary>Observation shadow : jamais d'impact sur le traitement historique, exceptions absorbées.</summary>
        private static void NotifyShadow(Action notify)
        {
            try
            {
                notify();
            }
            catch (Exception)
            {
                // Le shadow mode est purement observationnel.
            }
        }

        #region Public API

        public PurchaseOrderSapSyncResult CreateRequestsFromSap(DateTime fromDate, DateTime toDate, PurchaseOrderReferenceData referenceData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(referenceData);
            ValidateDateRange(fromDate, toDate);

            var result = new PurchaseOrderSapSyncResult();
            CreateRequestsFromSapCore(fromDate, toDate, referenceData, result, cancellationToken);
            LogSummary(result);
            return result;
        }

        public PurchaseOrderSapSyncResult UpdateOpenRequestsFromSap(PurchaseOrderReferenceData referenceData, CancellationToken cancellationToken = default, IProgress<PurchaseOrderSapSyncProgress>? progress = null)
        {
            ArgumentNullException.ThrowIfNull(referenceData);

            var result = new PurchaseOrderSapSyncResult();
            UpdateOpenRequestsFromSapCore(referenceData, result, cancellationToken, progress);
            LogSummary(result);
            return result;
        }

        public List<PurchaseOrderRequest> SearchRequestsFromSap(DateTime fromDate, DateTime toDate, PurchaseOrderReferenceData referenceData)
        {
            ArgumentNullException.ThrowIfNull(referenceData);
            ValidateDateRange(fromDate, toDate);
            return SearchSapHubRequestFromPurchaseRequestFromDates(fromDate, toDate, referenceData);
        }

        public void LoadRequestDetails(PurchaseOrderRequest request, PurchaseOrderReferenceData referenceData)
        {
            ArgumentNullException.ThrowIfNull(referenceData);
            UpdateRequestItemAttachment(request, referenceData);
        }

        public void RefreshRequestFromSap(PurchaseOrderRequest request, PurchaseOrderReferenceData referenceData, bool includePurchaseRequest)
        {
            ArgumentNullException.ThrowIfNull(referenceData);
            if (request == null)
                return;

            if (includePurchaseRequest)
                UpdateRequestFromSapHubPurchaseRequest(request, referenceData);

            if (!string.IsNullOrEmpty(request.SapPurchaseOrder))
            {
                UpdateRequestFromSapHubPurchaseOrder(request);
                UpdateRequestFromSapHubPurchaseOrderGoodsInvoiceReceipt(request);
            }
        }

        public bool SaveRequest(PurchaseOrderRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return CreateUpdateRequestOnDataBase(request);
        }

        public PurchaseOrderSapSyncResult Synchronize(DateTime fromDate, DateTime toDate, PurchaseOrderReferenceData referenceData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(referenceData);
            ValidateDateRange(fromDate, toDate);

            var result = new PurchaseOrderSapSyncResult();
            CreateRequestsFromSapCore(fromDate, toDate, referenceData, result, cancellationToken);
            UpdateOpenRequestsFromSapCore(referenceData, result, cancellationToken, null);
            LogSummary(result);
            return result;
        }

        #endregion

        #region Orchestration

        private static void ValidateDateRange(DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
            {
                string message = $"Plage de dates invalide pour la synchronisation SAP HUP : fromDate={fromDate:dd-MM-yyyy} est postÃ©rieure Ã  toDate={toDate:dd-MM-yyyy}.";
                TraceLog.AddTraceLog(message);
                throw new ArgumentOutOfRangeException(nameof(fromDate), fromDate, message);
            }
        }

        private void CreateRequestsFromSapCore(DateTime fromDate, DateTime toDate, PurchaseOrderReferenceData referenceData, PurchaseOrderSapSyncResult result, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<PurchaseOrderRequest> sapRequests;
            try
            {
                sapRequests = SearchSapHubRequestFromPurchaseRequestFromDates(fromDate, toDate, referenceData);
            }
            catch (Exception ex) when (!IsFatal(ex))
            {
                throw new PurchaseOrderSapSyncException(
                    $"Ã‰chec de la recherche SAP HUP des PO crÃ©Ã©s entre {fromDate:dd-MM-yyyy} et {toDate:dd-MM-yyyy}.", ex);
            }

            result.SapRequestsAnalyzed += sapRequests.Count;

            foreach (var request in sapRequests)
            {
                cancellationToken.ThrowIfCancellationRequested();

                PurchaseOrderExistence existence;
                try
                {
                    existence = GetExistenceInDataBase(request);
                }
                catch (Exception ex) when (!IsFatal(ex))
                {
                    RegisterError(result, "ContrÃ´le d'existence en base", request, ex);
                    continue;
                }

                if (existence != PurchaseOrderExistence.New)
                {
                    request.IsAlreadyExist = true;
                    result.ExistingRequestsSkipped++;
                    LogRequestLine(PurchaseOrderLogStatus.AlreadyExists, request);
                    TraceLog.AddTraceLog($"Demande SAP dÃ©jÃ  existante ({existence}) : PO={request.SapPurchaseOrder} PR={request.SapPurchaseRequest}, ignorÃ©e.");
                    continue;
                }

                try
                {
                    bool isNew = CreateUpdateRequestOnDataBase(request);
                    if (isNew)
                        result.RequestsCreated++;
                    else
                        result.NewSapRequestsUpsertedAsExisting++;
                    LogRequestLine(isNew ? PurchaseOrderLogStatus.Created : PurchaseOrderLogStatus.Updated, request);
                    result.AddSavedNewSapRequest(request);
                }
                catch (Exception ex) when (!IsFatal(ex))
                {
                    RegisterError(result, "CrÃ©ation de la demande en base", request, ex);
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private void UpdateOpenRequestsFromSapCore(PurchaseOrderReferenceData referenceData, PurchaseOrderSapSyncResult result, CancellationToken cancellationToken, IProgress<PurchaseOrderSapSyncProgress>? progress)
        {
            TraceLog.AddTraceLog("Start UpdateOpenRequestsFromSap");
            cancellationToken.ThrowIfCancellationRequested();

            var listFromPrToUpdate = new List<PurchaseOrderRequest>();
            int openRequestsWithSapPoWithoutSapPr;
            List<PoRequest> skippedPoWithoutPr;
            try
            {
                foreach (var dbReq in _purchaseOrderService.GetOpenSapPrRequests())
                    listFromPrToUpdate.Add(PurchaseOrderRequest.GetRequestFromDb(dbReq));

                // Dette technique conservÃ©e volontairement : la liste est chargÃ©e comme dans le comportement historique,
                // mais ses Ã©lÃ©ments ne sont pas traitÃ©s.
                skippedPoWithoutPr = _purchaseOrderService.GetOpenRequestsWithSapPoWithoutSapPr().ToList();
                openRequestsWithSapPoWithoutSapPr = skippedPoWithoutPr.Count;
            }
            catch (Exception ex) when (!IsFatal(ex))
            {
                throw new PurchaseOrderSapSyncException("Ã‰chec du chargement des demandes ouvertes (GetOpenSapPrRequests / GetOpenRequestsWithSapPoWithoutSapPr).", ex);
            }

            result.OpenRequestsAnalyzed += listFromPrToUpdate.Count;
            result.OpenRequestsWithSapPoWithoutSapPrNotProcessed += openRequestsWithSapPoWithoutSapPr;
            foreach (var skipped in skippedPoWithoutPr)
                TraceLog.AddTraceLog($"PO|RequestId={skipped.Idrequest}|PR=-|PO={(string.IsNullOrWhiteSpace(skipped.Sappoid) ? "-" : skipped.Sappoid)}|Status={PurchaseOrderLogStatus.Skipped}");
            TraceLog.AddTraceLog($"Demandes ouvertes avec PO SAP sans PR SAP trouvÃ©es mais non traitÃ©es (dette technique) : {openRequestsWithSapPoWithoutSapPr}");

            // Mode Batch PO : POs regroupes par lots (sans effet en Legacy). PR et UpdateRequestFromPO restent Legacy.
            if (_queryRouter != null)
            {
                _queryRouter.LogModes();
                _queryRouter.Prepare(OracleShadowApi.PurchaseOrderItemsFromPo, listFromPrToUpdate.Select(r => r.SapPurchaseOrder).Where(po => !string.IsNullOrEmpty(po)));
            }

            int currentStep = 0;
            int totalStep = listFromPrToUpdate.Count + openRequestsWithSapPoWithoutSapPr;
            progress?.Report(new PurchaseOrderSapSyncProgress(currentStep, totalStep));
            foreach (var req in listFromPrToUpdate)
            {
                cancellationToken.ThrowIfCancellationRequested();
                TraceLog.AddTraceLog($"Current Step: {currentStep}/{totalStep} Request {req.ID}");

                try
                {
                    UpdateRequestItemAttachment(req, referenceData);
                    UpdateRequestFromSapHubPurchaseRequest(req, referenceData);
                    if (!string.IsNullOrEmpty(req.SapPurchaseOrder))
                    {
                        UpdateRequestFromSapHubPurchaseOrder(req);
                        UpdateRequestFromSapHubPurchaseOrderGoodsInvoiceReceipt(req);
                    }
                    CreateUpdateRequestOnDataBase(req);
                    result.OpenRequestsUpdated++;
                    LogRequestLine(PurchaseOrderLogStatus.Updated, req);
                }
                catch (Exception ex) when (!IsFatal(ex))
                {
                    RegisterError(result, "Mise Ã  jour d'une demande ouverte depuis SAP HUP", req, ex);
                }

                currentStep++;
                progress?.Report(new PurchaseOrderSapSyncProgress(currentStep, totalStep));
                cancellationToken.ThrowIfCancellationRequested();
            }
            TraceLog.AddTraceLog("End UpdateOpenRequestsFromSap");
        }

        private static void RegisterError(PurchaseOrderSapSyncResult result, string operation, PurchaseOrderRequest request, Exception ex)
        {
            var error = new PurchaseOrderSapSyncError(operation, request.ID, request.SapPurchaseRequest, request.SapPurchaseOrder, ex);
            result.AddError(error);
            LogRequestLine(PurchaseOrderLogStatus.Failed, request);
            TraceLog.AddTraceLog($"Ã‰chec [{operation}] Request={request.ID} PR={request.SapPurchaseRequest} PO={request.SapPurchaseOrder} : {ex}");
        }

        private static void LogRequestLine(string status, PurchaseOrderRequest request)
        {
            string requestId = request.ID > 0 ? request.ID.ToString() : "n/a";
            string pr = string.IsNullOrWhiteSpace(request.SapPurchaseRequest) ? "-" : request.SapPurchaseRequest;
            string po = string.IsNullOrWhiteSpace(request.SapPurchaseOrder) ? "-" : request.SapPurchaseOrder;
            string line = $"PO|RequestId={requestId}|PR={pr}|PO={po}|Status={status}";
            if (!string.IsNullOrWhiteSpace(request.Vendor?.Number))
                line += $"|Vendor={request.Vendor.Number}";
            TraceLog.AddTraceLog(line);
        }

        private static void LogSummary(PurchaseOrderSapSyncResult result) =>
            TraceLog.AddTraceLog(result.ToSummaryLog());

        private static bool IsFatal(Exception ex) =>
            ex is OperationCanceledException
               or OutOfMemoryException
               or StackOverflowException
               or AccessViolationException
               or InsufficientExecutionStackException;

        #endregion

        #region DÃ©doublonnage en base

        private enum PurchaseOrderExistence
        {
            New,
            ExistsBySapPurchaseOrder,
            ExistsBySapPurchaseRequest
        }

        /// <summary>
        /// ContrÃ´le direct en base via les mÃ©thodes utilisÃ©es par CheckIfRequestAlreadyExist.
        /// Toute exception est propagÃ©e Ã  l'appelant (jamais interprÃ©tÃ©e comme "inexistante").
        /// </summary>
        private PurchaseOrderExistence GetExistenceInDataBase(PurchaseOrderRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.SapPurchaseOrder) && _purchaseOrderService.GetPoRequestFromSapPo(request.SapPurchaseOrder) != null)
                return PurchaseOrderExistence.ExistsBySapPurchaseOrder;

            if (!string.IsNullOrWhiteSpace(request.SapPurchaseRequest) && _purchaseOrderService.GetPoRequestFromSapPr(request.SapPurchaseRequest) != null)
                return PurchaseOrderExistence.ExistsBySapPurchaseRequest;

            return PurchaseOrderExistence.New;
        }

        #endregion

        #region SAP HUP

        private List<PurchaseOrderRequest> SearchSapHubRequestFromPurchaseRequestFromDates(DateTime fromDate, DateTime toDate, PurchaseOrderReferenceData referenceData)
        {
            TraceLog.AddTraceLog("start SearchSapHubRequestFromPurchaseRequestFromDates");
            var listRequest = new List<PurchaseOrderRequest>();

            TraceLog.AddTraceLog("start request:  GetPurchaseOrderBetweenDates(DateTime startDate, DateTime endDate)");
            var tempListPo = _sapHupService.GetPurchaseOrderBetweenDates(fromDate, toDate);
            if (tempListPo == null || tempListPo.Count == 0)
            {
                TraceLog.AddTraceLog($"no purchase order found between dates: {fromDate:dd-MM-yyyy} and {toDate:dd-MM-yyyy}");
                return listRequest;
            }

            foreach (var item in tempListPo)
            {
                string sapPurchaseOrder = item.EBELN;

                PurchaseOrderRequest currentRequest = listRequest.FirstOrDefault(x => x.SapPurchaseOrder == sapPurchaseOrder);
                string vendorNumber = (!string.IsNullOrWhiteSpace(item.FLIEF)) ? item.FLIEF : item.LIFNR;

                var currentItem = new PurchaseOrderItem()
                {
                    AccountAssignementCategory = item.KNTTP,
                    CostCenter = GetPurchaseOrderCostCenterFromNumber(item.KOSTL, referenceData),
                    InternalOrder = GetPurchaseOrderInternalOrderFromNumber(item.AUFNR),
                    Number = Convert.ToInt32(item.BNFPO),
                    Material = item.MATNR,
                    MaterialNumber = GetPurchaseOrderMaterialFromNumber(item.MATNR),
                    Description = item.TXZ01,
                    DeliveryDate = item.LFDAT.HasValue ? DateOnly.FromDateTime(item.LFDAT.Value) : (DateOnly?)null,
                    DeliveryPlant = new SapPlant() { Number = item.BERID },
                    DeliveryAdress = GetPurchaseOrderLocationFromNumber(item.BERID, referenceData),
                    Price = item.PREIS.HasValue ? (double)item.PREIS.Value : 0,
                    Quantity = item.MENGE.HasValue ? (double)item.MENGE.Value : 0,
                    RequestedBy = item.AFNAM,
                    Vendor = GetPurchaseOrderVendorFromNumber(vendorNumber),
                };

                if (currentRequest == null)
                {
                    currentRequest = new PurchaseOrderRequest() { SapPurchaseOrder = sapPurchaseOrder };
                    listRequest.Add(currentRequest);

                    currentRequest.SapCreatedBy = item.ERNAM;
                    currentRequest.Status = PurchaseOrderStatus.PO_CREATED;
                    currentRequest.SapPurchaseRequest = item.BANFN;
                    currentRequest.CostCenter = currentItem.CostCenter;
                    currentRequest.Vendor = currentItem.Vendor;
                    currentRequest.InternalOrder = currentItem.InternalOrder;
                    currentRequest.RequestedBy = currentItem.RequestedBy;
                    currentRequest.CreatedBy = currentItem.RequestedBy;
                    currentRequest.RequestType = GetPurchaseOrderTypeFromPrPoType(item.EBAN_BSART, item.EKKO_BSART, item.FLIEF);
                    TraceLog.AddTraceLog($"request type found: {item.EBAN_BSART}. return:{currentRequest.RequestType}");
                    currentRequest.SapCreatedOn = item.AEDAT.HasValue ? DateOnly.FromDateTime(item.AEDAT.Value) : (DateOnly?)null;
                    currentRequest.CreatedOn = item.AEDAT.HasValue ? DateOnly.FromDateTime(item.AEDAT.Value) : (DateOnly?)null;
                }

                if (currentRequest.RequestType == PurchaseOrderType.ZRMI)
                {
                    PurchaseOrderMaterial currentMaterial = referenceData.DienNlagMaterials.FirstOrDefault(mat => mat.Number == currentItem.Material);
                    currentItem.SelectedMaterial = currentMaterial ?? currentItem.MaterialNumber;
                }

                currentRequest.ListItem.Add(currentItem);
                currentRequest.UpdatedListItem.Add(currentItem);
            }

            TraceLog.AddTraceLog("end SearchSapHubRequestFromPurchaseRequestFromDates");
            return listRequest;
        }

        private void UpdateRequestFromSapHubPurchaseRequest(PurchaseOrderRequest currentRequest, PurchaseOrderReferenceData referenceData)
        {
            if (currentRequest == null || string.IsNullOrEmpty(currentRequest.SapPurchaseRequest))
                return;

            TraceLog.AddTraceLog("SAP Request PR: SqlRequestAllItemsFromPR");

            double totalOrdered = 0;
            bool firstItem = true;

            var prWatch = System.Diagnostics.Stopwatch.StartNew();
            var dbResult = _sapHupService.GetAllItemsFromPR(currentRequest.SapPurchaseRequest);
            prWatch.Stop();
            NotifyShadow(() => _shadowRecorder?.RecordPurchaseRequestItems(currentRequest.SapPurchaseRequest, dbResult, prWatch.ElapsedMilliseconds));

            foreach (var dbItem in dbResult)
            {
                int itemNumber = Convert.ToInt32(dbItem.BNFPO);
                PurchaseOrderItem currentItem = currentRequest.ListItem.FirstOrDefault(item => item.Number == itemNumber);
                if (currentItem != null)
                {
                    if (currentItem.DeliveryAdress == null)
                        currentItem.DeliveryAdress = GetSiteLocation(dbItem.BERID);

                    currentItem.Total_Ordered = dbItem.RLWRT.HasValue ? (double)dbItem.RLWRT.Value : 0;
                    currentItem.Price = dbItem.PREIS.HasValue ? (double)dbItem.PREIS.Value : 0;
                    currentItem.Quantity = dbItem.MENGE.HasValue ? (double)dbItem.MENGE.Value : 0;

                    currentItem.CostCenter = GetPurchaseOrderCostCenterFromNumber(dbItem.KOSTL, referenceData);
                    currentItem.InternalOrder = GetPurchaseOrderInternalOrderFromNumber(dbItem.AUFNR);
                    string vendorNumber = (!string.IsNullOrWhiteSpace(dbItem.FLIEF)) ? dbItem.FLIEF : dbItem.LIFNR;
                    currentItem.Vendor = GetPurchaseOrderVendorFromNumber(vendorNumber);
                    totalOrdered += currentItem.Total_Ordered;
                }

                if (firstItem)
                {
                    currentRequest.SapCreatedBy = dbItem.ERNAM;
                    currentRequest.SapCreatedOn = dbItem.ERDAT.HasValue ? DateOnly.FromDateTime(dbItem.ERDAT.Value) : (DateOnly?)null;
                    currentRequest.CostCenter = GetPurchaseOrderCostCenterFromNumber(dbItem.KOSTL, referenceData);
                    currentRequest.InternalOrder = GetPurchaseOrderInternalOrderFromNumber(dbItem.AUFNR);
                    string vendorNumber = (!string.IsNullOrWhiteSpace(dbItem.FLIEF)) ? dbItem.FLIEF : dbItem.LIFNR;
                    currentRequest.Vendor = GetPurchaseOrderVendorFromNumber(vendorNumber);
                    currentRequest.Status = string.IsNullOrEmpty(currentRequest.SapPurchaseOrder)
                        ? PurchaseOrderStatus.PR_CREATED
                        : PurchaseOrderStatus.PO_CREATED;

                    firstItem = false;
                }

                if (string.IsNullOrEmpty(currentRequest.SapPurchaseOrder?.Trim()) && !string.IsNullOrEmpty(dbItem.EBELN?.Trim()))
                    currentRequest.SapPurchaseOrder = dbItem.EBELN;
            }
            currentRequest.Total_Ordered = totalOrdered;
        }

        private void UpdateRequestFromSapHubPurchaseOrder(PurchaseOrderRequest currentRequest)
        {
            if (currentRequest == null || string.IsNullOrEmpty(currentRequest.SapPurchaseOrder))
                return;

            TraceLog.AddTraceLog("SAP Request PO: SqlRequestAllItemsFromPO");

            double totalOrdered = 0;
            bool firstItem = true;

            var poWatch = System.Diagnostics.Stopwatch.StartNew();
            List<AllItemsFromPOResult> dbResult;
            if (_queryRouter != null && _queryRouter.GetMode(OracleShadowApi.PurchaseOrderItemsFromPo) == OracleQueryMode.Batch)
            {
                dbResult = _queryRouter.GetPurchaseOrderItems(currentRequest.SapPurchaseOrder);
                poWatch.Stop();
            }
            else
            {
                dbResult = _sapHupService.GetAllItemsFromPO(currentRequest.SapPurchaseOrder);
                poWatch.Stop();
                NotifyShadow(() => _shadowRecorder?.RecordPurchaseOrderItems(currentRequest.SapPurchaseOrder, dbResult, poWatch.ElapsedMilliseconds));
            }

            foreach (var dbItem in dbResult)
            {
                int itemNumber = Convert.ToInt32(dbItem.EBELP);
                PurchaseOrderItem currentItem = currentRequest.ListItem.FirstOrDefault(item => item.Number == itemNumber);
                if (currentItem != null)
                {
                    if (currentItem.DeliveryAdress == null)
                        currentItem.DeliveryAdress = GetSiteLocation(dbItem.BERID);

                    currentItem.Total_Ordered = dbItem.NETWR.HasValue ? (double)dbItem.NETWR.Value : 0;
                    currentItem.Price = dbItem.PREIS.HasValue ? (double)dbItem.PREIS.Value : 0;
                    currentItem.Quantity = dbItem.MENGE.HasValue ? (double)dbItem.MENGE.Value : 0;
                    totalOrdered += currentItem.Total_Ordered;
                }

                if (firstItem)
                {
                    currentRequest.Status = PurchaseOrderStatus.PO_CREATED;
                    firstItem = false;
                }
            }

            if (totalOrdered > 0)
                currentRequest.Total_Ordered = totalOrdered;
        }

        private void UpdateRequestFromSapHubPurchaseOrderGoodsInvoiceReceipt(PurchaseOrderRequest currentRequest)
        {
            if (currentRequest == null || string.IsNullOrEmpty(currentRequest.SapPurchaseOrder))
                return;

            TraceLog.AddTraceLog("SAP Request GR: SqlRequestUpdateRequestFromPO");

            var sapItemList = new List<PurchaseOrderItemSapHubInformation>();

            var dbResult = _sapHupService.GetUpdateRequestFromPO(currentRequest.SapPurchaseOrder);
            foreach (var dbItem in dbResult)
            {
                var currentSapItem = new PurchaseOrderItemSapHubInformation()
                {
                    OperationStatus = GetOperationStatusFromNumber(dbItem.VGABE),
                    Number = Convert.ToInt32(dbItem.EBELP),
                    SapPurchaseOrder = dbItem.EBELN,
                    Quantity_GR = dbItem.MENG_GR.HasValue ? (double)dbItem.MENG_GR.Value : 0,
                    Quantity_Ordered = dbItem.MENGE_ORDER.HasValue ? (double)dbItem.MENGE_ORDER.Value : 0,
                    Price_GR = dbItem.DMBTR.HasValue ? (double)dbItem.DMBTR.Value : 0,
                    Price_Ordered = dbItem.NETWR.HasValue ? (double)dbItem.NETWR.Value : 0,
                    Price_Paid = dbItem.DMBTR.HasValue ? (double)dbItem.DMBTR.Value : 0,
                    CreditDebit_Info = dbItem.SHKZG,
                    Canceled_Value = dbItem.AREWR.HasValue ? (double)dbItem.AREWR.Value : 0,
                    Mouvement_Type = dbItem.BEWTP,
                    Closed_Check = GetCheckValue(dbItem.ELIKZ),
                };
                if (currentSapItem.CreditDebit_Info == "H")
                    currentSapItem.Price_Paid = currentSapItem.Price_Paid * -1;
                sapItemList.Add(currentSapItem);
            }

            currentRequest.Total_Invoice = 0;
            currentRequest.Total_Goods = 0;
            currentRequest.Total_Real_Goods = 0;

            foreach (var item in currentRequest.ListItem)
            {
                item.GoodReceiptStatus = PurchaseOrderStatus.CREATED;

                var tempGoodsReceiptList = sapItemList.Where(info => info.Number == item.Number && info.OperationStatus == PurchaseOrderStatus.GOODS_RECEIPT).ToList();
                if (tempGoodsReceiptList.Count > 0)
                {
                    item.Total_Ordered = tempGoodsReceiptList.First().Price_Ordered;
                    item.Total_Goods = tempGoodsReceiptList.Where(info => info.CreditDebit_Info == "S").Sum(info => info.Price_GR)
                                     - tempGoodsReceiptList.Where(info => info.CreditDebit_Info == "H").Sum(info => info.Price_GR);

                    if (tempGoodsReceiptList.Any(obj => obj.Closed_Check))
                        item.Closed_Check = true;

                    if (item.Total_Goods >= tempGoodsReceiptList.First().Price_Ordered || item.Closed_Check)
                        item.GoodReceiptStatus = PurchaseOrderStatus.GOODS_RECEIPT;
                    else
                        item.GoodReceiptStatus = PurchaseOrderStatus.PARTIAL_GOODS_RECEIPT;
                }

                var tempInvoiceReceiptList = sapItemList.Where(info => info.Number == item.Number && info.OperationStatus == PurchaseOrderStatus.INVOICE_RECEIPT).ToList();
                if (tempInvoiceReceiptList.Count > 0)
                {
                    item.Total_Ordered = tempInvoiceReceiptList.First().Price_Ordered;
                    item.Total_Invoice = tempInvoiceReceiptList.Sum(info => info.Price_Paid);
                    item.Total_Real_Goods = item.Total_Goods - tempInvoiceReceiptList.Where(obj => obj.Mouvement_Type == "K").Sum(info => info.Canceled_Value);

                    if (item.Total_Invoice >= item.Total_Real_Goods && item.Closed_Check)
                        item.GoodReceiptStatus = PurchaseOrderStatus.INVOICE_RECEIPT;
                    else
                        item.GoodReceiptStatus = PurchaseOrderStatus.PARTIAL_INVOICE_RECEIPT;
                }
            }

            currentRequest.Total_Goods = currentRequest.ListItem.Sum(obj => obj.Total_Goods);
            currentRequest.Total_Real_Goods = currentRequest.ListItem.Sum(obj => obj.Total_Real_Goods);
            currentRequest.Total_Invoice = currentRequest.ListItem.Sum(obj => obj.Total_Invoice);

            UpdateRequestStatus(currentRequest);
        }

        /// <summary>Reprise Ã  l'identique de UdpateRequestStatus (ordre des conditions conservÃ©).</summary>
        private static void UpdateRequestStatus(PurchaseOrderRequest currentRequest)
        {
            var items = currentRequest.ListItem;
            if (items.Count == items.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.INVOICE_RECEIPT))
                currentRequest.Status = PurchaseOrderStatus.CLOSED;
            else if (items.Count == items.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.GOODS_RECEIPT))
                currentRequest.Status = PurchaseOrderStatus.GOODS_RECEIPT;
            else if (items.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.INVOICE_RECEIPT) > 0)
                currentRequest.Status = PurchaseOrderStatus.PARTIAL_INVOICE_RECEIPT;
            else if (items.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.PARTIAL_INVOICE_RECEIPT) > 0)
                currentRequest.Status = PurchaseOrderStatus.PARTIAL_INVOICE_RECEIPT;
            else if (items.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.GOODS_RECEIPT) > 0)
                currentRequest.Status = PurchaseOrderStatus.PARTIAL_GOODS_RECEIPT;
            else if (items.Count(item => item.GoodReceiptStatus == PurchaseOrderStatus.PARTIAL_GOODS_RECEIPT) > 0)
                currentRequest.Status = PurchaseOrderStatus.PARTIAL_GOODS_RECEIPT;
        }

        #endregion

        #region Base de donnÃ©es

        private void UpdateRequestItemAttachment(PurchaseOrderRequest currentRequest, PurchaseOrderReferenceData referenceData)
        {
            if (currentRequest == null)
                return;

            currentRequest.ListItem.Clear();
            currentRequest.UpdatedListItem.Clear();
            currentRequest.ListAttachment.Clear();
            currentRequest.UpdatedListAttachment.Clear();

            foreach (var item in _purchaseOrderService.GetAllPoItems(currentRequest.ID))
            {
                PurchaseOrderItem currentItem = PurchaseOrderItem.GetFromDbItem(item);
                if (currentRequest.RequestType == PurchaseOrderType.ZRMI && !string.IsNullOrEmpty(currentItem.Material))
                    currentItem.SelectedMaterial = referenceData.DienNlagMaterials.FirstOrDefault(mat => mat.Number == currentItem.Material);

                // Correction explicite : un article sans fournisseur ou sans ordre interne en base (colonnes nulles)
                // n'est plus une erreur ("Nullable object must have a value") ; la valeur reste simplement nulle.
                currentItem.Vendor = item.Idvendor.HasValue
                    ? PurchaseOrderVendor.GetVendorFromDbItem(_purchaseOrderService.GetPoVendorById(item.Idvendor.Value).FirstOrDefault())
                    : null;
                currentItem.InternalOrder = item.Idio.HasValue
                    ? PurchaseOrderInternalOrder.GetInternalOrderFromDbItem(_purchaseOrderService.GetPoInternalOrderById(item.Idio.Value).FirstOrDefault())
                    : null;
                currentRequest.ListItem.Add(currentItem);
                currentRequest.UpdatedListItem.Add(currentItem);
                currentItem.DeliveryAdress = referenceData.DeliveryLocations.FirstOrDefault(add => add.Name == currentItem.DeliveryAdress.Name);
            }

            foreach (var attachment in _purchaseOrderService.GetAllPoAttachments(currentRequest.ID))
            {
                currentRequest.ListAttachment.Add(PurchaseOrderAttachment.GetFromDbAttachement(attachment));
                currentRequest.UpdatedListAttachment.Add(PurchaseOrderAttachment.GetFromDbAttachement(attachment));
            }
        }

        /// <summary>
        /// Reprise de CreateUpdateRequestOnDataBase. Retourne l'indicateur IsNew renvoyÃ© par UpsertRequest.
        /// </summary>
        private bool CreateUpdateRequestOnDataBase(PurchaseOrderRequest currentRequestToSave)
        {
            TraceLog.AddTraceLog($"start CreateUpdateRequestOnDataBase: Request:{currentRequestToSave.ID}");
            currentRequestToSave.ListAttachment.Clear();
            currentRequestToSave.ListItem.Clear();
            foreach (var item in currentRequestToSave.UpdatedListAttachment)
                currentRequestToSave.ListAttachment.Add(item);
            foreach (var item in currentRequestToSave.UpdatedListItem)
                currentRequestToSave.ListItem.Add(item);

            // Update Vendor DB
            PoVendor dbVendor = null;
            if (currentRequestToSave.Vendor != null && currentRequestToSave.Vendor.Number != null)
                dbVendor = _purchaseOrderService.UpsertVendor(currentRequestToSave.Vendor.Number, currentRequestToSave.Vendor.Description, currentRequestToSave.Vendor.Location);

            if (dbVendor != null)
            {
                currentRequestToSave.Vendor.IdVendor = dbVendor.Idvendor;
                foreach (var item in currentRequestToSave.ListItem)
                {
                    if (item.Vendor == null)
                        item.Vendor = new PurchaseOrderVendor() { IdVendor = dbVendor.Idvendor };
                    else
                        item.Vendor.IdVendor = dbVendor.Idvendor;
                }
            }

            // Update Internal Order
            if (currentRequestToSave.InternalOrder != null)
            {
                foreach (var item in currentRequestToSave.ListItem)
                {
                    if (item.InternalOrder == null)
                        item.InternalOrder = new PurchaseOrderInternalOrder() { IdIo = currentRequestToSave.InternalOrder.IdIo };
                    else
                        item.InternalOrder.IdIo = currentRequestToSave.InternalOrder.IdIo;
                }
            }

            // Create/update Request
            PoRequest dbRequestToSave = currentRequestToSave.GetDbrequest();
            if (dbRequestToSave.Idio == 0 && currentRequestToSave.InternalOrder?.Number != null)
                dbRequestToSave.Idio = _purchaseOrderService.GetPoInternalOrderByNumber(currentRequestToSave.InternalOrder.Number).FirstOrDefault()?.Idio ?? 0;

            var dbResult = _purchaseOrderService.UpsertRequest(dbRequestToSave);
            PoRequest dbRequest = dbResult.DbRequest;
            bool isNew = dbResult.IsNew;

            if (currentRequestToSave.Status == PurchaseOrderStatus.NEW || currentRequestToSave.Status == PurchaseOrderStatus.SENT)
            {
                double totalOrdered = 0;
                foreach (var item in currentRequestToSave.ListItem)
                {
                    totalOrdered += item.Quantity * item.Price;
                    item.Total_Ordered = item.Quantity * item.Price;
                }
                currentRequestToSave.Total_Ordered = totalOrdered;
            }

            if (isNew)
                currentRequestToSave.ID = dbRequest.Idrequest;

            // Update List Attachment
            List<PurchaseOrderAttachment> currentListAttachment = currentRequestToSave.ListAttachment.ToList();
            foreach (var item in currentListAttachment)
                item.IsDbSaved = false;

            List<PoAttachment> listDbAttachment = _purchaseOrderService.GetAllPoAttachmentsForUpdate(currentRequestToSave.ID);
            foreach (var dbAttachment in listDbAttachment)
            {
                PurchaseOrderAttachment attachment = currentListAttachment.FirstOrDefault(item => dbAttachment.Idattachment == item.IdAttachment);
                if (attachment != null)
                {
                    attachment.UpdateDbAttachment(dbAttachment);
                    attachment.IsDbSaved = true;
                }
                else
                {
                    _purchaseOrderService.DeletePoAttachment(dbAttachment.Idattachment);
                }
            }
            foreach (var attachment in currentListAttachment.Where(item => !item.IsDbSaved))
                _purchaseOrderService.AddPoAttachment(attachment.GetDbAttachment(dbRequest.Idrequest));

            // Update List Item
            List<PoItem> listDbItemForUpdate = currentRequestToSave.ListItem.Select(item => item.GetDbItem(currentRequestToSave.ID)).ToList();
            _purchaseOrderService.ReplaceItemsForRequest(currentRequestToSave.ID, listDbItemForUpdate);

            TraceLog.AddTraceLog("End CreateUpdateRequestOnDataBase");
            return isNew;
        }

        #endregion

        #region Helpers

        private static PurchaseOrderLocation GetSiteLocation(string siteLocationNumber) => siteLocationNumber switch
        {
            "1011" => new PurchaseOrderLocation() { Name = "MLS", Number = siteLocationNumber },
            "1012" => new PurchaseOrderLocation() { Name = "CHL", Number = siteLocationNumber },
            "1015" => new PurchaseOrderLocation() { Name = "LSY-PVC", Number = siteLocationNumber },
            "1050" => new PurchaseOrderLocation() { Name = "NIE", Number = siteLocationNumber },
            "1000" => new PurchaseOrderLocation() { Name = "PTO", Number = siteLocationNumber },
            "2010" => new PurchaseOrderLocation() { Name = "SPC", Number = siteLocationNumber },
            "1070" => new PurchaseOrderLocation() { Name = "CHINA", Number = siteLocationNumber },
            "8010" => new PurchaseOrderLocation() { Name = "DDY", Number = siteLocationNumber },
            _ => new PurchaseOrderLocation() { Name = "UNKNOWN", Number = siteLocationNumber },
        };

        private static PurchaseOrderStatus GetOperationStatusFromNumber(string value) => value switch
        {
            "1" => PurchaseOrderStatus.GOODS_RECEIPT,
            "2" => PurchaseOrderStatus.INVOICE_RECEIPT,
            _ => PurchaseOrderStatus.UNKNOWN,
        };

        private static bool GetCheckValue(string value) => value == "X";

        private static PurchaseOrderLocation GetPurchaseOrderLocationFromNumber(string number, PurchaseOrderReferenceData referenceData)
            => referenceData.DeliveryLocations.FirstOrDefault(item => item.Number == number);

        private PurchaseOrderInternalOrder GetPurchaseOrderInternalOrderFromNumber(string number)
        {
            PoInternalOrder currentDbItem = _purchaseOrderService.GetPoInternalOrderByNumber(number).FirstOrDefault();
            return PurchaseOrderInternalOrder.GetInternalOrderFromDbItem(currentDbItem);
        }

        private PurchaseOrderMaterial GetPurchaseOrderMaterialFromNumber(string number)
        {
            PoMatDienNlag currentDbItem = _purchaseOrderService.GetPoMatDienNlagByNumber(number);
            return PurchaseOrderMaterial.GetMaterialFromDbItem(currentDbItem);
        }

        private static PurchaseOrderCostCenter GetPurchaseOrderCostCenterFromNumber(string number, PurchaseOrderReferenceData referenceData)
        {
            number = number.TrimStart('0');

            return referenceData.CostCenters.FirstOrDefault(item => item.Number == number)
                ?? new PurchaseOrderCostCenter() { Number = number, Description = number };
        }

        private static PurchaseOrderType GetPurchaseOrderTypeFromPrPoType(string prType, string poType = null, string vendorNumber = null)
        {
            if (!string.IsNullOrEmpty(prType))
            {
                if (prType == "NB")
                {
                    if (string.IsNullOrEmpty(poType))
                    {
                        if (string.IsNullOrEmpty(vendorNumber))
                            return PurchaseOrderType.ZNB;
                        else if (Regex.IsMatch(vendorNumber, "^V"))
                            return PurchaseOrderType.ZICP;
                    }
                    else if (poType == "ZICP")
                        return PurchaseOrderType.ZICP;
                }
                else if (prType == "ZRMI")
                {
                    return PurchaseOrderType.ZRMI;
                }
            }

            return PurchaseOrderType.ZRMI;
        }

        private PurchaseOrderVendor GetPurchaseOrderVendorFromNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return null;

            var dbVendor = _sapHupService.GetSearchVendorFromNumber(number);
            if (dbVendor == null)
                return null;

            return new PurchaseOrderVendor()
            {
                Description = dbVendor.NAME1,
                Number = dbVendor.LIFNR,
                Location = dbVendor.LAND1
            };
        }

        #endregion
    }
}
