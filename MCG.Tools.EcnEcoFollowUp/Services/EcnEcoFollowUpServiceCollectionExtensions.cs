using MCG.CommonLib.Services.Statics;
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
            TraceLog.StartTimer("AddEcnEcoFollowUpServices");
            
            services.AddSingleton<IEcnEcoFollowUpWindowService, EcnEcoFollowUpWindowService>();

            services.AddTransient<EcnEcaWorkFlowTasksViewModel>();

            services.AddTransient<EcnEcoFollowUpDashboardSearchWindow>();
            services.AddTransient<EcnEcoFollowUpDashboardSearchWindowViewModel>();

            services.AddTransient<EcnEcoFollowUpDashboardViewModel>();
            services.AddTransient<EcnEcoFollowUpDashboardView>();

            services.AddSingleton<EcnEcoFollowUpFluentTabView>();
            services.AddSingleton<EcnEcoFollowUpViewModel>();
            
            services.AddTransient<EcoWorkFlowTasksViewModel>();

            services.AddTransient<EcnEcaWorkFlowTasksView>();
            services.AddTransient<EcoWorkFlowTasksView>();
            
            TraceLog.StopTimer("AddEcnEcoFollowUpServices");
            return services;
        }
    }
}
