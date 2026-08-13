using MCG.CommonLib.Services.Statics;
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
            TraceLog.StartTimer("AddCutLengthServices");

            services.AddSingleton<ICutLengthWindchillService, CutLengthWindchillService>();

            services.AddTransient<CutLengthBulkQuantity>();

            services.AddTransient<CutLengthCutUpdatePartView>();
            services.AddTransient<CutLengthCutUpdatePartViewModel>();

            services.AddTransient<CutLengthViewModel>();

            TraceLog.StopTimer("AddCutLengthServices");
            return services;
        }
    }
}
