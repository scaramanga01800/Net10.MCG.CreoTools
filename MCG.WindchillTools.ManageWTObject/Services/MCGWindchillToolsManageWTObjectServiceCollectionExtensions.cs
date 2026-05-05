using MCG.WindchillTools.ManageWTObject.Interfaces;
using MCG.WindchillTools.ManageWTObject.View;
using MCG.WindchillTools.ManageWTObject.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.WindchillTools.ManageWTObject.Services
{
    public static class MCGWindchillToolsManageWTObjectServiceCollectionExtensions
    {
        public static IServiceCollection AddMCGWindchillToolsManageWTObjectServices(this IServiceCollection services)
        {
            services.AddSingleton<IMcgWindchillToolsManageWTObjectWindowService, McgWindchillToolsManageWTObjectWindowService>();

            services.AddTransient<SearchWtDocumentPartView>();

            services.AddTransient<CreateWtDocumentMainView>();
            services.AddTransient<CreateWtDocumentViewModel>();

            services.AddTransient<CreateUpdateWtDocumentWtPartMainView>();
            services.AddTransient<CreateUpdateWtDocumentWtPartViewModel>();

            services.AddTransient<MassWtDocumentUpdateViewModel>();
            // On retourne la collection pour permettre le chaînage (fluent pattern)
            return services;
        }
    }
}
