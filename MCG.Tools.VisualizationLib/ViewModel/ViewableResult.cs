using MCG.WindchillRequestTool;
using MCG.WindchillRequestTool.ViewModel;
using System.Collections.Generic;

namespace MCG.Tools.VisualizationLib.ViewModel
{
    public class ViewableResult
    {
        public List<WindchillObjectViewableItemDownload> AllViewableDownload { get; set; }

        public WindchillObjectViewable ViewablePart { get; set; }

        public bool IsViewableSearchSuccesfull { get; set; } = false;
    }
}
