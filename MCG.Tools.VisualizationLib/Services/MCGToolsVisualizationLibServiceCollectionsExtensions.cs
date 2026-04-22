using MCG.Tools.VisualizationLib.Interfaces;
using MCG.Tools.VisualizationLib.View;
using MCG.Tools.VisualizationLib.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.Tools.VisualizationLib.Services
{
    public static class MCGToolsVisualizationLibServiceCollectionsExtensions
    {
        public static IServiceCollection AddMCGToolsVisualizationLibServices(this IServiceCollection services)
        {
            services.AddSingleton<VisualizationUpdateService>();

            services.AddSingleton<IWtDownloadViewableTools, WtDownloadViewableTools>();

            services.AddTransient<IMcgToolsVisualizationLibWindowService, McgToolsVisualizationLibWindowService>();

            services.AddSingleton<ConvertToPdfDataContext>();
            services.AddSingleton<ConvertToPdfViewModel>();

            services.AddTransient<ConvertToPdfMergeWindowView>();
            services.AddTransient<ConvertToPdfMergeWindowViewModel>();

            services.AddSingleton<DownloadVisualizationFileViewModel>();
            services.AddSingleton<DownloadVisualizationFileDataContext>();

            // On retourne la collection pour permettre le chaînage (fluent pattern)
            return services;
        }
    }
}
