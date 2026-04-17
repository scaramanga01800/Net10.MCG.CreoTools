using MCG.CommonLib.Exceptions;
using MCG.CommonLib.Services.Statics;
using System.Runtime.CompilerServices;

namespace MCG.Tools.PurchaseOrderFollowUp.Exceptions
{
    public class PurchaseOrderFollowUpException : McgCommonLibException
    {
        public PurchaseOrderFollowUpException(string message) : base(message)
        {
            TraceLog.AddTraceLog(Message);
        }

        public PurchaseOrderFollowUpException(string CurrentClass = "UnknownClass", Exception CurrentException = null, [CallerMemberName] string CurrentMethod = "UnknownMethod") : base($"Exception in {CurrentClass}.{CurrentMethod} : {CurrentException.Message}")
        {
            TraceLog.AddTraceLog(Message);
        }

        public new static void SendMessageBox(string CurrentClass = "UnknownClass", Exception CurrentException = null, [CallerMemberName] string CurrentMethod = "UnknownMethod")
        {
            try
            {
                string message = "";
                if (CurrentException != null)
                    message = $"Runtime issue in {CurrentClass}.{CurrentMethod}, Contact your CAD Admin. Error msg: {CurrentException.Message}";
                else
                    message = $"Runtime issue in {CurrentClass}.{CurrentMethod}, Contact your CAD Admin. Error msg: unknown Exception";
                TraceLog.AddTraceLog(message);
                SendMessageEmailLogFile(message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception in VisualizationException.VisualizationException : {ex.Message}");
            }
        }
    }
}
