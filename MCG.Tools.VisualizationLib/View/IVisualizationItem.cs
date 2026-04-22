using MCG.CommonLib.Models.Enums;
using MCG.Tools.VisualizationLib.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCG.Tools.VisualizationLib.View
{
    public interface IVisualizationItem
    {
        string PartNumber { get; set; }
        string PartRevision { get; set; }
        ObservableCollection<string> AllPartRevision { get; set; }
        string State { get; set; }
        string DescriptionEng { get; set; }
        string DescriptionLocal { get; set; }
        string PdmContext { get; set; }

        string Comment { get; set; }
        string DetailComment { get; set; }
        string AddedFrom { get; set; }
        bool IsDocumentFound { get; set; }
        bool IsSelected { get; set; }
        bool IsAllSelected { get; set; }
        DocumentTypeEnum ItemType { get; set; }
        DocumentTypeEnum ItemFrom { get; set; }

        ObservableCollection<VisualizationDocument> SearchedDocumentList { get; set; }

        event EventHandler IsSelectedEvent;
    }
}
