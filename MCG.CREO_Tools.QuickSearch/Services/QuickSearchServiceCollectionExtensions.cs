using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.QuickSearch.Interfaces;
using MCG.CREO_Tools.QuickSearch.View;
using MCG.CREO_Tools.QuickSearch.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.QuickSearch.Services
{
    public static class QuickSearchServiceCollectionExtensions
    {
        public static IServiceCollection AddQuickSearchServices(this IServiceCollection services)
        {
            TraceLog.StartTimer("AddQuickSearchServices");

            services.AddSingleton<IQuickSearchWindchillService, QuickSearchWindchillService>();

            services.AddSingleton<QuickSearchFluentRibbonTabView>();
            services.AddTransient<QuickSearchViewModel>();

            services.AddTransient<QuickSearchUpdatePartView>();
            services.AddTransient<QuickSearchUpdatePartViewModel>();
            
            services.AddTransient<QuickSearchWindowClassSubClassFromNumberView>();
            services.AddTransient<QuickSearchWindowClassSubClassFromNumberViewModel>();

            services.AddTransient<QuickSearchWindowRefDocFromNumberView>();
            services.AddTransient<QuickSearchWindowRefDocFromNumberViewModel>();
            
            TraceLog.StopTimer("AddQuickSearchServices");
            return services;
        }
    }
}

