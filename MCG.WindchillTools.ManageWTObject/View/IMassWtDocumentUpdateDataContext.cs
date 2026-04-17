using MCG.CommonLib.Models.Main;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.WindchillTools.ManageWTObject.View
{
    public interface IMassWtDocumentUpdateDataContext
    {
        bool ActionInProgress { get; set; }
        bool IsAllPartSelected { get; set; }

        ObservableCollection<MgtWtDocumentItem> WtDocumentList { get; set; }

        ObservableCollection<string> ListWindchillDocumentType { get; set; }
        string SelectedWindchillDocumentType { get; set; }

        ObservableCollection<string> ListWindchillPartType { get; set; }
        string SelectedWindchillPartType { get; set; }

        ObservableCollection<WindchillContentType> ListContentType { get; set; }

        ObservableCollection<WindchillContext> WindchillContextList { get; set; }

        WindchillContext SelectedWindchillContext { get; set; }

        ObservableCollection<string> ListWebterm { get; set; }
        ObservableCollection<string> ListWebtermLocal { get; set; }

        MCGLanguage SelectedLanguage { get; set; }
        ObservableCollection<MCGLanguage> ListLanguage { get; set; }
        ObservableCollection<string> ListGroup { get; set; }
        ObservableCollection<string> ListBrand { get; set; }

        string SelectedWebterm { get; set; }
        string SelectedLocalWebterm { get; set; }

        string StatusBarTextRight { get; set; }
        int TotalStep { get; set; }
        int CurrentStep { get; set; }
    }
}
