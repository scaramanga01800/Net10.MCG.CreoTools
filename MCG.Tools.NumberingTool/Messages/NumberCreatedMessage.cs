using MCG.Tools.NumberingTool.ViewModel;

namespace MCG.Tools.NumberingTool.Messages
{
    internal class NumberCreatedMessage
    {
        public NumberingToolTemplate  Template { get; set; }
        public NumberingToolItem Item { get; set; }
    }
}
