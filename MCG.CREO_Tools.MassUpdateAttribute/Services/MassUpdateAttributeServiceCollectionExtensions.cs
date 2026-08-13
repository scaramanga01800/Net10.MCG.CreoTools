using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MassUpdateAttribute.Interfaces;
using MCG.CREO_Tools.MassUpdateAttribute.View;
using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.MassUpdateAttribute.Services
{
    public static class MassUpdateAttributeServiceCollectionExtensions
    {
        public static IServiceCollection AddMassUpdateAttributeServices(this IServiceCollection services)
        {
            TraceLog.StartTimer("AddMassUpdateAttributeServices");

            services.AddSingleton<IMassUpdateAttributeWindowService, MassUpdateAttributeWindowService>();

            services.AddTransient<CreateNewCadDocumentFluentWindow>();
            services.AddTransient<CreateNewCadDocumentViewModel>();

            services.AddTransient<MassUpdateAttributeChangeName>();
            services.AddTransient<MassUpdateAttributeViewModel>();

            services.AddTransient<UpdateRelationsParametersMainView>();
            services.AddTransient<UpdateRelationsParametersViewModel>();
            
            TraceLog.StopTimer("AddMassUpdateAttributeServices");
            return services;
        }
    }
}
