using System;

namespace MCG.Tools.VisualizationLib.ViewModel
{
    public class VisualizationItemRevisionState
    {
        public string Revision { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;

        public override string ToString()
        {
            return Revision;
        }
    }
}
