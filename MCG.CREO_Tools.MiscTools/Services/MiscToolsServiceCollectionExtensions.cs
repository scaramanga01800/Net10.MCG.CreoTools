using MCG.CREO_Tools.MiscTools.Interfaces;
using MCG.CREO_Tools.MiscTools.View.BomEnvirConfig;
using MCG.CREO_Tools.MiscTools.View.CadAutoColor;
using MCG.CREO_Tools.MiscTools.View.CadDocRename;
using MCG.CREO_Tools.MiscTools.View.CraneSearch;
using MCG.CREO_Tools.MiscTools.View.MechanismTool;
using MCG.CREO_Tools.MiscTools.View.NumberCumulation;
using MCG.CREO_Tools.MiscTools.ViewModel.BomEnvirConfig;
using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColr;
using MCG.CREO_Tools.MiscTools.ViewModel.CadDocRename;
using MCG.CREO_Tools.MiscTools.ViewModel.CraneSearch;
using MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool;
using MCG.CREO_Tools.MiscTools.ViewModel.NumberCumulation;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.MiscTools.Services
{
    public static class MiscToolsServiceCollectionExtensions
    {
        public static IServiceCollection AddMiscToolsServices(this IServiceCollection services)
        {
            services.AddSingleton<IMiscToolsWindchillService, MiscToolsWindchillService>();

            services.AddTransient<BomEnvirConfigMainView>();
            services.AddTransient<BomEnvirConfigViewModel>();

            services.AddTransient<CadAutoColorMainView>();
            services.AddTransient<CadAutoColorViewModel>();

            services.AddTransient<CadDocRenameMainView>();
            services.AddTransient<CadDocRenameViewModel>();

            services.AddTransient<CraneSearchMainView>();
            services.AddTransient<CraneSearchViewModel>();

            services.AddTransient<MechanismAnalysisMainView>();
            services.AddTransient<MechanismAnalysisViewModel>();

            services.AddTransient<NumberCumulationMainView>();
            services.AddTransient<NumberCumulationViewModel>();

            return services;
        }
    }
}
