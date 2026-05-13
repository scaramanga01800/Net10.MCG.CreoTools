using MCG.CommonLib.Services.Statics;
using MCG.Tools.EcnDataCheck.Exceptions;
using MCG.Tools.EcnDataCheck.Models;
using MCG.Tools.EcnDataCheck.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MCG.Tools.EcnDataCheck.View
{
    public partial class EcnDataCheckTabContentView : UserControl
    {
        #region [REGION] Internal variables
        private bool IsAppAlreadyInit { get; set; } = false;
        private bool IsMouseInRowDetail { get; set; } = false;
        public EcnDataCheckViewModel CurrentDataContext { get; set; }
        #endregion

        public EcnDataCheckTabContentView()
        {
            try
            {
                InitializeComponent();
                DataContextChanged += EcnDataCheckTabContentView_DataContextChanged;    
            }
            catch (Exception ex)
            {
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void EcnDataCheckTabContentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAppAlreadyInit && DataContext != null && DataContext.GetType() == typeof(EcnDataCheckViewModel))
                {
                    CurrentDataContext = ((EcnDataCheckViewModel)DataContext);
                    IsAppAlreadyInit = true;
                }
            }
            catch (Exception ex)
            {
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void DataGridCadItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!IsMouseInRowDetail && DataGridParts.SelectedCells.Count > 0)
                {
                    CurrentDataContext.CurrentEcnDataCheckDataContext.SelectedDataCheckItem = (EcnDataCheckItem)DataGridParts.SelectedCells.First().Item;

                    if (!CurrentDataContext.CurrentEcnDataCheckDataContext.SelectedDataCheckItem.IsFirstRowDetailShow)
                    {
                        DataGridRow row = (DataGridRow)DataGridParts.ItemContainerGenerator.ContainerFromItem(CurrentDataContext.CurrentEcnDataCheckDataContext.SelectedDataCheckItem);

                        if (row != null)
                            if (row.DetailsVisibility == Visibility.Visible)
                                row.DetailsVisibility = Visibility.Collapsed;
                            else
                                row.DetailsVisibility = Visibility.Visible;
                    }
                    else
                        CurrentDataContext.CurrentEcnDataCheckDataContext.SelectedDataCheckItem.IsFirstRowDetailShow = false;
                }
            }
            catch (Exception ex)
            {
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            IsMouseInRowDetail = true;
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseInRowDetail = false;
        }

        private void DataGridHyperlinkColumn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string openLink = ((EcnDataCheckResultItem)((TextBlock)sender).DataContext).IssueDocumentationPath;
                if (!string.IsNullOrEmpty(openLink))
                    McgFileAndSystemTools.OpenFile(openLink);
            }
            catch (Exception ex)
            {
                EcnDataCheckException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
