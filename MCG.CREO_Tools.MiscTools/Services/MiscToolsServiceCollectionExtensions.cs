using MCG.CREO_Tools.MiscTools.Interfaces;
using MCG.CREO_Tools.MiscTools.View.BomEnvirConfig;
using MCG.CREO_Tools.MiscTools.View.CadAutoColor;
using MCG.CREO_Tools.MiscTools.View.CraneSearch;
using MCG.CREO_Tools.MiscTools.ViewModel.BomEnvirConfig;
using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColr;
using MCG.CREO_Tools.MiscTools.ViewModel.CraneSearch;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.MiscTools.Services
{
    public static class MiscToolsServiceCollectionExtensions
    {
        public static IServiceCollection AddMiscToolsServices(this IServiceCollection services)
        {
            services.AddSingleton<IMiscToolsWindchillService, MiscToolsWindchillService>();

            services.AddTransient<BomEnvirConfigMainView>();
            services.AddTransient<BomEnvirConfigViewModel>();

            services.AddTransient<CadAutoColorMainView>();
            services.AddTransient<CadAutoColorViewModel>();

            services.AddTransient<CraneSearchMainView>();
            services.AddTransient<CraneSearchViewModel>();


            return services;
        }
    }
}
