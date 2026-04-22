using System;

namespace MCG.Tools.VisualizationLib.Configuration
{
    public class DownloadVisualizationFileUserConfiguration
    {
        public bool IsPdfTiffMainSelected { get; set; }
        public bool IsPdfTiffSelected { get; set; }
        public bool IsOfficeDocSelected { get; set; }
        public bool IsPvzSelected { get; set; }
        public bool IsDxfSelected { get; set; }
        public bool IsStepSelected { get; set; }
        public bool IsIgesSelected { get; set; }
        public bool IsOtherSelected { get; set; }

        public bool IsColAddedFromShown { get; set; }
        public bool IsColDescriptionEngShown { get; set; }
        public bool IsColDescriptionLocalShown { get; set; }
        public bool IsColPdmContextShown { get; set; }
        public bool IsCreateZip { get; set; } = true;

        public bool IsAdmin { get; set; } = false;

        public string ExportFolder { get; set; }

        public event EventHandler UserConfigurationUpdateEvent;
        public void RaiseUserConfigurationUpdateEvent()
        {
            try
            {
                UserConfigurationUpdateEvent?.Invoke(this, new EventArgs());
            }
            catch (Exception)
            {
            }
        }
    }
}
