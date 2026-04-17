using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.Tools.PurchaseOrderFollowUp.Exceptions;
using MCG.Tools.PurchaseOrderFollowUp.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    /// <summary>
    /// Logique d'interaction pour PurchaseOrderFollowUpTabContentView.xaml
    /// </summary>
    public partial class PurchaseOrderFollowUpTabContentView : UserControl
    {
        bool IsAppAlreadyInit { get; set; } = false;
        public PurchaseOrderFollowUpViewModel CurrentDataContext { get; set; }

        public PurchaseOrderFollowUpTabContentView()
        {
            try
            {
                InitializeComponent();
                DataContextChanged += PurchaseOrderFollowUpTabContentView_DataContextChanged;
            }
            catch (Exception ex)
            {
                PurchaseOrderFollowUpException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void PurchaseOrderFollowUpTabContentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsAppAlreadyInit && DataContext != null && DataContext.GetType() == typeof(PurchaseOrderFollowUpViewModel))
            {
                List<DataGridColumn> tempColumns = new List<DataGridColumn>();
                tempColumns.AddRange(DataGridAllRequest.Columns);
                DataGridAllRequest.Columns.Clear();

                CurrentDataContext = (PurchaseOrderFollowUpViewModel)DataContext;
                IsAppAlreadyInit = true;

                PurchaseOrderColumnHeaderSearch CurrentColHeader;
                DataGridTextColumn CurrentCol;
                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col01"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "ID",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"ID");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col18"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "RequestType",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"RequestType");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col19"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "Description",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"Description");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col10"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "CreatedBy",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"CreatedBy");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col11"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "CreatedOn",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"CreatedOn");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col12"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "RequestedBy",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"RequestedBy");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col13"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "SapCreatedBy",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"SapCreatedBy");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col14"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "SapCreatedOn",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"SapCreatedOn");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col15"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "SapPurchaseRequest",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"SapPurchaseRequest");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col16"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "SapPurchaseOrder",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"SapPurchaseOrder");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col17"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "Status",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"Status");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col06"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "Vendor.Description",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"Vendor.Description");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col24"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "Vendor.Number",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"Vendor.Number");
                DataGridAllRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_CBCostCenter"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "CostCenter.Number",
                        FilterValue = "",
                        ListName = "ListShownRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"CostCenter.Number");
                DataGridAllRequest.Columns.Add(CurrentCol);

                foreach (var item in tempColumns)
                    DataGridAllRequest.Columns.Add(item);



                tempColumns = new List<DataGridColumn>();
                tempColumns.AddRange(DataGridMyRequest.Columns);
                DataGridMyRequest.Columns.Clear();

                CurrentDataContext = (PurchaseOrderFollowUpViewModel)DataContext;
                IsAppAlreadyInit = true;

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col01"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "ID",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"ID");
                DataGridMyRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col18"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "RequestType",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"RequestType");
                DataGridMyRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col19"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "Description",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"Description");
                DataGridMyRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col10"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "CreatedBy",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"CreatedBy");
                DataGridMyRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col11"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "CreatedOn",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"CreatedOn");
                DataGridMyRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col12"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "RequestedBy",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"RequestedBy");
                DataGridMyRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col13"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "SapCreatedBy",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"SapCreatedBy");
                DataGridMyRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col14"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "SapCreatedOn",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"SapCreatedOn");
                DataGridMyRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col15"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "SapPurchaseRequest",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"SapPurchaseRequest");
                DataGridMyRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col16"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "SapPurchaseOrder",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"SapPurchaseOrder");
                DataGridMyRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col17"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "Status",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"Status");
                DataGridMyRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col06"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "Vendor.Description",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"Vendor.Description");
                DataGridMyRequest.Columns.Add(CurrentCol);

                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_Col24"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "Vendor.Number",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"Vendor.Number");
                DataGridMyRequest.Columns.Add(CurrentCol);


                CurrentColHeader = new PurchaseOrderColumnHeaderSearch(McgWpfTools.GetStringResource("POF_CBCostCenter"),
                    DataContext,
                    new McgColumnData()
                    {
                        ColumnReference = "CostCenter.Number",
                        FilterValue = "",
                        ListName = "ListShownMyRequest"
                    });
                CurrentCol = new DataGridTextColumn() { Header = CurrentColHeader };

                CurrentCol.Binding = new Binding($"CostCenter.Number");
                DataGridMyRequest.Columns.Add(CurrentCol);

                foreach (var item in tempColumns)
                    DataGridMyRequest.Columns.Add(item);
            }
        }
    }
}
