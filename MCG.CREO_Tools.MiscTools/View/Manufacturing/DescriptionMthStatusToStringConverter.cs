using MCG.CommonLib.Services.Statics;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MCG.CREO_Tools.MiscTools.View.Manufacturing
{
    /// <summary>
    /// Traduit <see cref="ViewModel.Manufacturing.DescriptionMthStatus"/> en libelle localise,
    /// via les ressources deja fusionnees de <c>CREOToolsMiscToolsDictionary</c>.
    /// </summary>
    public sealed class DescriptionMthStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ViewModel.Manufacturing.DescriptionMthStatus status)
                return string.Empty;

            var resourceKey = status switch
            {
                ViewModel.Manufacturing.DescriptionMthStatus.Unchanged => "MFG_Status_Unchanged",
                ViewModel.Manufacturing.DescriptionMthStatus.Calculated => "MFG_Status_Calculated",
                ViewModel.Manufacturing.DescriptionMthStatus.ManuallyModified => "MFG_Status_ManuallyModified",
                ViewModel.Manufacturing.DescriptionMthStatus.UpdateRequired => "MFG_Status_UpdateRequired",
                ViewModel.Manufacturing.DescriptionMthStatus.CalculationImpossible => "MFG_Status_CalculationImpossible",
                ViewModel.Manufacturing.DescriptionMthStatus.WebtermError => "MFG_Status_WebtermError",
                _ => null
            };

            return resourceKey != null ? McgWpfTools.GetStringResource(resourceKey) : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
