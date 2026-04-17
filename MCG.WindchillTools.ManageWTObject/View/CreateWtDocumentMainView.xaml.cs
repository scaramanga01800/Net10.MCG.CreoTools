using Fluent;
using MCG.CommonLib.DataBaseAccess.Models.CreoToolsDb;
using MCG.CommonLib.Models.Enums;
using MCG.CommonLib.Models.Main;
using MCG.CommonLib.Services.Statics;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MCG.WindchillTools.ManageWTObject.View
{
    /// <summary>
    /// Logique d'interaction pour CreateWtDocumentMainView.xaml
    /// </summary>
    public partial class CreateWtDocumentMainView : RibbonWindow
    {
        private CreateWtDocumentViewModel CurrentDataContext { get; set; }

        public MgtWtDocumentItem WtDocumentItem { get; set; }

        public MessageBoxResult Return { get; set; } = MessageBoxResult.Cancel;

        public CreateWtDocumentMainView(CreateWtDocumentViewModel currentVM)
        {
            InitializeComponent();
            CurrentDataContext = currentVM;
            DataContext = CurrentDataContext;
        }

        public void SetCreateWtDocumentMainViewProperties(List<string> pListWindchillDocumentType,
                                                          List<string> pListWindchillPartType,
                                                          List<Webterm> pAllWebterm,
                                                          MCGLanguage LocalLanguage,
                                                          List<WindchillContext> pWindchillContextList,
                                                          List<string> ListGroup,
                                                          List<string> ListBrand)
        {
            CurrentDataContext.SetCreateWtDocumentProperties(pListWindchillDocumentType, pListWindchillPartType, pAllWebterm, LocalLanguage, pWindchillContextList, ListGroup, ListBrand);
        }

        private void Button_OK(object sender, RoutedEventArgs e)
        {
            if (CurrentDataContext.WtObject.NUMBER != null && CurrentDataContext.WtObject.NUMBER.Trim() != "" && CurrentDataContext.WtObject.REVISION?.Trim() != "")
            {
                WtDocumentItem = new MgtWtDocumentItem()
                {
                    Number = CurrentDataContext.WtObject.NUMBER?.Trim().ToUpper(),
                    Revision = McgReflectionTools.GetEnumValue<McgRevisionSchemaEnum>(CurrentDataContext.WtObject.REVISION),
                    WindchillDocumentType = CurrentDataContext.SelectedWindchillDocumentType,
                    WindchillPartType = CurrentDataContext.SelectedWindchillPartType,
                    WtDocumentObject = CurrentDataContext.WtObject,
                    WtPartObject = CurrentDataContext.WtObject,
                };
                CurrentDataContext.WtObject.ParentDocument = WtDocumentItem;

                Return = MessageBoxResult.OK;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(McgWpfTools.GetStringResource("MWT_AddDocumentErrorMsg"), McgWpfTools.GetStringResource("MWT_WindowAddDocument"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            Return = MessageBoxResult.Cancel;
            DialogResult = false;
            Close();
        }
    }
}
