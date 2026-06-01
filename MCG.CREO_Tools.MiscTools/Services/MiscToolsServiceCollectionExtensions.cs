using MCG.CREO_Tools.MiscTools.ViewModel.BomComparison;
using MCG.CREO_Tools.MiscTools.Interfaces;
using MCG.CREO_Tools.MiscTools.View.BomComparison;
using MCG.CREO_Tools.MiscTools.View.BomEnvirConfig;
using MCG.CREO_Tools.MiscTools.View.CadAutoColor;
using MCG.CREO_Tools.MiscTools.View.CadDocRename;
using MCG.CREO_Tools.MiscTools.View.CraneSearch;
using MCG.CREO_Tools.MiscTools.View.MechanismTool;
using MCG.CREO_Tools.MiscTools.View.NumberCumulation;
using MCG.CREO_Tools.MiscTools.View.QuickChange;
using MCG.CREO_Tools.MiscTools.View.SapBomExport;
using MCG.CREO_Tools.MiscTools.View.SapBomExportAllLevel;
using MCG.CREO_Tools.MiscTools.View.SapFertBom;
using MCG.CREO_Tools.MiscTools.View.WebtermRequest;
using MCG.CREO_Tools.MiscTools.ViewModel.BomEnvirConfig;
using MCG.CREO_Tools.MiscTools.ViewModel.CadAutoColr;
using MCG.CREO_Tools.MiscTools.ViewModel.CadDocRename;
using MCG.CREO_Tools.MiscTools.ViewModel.CraneSearch;
using MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool;
using MCG.CREO_Tools.MiscTools.ViewModel.NumberCumulation;
using MCG.CREO_Tools.MiscTools.ViewModel.QuickChange;
using MCG.CREO_Tools.MiscTools.ViewModel.SapBomExport;
using MCG.CREO_Tools.MiscTools.ViewModel.SapBomExportAllLevel;
using MCG.CREO_Tools.MiscTools.ViewModel.SapFertBom;
using MCG.CREO_Tools.MiscTools.ViewModel.WebtermRequest;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.CREO_Tools.MiscTools.Services
{
    public static class MiscToolsServiceCollectionExtensions
    {
        public static IServiceCollection AddMiscToolsServices(this IServiceCollection services)
        {
            services.AddSingleton<IMiscToolsWindchillService, MiscToolsWindchillService>();

            services.AddTransient<BomComparisonView>();
            services.AddTransient<BomComparisonViewModel>();
            services.AddTransient<BomComparisonTabMainView>();

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

            services.AddTransient<QuickChangeMainView>();
            services.AddTransient<QuickChangeViewModel>();

            services.AddTransient<SapBomExportMainView>();
            services.AddTransient<SapBomExportViewModel>();

            services.AddTransient<SapBomExportAllLevelMainView>();
            services.AddTransient<SapBomExportAllLevelViewModel>();

            services.AddTransient<SapFertMissingPart>();

            services.AddTransient<SapFertBomMainView>();
            services.AddTransient<SapFertBomViewModel>();

            services.AddTransient<WebtermRequestMainView>();
            services.AddTransient<WebtermRequestViewModel>();

            return services;
        }
    }
}
