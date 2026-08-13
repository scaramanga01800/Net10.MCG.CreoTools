using MCG.CommonLib.Services.Statics;
using MCG.Tools.PurchaseOrderFollowUp.Interfaces;
using MCG.Tools.PurchaseOrderFollowUp.View;
using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace MCG.Tools.PurchaseOrderFollowUp.Services
{
    public static class PurchaseOrderFollowUpServiceCollectionExtensions
    {
        public static IServiceCollection AddPurchaseOrderFollowUpServices(this IServiceCollection services)
        {
            TraceLog.StartTimer("AddPurchaseOrderFollowUpServices");
            // Services pour l'outil de suivi des commandes d'achat
            services.AddSingleton<PurchaseOrderFollowUpViewModel>();
            services.AddTransient<PurchaseOrderColumnHeaderSearchViewModel>();

            services.AddTransient<IPurchaseOrderFollowWindowService, PurchaseOrderFollowWindowService>();

            services.AddTransient<PurchaseOrderFollowCreateUpdateView>();
            services.AddTransient<PurchaseOrderFollowListRequestView>();
            services.AddTransient<PurchaseOrderFollowUpCreateUpdateVendorView>();
            services.AddTransient<PurchaseOrderFollowUpDuplicate>();
            services.AddTransient<PurchaseOrderFollowUpExtendedPartView>();
            services.AddTransient<PurchaseOrderFollowUpInternalOrderRequestView>();
            services.AddTransient<PurchaseOrderFollowUpSelectVendorView>();
            
            TraceLog.StopTimer("AddPurchaseOrderFollowUpServices");
            return services;
        }
    }
}
