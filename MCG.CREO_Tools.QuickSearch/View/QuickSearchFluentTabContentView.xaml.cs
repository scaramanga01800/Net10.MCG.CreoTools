using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.View;
using MCG.CREO_Tools.QuickSearch.Configuration;
using MCG.CREO_Tools.QuickSearch.Exceptions;
using MCG.CREO_Tools.QuickSearch.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MCG.CREO_Tools.QuickSearch.View
{
    public partial class QuickSearchFluentTabContentView : UserControl
    {
        private QuickSearchViewModel CurrentQuickSearchViewModel { get; set; }
        private bool IsAppAlreadyInit { get; set; } = false;
        private string ImageResourcePath { get; set; }

        public QuickSearchFluentTabContentView()
        {
            try
            {
                ImageResourcePath = CommonLibConstants.ImageResourcesPath;

                InitializeComponent();
                DataContextChanged += QuickSearchFluentTabContentView_DataContextChanged;
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void QuickSearchFluentTabContentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (!IsAppAlreadyInit && DataContext != null && DataContext.GetType() == typeof(QuickSearchViewModel))
                {
                    CurrentQuickSearchViewModel = (QuickSearchViewModel)DataContext;
                    IsAppAlreadyInit = true;
                    CurrentQuickSearchViewModel.CurrentQuickSearchDataContext.SubClassChangedEvent += UpdateSubClassColumn;
                    UpdateSubClassColumn();
                }

            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        public static string ToPascalFromUpper(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

        private void UpdateSubClassColumn(object sender = null, EventArgs e = null)
        {
            try
            {
                DgPart.Columns.Clear();
                QuickSearchColumnHeaderSearch CurrentColHeader;
                DataGridTextColumn CurrentCol;
                if (CurrentQuickSearchViewModel.CurrentQuickSearchDataContext.SelectedSubClassItem != null)
                {
                    foreach (var SubClassParam in CurrentQuickSearchViewModel.CurrentQuickSearchDataContext.SelectedSubClassItem.ShownPartSubClassParam)
                    {
                        CurrentColHeader = new QuickSearchColumnHeaderSearch();
                        CurrentColHeader.SetProperties(SubClassParam.Name, CurrentQuickSearchViewModel, QuickSearchConstants.ColumnMinWidth, SubClassParam);
                        CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };
                        CurrentCol.Binding = new Binding($"CurrentPart.{ToPascalFromUpper(SubClassParam.IdParam)}");
                        DgPart.Columns.Add(CurrentCol);
                    }
                }

                // Add Part Picture
                if (CurrentQuickSearchViewModel.CurrentQuickSearchDataContext.IsPartPictureShown)
                {
                    DataGridTemplateColumn PictureColumn = new DataGridTemplateColumn() { Header = McgWpfTools.GetStringResource("QS_ColHeader09") };
                    PictureColumn.Width = 50;

                    FrameworkElementFactory imageFactory = new FrameworkElementFactory(typeof(Image));
                    imageFactory.SetBinding(Image.SourceProperty, new Binding("CurrentPart.PARTPICTUREBIN"));

                    DataTemplate dataTemplate = new DataTemplate();
                    dataTemplate.VisualTree = imageFactory;

                    PictureColumn.CellTemplate = dataTemplate;
                    DgPart.Columns.Add(PictureColumn);
                }

                // Add columns for SAP information
                if (CurrentQuickSearchViewModel.CurrentQuickSearchDataContext.ShowSapCostVolumeInfo)
                {
                    Style cellStyleTextRight = new Style(typeof(DataGridCell));
                    cellStyleTextRight.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right));

                    // Add cost volume columns
                    string CurrentCurrency = McgBusinessTools.GetCurrencySymbol(CurrentQuickSearchViewModel.CurrentQuickSearchDataContext.SelectedSapPlant.Currency);
                    McgHeaderDoubleLabel CurrentMcgHeaderDoubleLabel;
                    
                    // Plant Cost
                    CurrentMcgHeaderDoubleLabel = new McgHeaderDoubleLabel(CurrentQuickSearchViewModel.CurrentQuickSearchDataContext.SelectedSapPlant.Number, CurrentCurrency);
                    CurrentCol = new DataGridTextColumn() { Header = CurrentMcgHeaderDoubleLabel };
                    Binding VarBinding = new Binding("PlantStdCost");
                    VarBinding.StringFormat = $"{{0:# ##0.00}} {CurrentCurrency}";
                    CurrentCol.Binding = VarBinding;
                    CurrentCol.CellStyle = cellStyleTextRight;
                    DgPart.Columns.Add(CurrentCol);

                    // Plant Volume
                    CurrentMcgHeaderDoubleLabel = new McgHeaderDoubleLabel(CurrentQuickSearchViewModel.CurrentQuickSearchDataContext.SelectedSapPlant.Number, "Vol.");
                    CurrentCol = new DataGridTextColumn() { Header = CurrentMcgHeaderDoubleLabel };
                    CurrentCol.Binding = new Binding("PlantVolume");
                    DgPart.Columns.Add(CurrentCol);

                    // Plant Procurement Type
                    CurrentMcgHeaderDoubleLabel = new McgHeaderDoubleLabel(CurrentQuickSearchViewModel.CurrentQuickSearchDataContext.SelectedSapPlant.Number, McgWpfTools.GetStringResource("QS_ColHeader08"));
                    CurrentCol = new DataGridTextColumn() { Header = CurrentMcgHeaderDoubleLabel };
                    CurrentCol.Binding = new Binding("ProcurementType");
                    DgPart.Columns.Add(CurrentCol);

                    McgHeaderCurrency CurrentMcgHeaderCurrency;
                    // France Average Cost
                    CurrentMcgHeaderCurrency = new McgHeaderCurrency(CurrentCurrency, McgWpfTools.GetBitmapImage($"{ImageResourcePath}Flag_FR.ico", 24));
                    CurrentCol = new DataGridTextColumn() { Header = CurrentMcgHeaderCurrency };
                    VarBinding = new Binding("FrenchAverageCost");
                    VarBinding.StringFormat = $"{{0:# ##0.00}} {CurrentCurrency}";
                    CurrentCol.Binding = VarBinding;
                    CurrentCol.CellStyle = cellStyleTextRight;

                    DgPart.Columns.Add(CurrentCol);

                    // Europe Average Cost
                    CurrentMcgHeaderCurrency = new McgHeaderCurrency(CurrentCurrency, McgWpfTools.GetBitmapImage($"{ImageResourcePath}Flag_European.ico", 24));
                    CurrentCol = new DataGridTextColumn() { Header = CurrentMcgHeaderCurrency };
                    VarBinding = new Binding("EuropeAverageCost");
                    VarBinding.StringFormat = $"{{0:# ##0.00}} {CurrentCurrency}";
                    CurrentCol.Binding = VarBinding;
                    CurrentCol.CellStyle = cellStyleTextRight;

                    DgPart.Columns.Add(CurrentCol);

                    // World Average Cost
                    CurrentMcgHeaderCurrency = new McgHeaderCurrency(CurrentCurrency, McgWpfTools.GetBitmapImage($"{ImageResourcePath}Earth.ico", 24));
                    CurrentCol = new DataGridTextColumn() { Header = CurrentMcgHeaderCurrency };
                    VarBinding = new Binding("WorldAverageCost");
                    VarBinding.StringFormat = $"{{0:# ##0.00}} {CurrentCurrency}";
                    CurrentCol.Binding = VarBinding;
                    CurrentCol.CellStyle = cellStyleTextRight;

                    DgPart.Columns.Add(CurrentCol);

                    // World Average Volume
                    CurrentMcgHeaderCurrency = new McgHeaderCurrency("Vol.", McgWpfTools.GetBitmapImage($"{ImageResourcePath}Earth.ico", 24));
                    CurrentCol = new DataGridTextColumn() { Header = CurrentMcgHeaderCurrency };
                    CurrentCol.Binding = new Binding("WorldAverageVolume");
                    DgPart.Columns.Add(CurrentCol);

                    // Plant with Max Volume
                    CurrentMcgHeaderDoubleLabel = new McgHeaderDoubleLabel(McgWpfTools.GetStringResource("QS_ColHeader05"), McgWpfTools.GetStringResource("QS_ColHeader05b"));
                    CurrentCol = new DataGridTextColumn() { Header = CurrentMcgHeaderDoubleLabel };
                    CurrentCol.Binding = new Binding("PlantMaxVolume");
                    DgPart.Columns.Add(CurrentCol);

                    // Plant with Max Volume
                    CurrentMcgHeaderDoubleLabel = new McgHeaderDoubleLabel(CurrentQuickSearchViewModel.CurrentQuickSearchDataContext.SelectedSapPlant.Number, $"{CurrentCurrency}/Kg");
                    CurrentCol = new DataGridTextColumn() { Header = CurrentMcgHeaderDoubleLabel };
                    CurrentCol.Binding = new Binding("PlantStdCostPerKg");
                    DgPart.Columns.Add(CurrentCol);
                }
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void StackPanel_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                SliderImage.Value += SliderImage.LargeChange * e.Delta / 30;
            }
            catch (Exception ex)
            {
                QuickSearchException.SendMessageBox(this.GetType().Name, ex);
            }
        }
    }
}
