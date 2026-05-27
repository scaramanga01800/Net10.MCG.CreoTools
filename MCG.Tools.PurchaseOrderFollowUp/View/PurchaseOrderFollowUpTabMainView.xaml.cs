using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.DataBaseAccess.Interfaces;
using MCG.CommonLib.SapTools.Interfaces;
using MCG.CommonLib.Services.Interfaces;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.PurchaseOrderFollowUp.Configuration;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.Interfaces;
using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System.Windows;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    /// <summary>
    /// Logique d'interaction pour PurchaseOrderFollowUpTabMainView.xaml
    /// </summary>
    public partial class PurchaseOrderFollowUpTabMainView : RibbonTabItem, IMcgToolApp
    {
        private ISapPurchasingService _sapPurchasingService;
        private ISapHupService _sapHupService;
        private IUserAuthorizationService _userAuthorizationService;
        private IPurchaseOrderService _purchaseOrderService;
        private IOracleMiscTools _oracleMiscTools;
        private IXmlSerializeTools _xmlSerializeTools;
        private IPurchaseOrderFollowWindowService _purchaseOrderFollowWindowService;
        private bool IsAlreadyInit { get; set; } = false;

        #region [REGION] Events Action
        public event EventHandler ActionInProgressEvent;
        public void RaiseActionInProgressEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                ActionInProgressEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler ActionDoneEvent;
        public void RaiseActionDoneEvent(object sender = null, EventArgs e = null)
        {
            try
            {
                ActionDoneEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        private bool IsAppAlreadyInit { get; set; } = false;
        private PurchaseOrderFollowUpViewModel CurrentDataContext { get; set; }

        public PurchaseOrderFollowUpTabMainView()
        {
            try
            {
                string MainAppFolder = Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);

                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;
                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{PurchaseOrderFollowUpConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();

                DataContextChanged += PurchaseOrderFollowUpTabMainView_DataContextChanged;
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PurchaseOrderFollowUpTabMainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsAlreadyInit && DataContext.GetType() == typeof(PurchaseOrderFollowUpViewModel))
            {
                CurrentDataContext = (PurchaseOrderFollowUpViewModel)DataContext;
                CurrentDataContext.ActionInProgressEvent += RaiseActionInProgressEvent;
                CurrentDataContext.ActionDoneEvent += RaiseActionDoneEvent;
                IsAlreadyInit=true;
            }
        }

        public void InitApp()
        {
            throw new NotImplementedException();
        }

    }
}
