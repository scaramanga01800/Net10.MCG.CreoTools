using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.JpgExport.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.JpgExport.Services
{
    public static class JpgExportServiceCollectionExtensions
    {
        public static IServiceCollection AddJpgExportServices(this IServiceCollection services)
        {
            TraceLog.StartTimer("AddJpgExportServices");

            services.AddSingleton<JpgExportViewModel>();

            TraceLog.StopTimer("AddJpgExportServices");
            return services;
        }
    }
}
