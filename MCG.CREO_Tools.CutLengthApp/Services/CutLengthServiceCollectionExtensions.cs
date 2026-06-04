using MCG.CREO_Tools.CutLengthApp.Interfaces;
using MCG.CREO_Tools.CutLengthApp.View;
using MCG.CREO_Tools.CutLengthApp.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.CutLengthApp.Services
{
    public static class CutLengthServiceCollectionExtensions
    {
        public static IServiceCollection AddCutLengthServices(this IServiceCollection services)
        {
            services.AddSingleton<ICutLengthWindchillService, CutLengthWindchillService>();

            services.AddTransient<CutLengthBulkQuantity>();

            services.AddTransient<CutLengthCutUpdatePartView>();
            services.AddTransient<CutLengthCutUpdatePartViewModel>();

            services.AddTransient<CutLengthViewModel>();

            return services;
        }
    }
}
