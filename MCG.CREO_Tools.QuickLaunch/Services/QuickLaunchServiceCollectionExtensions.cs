using MCG.CREO_Tools.QuickLaunch.View;
using MCG.CREO_Tools.QuickLaunch.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.QuickLaunch.Services
{
    public static class QuickLaunchServiceCollectionExtensions
    {
        public static IServiceCollection AddQuickLaunchServices(this IServiceCollection services)
        {
            services.AddTransient<QuickLaunchViewModel>();

            return services;
        }
    }
}
