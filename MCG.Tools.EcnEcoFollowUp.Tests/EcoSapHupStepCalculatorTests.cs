using MCG.CommonLib.DataBaseAccess.Models.SapHupDb;
using MCG.Tools.EcnEcoFollowUp.Services.EcoSapHup;

namespace MCG.Tools.EcnEcoFollowUp.Tests
{
    public class EcoSapHupStepCalculatorTests
    {
        private readonly EcoSapHupStepCalculator _calculator = new();
        private static readonly DateTime D1 = new(2024, 1, 10, 8, 0, 0);
        private static readonly DateTime D2 = new(2024, 2, 20, 9, 0, 0);
        private static readonly DateTime D3 = new(2024, 3, 30, 10, 0, 0);

        internal static DmEcoViewWfEcoTask Task(string? plant, string? coord, string? type, string? status = "STARTED", DateTime? created = null)
            => new() { Eco = "E-1", CalculatedPlant = plant, EcoCoord = coord, TypeItem = type, WiStatus = status, WiCreationDate = created };

        [Fact]
        public void EmptyList_ReturnsNoTask()
        {
            var r = _calculator.Calculate(new List<DmEcoViewWfEcoTask>());
            Assert.False(r.HasTasks);
            Assert.Equal("", r.MainNextStep);
            Assert.Null(r.MainLastWiCreatedOn);
            Assert.Equal("", r.SecondaryNextStep);
            Assert.Equal("", r.SecondaryPlants);
        }

        [Fact]
        public void OnlyCompletedTasks_ReturnsEmptySteps()
        {
            var r = _calculator.Calculate(new[] { Task("P1", "P1", "A", "COMPLETED", D1), Task("P2", "P1", "B", "COMPLETED", D2) });
            Assert.True(r.HasTasks);
            Assert.Equal("", r.MainNextStep);
            Assert.Equal("", r.SecondaryNextStep);
            Assert.Equal("", r.SecondaryPlants);
        }

        [Fact]
        public void OneMainTask()
        {
            var r = _calculator.Calculate(new[] { Task("P1", "P1", "STEP", created: D1) });
            Assert.Equal("STEP", r.MainNextStep);
            Assert.Equal(D1, r.MainLastWiCreatedOn);
            Assert.Equal("", r.SecondaryNextStep);
        }

        [Fact]
        public void SeveralMainTasks_MaxTypeItemOrdinal()
        {
            var r = _calculator.Calculate(new[] { Task("P1", "P1", "B", created: D2), Task("P1", "P1", "C", created: D3), Task("P1", "P1", "A", created: D1) });
            Assert.Equal("C", r.MainNextStep);
            Assert.Equal(D3, r.MainLastWiCreatedOn);
        }

        [Fact]
        public void TiesOnMaxTypeItem_LastInInputOrderWins()
        {
            var r = _calculator.Calculate(new[] { Task("P1", "P1", "X", created: D1), Task("P1", "P1", "X", created: D2) });
            Assert.Equal(D2, r.MainLastWiCreatedOn);
        }

        [Fact]
        public void OneSecondaryTask()
        {
            var r = _calculator.Calculate(new[] { Task("P2", "P1", "S", created: D1) });
            Assert.Equal("", r.MainNextStep);
            Assert.Equal("S", r.SecondaryNextStep);
            Assert.Equal(D1, r.SecondaryLastWiCreatedOn);
            Assert.Equal("P2", r.SecondaryPlants);
        }

        [Fact]
        public void SeveralAndDuplicateSecondaryPlants_DistinctFirstAppearanceOrder()
        {
            var r = _calculator.Calculate(new[]
            {
                Task("P3", "P1", "A"), Task("P2", "P1", "C", created: D2), Task("P3", "P1", "B"), Task("P2", "P1", "A")
            });
            Assert.Equal("P3,P2", r.SecondaryPlants);
            Assert.Equal("C", r.SecondaryNextStep);
            Assert.Equal(D2, r.SecondaryLastWiCreatedOn);
        }

        [Fact]
        public void NullCalculatedPlant_AndNullCoord_AreEqualAsEmpty()
        {
            var r = _calculator.Calculate(new[] { Task(null, null, "A"), Task(null, "P1", "B") });
            Assert.Equal("A", r.MainNextStep);
            Assert.Equal("B", r.SecondaryNextStep);
            Assert.Equal("", r.SecondaryPlants);
        }

        [Fact]
        public void NullTypeItem_SortedFirst_AndMappedAsEmpty()
        {
            var r = _calculator.Calculate(new[] { Task("P1", "P1", null, created: D1) });
            Assert.Equal("", r.MainNextStep);
            Assert.Equal(D1, r.MainLastWiCreatedOn);

            var r2 = _calculator.Calculate(new[] { Task("P1", "P1", "A", created: D2), Task("P1", "P1", null, created: D1) });
            Assert.Equal("A", r2.MainNextStep);
        }

        [Fact]
        public void NullCreationDate_IsKeptNull()
        {
            var r = _calculator.Calculate(new[] { Task("P1", "P1", "A") });
            Assert.Equal("A", r.MainNextStep);
            Assert.Null(r.MainLastWiCreatedOn);
        }

        [Fact]
        public void CompletedAndOpenMixed_CompletedIgnored()
        {
            var r = _calculator.Calculate(new[]
            {
                Task("P1", "P1", "Z", "COMPLETED", D3), Task("P1", "P1", "A", created: D1),
                Task("P9", "P1", "Z", "COMPLETED", D3), Task("P2", "P1", "B", created: D2)
            });
            Assert.Equal("A", r.MainNextStep);
            Assert.Equal("B", r.SecondaryNextStep);
            Assert.Equal("P2", r.SecondaryPlants);
        }

        [Fact]
        public void NullStatus_IsNotCompleted()
        {
            var r = _calculator.Calculate(new[] { Task("P1", "P1", "A", null, D1) });
            Assert.Equal("A", r.MainNextStep);
        }

        [Fact]
        public void CompletionStatus_IsCaseSensitive_AsHistorical()
        {
            var r = _calculator.Calculate(new[] { Task("P1", "P1", "A", "completed", D1) });
            Assert.Equal("A", r.MainNextStep);
        }

        [Fact]
        public void MaxTypeItem_IndependentOfOrdering_WhenUnique()
        {
            var a = _calculator.Calculate(new[] { Task("P1", "P1", "A"), Task("P1", "P1", "B") });
            var b = _calculator.Calculate(new[] { Task("P1", "P1", "B"), Task("P1", "P1", "A") });
            Assert.Equal(a.MainNextStep, b.MainNextStep);
        }
    }
}
