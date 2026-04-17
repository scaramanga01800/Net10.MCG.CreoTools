using MCG.CommonLib.Models.Main;
using MCG.WindchillRequestTool.Model.RestOdata;
using MCG.WindchillRequestTool.Model.Windchill;
using MCG.WindchillTools.ManageWTObject.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.WindchillTools.ManageWTObject.View
{
    public interface ICreateUpdateWtDocumentWtPartDataContext
    {

        bool ActionInProgress { get; set; }
        MgtWtDocumentItem CurrentWtObject { get; set; }
        bool WtDocumentSelected { get; set; }
        bool WtPartSelected { get; set; }
        ObservableCollection<MgtContentItem> ListContentItem { get; set; }
        ObservableCollection<string> ListWindchillDocumentType { get; set; }
        ObservableCollection<string> ListWindchillPartType { get; set; }

        ObservableCollection<WindchillContext> WindchillContextList { get; set; }
        WindchillContext SelectedWindchillContext { get; set; }

        ObservableCollection<string> ListWebterm { get; set; }
        ObservableCollection<string> ListWebtermLocal { get; set; }

        ObservableCollection<MCGLanguage> ListLanguage { get; set; }
        MCGLanguage SelectedLanguage { get; set; }

        ObservableCollection<WindchillContentType> ListContentType { get; set; }

        string FilterNumber { get; set; }

        ObservableCollection<RestOdataWtDocument> ListSearchWtDocument { get; set; }
        ObservableCollection<RestOdataWtPart> ListSearchWtPart { get; set; }

        ObservableCollection<string> AllUnits { get; set; }
        
        string StatusBarText { get; set; }

        ObservableCollection<WindchillObjectLinkType> WtObjectLinkList { get; set; }
        bool LinkWtDocumentWtPart { get; set; }

        ObservableCollection<string> MaterialList { get; set; }
        ObservableCollection<string> ListGroup { get; set; }
        ObservableCollection<string> ListSubGroup { get; set; }
        ObservableCollection<string> ListBrand { get; set; }
        ObservableCollection<string> ListOption { get; set; }

        string SelectedBrand { get; set; }
        string SelectedGroup { get; set; }
        string SelectedSubGroup { get; set; }
    }
}
