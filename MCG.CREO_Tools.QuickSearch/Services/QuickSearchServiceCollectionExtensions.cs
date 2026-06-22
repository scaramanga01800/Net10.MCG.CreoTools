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
            services.AddSingleton<IQuickSearchWindchillService, QuickSearchWindchillService>();

            services.AddTransient<QuickSearchViewModel>();

            services.AddTransient<QuickSearchUpdatePartView>();
            services.AddTransient<QuickSearchUpdatePartViewModel>();

            services.AddTransient<QuickSearchWindowClassSubClassFromNumberView>();
            services.AddTransient<QuickSearchWindowClassSubClassFromNumberViewModel>();

            services.AddTransient<QuickSearchWindowRefDocFromNumberViewModel>();

            return services;
        }
    }
}

