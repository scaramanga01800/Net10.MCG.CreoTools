using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    public partial class PurchaseOrderFollowSelectRequestTypeView : UserControl
    {
        public PurchaseOrderFollowSelectRequestTypeView()
        {
            try
            {
                InitializeComponent();
                DataContextChanged += PurchaseOrderFollowSelectRequestTypeView_DataContextChanged;
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PurchaseOrderFollowSelectRequestTypeView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (DataContext != null && DataContext.GetType() == typeof(PurchaseOrderFollowUpViewModel))
                {
                    ((PurchaseOrderFollowUpViewModel)DataContext).PurgeStartRequestTypeEvent();
                    ((PurchaseOrderFollowUpViewModel)DataContext).PurgeChangeRequestTypeQuestionEvent();

                    ((PurchaseOrderFollowUpViewModel)DataContext).StartRequestTypeEvent += PurchaseOrderFollowSelectRequestTypeView_StartRequestTypeEvent;
                    ((PurchaseOrderFollowUpViewModel)DataContext).ChangeRequestTypeQuestionEvent += PurchaseOrderFollowSelectRequestTypeView_ChangeRequestTypeQuestionEvent;
                }

            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

                    private bool IsRaiseUpdateRequestTypeEventDone { get; set; }
        private void PurchaseOrderFollowSelectRequestTypeView_ChangeRequestTypeQuestionEvent(object sender, EventArgs e)
        {
            try
            {
                Storyboard fadeOutStoryboard = (Storyboard)MyGrid.FindResource("fadeOutStoryboard");
                Storyboard fadeInStoryboard = (Storyboard)MyGrid.FindResource("fadeInStoryboard");

                if (fadeOutStoryboard != null && fadeInStoryboard != null)
                {
                    IsRaiseUpdateRequestTypeEventDone = false;


                    fadeOutStoryboard.Completed += (s, args) =>
                    {
                        if (!IsRaiseUpdateRequestTypeEventDone) 
                          ((PurchaseOrderFollowUpViewModel)DataContext).RaiseUpdateRequestTypeEvent();
                        fadeInStoryboard.Begin(MyGrid);
                        IsRaiseUpdateRequestTypeEventDone=true;
                    };
                    fadeOutStoryboard.Begin(MyGrid);

                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PurchaseOrderFollowSelectRequestTypeView_StartRequestTypeEvent(object sender, EventArgs e)
        {
            try
            {
                Storyboard fadeInStoryboard = (Storyboard)MyGrid.FindResource("fadeInStoryboard");
                if (fadeInStoryboard != null)
                    fadeInStoryboard.Begin(MyGrid);

            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            //Storyboard fadeOutStoryboard = (Storyboard)MyGrid.FindResource("fadeOutStoryboard");
            //Storyboard fadeInStoryboard = (Storyboard)MyGrid.FindResource("fadeInStoryboard");

            //if (fadeOutStoryboard != null && fadeInStoryboard != null)
            //{
            //    fadeOutStoryboard.Completed += (s, args) =>
            //    {
            //        fadeInStoryboard.Begin(MyGrid);
            //    };

            //    fadeOutStoryboard.Begin(MyGrid);
            //}
        }
    }
}

