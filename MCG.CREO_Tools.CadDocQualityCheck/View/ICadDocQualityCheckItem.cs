using MCG.CommonLib.Models.Enums;
using MCG.CREO_Tools.CadDocQualityCheck.ViewModel;
using System.Collections.ObjectModel;

namespace MCG.CREO_Tools.CadDocQualityCheck.View
{
    public interface ICadDocQualityCheckItem
    {
        event EventHandler IsSelectedEvent;

        string Number { get; set; }
        bool IsUpdated { get; set; }
        bool IsSelected { get; set; }
        bool IsCheckedIn { get; set; }
        bool IsCheckedOut { get; set; }
        bool IsLocallyModified { get; set; }
        bool IsReadOnly { get; set; }
        bool IsFound { get; set; }
        string Status { get; set; }
        string Comment { get; set; }
        CadDocCheckStatus LayersStatus { get; set; }
        CadDocCheckStatus RelationsStatus { get; set; }
        CadDocCheckStatus AttributesStatus { get; set; }
        CadDocCheckStatus ComponentStatus { get; set; }
        CadDocCheckStatus FeatureStatus { get; set; }

        string CurrentPreRegenRelations { get; set; }
        string CurrentPostRegenRelations { get; set; }

        string NewPreRegenRelations { get; set; }
        string NewPostRegenRelations { get; set; }

        bool IsPostRegenRelationsOk { get; set; }
        bool IsPreRegenRelationsOk { get; set; }

        ObservableCollection<CadDocRelationLineItem> ListCurrentPreRegenRelations { get; set; }
        ObservableCollection<CadDocRelationLineItem> ListCurrentPostRegenRelations { get; set; }

        ObservableCollection<CadDocLayerItem> ListLayers { get; set; }
        ObservableCollection<CadDocAttributeItem> ListAttributes { get; set; }

        CadDocCheckStatus MaterialStatus { get; set; }
        bool IsMaterialAssigned { get; set; }
        bool IsNotDefaultMaterialAssigned { get; set; }
        bool IsMaterialConditionDefined { get; set; }

        bool IsUnitsOk { get; set; }

        EpmDocumentTypeEnum CadDocSubType { get; set; }

        ObservableCollection<CadDocQualityCheckResultItem> ListQualityCheckResult { get; set; }

        CadDocTemplate Template { get; set; }
    }
}
