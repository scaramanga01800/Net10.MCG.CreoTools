using MCG.CREO_Tools.ProfileApp.Interfaces;
using MCG.CREO_Tools.ProfileApp.View;
using MCG.CREO_Tools.ProfileApp.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.ProfileApp.Services
{
    public static class ProfileAppServiceCollectionExtensions
    {
        public static IServiceCollection AddProfileAppServices(this IServiceCollection services)
        {
            services.AddSingleton<IProfileAppWindowService, ProfileAppWindowService>();
            services.AddTransient<ProfileUpdateProfileView>();

            services.AddTransient<ProfileViewModel>();

            return services;
        }
    }
}
