using MCG.CommonLib.Services.Statics;
using MCG.CREO_Tools.MiscTools.Exceptions;

namespace MCG.CREO_Tools.MiscTools.ViewModel.MechanismTool
{
    public class AnalysisFileContentException : MiscToolsException
    {
        public AnalysisFileContentException(string message) : base(message)
        {
            TraceLog.AddTraceLog(Message);
        }
    }
}
