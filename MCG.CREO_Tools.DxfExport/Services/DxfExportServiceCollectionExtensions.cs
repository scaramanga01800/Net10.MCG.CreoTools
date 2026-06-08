using MCG.CREO_Tools.DxfExport.Interfaces;
using MCG.CREO_Tools.DxfExport.View;
using MCG.CREO_Tools.DxfExport.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.DxfExport.Services
{
    public static class DxfExportServiceCollectionExtensions
    {
        public static IServiceCollection AddDxfExportServices(this IServiceCollection services)
        {
            services.AddSingleton<IDxfExportWindchillService, DxfExportWindchillService>();
            
            services.AddTransient<BackUpCadDocumentView>();
            services.AddTransient<BackUpCadDocumentViewModel>();

            services.AddTransient<DxfDwgDrawingExportMainView>();
            services.AddTransient<DxfDwgDrawingExportViewModel>();

            services.AddTransient<DxfExportViewModel>();

            return services;
        }
    }
}