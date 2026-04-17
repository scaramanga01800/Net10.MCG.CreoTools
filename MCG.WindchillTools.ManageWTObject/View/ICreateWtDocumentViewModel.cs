using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MCG.WindchillTools.ManageWTObject.View
{
    public interface ICreateWtDocumentViewModel
    {
        MgtWtObject WtObject { get; set; }
        ObservableCollection<string> ListWindchillDocumentType { get; set; }
        string SelectedWindchillDocumentType { get; set; }
        ObservableCollection<string> ListWindchillPartType { get; set; }
        string SelectedWindchillPartType { get; set; }
        ObservableCollection<string> ListWebterm { get; set; }
        ObservableCollection<string> ListWebtermLocal { get; set; }
        string SelectedWebterm { get; set; }
        string SelectedLocalWebterm { get; set; }

        ObservableCollection<WindchillContext> WindchillContextList { get; set; }

        ICommand CommandSelectContext { get; }
    }
}
