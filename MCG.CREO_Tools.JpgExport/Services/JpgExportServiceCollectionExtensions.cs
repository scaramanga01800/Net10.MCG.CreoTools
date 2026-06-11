using MCG.CREO_Tools.JpgExport.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.JpgExport.Services
{
    public static class JpgExportServiceCollectionExtensions
    {
        public static IServiceCollection AddJpgExportServices(this IServiceCollection services)
        {
            services.AddSingleton<JpgExportViewModel>();
            return services;
        }
    }
}
