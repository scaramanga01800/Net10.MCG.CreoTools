using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.DataBaseAccess.Models.SapHupDbResult;
using MCG.Tools.PurchaseOrderFollowUp.Models;
using MCG.Tools.PurchaseOrderFollowUp.Services;
using Moq;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace MCG.Tools.PurchaseOrderFollowUp.Tests
{
    public sealed class PurchaseOrderLoggingTests : IDisposable
    {
        private sealed class CollectingSink : ILogEventSink
        {
            private readonly List<string> _lines = new();
            public void Emit(LogEvent logEvent)
            {
                lock (_lines)
                    _lines.Add(Normalize(logEvent.RenderMessage()));
            }

            private static string Normalize(string message)
            {
                if (!message.StartsWith("Date : ", StringComparison.Ordinal))
                    return message;

                int start = new[]
                {
                    message.IndexOf("PO|", StringComparison.Ordinal),
                    message.IndexOf("Purchase Order summary", StringComparison.Ordinal)
                }.Where(i => i >= 0).DefaultIfEmpty(-1).Min();

                return start >= 0 ? message[start..] : message;
            }

            public List<string> Snapshot()
            {
                lock (_lines)
                    return _lines.ToList();
            }
        }

        private static readonly DateTime From = new(2025, 1, 1);
        private static readonly DateTime To = new(2025, 1, 15);

        private readonly CollectingSink _sink = new();
        private readonly ILogger _previousLogger;
        private readonly Mock<IPurchaseOrderService> _db = new(MockBehavior.Loose);
        private readonly Mock<ISapHupService> _sap = new(MockBehavior.Loose);
        private readonly PurchaseOrderReferenceData _referenceData = new(null, null, null);

        public PurchaseOrderLoggingTests()
        {
            _previousLogger = Log.Logger;
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Sink(_sink).CreateLogger();

            _db.Setup(s => s.GetPoInternalOrderByNumber(It.IsAny<string>())).Returns(new List<PoInternalOrder>());
            _db.Setup(s => s.GetAllPoAttachmentsForUpdate(It.IsAny<int>())).Returns(new List<PoAttachment>());
            _db.Setup(s => s.UpsertRequest(It.IsAny<PoRequest>())).Returns((new PoRequest { Idrequest = 42 }, true));
            _db.Setup(s => s.GetOpenSapPrRequests()).Returns(new List<PoRequest>());
            _db.Setup(s => s.GetOpenRequestsWithSapPoWithoutSapPr()).Returns(new List<PoRequest>());
            _sap.Setup(s => s.GetSearchVendorFromNumber(It.IsAny<string>()))
                .Returns(new VendorSearchResult { LIFNR = "100200", NAME1 = "Vendor", LAND1 = "FR" });
        }

        public void Dispose() => Log.Logger = _previousLogger;

        private PurchaseOrderSapSyncService CreateService() => new(_db.Object, _sap.Object);

        private static PurchaseOrderBetweenDatesResult SapRow(string ebeln, string banfn) => new()
        {
            EBELN = ebeln,
            BANFN = banfn,
            BNFPO = 10,
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

        private List<string> RequestLines(string key) =>
            _sink.Snapshot().Where(l => l.StartsWith("PO|", StringComparison.Ordinal) && l.Contains(key, StringComparison.Ordinal)).ToList();

        [Fact]
        public void Created_LogsOneLineWithRequestPrPoAndStatus()
        {
            SetupSapRows(SapRow("4300184618", "1300190100"));

            CreateService().CreateRequestsFromSap(From, To, _referenceData);

            string line = Assert.Single(RequestLines("4300184618"));
            Assert.StartsWith("PO|RequestId=", line);
            Assert.Contains("|PR=1300190100|", line);
            Assert.Contains("|PO=4300184618|", line);
            Assert.Contains("|Status=CREATED", line);
        }

        [Fact]
        public void UpsertedAsExisting_LogsUpdated()
        {
            SetupSapRows(SapRow("4300184619", "1300190101"));
            _db.Setup(s => s.UpsertRequest(It.IsAny<PoRequest>())).Returns((new PoRequest { Idrequest = 7 }, false));

            CreateService().CreateRequestsFromSap(From, To, _referenceData);

            string line = Assert.Single(RequestLines("4300184619"));
            Assert.Contains("|Status=UPDATED", line);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AlreadyExists_LogsAlreadyExists_ForPoAndPr(bool byPo)
        {
            string po = byPo ? "4300184620" : "4300184621";
            string pr = byPo ? "1300190102" : "1300190103";
            SetupSapRows(SapRow(po, pr));
            if (byPo)
                _db.Setup(s => s.GetPoRequestFromSapPo(po)).Returns(new PoRequest { Idrequest = 1 });
            else
                _db.Setup(s => s.GetPoRequestFromSapPr(pr)).Returns(new PoRequest { Idrequest = 1 });

            CreateService().CreateRequestsFromSap(From, To, _referenceData);

            string line = Assert.Single(RequestLines(po));
            Assert.Contains($"|PR={pr}|PO={po}|Status=ALREADY_EXISTS", line);
        }

        [Fact]
        public void Failure_LogsFailedLineWithPrAndPo_WithoutExceptionPayload()
        {
            SetupSapRows(SapRow("4300184622", "1300190104"));
            _db.Setup(s => s.GetPoRequestFromSapPo("4300184622")).Throws(new InvalidOperationException("Oracle indisponible"));

            var result = CreateService().CreateRequestsFromSap(From, To, _referenceData);

            Assert.Equal(1, result.Failures);
            string line = Assert.Single(RequestLines("4300184622"));
            Assert.Contains("|PR=1300190104|PO=4300184622|Status=FAILED", line);
            Assert.DoesNotContain("Oracle", line);
        }

        [Fact]
        public void PoAbsent_LogsDashForPo_AndPrIsSearchable()
        {
            SetupSapRows(SapRow("", "1300190105"));

            CreateService().CreateRequestsFromSap(From, To, _referenceData);

            string line = Assert.Single(RequestLines("1300190105"));
            Assert.Contains("|PR=1300190105|PO=-|Status=CREATED", line);
        }

        [Fact]
        public void PrAbsent_LogsDashForPr_AndPoIsSearchable()
        {
            SetupSapRows(SapRow("4300184623", ""));

            CreateService().CreateRequestsFromSap(From, To, _referenceData);

            string line = Assert.Single(RequestLines("4300184623"));
            Assert.Contains("|PR=-|PO=4300184623|Status=CREATED", line);
        }

        [Fact]
        public void OnePoLinePerRequest_AndSummaryMatchesCounters()
        {
            SetupSapRows(SapRow("4300184630", "1300190110"), SapRow("4300184631", "1300190111"), SapRow("4300184632", "1300190112"));
            _db.Setup(s => s.GetPoRequestFromSapPo("4300184631")).Returns(new PoRequest { Idrequest = 1 });
            _db.Setup(s => s.GetPoRequestFromSapPo("4300184632")).Throws(new InvalidOperationException("x"));
            _db.Setup(s => s.GetOpenRequestsWithSapPoWithoutSapPr())
                .Returns(new List<PoRequest> { new() { Idrequest = 900, Sappoid = "4300184640" } });

            var result = CreateService().Synchronize(From, To, _referenceData);

            Assert.Single(RequestLines("4300184630"));
            Assert.Contains("Status=ALREADY_EXISTS", Assert.Single(RequestLines("4300184631")));
            Assert.Contains("Status=FAILED", Assert.Single(RequestLines("4300184632")));
            Assert.Contains("Status=SKIPPED", Assert.Single(RequestLines("4300184640")));

            string summary = Assert.Single(_sink.Snapshot(), l => l.StartsWith("Purchase Order summary", StringComparison.Ordinal));
            Assert.Equal(result.ToSummaryLog(), summary);
            Assert.Contains("RequestsAnalyzed=3", summary);
            Assert.Contains("Created=1", summary);
            Assert.Contains("Updated=0", summary);
            Assert.Contains("AlreadyExists=1", summary);
            Assert.Contains("Skipped=1", summary);
            Assert.Contains("Failed=1", summary);
        }

        [Fact]
        public void Logs_DoNotContainSqlOrPayload()
        {
            SetupSapRows(SapRow("4300184650", "1300190120"));

            CreateService().Synchronize(From, To, _referenceData);

            foreach (string line in _sink.Snapshot().Where(l => l.StartsWith("PO|", StringComparison.Ordinal)))
            {
                Assert.DoesNotContain("SELECT", line, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("INSERT", line, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("PREIS", line, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
