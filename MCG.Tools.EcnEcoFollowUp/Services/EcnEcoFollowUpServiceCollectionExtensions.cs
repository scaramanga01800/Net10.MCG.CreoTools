using MCG.CommonLib.Services.Statics;
using MCG.Tools.EcnEcoFollowUp.Interfaces;
using MCG.Tools.EcnEcoFollowUp.View;
using MCG.Tools.EcnEcoFollowUp.ViewModel;
using MCG.Tools.EcnEcoFollowUp.Services.EcoSapHup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MCG.Tools.EcnEcoFollowUp.Services
{
    public static class EcnEcoFollowUpServiceCollectionExtensions
    {
        public static IServiceCollection AddEcnEcoFollowUpServices(this IServiceCollection services)
        {
            TraceLog.StartTimer("AddEcnEcoFollowUpServices");
            
            services.AddSingleton<IEcnEcoFollowUpWindowService, EcnEcoFollowUpWindowService>();

            // Synchronisation SAP HUP ECO headless
            services.TryAddSingleton(TimeProvider.System);
            services.AddSingleton<IEcoSapHupStepCalculator, EcoSapHupStepCalculator>();
            services.AddTransient<IEcoSapHupSyncRepository, EcoSapHupSyncRepository>();
            services.AddTransient<IEcoSapHupSyncService, EcoSapHupSyncService>();

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
