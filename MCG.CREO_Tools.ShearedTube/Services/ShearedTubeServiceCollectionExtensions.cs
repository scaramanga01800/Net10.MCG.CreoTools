
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.ShearedTube.View;
using MCG.CREO_Tools.ShearedTube.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.ShearedTube.Services
{
    public static class ShearedTubeServiceCollectionExtensions
    {

        public static IServiceCollection AddShearedTubeServices(this IServiceCollection services)
        {
            TraceLog.StartTimer("AddShearedTubeServices");

            services.AddSingleton<ShearedTubeFluentTabMainView>();
            services.AddTransient<ShearedTubeViewModel>();
            
            TraceLog.StopTimer("AddShearedTubeServices");
            return services;
        }
    }
}
