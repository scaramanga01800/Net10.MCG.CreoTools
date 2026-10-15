using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.DataBaseAccess.Models.SapHupDb;
using MCG.Tools.EcnEcoFollowUp.Models.EcoSapHup;
using MCG.Tools.EcnEcoFollowUp.Services.EcoSapHup;

namespace MCG.Tools.EcnEcoFollowUp.Tests
{
    public class EcoSapHupMapperTests
    {
        private static readonly DateOnly Today = new(2025, 3, 7);

        private static DmEcoViewStatusDate MainRow(byte? status) => new()
        {
            Eco = "E-1", EcoDesc = "desc", EcoCoord = "P1", EcoCoordDesc = "Plant 1",
            EcoCreationDate = new DateTime(2024, 1, 1, 13, 45, 0), EcoValidFrom = new DateTime(2024, 1, 5),
            EcoCateg = "C", EcoProject = "PR", EcoSubLine = "SL", EcoUrge = "U",
            EcoCurrentStatus = status,
            EcoStat1Date = new DateTime(2024, 1, 2), EcoStat2Date = new DateTime(2024, 1, 3), EcoStat3Date = new DateTime(2024, 1, 4),
            EcoLastChange = new DateTime(2024, 6, 1, 23, 59, 0)
        };

        private static readonly EcoSapHupStepResult Steps = new(true, "MAIN", new DateTime(2024, 2, 1, 5, 0, 0), "SEC", new DateTime(2024, 2, 2), "P2,P3");

        private static Ecnecofollowup Apply(byte? status, bool lastChangeDiffers = false, Ecnecofollowup? eco = null)
        {
            eco ??= new Ecnecofollowup { Id = 1, EcnNumber = "E-1", EcoTmlpseWiClose = 42, EcoStatus = "01", EcoClosedOn = new DateOnly(2000, 1, 1), EcoWfStartedOn = new DateOnly(2000, 1, 1) };
            EcoSapHupMapper.Apply(eco, EcoSapHupMapper.CreateSnapshot(MainRow(status), Steps, lastChangeDiffers), Today);
            return eco;
        }

        [Fact]
        public void Status99()
        {
            var e = Apply(99);
            Assert.Equal("99", e.EcoStatus);
            Assert.Null(e.EcoClosedOn);
            Assert.Null(e.EcoWfStartedOn);
            Assert.Equal("MAIN", e.EcoNextStep);
            Assert.Equal("07/03/2025", e.SapUpdateStatus);
        }

        [Fact]
        public void Status1()
        {
            var e = Apply(1);
            Assert.Equal("01", e.EcoStatus);
            Assert.Equal(new DateOnly(2024, 1, 2), e.EcoWfStartedOn);
            Assert.Null(e.EcoClosedOn);
            Assert.Equal(new DateOnly(2024, 2, 1), e.EcoLastWiCreatedOn);
        }

        [Fact]
        public void Status2_ClearsMainStep_AndNoUpdateRequired()
        {
            var e = Apply(2);
            Assert.Equal("02", e.EcoStatus);
            Assert.Equal(new DateOnly(2024, 1, 2), e.EcoWfStartedOn);
            Assert.Equal(new DateOnly(2024, 1, 3), e.EcoClosedOn);
            Assert.Equal("", e.EcoNextStep);
            Assert.Null(e.EcoLastWiCreatedOn);
            Assert.Equal("SEC", e.EcoNextStepSecondary);
            Assert.Equal(EcoSapHupMapper.NoUpdateRequired, e.SapUpdateStatus);
        }

        [Fact]
        public void Status3()
        {
            var e = Apply(3);
            Assert.Equal("03", e.EcoStatus);
            Assert.Equal(new DateOnly(2024, 1, 4), e.EcoClosedOn);
            Assert.Equal("", e.EcoNextStep);
            Assert.Equal(EcoSapHupMapper.NoUpdateRequired, e.SapUpdateStatus);
        }

        [Fact]
        public void NullStatus_KeepsStatusAndDates()
        {
            var e = Apply(null);
            Assert.Equal("01", e.EcoStatus);
            Assert.Equal(new DateOnly(2000, 1, 1), e.EcoClosedOn);
            Assert.Equal(new DateOnly(2000, 1, 1), e.EcoWfStartedOn);
            Assert.Equal("07/03/2025", e.SapUpdateStatus);
        }

        [Fact]
        public void DateTimeToDateOnly_DropsTime()
        {
            Assert.Equal(new DateOnly(2024, 1, 1), EcoSapHupMapper.ToDateOnly(new DateTime(2024, 1, 1, 23, 59, 59)));
            Assert.Null(EcoSapHupMapper.ToDateOnly(null));
            Assert.Equal(new DateOnly(2024, 1, 1), Apply(1).EcoCreatedOn);
        }

        [Fact]
        public void LastChangeDiffers_OverridesClosedOn_AfterSapUpdateStatus()
        {
            var e = Apply(2, lastChangeDiffers: true);
            Assert.Equal(new DateOnly(2024, 6, 1), e.EcoClosedOn);
            Assert.Equal(EcoSapHupMapper.NoUpdateRequired, e.SapUpdateStatus);
        }

        [Fact]
        public void NoLastChangeDifference_KeepsNormalMapping()
            => Assert.Equal(new DateOnly(2024, 1, 3), Apply(2).EcoClosedOn);

        [Fact]
        public void LastChangeDiffers_Comparison()
        {
            Assert.False(EcoSapHupMapper.LastChangeDiffers(new DateOnly(2024, 6, 1), new DateTime(2024, 6, 1, 10, 0, 0)));
            Assert.True(EcoSapHupMapper.LastChangeDiffers(new DateOnly(2024, 6, 1), new DateTime(2024, 6, 2)));
            Assert.True(EcoSapHupMapper.LastChangeDiffers(new DateOnly(2024, 6, 1), null));
        }

        [Fact]
        public void MissingRequiredDate_PreventsNoUpdateRequired()
        {
            var main = MainRow(2);
            main.EcoValidFrom = null;
            var eco = new Ecnecofollowup { Id = 1 };
            EcoSapHupMapper.Apply(eco, EcoSapHupMapper.CreateSnapshot(main, Steps, false), Today);
            Assert.Equal("07/03/2025", eco.SapUpdateStatus);
        }

        [Fact]
        public void SapUpdateStatusFormat_IsInvariant()
            => Assert.Equal("01/12/2025", EcoSapHupMapper.FormatSapUpdateStatusDate(new DateOnly(2025, 12, 1)));

        [Fact]
        public void SecondaryPlantsFormatting()
        {
            Assert.Equal("P2,P3", Apply(1).EcoWiSecondaryPlants);
            Assert.Equal("A,B", EcoSapHupStepCalculator.FormatSecondaryPlants(new[] { "A", "B", "A" }));
            Assert.Equal("", EcoSapHupStepCalculator.FormatSecondaryPlants(Array.Empty<string>()));
        }

        [Fact]
        public void TmlpseWiClose_IsPreserved_UnresolvedSource()
            => Assert.Equal(42, Apply(1).EcoTmlpseWiClose);

        [Fact]
        public void NullOptionalStrings_MappedAsEmpty_AsHistoricalReader()
        {
            var main = MainRow(1);
            main.EcoCateg = null; main.EcoProject = null; main.EcoCoord = null;
            var eco = new Ecnecofollowup { Id = 1, EcoTmlpseWiClose = 5, PdmContext = "ctx" };
            EcoSapHupMapper.Apply(eco, EcoSapHupMapper.CreateSnapshot(main, Steps, false), Today);
            Assert.Equal("", eco.EcoCateg);
            Assert.Equal("", eco.EcoProject);
            Assert.Equal("", eco.EcoMainPlant);
            Assert.Equal(5, eco.EcoTmlpseWiClose);
            Assert.Equal("ctx", eco.PdmContext);
        }
    }
}
