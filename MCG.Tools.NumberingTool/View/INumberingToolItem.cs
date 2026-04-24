namespace MCG.Tools.NumberingTool.View
{
    interface INumberingToolItem
    {
        string Number { get; set; }
        string CreatedBy { get; set; }
        string Description { get; set; }
        string Product { get; set; }
        string Format { get; set; }
        DateTime CreatedOn { get; set; }
        bool IsUpdated { get; set; }
    }
}
