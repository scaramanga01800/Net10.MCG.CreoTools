using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.DataBaseAccess.Models.SapHupDb;
using MCG.Tools.EcnEcoFollowUp.Models.EcoSapHup;
using System.Globalization;

namespace MCG.Tools.EcnEcoFollowUp.Services.EcoSapHup
{
    /// <summary>
    /// Pure mapping rules reconstructed from EcnEcoFollowUpManagement.UpdateEcoFromSapHup.
    /// </summary>
    public static class EcoSapHupMapper
    {
        public const string NoUpdateRequired = "NO UPDATE REQUIRED";
        public const string SapUpdateStatusDateFormat = "dd/MM/yyyy";

        /// <summary>Explicit DateTime? to DateOnly? conversion (time component dropped).</summary>
        public static DateOnly? ToDateOnly(DateTime? value)
            => value.HasValue ? DateOnly.FromDateTime(value.Value) : null;

        /// <summary>Historical format DateTime.Now.ToString("dd/MM/yyyy"), made culture-independent.</summary>
        public static string FormatSapUpdateStatusDate(DateOnly processingDate)
            => processingDate.ToString(SapUpdateStatusDateFormat, CultureInfo.InvariantCulture);

        /// <summary>Historical comparison eco.ECO_CLOSED_ON != ECO_LAST_CHANGE, at day granularity.</summary>
        public static bool LastChangeDiffers(DateOnly? storedClosedOn, DateTime? sapLastChange)
            => storedClosedOn != ToDateOnly(sapLastChange);

        public static EcoSapHupSnapshot CreateSnapshot(DmEcoViewStatusDate main, EcoSapHupStepResult steps, bool lastChangeDiffers)
        {
            ArgumentNullException.ThrowIfNull(main);
            ArgumentNullException.ThrowIfNull(steps);

            // Historical GetStringColumnItem returned "" for DB NULL.
            return new EcoSapHupSnapshot
            {
                Eco = main.Eco,
                MainPlant = main.EcoCoord ?? string.Empty,
                MainPlantDesc = main.EcoCoordDesc ?? string.Empty,
                CreatedOn = ToDateOnly(main.EcoCreationDate),
                EffectivityDate = ToDateOnly(main.EcoValidFrom),
                Categ = main.EcoCateg ?? string.Empty,
                Project = main.EcoProject ?? string.Empty,
                SubLine = main.EcoSubLine ?? string.Empty,
                Urge = main.EcoUrge ?? string.Empty,
                CurrentStatus = main.EcoCurrentStatus,
                Stat1Date = ToDateOnly(main.EcoStat1Date),
                Stat2Date = ToDateOnly(main.EcoStat2Date),
                Stat3Date = ToDateOnly(main.EcoStat3Date),
                LastChange = ToDateOnly(main.EcoLastChange),
                LastChangeDiffersFromStoredClosedOn = lastChangeDiffers,
                Steps = steps
            };
        }

        /// <summary>
        /// Applies the SAP HUP snapshot to the entity. EcoTmlpseWiClose is intentionally NOT modified
        /// (historical source column TMLPSE_WI_CLOSE of DM_ECO_VIEW_STATUS_DATE is not exposed by the .NET 10 model).
        /// </summary>
        public static void Apply(Ecnecofollowup eco, EcoSapHupSnapshot snapshot, DateOnly processingDate)
        {
            ArgumentNullException.ThrowIfNull(eco);
            ArgumentNullException.ThrowIfNull(snapshot);

            eco.EcoMainPlant = snapshot.MainPlant;
            eco.EcoMainPlantDesc = snapshot.MainPlantDesc;
            eco.EcoCreatedOn = snapshot.CreatedOn;
            eco.EcoEffectivityDate = snapshot.EffectivityDate;
            eco.EcoCateg = snapshot.Categ;
            eco.EcoProject = snapshot.Project;
            eco.EcoSubLine = snapshot.SubLine;
            eco.EcoNextStep = snapshot.Steps.MainNextStep;
            eco.EcoUrge = snapshot.Urge;
            eco.EcoLastWiCreatedOn = ToDateOnly(snapshot.Steps.MainLastWiCreatedOn);
            eco.EcoNextStepSecondary = snapshot.Steps.SecondaryNextStep;
            eco.EcoLastWiCreatedOnSecondary = ToDateOnly(snapshot.Steps.SecondaryLastWiCreatedOn);
            eco.EcoWiSecondaryPlants = snapshot.Steps.SecondaryPlants;

            switch (snapshot.CurrentStatus)
            {
                case 99:
                    eco.EcoStatus = "99";
                    eco.EcoClosedOn = null;
                    eco.EcoWfStartedOn = null;
                    break;
                case 1:
                    eco.EcoStatus = "01";
                    eco.EcoWfStartedOn = snapshot.Stat1Date;
                    eco.EcoClosedOn = null;
                    break;
                case 2:
                    eco.EcoStatus = "02";
                    eco.EcoWfStartedOn = snapshot.Stat1Date;
                    eco.EcoClosedOn = snapshot.Stat2Date;
                    eco.EcoNextStep = string.Empty;
                    eco.EcoLastWiCreatedOn = null;
                    break;
                case 3:
                    eco.EcoStatus = "03";
                    eco.EcoWfStartedOn = snapshot.Stat1Date;
                    eco.EcoClosedOn = snapshot.Stat3Date;
                    eco.EcoNextStep = string.Empty;
                    eco.EcoLastWiCreatedOn = null;
                    break;
                    // Other values (null included): historical code left status and dates unchanged.
            }

            eco.SapUpdateStatus =
                (eco.EcoStatus == "02" || eco.EcoStatus == "03")
                && eco.EcoCreatedOn.HasValue
                && eco.EcoWfStartedOn.HasValue
                && eco.EcoEffectivityDate.HasValue
                && eco.EcoClosedOn.HasValue
                    ? NoUpdateRequired
                    : FormatSapUpdateStatusDate(processingDate);

            // Historical: applied after SAP_UPDATE_STATUS (ListEco02ToUpdate -> LastUpdate), now transported explicitly.
            if (snapshot.LastChangeDiffersFromStoredClosedOn)
                eco.EcoClosedOn = snapshot.LastChange;
        }
    }
}
