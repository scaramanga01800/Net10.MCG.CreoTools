
using MCG.CREO_Tools.ShearedTube.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.ShearedTube.Services
{
    public static class ShearedTubeServiceCollectionExtensions
    {

        public static IServiceCollection AddShearedTubeServices(this IServiceCollection services)
        {
            services.AddTransient<ShearedTubeViewModel>();

            return services;
        }
    }
}
