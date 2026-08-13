using Fluent;
using MCG.CommonLib.Configuration;
using MCG.CommonLib.Services.Statics;
using MCG.CommonLib.WpfComponent.Interfaces;
using MCG.CommonLib.WpfComponent.View.Attributecolumn;
using MCG.CommonLib.WpfComponent.ViewModel;
using MCG.CREO_Tools.MassUpdateAttribute.Configuration;
using MCG.CREO_Tools.MassUpdateAttribute.Exceptions;
using MCG.CREO_Tools.MassUpdateAttribute.ViewModel;

namespace MCG.CREO_Tools.MassUpdateAttribute.View
{
    public partial class CreateNewCadDocumentFluentWindow : RibbonWindow
    {
        private CreateNewCadDocumentViewModel CurrentCreateNewCadDocumentViewModel;
        private string MainAppFolder;

        public CreateNewCadDocumentFluentWindow(CreateNewCadDocumentViewModel currentViewModel, ISharedAppContext sharedAppContext)
        {
            try
            {
                TraceLog.AddTraceLog("Create CreateNewCadDocumentFluentWindow");
                CurrentCreateNewCadDocumentViewModel = currentViewModel;
                DataContext = CurrentCreateNewCadDocumentViewModel;

                MainAppFolder = System.Environment.GetEnvironmentVariable(CommonLibConstants.MainAppFolderEnvirName);
                if (MainAppFolder == null || MainAppFolder == "")
                    MainAppFolder = CommonLibConstants.MainAppFolder;

                McgWpfTools.MergeLacalizedDictionary($"{MainAppFolder}\\{CommonLibConstants.ResourcesFolder}\\{MassUpdateAttributeConstants.MainDictionary}", UriKind.Absolute);

                InitializeComponent();
                AddOtherAttributes();
                McgWpfTools.UpdateMergeDictionaries(sharedAppContext.CurrentLanguage?.Language?.CultureInfo?.Substring(0, 2));
            }
            catch (Exception ex)
            {
                MassUpdateAttributeException.SendMessageBox(this.GetType().Name, ex);
            }
        }

        private void AddOtherAttributes()
        {
            try
            {
                if (CurrentCreateNewCadDocumentViewModel.CurrentCreateNewCadDocumentDataContext.ListOtherAttributes != null)
                {
                    McgAttributeGridTextFluent CurrentMcgAttributeGridText = null;

                    McgAttributeGridComboBoxFluent CurrentMcgAttributeGridComboBox = null;

                    foreach (var elem in CurrentCreateNewCadDocumentViewModel.CurrentCreateNewCadDocumentDataContext.ListOtherAttributes)
                    {
                        if (elem.ColumnType == McgColumnType.TEXT)
                        {
                            CurrentMcgAttributeGridText = new McgAttributeGridTextFluent();
                            CurrentMcgAttributeGridText.CurrentMcgAttributeHeaderViewModel.UpdateHeader(elem);
                            SpOtherAttributes.Children.Add(CurrentMcgAttributeGridText);
                            elem.ParentAttributeObject = CurrentMcgAttributeGridText;
                        }
                        else if (elem.ColumnType == McgColumnType.COMBOBOX || elem.ColumnType == McgColumnType.TEMPLATECOMBOBOX)
                        {
                            CurrentMcgAttributeGridComboBox = new McgAttributeGridComboBoxFluent();
                            CurrentMcgAttributeGridComboBox.CurrentMcgAttributeHeaderViewModel.UpdateHeader(elem);
                            SpOtherAttributes.Children.Add(CurrentMcgAttributeGridComboBox);
                            elem.ParentAttributeObject = CurrentMcgAttributeGridComboBox;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MassUpdateAttributeException(this.GetType().Name, ex);
            }
        }


    }
}
