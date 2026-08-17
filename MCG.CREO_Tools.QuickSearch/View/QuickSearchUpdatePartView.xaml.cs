using Fluent;
using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.QuickSearch.Exceptions;
using MCG.CREO_Tools.QuickSearch.ViewModel;
using System.Windows.Data;

namespace MCG.CREO_Tools.QuickSearch.View
{
    public partial class QuickSearchUpdatePartView : RibbonWindow
    {
        public QuickSearchUpdatePartViewModel CurrentDataContext { get; set; }

        public QuickSearchUpdatePartView(QuickSearchUpdatePartViewModel currentViewModel)
        {
            try
            {
                CurrentDataContext = currentViewModel;
                DataContext = currentViewModel;
                InitializeComponent();
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public void SetProperties(QuickSearchPart selectedPartItem)
        {
            try
            {
                CurrentDataContext = new QuickSearchUpdatePartViewModel();
                CurrentDataContext.PartItem = selectedPartItem;
                DataContext = CurrentDataContext;
                UpdateSubClassColumn();
            }
            catch (Exception ex)
            { 
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void UpdateSubClassColumn()
        {
            try
            {
                if (DataContext != null && DataContext.GetType() == typeof(QuickSearchUpdatePartViewModel))
                    CurrentDataContext = (QuickSearchUpdatePartViewModel)DataContext;

                if (CurrentDataContext != null && CurrentDataContext.PartItem.SubClassItem != null)
                {
                    SpParameters.Children.Clear();
                    Fluent.TextBox CurrentTextBox = null;

                    foreach (var SubClassParam in CurrentDataContext.PartItem.SubClassItem.AllPartSubClassParam)
                    {
                        CurrentTextBox = new Fluent.TextBox();
                        CurrentTextBox.Header = SubClassParam.Name;
                        CurrentTextBox.SetBinding(Fluent.TextBox.TextProperty, new Binding($"PartItem.UpdatedPart.{McgBusinessTools.Capitalize(SubClassParam.IdParam)}"));
                        SpParameters.Children.Add(CurrentTextBox);
                    }

                    CurrentTextBox = new Fluent.TextBox();
                    CurrentTextBox.Header = "ID Class";
                    CurrentTextBox.IsEnabled = false;
                    CurrentTextBox.SetBinding(Fluent.TextBox.TextProperty, new Binding($"PartItem.SubClassItem.Name"));
                    SpParameters.Children.Add(CurrentTextBox);
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
