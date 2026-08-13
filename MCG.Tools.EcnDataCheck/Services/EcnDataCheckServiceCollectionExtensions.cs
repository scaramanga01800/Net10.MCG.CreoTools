using MCG.CommonLib.Services.Statics;
using MCG.Tools.EcnDataCheck.Interfaces;
using MCG.Tools.EcnDataCheck.View;
using MCG.Tools.EcnDataCheck.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.Tools.EcnDataCheck.Services
{
    public static class EcnDataCheckServiceCollectionExtensions
    {
        public static IServiceCollection AddEcnDataCheckServices(this IServiceCollection services)
        {
            TraceLog.StartTimer("AddEcnDataCheckServices");

            services.AddSingleton<IEcnDataCheckWindchillService, EcnDataCheckWindchillService>();

            services.AddSingleton<EcnDataCheckViewModel>();

            services.AddTransient<EcnDataCheckEcaSelection>();

            // Register your services here
            TraceLog.StopTimer("AddEcnDataCheckServices");
            return services;
        }
    }
}
