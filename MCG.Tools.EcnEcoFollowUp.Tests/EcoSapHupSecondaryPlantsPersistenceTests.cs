using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.DataBaseAccess.Models.SapHupDb;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.Models.EcoSapHup;
using MCG.Tools.EcnEcoFollowUp.Services.EcoSapHup;
using Moq;

namespace MCG.Tools.EcnEcoFollowUp.Tests
{
    /// <summary>Determinisme de ECO_WI_SECONDARY_PLANTS via le service reel et un depot de test (aucune base).</summary>
    public class EcoSapHupSecondaryPlantsPersistenceTests
    {
        private sealed class FixedTimeProvider : TimeProvider
        {
            public override DateTimeOffset GetUtcNow() => new(2025, 3, 7, 12, 0, 0, TimeSpan.Zero);
            public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
        }

        private readonly Mock<IEcoSapHupSyncRepository> _repo = new(MockBehavior.Strict);
        private readonly Mock<ISapHupService> _sap = new(MockBehavior.Strict);
        private readonly Ecnecofollowup _eco = new() { Id = 1, EcnNumber = "E-1", EcoTmlpseWiClose = 7 };
        private int _updateCalls;

        private static string Snapshot(Ecnecofollowup e) => string.Join("|",
            typeof(Ecnecofollowup).GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string)).Select(p => p.GetValue(e)?.ToString()));

        private EcoSapHupSyncService CreateService()
        {
            _repo.Setup(r => r.GetCandidates(new DateOnly(2024, 3, 7))).Returns(new[] { new EcoSapHupCandidate(1, "E-1", null, false, true) });
            // Reproduit le contrat du depot : true uniquement si au moins un champ a change (ChangeTracker.HasChanges).
            _repo.Setup(r => r.UpdateEco(It.IsAny<int>(), It.IsAny<Action<Ecnecofollowup>>()))
                .Returns((int _, Action<Ecnecofollowup> apply) =>
                {
                    _updateCalls++;
                    string before = Snapshot(_eco);
                    apply(_eco);
                    return before != Snapshot(_eco);
                });
            return new EcoSapHupSyncService(_repo.Object, _sap.Object, new EcoSapHupStepCalculator(), new FixedTimeProvider());
        }

        private void Sap(params string[] plantsInOracleOrder)
        {
            _sap.Setup(s => s.GetEcoViewStatusData("E-1")).Returns(new List<DmEcoViewStatusDate>
            {
                new()
                {
                    Eco = "E-1", EcoDesc = "d", EcoCoord = "P1", EcoCurrentStatus = 2,
                    EcoCreationDate = new DateTime(2024, 1, 1), EcoValidFrom = new DateTime(2024, 1, 2),
                    EcoStat1Date = new DateTime(2024, 1, 3), EcoStat2Date = new DateTime(2024, 1, 4), EcoLastChange = new DateTime(2024, 1, 4)
                }
            });
            _sap.Setup(s => s.GetEcoWfTaskData("E-1")).Returns(plantsInOracleOrder
                .Select(p => EcoSapHupStepCalculatorTests.Task(p, "P1", "S", created: new DateTime(2024, 2, 1))).ToList());
        }

        private EcoSapHupSyncResult Sync() => CreateService().Synchronize();

        [Fact]
        public void CanonicalExisting_DifferentOracleOrder_NoUpdate_Unchanged()
        {
            Sap("US03", "DE01", "FR02");
            Assert.Equal(1, Sync().Updated);
            Assert.Equal("DE01,FR02,US03", _eco.EcoWiSecondaryPlants);

            _sap.Reset();
            _repo.Reset();
            Sap("FR02", "US03", "DE01");
            var second = Sync();
            Assert.Equal(0, second.Updated);
            Assert.Equal(1, second.Unchanged);
            Assert.Equal("DE01,FR02,US03", _eco.EcoWiSecondaryPlants);
        }

        [Fact]
        public void NonCanonicalExisting_NormalizedOnce_ThenStable_OtherColumnsUntouched()
        {
            _eco.EcoWiSecondaryPlants = "US03,DE01"; // ancien ordre d'apparition Oracle
            Sap("US03", "DE01");
            var first = Sync();
            Assert.Equal(1, first.Updated);
            Assert.Equal("DE01,US03", _eco.EcoWiSecondaryPlants);

            string stable = Snapshot(_eco);
            for (int i = 0; i < 2; i++)
            {
                _sap.Reset(); _repo.Reset();
                Sap(i == 0 ? "DE01" : "US03", i == 0 ? "US03" : "DE01");
                var r = Sync();
                Assert.Equal(0, r.Updated);
                Assert.Equal(1, r.Unchanged);
                Assert.Equal(stable, Snapshot(_eco));
            }
        }

        [Theory]
        [InlineData("DE01,US03", new[] { "US03", "DE01", "FR02" }, "DE01,FR02,US03")] // ajout
        [InlineData("DE01,FR02,US03", new[] { "US03", "DE01" }, "DE01,US03")]          // suppression
        [InlineData("DE01,US03", new[] { "US03", "DE01", "US03", "DE01" }, "DE01,US03")] // doublons : pas de maj
        [InlineData("DE01", new[] { "de01" }, "de01")]                                   // casse : ordinal
        public void SetChanges_FollowOrdinalSemantics(string existing, string[] oracle, string expected)
        {
            _eco.EcoWiSecondaryPlants = existing;
            Sap(oracle);
            Sync(); // stabilise les autres colonnes ECO : seul ECO_WI_SECONDARY_PLANTS est ensuite controle
            _sap.Reset(); _repo.Reset();
            _eco.EcoWiSecondaryPlants = existing;
            Sap(oracle);
            var r = Sync();
            Assert.Equal(expected, _eco.EcoWiSecondaryPlants);
            bool changesExpected = existing != expected;
            Assert.Equal(changesExpected ? 1 : 0, r.Updated);
            if (!changesExpected)
                Assert.Equal(1, r.Unchanged);
            Assert.Equal(1, r.Updated + r.Unchanged);
        }

        [Fact]
        public void SeparatorIsCommaWithoutSpaces()
        {
            Sap("B", "A");
            Sync();
            Assert.Equal("A,B", _eco.EcoWiSecondaryPlants);
            Assert.DoesNotContain(" ", _eco.EcoWiSecondaryPlants);
        }
    }
}
