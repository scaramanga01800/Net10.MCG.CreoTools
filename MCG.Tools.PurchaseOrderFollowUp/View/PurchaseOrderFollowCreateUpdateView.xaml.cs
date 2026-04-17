using Fluent;
using MCG.CommonLib.Services.Statics;
using MCG.Tools.PurchaseOrderFollowUp.Configuration;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TextBox = System.Windows.Controls.TextBox;

namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    /// <summary>
    /// Logique d'interaction pour PurchaseOrderFollowCreateUpdateView.xaml
    /// </summary>
    public partial class PurchaseOrderFollowCreateUpdateView : RibbonWindow
    {

        private DoubleAnimation fadeInAnimationVendor;
        private DoubleAnimation fadeOutAnimationVendor;
        private DoubleAnimation fadeInAnimationOrder;
        private DoubleAnimation fadeOutAnimationOrder;
        private DoubleAnimation fadeInAnimationSelectRequest;
        private DoubleAnimation fadeOutAnimationSelectRequest;

        public PurchaseOrderFollowCreateUpdateView(PurchaseOrderFollowUpViewModel viewModel)
        {
            try
            {
                InitializeComponent();
                this.DataContext = viewModel;

                fadeInAnimationVendor = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(1)
                };

                fadeOutAnimationVendor = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(1)
                };

                fadeInAnimationOrder = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(1)
                };

                fadeOutAnimationOrder = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(1)
                };

                fadeInAnimationSelectRequest = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(1)
                };

                fadeOutAnimationSelectRequest = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(1)
                };

                fadeInAnimationVendor.Completed += (s, e) => SearchVendorUserControl.BeginAnimation(UIElement.OpacityProperty, null);
                fadeOutAnimationVendor.Completed += (s, e) =>
                {
                    SearchVendorUserControl.Visibility = Visibility.Collapsed;
                    SearchVendorUserControl.BeginAnimation(UIElement.OpacityProperty, null);
                };

                fadeInAnimationOrder.Completed += (s, e) => SearchOrderUserControl.BeginAnimation(UIElement.OpacityProperty, null);
                fadeOutAnimationOrder.Completed += (s, e) =>
                {
                    SearchOrderUserControl.Visibility = Visibility.Collapsed;
                    SearchOrderUserControl.BeginAnimation(UIElement.OpacityProperty, null);
                };

                fadeInAnimationSelectRequest.Completed += (s, e) => SearchOrderUserControl.BeginAnimation(UIElement.OpacityProperty, null);
                fadeOutAnimationSelectRequest.Completed += (s, e) =>
                {
                    SelectRequestType.Visibility = Visibility.Collapsed;
                    SelectRequestType.BeginAnimation(UIElement.OpacityProperty, null);
                };

                DataContextChanged += PurchaseOrderFollowCreateUpdateView_DataContextChanged;
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PurchaseOrderFollowCreateUpdateView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (DataContext != null && DataContext.GetType() == typeof(PurchaseOrderFollowUpViewModel))
                {
                    ((PurchaseOrderFollowUpViewModel)DataContext).PurgeEndRequestTypeEvent();
                    ((PurchaseOrderFollowUpViewModel)DataContext).EndRequestTypeEvent += PurchaseOrderFollowCreateUpdateView_EndRequestTypeEvent;
                    ((PurchaseOrderFollowUpViewModel)DataContext).PurgeUpdateNotAllowedEvent();
                    ((PurchaseOrderFollowUpViewModel)DataContext).UpdateNotAllowedEvent += PurchaseOrderFollowCreateUpdateView_UpdateNotAllowedEvent;
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PurchaseOrderFollowCreateUpdateView_UpdateNotAllowedEvent(object sender, EventArgs e)
        {
            try
            {
                Storyboard StBmaxRowStoryboard = (Storyboard)FindResource("UpdateNotAllowedStoryboard");
                if (StBmaxRowStoryboard != null)
                    StBmaxRowStoryboard.Begin(UpdateNotAllowed);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PurchaseOrderFollowCreateUpdateView_EndRequestTypeEvent(object sender, EventArgs e)
        {
            try
            {
                SelectRequestType.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimationSelectRequest);
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        #region [REGION] Methods for Drag and Drop
        private void MainSP_Drop(object sender, DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Collapsed;
        }

        private void MainSP_DragEnter(object sender, DragEventArgs e)
        {
            if (((PurchaseOrderFollowUpViewModel)DataContext).CurrentDataContext.CurrentRequest.IsUpdateAllowed)
                ImageDragDrop.Visibility = Visibility.Visible;
        }

        private void MainSP_DragLeave(object sender, DragEventArgs e)
        {
            ImageDragDrop.Visibility = Visibility.Collapsed;
        }
        #endregion

        private void ButtonSearchVendor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SearchVendorUserControl.Visibility == Visibility.Collapsed)
                {
                    if (SearchOrderUserControl.Visibility == Visibility.Visible)
                        SearchOrderUserControl.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimationOrder);


                    var positionTransform = ButtonSearchVendor.TransformToAncestor(PoMainWindow);
                    var position = positionTransform.Transform(new Point(0, 0));
                    var offset = new Vector(ButtonSearchVendor.ActualWidth + 2, -22);
                    var newPosition = position + offset;

                    Canvas.SetLeft(SearchVendorUserControl, newPosition.X);
                    Canvas.SetTop(SearchVendorUserControl, newPosition.Y);

                    SearchVendorUserControl.Visibility = Visibility.Visible;
                    SearchVendorUserControl.BeginAnimation(UIElement.OpacityProperty, fadeInAnimationVendor);
                }
                else
                {
                    SearchVendorUserControl.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimationVendor);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ButtonSearchOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SearchOrderUserControl.Visibility == Visibility.Collapsed)
                {
                    if (SearchVendorUserControl.Visibility == Visibility.Visible)
                        SearchVendorUserControl.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimationVendor);

                    var positionTransform = ButtonSearchOrder.TransformToAncestor(PoMainWindow);
                    var position = positionTransform.Transform(new Point(0, 0));
                    var offset = new Vector(ButtonSearchOrder.ActualWidth + 2, -22);
                    var newPosition = position + offset;

                    Canvas.SetLeft(SearchOrderUserControl, newPosition.X);
                    Canvas.SetTop(SearchOrderUserControl, newPosition.Y);

                    SearchOrderUserControl.Visibility = Visibility.Visible;
                    SearchOrderUserControl.BeginAnimation(UIElement.OpacityProperty, fadeInAnimationOrder);
                }
                else
                {
                    SearchOrderUserControl.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimationOrder);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void ButtonSearchRequestType_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectRequestType.Visibility == Visibility.Collapsed)
                {
                    if (SelectRequestType.Visibility == Visibility.Visible)
                        SelectRequestType.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimationSelectRequest);

                    var positionTransform = ButtonSearchRequestType.TransformToAncestor(PoMainWindow);
                    var position = positionTransform.Transform(new Point(0, 0));
                    var offset = new Vector(ButtonSearchRequestType.ActualWidth + 2, -22);
                    var newPosition = position + offset;

                    Canvas.SetLeft(SelectRequestType, newPosition.X);
                    Canvas.SetTop(SelectRequestType, newPosition.Y);

                    SelectRequestType.Visibility = Visibility.Visible;
                    SelectRequestType.BeginAnimation(UIElement.OpacityProperty, fadeInAnimationSelectRequest);
                }
                else
                {
                    SelectRequestType.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimationSelectRequest);
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as TextBox;
            string text = textBox.Text;
            int maxLength = PurchaseOrderFollowUpConstants.MaxSapDesignationCarac;

            // Vérifie si la longueur du texte après l'ajout du nouveau caractère dépasse la limite de caractères autorisée
            if ((text + e.Text).Length > maxLength)
            {
                e.Handled = true; // Empêche la saisie
            }
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        private void PoMainWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (!((PurchaseOrderFollowUpViewModel)DataContext).CurrentDataContext.CurrentRequest.CanBeClosedWithoutSaving
                    && (((PurchaseOrderFollowUpViewModel)DataContext).CurrentDataContext.CurrentRequest.Status == PurchaseOrderStatus.NEW
                    || ((PurchaseOrderFollowUpViewModel)DataContext).CurrentDataContext.CurrentRequest.Status == PurchaseOrderStatus.SENT))
                {
                    MessageBoxResult result = MessageBox.Show(McgMiscTools.GetStringResource("POF_WindowCloseRequestWithoutSaving"), McgMiscTools.GetStringResource("POF_TitleWindowCloseRequestWithoutSaving"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        // Cancel Closing
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

    }
}
