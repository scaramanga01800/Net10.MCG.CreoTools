using MCG.CREO_Tools.MiscTools.ViewModel.WebtermRequest;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.MiscTools.View.WebtermRequest
{
    public interface IWebtermRequestItem
    {
        WebtermRequestClass SelectedClass { get; set; }
        int SelectedClassIndex { get; set; }
        string QualInspGrp { get; set; }
        string DefaulUnit { get; set; }
        double MinMass { get; set; }
        double MaxMass { get; set; }
        string TermEn { get; set; }
        string DescriptionEn { get; set; }
        string TermUpperCaseEn { get; set; }
        string TermAbbrevitationEn { get; set; }
        string AttributeEn { get; set; }
        string AttributeExampleEn { get; set; }
        string TermFr { get; set; }
        string DescriptionFr { get; set; }
        string TermUpperCaseFr { get; set; }
        string TermAbbrevitationFr { get; set; }
        string AttributeFr { get; set; }
        string AttributeExampleFr { get; set; }
        string TermDe { get; set; }
        string DescriptionDe { get; set; }
        string TermUpperCaseDe { get; set; }
        string TermAbbrevitationDe { get; set; }
        string AttributeDe { get; set; }
        string AttributeExampleDe { get; set; }

        ObservableCollection<string> ListImage { get; set; }
        string SelectedImage { get; set; }
    }
}
