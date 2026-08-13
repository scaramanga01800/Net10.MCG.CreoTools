using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.WindchillTools.ManageWTObject.Configuration;
using MCG.WindchillTools.ManageWTObject.Exceptions;
using MCG.WindchillTools.ManageWTObject.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MCG.WindchillTools.ManageWTObject.View
{
    public partial class MassWtDocumentUpdateRibbonView : RibbonTabItem
    {

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

        private bool IsAlreadyInit { get; set; } = false;
       
        public MassWtDocumentUpdateRibbonView()
        {
            try
            {
                TraceLog.AddTraceLog("Create MassWtDocumentUpdateRibbonView");
                string MainAppFolder = Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                TraceLog.AddTraceLog($"MassWtDocumentUpdateRibbonView: Local App Directory {MainAppFolder}");

                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{ManageWTObjectConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();

                DataContextChanged += MassWtDocumentUpdateRibbonView_DataContextChanged;
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void MassWtDocumentUpdateRibbonView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAlreadyInit && DataContext.GetType() == typeof(MassWtDocumentUpdateViewModel))
                {
                    ((MassWtDocumentUpdateViewModel)DataContext).ActionDoneEvent += RaiseActionDoneEvent;
                    ((MassWtDocumentUpdateViewModel)DataContext).ActionInProgressEvent += RaiseActionInProgressEvent;
                    IsAlreadyInit = true;
                }
            }
            catch (Exception ex)
            {
                ManageWTObjectException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void BorderDragXls_Drop(object sender, DragEventArgs e)
        {
            if (sender.GetType() == typeof(Border))
            {
                ((Border)sender).Opacity = 0.5;
            }
        }

        private void BorderDragXls_DragEnter(object sender, DragEventArgs e)
        {
            if (sender.GetType() == typeof(Border))
            {
                ((Border)sender).Opacity = 1;
            }
        }

        private void BorderDragXls_DragLeave(object sender, DragEventArgs e)
        {
            if (sender.GetType() == typeof(Border))
            {
                ((Border)sender).Opacity = 0.5;
            }
        }

        private void BorderDragSecondary_Drop(object sender, DragEventArgs e)
        {
            if (sender.GetType() == typeof(Border))
            {
                ((Border)sender).Opacity = 0.5;
            }
        }

        private void BorderDragSecondary_DragEnter(object sender, DragEventArgs e)
        {
            if (sender.GetType() == typeof(Border))
            {
                ((Border)sender).Opacity = 1;
            }
        }

        private void BorderDragSecondary_DragLeave(object sender, DragEventArgs e)
        {
            if (sender.GetType() == typeof(Border))
            {
                ((Border)sender).Opacity = 0.5;
            }
        }
    }
}
