using CommunityToolkit.Mvvm.Messaging;
using Fluent;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.NumberingTool.Exceptions;
using MCG.Tools.NumberingTool.Messages;
using MCG.Tools.NumberingTool.ViewModel;

namespace MCG.Tools.NumberingTool.View
{
    public partial class NumberingToolUpdateCreateFluentView : RibbonWindow
    {
        public NumberingToolUpdateCreateViewModel CurrentDataContext { get; set; }

        #region [REGION] Events
        public event EventHandler CreateNumberEvent;
        public void RaiseCreateNumberEvent()
        {
            try
            {
                CreateNumberEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        public event EventHandler UpdateNumberEvent;
        public void RaiseUpdateNumberEvent()
        {
            try
            {
                UpdateNumberEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
        #endregion

        public NumberingToolUpdateCreateFluentView(NumberingToolUpdateCreateViewModel viewModel)
        {
            try
            {
                TraceLog.AddTraceLog($"Enter NumberingToolUpdateCreateView");
                CurrentDataContext = viewModel;
                CurrentDataContext.CreateNumberEvent += CreateNumber_End;
                CurrentDataContext.UpdateNumberEvent += UpdateNumber_End;
                this.DataContext = CurrentDataContext;
                InitializeComponent();
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void SetNumberingToolUpdateCreateFluentViewProperties(bool IsNewNumber, NumberingToolTemplate SelectedNumberingTemplate, List<string> SearchProductList, List<string> ListFormat, NumberingToolItem CurrentItem = null, bool IsDetailShown = true)
        {
            try
            {
                CurrentDataContext.SetNumberingToolUpdateCreateViewModelProperties(IsNewNumber, SelectedNumberingTemplate, SearchProductList, ListFormat, CurrentItem, IsDetailShown);
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void CreateNumber_End(object sender, EventArgs e)
        {
            try
            {
                Close();
                RaiseCreateNumberEvent();
                WeakReferenceMessenger.Default.Send(new NumberCreatedMessage() { Template = CurrentDataContext.SelectedNumberingTemplate, Item = CurrentDataContext.CurrentItem });
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateNumber_End(object sender, EventArgs e)
        {
            try
            {
                Close();
                RaiseUpdateNumberEvent();
                WeakReferenceMessenger.Default.Send(new NumberUpdatedMessage() { Item = CurrentDataContext.CurrentItem });
            }
            catch (Exception ex)
            {
                NumberingToolException.SendMessageBox(this.GetType().Name, ex);
            }
        }

    }
}
