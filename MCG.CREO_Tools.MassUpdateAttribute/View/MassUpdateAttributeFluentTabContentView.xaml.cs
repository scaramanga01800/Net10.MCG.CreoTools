using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.CREO_Tools.MassUpdateAttribute.Exceptions;
using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MCG.CREO_Tools.MassUpdateAttribute.View
{
    public partial class MassUpdateAttributeFluentTabContentView : UserControl
    {
        #region [REGION] Internal variables
        public MassUpdateAttributeViewModel CurrentMassUpdAttribViewModel { get; set; }
        private bool IsMultiSelectionInProgress { get; set; } = false;
        public int SelectedIndex { get; set; } = -1;
        public int PreviousSelectedIndex { get; set; } = -1;
        public bool IsAppAlreadyInit { get; set; } = false;
        #endregion

        public MassUpdateAttributeFluentTabContentView()
        {
            InitializeComponent();
            DataContextChanged += MassUpdateAttributeFluentTabContentView_DataContextChanged;
        }

        public void InitApp()
        {
            try
            {
                if (!IsAppAlreadyInit)
                {
                    CurrentMassUpdAttribViewModel = (MassUpdateAttributeViewModel)DataContext;

                    // Update column
                    Binding aBindingCol;
                    McgAttributeTextColumn CurrentTextCol = null;
                    McgAttributeComboBoxColumn CurrentCombBoxCol = null;
                    DataGridTemplateColumn CurrentDataGridTemplateColumn = null;

                    // Add all other Columns
                    foreach (McgAttributeColumnHeaderInfo elem in CurrentMassUpdAttribViewModel.CurrentMassUpdAttribDataContext.ListColumns)
                    {
                        aBindingCol = new Binding(elem.ClassAttributeID);
                        aBindingCol.Mode = BindingMode.TwoWay;
                        aBindingCol.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                        if (elem.ColumnType == McgColumnType.TEXT)
                        {
                            CurrentTextCol = new McgAttributeTextColumn(elem);
                            CurrentTextCol.Binding = aBindingCol;
                            CurrentTextCol.IsReadOnly = elem.IsColumnReadOnly;
                            CurrentTextCol.MainDataContext = CurrentMassUpdAttribViewModel;
                            DataGridAttributes.Columns.Add(CurrentTextCol);

                        }
                        else if (elem.ColumnType == McgColumnType.COMBOBOX)
                        {
                            CurrentCombBoxCol = new McgAttributeComboBoxColumn(elem);
                            CurrentCombBoxCol.SelectedValueBinding = aBindingCol;
                            CurrentCombBoxCol.ItemsSource = elem.ListValue;
                            CurrentCombBoxCol.IsReadOnly = elem.IsColumnReadOnly;
                            CurrentCombBoxCol.MainDataContext = CurrentMassUpdAttribViewModel;

                            DataGridAttributes.Columns.Add(CurrentCombBoxCol);
                        }
                        else if (elem.ColumnType == McgColumnType.TEMPLATECOMBOBOX)
                        {
                            CurrentDataGridTemplateColumn = new DataGridTemplateColumn();

                            CurrentDataGridTemplateColumn.Header = elem.AttributeName;
                            var cellTemplate = new DataTemplate();
                            var comboBoxFactory = new FrameworkElementFactory(typeof(ComboBox));
                            comboBoxFactory.SetBinding(ComboBox.ItemsSourceProperty, new Binding(elem.PropertyList));
                            comboBoxFactory.SetBinding(ComboBox.SelectedValueProperty, new Binding(elem.PropertySelectedItem)
                            {
                                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                                Mode = BindingMode.TwoWay
                            });
                            cellTemplate.VisualTree = comboBoxFactory;
                            CurrentDataGridTemplateColumn.CellTemplate = cellTemplate;

                            DataGridAttributes.Columns.Add(CurrentDataGridTemplateColumn);
                        }
                    }

                    IsAppAlreadyInit = true;

                    CurrentMassUpdAttribViewModel.CurrentMassUpdAttribDataContext.ShownCadModels.CollectionChanged += new NotifyCollectionChangedEventHandler((sender, e) => SubscribeToIsSelectedEvent(sender, e));
                }
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void MassUpdateAttributeFluentTabContentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (DataContext != null && DataContext.GetType() == typeof(MassUpdateAttributeViewModel))
                {
                    //CurrentMassUpdAttribViewModel = (MassUpdateAttributeViewModel)DataContext;
                    InitApp();
                    DataContextChanged -= MassUpdateAttributeFluentTabContentView_DataContextChanged;
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        #region [REGION] Methods to manage Multi selection
        private void SubscribeToIsSelectedEvent(object sender, EventArgs e)
        {
            try
            {
                StartSubscribeToAllEcnEcoIsSelected();
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void StartSubscribeToAllEcnEcoIsSelected()
        {
            try
            {
                foreach (var item in CurrentMassUpdAttribViewModel.CurrentMassUpdAttribDataContext.ShownCadModels)
                {
                    item.IsSelectedEvent -= CheckIfMultiselection;
                    item.IsSelectedEvent += CheckIfMultiselection;
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        private void CheckIfMultiselection(object sender, EventArgs e)
        {
            try
            {
                if (!IsMultiSelectionInProgress)
                {
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                    {
                        SelectedIndex = CurrentMassUpdAttribViewModel.CurrentMassUpdAttribDataContext.SelectedIndex;
                        MultiSelectionAction(((MassUpdateAttributeItem)sender).IsSelected);
                    }
                    else
                        PreviousSelectedIndex = CurrentMassUpdAttribViewModel.CurrentMassUpdAttribDataContext.SelectedIndex;
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }

        public void MultiSelectionAction(bool SelectedValue)
        {
            try
            {
                IsMultiSelectionInProgress = true;
                for (int index = Math.Min(PreviousSelectedIndex, SelectedIndex); index <= Math.Max(PreviousSelectedIndex, SelectedIndex); index++)
                    ((MassUpdateAttributeItem)DataGridAttributes.Items[index]).IsSelected = SelectedValue;
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
            finally
            {
                IsMultiSelectionInProgress = false;
            }
        }
        #endregion
    }
}
