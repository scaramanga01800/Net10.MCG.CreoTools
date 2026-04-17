using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    /// <summary>
    /// Logique d'interaction pour PurchaseOrderFollowUpSearchItemView.xaml
    /// </summary>
    public partial class PurchaseOrderFollowUpSearchItemView : UserControl
    {
        public event EventHandler MaxRowSearchedEvent;
        public void RaiseMaxRowSearchedEvent()
        {
            try
            {
                MaxRowSearchedEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }

        private DoubleAnimation fadeOutAnimation;

        public PurchaseOrderFollowUpSearchItemView()
        {
            try
            {
                InitializeComponent();

                fadeOutAnimation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(1)
                };

                fadeOutAnimation.Completed += (s, e) =>
                {
                    this.Visibility = Visibility.Collapsed;
                    this.BeginAnimation(UIElement.OpacityProperty, null);
                };

                DataContextChanged += PurchaseOrderFollowUpSearchItem_DataContextChanged;
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PurchaseOrderFollowUpSearchItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (DataContext != null && DataContext.GetType() == typeof(PurchaseOrderFollowUpViewModel))
                    ((PurchaseOrderFollowUpViewModel)DataContext).MaxRowSearchedEvent += PurchaseOrderFollowUpSearchItem_MaxRowSearchedEvent;

            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PurchaseOrderFollowUpSearchItem_MaxRowSearchedEvent(object sender, EventArgs e)
        {
            try
            {
                Storyboard maxRowStoryboard = (Storyboard)FindResource("MaxRowStoryboard");
                if (maxRowStoryboard != null )
                    maxRowStoryboard.Begin(MaxRowLabel);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ButtonCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                    parentWindow.Close();
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
