using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.Tools.EcnEcoFollowUp.Services.EcoSapHup;

namespace MCG.Tools.EcnEcoFollowUp.Tests
{
    public class EcoSapHupCandidateSelectionTests
    {
        private static readonly DateOnly ClosedAfter = new(2024, 3, 7);
        private static readonly DateOnly Inside = new(2024, 6, 1);
        private static readonly DateOnly Outside = new(2023, 1, 1);

        private static Ecnecofollowup Eco(int id, string? status, DateOnly? closedOn, string? ecnState, string? sapUpdate = EcoSapHupMapper.NoUpdateRequired)
            => new() { Id = id, EcnNumber = $"E-{id:000}", EcoStatus = status, EcoClosedOn = closedOn, EcnState = ecnState, SapUpdateStatus = sapUpdate };

        private static IReadOnlyList<MCG.Tools.EcnEcoFollowUp.Models.EcoSapHup.EcoSapHupCandidate> Select(params Ecnecofollowup[] rows)
            => EcoSapHupSyncRepository.SelectCandidates(rows.AsQueryable(), ClosedAfter);

        [Fact]
        public void GroupA_WithNonResolvedEcnState_IsSelected()
        {
            var c = Assert.Single(Select(Eco(1, "02", Inside, "Open")));
            Assert.True(c.IsRollingYearCandidate);
            Assert.False(c.IsPendingSapUpdateCandidate);
        }

        [Fact]
        public void GroupA_WithNullEcnState_IsSelected()
            => Assert.True(Assert.Single(Select(Eco(1, "03", Inside, null))).IsRollingYearCandidate);

        [Theory]
        [InlineData("02")]
        [InlineData("03")]
        public void GroupA_ClosedWithinRollingYear_IsSelected(string status)
            => Assert.Single(Select(Eco(1, status, Inside, "Open")));

        [Fact]
        public void GroupA_OutsideRollingYear_OrOnBoundary_IsNotSelected()
            => Assert.Empty(Select(Eco(1, "02", Outside, "Open"), Eco(2, "03", ClosedAfter, "Open"), Eco(3, "02", null, "Open")));

        [Fact]
        public void GroupBOnly_IsSelected()
        {
            var c = Assert.Single(Select(Eco(1, "01", null, "Resolved", null)));
            Assert.False(c.IsRollingYearCandidate);
            Assert.True(c.IsPendingSapUpdateCandidate);
        }

        [Fact]
        public void BothGroups_SelectedOnce_WithBothFlags()
        {
            var c = Assert.Single(Select(Eco(1, "02", Inside, "Resolved", "07/03/2025")));
            Assert.True(c.IsRollingYearCandidate);
            Assert.True(c.IsPendingSapUpdateCandidate);
        }

        [Fact]
        public void NoGroup_IsNotSelected()
            => Assert.Empty(Select(
                Eco(1, "01", Inside, "Open", null),
                Eco(2, "99", Inside, "Resolved"),
                Eco(3, "02", Outside, "Resolved")));

        [Fact]
        public void Union_IsOrderedByEcnNumberThenId()
        {
            var r = Select(Eco(3, "02", Inside, "Open"), Eco(1, "01", null, "Resolved", null), Eco(2, "03", Inside, "Resolved", null));
            Assert.Equal(new[] { 1, 2, 3 }, r.Select(c => c.Id));
        }
    }
}
