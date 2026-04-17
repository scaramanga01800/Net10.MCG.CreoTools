namespace MCG.Tools.PurchaseOrderFollowUp.View
{
    public interface IPurchaseOrderLocation
    {
        string Name { get; set; }
        string Description { get; set; }
        string Country { get; set; }
        string Adress { get; set; }
    }
}
