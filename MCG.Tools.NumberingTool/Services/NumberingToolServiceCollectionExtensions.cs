using MCG.Tools.NumberingTool.Interfaces;
using MCG.Tools.NumberingTool.View;
using MCG.Tools.NumberingTool.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.Tools.NumberingTool.Services
{
    public static class NumberingToolServiceCollectionExtensions
    {
        public static IServiceCollection AddNumberingToolServices(this IServiceCollection services)
        {
            services.AddSingleton<INumberingToolWindowService, NumberingToolWindowService>();

            services.AddTransient<NumberingToolUpdateCreateViewModel>();
            services.AddTransient<NumberingToolUpdateCreateFluentView>();

            services.AddTransient<NumberingToolCreateSeveralFluentView>();

            services.AddTransient<NumberingToolViewModel>();
            services.AddTransient<NumberingToolFluentMainView>();

            return services;
        }
    }
}
