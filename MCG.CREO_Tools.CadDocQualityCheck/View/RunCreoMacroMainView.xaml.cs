using Fluent;
using MCG.CommonLib.Services.Statics;

namespace MCG.CREO_Tools.CadDocQualityCheck.View
{
    public partial class RunCreoMacroMainView : RibbonTabItem
    {
        public RunCreoMacroMainView()
        {
                TraceLog.AddTraceLog("Create RunCreoMacroMainView");
            InitializeComponent();
        }
    }
}
