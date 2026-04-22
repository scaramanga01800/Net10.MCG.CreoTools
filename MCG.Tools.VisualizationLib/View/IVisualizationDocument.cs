using MCG.CommonLib.Models.Enums;
using MCG.Tools.VisualizationLib.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCG.Tools.VisualizationLib.View
{
    public interface IVisualizationDocument
    {
        string DocumentNumber { get; set; }
        string DocumentRevision { get; set; }
        string Comment { get; set; }
        bool IsSelected { get; set; }

        DocumentTypeEnum DocumentType { get; set; }
    }
}
