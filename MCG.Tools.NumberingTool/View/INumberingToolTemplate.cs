namespace MCG.Tools.NumberingTool.View
{
    interface INumberingToolTemplate
    {
         string NumberingTemplate { get; set; }
         string Description { get; set; }
         bool IsRangeAuthorized { get; set; }
         int MaxRange { get; set; }
    }
}
