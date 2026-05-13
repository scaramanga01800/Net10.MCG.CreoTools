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
            services.AddSingleton<IEcnDataCheckWindchillService, EcnDataCheckWindchillService>();


            services.AddSingleton<EcnDataCheckViewModel>();

            services.AddTransient<EcnDataCheckEcaSelection>();

            // Register your services here
            return services;
        }
    }
}
