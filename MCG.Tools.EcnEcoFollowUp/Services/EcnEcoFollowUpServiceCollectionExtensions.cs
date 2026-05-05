using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.View;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.Tools.EcnEcoFollowUp.Services
{
    public static class EcnEcoFollowUpServiceCollectionExtensions
    {
        public static IServiceCollection AddEcnEcoFollowUpServices(this IServiceCollection services)
        {
            services.AddSingleton<IEcnEcoFollowUpWindowService, EcnEcoFollowUpWindowService>();

            services.AddTransient<EcnEcaWorkFlowTasksViewModel>();
            services.AddTransient<EcnEcoFollowUpDashboardSearchWindowViewModel>();
            services.AddTransient<EcnEcoFollowUpDashboardViewModel>();

            services.AddTransient<EcnEcoFollowUpViewModel>();
            
            services.AddTransient<EcoWorkFlowTasksViewModel>();

            services.AddTransient<EcnEcaWorkFlowTasksView>();
            services.AddTransient<EcoWorkFlowTasksView>();

            return services;
        }
    }
}
