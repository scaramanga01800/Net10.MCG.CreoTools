using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.DataBaseAccess.Models.SapHupDbResult;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.Models;
using MCG.Tools.PurchaseOrderFollowUp.Services;
using Moq;
using System.Reflection;

namespace MCG.Tools.PurchaseOrderFollowUp.Tests
{
    public class PurchaseOrderSapSyncServiceTests
    {
        private static readonly DateTime From = new(2025, 1, 1);
        private static readonly DateTime To = new(2025, 1, 15);

        private readonly Mock<IPurchaseOrderService> _db = new(MockBehavior.Loose);
        private readonly Mock<ISapHupService> _sap = new(MockBehavior.Loose);
        private readonly PurchaseOrderReferenceData _referenceData = new(null, null, null);

        public PurchaseOrderSapSyncServiceTests()
        {
            _db.Setup(s => s.GetPoInternalOrderByNumber(It.IsAny<string>())).Returns(new List<PoInternalOrder>());
            _db.Setup(s => s.GetAllPoAttachmentsForUpdate(It.IsAny<int>())).Returns(new List<PoAttachment>());
            _db.Setup(s => s.UpsertRequest(It.IsAny<PoRequest>())).Returns((new PoRequest { Idrequest = 42 }, true));
            _db.Setup(s => s.GetOpenSapPrRequests()).Returns(new List<PoRequest>());
            _db.Setup(s => s.GetOpenRequestsWithSapPoWithoutSapPr()).Returns(new List<PoRequest>());
            _sap.Setup(s => s.GetSearchVendorFromNumber(It.IsAny<string>()))
                .Returns(new VendorSearchResult { LIFNR = "100200", NAME1 = "Vendor", LAND1 = "FR" });
        }

        private PurchaseOrderSapSyncService CreateService() => new(_db.Object, _sap.Object);

        private static PurchaseOrderBetweenDatesResult SapRow(string ebeln = "4500000001", string banfn = "1000000001", short item = 10) => new()
        {
            EBELN = ebeln,
            BANFN = banfn,
            BNFPO = item,
            KOSTL = "0001234",
            AUFNR = "IO1",
            MATNR = "MAT1",
            BERID = "1011",
            LIFNR = "100200",
            EBAN_BSART = "NB",
            PREIS = 10m,
            MENGE = 2m,
            ERNAM = "USER",
            AFNAM = "REQUESTER",
            AEDAT = new DateTime(2025, 1, 5),
        };

        private void SetupSapRows(params PurchaseOrderBetweenDatesResult[] rows) =>
            _sap.Setup(s => s.GetPurchaseOrderBetweenDates(From, To)).Returns(rows.ToList());

        [Fact]
        public void CreateRequestsFromSap_ValidRangeAndEmptySapResult_ReturnsZeroCounters()
        {
            SetupSapRows();

            var result = CreateService().CreateRequestsFromSap(From, To, _referenceData);

            Assert.Equal(0, result.SapRequestsAnalyzed);
            Assert.Equal(0, result.RequestsCreated);
            Assert.False(result.HasFailures);
            _sap.Verify(s => s.GetPurchaseOrderBetweenDates(From, To), Times.Once);
            _db.Verify(s => s.UpsertRequest(It.IsAny<PoRequest>()), Times.Never);
        }

        [Fact]
        public void CreateRequestsFromSap_InvalidRange_ThrowsWithoutCallingSap()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => CreateService().CreateRequestsFromSap(To, From, _referenceData));

            Assert.Equal("fromDate", ex.ParamName);
            _sap.VerifyNoOtherCalls();
        }

        [Fact]
        public void Synchronize_InvalidRange_ThrowsWithoutAnyExternalCall()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateService().Synchronize(To, From, _referenceData));

            _sap.VerifyNoOtherCalls();
            _db.VerifyNoOtherCalls();
        }

        [Fact]
        public void CreateRequestsFromSap_RequestNotInDataBase_IsCreated()
        {
            SetupSapRows(SapRow(), SapRow(item: 20));

            var result = CreateService().CreateRequestsFromSap(From, To, _referenceData);

            Assert.Equal(1, result.SapRequestsAnalyzed);
            Assert.Equal(1, result.RequestsCreated);
            Assert.Equal(0, result.ExistingRequestsSkipped);
            Assert.False(result.HasFailures);
            _db.Verify(s => s.GetPoRequestFromSapPo("4500000001"), Times.Once);
            _db.Verify(s => s.GetPoRequestFromSapPr("1000000001"), Times.Once);
            _db.Verify(s => s.UpsertRequest(It.Is<PoRequest>(r => r.Sappoid == "4500000001" && r.Sapprid == "1000000001")), Times.Once);
            _db.Verify(s => s.ReplaceItemsForRequest(42, It.Is<IEnumerable<PoItem>>(items => items.Count() == 2)), Times.Once);
        }

        [Fact]
        public void CreateRequestsFromSap_UpsertReportsExisting_CountedAsUpsertedExisting()
        {
            SetupSapRows(SapRow());
            _db.Setup(s => s.UpsertRequest(It.IsAny<PoRequest>())).Returns((new PoRequest { Idrequest = 7 }, false));

            var result = CreateService().CreateRequestsFromSap(From, To, _referenceData);

            Assert.Equal(0, result.RequestsCreated);
            Assert.Equal(1, result.NewSapRequestsUpsertedAsExisting);
            Assert.False(result.HasFailures);
        }

        [Fact]
        public void CreateRequestsFromSap_RequestExistsBySapPr_IsSkipped()
        {
            SetupSapRows(SapRow());
            _db.Setup(s => s.GetPoRequestFromSapPr("1000000001")).Returns(new PoRequest { Idrequest = 1 });

            var result = CreateService().CreateRequestsFromSap(From, To, _referenceData);

            Assert.Equal(1, result.ExistingRequestsSkipped);
            Assert.Equal(0, result.RequestsCreated);
            _db.Verify(s => s.UpsertRequest(It.IsAny<PoRequest>()), Times.Never);
        }

        [Fact]
        public void CreateRequestsFromSap_RequestExistsBySapPo_IsSkippedWithoutPrCheck()
        {
            SetupSapRows(SapRow());
            _db.Setup(s => s.GetPoRequestFromSapPo("4500000001")).Returns(new PoRequest { Idrequest = 1 });

            var result = CreateService().CreateRequestsFromSap(From, To, _referenceData);

            Assert.Equal(1, result.ExistingRequestsSkipped);
            _db.Verify(s => s.GetPoRequestFromSapPr(It.IsAny<string>()), Times.Never);
            _db.Verify(s => s.UpsertRequest(It.IsAny<PoRequest>()), Times.Never);
        }

        [Fact]
        public void CreateRequestsFromSap_ExistenceCheckFails_IsFailureAndNotCreated()
        {
            SetupSapRows(SapRow(), SapRow(ebeln: "4500000002", banfn: "1000000002"));
            var dbError = new InvalidOperationException("Oracle indisponible");
            _db.Setup(s => s.GetPoRequestFromSapPo("4500000001")).Throws(dbError);

            var result = CreateService().CreateRequestsFromSap(From, To, _referenceData);

            Assert.Equal(2, result.SapRequestsAnalyzed);
            Assert.Equal(1, result.Failures);
            Assert.Equal(1, result.RequestsCreated);
            Assert.Same(dbError, result.Errors[0].Exception);
            Assert.Equal("4500000001", result.Errors[0].SapPurchaseOrder);
            _db.Verify(s => s.UpsertRequest(It.Is<PoRequest>(r => r.Sappoid == "4500000001")), Times.Never);
        }

        [Fact]
        public void CreateRequestsFromSap_CancelledBeforeIteration_ThrowsAndPersistsNothing()
        {
            SetupSapRows(SapRow());
            using var cts = new CancellationTokenSource();
            _sap.Setup(s => s.GetPurchaseOrderBetweenDates(From, To))
                .Returns(() => { cts.Cancel(); return new List<PurchaseOrderBetweenDatesResult> { SapRow() }; });

            Assert.ThrowsAny<OperationCanceledException>(() => CreateService().CreateRequestsFromSap(From, To, _referenceData, cts.Token));

            _db.Verify(s => s.GetPoRequestFromSapPo(It.IsAny<string>()), Times.Never);
            _db.Verify(s => s.UpsertRequest(It.IsAny<PoRequest>()), Times.Never);
        }

        [Fact]
        public void CreateRequestsFromSap_SapThrows_WrapsWithContextAndKeepsOriginalException()
        {
            var sapError = new TimeoutException("SAP HUP timeout");
            _sap.Setup(s => s.GetPurchaseOrderBetweenDates(From, To)).Throws(sapError);

            var ex = Assert.Throws<PurchaseOrderSapSyncException>(() => CreateService().CreateRequestsFromSap(From, To, _referenceData));

            Assert.Same(sapError, ex.InnerException);
            Assert.Contains("01-01-2025", ex.Message);
        }

        [Fact]
        public void UpdateOpenRequestsFromSap_PoWithoutPr_LoadedButNotProcessed()
        {
            _db.Setup(s => s.GetOpenRequestsWithSapPoWithoutSapPr())
                .Returns(new List<PoRequest> { new() { Idrequest = 1 }, new() { Idrequest = 2 }, new() { Idrequest = 3 } });

            var result = CreateService().UpdateOpenRequestsFromSap(_referenceData);

            Assert.Equal(3, result.OpenRequestsWithSapPoWithoutSapPrNotProcessed);
            Assert.Equal(0, result.OpenRequestsUpdated);
            _sap.VerifyNoOtherCalls();
            _db.Verify(s => s.UpsertRequest(It.IsAny<PoRequest>()), Times.Never);
        }

        [Fact]
        public void UpdateOpenRequestsFromSap_ItemWithoutVendorAndInternalOrder_IsUpdatedWithoutFailure()
        {
            _db.Setup(s => s.GetOpenSapPrRequests()).Returns(new List<PoRequest>
            {
                new() { Idrequest = 526, Sapprid = "1300179994", Status = "PO_CREATED", Requestype = "ZRMI" }
            });
            _db.Setup(s => s.GetAllPoItems(526)).Returns(new List<PoItem>
            {
                new() { Iditem = 1, Number = 10, Price = 5, Quantity = 2, Grstatus = "CREATED", Idvendor = null, Idio = null }
            });
            _db.Setup(s => s.GetAllPoAttachments(526)).Returns(new List<PoAttachment>());
            _sap.Setup(s => s.GetAllItemsFromPR("1300179994")).Returns(new List<AllItemsFromPRResult>());

            var result = CreateService().UpdateOpenRequestsFromSap(_referenceData);

            Assert.Equal(1, result.OpenRequestsAnalyzed);
            Assert.Equal(1, result.OpenRequestsUpdated);
            Assert.Equal(0, result.Failures);
            _db.Verify(s => s.GetPoVendorById(It.IsAny<int?>()), Times.Never);
            _db.Verify(s => s.GetPoInternalOrderById(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public void Service_HasNoUiDependency()
        {
            var type = typeof(PurchaseOrderSapSyncService);
            var ctorParameters = type.GetConstructors().Single().GetParameters().Select(p => p.ParameterType).ToList();
            Assert.Equal(new[] { typeof(IPurchaseOrderService), typeof(ISapHupService), typeof(MCG.CommonLib.DataBaseAccess.Shadow.IOracleShadowRecorder), typeof(MCG.CommonLib.DataBaseAccess.OracleQuery.IOracleQueryRouter) }, ctorParameters);

            var fieldTypes = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Select(f => f.FieldType);
            Assert.DoesNotContain(fieldTypes, t => (t.Namespace ?? string.Empty).StartsWith("System.Windows", StringComparison.Ordinal));
            Assert.DoesNotContain(fieldTypes, t => t.Name.Contains("ViewModel", StringComparison.Ordinal) || t.Name.Contains("WindowService", StringComparison.Ordinal));
        }
    }
}
