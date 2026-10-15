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

            services.AddEcnEcoFollowUpBatchServices();

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

        /// <summary>
        /// Headless ECN/ECO batch services only (no View/ViewModel/Window). Infrastructure
        /// (CreoToolsDbContext, ISapHupService, IWindchillBatchCredentialProvider, Windchill request services)
        /// must be registered by the host via AddMcgDataBaseAccessServices and AddMCGWindchillRequestToolServices.
        /// </summary>
        public static IServiceCollection AddEcnEcoFollowUpBatchServices(this IServiceCollection services)
        {
            // Synchronisation SAP HUP ECO headless
            services.TryAddSingleton(TimeProvider.System);
            services.AddSingleton<IEcoSapHupStepCalculator, EcoSapHupStepCalculator>();
            services.AddTransient<IEcoSapHupSyncRepository, EcoSapHupSyncRepository>();
            services.AddTransient<IEcoSapHupSyncService, EcoSapHupSyncService>();

            // Calcul pur des dates workflow ECN (ExtractPdmWfTaskDate), sans état
            services.AddSingleton<IEcnWorkflowDateCalculator, Workflow.EcnWorkflowDateCalculator>();

            // Synchronisation PDM context headless (BatchModeUpdatePdmContext)
            services.AddTransient<IPdmContextSyncRepository, PdmContext.PdmContextSyncRepository>();
            services.AddTransient<IPdmContextSyncService, PdmContext.PdmContextSyncService>();

            // Synchronisation ECN headless (BatchModeAddAllNewEcn + BatchModeUpdateEcnWfDate)
            services.AddTransient<IEcnFollowUpSyncRepository, EcnFollowUp.EcnFollowUpSyncRepository>();
            services.AddTransient<IEcnFollowUpSyncService, EcnFollowUp.EcnFollowUpSyncService>();

            // Orchestrateur ECN/ECO headless (PdmContext -> EcnFollowUp -> EcoSapHup)
            services.AddTransient<IEcnEcoFollowUpBatchService, EcnEcoBatch.EcnEcoFollowUpBatchService>();

            return services;
        }
    }
}
