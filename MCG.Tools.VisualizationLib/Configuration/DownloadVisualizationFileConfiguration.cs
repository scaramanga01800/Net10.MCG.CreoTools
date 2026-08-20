using MCG.CommonLib.Models.SAP;
using System.Collections.Generic;

namespace MCG.Tools.VisualizationLib.Configuration
{
    public class DownloadVisualizationFileConfiguration
    {
        public List<string> OptionalWatermarkValues { get; set; }
        public List<SapPlant> ListSapPlant { get; set; }
    }
}
